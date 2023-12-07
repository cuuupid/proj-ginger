using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Mobile;

namespace StardewValley.Menus
{
	public class MenuWithInventory : IClickableMenu
	{
		public const int region_okButton = 4857;

		public const int region_trashCan = 5948;

		public string descriptionText = "";

		public string hoverText = "";

		public string descriptionTitle = "";

		public InventoryMenu inventory;

		public Item heldItem;

		public Item hoveredItem;

		public int wiggleWordsTimer;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent trashCan;

		public float trashCanLidRotation;

		public ClickableComponent dropItemInvisibleButton;

		public MenuWithInventory(InventoryMenu.highlightThisItem highlighterMethod = null, bool okButton = false, bool trashCan = false, int xPositionOnScreen = 0, int yPositionOnScreen = 0, int width = 1280, int height = 720)
		{
			base.width = Game1.uiViewport.Width - 2 * Game1.xEdge;
			base.xPositionOnScreen = Game1.xEdge;
			initializeUpperRightCloseButton();
			base.yPositionOnScreen = yPositionOnScreen;
			base.height = Game1.uiViewport.Height - yPositionOnScreen;
			if (base.height > 1080)
			{
				height = (base.height = 1080);
				yPositionOnScreen = (base.yPositionOnScreen = (Game1.uiViewport.Height - base.height) / 2);
			}
			inventory = new InventoryMenu(base.xPositionOnScreen, base.yPositionOnScreen + base.height / 2, playerInventory: true, null, highlighterMethod, -1, 3, 0, 0, drawSlots: true, base.width, base.height / 2 - (MobileDisplay.IsiPhoneX ? 32 : 0), trashCan);
			inventory.isOnMultiInventoryPage = true;
			if (okButton)
			{
				this.okButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
				{
					myID = 4857,
					upNeighborID = 5948,
					leftNeighborID = 12
				};
			}
			dropItemInvisibleButton = new ClickableComponent(new Rectangle(xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128, yPositionOnScreen - 12, 64, 64), "");
		}

		public void movePosition(int dx, int dy)
		{
		}

		public override bool readyToClose()
		{
			return heldItem == null;
		}

		public override bool isWithinBounds(int x, int y)
		{
			return base.isWithinBounds(x, y);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (upperRightCloseButton != null && upperRightCloseButton.containsPoint(x, y) && readyToClose())
			{
				OnTapCloseButton();
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			inventory.receiveLeftClick(x, y, playSound);
			heldItem = inventory.selectItemAt(x, y, heldItem);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			inventory.performHoverAction(x, y);
		}

		public override void update(GameTime time)
		{
			if (wiggleWordsTimer > 0)
			{
				wiggleWordsTimer -= time.ElapsedGameTime.Milliseconds;
			}
			inventory.update(time);
		}

		public virtual void draw(SpriteBatch b, bool drawUpperPortion = true, bool drawDescriptionArea = true, int red = -1, int green = -1, int blue = -1)
		{
			inventory.draw(b, red, green, blue);
			inventory.drawInfoPanel(b, force: true);
			upperRightCloseButton.draw(b, Color.White, -1f);
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			if (yPositionOnScreen < IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
			{
				yPositionOnScreen = IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder;
			}
			if (xPositionOnScreen < Game1.xEdge)
			{
				xPositionOnScreen = Game1.xEdge;
			}
			int num = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16;
			if (okButton != null)
			{
				okButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
			}
		}

		public override void draw(SpriteBatch b)
		{
			throw new NotImplementedException();
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			inventory.releaseLeftClick(x, y);
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			inventory.leftClickHeld(x, y);
		}

		public override void receiveGamePadButton(Buttons b)
		{
			inventory.receiveGamePadButton(b);
			if (b == Buttons.B)
			{
				receiveLeftClick(upperRightCloseButton.bounds.X, upperRightCloseButton.bounds.Y);
			}
		}
	}
}
