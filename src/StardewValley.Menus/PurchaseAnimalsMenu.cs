using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Mobile;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class PurchaseAnimalsMenu : IClickableMenu
	{
		public const int region_okButton = 101;

		public const int region_doneNamingButton = 102;

		public const int region_randomButton = 103;

		public const int region_namingBox = 104;

		public static int menuHeight = 320;

		public static int menuWidth = 448;

		public List<ClickableTextureComponent> animalsToPurchase = new List<ClickableTextureComponent>();

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent doneNamingButton;

		public ClickableTextureComponent randomButton;

		public ClickableTextureComponent hovered;

		public ClickableComponent textBoxCC;

		private bool onFarm;

		private bool namingAnimal;

		private bool freeze;

		private FarmAnimal animalBeingPurchased;

		private TextBox textBox;

		private TextBoxEvent e;

		private Building newAnimalHome;

		private int priceOfAnimal;

		public bool readOnly;

		public ClickableTextureComponent tickButton;

		public ClickableTextureComponent cancelButton;

		private Building _selectedBuilding;

		private int headerX;

		private int headerY;

		private int headerWidth;

		private int goldX;

		private int goldY;

		private int scrollerX;

		private int scrollerY;

		private int scrollerWidth;

		private int scrollerHeight;

		private int descX;

		private int descY;

		private int descWidth;

		private int descHeight;

		private int buyX;

		private int buyY;

		private int buyWidth;

		private int buyHeight;

		private int itemHeight;

		private int itemWidth;

		private int scrollbarWidth;

		private int itemsPerPage;

		private int currentlySelectedItem;

		private bool buyButtonisHeld;

		private bool buyButtonVisible;

		private bool scrollbarVisible;

		private bool scrolling;

		private bool tickButtonHeld;

		private bool cancelButtonHeld;

		private bool doneNamingButtonHeld;

		private bool randomButtonHeld;

		private int currency;

		private ClickableComponent buyButton;

		private string headerString;

		private string nameString;

		private string descString;

		private MobileScrollbar newScrollbar;

		private MobileScrollbox scrollArea;

		private List<ClickableComponent> itemBox = new List<ClickableComponent>();

		private Microsoft.Xna.Framework.Rectangle clip;

		private int _drawAtX = -1;

		private int _drawAtY = -1;

		private int _lastTapX = -1;

		private int _lastTapY = -1;

		private int _selectedItemIndex = -1;

		private ClickableTextureComponent _selectedButton;

		private int selectedBuildingIndex
		{
			get
			{
				Farm farm = Game1.getLocationFromName("Farm") as Farm;
				int num = 0;
				foreach (Building building in farm.buildings)
				{
					if (_selectedBuilding == building)
					{
						return num;
					}
					num++;
				}
				return -1;
			}
		}

		public PurchaseAnimalsMenu(List<Object> stock)
			: base(Game1.viewport.Width / 2 - menuWidth / 2 - IClickableMenu.borderWidth * 2, (Game1.viewport.Height - menuHeight - IClickableMenu.borderWidth * 2) / 4, menuWidth + IClickableMenu.borderWidth * 2, menuHeight + IClickableMenu.borderWidth, showUpperRightCloseButton: true)
		{
			initializeUpperRightCloseButton();
			float num = (float)Game1.uiViewport.Width / 1280f;
			float num2 = (float)Game1.uiViewport.Height / 720f;
			headerX = (int)(num * 68f) + Game1.xEdge;
			headerY = (int)(num2 * 24f);
			headerWidth = (int)(num * 628f);
			scrollerX = headerX;
			scrollerY = (int)(num2 * 104f);
			scrollerWidth = headerWidth;
			scrollerHeight = (int)(num2 * 596f);
			descX = scrollerX + scrollerWidth + 24;
			descY = scrollerY;
			descWidth = Game1.uiViewport.Width - headerX - descX;
			descHeight = scrollerHeight;
			goldX = descX - 28 + (descWidth - 256) / 2;
			goldY = 0;
			buyWidth = (int)((double)descWidth * 0.75);
			buyX = descX + (descWidth - buyWidth) / 2;
			buyHeight = (int)(80f * num2);
			buyY = descY + descHeight - (int)((double)buyHeight * 1.5);
			buyButton = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(buyX, buyY, buyWidth, buyHeight), "");
			buyButtonVisible = true;
			currency = 0;
			headerString = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11354").Replace(":", "");
			clip = new Microsoft.Xna.Framework.Rectangle(scrollerX + 16, scrollerY + 16, scrollerWidth - 32, scrollerHeight - 32);
			scrollbarWidth = 24;
			itemHeight = 104;
			itemsPerPage = clip.Height / itemHeight;
			if (stock.Count > itemsPerPage)
			{
				itemWidth = clip.Width - 16 - scrollbarWidth;
				scrollbarVisible = true;
			}
			else
			{
				itemWidth = clip.Width - 16;
				scrollbarVisible = false;
			}
			newScrollbar = new MobileScrollbar(clip.X + clip.Width - scrollbarWidth - 16, clip.Y, 44, clip.Height, 0, 32);
			scrollArea = new MobileScrollbox(clipRect: new Microsoft.Xna.Framework.Rectangle(clip.X, clip.Y, clip.Width, clip.Height), boxX: clip.X + 8, boxY: clip.Y, boxWidth: itemWidth, boxHeight: clip.Height, boxContentHeight: (stock.Count - itemsPerPage) * itemHeight, scrollBar: newScrollbar);
			height += 64;
			for (int i = 0; i < stock.Count; i++)
			{
				itemBox.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(clip.X + 8, clip.Y + i * itemHeight, itemWidth, itemHeight), stock[i]));
				animalsToPurchase.Add(new ClickableTextureComponent(stock[i].salePrice().ToString() ?? "", new Microsoft.Xna.Framework.Rectangle(clip.X + 24, clip.Y + i * itemHeight + 20, 128, 64), null, stock[i].Name, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(i % 3 * 16 * 2, 448 + i / 3 * 16, 32, 16), 4f, stock[i].type.Value == null)
				{
					item = stock[i]
				});
			}
			currentlySelectedItem = -1;
			selectItem(0);
			menuHeight = 320;
			menuWidth = 448;
			textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
			textBox.X = Game1.uiViewport.Width / 2 - 192;
			textBox.Y = Game1.uiViewport.Height / 2;
			textBox.Width = 256;
			textBox.Height = 192;
			textBox.textLimit = 25;
			e = textBoxEnter;
			textBoxCC = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(textBox.X, textBox.Y, 192, 48), "")
			{
				myID = 104,
				rightNeighborID = 102,
				downNeighborID = 101
			};
			randomButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(textBox.X - 104, textBox.Y - 12, 80, 80), Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(87, 22, 20, 20), 4f, drawShadow: true);
			randomButtonHeld = false;
			doneNamingButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(textBox.X + textBox.Width + 32 + 4, textBox.Y - 12, 80, 80), Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(0, 0, 20, 20), 4f, drawShadow: true);
			doneNamingButtonHeld = false;
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
			tickButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(-100, -100, 80, 80), null, null, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(0, 0, 20, 20), 4f, drawShadow: true);
			tickButtonHeld = false;
			cancelButton = new ClickableTextureComponent("Cancel", new Microsoft.Xna.Framework.Rectangle(-100, -100, 80, 80), null, null, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 0, 20, 20), 4f, drawShadow: true);
			cancelButtonHeld = false;
		}

		public override bool shouldClampGamePadCursor()
		{
			return onFarm;
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public void textBoxEnter(TextBox sender)
		{
			if (!namingAnimal || sender == null)
			{
				return;
			}
			if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is PurchaseAnimalsMenu))
			{
				textBox.OnEnterPressed -= e;
				return;
			}
			string text = sender.Text.Trim();
			if (text.Length >= 1)
			{
				if (Utility.areThereAnyOtherAnimalsWithThisName(text))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11308"));
					return;
				}
				textBox.OnEnterPressed -= e;
				animalBeingPurchased.Name = text;
				animalBeingPurchased.displayName = text;
				animalBeingPurchased.home = newAnimalHome;
				animalBeingPurchased.homeLocation.Value = new Vector2((int)newAnimalHome.tileX, (int)newAnimalHome.tileY);
				animalBeingPurchased.setRandomPosition(animalBeingPurchased.home.indoors);
				(newAnimalHome.indoors.Value as AnimalHouse).animals.Add(animalBeingPurchased.myID, animalBeingPurchased);
				(newAnimalHome.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(animalBeingPurchased.myID);
				newAnimalHome = null;
				namingAnimal = false;
				Game1.player.Money -= priceOfAnimal;
				setUpForReturnAfterPurchasingAnimal();
			}
		}

		public void setUpForReturnAfterPurchasingAnimal()
		{
			LocationRequest locationRequest = Game1.getLocationRequest("AnimalShop");
			locationRequest.OnWarp += delegate
			{
				onFarm = false;
				Game1.player.viewingLocation.Value = null;
				if (okButton != null)
				{
					okButton.bounds.X = xPositionOnScreen + width + 4;
				}
				Game1.displayHUD = true;
				Game1.displayFarmer = true;
				freeze = false;
				textBox.OnEnterPressed -= e;
				textBox.Selected = false;
				Game1.viewportFreeze = false;
				marnieAnimalPurchaseMessage();
			};
			Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);
		}

		public void marnieAnimalPurchaseMessage()
		{
			exitThisMenu();
			Game1.player.forceCanMove();
			freeze = false;
			Game1.drawDialogue(Game1.getCharacterFromName("Marnie"), animalBeingPurchased.isMale() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11311", animalBeingPurchased.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11314", animalBeingPurchased.displayName));
		}

		public void setUpForAnimalPlacement()
		{
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.displayFarmer = false;
			Game1.currentLocation = Game1.getLocationFromName("Farm");
			Game1.player.viewingLocation.Value = "Farm";
			Game1.currentLocation.resetForPlayerEntry();
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.globalFadeToClear();
			onFarm = true;
			_selectedButton = null;
			freeze = false;
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Game1.viewport.Location = new Location(3136, 320);
			Game1.panScreen(0, 0);
		}

		public void setUpForReturnToShopMenu()
		{
			freeze = false;
			Game1.displayFarmer = true;
			LocationRequest locationRequest = Game1.getLocationRequest("AnimalShop");
			locationRequest.OnWarp += delegate
			{
				onFarm = false;
				Game1.player.viewingLocation.Value = null;
				Game1.displayHUD = true;
				Game1.viewportFreeze = false;
				namingAnimal = false;
				textBox.OnEnterPressed -= e;
				textBox.Selected = false;
				if (Game1.options.SnappyMenus)
				{
					snapToDefaultClickableComponent();
				}
			};
			Game1.warpFarmer(locationRequest, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.IsFading() || freeze)
			{
				return;
			}
			_lastTapX = -1;
			_lastTapY = -1;
			if (onFarm)
			{
				if (namingAnimal && upperRightCloseButton.containsPoint(x, y))
				{
					Game1.playSound("smallSelect");
					namingAnimal = false;
				}
				if ((!namingAnimal && (cancelButton.containsPoint(x, y) || tickButton.containsPoint(x, y))) || (namingAnimal && (randomButton.containsPoint(x, y) || doneNamingButton.containsPoint(x, y))))
				{
					Game1.playSound("smallSelect");
				}
			}
			if (!onFarm)
			{
				base.receiveLeftClick(x, y, playSound);
				if (scrollbarVisible)
				{
					scrollArea.receiveLeftClick(x, y);
					if (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y))
					{
						scrolling = true;
					}
				}
				for (int i = 0; i < itemBox.Count; i++)
				{
					if (itemBox[i].containsPoint(x, y))
					{
						selectItem(i);
					}
				}
			}
			_ = onFarm;
		}

		public override bool overrideSnappyMenuCursorMovementBan()
		{
			if (onFarm)
			{
				return !namingAnimal;
			}
			return false;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			int num = (int)Math.Ceiling((float)clip.Height / (float)itemHeight);
			int num2 = (int)Math.Floor((float)scrollArea.getYOffsetForScroll() / (float)itemHeight);
			int num3 = -num2 + num;
			if (!onFarm)
			{
				switch (b)
				{
				case Buttons.DPadUp:
				case Buttons.LeftThumbstickUp:
					_selectedItemIndex = Math.Max(0, _selectedItemIndex - 1);
					selectItem(_selectedItemIndex);
					if (_selectedItemIndex < -num2)
					{
						scrollArea.yOffsetForScroll += itemHeight;
						updateItemButtons();
					}
					break;
				case Buttons.DPadDown:
				case Buttons.LeftThumbstickDown:
					_selectedItemIndex = Math.Min(itemBox.Count - 1, _selectedItemIndex + 1);
					selectItem(_selectedItemIndex);
					if (_selectedItemIndex >= num3 - 1)
					{
						scrollArea.yOffsetForScroll -= itemHeight;
						updateItemButtons();
					}
					break;
				case Buttons.A:
					if (_selectedItemIndex > -1)
					{
						OnClickBuyAnimal();
					}
					break;
				case Buttons.B:
					base.receiveGamePadButton(b);
					break;
				}
			}
			else
			{
				if (!onFarm)
				{
					return;
				}
				if (namingAnimal)
				{
					switch (b)
					{
					case Buttons.DPadLeft:
						if (_selectedButton == randomButton)
						{
							_selectedButton = upperRightCloseButton;
						}
						else if (_selectedButton == upperRightCloseButton)
						{
							_selectedButton = doneNamingButton;
						}
						else
						{
							_selectedButton = randomButton;
						}
						break;
					case Buttons.DPadRight:
						if (_selectedButton == doneNamingButton)
						{
							_selectedButton = upperRightCloseButton;
						}
						else if (_selectedButton == upperRightCloseButton)
						{
							_selectedButton = randomButton;
						}
						else
						{
							_selectedButton = doneNamingButton;
						}
						break;
					case Buttons.A:
						if (_selectedButton == doneNamingButton)
						{
							OnClickDoneNaming();
						}
						else if (_selectedButton == randomButton)
						{
							OnClickRandomNameButton();
						}
						else if (_selectedButton == upperRightCloseButton)
						{
							Game1.playSound("smallSelect");
							namingAnimal = false;
						}
						break;
					}
				}
				else
				{
					if (!(Game1.currentLocation is Farm farm))
					{
						return;
					}
					switch (b)
					{
					case Buttons.DPadUp:
					case Buttons.LeftThumbstickUp:
					{
						UnhighlightBuildings();
						int num4 = selectedBuildingIndex;
						num4 = ((num4 > 0) ? (num4 - 1) : (farm.buildings.Count - 1));
						if (num4 < 0)
						{
							break;
						}
						int num5 = 0;
						{
							foreach (Building building in farm.buildings)
							{
								if (num5 == num4)
								{
									SelectBuilding(building);
									break;
								}
								num5++;
							}
							break;
						}
					}
					case Buttons.DPadDown:
					case Buttons.LeftThumbstickDown:
					{
						UnhighlightBuildings();
						int num6 = selectedBuildingIndex;
						num6 = ((num6 >= 0 && num6 < farm.buildings.Count - 1) ? (num6 + 1) : 0);
						if (num6 >= farm.buildings.Count)
						{
							break;
						}
						int num7 = 0;
						{
							foreach (Building building2 in farm.buildings)
							{
								if (num7 == num6)
								{
									SelectBuilding(building2);
									break;
								}
								num7++;
							}
							break;
						}
					}
					case Buttons.DPadLeft:
						if (_selectedButton == cancelButton && _selectedBuilding != null && _selectedBuilding.buildingType.Contains(animalBeingPurchased.buildingTypeILiveIn) && !(_selectedBuilding.indoors.Value as AnimalHouse).isFull())
						{
							_selectedButton = tickButton;
						}
						else
						{
							_selectedButton = cancelButton;
						}
						break;
					case Buttons.DPadRight:
						if (_selectedButton == tickButton)
						{
							_selectedButton = cancelButton;
						}
						else if (_selectedBuilding != null && _selectedBuilding.buildingType.Contains(animalBeingPurchased.buildingTypeILiveIn) && !(_selectedBuilding.indoors.Value as AnimalHouse).isFull())
						{
							_selectedButton = tickButton;
						}
						break;
					case Buttons.A:
						if (_selectedButton == tickButton && _selectedBuilding != null)
						{
							PlaceAnimalInSelectedBuilding();
						}
						else if (_selectedButton == cancelButton)
						{
							OnClickCancelButton();
						}
						break;
					}
				}
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.globalFade || freeze)
			{
				return;
			}
			if (!Game1.globalFade && onFarm)
			{
				if (!namingAnimal)
				{
					if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose() && !Game1.IsFading())
					{
						setUpForReturnToShopMenu();
					}
					else if (!Game1.options.SnappyMenus)
					{
						if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
						{
							Game1.panScreen(0, 4);
						}
						else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
						{
							Game1.panScreen(4, 0);
						}
						else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
						{
							Game1.panScreen(0, -4);
						}
						else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
						{
							Game1.panScreen(-4, 0);
						}
					}
				}
				else if (Game1.options.SnappyMenus)
				{
					if (!textBox.Selected && Game1.options.doesInputListContain(Game1.options.menuButton, key))
					{
						setUpForReturnToShopMenu();
						Game1.playSound("smallSelect");
					}
					else if (!textBox.Selected || !Game1.options.doesInputListContain(Game1.options.menuButton, key))
					{
						base.receiveKeyPress(key);
					}
				}
			}
			else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.IsFading())
			{
				if (readyToClose())
				{
					Game1.player.forceCanMove();
					Game1.exitActiveMenu();
					Game1.playSound("bigDeSelect");
				}
			}
			else if (Game1.options.SnappyMenus)
			{
				base.receiveKeyPress(key);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (!onFarm)
			{
				scrollArea.update(time);
				updateItemButtons();
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public static string getAnimalTitle(string name)
		{
			if (name != null)
			{
				switch (name.Length)
				{
				case 7:
					switch (name[0])
					{
					case 'C':
						if (!(name == "Chicken"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5922");
					case 'O':
						if (!(name == "Ostrich"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Ostrich_Name");
					}
					break;
				case 4:
					switch (name[0])
					{
					case 'D':
						if (!(name == "Duck"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5937");
					case 'G':
						if (!(name == "Goat"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5933");
					}
					break;
				case 6:
					if (!(name == "Rabbit"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5945");
				case 9:
					if (!(name == "Dairy Cow"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5927");
				case 3:
					if (!(name == "Pig"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5948");
				case 5:
					if (!(name == "Sheep"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5942");
				}
			}
			return "";
		}

		public static string getAnimalDescription(string name)
		{
			if (name != null)
			{
				switch (name.Length)
				{
				case 7:
					switch (name[0])
					{
					case 'C':
						if (!(name == "Chicken"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11334") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
					case 'O':
						if (!(name == "Ostrich"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Ostrich_Description") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
					}
					break;
				case 4:
					switch (name[0])
					{
					case 'D':
						if (!(name == "Duck"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11337") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
					case 'G':
						if (!(name == "Goat"))
						{
							break;
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11349") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
					}
					break;
				case 6:
					if (!(name == "Rabbit"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11340") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
				case 9:
					if (!(name == "Dairy Cow"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11343") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
				case 3:
					if (!(name == "Pig"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11346") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
				case 5:
					if (!(name == "Sheep"))
					{
						break;
					}
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11352") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
				}
			}
			return "";
		}

		public override void draw(SpriteBatch b)
		{
			if (!onFarm && !Game1.dialogueUp && !Game1.IsFading())
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				IClickableMenu.drawTextureBox(b, scrollerX, scrollerY, scrollerWidth, scrollerHeight, Color.White);
				IClickableMenu.drawTextureBox(b, descX, descY, descWidth, descHeight, Color.White);
				SpriteText.drawScrollText(b, headerString, Game1.dialogueFont, headerX, headerY, headerWidth);
				if (buyButtonVisible)
				{
					IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.dialogueFont, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(256, 256, 10, 10), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(193 + currency * 9, 373, 9, 10), priceOfAnimal.ToString() ?? "", buyX, buyY, buyWidth, buyHeight, Color.White, 4f, drawShadow: true, iconLeft: false, isClickable: true, buyButtonisHeld);
				}
				Game1.dayTimeMoneyBox.drawMoneyBox(b, goldX, goldY, oldGFX: true);
				scrollArea.setUpForScrollBoxDrawing(b);
				int yOffsetForScroll = scrollArea.getYOffsetForScroll();
				for (int i = 0; i < itemBox.Count; i++)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(384, 396, 15, 15), itemBox[i].bounds.X, itemBox[i].bounds.Y + 4, itemBox[i].bounds.Width, itemBox[i].bounds.Height - 8, (i == currentlySelectedItem) ? Color.Wheat : Color.White, 4f, drawShadow: false);
					animalsToPurchase[i].draw(b, ((animalsToPurchase[i].item as Object).type.Value != null) ? (Color.Black * 0.4f) : Color.White, 0.087f);
					Utility.drawTextWithShadow(b, getAnimalTitle(itemBox[i].item.Name), Game1.dialogueFont, new Vector2(itemBox[i].bounds.X + 16 + 128, itemBox[i].bounds.Y + 32), Game1.textColor, 1f, 0.0871f);
					if (i == _selectedItemIndex)
					{
						IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), itemBox[i].bounds.X + 4, itemBox[i].bounds.Y + 8, itemBox[i].bounds.Width - 8, itemBox[i].bounds.Height - 16, Color.White, 4f, drawShadow: false);
					}
				}
				scrollArea.finishScrollBoxDrawing(b);
				if (scrollbarVisible)
				{
					newScrollbar.draw(b);
				}
				string text = "";
				if ((animalsToPurchase[currentlySelectedItem].item as Object).type.Value != null)
				{
					text = text + "\n\n" + (animalsToPurchase[currentlySelectedItem].item as Object).type.Value;
				}
				if (animalBeingPurchased != null && priceOfAnimal > Game1.player.Money)
				{
					text = text + "\n\n" + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11326", animalBeingPurchased.displayType);
				}
				IClickableMenu.drawMobileToolTip(b, descX + 16, descY + 16, descWidth - 32, descHeight - 32, 34, descString + text, nameString, itemBox[currentlySelectedItem].item, heldItem: false, -1, 0, -1, -1, null, itemBox[currentlySelectedItem].item.salePrice());
			}
			else if (!Game1.IsFading() && onFarm)
			{
				if (_selectedBuilding != null && _selectedBuilding.buildingType.Contains(animalBeingPurchased.buildingTypeILiveIn) && !(_selectedBuilding.indoors.Value as AnimalHouse).isFull())
				{
					SetTickButtonBounds();
					tickButton.draw(b);
				}
				SetCancelButtonBounds();
				cancelButton.draw(b);
				if (_selectedButton != null)
				{
					IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), _selectedButton.bounds.X - 4, _selectedButton.bounds.Y - 4, _selectedButton.bounds.Width + 8, _selectedButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
				}
				string s = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11355", animalBeingPurchased.displayHouse, animalBeingPurchased.displayType);
				SpriteText.drawStringWithScrollBackground(b, s, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, 16);
				if (namingAnimal)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
					string text2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11357");
					SpriteText.drawStringWithScrollCenteredAt(b, text2, Game1.uiViewport.Width / 2, Game1.uiViewport.Height / 2 - 128 + 8, text2);
					textBox.Draw(b);
					doneNamingButton.draw(b);
					randomButton.draw(b);
					if (_selectedButton != null)
					{
						IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), _selectedButton.bounds.X - 4, _selectedButton.bounds.Y - 4, _selectedButton.bounds.Width + 8, _selectedButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
					}
					upperRightCloseButton.draw(b);
				}
			}
			if (hovered != null)
			{
				if ((hovered.item as Object).Type != null)
				{
					IClickableMenu.drawHoverText(b, Game1.parseText((hovered.item as Object).Type, Game1.dialogueFont, 320), Game1.dialogueFont);
				}
				else
				{
					string animalTitle = getAnimalTitle(hovered.hoverText);
					SpriteText.drawStringWithScrollCenteredAt(b, animalTitle, xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 64, yPositionOnScreen + height + -32 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "Truffle Pig");
					SpriteText.drawStringWithScrollCenteredAt(b, "$" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", hovered.item.salePrice()), xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 128, yPositionOnScreen + height + 64 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "$99999999g", (Game1.player.Money >= hovered.item.salePrice()) ? 1f : 0.5f);
					string animalDescription = getAnimalDescription(hovered.hoverText);
					IClickableMenu.drawHoverText(b, Game1.parseText(animalDescription, Game1.smallFont, 320), Game1.smallFont, 0, 0, -1, animalTitle);
				}
			}
			Game1.mouseCursorTransparency = 1f;
			if (!onFarm)
			{
				base.draw(b);
			}
		}

		private void SelectBuilding(Building building)
		{
			_selectedBuilding = building;
			HighlightSelectedBuilding();
			SetCurrentViewportTargetToCenterOnBuilding(_selectedBuilding);
			Game1.playSound("smallSelect");
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if ((Game1.options.gamepadControls && direction == -1) || direction == 1)
			{
				direction *= 100;
			}
			base.receiveScrollWheelAction(direction);
			scrollArea.receiveScrollWheelAction(direction);
		}

		private void SetTickButtonBounds()
		{
			tickButton.bounds.X = Game1.uiViewport.Width - Game1.xEdge - tickButton.bounds.Width - 50;
			tickButton.bounds.Y = Game1.uiViewport.Height - tickButton.bounds.Height - 50;
			if (tickButtonHeld)
			{
				tickButton.bounds.X += 4;
				tickButton.bounds.Y += 4;
			}
		}

		private void SetCancelButtonBounds()
		{
			cancelButton.bounds.X = Game1.uiViewport.Width - Game1.xEdge - tickButton.bounds.Width - 50 - cancelButton.bounds.Width - 10;
			cancelButton.bounds.Y = Game1.uiViewport.Height - cancelButton.bounds.Height - 50;
			if (cancelButtonHeld)
			{
				cancelButton.bounds.X += 4;
				cancelButton.bounds.Y += 4;
			}
		}

		public void selectItem(int i)
		{
			if (itemBox[i] != null)
			{
				int num = (priceOfAnimal = itemBox[i].item.salePrice());
				currentlySelectedItem = i;
				Game1.playSound("smallSelect");
				nameString = getAnimalTitle(itemBox[i].item.Name);
				descString = getAnimalDescription(itemBox[i].item.Name).Replace("\n", " ");
				if ((itemBox[i].item as Object).type.Value == null && Game1.player.Money >= num)
				{
					buyButtonVisible = true;
				}
				else
				{
					buyButtonVisible = false;
				}
			}
		}

		private void OnClickBuyAnimal()
		{
			Game1.globalFadeToBlack(setUpForAnimalPlacement);
			Game1.playSound("smallSelect");
			onFarm = true;
			animalBeingPurchased = new FarmAnimal(itemBox[currentlySelectedItem].item.Name, Game1.multiplayer.getNewID(), Game1.player.uniqueMultiplayerID);
		}

		private void PlaceAnimalInSelectedBuilding()
		{
			if (_selectedBuilding.buildingType.Contains(animalBeingPurchased.buildingTypeILiveIn))
			{
				if ((_selectedBuilding.indoors.Value as AnimalHouse).isFull())
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11321"));
				}
				else if ((byte)animalBeingPurchased.harvestType != 2)
				{
					namingAnimal = true;
					_selectedButton = null;
					newAnimalHome = _selectedBuilding;
					if (animalBeingPurchased.sound != null && Game1.soundBank != null)
					{
						ICue cue = Game1.soundBank.GetCue(animalBeingPurchased.sound);
						cue.SetVariable("Pitch", 1200 + Game1.random.Next(-200, 201));
						cue.Play();
					}
					textBox.OnEnterPressed += e;
					textBox.Text = animalBeingPurchased.displayName;
					Game1.keyboardDispatcher.Subscriber = textBox;
					if (Game1.options.SnappyMenus)
					{
						currentlySnappedComponent = getComponentWithID(104);
						snapCursorToCurrentSnappedComponent();
					}
				}
				else if (Game1.player.Money >= priceOfAnimal)
				{
					newAnimalHome = _selectedBuilding;
					animalBeingPurchased.home = newAnimalHome;
					animalBeingPurchased.homeLocation.Value = new Vector2((int)newAnimalHome.tileX, (int)newAnimalHome.tileY);
					animalBeingPurchased.setRandomPosition(animalBeingPurchased.home.indoors);
					(newAnimalHome.indoors.Value as AnimalHouse).animals.Add(animalBeingPurchased.myID, animalBeingPurchased);
					(newAnimalHome.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(animalBeingPurchased.myID);
					newAnimalHome = null;
					namingAnimal = false;
					if (animalBeingPurchased.sound != null && Game1.soundBank != null)
					{
						ICue cue2 = Game1.soundBank.GetCue(animalBeingPurchased.sound);
						cue2.SetVariable("Pitch", 1200 + Game1.random.Next(-200, 201));
						cue2.Play();
					}
					Game1.player.Money -= priceOfAnimal;
					Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11324", animalBeingPurchased.displayType), Color.LimeGreen, 3500f));
					animalBeingPurchased = new FarmAnimal(animalBeingPurchased.type.Value, Game1.multiplayer.getNewID(), Game1.player.uniqueMultiplayerID);
				}
				else if (Game1.player.Money < priceOfAnimal)
				{
					Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
				}
			}
			else
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11326", animalBeingPurchased.displayType));
			}
		}

		private void resetButtons()
		{
			buyButtonisHeld = false;
			cancelButtonHeld = false;
			tickButtonHeld = false;
			tickButton.drawShadow = true;
			cancelButton.drawShadow = true;
			randomButton.drawShadow = true;
			doneNamingButton.drawShadow = true;
			doneNamingButtonHeld = false;
			randomButtonHeld = false;
			doneNamingButton.bounds.X = textBox.X + textBox.Width + 32 + 4;
			doneNamingButton.bounds.Y = textBox.Y - 12;
			randomButton.bounds.X = textBox.X - 104;
			randomButton.bounds.Y = textBox.Y - 12;
		}

		private void updateItemButtons()
		{
			for (int i = 0; i < itemBox.Count; i++)
			{
				itemBox[i].bounds.Y = scrollArea.getYOffsetForScroll() + clip.Y + i * itemHeight;
				animalsToPurchase[i].bounds.Y = scrollArea.getYOffsetForScroll() + clip.Y + i * itemHeight + 20;
			}
		}

		private void TestToPan(int x, int y)
		{
			if (onFarm && !Game1.globalFade && (x <= cancelButton.bounds.X - cancelButton.bounds.Width || y <= cancelButton.bounds.Y - cancelButton.bounds.Height) && !tickButtonHeld)
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

		public static void SetCurrentViewportTargetToCenterOnBuilding(Building building)
		{
			if (Game1.currentLocation != null)
			{
				int num = Math.Min(0, (Game1.currentLocation.Map.DisplayWidth - IClickableMenu.viewport.Width) / 2);
				if (Game1.options.verticalToolbar && Game1.displayHUD && Toolbar.visible)
				{
					num = Math.Min(-(int)((float)Toolbar.toolbarWidth / Game1.options.zoomLevel), (Game1.currentLocation.Map.DisplayWidth - IClickableMenu.viewport.Width) / 2);
				}
				int num2 = Game1.currentLocation.Map.DisplayWidth - IClickableMenu.viewport.Width;
				int num3 = Math.Min(0, (Game1.currentLocation.Map.DisplayHeight - IClickableMenu.viewport.Height) / 2);
				int num4 = Game1.currentLocation.Map.DisplayHeight - IClickableMenu.viewport.Height;
				Game1.currentViewportTarget.X = (int)building.tileX * 64 + (int)building.tilesWide * 64 / 2 - IClickableMenu.viewport.Width / 2;
				Game1.currentViewportTarget.Y = (int)building.tileY * 64 + (int)building.tilesHigh * 64 / 2 - IClickableMenu.viewport.Height / 2;
				Game1.currentViewportTarget.X = Math.Max(num, Math.Min(Game1.currentViewportTarget.X, num2));
				Game1.currentViewportTarget.Y = Math.Max(num3, Math.Min(Game1.currentViewportTarget.Y, num4));
				Game1.viewport.X = (int)Game1.currentViewportTarget.X;
				Game1.viewport.Y = (int)Game1.currentViewportTarget.Y;
			}
		}

		private void UnhighlightBuildings()
		{
			Farm farm = Game1.getLocationFromName("Farm") as Farm;
			foreach (Building building in farm.buildings)
			{
				building.color.Value = Color.White;
			}
		}

		private void HighlightSelectedBuilding()
		{
			if (_selectedBuilding.buildingType.Contains(animalBeingPurchased.buildingTypeILiveIn) && !(_selectedBuilding.indoors.Value as AnimalHouse).isFull())
			{
				_selectedBuilding.color.Value = Color.LightGreen * 0.8f;
				return;
			}
			if (_selectedBuilding.buildingType.Value.Contains(animalBeingPurchased.buildingTypeILiveIn.Value))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11321"));
			}
			else
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11326", animalBeingPurchased.displayType));
			}
			_selectedBuilding.color.Value = Color.Red * 0.8f;
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (!onFarm)
			{
				buyButtonisHeld = buyButton.containsPoint(x, y);
				if (scrollArea.panelScrolling)
				{
					updateItemButtons();
				}
				scrollArea.leftClickHeld(x, y);
				if (scrollbarVisible && scrolling && (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y)))
				{
					float num = newScrollbar.setY(y);
					scrollArea.setYOffsetForScroll(-(int)(num * (float)(itemBox.Count - itemsPerPage) * (float)itemHeight / 100f));
					updateItemButtons();
				}
			}
			else
			{
				if (PinchZoom.Instance.CheckForPinchZoom())
				{
					return;
				}
				TestToPan(Game1.input.GetMouseState().X, Game1.input.GetMouseState().Y);
				if (tickButton.containsPoint(x, y))
				{
					tickButtonHeld = true;
					SetTickButtonBounds();
					tickButton.drawShadow = false;
				}
				else
				{
					tickButtonHeld = false;
					SetTickButtonBounds();
					tickButton.drawShadow = true;
				}
				if (cancelButton.containsPoint(x, y))
				{
					cancelButtonHeld = true;
					cancelButton.drawShadow = false;
				}
				else
				{
					cancelButtonHeld = false;
					cancelButton.drawShadow = true;
				}
			}
			if (namingAnimal)
			{
				if (doneNamingButton.containsPoint(x, y))
				{
					doneNamingButton.bounds.X = textBox.X + textBox.Width + 32 + 4 - 4;
					doneNamingButton.bounds.Y = textBox.Y - 12 + 4;
					doneNamingButtonHeld = true;
					doneNamingButton.drawShadow = false;
				}
				else
				{
					doneNamingButton.bounds.X = textBox.X + textBox.Width + 32 + 4;
					doneNamingButton.bounds.Y = textBox.Y - 12;
					doneNamingButtonHeld = false;
					doneNamingButton.drawShadow = true;
				}
				if (randomButton.containsPoint(x, y))
				{
					randomButton.bounds.X = textBox.X - 104 - 4;
					randomButton.bounds.Y = textBox.Y - 12 + 4;
					randomButtonHeld = true;
					randomButton.drawShadow = false;
				}
				else
				{
					randomButton.bounds.X = textBox.X - 104;
					randomButton.bounds.Y = textBox.Y - 12;
					randomButtonHeld = false;
					randomButton.drawShadow = true;
				}
			}
		}

		private void OnClickRandomNameButton()
		{
			animalBeingPurchased.Name = Dialogue.randomName();
			animalBeingPurchased.displayName = animalBeingPurchased.name;
			textBox.Text = animalBeingPurchased.displayName;
			Game1.playSound("drumkit6");
		}

		private void OnClickDoneNaming()
		{
			_selectedButton = null;
			textBoxEnter(textBox);
			Game1.playSound("smallSelect");
		}

		private void OnClickCancelButton()
		{
			_selectedButton = null;
			setUpForReturnToShopMenu();
			Game1.playSound("smallSelect");
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (buyButtonVisible && buyButton.containsPoint(x, y) && buyButtonisHeld && !readOnly)
			{
				OnClickBuyAnimal();
			}
			if (onFarm && !namingAnimal && cancelButtonHeld && cancelButton.containsPoint(x, y))
			{
				OnClickCancelButton();
			}
			if (namingAnimal)
			{
				if (doneNamingButton.containsPoint(x, y))
				{
					OnClickDoneNaming();
				}
				else if (namingAnimal && randomButton.containsPoint(x, y))
				{
					OnClickRandomNameButton();
				}
				textBox.Update();
			}
			if (onFarm)
			{
				if (_selectedBuilding != null && tickButton != null && tickButton.containsPoint(x, y))
				{
					PlaceAnimalInSelectedBuilding();
				}
				if (!namingAnimal)
				{
					Vector2 tile = new Vector2((Game1.getMouseX(ui_scale: false) + Game1.viewport.X) / 64, (Game1.getMouseY(ui_scale: false) + Game1.viewport.Y) / 64);
					Farm farm = Game1.getLocationFromName("Farm") as Farm;
					Building building = farm.getBuildingAt(tile);
					if (building != null && building.isUnderConstruction())
					{
						building = null;
					}
					if (_selectedBuilding == building)
					{
						_selectedBuilding = null;
					}
					else
					{
						_selectedBuilding = building;
					}
					if (building == null)
					{
						_selectedBuilding = null;
					}
					UnhighlightBuildings();
					if (_selectedBuilding != null)
					{
						HighlightSelectedBuilding();
					}
				}
			}
			resetButtons();
			base.releaseLeftClick(x, y);
			scrollArea.releaseLeftClick(x, y);
		}
	}
}
