using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Mobile;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class AnimalQueryMenu : IClickableMenu
	{
		private enum Button
		{
			None,
			Move,
			Sell,
			AllowReproduction,
			Cancel,
			Tick
		}

		public const int region_okButton = 101;

		public const int region_love = 102;

		public const int region_sellButton = 103;

		public const int region_moveHomeButton = 104;

		public const int region_noButton = 105;

		public const int region_allowReproductionButton = 106;

		public const int region_fullnessHover = 107;

		public const int region_happinessHover = 108;

		public const int region_loveHover = 109;

		public const int region_textBoxCC = 110;

		public const int region_closeButton = 111;

		public int panelWidth;

		public int buttonWidth;

		public int buttonOffset;

		public int yBoxDepth;

		public int xBoxOff;

		public int yBoxPos;

		public int buttonHeight;

		public int dialogueBoxPad = 16;

		public ClickableComponent moveHomeButton;

		public ClickableComponent sellButton;

		public ClickableComponent allowReproductionButton;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent love;

		public ClickableTextureComponent yesButton;

		public ClickableTextureComponent noButton;

		public ClickableTextureComponent tickButton;

		public ClickableTextureComponent cancelButton;

		private Building _selectedBuilding;

		public int mobTileSize = 64;

		public float widthMod;

		public float heightMod;

		private int _drawAtX = -1;

		private int _drawAtY = -1;

		private int _lastTapX = -1;

		private int _lastTapY = -1;

		private bool tickButtonHeld;

		private bool cancelButtonHeld;

		private bool drawTickButton;

		public new static int width = 384;

		public new static int height = 512;

		private FarmAnimal animal;

		private TextBox textBox;

		private TextBoxEvent e;

		public ClickableComponent fullnessHover;

		public ClickableComponent happinessHover;

		public ClickableComponent loveHover;

		public ClickableComponent textBoxCC;

		private double fullnessLevel;

		private double happinessLevel;

		private double loveLevel;

		private bool confirmingSell;

		private bool movingAnimal;

		private string hoverText = "";

		private string parentName;

		private bool moveHeld;

		private bool sellHeld;

		private bool pregHeld;

		public ClickableTextureComponent closeButton;

		private Button _selectedButton;

		private bool _clickedCancel;

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

		public AnimalQueryMenu(FarmAnimal animal)
			: base(Game1.uiViewport.Width / 2 - width / 2, Game1.uiViewport.Height / 2 - height / 2, width, height)
		{
			Game1.player.Halt();
			Game1.player.faceGeneralDirection(animal.Position);
			width = 384;
			height = 512;
			this.animal = animal;
			width = Game1.uiViewport.Width;
			height = Game1.uiViewport.Height;
			widthMod = (float)width / 1280f;
			heightMod = (float)height / 720f;
			moveHeld = (sellHeld = (pregHeld = false));
			buttonHeight = Math.Min(80, (int)(80f * heightMod));
			panelWidth = (int)((float)width * 0.7f);
			int num = Math.Min((int)((float)height * heightMod * (float)buttonHeight / 720f / 10f), buttonHeight / 4);
			yBoxDepth = 330;
			buttonWidth = (int)((float)panelWidth * 0.8f);
			textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
			textBox.Width = panelWidth;
			textBox.X = (width - panelWidth) / 2;
			textBox.Y = num * 4;
			textBox.isScroll = true;
			textBox.textLimit = 15;
			textBox.Height = mobTileSize * height / 720;
			xBoxOff = (width - panelWidth) / 2 - dialogueBoxPad;
			yBoxPos = textBox.Y + num;
			buttonOffset = (panelWidth - buttonWidth) / 2;
			textBoxCC = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(textBox.X, textBox.Y, textBox.Width, mobTileSize), "")
			{
				myID = 110,
				downNeighborID = 104
			};
			textBox.Text = animal.displayName;
			Game1.keyboardDispatcher.Subscriber = textBox;
			textBox.Selected = false;
			if ((long)animal.parentId != -1)
			{
				FarmAnimal farmAnimal = Utility.getAnimal(animal.parentId);
				if (farmAnimal != null)
				{
					parentName = farmAnimal.displayName;
				}
			}
			if (animal.sound.Value != null && Game1.soundBank != null)
			{
				ICue cue = Game1.soundBank.GetCue(animal.sound.Value);
				cue.SetVariable("Pitch", 1200 + Game1.random.Next(-200, 201));
				cue.Play();
			}
			int x = (width - panelWidth) / 2 + buttonOffset;
			int num2 = yBoxPos + yBoxDepth + num * 5;
			closeButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(width - 68 - Game1.xEdge, 0, 68, 68), Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(62, 0, 17, 17), 4f, drawShadow: true);
			moveHomeButton = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(x, num2, buttonWidth, buttonHeight), "moveHomeButton");
			num2 += num + buttonHeight;
			sellButton = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(x, num2, buttonWidth, buttonHeight), "sellButton");
			num2 += num + buttonHeight;
			if (!animal.isBaby() && !animal.isCoopDweller())
			{
				allowReproductionButton = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(x, num2, buttonWidth, buttonHeight), "reproButton");
			}
			okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(width / 1280 * xPositionOnScreen + width + 4 - 400, height / 720 * yPositionOnScreen + height - 64 - IClickableMenu.borderWidth, 128, 128), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 101,
				upNeighborID = 103
			};
			tickButton = new ClickableTextureComponent("OK", new Microsoft.Xna.Framework.Rectangle(-100, -100, 80, 80), null, null, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(0, 0, 20, 20), 4f);
			cancelButton = new ClickableTextureComponent("Cancel", new Microsoft.Xna.Framework.Rectangle(-100, -100, 80, 80), null, null, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 0, 20, 20), 4f);
			fullnessLevel = (float)(int)(byte)animal.fullness / 255f;
			if (animal.home != null && animal.home.indoors.Value != null)
			{
				int num3 = animal.home.indoors.Value.numberOfObjectsWithName("Hay");
				if (num3 > 0)
				{
					int count = (animal.home.indoors.Value as AnimalHouse).animalsThatLiveHere.Count;
					fullnessLevel = Math.Min(1.0, fullnessLevel + (double)num3 / (double)count);
				}
			}
			else
			{
				Utility.fixAllAnimals();
			}
			happinessLevel = (float)(int)(byte)animal.happiness / 255f;
			loveLevel = (float)(int)animal.friendshipTowardFarmer / 1000f;
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(101);
			snapCursorToCurrentSnappedComponent();
		}

		public void textBoxEnter(TextBox sender)
		{
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (Game1.options.menuButton.Contains(new InputButton(key)) && (textBox == null || !textBox.Selected))
			{
				Game1.playSound("smallSelect");
				if (readyToClose())
				{
					Game1.exitActiveMenu();
					if (textBox.Text.Length > 0 && !Utility.areThereAnyOtherAnimalsWithThisName(textBox.Text))
					{
						animal.displayName = textBox.Text;
						animal.Name = textBox.Text;
					}
				}
				else if (movingAnimal)
				{
					Game1.globalFadeToBlack(prepareForReturnFromPlacement);
				}
			}
			else if (Game1.options.SnappyMenus && (!Game1.options.menuButton.Contains(new InputButton(key)) || textBox == null || !textBox.Selected))
			{
				base.receiveKeyPress(key);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		public void finishedPlacingAnimal()
		{
			Game1.exitActiveMenu();
			Game1.currentLocation = Game1.player.currentLocation;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear();
			Game1.displayHUD = true;
			Game1.viewportFreeze = false;
			Game1.displayFarmer = true;
			Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_HomeChanged"), Color.LimeGreen, 3500f));
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (movingAnimal)
			{
				if (drawTickButton && tickButton != null && tickButton.containsPoint(x, y) && _selectedBuilding.buildingType.Contains(animal.buildingTypeILiveIn) && !(_selectedBuilding.indoors.Value as AnimalHouse).isFull())
				{
					tickButtonHeld = true;
					cancelButtonHeld = false;
					Game1.playSound("smallSelect");
					return;
				}
				if (cancelButton != null && cancelButton.containsPoint(x, y))
				{
					cancelButtonHeld = true;
					tickButtonHeld = false;
					Game1.playSound("smallSelect");
					return;
				}
				_lastTapX = -1;
				_lastTapY = -1;
				Vector2 tile = new Vector2((Game1.getMouseX(ui_scale: false) + Game1.viewport.X) / 64, (Game1.getMouseY(ui_scale: false) + Game1.viewport.Y) / 64);
				Farm farm = Game1.getLocationFromName("Farm") as Farm;
				Building buildingAt = farm.getBuildingAt(tile);
				drawTickButton = false;
				foreach (Building building in farm.buildings)
				{
					building.color.Value = Color.White;
				}
				if (_selectedBuilding == buildingAt || buildingAt == null)
				{
					_selectedBuilding = null;
				}
				else
				{
					if (buildingAt == null)
					{
						return;
					}
					_selectedBuilding = buildingAt;
					if (_selectedBuilding.buildingType.Contains(animal.buildingTypeILiveIn) && !(_selectedBuilding.indoors.Value as AnimalHouse).isFull())
					{
						_selectedBuilding.color.Value = Color.LightGreen * 0.8f;
						drawTickButton = true;
						Game1.playSound("smallSelect");
						return;
					}
					_selectedBuilding.color.Value = Color.Red * 0.8f;
					if (!_selectedBuilding.buildingType.Contains(animal.buildingTypeILiveIn))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_CantLiveThere", animal.shortDisplayType()));
					}
					else if ((_selectedBuilding.indoors.Value as AnimalHouse).isFull())
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_BuildingFull"));
					}
					else if (_selectedBuilding.Equals(animal.home))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_AlreadyHome"));
					}
					Game1.playSound("bigDeSelect");
				}
				return;
			}
			if (confirmingSell)
			{
				if (yesButton.containsPoint(x, y))
				{
					OnClickYes();
				}
				else if (noButton.containsPoint(x, y))
				{
					OnClickNo();
				}
				return;
			}
			if (closeButton != null && closeButton.containsPoint(x, y) && readyToClose())
			{
				Game1.exitActiveMenu();
				if (textBox.Text.Length > 0 && !Utility.areThereAnyOtherAnimalsWithThisName(textBox.Text))
				{
					animal.displayName = textBox.Text;
					animal.name.Value = textBox.Text;
				}
				Game1.playSound("smallSelect");
			}
			if (tickButton != null && tickButton.containsPoint(x, y) && readyToClose())
			{
				Game1.exitActiveMenu();
				if (textBox.Text.Length > 0 && !Utility.areThereAnyOtherAnimalsWithThisName(textBox.Text))
				{
					animal.displayName = textBox.Text;
					animal.Name = textBox.Text;
				}
				Game1.playSound("smallSelect");
			}
			if (sellButton.containsPoint(x, y))
			{
				sellHeld = true;
				Game1.playSound("smallSelect");
			}
			if (moveHomeButton.containsPoint(x, y))
			{
				moveHeld = true;
				Game1.playSound("smallSelect");
			}
			if (allowReproductionButton != null && allowReproductionButton.containsPoint(x, y))
			{
				pregHeld = true;
			}
			textBox.Update();
		}

		public void prepareForAnimalPlacement()
		{
			_clickedCancel = false;
			SetTickButtonBounds();
			SetCancelButtonBounds();
			movingAnimal = true;
			Game1.currentLocation = Game1.getLocationFromName("Farm");
			Game1.globalFadeToClear();
			SetOKButtonBounds();
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Game1.viewport.Location = new Location(3136, 320);
			Game1.panScreen(0, 0);
			Game1.currentLocation.resetForPlayerEntry();
			Game1.displayFarmer = false;
		}

		public void prepareForReturnFromPlacement()
		{
			Game1.currentLocation = Game1.player.currentLocation;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear();
			SetOKButtonBounds(movingAnimal: false);
			Game1.displayHUD = true;
			Game1.viewportFreeze = false;
			Game1.displayFarmer = true;
			movingAnimal = false;
		}

		public override bool readyToClose()
		{
			textBox.Selected = false;
			if (base.readyToClose() && !movingAnimal)
			{
				return !Game1.globalFade;
			}
			return false;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (readyToClose())
			{
				Game1.exitActiveMenu();
				if (textBox.Text.Length > 0 && !Utility.areThereAnyOtherAnimalsWithThisName(textBox.Text))
				{
					animal.displayName = textBox.Text;
					animal.Name = textBox.Text;
				}
				Game1.playSound("smallSelect");
			}
			else if (movingAnimal)
			{
				Game1.globalFadeToBlack(prepareForReturnFromPlacement);
			}
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void draw(SpriteBatch b)
		{
			if (!movingAnimal && !Game1.globalFade)
			{
				b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, width, height), Color.Black * 0.75f);
				int num = yBoxPos;
				Game1.drawDialogueBox(xBoxOff, num, panelWidth + dialogueBoxPad * 2, num + yBoxDepth, speaker: false, drawOnlyBox: true);
				if ((byte)animal.harvestType != 2)
				{
					textBox.Draw(b);
				}
				int num2 = ((int)animal.age + 1) / 28 + 1;
				string text = ((num2 <= 1) ? Game1.content.LoadString("Strings\\UI:AnimalQuery_Age1") : Game1.content.LoadString("Strings\\UI:AnimalQuery_AgeN", num2));
				if ((int)animal.age < (byte)animal.ageWhenMature)
				{
					text += Game1.content.LoadString("Strings\\UI:AnimalQuery_AgeBaby");
				}
				int num3 = (int)((float)(width - 2 * xBoxOff) - Game1.dialogueFont.MeasureString(text).X - 48f) / 2;
				Utility.drawTextWithShadow(b, text, Game1.dialogueFont, new Vector2(xBoxOff + IClickableMenu.spaceToClearSideBorder + num3, num + IClickableMenu.spaceToClearTopBorder), Game1.textColor);
				int num4 = 0;
				if (parentName != null)
				{
					num += 42;
					string text2 = Game1.content.LoadString("Strings\\UI:AnimalQuery_Parent", parentName);
					num3 = (int)((float)(width - 2 * xBoxOff) - Game1.smallFont.MeasureString(text2).X - 48f) / 2;
					num4 = 42;
					Utility.drawTextWithShadow(b, text2, Game1.smallFont, new Vector2(xBoxOff + IClickableMenu.spaceToClearSideBorder + num3, num + IClickableMenu.spaceToClearTopBorder), Game1.textColor);
				}
				int num5 = (int)((loveLevel * 1000.0 % 200.0 >= 100.0) ? (loveLevel * 1000.0 / 200.0) : (-100.0));
				num += 42;
				num3 = (width - 168) / 2;
				for (int i = 0; i < 5; i++)
				{
					b.Draw(Game1.mouseCursors, new Vector2(num3 + 32 * i, num + IClickableMenu.spaceToClearTopBorder), new Microsoft.Xna.Framework.Rectangle(211 + ((loveLevel * 1000.0 <= (double)((i + 1) * 195)) ? 7 : 0), 428, 7, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.089f);
					if (num5 == i)
					{
						b.Draw(Game1.mouseCursors, new Vector2(num3 + 32 * i, num + IClickableMenu.spaceToClearTopBorder), new Microsoft.Xna.Framework.Rectangle(211, 428, 4, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0891f);
					}
				}
				num += 42;
				Utility.drawTextWithShadow(b, Game1.parseText(animal.getMoodMessage(), Game1.dialogueFont, width - 2 * xBoxOff - 6 * IClickableMenu.spaceToClearSideBorder), Game1.dialogueFont, new Vector2(xBoxOff + IClickableMenu.spaceToClearSideBorder * 3, num + IClickableMenu.spaceToClearTopBorder), Game1.textColor);
				IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.dialogueFont, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(256, 256, 10, 10), Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(89, 68, 9, 9), Game1.content.LoadString("Strings\\UI:AnimalQuery_Move"), moveHomeButton.bounds.X, moveHomeButton.bounds.Y, moveHomeButton.bounds.Width, moveHomeButton.bounds.Height, Color.White, 4f, drawShadow: true, iconLeft: true, isClickable: true, moveHeld);
				if (_selectedButton == Button.Move)
				{
					IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), moveHomeButton.bounds.X - 4, moveHomeButton.bounds.Y - 4, moveHomeButton.bounds.Width + 8, moveHomeButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
				}
				IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.dialogueFont, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(256, 256, 10, 10), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(193, 373, 9, 9), Game1.content.LoadString("Strings\\UI:AnimalQuery_Sell", animal.getSellPrice()), sellButton.bounds.X, sellButton.bounds.Y, sellButton.bounds.Width, sellButton.bounds.Height, Color.White, 4f, drawShadow: true, iconLeft: true, isClickable: true, sellHeld);
				if (_selectedButton == Button.Sell)
				{
					IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), sellButton.bounds.X - 4, sellButton.bounds.Y - 4, sellButton.bounds.Width + 8, sellButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
				}
				if (allowReproductionButton != null)
				{
					IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.dialogueFont, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(256, 256, 10, 10), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(animal.allowReproduction ? 128 : 137, 393, 9, 9), Game1.content.LoadString("Strings\\UI:AnimalQuery_AllowReproduction"), allowReproductionButton.bounds.X, allowReproductionButton.bounds.Y, allowReproductionButton.bounds.Width, allowReproductionButton.bounds.Height, Color.White, 4f, drawShadow: true, iconLeft: true, isClickable: true, pregHeld);
					if (_selectedButton == Button.AllowReproduction)
					{
						IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), allowReproductionButton.bounds.X - 4, allowReproductionButton.bounds.Y - 4, allowReproductionButton.bounds.Width + 8, allowReproductionButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
					}
				}
				closeButton.draw(b);
				if (confirmingSell)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
					Game1.drawDialogueBox(width / 2 - 256, height / 2 - 256, 512, 384, speaker: false, drawOnlyBox: true);
					string text3 = Game1.content.LoadString("Strings\\UI:AnimalQuery_ConfirmSell");
					b.DrawString(Game1.dialogueFont, text3, new Vector2((float)(width / 2) - Game1.dialogueFont.MeasureString(text3).X / 2f, height / 2 - 128), Game1.textColor);
					yesButton.draw(b);
					if (_selectedButton == Button.Tick)
					{
						IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), yesButton.bounds.X - 4, yesButton.bounds.Y - 4, yesButton.bounds.Width + 8, yesButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
					}
					noButton.draw(b);
					if (_selectedButton == Button.Cancel)
					{
						IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), noButton.bounds.X - 4, noButton.bounds.Y - 4, noButton.bounds.Width + 8, noButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
					}
				}
			}
			else
			{
				if (Game1.globalFade)
				{
					return;
				}
				string s = Game1.content.LoadString("Strings\\UI:AnimalQuery_ChooseBuilding", animal.displayHouse, animal.displayType);
				SpriteText.drawStringWithScrollCenteredAt(b, s, width / 2, 32, "", 1f, -1, 0, 0.001f);
				if (drawTickButton)
				{
					SetTickButtonBounds();
					tickButton.draw(b);
					if (_selectedButton == Button.Tick)
					{
						IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), tickButton.bounds.X - 4, tickButton.bounds.Y - 4, tickButton.bounds.Width + 8, tickButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
					}
				}
				SetCancelButtonBounds();
				cancelButton.draw(b);
				if (_selectedButton == Button.Cancel)
				{
					IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), cancelButton.bounds.X - 4, cancelButton.bounds.Y - 4, cancelButton.bounds.Width + 8, cancelButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
				}
			}
		}

		private void OnClickYes()
		{
			Game1.player.Money += animal.getSellPrice();
			try
			{
				(animal.home.indoors.Value as AnimalHouse).animalsThatLiveHere.Remove(animal.myID);
			}
			catch (Exception)
			{
				foreach (GameLocation location in Game1.locations)
				{
					if (location is AnimalHouse && ((AnimalHouse)location).animalsThatLiveHere.Contains(animal.myID))
					{
						((AnimalHouse)location).animalsThatLiveHere.Remove(animal.myID);
						break;
					}
				}
			}
			animal.health.Value = -1;
			int num = animal.frontBackSourceRect.Width / 2;
			for (int i = 0; i < num; i++)
			{
				int num2 = Game1.random.Next(25, 200);
				Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(5, animal.Position + new Vector2(Game1.random.Next(-32, animal.frontBackSourceRect.Width * 3), Game1.random.Next(-32, animal.frontBackSourceRect.Height * 3)), new Color(255 - num2, 255, 255 - num2), 8, flipped: false, (Game1.random.NextDouble() < 0.5) ? 50 : Game1.random.Next(30, 200), 0, 64, -1f, 64, (!(Game1.random.NextDouble() < 0.5)) ? Game1.random.Next(0, 600) : 0)
				{
					scale = (float)Game1.random.Next(2, 5) * 0.25f,
					alpha = (float)Game1.random.Next(2, 5) * 0.25f,
					motion = new Vector2(0f, (float)(0.0 - Game1.random.NextDouble()))
				});
			}
			Game1.playSound("newRecipe");
			Game1.playSound("money");
			Game1.exitActiveMenu();
		}

		private void OnClickNo()
		{
			confirmingSell = false;
			Game1.playSound("smallSelect");
			if (Game1.options.SnappyMenus)
			{
				currentlySnappedComponent = getComponentWithID(103);
				snapCursorToCurrentSnappedComponent();
			}
		}

		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return movingAnimal;
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
			cancelButton.bounds.Y = Game1.uiViewport.Height - tickButton.bounds.Height - 50;
			if (cancelButtonHeld)
			{
				cancelButton.bounds.X += 4;
				cancelButton.bounds.Y += 4;
			}
		}

		private void SetOKButtonBounds(bool movingAnimal = true)
		{
			if (movingAnimal)
			{
				okButton.bounds.X = Game1.uiViewport.Width - 128;
				okButton.bounds.Y = Game1.uiViewport.Height - 128;
			}
			else
			{
				okButton.bounds.X = xPositionOnScreen + width + 4;
				okButton.bounds.Y = yPositionOnScreen + height - 64 - IClickableMenu.borderWidth;
			}
		}

		private void OnClickTickButton()
		{
			Game1.playSound("smallSelect");
			(animal.home.indoors.Value as AnimalHouse).animalsThatLiveHere.Remove(animal.myID);
			if ((animal.home.indoors.Value as AnimalHouse).animals.ContainsKey(animal.myID))
			{
				(animal.home.indoors.Value as AnimalHouse).animals.Remove(animal.myID);
				(_selectedBuilding.indoors.Value as AnimalHouse).animals.Add(animal.myID, animal);
			}
			animal.home = _selectedBuilding;
			animal.homeLocation.Value = new Vector2((int)_selectedBuilding.tileX, (int)_selectedBuilding.tileY);
			(_selectedBuilding.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(animal.myID);
			animal.makeSound();
			Game1.globalFadeToBlack(finishedPlacingAnimal);
		}

		private void OnClickCancelButton()
		{
			_clickedCancel = true;
			Game1.globalFadeToBlack(prepareForReturnFromPlacement);
			Game1.playSound("smallSelect");
		}

		private void OnClickSell()
		{
			okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width + 4 - 400, yPositionOnScreen + height - 64 - IClickableMenu.borderWidth, 128, 128), Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(0, 0, 20, 20), 4f);
			yesButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(width / 2 + 30, height / 2 - 40, 80, 80), Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(0, 0, 20, 20), 4f);
			noButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(width / 2 - 110, height / 2 - 40, 80, 80), Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(20, 0, 20, 20), 4f);
			Game1.playSound("smallSelect");
			confirmingSell = true;
		}

		private void OnClickMove()
		{
			Game1.playSound("smallSelect");
			Game1.globalFadeToBlack(prepareForAnimalPlacement);
		}

		private void OnClickAllowPregnancy()
		{
			Game1.playSound("drumkit6");
			animal.allowReproduction.Value = !animal.allowReproduction;
		}

		private void TestToPan(int x, int y)
		{
			if (movingAnimal && !tickButtonHeld)
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
			if (_selectedBuilding.buildingType.Contains(animal.buildingTypeILiveIn) && !(_selectedBuilding.indoors.Value as AnimalHouse).isFull())
			{
				_selectedBuilding.color.Value = Color.LightGreen * 0.8f;
			}
			else
			{
				_selectedBuilding.color.Value = Color.Red * 0.8f;
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (movingAnimal)
			{
				switch (b)
				{
				case Buttons.DPadUp:
				case Buttons.LeftThumbstickUp:
				{
					UnhighlightBuildings();
					int num3 = selectedBuildingIndex;
					num3 = ((num3 > 0) ? (num3 - 1) : (((Farm)Game1.currentLocation).buildings.Count - 1));
					if (num3 < 0)
					{
						break;
					}
					int num4 = 0;
					{
						foreach (Building building in ((Farm)Game1.currentLocation).buildings)
						{
							if (num4 == num3)
							{
								_selectedBuilding = building;
								HighlightSelectedBuilding();
								Game1.playSound("smallSelect");
								break;
							}
							num4++;
						}
						break;
					}
				}
				case Buttons.DPadDown:
				case Buttons.LeftThumbstickDown:
				{
					UnhighlightBuildings();
					int num = selectedBuildingIndex;
					num = ((num >= 0 && num < ((Farm)Game1.currentLocation).buildings.Count - 1) ? (num + 1) : 0);
					if (num >= ((Farm)Game1.currentLocation).buildings.Count)
					{
						break;
					}
					int num2 = 0;
					{
						foreach (Building building2 in ((Farm)Game1.currentLocation).buildings)
						{
							if (num2 == num)
							{
								_selectedBuilding = building2;
								HighlightSelectedBuilding();
								Game1.playSound("smallSelect");
								break;
							}
							num2++;
						}
						break;
					}
				}
				case Buttons.DPadLeft:
				case Buttons.DPadRight:
				case Buttons.LeftThumbstickLeft:
				case Buttons.LeftThumbstickRight:
					if (_selectedButton == Button.Cancel && drawTickButton)
					{
						_selectedButton = Button.Tick;
					}
					else
					{
						_selectedButton = Button.Cancel;
					}
					break;
				case Buttons.A:
					if (_selectedButton == Button.Tick)
					{
						OnClickTickButton();
					}
					else
					{
						OnClickCancelButton();
					}
					break;
				}
				return;
			}
			if (confirmingSell)
			{
				switch (b)
				{
				case Buttons.DPadLeft:
				case Buttons.DPadRight:
				case Buttons.LeftThumbstickLeft:
				case Buttons.LeftThumbstickRight:
					if (_selectedButton == Button.Tick)
					{
						_selectedButton = Button.Cancel;
					}
					else
					{
						_selectedButton = Button.Tick;
					}
					break;
				case Buttons.A:
					if (_selectedButton == Button.Tick)
					{
						OnClickYes();
					}
					else
					{
						OnClickNo();
					}
					break;
				}
				return;
			}
			switch (b)
			{
			case Buttons.DPadUp:
			case Buttons.LeftThumbstickUp:
				if (_selectedButton == Button.Move)
				{
					if (allowReproductionButton != null)
					{
						_selectedButton = Button.AllowReproduction;
					}
					else
					{
						_selectedButton = Button.Sell;
					}
				}
				else if (_selectedButton == Button.Sell)
				{
					_selectedButton = Button.Move;
				}
				else if (_selectedButton == Button.AllowReproduction)
				{
					_selectedButton = Button.Sell;
				}
				else if (allowReproductionButton != null)
				{
					_selectedButton = Button.AllowReproduction;
				}
				else
				{
					_selectedButton = Button.Sell;
				}
				break;
			case Buttons.DPadDown:
			case Buttons.LeftThumbstickDown:
				if (_selectedButton == Button.Move)
				{
					_selectedButton = Button.Sell;
				}
				else if (_selectedButton == Button.Sell)
				{
					if (allowReproductionButton != null)
					{
						_selectedButton = Button.AllowReproduction;
					}
					else
					{
						_selectedButton = Button.Move;
					}
				}
				else if (_selectedButton == Button.AllowReproduction)
				{
					_selectedButton = Button.Move;
				}
				else
				{
					_selectedButton = Button.Move;
				}
				break;
			case Buttons.B:
				Game1.playSound("bigDeSelect");
				exitThisMenu();
				break;
			case Buttons.A:
				if (_selectedButton == Button.Move)
				{
					OnClickMove();
				}
				else if (_selectedButton == Button.Sell)
				{
					OnClickSell();
				}
				else if (_selectedButton == Button.AllowReproduction)
				{
					OnClickAllowPregnancy();
				}
				break;
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			if (movingAnimal)
			{
				if (tickButton != null && tickButtonHeld && !tickButton.containsPoint(x, y))
				{
					tickButtonHeld = false;
				}
				if (cancelButton != null && cancelButtonHeld && !cancelButton.containsPoint(x, y))
				{
					cancelButtonHeld = false;
				}
				if (PinchZoom.Instance.CheckForPinchZoom())
				{
					return;
				}
				TestToPan(Game1.input.GetMouseState().X, Game1.input.GetMouseState().Y);
			}
			if (sellHeld && !sellButton.containsPoint(x, y))
			{
				sellHeld = false;
			}
			else if (moveHeld && !moveHomeButton.containsPoint(x, y))
			{
				moveHeld = false;
			}
			else if (pregHeld && !allowReproductionButton.containsPoint(x, y))
			{
				pregHeld = false;
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (movingAnimal)
			{
				if (tickButton != null && tickButtonHeld && tickButton.containsPoint(x, y) && _selectedBuilding.buildingType.Contains(animal.buildingTypeILiveIn) && !(_selectedBuilding.indoors.Value as AnimalHouse).isFull())
				{
					OnClickTickButton();
				}
				else if (cancelButton != null && cancelButtonHeld && cancelButton.containsPoint(x, y) && !_clickedCancel)
				{
					OnClickCancelButton();
				}
			}
			if (sellHeld && sellButton.containsPoint(x, y))
			{
				OnClickSell();
			}
			else if (moveHeld && moveHomeButton.containsPoint(x, y))
			{
				OnClickMove();
			}
			else if (pregHeld && allowReproductionButton.containsPoint(x, y))
			{
				OnClickAllowPregnancy();
			}
			pregHeld = (moveHeld = (sellHeld = false));
		}
	}
}
