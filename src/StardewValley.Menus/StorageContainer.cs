using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class StorageContainer : MenuWithInventory
	{
		public delegate bool behaviorOnItemChange(Item i, int position, Item old, StorageContainer container, bool onRemoval = false);

		public InventoryMenu ItemsToGrabMenu;

		private TemporaryAnimatedSprite poof;

		private behaviorOnItemChange itemChangeBehavior;

		private Rectangle topInv;

		private Rectangle bottomInv;

		private float widthMod;

		private float heightMod;

		private new int width;

		private new int height;

		public StorageContainer(IList<Item> inventory, int capacity, int rows = 3, behaviorOnItemChange itemChangeBehavior = null, InventoryMenu.highlightThisItem highlightMethod = null)
			: base(highlightMethod, okButton: true, trashCan: true, Game1.xEdge)
		{
			this.itemChangeBehavior = itemChangeBehavior;
			int num = 64 * (capacity / rows);
			int num2 = 64 * rows + 16;
			xPositionOnScreen = Game1.xEdge;
			yPositionOnScreen = 0;
			width = Game1.uiViewport.Width - Game1.xEdge * 2;
			height = Game1.uiViewport.Height - yPositionOnScreen;
			widthMod = (float)width / 1280f;
			heightMod = (float)height / 720f;
			topInv = new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height / 2);
			bottomInv = new Rectangle(xPositionOnScreen, yPositionOnScreen + height / 2, width, height / 2);
			ItemsToGrabMenu = new InventoryMenu(topInv.X, topInv.Y, playerInventory: false, inventory, null, capacity, rows, 0, 0, drawSlots: true, width, height / 2, showTrash: false, showOrganizeButton: false);
			int invWidth = ItemsToGrabMenu.getInvWidth();
			ItemsToGrabMenu.movePosition((width - invWidth) / 2 - ItemsToGrabMenu.xOffset, 0);
			topInv.X = ItemsToGrabMenu.getInvX() - 16;
			topInv.Width = invWidth + 32;
			for (int i = 0; i < ItemsToGrabMenu.actualInventory.Count; i++)
			{
				if (i >= ItemsToGrabMenu.actualInventory.Count - ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows)
				{
					ItemsToGrabMenu.inventory[i].downNeighborID = i + 53910;
				}
			}
			for (int j = 0; j < base.inventory.inventory.Count; j++)
			{
				base.inventory.inventory[j].myID = j + 53910;
				if (base.inventory.inventory[j].downNeighborID != -1)
				{
					base.inventory.inventory[j].downNeighborID += 53910;
				}
				if (base.inventory.inventory[j].rightNeighborID != -1)
				{
					base.inventory.inventory[j].rightNeighborID += 53910;
				}
				if (base.inventory.inventory[j].leftNeighborID != -1)
				{
					base.inventory.inventory[j].leftNeighborID += 53910;
				}
				if (base.inventory.inventory[j].upNeighborID != -1)
				{
					base.inventory.inventory[j].upNeighborID += 53910;
				}
				if (j < 12)
				{
					base.inventory.inventory[j].upNeighborID = ItemsToGrabMenu.actualInventory.Count - ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows;
				}
			}
			dropItemInvisibleButton.myID = -500;
			ItemsToGrabMenu.dropItemInvisibleButton.myID = -500;
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				setCurrentlySnappedComponentTo(53910);
				snapCursorToCurrentSnappedComponent();
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			int num = 64 * (ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows);
			int num2 = 64 * ItemsToGrabMenu.rows + 16;
			ItemsToGrabMenu = new InventoryMenu(Game1.uiViewport.Width / 2 - num / 2, yPositionOnScreen + 64, playerInventory: false, ItemsToGrabMenu.actualInventory, null, ItemsToGrabMenu.capacity, ItemsToGrabMenu.rows);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			Item item = heldItem;
			int num = item?.Stack ?? (-1);
			if (bottomInv.Contains(x, y))
			{
				base.receiveLeftClick(x, y, playSound: false);
				if (itemChangeBehavior == null && item == null && heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					heldItem = ItemsToGrabMenu.tryToAddItem(heldItem, "Ship");
				}
			}
			bool flag = true;
			if (ItemsToGrabMenu.isWithinBounds(x, y))
			{
				heldItem = ItemsToGrabMenu.leftClick(x, y, heldItem, playSound: false);
				if ((heldItem != null && item == null) || (heldItem != null && item != null && !heldItem.Equals(item)))
				{
					if (itemChangeBehavior != null)
					{
						flag = itemChangeBehavior(heldItem, ItemsToGrabMenu.getInventoryPositionOfClick(x, y), item, this, onRemoval: true);
					}
					if (flag)
					{
						Game1.playSound("dwop");
					}
				}
				if ((heldItem == null && item != null) || (heldItem != null && item != null && !heldItem.Equals(item)))
				{
					Item one = heldItem;
					if (heldItem == null && ItemsToGrabMenu.getItemAt(x, y) != null && num < ItemsToGrabMenu.getItemAt(x, y).Stack)
					{
						one = item.getOne();
						one.Stack = num;
					}
					if (itemChangeBehavior != null)
					{
						flag = itemChangeBehavior(item, ItemsToGrabMenu.getInventoryPositionOfClick(x, y), one, this);
					}
					if (flag)
					{
						Game1.playSound("Ship");
					}
				}
				if (heldItem is Object && (bool)(heldItem as Object).isRecipe)
				{
					string key = heldItem.Name.Substring(0, heldItem.Name.IndexOf("Recipe") - 1);
					try
					{
						if ((heldItem as Object).Category == -7)
						{
							Game1.player.cookingRecipes.Add(key, 0);
						}
						else
						{
							Game1.player.craftingRecipes.Add(key, 0);
						}
						poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
						Game1.playSound("newRecipe");
					}
					catch (Exception)
					{
					}
					heldItem = null;
				}
				else if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && Game1.player.addItemToInventoryBool(heldItem))
				{
					heldItem = null;
					if (itemChangeBehavior != null)
					{
						flag = itemChangeBehavior(heldItem, ItemsToGrabMenu.getInventoryPositionOfClick(x, y), item, this, onRemoval: true);
					}
					if (flag)
					{
						Game1.playSound("coin");
					}
				}
			}
			if (upperRightCloseButton.containsPoint(x, y) && readyToClose())
			{
				Game1.playSound("bigDeSelect");
				Game1.exitActiveMenu();
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			int num = ((heldItem != null) ? heldItem.Stack : 0);
			Item item = heldItem;
			if (isWithinBounds(x, y))
			{
				base.receiveRightClick(x, y, playSound: true);
				if (itemChangeBehavior == null && item == null && heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					heldItem = ItemsToGrabMenu.tryToAddItem(heldItem, "Ship");
				}
			}
			if (!ItemsToGrabMenu.isWithinBounds(x, y))
			{
				return;
			}
			heldItem = ItemsToGrabMenu.rightClick(x, y, heldItem, playSound: false);
			if ((heldItem != null && item == null) || (heldItem != null && item != null && !heldItem.Equals(item)) || (heldItem != null && item != null && heldItem.Equals(item) && heldItem.Stack != num))
			{
				if (itemChangeBehavior != null)
				{
					itemChangeBehavior(heldItem, ItemsToGrabMenu.getInventoryPositionOfClick(x, y), item, this, onRemoval: true);
				}
				Game1.playSound("dwop");
			}
			if ((heldItem == null && item != null) || (heldItem != null && item != null && !heldItem.Equals(item)))
			{
				if (itemChangeBehavior != null)
				{
					itemChangeBehavior(item, ItemsToGrabMenu.getInventoryPositionOfClick(x, y), heldItem, this);
				}
				Game1.playSound("Ship");
			}
			if (heldItem is Object && (bool)(heldItem as Object).isRecipe)
			{
				string key = heldItem.Name.Substring(0, heldItem.Name.IndexOf("Recipe") - 1);
				try
				{
					if ((heldItem as Object).Category == -7)
					{
						Game1.player.cookingRecipes.Add(key, 0);
					}
					else
					{
						Game1.player.craftingRecipes.Add(key, 0);
					}
					poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
					Game1.playSound("newRecipe");
				}
				catch (Exception)
				{
				}
				heldItem = null;
			}
			else if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && Game1.player.addItemToInventoryBool(heldItem))
			{
				heldItem = null;
				Game1.playSound("coin");
				if (itemChangeBehavior != null)
				{
					itemChangeBehavior(heldItem, ItemsToGrabMenu.getInventoryPositionOfClick(x, y), item, this, onRemoval: true);
				}
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (poof != null && poof.update(time))
			{
				poof = null;
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			ItemsToGrabMenu.hover(x, y, heldItem);
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.75f);
			IClickableMenu.drawTextureBox(b, topInv.X, topInv.Y, topInv.Width, topInv.Height, Color.White);
			IClickableMenu.drawTextureBox(b, bottomInv.X, bottomInv.Y, bottomInv.Width, bottomInv.Height, Color.White);
			base.draw(b, drawUpperPortion: false, drawDescriptionArea: false);
			ItemsToGrabMenu.draw(b);
			if (poof != null)
			{
				poof.draw(b, localPosition: true);
			}
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
			if (heldItem != null)
			{
				heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
			}
			drawMouse(b);
			if (ItemsToGrabMenu.descriptionTitle != null && ItemsToGrabMenu.descriptionTitle.Length > 1)
			{
				IClickableMenu.drawHoverText(b, ItemsToGrabMenu.descriptionTitle, Game1.smallFont, 32 + ((heldItem != null) ? 16 : (-21)), 32 + ((heldItem != null) ? 16 : (-21)));
			}
		}
	}
}
