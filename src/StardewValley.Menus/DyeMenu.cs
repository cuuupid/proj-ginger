using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class DyeMenu : MenuWithInventory
	{
		protected int _timeUntilCraft;

		public List<ClickableTextureComponent> dyePots;

		public ClickableTextureComponent dyeButton;

		public const int DYE_POT_ID_OFFSET = 5000;

		public Texture2D dyeTexture;

		protected Dictionary<Item, int> _highlightDictionary;

		protected Dictionary<string, Item> _lastValidEquippedItems;

		protected bool _shouldPrismaticDye;

		protected List<Vector2> _slotDrawPositions;

		protected int _hoveredPotIndex = -1;

		protected int[] _dyeDropAnimationFrames;

		public const int MILLISECONDS_PER_DROP_FRAME = 50;

		public const int TOTAL_DROP_FRAMES = 10;

		public string[][] validPotColors = new string[6][]
		{
			new string[4] { "color_red", "color_salmon", "color_dark_red", "color_pink" },
			new string[5] { "color_orange", "color_dark_orange", "color_dark_brown", "color_brown", "color_copper" },
			new string[4] { "color_yellow", "color_dark_yellow", "color_gold", "color_sand" },
			new string[5] { "color_green", "color_dark_green", "color_lime", "color_yellow_green", "color_jade" },
			new string[6] { "color_blue", "color_dark_blue", "color_dark_cyan", "color_light_cyan", "color_cyan", "color_aquamarine" },
			new string[6] { "color_purple", "color_dark_purple", "color_dark_pink", "color_pale_violet_red", "color_poppyseed", "color_iridium" }
		};

		protected bool _heldItemIsEquipped;

		protected string displayedDescription = "";

		public List<ClickableTextureComponent> dyedClothesDisplays;

		protected Vector2 _dyedClothesDisplayPosition;

		private Rectangle infoBox;

		private Rectangle bottomInv;

		private Rectangle dyePanelRect;

		private float widthMod;

		private float heightMod;

		private new int width;

		private new int height;

		private int dyePanelX;

		private int dyePanelY;

		private int dyePanelHeight;

		private int dyePanelWidth;

		private int red = 50;

		private int green = 160;

		private int blue = 255;

		private float dyePanelWidthRatio;

		private float dyePanelHightRatio;

		private float dyePanelRatioWH = 1.775f;

		private int _selectedItemIndex = -1;

		private bool _showTooltip;

		public DyeMenu()
			: base(null, okButton: true, trashCan: true, Game1.xEdge)
		{
			Game1.playSound("bigSelect");
			inventory.showOrganizeButton = false;
			xPositionOnScreen = Game1.xEdge;
			yPositionOnScreen = 0;
			width = Game1.uiViewport.Width - Game1.xEdge * 2;
			height = Game1.uiViewport.Height;
			widthMod = (float)width / 1280f;
			heightMod = (float)height / 720f;
			dyePanelX = xPositionOnScreen;
			dyePanelY = yPositionOnScreen;
			dyePanelHeight = height / 2;
			dyePanelWidth = (int)((float)dyePanelHeight * dyePanelRatioWH);
			int num = height - dyePanelHeight;
			dyePanelRect = new Rectangle(dyePanelX + 18, dyePanelY + 16, dyePanelWidth - 32, dyePanelHeight - 40);
			infoBox = new Rectangle(dyePanelWidth + dyePanelX, dyePanelY, width - dyePanelWidth, dyePanelHeight);
			bottomInv = new Rectangle(xPositionOnScreen, dyePanelY + dyePanelHeight, width, num);
			dyePanelWidthRatio = (float)(dyePanelWidth - 32) / 142f;
			dyePanelHightRatio = (float)(dyePanelHeight - 40) / 80f;
			inventory.movePosition(0, (dyePanelY + dyePanelHeight - inventory.getInvY()) / 2);
			inventory.highlightMethod = HighlightItems;
			dyeTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\dye_bench");
			dyedClothesDisplays = new List<ClickableTextureComponent>();
			_CreateButtons();
			if (okButton != null)
			{
				okButton.leftNeighborID = 11;
			}
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
			GenerateHighlightDictionary();
			_UpdateDescriptionText();
		}

		protected void _CreateButtons()
		{
			_slotDrawPositions = inventory.GetSlotDrawPositions();
			Dictionary<int, Item> dictionary = new Dictionary<int, Item>();
			if (dyePots != null)
			{
				for (int i = 0; i < dyePots.Count; i++)
				{
					dictionary[i] = dyePots[i].item;
				}
			}
			dyePots = new List<ClickableTextureComponent>();
			for (int j = 0; j < validPotColors.Length; j++)
			{
				ClickableTextureComponent item = new ClickableTextureComponent(new Rectangle(dyePanelRect.X + (int)(17f * dyePanelWidthRatio) + (int)(18f * dyePanelWidthRatio * (float)j), dyePanelRect.Y + (int)(33f * dyePanelHightRatio), (int)(16f * (4f * widthMod)), (int)(16f * (4f * heightMod))), dyeTexture, new Rectangle(32 + 16 * j, 80, 16, 16), dyePanelWidthRatio)
				{
					myID = j + 5000,
					downNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					upNeighborID = -99998,
					item = (dictionary.ContainsKey(j) ? dictionary[j] : null)
				};
				dyePots.Add(item);
			}
			_dyeDropAnimationFrames = new int[dyePots.Count];
			for (int k = 0; k < _dyeDropAnimationFrames.Length; k++)
			{
				_dyeDropAnimationFrames[k] = -1;
			}
			dyeButton = new ClickableTextureComponent(new Rectangle(dyePanelRect.Right - (int)(24f * dyePanelWidthRatio), dyePanelRect.Bottom - (int)(24f * dyePanelHightRatio) - 5, 96, 96), dyeTexture, new Rectangle(0, 80, 24, 24), dyePanelWidthRatio)
			{
				myID = 1000,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				item = ((dyeButton != null) ? dyeButton.item : null)
			};
			if (inventory.inventory != null && inventory.inventory.Count >= 12)
			{
				for (int l = 0; l < 12; l++)
				{
					if (inventory.inventory[l] != null)
					{
						inventory.inventory[l].upNeighborID = -99998;
					}
				}
			}
			dyedClothesDisplays.Clear();
			_dyedClothesDisplayPosition = new Vector2(infoBox.X + infoBox.Width / 2, infoBox.Height - 16 - 64);
			Vector2 dyedClothesDisplayPosition = _dyedClothesDisplayPosition;
			int num = 0;
			if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
			{
				num++;
			}
			if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
			{
				num++;
			}
			dyedClothesDisplayPosition.X -= num * 64 / 2;
			if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
			{
				ClickableTextureComponent clickableTextureComponent = new ClickableTextureComponent(new Rectangle((int)dyedClothesDisplayPosition.X, (int)dyedClothesDisplayPosition.Y, 64, 64), null, new Rectangle(0, 0, 64, 64), 4f);
				clickableTextureComponent.item = Game1.player.shirtItem.Value;
				dyedClothesDisplayPosition.X += 64f;
				dyedClothesDisplays.Add(clickableTextureComponent);
			}
			if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
			{
				ClickableTextureComponent clickableTextureComponent2 = new ClickableTextureComponent(new Rectangle((int)dyedClothesDisplayPosition.X, (int)dyedClothesDisplayPosition.Y, 64, 64), null, new Rectangle(0, 0, 64, 64), 4f);
				clickableTextureComponent2.item = Game1.player.pantsItem.Value;
				dyedClothesDisplayPosition.X += 64f;
				dyedClothesDisplays.Add(clickableTextureComponent2);
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public bool IsBusy()
		{
			return _timeUntilCraft > 0;
		}

		public override bool readyToClose()
		{
			if (base.readyToClose() && heldItem == null)
			{
				return !IsBusy();
			}
			return false;
		}

		public bool HighlightItems(Item i)
		{
			if (i == null)
			{
				return false;
			}
			if (i != null && !i.canBeTrashed())
			{
				return false;
			}
			if (_highlightDictionary == null)
			{
				GenerateHighlightDictionary();
			}
			if (!_highlightDictionary.ContainsKey(i))
			{
				_highlightDictionary = null;
				GenerateHighlightDictionary();
			}
			if (_hoveredPotIndex >= 0)
			{
				return _hoveredPotIndex == _highlightDictionary[i];
			}
			if (_highlightDictionary[i] >= 0)
			{
				return dyePots[_highlightDictionary[i]].item == null;
			}
			return false;
		}

		public void GenerateHighlightDictionary()
		{
			_highlightDictionary = new Dictionary<Item, int>();
			List<Item> list = new List<Item>(inventory.actualInventory);
			foreach (Item item in list)
			{
				if (item != null)
				{
					_highlightDictionary[item] = GetPotIndex(item);
				}
			}
		}

		private void _DyePotClicked(ClickableTextureComponent dye_pot)
		{
			Item item = dye_pot.item;
			int num = dyePots.IndexOf(dye_pot);
			if (num < 0)
			{
				return;
			}
			if (heldItem == null && inventory.currentlySelectedItem != -1)
			{
				heldItem = inventory.actualInventory[inventory.currentlySelectedItem];
			}
			if (heldItem == null || (heldItem.canBeTrashed() && GetPotIndex(heldItem) == num))
			{
				if (dye_pot.item != null && heldItem != null && dye_pot.item.canStackWith(heldItem))
				{
					heldItem.Stack++;
					dye_pot.item = null;
					Game1.playSound("quickSlosh");
					return;
				}
				dye_pot.item = ((heldItem == null) ? null : heldItem.getOne());
				if (heldItem != null)
				{
					heldItem.Stack--;
				}
				if (heldItem != null && heldItem.Stack <= 0)
				{
					heldItem = item;
					if (inventory.currentlySelectedItem != -1)
					{
						Game1.player.removeItemFromInventory(inventory.currentlySelectedItem);
					}
				}
				else if (heldItem != null && item != null)
				{
					Item item2 = Game1.player.addItemToInventory(heldItem);
					if (item2 != null)
					{
						Game1.createItemDebris(item2, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
					}
					heldItem = item;
				}
				else if (item != null)
				{
					heldItem = item;
				}
				else if (heldItem != null && item == null && Keyboard.GetState().IsKeyDown(Keys.LeftShift))
				{
					Game1.player.addItemToInventory(heldItem);
					heldItem = null;
				}
				if (item != dye_pot.item)
				{
					_dyeDropAnimationFrames[num] = 0;
					Game1.playSound("quickSlosh");
					int num2 = 0;
					for (int i = 0; i < dyePots.Count; i++)
					{
						if (dyePots[i].item != null)
						{
							num2++;
						}
					}
					if (num2 >= dyePots.Count)
					{
						DelayedAction.playSoundAfterDelay("newArtifact", 200);
					}
				}
				_highlightDictionary = null;
				GenerateHighlightDictionary();
			}
			_UpdateDescriptionText();
		}

		public Color GetColorForPot(int index)
		{
			return index switch
			{
				0 => new Color(220, 0, 0), 
				1 => new Color(255, 128, 0), 
				2 => new Color(255, 230, 0), 
				3 => new Color(10, 143, 0), 
				4 => new Color(46, 105, 203), 
				5 => new Color(115, 41, 181), 
				_ => Color.Black, 
			};
		}

		public int GetPotIndex(Item item)
		{
			for (int i = 0; i < validPotColors.Length; i++)
			{
				for (int j = 0; j < validPotColors[i].Length; j++)
				{
					if (item.HasContextTag(validPotColors[i][j]))
					{
						return i;
					}
				}
			}
			return -1;
		}

		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.Delete)
			{
				if (heldItem != null && heldItem.canBeTrashed())
				{
					Utility.trashItem(heldItem);
					heldItem = null;
				}
			}
			else
			{
				base.receiveKeyPress(key);
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (inventory.dragItem != -1 || inventory.currentlySelectedItem != -1)
			{
				foreach (ClickableTextureComponent dyePot in dyePots)
				{
					if (dyePot.containsPoint(x, y))
					{
						_DyePotClicked(dyePot);
						inventory.currentlySelectedItem = -1;
						inventory.dragItem = -1;
						heldItem = null;
					}
				}
			}
			else if (inventory.dragItem == -1 && inventory.currentlySelectedItem == -1)
			{
				foreach (ClickableTextureComponent dyePot2 in dyePots)
				{
					if (dyePot2.containsPoint(x, y) && dyePot2.item != null)
					{
						Item item = Game1.player.addItemToInventory(dyePot2.item);
						if (item != null)
						{
							Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
						}
						dyePot2.item = null;
						inventory.currentlySelectedItem = -1;
						inventory.dragItem = -1;
						heldItem = null;
						Game1.playSound("bigSelect");
					}
				}
			}
			base.releaseLeftClick(x, y);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			Item item = heldItem;
			if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
			{
				_OnCloseMenu();
			}
			bool flag = Game1.player.IsEquippedItem(item);
			base.receiveLeftClick(x, y, heldItem != null || !Keyboard.GetState().IsKeyDown(Keys.LeftShift));
			if (!dyeButton.containsPoint(x, y))
			{
				return;
			}
			if (heldItem == null && CanDye())
			{
				Game1.playSound("glug");
				for (int i = 0; i < dyePots.Count; i++)
				{
					if (dyePots[i].item != null)
					{
						dyePots[i].item.Stack--;
						if (dyePots[i].item.Stack <= 0)
						{
							dyePots[i].item = null;
						}
					}
				}
				Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.DyePots);
				_UpdateDescriptionText();
			}
			else
			{
				Game1.playSound("sell");
			}
		}

		public bool CanDye()
		{
			for (int i = 0; i < dyePots.Count; i++)
			{
				if (dyePots[i].item == null)
				{
					return false;
				}
			}
			return true;
		}

		protected virtual bool CheckHeldItem(Func<Item, bool> f = null)
		{
			return f?.Invoke(heldItem) ?? (heldItem != null);
		}

		public static bool IsWearingDyeable()
		{
			if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.dyeable.Value)
			{
				return true;
			}
			if (Game1.player.pantsItem.Value != null && Game1.player.pantsItem.Value.dyeable.Value)
			{
				return true;
			}
			return false;
		}

		protected void _UpdateDescriptionText()
		{
			if (!IsWearingDyeable())
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:DyePot_NoDyeable");
			}
			else if (CanDye())
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:DyePot_CanDye");
			}
			else
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:DyePot_Help");
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (!IsBusy())
			{
				base.receiveRightClick(x, y, playSound: true);
			}
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			int yPosition = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
			inventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPosition, playerInventory: false, null, inventory.highlightMethod);
			_CreateButtons();
		}

		public override void emergencyShutDown()
		{
			_OnCloseMenu();
			base.emergencyShutDown();
		}

		public override void update(GameTime time)
		{
			base.update(time);
			descriptionText = displayedDescription;
			if (CanDye())
			{
				dyeButton.sourceRect.Y = 180;
				dyeButton.sourceRect.X = (int)(time.TotalGameTime.TotalMilliseconds % 600.0 / 100.0) * 24;
			}
			else
			{
				dyeButton.sourceRect.Y = 80;
				dyeButton.sourceRect.X = 0;
			}
			for (int i = 0; i < dyePots.Count; i++)
			{
				if (_dyeDropAnimationFrames[i] >= 0)
				{
					_dyeDropAnimationFrames[i] += time.ElapsedGameTime.Milliseconds;
					if (_dyeDropAnimationFrames[i] >= 500)
					{
						_dyeDropAnimationFrames[i] = -1;
					}
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			int num = yPositionOnScreen;
			int num2 = xPositionOnScreen;
			yPositionOnScreen = -80;
			xPositionOnScreen = -16;
			Game1.drawDialogueBox(dyePanelX - 16, -80, dyePanelWidth + 28, dyePanelHeight + 96, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe: true, red, green, blue);
			Game1.drawDialogueBox(infoBox.X - 28, -80, infoBox.Width + 44, infoBox.Height + 96, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe: true, red, green, blue);
			Game1.drawDialogueBox(bottomInv.X - 16, bottomInv.Y - 102, bottomInv.Width + 32, bottomInv.Height + 121, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe: true, red, green, blue);
			int num3 = base.width;
			base.width += 32;
			base.width = num3;
			drawVerticalUpperIntersectingPartition(b, infoBox.X - 38, bottomInv.Y - 32 + 2, red, green, blue);
			yPositionOnScreen = num;
			xPositionOnScreen = num2;
			base.draw(b, drawUpperPortion: true, drawDescriptionArea: true, red, green, blue);
			b.Draw(dyeTexture, dyePanelRect, new Rectangle(0, 0, 142, 80), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.087f);
			for (int i = 0; i < _slotDrawPositions.Count; i++)
			{
				if (i >= inventory.actualInventory.Count || inventory.actualInventory[i] == null || !_highlightDictionary.ContainsKey(inventory.actualInventory[i]))
				{
					continue;
				}
				int num4 = _highlightDictionary[inventory.actualInventory[i]];
				if (num4 >= 0)
				{
					Color colorForPot = GetColorForPot(num4);
					if (_hoveredPotIndex == -1 && HighlightItems(inventory.actualInventory[i]))
					{
						b.Draw(dyeTexture, _slotDrawPositions[i], new Rectangle(32, 96, 32, 32), colorForPot, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.087f);
					}
				}
			}
			dyeButton.draw(b, Color.White * (CanDye() ? 1f : 0.55f), 0.096f);
			dyeButton.drawItem(b, 16, 16, 0.089f);
			int num5 = 0;
			if (descriptionText != "")
			{
				num5 = Utility.drawMultiLineTextWithShadow(position: new Vector2(infoBox.X + 16, infoBox.Y + 32), b: b, text: displayedDescription, font: Game1.smallFont, width: infoBox.Width - (upperRightCloseButton.bounds.Width + 16), height: infoBox.Height - 24, col: Color.Black, centreY: false, actuallyDrawIt: true, drawShadows: false);
			}
			string text = Game1.content.LoadString("Strings\\UI:DyePot_WillDye");
			Vector2 dyedClothesDisplayPosition = _dyedClothesDisplayPosition;
			Utility.drawTextWithColoredShadow(position: new Vector2(dyedClothesDisplayPosition.X - Game1.smallFont.MeasureString(text).X / 2f, (float)(int)dyedClothesDisplayPosition.Y - Game1.smallFont.MeasureString(text).Y), b: b, text: text, font: Game1.smallFont, color: Game1.textColor * 0.75f, shadowColor: Color.Black * 0.2f);
			foreach (ClickableTextureComponent dyedClothesDisplay in dyedClothesDisplays)
			{
				dyedClothesDisplay.drawItem(b);
			}
			for (int j = 0; j < dyePots.Count; j++)
			{
				dyePots[j].drawItem(b, 0, -16);
				if (_dyeDropAnimationFrames[j] >= 0)
				{
					Color colorForPot2 = GetColorForPot(j);
					b.Draw(dyeTexture, new Vector2(dyePots[j].bounds.X, dyePots[j].bounds.Y - 12), new Rectangle(_dyeDropAnimationFrames[j] / 50 * 16, 128, 16, 16), colorForPot2, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
				}
				dyePots[j].draw(b);
			}
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont, (heldItem != null) ? 32 : 0, (heldItem != null) ? 32 : 0);
			}
			else if (hoveredItem != null)
			{
				IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem, heldItem != null);
			}
			inventory.drawDragItem(b);
			inventory.drawInfoPanel(b, force: true);
			if (!Game1.options.hardwareCursor && !Game1.options.hardwareCursor)
			{
				drawMouse(b);
			}
		}

		protected override void cleanupBeforeExit()
		{
			_OnCloseMenu();
		}

		protected void _OnCloseMenu()
		{
			Utility.CollectOrDrop(heldItem);
			for (int i = 0; i < dyePots.Count; i++)
			{
				if (dyePots[i].item != null)
				{
					Utility.CollectOrDrop(dyePots[i].item);
				}
			}
			heldItem = null;
			dyeButton.item = null;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			switch (b)
			{
			case Buttons.DPadUp:
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
			case Buttons.LeftThumbstickUp:
			{
				bool flag2 = false;
				for (int num = Math.Max(0, _selectedItemIndex - 1); num >= 0; num--)
				{
					Item itemAt2 = inventory.GetItemAt(num);
					if (inventory.highlightMethod(itemAt2))
					{
						_selectedItemIndex = num;
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					break;
				}
				for (int num2 = inventory.inventory.Count - 1; num2 > _selectedItemIndex; num2--)
				{
					Item itemAt2 = inventory.GetItemAt(num2);
					if (itemAt2 != null && inventory.highlightMethod(itemAt2))
					{
						_selectedItemIndex = num2;
						break;
					}
				}
				break;
			}
			case Buttons.DPadDown:
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickDown:
			case Buttons.LeftThumbstickRight:
			{
				bool flag = false;
				for (int i = Math.Max(0, _selectedItemIndex + 1); i < inventory.inventory.Count; i++)
				{
					Item itemAt = inventory.GetItemAt(i);
					if (itemAt != null && inventory.highlightMethod(itemAt))
					{
						_selectedItemIndex = i;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
				for (int j = 0; j < _selectedItemIndex; j++)
				{
					Item itemAt = inventory.GetItemAt(j);
					if (itemAt != null && inventory.highlightMethod(itemAt))
					{
						_selectedItemIndex = j;
						break;
					}
				}
				break;
			}
			case Buttons.A:
				_showTooltip = !_showTooltip;
				if (!_showTooltip)
				{
					inventory.GamePadHideInfoPanel();
				}
				break;
			case Buttons.X:
				if (CanDye())
				{
					receiveLeftClick(dyeButton.bounds.X, dyeButton.bounds.Y);
					return;
				}
				if (_selectedItemIndex > -1)
				{
					heldItem = inventory.actualInventory[_selectedItemIndex];
					if (heldItem != null)
					{
						int potIndex = GetPotIndex(heldItem);
						_DyePotClicked(dyePots[potIndex]);
					}
				}
				break;
			case Buttons.B:
				heldItem = null;
				Game1.playSound("smallSelect");
				OnTapCloseButton();
				Game1.player.forceCanMove();
				break;
			}
			if (_selectedItemIndex > -1)
			{
				inventory.currentlySelectedItem = _selectedItemIndex;
				if (_showTooltip)
				{
					inventory.GamePadShowInfoPanel();
				}
			}
		}
	}
}
