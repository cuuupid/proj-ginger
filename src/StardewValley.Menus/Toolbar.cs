using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Locations;
using StardewValley.Mobile;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class Toolbar : IClickableMenu
	{
		private const int TICKS_BEFORE_TAP_HOLD_KICKS_IN = 4000000;

		private long hoverTicksAtStart;

		private int xOffset = 12;

		private int yOffset = 12;

		private Vector2 _tooltipPosition;

		private bool vertical = true;

		public static int toolbarWidth;

		private int toolbarHeight;

		private int _itemSlotSize;

		public static int toolBarItemWidth;

		public static bool toolbarPressed;

		private int _nextToolIndex = -2147483648;

		private int _toolbarPaddingX = Game1.toolbarPaddingX;

		private int _startTapPositionX = -1;

		private int _startTapPositionY = -1;

		private int _startIndex;

		private int _drawStartIndex;

		private bool _showTooltip = true;

		public bool alignTop;

		public Microsoft.Xna.Framework.Rectangle toolbarTextSource = new Microsoft.Xna.Framework.Rectangle(0, 256, 60, 60);

		private List<ClickableComponent> buttons = new List<ClickableComponent>();

		private string hoverTitle = "";

		private Item hoverItem;

		private Item lastHoverItem;

		private float transparency = 1f;

		private bool _ignoreRelease;

		private int _shoulderButtonDownCount;

		private const int UPDATES_TO_SHOW_TOOLTIP = 20;

		public int itemSlotSize
		{
			get
			{
				return Math.Max(1, _itemSlotSize);
			}
			set
			{
				_itemSlotSize = value;
			}
		}

		public static bool visible
		{
			get
			{
				if (Game1.displayHUD)
				{
					if (Game1.activeClickableMenu != null)
					{
						return !(Game1.activeClickableMenu.GetType() != typeof(DialogueBox));
					}
					return true;
				}
				return false;
			}
		}

		private new xTile.Dimensions.Rectangle viewport => Game1.uiViewport;

		public int screenWidth => Game1.uiViewport.Width;

		public int screenHeight => Game1.uiViewport.Height;

		private int maxScrollIndex => Math.Min(Game1.player.maxItems, buttons.Count) - Math.Min(maxVisibleItems, Game1.player.maxItems);

		private int HorizontalBottomStartY
		{
			get
			{
				if (buttons.Count > 0)
				{
					return buttons[0].bounds.Y;
				}
				return Game1.uiViewport.Height - itemSlotSize - 12;
			}
		}

		public int startIndex
		{
			get
			{
				return _startIndex;
			}
			set
			{
				_startIndex = (_drawStartIndex = Math.Max(0, Math.Min(value, maxScrollIndex)));
			}
		}

		public int maxVisibleItems
		{
			get
			{
				if (Game1.options.verticalToolbar)
				{
					return Math.Min(screenHeight / 32, (int)Math.Floor((decimal)screenHeight / (decimal)itemSlotSize));
				}
				int num = screenWidth - 12 - Game1.toolbarPaddingX - (screenWidth - (int)(Game1.dayTimeMoneyBox.position.X * Game1.DateTimeScale));
				return (int)Math.Floor((decimal)num / (decimal)Game1.maxItemSlotSize);
			}
		}

		public Toolbar()
			: base(4, 0, Game1.maxItemSlotSize * 12 + 24, Game1.maxItemSlotSize + 24)
		{
			vertical = Game1.options.verticalToolbar;
			if (vertical)
			{
				resetToolbar();
			}
			AddItems(12);
		}

		private void testToAddMoreItems()
		{
			if (Game1.player != null && Game1.player.items != null && (int)Game1.player.maxItems > buttons.Count)
			{
				AddItems(Game1.player.maxItems);
			}
		}

		private void AddItems(int totalItems)
		{
			for (int i = buttons.Count; i < totalItems; i++)
			{
				if (vertical)
				{
					buttons.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen, yOffset + i * itemSlotSize, itemSlotSize, itemSlotSize), i.ToString() ?? ""));
				}
				else
				{
					buttons.Add(new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(Game1.toolbarPaddingX + xOffset + i * Game1.maxItemSlotSize, yPositionOnScreen - Game1.maxItemSlotSize - 12, Game1.maxItemSlotSize, Game1.maxItemSlotSize), i.ToString() ?? ""));
				}
			}
		}

		public Vector2 getIconPosition(string itemName)
		{
			for (int i = 0; i < buttons.Count; i++)
			{
				if (Game1.player.items[i] != null && Game1.player.items[i].Name == itemName)
				{
					return getIconPosition(i);
				}
			}
			return new Vector2(-999f, -999f);
		}

		public Vector2 getIconPosition(int index)
		{
			if (buttons[index] != null)
			{
				return (!vertical) ? new Vector2(buttons[index].bounds.X + buttons[index].bounds.Width / 2, buttons[index].bounds.Y + buttons[index].bounds.Height / 2) : new Vector2(Game1.toolbarPaddingX + buttons[index].bounds.X + (toolBarItemWidth - Game1.toolbarPaddingX) / 2, buttons[index].bounds.Y + buttons[index].bounds.Height / 2);
			}
			return new Vector2(0f, 0f);
		}

		private void testToScrollToolbar(int x, int y)
		{
			if (TutorialManager.Instance.showTheTutorials && !TutorialManager.Instance.isTutorialComplete(tutorialType.USE_HOE))
			{
				return;
			}
			if (Game1.options.verticalToolbar && x <= Game1.toolbarPaddingX + itemSlotSize + 12)
			{
				if (_startTapPositionY == -1)
				{
					_startTapPositionY = y;
				}
				else
				{
					updateDrawStartIndex(x, y);
				}
			}
			else if (!Game1.options.verticalToolbar && ((alignTop && y <= itemSlotSize + 12) || (!alignTop && y >= HorizontalBottomStartY)))
			{
				if (_startTapPositionX == -1)
				{
					_startTapPositionX = x;
				}
				else
				{
					updateDrawStartIndex(x, y);
				}
			}
		}

		private void updateDrawStartIndex(int x, int y)
		{
			if (TutorialManager.Instance.showTheTutorials && !TutorialManager.Instance.isTutorialComplete(tutorialType.USE_HOE))
			{
				return;
			}
			int num;
			if (Game1.options.verticalToolbar)
			{
				if (x > Game1.xEdge + Game1.toolbarPaddingX + itemSlotSize + 12)
				{
					return;
				}
				num = _startTapPositionY - y;
			}
			else if (alignTop && y <= itemSlotSize + 12)
			{
				num = _startTapPositionX - x;
			}
			else
			{
				if (alignTop || y < HorizontalBottomStartY)
				{
					return;
				}
				num = _startTapPositionX - x;
			}
			int num2 = (int)Math.Round((double)num / (double)itemSlotSize);
			int drawStartIndex = _drawStartIndex;
			_drawStartIndex = Math.Max(0, Math.Min(_startIndex + num2, maxScrollIndex));
			if (_drawStartIndex != drawStartIndex)
			{
				_showTooltip = false;
			}
		}

		private void updateScrollIndex(int x, int y)
		{
			_startIndex = _drawStartIndex;
			_startTapPositionX = -1;
			_startTapPositionY = -1;
			_showTooltip = true;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.virtualJoypad.joystickHeld)
			{
				_ignoreRelease = true;
			}
			else
			{
				if (Game1.currentLocation is MermaidHouse)
				{
					return;
				}
				if (!Game1.options.verticalToolbar)
				{
					if (x > maxVisibleItems * itemSlotSize + Game1.toolbarPaddingX)
					{
						return;
					}
				}
				else if (x > maxVisibleItems * Game1.maxItemSlotSize + Game1.toolbarPaddingX)
				{
					return;
				}
				testToScrollToolbar(x, y);
				if (!toolbarPressed)
				{
					hoverTicksAtStart = DateTime.Now.Ticks;
				}
				if ((bool)Game1.player.usingTool)
				{
					return;
				}
				foreach (ClickableComponent button in buttons)
				{
					if (button.containsPoint(x, y))
					{
						toolbarPressed = true;
						break;
					}
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			toolbarPressed = false;
			if (_ignoreRelease)
			{
				_ignoreRelease = false;
			}
			else
			{
				if (Game1.currentLocation is MermaidHouse || Game1.player.isEating || !Game1.displayFarmer || Game1.currentLocation.tapToMove.TapHoldActive || (!Game1.options.verticalToolbar && x > maxVisibleItems * itemSlotSize + Game1.toolbarPaddingX))
				{
					return;
				}
				updateScrollIndex(x, y);
				hoverTicksAtStart = DateTime.Now.Ticks;
				if ((bool)Game1.player.usingTool)
				{
					foreach (ClickableComponent button in buttons)
					{
						if (button.containsPoint(x, y))
						{
							int num = Convert.ToInt32(button.name) + _drawStartIndex;
							if (Game1.player.CurrentToolIndex == num)
							{
								_nextToolIndex = -1;
							}
							else if (num >= _drawStartIndex && num < _drawStartIndex + maxVisibleItems)
							{
								_nextToolIndex = num;
							}
							break;
						}
					}
					return;
				}
				foreach (ClickableComponent button2 in buttons)
				{
					if (!button2.containsPoint(x, y))
					{
						continue;
					}
					int num2 = Convert.ToInt32(button2.name) + _drawStartIndex;
					if (Game1.player.CurrentToolIndex == num2)
					{
						Game1.player.CurrentToolIndex = -1;
					}
					else if (num2 >= _drawStartIndex && num2 < _drawStartIndex + maxVisibleItems)
					{
						Game1.player.CurrentToolIndex = num2;
						Game1.currentLocation.tapToMove.ClearAutoSelectTool();
						if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is MeleeWeapon)
						{
							TapToMove.mostRecentlyChosenMeleeWeapon = (MeleeWeapon)Game1.player.CurrentTool;
						}
						TutorialManager.Instance.TestForHoeSelected();
						if (Game1.player.ActiveObject != null)
						{
							Game1.player.showCarrying();
							Game1.playSound("pickUpItem");
						}
						else
						{
							Game1.player.showNotCarrying();
							Game1.playSound("stoneStep");
						}
					}
					break;
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			if (Game1.virtualJoypad.joystickHeld || Game1.virtualJoypad.joystickWasJustHeld || !Game1.displayHUD || Game1.currentLocation.tapToMove.TapHoldActive)
			{
				return;
			}
			hoverItem = null;
			if (!Game1.options.verticalToolbar && x > maxVisibleItems * itemSlotSize)
			{
				return;
			}
			testToScrollToolbar(x, y);
			foreach (ClickableComponent button in buttons)
			{
				if (button.containsPoint(x, y))
				{
					int num = Convert.ToInt32(button.name);
					if (num + _drawStartIndex < Game1.player.items.Count && Game1.player.items[num + _drawStartIndex] != null)
					{
						hoverTitle = Game1.player.items[num + _drawStartIndex].DisplayName;
						hoverItem = Game1.player.items[num + _drawStartIndex];
						if (hoverItem != lastHoverItem)
						{
							hoverTicksAtStart = DateTime.Now.Ticks;
						}
						lastHoverItem = hoverItem;
						if (Game1.options.verticalToolbar)
						{
							button.scale = Math.Min(button.scale + 0.05f, 1.1f);
							_tooltipPosition = new Vector2(Game1.toolbarPaddingX + itemSlotSize + 64, yOffset + num * itemSlotSize);
						}
						else
						{
							_tooltipPosition = new Vector2(12 + (num + 1) * itemSlotSize, 0f);
						}
					}
				}
				else if (!Game1.options.verticalToolbar)
				{
					button.scale = Math.Max(button.scale - 0.025f, 1f);
				}
			}
		}

		public void shifted(bool right)
		{
			if (right)
			{
				for (int i = 0; i < buttons.Count; i++)
				{
					buttons[i].scale = 1f + (float)i * 0.03f;
				}
				return;
			}
			for (int num = buttons.Count - 1; num >= 0; num--)
			{
				buttons[num].scale = 1f + (float)(11 - num) * 0.03f;
			}
		}

		public void ScrollToolbarOne(bool forward)
		{
			if (Game1.player.UsingTool)
			{
				return;
			}
			if (forward)
			{
				Game1.player.CurrentToolIndex++;
				if (Game1.player.CurrentToolIndex >= Game1.player.MaxItems)
				{
					Game1.player.CurrentToolIndex = Game1.player.MaxItems - 1;
				}
				if (Game1.player.CurrentToolIndex >= _drawStartIndex + maxVisibleItems - 3)
				{
					_startIndex = (_drawStartIndex = Math.Min(Math.Max(0, Game1.player.CurrentToolIndex - maxVisibleItems + 2), maxScrollIndex));
				}
			}
			else
			{
				Game1.player.CurrentToolIndex--;
				if (Game1.player.CurrentToolIndex < 0)
				{
					Game1.player.CurrentToolIndex = 0;
				}
				if (Game1.player.CurrentToolIndex <= _startIndex)
				{
					_startIndex = (_drawStartIndex = Math.Max(0, Game1.player.CurrentToolIndex - 1));
				}
			}
		}

		public override void update(GameTime time)
		{
			if (!Game1.player.usingTool && _nextToolIndex != -2147483648)
			{
				Game1.player.CurrentToolIndex = _nextToolIndex;
				_nextToolIndex = -2147483648;
				Game1.currentLocation.tapToMove.ClearAutoSelectTool();
				if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is MeleeWeapon)
				{
					TapToMove.mostRecentlyChosenMeleeWeapon = (MeleeWeapon)Game1.player.CurrentTool;
				}
			}
			testToAddMoreItems();
			try
			{
				GamePadState gamePadState = Game1.input.GetGamePadState();
				if (Game1.player.CurrentToolIndex <= -1)
				{
					return;
				}
				if (gamePadState.IsButtonDown(Buttons.RightShoulder) || gamePadState.IsButtonDown(Buttons.LeftShoulder))
				{
					_shoulderButtonDownCount++;
					if (_shoulderButtonDownCount >= 20)
					{
						_showTooltip = true;
						hoverTitle = Game1.player.items[Game1.player.CurrentToolIndex].DisplayName;
						hoverItem = Game1.player.items[Game1.player.CurrentToolIndex];
						hoverTicksAtStart = -4000000L;
						int num = Math.Max(0, Game1.player.CurrentToolIndex - _drawStartIndex);
						if (Game1.options.verticalToolbar)
						{
							_tooltipPosition = new Vector2(Game1.toolbarPaddingX + itemSlotSize + 64, yOffset + num * itemSlotSize);
						}
						else
						{
							_tooltipPosition = new Vector2(12 + (num + 1) * itemSlotSize, 0f);
						}
					}
				}
				else if (_shoulderButtonDownCount > 0)
				{
					_showTooltip = false;
					hoverTitle = null;
					hoverItem = null;
					_shoulderButtonDownCount = 0;
				}
			}
			catch (Exception)
			{
				_showTooltip = false;
				hoverTitle = null;
				hoverItem = null;
				_shoulderButtonDownCount = 0;
			}
		}

		public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
		{
			resetToolbar();
			UpdateButtonBounds();
		}

		public override bool isWithinBounds(int x, int y)
		{
			if (Game1.options.verticalToolbar)
			{
				return new Microsoft.Xna.Framework.Rectangle(buttons.First().bounds.X, buttons.First().bounds.Y, itemSlotSize, itemSlotSize * 12).Contains(x, y);
			}
			return new Microsoft.Xna.Framework.Rectangle(buttons.First().bounds.X, buttons.First().bounds.Y, 12 * Game1.maxItemSlotSize, Game1.maxItemSlotSize).Contains(x, y);
		}

		public override void draw(SpriteBatch b)
		{
			if (!visible)
			{
				return;
			}
			if (vertical != Game1.options.verticalToolbar)
			{
				vertical = Game1.options.verticalToolbar;
				resetToolbar();
				UpdateButtonBounds();
			}
			alignTop = false;
			Point center = Game1.player.GetBoundingBox().Center;
			Vector2 vector = Game1.GlobalToLocal(globalPosition: new Vector2(center.X, center.Y), viewport: viewport);
			if (Game1.options.pinToolbarToggle)
			{
				alignTop = false;
				transparency = Math.Min(1f, transparency + 0.075f);
				if (vector.Y > (float)(screenHeight - 192))
				{
					transparency = Math.Max(0.33f, transparency - 0.15f);
				}
			}
			else
			{
				alignTop = vector.Y > (float)(Game1.viewport.Height / 2 + 64);
				transparency = 1f;
			}
			if (_toolbarPaddingX != Game1.toolbarPaddingX)
			{
				resetToolbar();
			}
			if (_itemSlotSize != Game1.maxItemSlotSize)
			{
				resetToolbar();
			}
			if (Game1.options.verticalToolbar)
			{
				yPositionOnScreen = 0;
			}
			else if (alignTop)
			{
				yPositionOnScreen = Game1.maxItemSlotSize + 24;
			}
			else
			{
				yPositionOnScreen = screenHeight;
			}
			UpdateButtonBounds();
			if (Game1.options.verticalToolbar)
			{
				b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(Game1.toolbarPaddingX + itemSlotSize, 0, 16, screenHeight), new Microsoft.Xna.Framework.Rectangle(224, 32, 16, 4), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
				int num = yPositionOnScreen + yOffset + maxVisibleItems * itemSlotSize;
				if (num < screenHeight)
				{
					b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(0, num, Game1.toolbarPaddingX + itemSlotSize, screenHeight - num), new Microsoft.Xna.Framework.Rectangle(12, 296, 4, 4), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
				}
			}
			else
			{
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, toolbarTextSource, Game1.toolbarPaddingX, yPositionOnScreen - itemSlotSize - 24, itemSlotSize * maxVisibleItems + 24, itemSlotSize + 24, Color.White * transparency, 1f, drawShadow: false);
			}
			int num2 = _drawStartIndex + maxVisibleItems;
			if (Game1.options.verticalToolbar)
			{
				num2++;
			}
			for (int i = _drawStartIndex; i < num2; i++)
			{
				if (Game1.options.verticalToolbar)
				{
					Vector2 vector2 = new Vector2(Game1.toolbarPaddingX, yPositionOnScreen + yOffset + i * itemSlotSize);
					vector2.Y -= _drawStartIndex * itemSlotSize;
					if (i >= (int)Game1.player.maxItems)
					{
						b.Draw(Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(0, (int)vector2.Y, Game1.toolbarPaddingX + itemSlotSize, itemSlotSize), new Microsoft.Xna.Framework.Rectangle(41, 1, 20, 20), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
						b.Draw(Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle(0, (int)vector2.Y + itemSlotSize, Game1.toolbarPaddingX + itemSlotSize, itemSlotSize), new Microsoft.Xna.Framework.Rectangle(41, 1, 20, 20), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
						continue;
					}
					if (i % 2 == 0)
					{
						b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(0, (int)vector2.Y, Game1.toolbarPaddingX + itemSlotSize, itemSlotSize), new Microsoft.Xna.Framework.Rectangle(12, 296, 4, 4), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
					}
					else
					{
						b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(0, (int)vector2.Y, Game1.toolbarPaddingX + itemSlotSize, itemSlotSize), new Microsoft.Xna.Framework.Rectangle(12, 280, 4, 4), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
					}
					if (Game1.player.CurrentToolIndex == i)
					{
						b.Draw(Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(0, (int)vector2.Y, Game1.toolbarPaddingX + itemSlotSize, itemSlotSize), new Microsoft.Xna.Framework.Rectangle(150, 470, 4, 4), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
					}
				}
				else
				{
					Vector2 vector2 = new Vector2(Game1.toolbarPaddingX + xOffset + i * Game1.maxItemSlotSize, yPositionOnScreen - Game1.maxItemSlotSize - 12);
					vector2.X -= _drawStartIndex * itemSlotSize;
					if (i >= (int)Game1.player.maxItems)
					{
						b.Draw(Game1.mobileSpriteSheet, new Microsoft.Xna.Framework.Rectangle((int)vector2.X, (int)vector2.Y, itemSlotSize, itemSlotSize), new Microsoft.Xna.Framework.Rectangle(41, 1, 20, 20), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
					}
					else if (Game1.player.CurrentToolIndex == i)
					{
						b.Draw(sourceRectangle: new Microsoft.Xna.Framework.Rectangle(20, 96, 20, 20), texture: Game1.mobileSpriteSheet, position: vector2, color: Color.White * transparency, rotation: 0f, origin: new Vector2(0f, 0f), scale: 4f * ((float)itemSlotSize / 80f), effects: SpriteEffects.None, layerDepth: 1E-07f);
					}
					else
					{
						b.Draw(sourceRectangle: new Microsoft.Xna.Framework.Rectangle(0, 96, 20, 20), texture: Game1.mobileSpriteSheet, position: vector2, color: Color.White * transparency, rotation: 0f, origin: new Vector2(0f, 0f), scale: 4f * ((float)itemSlotSize / 80f), effects: SpriteEffects.None, layerDepth: 1E-07f);
					}
				}
			}
			if (Game1.options.verticalToolbar && hoverItem != null)
			{
				b.Draw(Game1.menuTexture, new Microsoft.Xna.Framework.Rectangle(0, (int)_tooltipPosition.Y, Game1.toolbarPaddingX + itemSlotSize, itemSlotSize), new Microsoft.Xna.Framework.Rectangle(88, 592, 4, 4), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-06f);
			}
			float num3 = (float)itemSlotSize / 72f;
			num3 = (float)(Math.Floor((double)num3 * 100.0) / 100.0);
			for (int j = _drawStartIndex; j < num2; j++)
			{
				if (Game1.options.verticalToolbar)
				{
					Vector2 vector2 = new Vector2(Game1.toolbarPaddingX, yPositionOnScreen + yOffset + j * itemSlotSize);
					vector2.Y -= _drawStartIndex * itemSlotSize;
					if (Game1.player.items.Count > j && Game1.player.items.ElementAt(j) != null)
					{
						Game1.player.items[j].itemSlotSize = itemSlotSize;
						Game1.player.items[j].drawInToolbar = true;
						Game1.player.items[j].drawInMenu(b, vector2, num3, transparency, 1E-07f);
						Game1.player.items[j].drawInToolbar = false;
						Game1.player.items[j].itemSlotSize = 64;
					}
				}
				else
				{
					Vector2 vector2 = new Vector2(Game1.toolbarPaddingX + xOffset + j * Game1.maxItemSlotSize, yPositionOnScreen - Game1.maxItemSlotSize - yOffset);
					vector2.X -= _drawStartIndex * itemSlotSize;
					if (Game1.player.items.Count > j && Game1.player.items.ElementAt(j) != null)
					{
						Game1.player.items[j].itemSlotSize = Game1.maxItemSlotSize;
						Game1.player.items[j].drawInMenu(b, vector2, num3, transparency, 1E-07f);
						Game1.player.items[j].itemSlotSize = 64;
					}
				}
			}
			if (hoverItem == null || !_showTooltip)
			{
				return;
			}
			if (Game1.options.verticalToolbar)
			{
				if (_tooltipPosition.Y > (float)(screenHeight - (Game1.maxItemSlotSize + 24)))
				{
					_tooltipPosition.Y = screenHeight - (Game1.maxItemSlotSize + 24);
				}
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, toolbarTextSource, (int)_tooltipPosition.X, (int)_tooltipPosition.Y, itemSlotSize + 24, itemSlotSize + 24, Color.White * transparency, 1f, drawShadow: false);
				hoverItem.itemSlotSize = itemSlotSize;
				hoverItem.drawInMenu(b, new Vector2((int)_tooltipPosition.X + 12, (int)_tooltipPosition.Y + 12), num3, transparency, 1E-07f);
				hoverItem.itemSlotSize = 64;
			}
			if (DateTime.Now.Ticks - hoverTicksAtStart > 4000000)
			{
				if (Game1.options.verticalToolbar)
				{
					IClickableMenu.drawToolTipOverridePosition(b, hoverItem.getDescription(), hoverItem.DisplayName, hoverItem, (int)_tooltipPosition.X + itemSlotSize + 36, (int)_tooltipPosition.Y);
				}
				else
				{
					if (alignTop)
					{
						_tooltipPosition.Y = Game1.maxItemSlotSize + 28;
					}
					else
					{
						_tooltipPosition.Y = Math.Max(0, screenHeight - 640);
					}
					IClickableMenu.drawToolTipOverridePosition(b, hoverItem.getDescription(), hoverItem.DisplayName, hoverItem, (int)_tooltipPosition.X, (int)_tooltipPosition.Y);
				}
			}
			hoverItem = null;
		}

		public void resetToolbar()
		{
			if (Game1.options.verticalToolbar)
			{
				xPositionOnScreen = 0;
				yPositionOnScreen = 0;
				yOffset = 0;
				toolbarHeight = screenHeight;
				itemSlotSize = Math.Min(200, Game1.maxItemSlotSize);
				toolBarItemWidth = Game1.toolbarPaddingX + itemSlotSize;
				toolbarWidth = toolBarItemWidth + 16;
				_toolbarPaddingX = Game1.toolbarPaddingX;
			}
			else
			{
				yOffset = 12;
				int num = (toolBarItemWidth = (itemSlotSize = Game1.maxItemSlotSize));
				toolbarWidth = Game1.toolbarPaddingX + Game1.maxItemSlotSize * 12 + xOffset * 2;
				toolbarHeight = Game1.maxItemSlotSize + yOffset * 2;
			}
		}

		private void UpdateButtonBounds()
		{
			for (int i = 0; i < buttons.Count; i++)
			{
				if (Game1.options.verticalToolbar)
				{
					buttons[i].bounds = new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen, i * itemSlotSize, Game1.toolbarPaddingX + itemSlotSize, itemSlotSize);
				}
				else
				{
					buttons[i].bounds = new Microsoft.Xna.Framework.Rectangle(Game1.toolbarPaddingX + xOffset + i * Game1.maxItemSlotSize, yPositionOnScreen - Game1.maxItemSlotSize - yOffset, Game1.maxItemSlotSize, Game1.maxItemSlotSize);
				}
			}
		}
	}
}
