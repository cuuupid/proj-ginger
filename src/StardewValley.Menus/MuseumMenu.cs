using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Mobile;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class MuseumMenu : MenuWithInventory
	{
		public const int startingState = 0;

		public const int placingInMuseumState = 1;

		public const int exitingState = 2;

		public int fadeTimer;

		public int state;

		public int menuPositionOffset;

		public bool fadeIntoBlack;

		public bool menuMovingDown;

		public float blackFadeAlpha;

		public SparklingText sparkleText;

		public Vector2 globalLocationOfSparklingArtifact;

		private Microsoft.Xna.Framework.Rectangle _inventoryRect;

		private int _drawAtX = -1;

		private int _drawAtY = -1;

		private int _lastTapX = -1;

		private int _lastTapY = -1;

		private const int DRAG_THRESHOLD = 5;

		private bool _dragging;

		private bool previouslyPlaced;

		private bool movingExistingPiece;

		public bool rearrangeMode;

		private Vector2 _startTileLocation;

		private float _lastZoom;

		private bool holdingMuseumPiece;

		public bool reOrganizing;

		public MuseumMenu(InventoryMenu.highlightThisItem highlighterMethod, bool rearrange = false)
			: base((Game1.currentLocation as LibraryMuseum).isItemSuitableForDonation, okButton: true, trashCan: false, Game1.xEdge)
		{
			rearrangeMode = rearrange;
			inventory.showOrganizeButton = false;
			xPositionOnScreen = Game1.xEdge;
			yPositionOnScreen = 0;
			width = Game1.uiViewport.Width - Game1.xEdge * 2;
			height = Game1.uiViewport.Height - yPositionOnScreen;
			int num = Math.Min(height / 2, 540);
			_inventoryRect = new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen, yPositionOnScreen + height - num, width, num);
			int invY = inventory.getInvY();
			int invHeight = inventory.getInvHeight();
			int num2 = _inventoryRect.Y + (_inventoryRect.Height - inventory.getInvHeight()) / 2 - inventory.getInvY();
			fadeTimer = 800;
			fadeIntoBlack = true;
			Game1.player.forceCanMove();
			if (Game1.options.SnappyMenus)
			{
				if (okButton != null)
				{
					okButton.myID = 106;
				}
				populateClickableComponentList();
				currentlySnappedComponent = getComponentWithID(0);
				snapCursorToCurrentSnappedComponent();
			}
			Game1.displayHUD = false;
		}

		public override bool shouldClampGamePadCursor()
		{
			return true;
		}

		public override void receiveKeyPress(Keys key)
		{
			if (fadeTimer > 0)
			{
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.menuButton) && readyToClose())
			{
				state = 2;
				fadeTimer = 500;
				fadeIntoBlack = true;
			}
			else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.menuButton) && !holdingMuseumPiece && menuMovingDown)
			{
				if (heldItem != null)
				{
					Game1.playSound("bigDeSelect");
					Utility.CollectOrDrop(heldItem);
					heldItem = null;
				}
				ReturnToDonatableItems();
			}
			else if (Game1.options.SnappyMenus && heldItem == null && !reOrganizing)
			{
				base.receiveKeyPress(key);
			}
			if (!Game1.options.SnappyMenus)
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
			else
			{
				if (heldItem == null && !reOrganizing)
				{
					return;
				}
				LibraryMuseum libraryMuseum = Game1.currentLocation as LibraryMuseum;
				Vector2 vector = new Vector2((Game1.getMouseX(ui_scale: false) + Game1.viewport.X) / 64, (Game1.getMouseY(ui_scale: false) + Game1.viewport.Y) / 64);
				if (!libraryMuseum.isTileSuitableForMuseumPiece((int)vector.X, (int)vector.Y) && (!reOrganizing || !libraryMuseum.museumPieces.ContainsKey(vector)))
				{
					vector = libraryMuseum.getFreeDonationSpot();
					Game1.setMousePosition((int)Utility.ModifyCoordinateForUIScale(vector.X * 64f - (float)Game1.viewport.X + 32f), (int)Utility.ModifyCoordinateForUIScale(vector.Y * 64f - (float)Game1.viewport.Y + 32f));
					return;
				}
				if (key == Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton))
				{
					vector = libraryMuseum.findMuseumPieceLocationInDirection(vector, 0, 21, !reOrganizing);
				}
				else if (key == Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton))
				{
					vector = libraryMuseum.findMuseumPieceLocationInDirection(vector, 1, 21, !reOrganizing);
				}
				else if (key == Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton))
				{
					vector = libraryMuseum.findMuseumPieceLocationInDirection(vector, 2, 21, !reOrganizing);
				}
				else if (key == Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton))
				{
					vector = libraryMuseum.findMuseumPieceLocationInDirection(vector, 3, 21, !reOrganizing);
				}
				if (!Game1.viewport.Contains(new Location((int)(vector.X * 64f + 32f), Game1.viewport.Y + 1)))
				{
					Game1.panScreen((int)(vector.X * 64f - (float)Game1.viewport.X), 0);
				}
				else if (!Game1.viewport.Contains(new Location(Game1.viewport.X + 1, (int)(vector.Y * 64f + 32f))))
				{
					Game1.panScreen(0, (int)(vector.Y * 64f - (float)Game1.viewport.Y));
				}
				Game1.setMousePosition((int)Utility.ModifyCoordinateForUIScale((int)vector.X * 64 - Game1.viewport.X + 32), (int)Utility.ModifyCoordinateForUIScale((int)vector.Y * 64 - Game1.viewport.Y + 32));
			}
		}

		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return false;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.B)
			{
				receiveLeftClick(upperRightCloseButton.bounds.X, upperRightCloseButton.bounds.Y);
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (fadeTimer > 0)
			{
				return;
			}
			Item item = heldItem;
			_lastTapX = -1;
			_lastTapY = -1;
			if (upperRightCloseButton != null && upperRightCloseButton.containsPoint(x, y))
			{
				if (holdingMuseumPiece && heldItem != null)
				{
					if (previouslyPlaced)
					{
						(Game1.currentLocation as LibraryMuseum).museumPieces.Add(_startTileLocation, heldItem.parentSheetIndex);
					}
					holdingMuseumPiece = false;
					heldItem = null;
					return;
				}
				if (playSound)
				{
					Game1.playSound("bigDeSelect");
				}
				heldItem = null;
				inventory.currentlySelectedItem = -1;
				state = 2;
				fadeTimer = 800;
				fadeIntoBlack = true;
			}
			else
			{
				if (0 == 0 && heldItem == null && inventory.isWithinBounds(x, y))
				{
					_ = rearrangeMode;
				}
				if (heldItem != null && item == null)
				{
					menuMovingDown = true;
				}
				if (okButton != null && okButton.containsPoint(x, y) && readyToClose())
				{
					state = 2;
					fadeTimer = 800;
					fadeIntoBlack = true;
					Game1.playSound("bigDeSelect");
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (!holdingMuseumPiece)
			{
				if (!rearrangeMode)
				{
					heldItem = inventory.selectItemAt(x, y, heldItem);
				}
				if (!_dragging && heldItem == null && (!inventory.isWithinBounds(x, y) || rearrangeMode))
				{
					int num = (Game1.getMouseX(ui_scale: false) + Game1.viewport.X) / 64;
					int num2 = (Game1.getMouseY(ui_scale: false) + Game1.viewport.Y) / 64;
					Vector2 vector = new Vector2(num, num2);
					LibraryMuseum libraryMuseum = (LibraryMuseum)Game1.currentLocation;
					if (libraryMuseum.museumPieces.ContainsKey(vector))
					{
						_startTileLocation = vector;
						heldItem = new Object(libraryMuseum.museumPieces[vector], 1);
						libraryMuseum.museumPieces.Remove(vector);
						holdingMuseumPiece = true;
						previouslyPlaced = true;
					}
				}
				else if (inventory.currentlySelectedItem == -1)
				{
					heldItem = null;
				}
				if (heldItem != null && inventory.currentlySelectedItem != -1)
				{
					holdingMuseumPiece = true;
					previouslyPlaced = false;
				}
			}
			else if (holdingMuseumPiece && !_dragging)
			{
				if (!movingExistingPiece)
				{
					if (rearrangeMode)
					{
						SwapItem();
					}
					else
					{
						placeItem(x, y, heldItem);
					}
				}
				movingExistingPiece = false;
			}
			_dragging = false;
			_lastTapX = -1;
			_lastTapY = -1;
		}

		public virtual void ReturnToDonatableItems()
		{
			menuMovingDown = false;
			holdingMuseumPiece = false;
			reOrganizing = false;
			if (Game1.options.SnappyMenus)
			{
				movePosition(0, -menuPositionOffset);
				menuPositionOffset = 0;
				base.snapCursorToCurrentSnappedComponent();
			}
		}

		private bool SwapItem()
		{
			if (heldItem == null)
			{
				return false;
			}
			int num = (Game1.getMouseX(ui_scale: false) + Game1.viewport.X) / 64;
			int num2 = (Game1.getMouseY(ui_scale: false) + Game1.viewport.Y) / 64;
			Vector2 key = new Vector2(num, num2);
			LibraryMuseum libraryMuseum = (LibraryMuseum)Game1.currentLocation;
			if (libraryMuseum.museumPieces.TryGetValue(key, out var value))
			{
				Item item = new Object(value, 1);
				libraryMuseum.museumPieces.Remove(key);
				libraryMuseum.museumPieces[key] = heldItem.parentSheetIndex;
				if (item != null)
				{
					libraryMuseum.museumPieces[_startTileLocation] = item.parentSheetIndex;
				}
				heldItem = null;
				holdingMuseumPiece = false;
				movingExistingPiece = false;
				_dragging = false;
				return true;
			}
			if (libraryMuseum.isTileSuitableForMuseumPiece((int)key.X, (int)key.Y))
			{
				libraryMuseum.museumPieces[key] = heldItem.parentSheetIndex;
				heldItem = null;
				holdingMuseumPiece = false;
				movingExistingPiece = false;
				_dragging = false;
				return true;
			}
			return false;
		}

		public override bool readyToClose()
		{
			if (!holdingMuseumPiece && heldItem == null)
			{
				return !menuMovingDown;
			}
			return false;
		}

		protected override void cleanupBeforeExit()
		{
			if (heldItem != null)
			{
				heldItem = Game1.player.addItemToInventory(heldItem);
				if (heldItem != null)
				{
					Game1.createItemDebris(heldItem, Game1.player.Position, -1);
					heldItem = null;
				}
			}
			Game1.displayHUD = true;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			Item item = heldItem;
			if (fadeTimer <= 0)
			{
				base.receiveRightClick(x, y, playSound: true);
			}
			if (heldItem != null && item == null)
			{
				menuMovingDown = true;
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (sparkleText != null && sparkleText.update(time))
			{
				sparkleText = null;
			}
			if (fadeTimer > 0)
			{
				fadeTimer -= time.ElapsedGameTime.Milliseconds;
				if (fadeIntoBlack)
				{
					blackFadeAlpha = 0f + (1500f - (float)fadeTimer) / 1500f;
				}
				else
				{
					blackFadeAlpha = 1f - (1500f - (float)fadeTimer) / 1500f;
				}
				if (fadeTimer <= 0)
				{
					switch (state)
					{
					case 0:
						state = 1;
						_lastZoom = PinchZoom.Instance.ZoomLevel;
						PinchZoom.Instance.SetZoomLevel(1f);
						Game1.options.desiredBaseZoomLevel = (Game1.options.baseZoomLevel = 1f);
						Game1.game1.refreshWindowSettings();
						Game1.viewportFreeze = true;
						Game1.viewport.Location = new Location(1152, 128);
						Game1.clampViewportToGameMap();
						fadeTimer = 800;
						fadeIntoBlack = false;
						break;
					case 2:
						Game1.viewportFreeze = false;
						PinchZoom.Instance.SetZoomLevel(_lastZoom);
						Game1.options.desiredBaseZoomLevel = (Game1.options.baseZoomLevel = _lastZoom);
						Game1.game1.refreshWindowSettings();
						fadeIntoBlack = false;
						fadeTimer = 800;
						state = 3;
						break;
					case 3:
						exitThisMenuNoSound();
						break;
					}
				}
			}
			if (menuMovingDown && menuPositionOffset < height / 3)
			{
				menuPositionOffset += 8;
				movePosition(0, 8);
			}
			else if (!menuMovingDown && menuPositionOffset > 0)
			{
				menuPositionOffset -= 8;
				movePosition(0, -8);
			}
			if (!PinchZoom.Instance.CheckForPinchZoom())
			{
				MouseState mouseState = Game1.input.GetMouseState();
				TestToPan(mouseState.X, mouseState.Y, mouseState.LeftButton);
			}
		}

		private void snapCursorToCurrentMuseumSpot()
		{
			if (menuMovingDown)
			{
				Vector2 vector = new Vector2((Game1.getMouseX(ui_scale: false) + Game1.viewport.X) / 64, (Game1.getMouseY(ui_scale: false) + Game1.viewport.Y) / 64);
				Game1.setMousePosition((int)vector.X * 64 - Game1.viewport.X + 32, (int)vector.Y * 64 - Game1.viewport.Y + 32, ui_scale: false);
			}
		}

		public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			movePosition(0, Game1.viewport.Height - yPositionOnScreen - height);
			Game1.player.forceCanMove();
		}

		public override void draw(SpriteBatch b)
		{
			if ((fadeTimer <= 0 || !fadeIntoBlack) && state != 3)
			{
				DrawPlacementGrid(b);
				if (!holdingMuseumPiece && !rearrangeMode)
				{
					IClickableMenu.drawTextureBox(b, _inventoryRect.X, _inventoryRect.Y, _inventoryRect.Width, _inventoryRect.Height, Color.White);
					base.draw(b, drawUpperPortion: false, drawDescriptionArea: false);
				}
				else if (heldItem != null)
				{
					Vector2 location = new Vector2(Game1.getMouseX(), Game1.getMouseY() - 128);
					heldItem.drawInMenu(b, location, 2f, 0.5f, 0.0865f);
				}
				if (rearrangeMode || (heldItem != null && !holdingMuseumPiece))
				{
					upperRightCloseButton.draw(b);
				}
				drawMouse(b);
				if (sparkleText != null)
				{
					sparkleText.draw(b, Utility.ModifyCoordinatesForUIScale(Game1.GlobalToLocal(Game1.viewport, globalLocationOfSparklingArtifact)));
				}
			}
			b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * blackFadeAlpha);
		}

		private void TestToPan(int x, int y, ButtonState leftButton)
		{
			if (upperRightCloseButton == null || upperRightCloseButton.containsPoint(x, y) || leftButton != ButtonState.Pressed)
			{
				return;
			}
			if (_lastTapX != -1 && _lastTapY != -1)
			{
				int num = (int)((float)(_lastTapX - x) / Game1.options.zoomLevel);
				int num2 = (int)((float)(_lastTapY - y) / Game1.options.zoomLevel);
				if (num != 0 || num2 != 0)
				{
					if (!_dragging && (Math.Abs(num) > 5 || Math.Abs(num2) > 5))
					{
						_dragging = true;
					}
					Game1.panScreen(num, num2, inventory.getInvHeight());
				}
			}
			_drawAtX = (int)((float)x / Game1.options.zoomLevel);
			_drawAtY = (int)((float)y / Game1.options.zoomLevel);
			_lastTapX = x;
			_lastTapY = y;
		}

		public void DrawPlacementGrid(SpriteBatch b)
		{
			if ((fadeTimer > 0 && fadeIntoBlack) || state == 3 || heldItem == null)
			{
				return;
			}
			Game1.StartWorldDrawInUI(b);
			for (int i = Game1.viewport.Y / 64 - 1; i < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 2; i++)
			{
				for (int j = Game1.viewport.X / 64 - 1; j < (Game1.viewport.X + Game1.viewport.Width) / 64 + 1; j++)
				{
					if ((Game1.currentLocation as LibraryMuseum).isTileSuitableForMuseumPiece(j, i))
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(j, i) * 64f), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 29), Color.LightGreen);
					}
				}
			}
			Game1.EndWorldDrawInUI(b);
		}

		private bool placeItem(int x, int y, Item oldItem)
		{
			if (oldItem != null && heldItem != null)
			{
				int num = (Game1.getMouseX(ui_scale: false) + Game1.viewport.X) / 64;
				int num2 = (Game1.getMouseY(ui_scale: false) + Game1.viewport.Y) / 64;
				if ((Game1.currentLocation as LibraryMuseum).isTileSuitableForMuseumPiece(num, num2) && (Game1.currentLocation as LibraryMuseum).isItemSuitableForDonation(heldItem))
				{
					int count = (Game1.currentLocation as LibraryMuseum).getRewardsForPlayer(Game1.player).Count;
					(Game1.currentLocation as LibraryMuseum).museumPieces.Add(new Vector2(num, num2), (heldItem as Object).parentSheetIndex);
					Game1.playSound("stoneStep");
					holdingMuseumPiece = false;
					if (!previouslyPlaced)
					{
						if ((Game1.currentLocation as LibraryMuseum).getRewardsForPlayer(Game1.player).Count > count)
						{
							sparkleText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:NewReward"), Color.MediumSpringGreen, Color.White);
							Game1.playSound("reward");
							globalLocationOfSparklingArtifact = new Vector2((float)(num * 64 + 32) - sparkleText.textWidth / 2f, num2 * 64 - 48);
						}
						else
						{
							Game1.playSound("newArtifact");
						}
					}
					Game1.player.completeQuest(24);
					heldItem.Stack--;
					if (heldItem.Stack <= 0 && !rearrangeMode)
					{
						Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
						inventory.currentlySelectedItem = -1;
						holdingMuseumPiece = false;
						heldItem = null;
					}
					heldItem = null;
					inventory.currentlySelectedItem = -1;
					holdingMuseumPiece = false;
					menuMovingDown = false;
					int num3 = (Game1.currentLocation as LibraryMuseum).museumPieces.Count();
					if (num3 >= 95)
					{
						Game1.getAchievement(5);
					}
					else if (num3 >= 40)
					{
						Game1.getAchievement(28);
					}
					return true;
				}
			}
			return false;
		}
	}
}
