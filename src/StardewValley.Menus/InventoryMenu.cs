using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	public class InventoryMenu : IClickableMenu
	{
		public delegate bool highlightThisItem(Item i);

		public enum BorderSide
		{
			Top,
			Left,
			Right,
			Bottom
		}

		public const int region_inventorySlot0 = 0;

		public const int region_inventorySlot1 = 1;

		public const int region_inventorySlot2 = 2;

		public const int region_inventorySlot3 = 3;

		public const int region_inventorySlot4 = 4;

		public const int region_inventorySlot5 = 5;

		public const int region_inventorySlot6 = 6;

		public const int region_inventorySlot7 = 7;

		public const int region_inventorySlot8 = 8;

		public const int region_inventorySlot9 = 9;

		public const int region_inventorySlot10 = 10;

		public const int region_inventorySlot11 = 11;

		public const int region_inventorySlot12 = 12;

		public const int region_inventorySlot13 = 13;

		public const int region_inventorySlot14 = 14;

		public const int region_inventorySlot15 = 15;

		public const int region_inventorySlot16 = 16;

		public const int region_inventorySlot17 = 17;

		public const int region_inventorySlot18 = 18;

		public const int region_inventorySlot19 = 19;

		public const int region_inventorySlot20 = 20;

		public const int region_inventorySlot21 = 21;

		public const int region_inventorySlot22 = 22;

		public const int region_inventorySlot23 = 23;

		public const int region_inventorySlot24 = 24;

		public const int region_inventorySlot25 = 25;

		public const int region_inventorySlot26 = 26;

		public const int region_inventorySlot27 = 27;

		public const int region_inventorySlot28 = 28;

		public const int region_inventorySlot29 = 29;

		public const int region_inventorySlot30 = 30;

		public const int region_inventorySlot31 = 31;

		public const int region_inventorySlot32 = 32;

		public const int region_inventorySlot33 = 33;

		public const int region_inventorySlot34 = 34;

		public const int region_inventorySlot35 = 35;

		public const int region_dropButton = 107;

		public const int region_inventoryArea = 9000;

		public string hoverText = "";

		public string hoverTitle = "";

		public string descriptionTitle = "";

		public string descriptionText = "";

		public List<ClickableComponent> inventory = new List<ClickableComponent>();

		protected Dictionary<int, double> _iconShakeTimer = new Dictionary<int, double>();

		public IList<Item> actualInventory;

		public highlightThisItem highlightMethod;

		public ItemGrabMenu.behaviorOnItemSelect onAddItem;

		public bool playerInventory;

		public bool drawSlots;

		public bool showGrayedOutSlots;

		public int capacity;

		public int rows;

		public int horizontalGap;

		public int verticalGap;

		public ClickableComponent dropItemInvisibleButton;

		public string moveItemSound = "dwop";

		private int hoverAmount;

		public bool canMoveItems = true;

		public IList<Item> otherInventoryForTrash;

		public int otherInventoryTrashItemIndex;

		public int otherInventoryTrashItemStack;

		public int xOffset;

		public int yOffset;

		public int squareSide = 80;

		private int infoWidth = 300;

		public int invOffset;

		public int hGap;

		public int additionalYOffset;

		private Rectangle fadeRect;

		public float scaleFactor;

		private float widthMod;

		private float heightMod;

		private float iconGapMultiplier;

		private float heldTimer;

		private float stackTimer;

		private float tapHoldTime = 0.5f;

		private float startStackTime = 1f;

		private bool showTrash;

		private bool holdingOrganizeButton;

		private bool holdingTrashCan;

		private bool externalHoldingTrashCan;

		public bool showItemInfo;

		public bool showOrganizeButton;

		public bool drawHeldItem;

		public ClickableTextureComponent trashCan;

		public ClickableTextureComponent organizeButton;

		public int currentlySelectedItem;

		public int currentlyStackingItem;

		public int inventoryItemHeld;

		private int infoPanelTextSize;

		private int infoPanelWidth;

		private int infoPanelHeight;

		private Vector2 infoPanelPosition;

		private Item actualItemSelected;

		private int itemsXoff;

		private int itemsYoff;

		public int furthestX;

		public int furthestY;

		private float trashCanLidRotation;

		private float dragScale;

		public int trashX;

		public int trashY;

		public int orgX;

		public int orgY;

		public int dragX;

		public int dragY;

		public int startDragX;

		public int startDragY;

		public int dragItem;

		public int currentlyHighlightedEmptySlot;

		public int deltaForDrag;

		public bool isOnMultiInventoryPage;

		public int currentlyHeldStack;

		private int _lineNumber;

		private float stackIncrementTime = 1f;

		private float oldStackTimer;

		private bool _showTooltip;

		private bool _movingItem;

		private Rectangle _infoPanelRect = new Rectangle(0, 0, 1, 1);

		public Item selectedItem
		{
			get
			{
				if (currentlySelectedItem == -1)
				{
					return null;
				}
				return actualInventory[currentlySelectedItem];
			}
		}

		public int totalItemSlots => Math.Min(Game1.player.maxItems, actualInventory.Count);

		public InventoryMenu(int xPosition, int yPosition, bool playerInventory, IList<Item> actualInventory = null, highlightThisItem highlightMethod = null, int capacity = -1, int rows = 3, int horizontalGap = 0, int verticalGap = 0, bool drawSlots = true, int width = 0, int height = 0, bool showTrash = true, bool showOrganizeButton = true, int addYOffset = 0, bool drawHeldItem = false, int xOff = -1, int yOff = -1, int forceSquareSide = -1)
			: base(xPosition, yPosition, width, height)
		{
			this.drawSlots = drawSlots;
			this.horizontalGap = horizontalGap;
			this.verticalGap = verticalGap;
			this.rows = rows;
			this.capacity = ((capacity == -1) ? 36 : capacity);
			this.playerInventory = playerInventory;
			this.actualInventory = actualInventory;
			if (actualInventory == null)
			{
				this.actualInventory = Game1.player.items;
			}
			currentlyHeldStack = -1;
			currentlyStackingItem = -1;
			dragItem = -1;
			currentlyHighlightedEmptySlot = -1;
			for (int i = 0; i < (int)Game1.player.maxItems; i++)
			{
				if (Game1.player.items.Count <= i)
				{
					Game1.player.items.Add(null);
				}
			}
			this.showTrash = showTrash;
			if (width == 0)
			{
				base.width = Game1.uiViewport.Width;
			}
			widthMod = (float)base.width / 1280f;
			heightMod = (float)base.height / 360f;
			deltaForDrag = 16;
			itemsYoff = (int)(11f * heightMod) + additionalYOffset;
			itemsXoff = (int)(11f * widthMod);
			if (forceSquareSide == -1)
			{
				squareSide = Math.Min((int)(90f * heightMod), (int)(90f * widthMod));
			}
			else
			{
				squareSide = forceSquareSide;
			}
			invOffset = (int)((float)squareSide * 0.15f);
			if (xOff == -1)
			{
				xOffset = Math.Min(base.width - squareSide * 3, squareSide / 2);
			}
			else
			{
				xOffset = xOff;
			}
			if (yOff == -1)
			{
				yOffset = (height - squareSide * 3) / 2;
			}
			else
			{
				yOffset = yOff;
			}
			fadeRect = new Rectangle(0, yPosition + 1, Game1.graphics.GraphicsDevice.Viewport.Bounds.Width, Game1.graphics.GraphicsDevice.Viewport.Bounds.Height - yPosition + 1);
			scaleFactor = (float)squareSide / 64f;
			currentlySelectedItem = -1;
			inventoryItemHeld = -1;
			actualItemSelected = null;
			this.showOrganizeButton = showOrganizeButton;
			int num = this.capacity / rows + ((this.capacity % rows > 0) ? 1 : 0);
			hGap = Math.Max(0, (int)(((float)base.width * 0.85f - (float)(num * squareSide)) / (float)(num - 1)));
			hGap = Math.Min(16, hGap);
			verticalGap = 8;
			furthestX = xPosition + xOffset + this.capacity / rows * (hGap + squareSide);
			furthestY = yPosition + yOffset + rows * (verticalGap + squareSide);
			for (int j = 0; j < this.capacity; j++)
			{
				int num2 = ((rows > 1) ? (j / num) : 0);
				int num3 = j - num2 * num;
				Vector2 vector = new Vector2(xPositionOnScreen + xOffset + num3 * (squareSide + hGap), yPositionOnScreen + yOffset + num2 * (squareSide + verticalGap));
				inventory.Add(new ClickableComponent(new Rectangle((int)vector.X, (int)vector.Y, squareSide + hGap, squareSide + verticalGap), j.ToString() ?? "")
				{
					myID = j,
					leftNeighborID = ((j % (this.capacity / rows) != 0) ? (j - 1) : (-1)),
					rightNeighborID = (((j + 1) % (this.capacity / rows) != 0) ? (j + 1) : ((j > this.capacity / rows) ? 105 : 106)),
					downNeighborID = ((j >= this.actualInventory.Count - this.capacity / rows) ? ((j % (this.capacity / rows) < 2) ? 101 : 102) : (j + this.capacity / rows)),
					upNeighborID = ((j < this.capacity / rows) ? (12340 + j) : (j - this.capacity / rows)),
					region = 9000,
					upNeighborImmutable = true,
					downNeighborImmutable = true,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
			}
			int num4 = xPosition + xOffset + 12 * squareSide + 11 * hGap;
			int num5 = yPosition + yOffset + 3 * squareSide;
			int num6 = 64;
			trashX = num4 + (width + xPosition - num4 - num6) / 2;
			trashY = num5 - 104;
			trashCan = new ClickableTextureComponent(new Rectangle(trashX, trashY, 64, 150), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f)
			{
				myID = 105,
				region = 105,
				leftNeighborID = Game1.player.MaxItems - 1,
				upNeighborID = 106
			};
			trashCan.drawShadow = true;
			int num7 = 64;
			orgY = yPosition + yOffset - ((height < 300) ? 16 : 0);
			orgX = num4 + (width + xPosition - num4 - num7) / 2;
			if (showOrganizeButton)
			{
				organizeButton = new ClickableTextureComponent(new Rectangle(orgX, orgY, 64, 64), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f)
				{
					myID = 106,
					region = 106,
					leftNeighborID = 9000,
					downNeighborID = 105
				};
				organizeButton.drawShadow = true;
			}
			showItemInfo = false;
			this.highlightMethod = highlightMethod;
			if (highlightMethod == null)
			{
				this.highlightMethod = highlightAllItems;
			}
			dropItemInvisibleButton = new ClickableComponent(new Rectangle(xPosition - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128, yPositionOnScreen - 12, 64, 64), "")
			{
				myID = (playerInventory ? 107 : (-500)),
				rightNeighborID = 0
			};
			Game1.options.snappyMenus = true;
			if (Game1.options.SnappyMenus)
			{
				currentlySnappedComponent = inventory[0];
				snapCursorToCurrentSnappedComponent();
				populateClickableComponentList();
			}
		}

		public List<ClickableComponent> GetBorder(BorderSide side)
		{
			List<ClickableComponent> list = new List<ClickableComponent>();
			int num = capacity / rows;
			switch (side)
			{
			case BorderSide.Bottom:
			{
				for (int l = 0; l < inventory.Count; l++)
				{
					if (l >= actualInventory.Count - num)
					{
						list.Add(inventory[l]);
					}
				}
				break;
			}
			case BorderSide.Top:
			{
				for (int j = 0; j < inventory.Count; j++)
				{
					if (j < num)
					{
						list.Add(inventory[j]);
					}
				}
				break;
			}
			case BorderSide.Left:
			{
				for (int k = 0; k < inventory.Count; k++)
				{
					if (k % num == 0)
					{
						list.Add(inventory[k]);
					}
				}
				break;
			}
			case BorderSide.Right:
			{
				for (int i = 0; i < inventory.Count; i++)
				{
					if (i % num == num - 1)
					{
						list.Add(inventory[i]);
					}
				}
				break;
			}
			}
			return list;
		}

		public static bool highlightAllItems(Item i)
		{
			return true;
		}

		public static bool highlightNoItems(Item i)
		{
			return false;
		}

		public void movePosition(int x, int y)
		{
			xPositionOnScreen += x;
			yPositionOnScreen += y;
			foreach (ClickableComponent item in inventory)
			{
				item.bounds.X += x;
				item.bounds.Y += y;
			}
			dropItemInvisibleButton.bounds.X += x;
			dropItemInvisibleButton.bounds.Y += y;
			if (organizeButton != null)
			{
				organizeButton.bounds.X += x;
				organizeButton.bounds.Y += y;
			}
			if (trashCan != null)
			{
				trashCan.bounds.X += x;
				trashCan.bounds.Y += y;
				trashX += x;
				trashY += y;
			}
		}

		public void ShakeItem(Item item)
		{
			ShakeItem(actualInventory.IndexOf(item));
		}

		public void ShakeItem(int index)
		{
			if (index >= 0 && index < inventory.Count)
			{
				_iconShakeTimer[index] = Game1.currentGameTime.TotalGameTime.TotalSeconds + 0.5;
			}
		}

		public Item tryToAddItem(Item toPlace, string sound = "coin", bool allowStack = true)
		{
			if (toPlace == null)
			{
				return null;
			}
			int stack = toPlace.Stack;
			foreach (ClickableComponent item in inventory)
			{
				int num = Convert.ToInt32(item.name);
				if (!allowStack || num >= actualInventory.Count || actualInventory[num] == null || !highlightMethod(actualInventory[num]) || !actualInventory[num].canStackWith(toPlace))
				{
					continue;
				}
				toPlace.Stack = actualInventory[num].addToStack(toPlace);
				if (toPlace.Stack > 0)
				{
					continue;
				}
				try
				{
					Game1.playSound(sound);
					if (onAddItem != null)
					{
						onAddItem(toPlace, playerInventory ? Game1.player : null);
					}
				}
				catch (Exception)
				{
				}
				return null;
			}
			foreach (ClickableComponent item2 in inventory)
			{
				int num2 = Convert.ToInt32(item2.name);
				if (num2 >= actualInventory.Count || (actualInventory[num2] != null && !highlightMethod(actualInventory[num2])) || actualInventory[num2] != null)
				{
					continue;
				}
				if (!string.IsNullOrEmpty(sound))
				{
					try
					{
						Game1.playSound(sound);
					}
					catch (Exception)
					{
					}
				}
				return Utility.addItemToInventory(toPlace, num2, actualInventory, onAddItem, allowStack);
			}
			if (toPlace.Stack < stack)
			{
				Game1.playSound(sound);
			}
			return toPlace;
		}

		public int getInventoryPositionOfClick(int x, int y)
		{
			for (int i = 0; i < inventory.Count; i++)
			{
				if (inventory[i] != null && inventory[i].bounds.Contains(x, y))
				{
					return Convert.ToInt32(inventory[i].name);
				}
			}
			return -1;
		}

		public Item leftClick(int x, int y, Item toPlace, bool playSound = true)
		{
			foreach (ClickableComponent item in inventory)
			{
				if (!item.containsPoint(x, y))
				{
					continue;
				}
				int num = Convert.ToInt32(item.name);
				if (num >= actualInventory.Count || (actualInventory[num] != null && !highlightMethod(actualInventory[num]) && !actualInventory[num].canStackWith(toPlace)))
				{
					continue;
				}
				if (actualInventory[num] != null)
				{
					if (toPlace != null)
					{
						if (playSound)
						{
							Game1.playSound("stoneStep");
						}
						return Utility.addItemToInventory(toPlace, num, actualInventory, onAddItem);
					}
					if (playSound)
					{
						Game1.playSound(moveItemSound);
					}
					return Utility.removeItemFromInventory(num, actualInventory);
				}
				if (toPlace != null)
				{
					if (playSound)
					{
						Game1.playSound("stoneStep");
					}
					return Utility.addItemToInventory(toPlace, num, actualInventory, onAddItem);
				}
			}
			return toPlace;
		}

		public Vector2 snapToClickableComponent(int x, int y)
		{
			foreach (ClickableComponent item in inventory)
			{
				if (item.containsPoint(x, y))
				{
					return new Vector2(item.bounds.X, item.bounds.Y);
				}
			}
			return new Vector2(x, y);
		}

		public Item getItemAt(int x, int y)
		{
			foreach (ClickableComponent item in inventory)
			{
				if (item.containsPoint(x, y))
				{
					return getItemFromClickableComponent(item);
				}
			}
			return null;
		}

		public Item getItemFromClickableComponent(ClickableComponent c)
		{
			if (c != null)
			{
				int num = Convert.ToInt32(c.name);
				if (num < actualInventory.Count)
				{
					return actualInventory[num];
				}
			}
			return null;
		}

		public Item rightClick(int x, int y, Item toAddTo, bool playSound = true, bool onlyCheckToolAttachments = false)
		{
			foreach (ClickableComponent item in inventory)
			{
				int num = Convert.ToInt32(item.name);
				if (!item.containsPoint(x, y) || num >= actualInventory.Count || (actualInventory[num] != null && !highlightMethod(actualInventory[num])) || num >= actualInventory.Count || actualInventory[num] == null)
				{
					continue;
				}
				if (actualInventory[num] is Tool && (toAddTo == null || toAddTo is Object) && (actualInventory[num] as Tool).canThisBeAttached((Object)toAddTo))
				{
					return (actualInventory[num] as Tool).attach((toAddTo == null) ? null : ((Object)toAddTo));
				}
				if (onlyCheckToolAttachments)
				{
					return toAddTo;
				}
				if (toAddTo == null)
				{
					if (actualInventory[num].maximumStackSize() != -1)
					{
						if (num == Game1.player.CurrentToolIndex && actualInventory[num] != null && actualInventory[num].Stack == 1)
						{
							actualInventory[num].actionWhenStopBeingHeld(Game1.player);
						}
						Item one = actualInventory[num].getOne();
						if (actualInventory[num].Stack > 1 && Game1.isOneOfTheseKeysDown(Game1.oldKBState, new InputButton[1]
						{
							new InputButton(Keys.LeftShift)
						}))
						{
							one.Stack = (int)Math.Ceiling((double)actualInventory[num].Stack / 2.0);
							actualInventory[num].Stack = actualInventory[num].Stack / 2;
						}
						else if (actualInventory[num].Stack == 1)
						{
							actualInventory[num] = null;
						}
						else
						{
							actualInventory[num].Stack--;
						}
						if (actualInventory[num] != null && actualInventory[num].Stack <= 0)
						{
							actualInventory[num] = null;
						}
						if (playSound)
						{
							Game1.playSound(moveItemSound);
						}
						return one;
					}
				}
				else
				{
					if (!actualInventory[num].canStackWith(toAddTo) || toAddTo.Stack >= toAddTo.maximumStackSize())
					{
						continue;
					}
					if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, new InputButton[1]
					{
						new InputButton(Keys.LeftShift)
					}))
					{
						toAddTo.Stack += (int)Math.Ceiling((double)actualInventory[num].Stack / 2.0);
						actualInventory[num].Stack = actualInventory[num].Stack / 2;
					}
					else
					{
						toAddTo.Stack++;
						actualInventory[num].Stack--;
					}
					if (playSound)
					{
						Game1.playSound(moveItemSound);
					}
					if (actualInventory[num].Stack <= 0)
					{
						if (num == Game1.player.CurrentToolIndex)
						{
							actualInventory[num].actionWhenStopBeingHeld(Game1.player);
						}
						actualInventory[num] = null;
					}
					return toAddTo;
				}
			}
			return toAddTo;
		}

		public Item hover(int x, int y, Item heldItem)
		{
			return null;
		}

		public override void setUpForGamePadMode()
		{
			if (!Game1.options.gamepadControls)
			{
				base.setUpForGamePadMode();
				if (inventory != null && inventory.Count > 0)
				{
					Game1.setMousePosition(inventory[0].bounds.Right - inventory[0].bounds.Width / 8, inventory[0].bounds.Bottom - inventory[0].bounds.Height / 8);
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			draw(b, -1, -1, -1);
		}

		public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
		{
			for (int i = 0; i < inventory.Count; i++)
			{
				if (_iconShakeTimer.ContainsKey(i) && Game1.currentGameTime.TotalGameTime.TotalSeconds >= _iconShakeTimer[i])
				{
					_iconShakeTimer.Remove(i);
				}
			}
			if (showTrash)
			{
				trashCan.draw(b);
				b.Draw(Game1.mouseCursors, new Vector2(trashCan.bounds.X + 60, trashCan.bounds.Y + 40), new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10), Color.White, trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.086f);
			}
			if (showOrganizeButton)
			{
				organizeButton.draw(b);
			}
			Color color = ((red == -1) ? Color.White : new Color((int)Utility.Lerp(red, Math.Min(255, red + 150), 0.65f), (int)Utility.Lerp(green, Math.Min(255, green + 150), 0.65f), (int)Utility.Lerp(blue, Math.Min(255, blue + 150), 0.65f)));
			Texture2D texture = ((red == -1) ? Game1.menuTexture : Game1.uncoloredMenuTexture);
			if (drawSlots)
			{
				for (int j = 0; j < capacity; j++)
				{
					Vector2 position = new Vector2(inventory[j].bounds.X, inventory[j].bounds.Y);
					b.Draw(texture, position, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, (currentlySelectedItem == j) ? 56 : 10), color, 0f, Vector2.Zero, scaleFactor, SpriteEffects.None, 0.05f);
					if (j == currentlyHighlightedEmptySlot)
					{
						b.Draw(Game1.mouseCursors, position, new Rectangle(194, 388, 16, 16), Color.White, 0f, Vector2.Zero, scaleFactor * 4f, SpriteEffects.None, 0.05f);
					}
					if ((playerInventory || showGrayedOutSlots) && j >= (int)Game1.player.maxItems)
					{
						b.Draw(texture, position, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 57), color * 0.5f, 0f, Vector2.Zero, scaleFactor, SpriteEffects.None, 0.05f);
					}
					if (j < 12)
					{
						_ = playerInventory;
					}
					position.X += invOffset;
					position.Y += invOffset;
				}
			}
			for (int k = 0; k < capacity; k++)
			{
				if (actualInventory.Count > k && actualInventory.ElementAt(k) != null)
				{
					Vector2 location = new Vector2(inventory[k].bounds.X, inventory[k].bounds.Y);
					if (_iconShakeTimer.ContainsKey(k))
					{
						location += 1f * new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
					}
					if (k != dragItem)
					{
						float scaleSize = (float)squareSide / (float)actualInventory[k].itemSlotSize * 3f / 4f;
						actualInventory[k].itemSlotSize = squareSide;
						actualInventory[k].drawInMenu(b, location, scaleSize, (!highlightMethod(actualInventory[k])) ? 0.2f : 1f, 0.0865f, StackDrawType.Draw);
						actualInventory[k].itemSlotSize = 64;
					}
					else if (k == dragItem && currentlyHeldStack != -1)
					{
						Item one = actualInventory[k].getOne();
						one.Stack = actualInventory[k].Stack - currentlyHeldStack;
						float scaleSize2 = (float)squareSide / (float)actualInventory[k].itemSlotSize * 3f / 4f;
						one.itemSlotSize = squareSide;
						one.drawInMenu(b, location, scaleSize2, (!highlightMethod(actualInventory[k])) ? 0.2f : 1f, 0.0865f, StackDrawType.Draw);
						actualInventory[k].itemSlotSize = 64;
					}
				}
			}
			if (showItemInfo)
			{
				drawInfoPanel(b);
			}
			if (hoverText != null && !hoverText.Equals("") && hoverAmount > 0)
			{
				drawMobileFloatingToolTip(b, Game1.uiViewport.Width / 2, trashCan.bounds.Y - 64, 6, squareSide, hoverText, hoverTitle, null, heldItem: true, -1, 0, -1, -1, null, hoverAmount);
			}
		}

		public List<Vector2> GetSlotDrawPositions()
		{
			List<Vector2> list = new List<Vector2>();
			for (int i = 0; i < capacity; i++)
			{
				list.Add(getColorPositionOfItem(i));
			}
			return list;
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			try
			{
				_lineNumber = 1;
				otherInventoryTrashItemIndex = -1;
				otherInventoryForTrash = null;
				externalHoldingTrashCan = false;
				_lineNumber = 2;
				int num = (playerInventory ? ((int)Game1.player.maxItems) : inventory.Count);
				_lineNumber = 3;
				for (int i = 0; i < num; i++)
				{
					_lineNumber = 4;
					if (inventory[i] == null)
					{
						continue;
					}
					_lineNumber = 5;
					if (!inventory[i].bounds.Contains(x, y) || i >= actualInventory.Count() || (actualInventory[i] != null && !highlightMethod(actualInventory[i])))
					{
						continue;
					}
					_lineNumber = 6;
					startDragX = x;
					startDragY = y;
					_lineNumber = 7;
					if (currentlySelectedItem != -1 && i == currentlySelectedItem && currentlyHeldStack != -1)
					{
						_lineNumber = 8;
						inventoryItemHeld = i;
						heldTimer = 0f;
						stackTimer = 0f;
						oldStackTimer = 0f;
						stackIncrementTime = 0.4f;
						return;
					}
					_lineNumber = 9;
					if (currentlySelectedItem != -1 && getItemFromClickableComponent(inventory[i]) == null && i != currentlySelectedItem)
					{
						_lineNumber = 10;
						Item itemFromClickableComponent = getItemFromClickableComponent(inventory[currentlySelectedItem]);
						_lineNumber = 11;
						if (currentlyHeldStack != -1)
						{
							_lineNumber = 12;
							Item one = itemFromClickableComponent.getOne();
							_lineNumber = 13;
							one.Stack = currentlyHeldStack;
							_lineNumber = 14;
							Utility.addItemToInventory(one, i, actualInventory);
							_lineNumber = 15;
							itemFromClickableComponent.Stack -= currentlyHeldStack;
							_lineNumber = 16;
							if (itemFromClickableComponent.Stack <= 0)
							{
								_lineNumber = 17;
								Utility.removeItemFromInventory(currentlySelectedItem, actualInventory);
							}
							_lineNumber = 18;
							currentlyHeldStack = -1;
							currentlySelectedItem = -1;
							dragItem = -1;
							currentlyHighlightedEmptySlot = -1;
							currentlyStackingItem = -1;
							inventoryItemHeld = -1;
							Game1.playSound("smallSelect");
							_lineNumber = 19;
						}
						else
						{
							_lineNumber = 20;
							if (currentlySelectedItem != i && itemFromClickableComponent != null && itemFromClickableComponent.Stack > 1 && dragItem == -1)
							{
								_lineNumber = 21;
								dragX = x;
								dragY = y;
								currentlyStackingItem = i;
								_lineNumber = 22;
								startDragX = inventory[currentlyStackingItem].bounds.X;
								startDragY = inventory[currentlyStackingItem].bounds.Y;
								_lineNumber = 23;
							}
							else
							{
								_lineNumber = 24;
								Utility.removeItemFromInventory(currentlySelectedItem, actualInventory);
								_lineNumber = 25;
								Utility.addItemToInventory(itemFromClickableComponent, i, actualInventory);
								_lineNumber = 26;
								dragItem = -1;
								currentlySelectedItem = -1;
								currentlyHighlightedEmptySlot = -1;
								currentlyStackingItem = -1;
								inventoryItemHeld = -1;
								Game1.playSound("smallSelect");
								_lineNumber = 27;
							}
						}
						return;
					}
					_lineNumber = 28;
					actualItemSelected = getItemFromClickableComponent(inventory[i]);
					_lineNumber = 29;
					if (actualItemSelected == null)
					{
						continue;
					}
					_lineNumber = 30;
					if (currentlyHeldStack != -1 && currentlySelectedItem > -1 && currentlySelectedItem < actualInventory.Count && currentlySelectedItem < inventory.Count && actualInventory[currentlySelectedItem].canStackWith(actualItemSelected))
					{
						_lineNumber = 31;
						Item one2 = getItemFromClickableComponent(inventory[currentlySelectedItem]).getOne();
						_lineNumber = 32;
						Item itemFromClickableComponent2 = getItemFromClickableComponent(inventory[currentlySelectedItem]);
						_lineNumber = 33;
						one2.Stack = currentlyHeldStack;
						_lineNumber = 34;
						Item item;
						if (currentlyHighlightedEmptySlot != -1)
						{
							_lineNumber = 35;
							item = tryToAddItemToSlotNumber(one2, currentlyHighlightedEmptySlot);
							_lineNumber = 36;
						}
						else
						{
							_lineNumber = 37;
							item = tryToAddItemAt(one2, x, y);
							_lineNumber = 38;
						}
						_lineNumber = 39;
						if (item == null)
						{
							_lineNumber = 40;
							itemFromClickableComponent2.Stack -= currentlyHeldStack;
							_lineNumber = 41;
							if (itemFromClickableComponent2.Stack <= 0)
							{
								_lineNumber = 42;
								Utility.removeItemFromInventory(currentlySelectedItem, actualInventory);
								_lineNumber = 43;
							}
							_lineNumber = 44;
						}
						else
						{
							_lineNumber = 45;
							itemFromClickableComponent2.Stack = itemFromClickableComponent2.Stack - currentlyHeldStack + item.Stack;
							_lineNumber = 46;
						}
						_lineNumber = 47;
						currentlyHeldStack = -1;
						currentlySelectedItem = -1;
						dragItem = -1;
						inventoryItemHeld = -1;
						Game1.playSound("coin");
						_lineNumber = 48;
						return;
					}
					_lineNumber = 49;
					startDragX = x;
					startDragY = y;
					currentlyHeldStack = -1;
					currentlyStackingItem = -1;
					Game1.playSound("smallSelect");
					inventoryItemHeld = i;
					showItemInfo = false;
					heldTimer = 0f;
					stackTimer = 0f;
					_lineNumber = 50;
					infoPanelPosition = getPositionOfSellPanel(x, y);
					_lineNumber = 51;
					if (i < actualInventory.Count())
					{
						_lineNumber = 52;
						string hoverBoxText = actualInventory[i].getHoverBoxText(actualItemSelected);
						_lineNumber = 53;
						if (hoverBoxText != null)
						{
							_lineNumber = 54;
							hoverText = hoverBoxText;
							continue;
						}
						_lineNumber = 55;
						hoverText = actualInventory[i].getDescription();
						_lineNumber = 56;
						hoverTitle = actualInventory[i].DisplayName;
						_lineNumber = 57;
					}
				}
				_lineNumber = 58;
				if (showOrganizeButton && organizeButton != null && organizeButton.containsPoint(x, y))
				{
					_lineNumber = 59;
					holdingOrganizeButton = true;
					organizeButton.drawShadow = false;
					Game1.playSound("smallSelect");
					organizeButton.bounds.X -= 4;
					organizeButton.bounds.Y += 4;
					currentlySelectedItem = -1;
					_lineNumber = 60;
				}
				_lineNumber = 61;
				if (currentlySelectedItem != -1 && showTrash && trashCan != null && trashCan.containsPoint(x, y))
				{
					_lineNumber = 62;
					holdingTrashCan = true;
					trashCan.drawShadow = false;
					Game1.playSound("smallSelect");
					trashCan.bounds.X = trashX - 4;
					trashCan.bounds.Y = trashY + 4;
					_lineNumber = 63;
				}
				_lineNumber = 64;
			}
			catch (Exception)
			{
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			hoverAmount = -1;
			if (!holdingTrashCan && trashCanLidRotation != 0f)
			{
				trashCanLidRotation = Math.Max(trashCanLidRotation - (float)Math.PI / 24f, 0f);
				if (trashCanLidRotation == 0f)
				{
					Game1.playSound("thudStep");
				}
			}
		}

		public int getInvWidth()
		{
			int num = 0;
			for (int i = 0; i < inventory.Count; i++)
			{
				int num2 = inventory[i].bounds.X + inventory[i].bounds.Width - inventory.First().bounds.X;
				if (num2 > num)
				{
					num = num2;
				}
			}
			return num;
		}

		public int getInvX()
		{
			return inventory.First().bounds.X;
		}

		public int getInvHeight()
		{
			return inventory.Last().bounds.Y + inventory.Last().bounds.Height - inventory.First().bounds.Y;
		}

		public int getInvY()
		{
			return inventory.First().bounds.Y;
		}

		public void drawInfoPanel(SpriteBatch b, bool force = false)
		{
			if (showItemInfo && (force || !isOnMultiInventoryPage) && actualItemSelected != null)
			{
				if (rows > 3)
				{
					drawMobileFloatingToolTip(b, Game1.uiViewport.Width / 2, (int)infoPanelPosition.Y + itemsYoff * 2, 6, squareSide, hoverText, hoverTitle, actualItemSelected, heldItem: false, -1, 0, -1, -1, null, -1, currentlyHeldStack);
				}
				else
				{
					drawMobileFloatingToolTip(b, (int)infoPanelPosition.X + itemsXoff * 2, (int)infoPanelPosition.Y + itemsYoff * 2, currentlySelectedItem, squareSide, hoverText, hoverTitle, actualItemSelected, heldItem: false, -1, 0, -1, -1, null, -1, currentlyHeldStack);
				}
			}
		}

		public void drawDragItem(SpriteBatch b)
		{
			if (dragItem <= -1)
			{
				return;
			}
			Vector2 location = new Vector2(dragX, (float)dragY - dragScale * 64f);
			if (actualInventory.Count <= dragItem || actualInventory[dragItem] == null)
			{
				return;
			}
			if (currentlyHeldStack != -1)
			{
				Item one = actualInventory[dragItem].getOne();
				if (one != null)
				{
					one.Stack = currentlyHeldStack;
					one.drawInMenu(b, location, dragScale, 0.5f, 0.0865f);
				}
			}
			else
			{
				actualInventory[dragItem].drawInMenu(b, location, dragScale, 0.5f, 0.0865f);
			}
		}

		public Item addItemTo(int existingItemSlotNumber, Item itemToAdd)
		{
			if (actualInventory[existingItemSlotNumber].Name.Equals(itemToAdd.Name))
			{
				if (actualInventory[existingItemSlotNumber].canStackWith(itemToAdd) && actualInventory[existingItemSlotNumber].Stack < itemToAdd.maximumStackSize())
				{
					int num = Math.Min(itemToAdd.Stack, itemToAdd.maximumStackSize() - actualInventory[existingItemSlotNumber].Stack);
					itemToAdd.Stack -= num;
					actualInventory[existingItemSlotNumber].Stack += num;
					if (itemToAdd.Stack <= 0)
					{
						itemToAdd = null;
					}
				}
				return itemToAdd;
			}
			if (actualInventory[existingItemSlotNumber] is Tool && (itemToAdd == null || itemToAdd is Object) && (actualInventory[existingItemSlotNumber] as Tool).canThisBeAttached((Object)itemToAdd))
			{
				return (actualInventory[existingItemSlotNumber] as Tool).attach((itemToAdd == null) ? null : ((Object)itemToAdd));
			}
			if (actualInventory[existingItemSlotNumber].canStackWith(itemToAdd) && itemToAdd.Stack < itemToAdd.maximumStackSize())
			{
				itemToAdd.Stack++;
				actualInventory[existingItemSlotNumber].Stack--;
				Game1.playSound("dwop");
				if (actualInventory[existingItemSlotNumber].Stack <= 0)
				{
					if (existingItemSlotNumber == Game1.player.CurrentToolIndex)
					{
						actualInventory[existingItemSlotNumber].actionWhenStopBeingHeld(Game1.player);
					}
					actualInventory[existingItemSlotNumber] = null;
				}
				return itemToAdd;
			}
			return null;
		}

		public bool addItemAt(Item item, int x, int y, bool allowStack = true)
		{
			Item item2 = null;
			for (int i = 0; i < (playerInventory ? ((int)Game1.player.maxItems) : capacity); i++)
			{
				if (inventory[i] != null && inventory[i].bounds.Contains(x, y) && (getItemFromClickableComponent(inventory[i]) == null || (allowStack && getItemFromClickableComponent(inventory[i]).canStackWith(item))))
				{
					bool hasBeenInInventory = item.HasBeenInInventory;
					item2 = Utility.addItemToInventory(item, i, actualInventory, onAddItem, allowStack);
					bool hasBeenInInventory2 = item.HasBeenInInventory;
					if (playerInventory)
					{
						item.HasBeenInInventory = hasBeenInInventory;
						Game1.player.fakeAddItemToInventoryBool(item);
						item.HasBeenInInventory = hasBeenInInventory2;
					}
					return true;
				}
			}
			return false;
		}

		public Item tryToAddItemAt(Item item, int x, int y, bool allowStack = true)
		{
			Item item2 = null;
			for (int i = 0; i < (playerInventory ? ((int)Game1.player.maxItems) : capacity); i++)
			{
				if (inventory[i] == null || !inventory[i].bounds.Contains(x, y) || (getItemFromClickableComponent(inventory[i]) != null && (!allowStack || !getItemFromClickableComponent(inventory[i]).canStackWith(item))))
				{
					continue;
				}
				bool hasBeenInInventory = item.HasBeenInInventory;
				bool hasBeenPickedUpByFarmer = item is Object && (bool)(item as Object).hasBeenPickedUpByFarmer;
				item2 = Utility.addItemToInventory(item, i, actualInventory, onAddItem, allowStack);
				bool hasBeenPickedUpByFarmer2 = item is Object && (bool)(item as Object).hasBeenPickedUpByFarmer;
				bool hasBeenInInventory2 = item.HasBeenInInventory;
				if (playerInventory)
				{
					item.HasBeenInInventory = hasBeenInInventory;
					if (item is Object)
					{
						(item as Object).HasBeenPickedUpByFarmer = hasBeenPickedUpByFarmer;
					}
					Game1.player.fakeAddItemToInventoryBool(item);
					item.HasBeenInInventory = hasBeenInInventory2;
					if (item is Object)
					{
						(item as Object).HasBeenPickedUpByFarmer = hasBeenPickedUpByFarmer2;
					}
				}
				return item2;
			}
			return item;
		}

		public Item tryToAddItemToSlotNumber(Item item, int slotNumber, bool allowStack = true)
		{
			Item item2 = null;
			if (inventory[slotNumber] != null && (getItemFromClickableComponent(inventory[slotNumber]) == null || (allowStack && getItemFromClickableComponent(inventory[slotNumber]).canStackWith(item))))
			{
				bool hasBeenInInventory = item.HasBeenInInventory;
				bool hasBeenPickedUpByFarmer = item is Object && (bool)(item as Object).hasBeenPickedUpByFarmer;
				item2 = Utility.addItemToInventory(item, slotNumber, actualInventory, onAddItem, allowStack);
				bool hasBeenInInventory2 = item.HasBeenInInventory;
				bool hasBeenPickedUpByFarmer2 = item is Object && (bool)(item as Object).hasBeenPickedUpByFarmer;
				if (playerInventory)
				{
					item.HasBeenInInventory = hasBeenInInventory;
					if (item is Object)
					{
						(item as Object).HasBeenPickedUpByFarmer = hasBeenPickedUpByFarmer;
					}
					Game1.player.fakeAddItemToInventoryBool(item);
					item.HasBeenInInventory = hasBeenInInventory2;
					if (item is Object)
					{
						(item as Object).HasBeenPickedUpByFarmer = hasBeenPickedUpByFarmer2;
					}
				}
				return item2;
			}
			return item;
		}

		public override void releaseLeftClick(int x, int y)
		{
			try
			{
				_lineNumber = 0;
				if (!_showTooltip)
				{
					hoverText = "";
				}
				hoverAmount = -1;
				inventoryItemHeld = -1;
				_lineNumber = 1;
				if (externalHoldingTrashCan && trashCan.containsPoint(x, y))
				{
					_lineNumber = 2;
					if (otherInventoryForTrash != null && otherInventoryTrashItemIndex != -1)
					{
						_lineNumber = 3;
						if (otherInventoryForTrash[otherInventoryTrashItemIndex].canBeTrashed())
						{
							if (otherInventoryTrashItemStack == -1 || otherInventoryTrashItemStack >= otherInventoryForTrash[otherInventoryTrashItemIndex].Stack)
							{
								_lineNumber = 4;
								Utility.trashItem(otherInventoryForTrash[otherInventoryTrashItemIndex]);
								_lineNumber = 5;
								Utility.removeItemFromInventory(otherInventoryTrashItemIndex, otherInventoryForTrash);
								_lineNumber = 6;
							}
							else
							{
								_lineNumber = 7;
								Utility.trashItem(otherInventoryForTrash[otherInventoryTrashItemIndex], otherInventoryTrashItemStack);
								_lineNumber = 8;
								otherInventoryForTrash[otherInventoryTrashItemIndex].Stack -= otherInventoryTrashItemStack;
								_lineNumber = 9;
							}
							_lineNumber = 10;
							otherInventoryTrashItemIndex = -1;
							currentlyHeldStack = -1;
							currentlyHighlightedEmptySlot = -1;
							currentlySelectedItem = -1;
							otherInventoryForTrash = null;
							externalHoldingTrashCan = false;
							holdingTrashCan = false;
							_lineNumber = 11;
							return;
						}
					}
					_lineNumber = 12;
				}
				_lineNumber = 13;
				if (currentlyStackingItem != -1)
				{
					_lineNumber = 14;
					Item itemFromClickableComponent = getItemFromClickableComponent(inventory[currentlySelectedItem]);
					_lineNumber = 15;
					if (itemFromClickableComponent != null)
					{
						_lineNumber = 16;
						if (itemFromClickableComponent.Stack == 0)
						{
							_lineNumber = 17;
							Utility.removeItemFromInventory(currentlySelectedItem, actualInventory);
							_lineNumber = 18;
						}
						_lineNumber = 19;
						Item itemFromClickableComponent2 = getItemFromClickableComponent(inventory[currentlyStackingItem]);
						_lineNumber = 20;
						if (itemFromClickableComponent2 != null)
						{
							_lineNumber = 21;
							if (itemFromClickableComponent2.Stack == 0)
							{
								_lineNumber = 22;
								Utility.removeItemFromInventory(currentlyStackingItem, actualInventory);
								_lineNumber = 23;
							}
							_lineNumber = 24;
							currentlySelectedItem = -1;
							currentlyStackingItem = -1;
							currentlyHighlightedEmptySlot = -1;
							dragItem = -1;
							_lineNumber = 25;
						}
						else
						{
							_lineNumber = 26;
							Utility.removeItemFromInventory(currentlySelectedItem, actualInventory);
							_lineNumber = 27;
							Utility.addItemToInventory(itemFromClickableComponent, currentlyStackingItem, actualInventory);
							_lineNumber = 28;
							dragItem = -1;
							currentlySelectedItem = -1;
							currentlyHighlightedEmptySlot = -1;
							currentlyStackingItem = -1;
							inventoryItemHeld = -1;
							Game1.playSound("smallSelect");
							_lineNumber = 29;
						}
						return;
					}
				}
				_lineNumber = 31;
				currentlyStackingItem = -1;
				if (isWithinBounds(x, y))
				{
					_lineNumber = 32;
					base.releaseLeftClick(x, y);
					_lineNumber = 33;
					showItemInfo = false;
					if (dragItem != -1)
					{
						_lineNumber = 34;
						if (dragItem == currentlyHighlightedEmptySlot)
						{
							_lineNumber = 35;
							Game1.playSound("shwip");
							currentlySelectedItem = -1;
							dragItem = -1;
							currentlyHighlightedEmptySlot = -1;
							inventoryItemHeld = -1;
							currentlyHeldStack = -1;
							_lineNumber = 36;
							return;
						}
						_lineNumber = 37;
						Item itemAt = getItemAt(x, y);
						_lineNumber = 38;
						if (currentlyHighlightedEmptySlot != -1)
						{
							_lineNumber = 39;
							Item itemFromClickableComponent3 = getItemFromClickableComponent(inventory[currentlyHighlightedEmptySlot]);
							_lineNumber = 40;
							if (itemFromClickableComponent3 == null)
							{
								_lineNumber = 41;
								Item itemFromClickableComponent4 = getItemFromClickableComponent(inventory[dragItem]);
								_lineNumber = 42;
								if (currentlyHeldStack != -1)
								{
									_lineNumber = 43;
									Item one = itemFromClickableComponent4.getOne();
									_lineNumber = 44;
									one.Stack = currentlyHeldStack;
									Utility.addItemToInventory(one, currentlyHighlightedEmptySlot, actualInventory);
									_lineNumber = 45;
									itemFromClickableComponent4.Stack -= currentlyHeldStack;
									if (itemFromClickableComponent4.Stack <= 0)
									{
										_lineNumber = 46;
										Utility.removeItemFromInventory(currentlySelectedItem, actualInventory);
										_lineNumber = 47;
									}
									_lineNumber = 48;
									currentlyHeldStack = -1;
									currentlySelectedItem = -1;
									dragItem = -1;
									currentlyHighlightedEmptySlot = -1;
									currentlyStackingItem = -1;
									inventoryItemHeld = -1;
									Game1.playSound("smallSelect");
									_lineNumber = 49;
								}
								else
								{
									_lineNumber = 50;
									Utility.removeItemFromInventory(dragItem, actualInventory);
									_lineNumber = 51;
									Utility.addItemToInventory(itemFromClickableComponent4, currentlyHighlightedEmptySlot, actualInventory);
									_lineNumber = 52;
									Game1.playSound("shwip");
									currentlySelectedItem = -1;
									dragItem = -1;
									currentlyHighlightedEmptySlot = -1;
									inventoryItemHeld = -1;
									currentlyHeldStack = -1;
									_lineNumber = 53;
								}
								return;
							}
							_lineNumber = 54;
							if (currentlyHeldStack == -1)
							{
								_lineNumber = 55;
								Item itemFromClickableComponent5 = getItemFromClickableComponent(inventory[dragItem]);
								_lineNumber = 56;
								Item item = addItemTo(currentlyHighlightedEmptySlot, itemFromClickableComponent5);
								_lineNumber = 57;
								Utility.removeItemFromInventory(dragItem, actualInventory);
								_lineNumber = 58;
								if (item != null)
								{
									_lineNumber = 59;
									Utility.addItemToInventory(item, dragItem, actualInventory);
									_lineNumber = 60;
								}
								else
								{
									_lineNumber = 61;
									actualInventory[dragItem] = null;
									_lineNumber = 62;
								}
								_lineNumber = 63;
								Game1.playSound("shwip");
								currentlySelectedItem = -1;
								dragItem = -1;
								currentlyHighlightedEmptySlot = -1;
								inventoryItemHeld = -1;
								_lineNumber = 64;
								return;
							}
							_lineNumber = 65;
							Item itemFromClickableComponent6 = getItemFromClickableComponent(inventory[dragItem]);
							_lineNumber = 66;
							Item one2 = itemFromClickableComponent6.getOne();
							_lineNumber = 67;
							one2.Stack = currentlyHeldStack;
							itemFromClickableComponent6.Stack -= currentlyHeldStack;
							_lineNumber = 68;
							Item item2 = addItemTo(currentlyHighlightedEmptySlot, one2);
							_lineNumber = 69;
							if (item2 != null)
							{
								_lineNumber = 70;
								Utility.addItemToInventory(item2, dragItem, actualInventory);
								_lineNumber = 71;
							}
							else
							{
								_lineNumber = 72;
								if (itemFromClickableComponent6.Stack <= 0)
								{
									_lineNumber = 73;
									Utility.removeItemFromInventory(dragItem, actualInventory);
									_lineNumber = 74;
								}
							}
							_lineNumber = 75;
							Game1.playSound("shwip");
							currentlySelectedItem = -1;
							dragItem = -1;
							currentlyHighlightedEmptySlot = -1;
							inventoryItemHeld = -1;
							_lineNumber = 76;
							return;
						}
						if (itemAt != null)
						{
							_lineNumber = 77;
							int i;
							for (i = 0; i < inventory.Count; i++)
							{
								_lineNumber = 78;
								if (inventory[i].containsPoint(x, y))
								{
									break;
								}
							}
							_lineNumber = 79;
							Item itemFromClickableComponent7 = getItemFromClickableComponent(inventory[dragItem]);
							_lineNumber = 80;
							actualInventory[dragItem] = itemAt;
							_lineNumber = 81;
							actualInventory[i] = itemFromClickableComponent7;
							_lineNumber = 82;
							Game1.playSound("shwip");
							_lineNumber = 83;
						}
						else if (trashCan.containsPoint(x, y))
						{
							_lineNumber = 84;
							Item itemFromClickableComponent8 = getItemFromClickableComponent(inventory[dragItem]);
							_lineNumber = 85;
							holdingTrashCan = false;
							trashCan.drawShadow = true;
							Game1.playSound("smallSelect");
							trashCan.bounds.X = trashX;
							trashCan.bounds.Y = trashY;
							_lineNumber = 86;
							if (itemFromClickableComponent8 != null && itemFromClickableComponent8.canBeTrashed())
							{
								_lineNumber = 87;
								if (itemFromClickableComponent8 is Object && Game1.player.specialItems.Contains((itemFromClickableComponent8 as Object).parentSheetIndex))
								{
									_lineNumber = 88;
									Game1.player.specialItems.Remove((itemFromClickableComponent8 as Object).parentSheetIndex);
									_lineNumber = 89;
								}
								_lineNumber = 90;
								if (currentlyHeldStack != -1)
								{
									_lineNumber = 91;
									itemFromClickableComponent8.Stack -= currentlyHeldStack;
									Utility.trashItem(itemFromClickableComponent8, currentlyHeldStack);
									_lineNumber = 92;
									if (itemFromClickableComponent8.Stack <= 0)
									{
										_lineNumber = 93;
										Utility.removeItemFromInventory(currentlySelectedItem, actualInventory);
										_lineNumber = 94;
									}
									_lineNumber = 95;
								}
								else
								{
									_lineNumber = 96;
									Utility.trashItem(itemFromClickableComponent8);
									_lineNumber = 97;
									Utility.removeItemFromInventory(currentlySelectedItem, actualInventory);
									_lineNumber = 98;
								}
								_lineNumber = 99;
								currentlyHeldStack = -1;
								currentlySelectedItem = -1;
								holdingTrashCan = false;
								currentlySelectedItem = -1;
								dragItem = -1;
								inventoryItemHeld = -1;
								_lineNumber = 100;
								return;
							}
						}
						_lineNumber = 101;
						dragItem = -1;
						inventoryItemHeld = -1;
						currentlySelectedItem = -1;
						showItemInfo = false;
						heldTimer = 0f;
						currentlyHighlightedEmptySlot = -1;
						_lineNumber = 102;
						return;
					}
					_lineNumber = 103;
					if (organizeButton != null && holdingOrganizeButton && organizeButton.containsPoint(x, y))
					{
						_lineNumber = 104;
						holdingOrganizeButton = false;
						organizeButton.drawShadow = true;
						_lineNumber = 105;
						ItemGrabMenu.organizeItemsInList(actualInventory);
						_lineNumber = 106;
						Game1.playSound("smallSelect");
						organizeButton.bounds.X = orgX;
						organizeButton.bounds.Y = orgY;
						_lineNumber = 107;
					}
					_lineNumber = 108;
					if (showTrash && trashCan != null && trashCan.containsPoint(x, y))
					{
						_lineNumber = 109;
						if (currentlySelectedItem > -1)
						{
							_lineNumber = 110;
							Item itemFromClickableComponent9 = getItemFromClickableComponent(inventory[currentlySelectedItem]);
							_lineNumber = 111;
							holdingTrashCan = false;
							trashCan.drawShadow = true;
							Game1.playSound("smallSelect");
							trashCan.bounds.X = trashX;
							trashCan.bounds.Y = trashY;
							_lineNumber = 112;
							if (itemFromClickableComponent9 != null && itemFromClickableComponent9.canBeTrashed())
							{
								_lineNumber = 113;
								if (itemFromClickableComponent9 is Object && Game1.player.specialItems.Contains((itemFromClickableComponent9 as Object).parentSheetIndex))
								{
									_lineNumber = 114;
									Game1.player.specialItems.Remove((itemFromClickableComponent9 as Object).parentSheetIndex);
									_lineNumber = 115;
								}
								_lineNumber = 116;
								Utility.trashItem(itemFromClickableComponent9);
								_lineNumber = 117;
								Utility.removeItemFromInventory(currentlySelectedItem, actualInventory);
								_lineNumber = 118;
								currentlySelectedItem = -1;
								holdingTrashCan = false;
								_lineNumber = 119;
							}
						}
					}
					inventoryItemHeld = -1;
				}
				_lineNumber = 120;
				dragItem = -1;
				currentlyHighlightedEmptySlot = -1;
				_lineNumber = 121;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public void doOpenTrashCan(IList<Item> inventoryItemIsFrom, int itemIndex, int theStack = -1)
		{
			otherInventoryForTrash = inventoryItemIsFrom;
			otherInventoryTrashItemIndex = itemIndex;
			otherInventoryTrashItemStack = theStack;
			holdingTrashCan = true;
			externalHoldingTrashCan = true;
		}

		public override void update(GameTime time)
		{
			if (!holdingTrashCan && trashCanLidRotation != 0f)
			{
				trashCanLidRotation = Math.Max(trashCanLidRotation - (float)Math.PI / 24f, 0f);
				if (trashCanLidRotation == 0f)
				{
					Game1.playSound("thudStep");
				}
			}
			if (currentlySelectedItem > -1 && actualInventory.Count > currentlySelectedItem && actualInventory[currentlySelectedItem] != null && actualInventory[currentlySelectedItem] is Object && (Game1.getLocationFromName("CommunityCenter") as CommunityCenter).couldThisIngredienteBeUsedInABundle(actualInventory[currentlySelectedItem] as Object))
			{
				GameMenu.bundleItemHovered = true;
			}
			else
			{
				GameMenu.bundleItemHovered = false;
			}
		}

		public Vector2 getPositionOfSellPanel(int x, int y)
		{
			for (int i = 0; i < inventory.Count; i++)
			{
				if (i % 12 <= 5)
				{
					if (inventory[i] != null && inventory[i].bounds.Contains(x, y))
					{
						return new Vector2(inventory[i].bounds.X + squareSide * 2, yPositionOnScreen);
					}
				}
				else if (inventory[i] != null && inventory[i].bounds.Contains(x, y))
				{
					return new Vector2(inventory[i].bounds.X - squareSide * 6, yPositionOnScreen);
				}
			}
			return new Vector2(-1f, -1f);
		}

		public Rectangle getIconBoundsAt(int x, int y)
		{
			for (int i = 0; i < inventory.Count; i++)
			{
				if (inventory[i] != null && inventory[i].bounds.Contains(x, y))
				{
					return inventory[i].bounds;
				}
			}
			return new Rectangle(-1, -1, -1, -1);
		}

		private void intializeDragItem(int dragItem, int x, int y)
		{
			if (dragItem != -1)
			{
				dragX = x;
				dragY = y;
				dragScale = 1f;
			}
		}

		public bool inventoryContainsPoint(int x, int y)
		{
			if (inventory.Count <= 0)
			{
				return false;
			}
			if (x >= inventory[0].bounds.X && x <= inventory[inventory.Count - 1].bounds.X && y >= inventory[0].bounds.Y && y <= inventory[inventory.Count - 1].bounds.Y)
			{
				return true;
			}
			return false;
		}

		public void highlightIfHoverOverSlot(int x, int y, bool itemFromOtherInventory = false, Item itemBeingDragged = null, bool autoStack = false)
		{
			currentlyHighlightedEmptySlot = -1;
			int num = (playerInventory ? ((int)Game1.player.maxItems) : inventory.Count);
			if (!isWithinBounds(x, y) || trashCan.containsPoint(x, y) || getInventoryPositionOfClick(x, y) == -1)
			{
				return;
			}
			if (autoStack)
			{
				for (int i = 0; i < num; i++)
				{
					if (i >= inventory.Count)
					{
						return;
					}
					Item itemFromClickableComponent = getItemFromClickableComponent(inventory[i]);
					if (itemFromClickableComponent != null && itemFromOtherInventory && autoStack && itemBeingDragged != null && itemBeingDragged.canStackWith(itemFromClickableComponent) && itemBeingDragged.Stack + itemFromClickableComponent.Stack <= itemFromClickableComponent.maximumStackSize())
					{
						currentlyHighlightedEmptySlot = i;
						return;
					}
				}
			}
			for (int j = 0; j < num; j++)
			{
				if (j >= inventory.Count)
				{
					return;
				}
				if (!inventory[j].bounds.Contains(x, y))
				{
					continue;
				}
				if (itemFromOtherInventory)
				{
					if (inventory[j] != null && itemBeingDragged != null && itemBeingDragged.canStackWith(getItemFromClickableComponent(inventory[j])))
					{
						currentlyHighlightedEmptySlot = j;
						return;
					}
					if (inventory[j] != null && getItemFromClickableComponent(inventory[j]) == null)
					{
						currentlyHighlightedEmptySlot = j;
						return;
					}
				}
				if (currentlySelectedItem != -1 && inventory[j] != null)
				{
					Item itemFromClickableComponent2 = getItemFromClickableComponent(inventory[j]);
					if (itemFromClickableComponent2 == null && j != currentlySelectedItem)
					{
						currentlyHighlightedEmptySlot = j;
						return;
					}
					if (itemFromClickableComponent2 is Tool && currentlySelectedItem < actualInventory.Count && (actualInventory[currentlySelectedItem] == null || actualInventory[currentlySelectedItem] is Object) && (itemFromClickableComponent2 as Tool).canThisBeAttached((Object)actualInventory[currentlySelectedItem]))
					{
						currentlyHighlightedEmptySlot = j;
						return;
					}
					if (currentlySelectedItem < actualInventory.Count && actualInventory[currentlySelectedItem] != null && actualInventory[currentlySelectedItem].canStackWith(itemFromClickableComponent2))
					{
						currentlyHighlightedEmptySlot = j;
						return;
					}
				}
			}
			for (int k = 0; k < num && k < inventory.Count; k++)
			{
				if (itemFromOtherInventory && inventory[k] != null && itemBeingDragged != null && getItemFromClickableComponent(inventory[k]) == null)
				{
					currentlyHighlightedEmptySlot = k;
					break;
				}
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			if (externalHoldingTrashCan)
			{
				if (trashCan.bounds.Contains(x, y))
				{
					holdingTrashCan = true;
					if (trashCanLidRotation <= 0f)
					{
						Game1.playSound("trashcanlid");
					}
					trashCanLidRotation = Math.Min(trashCanLidRotation + (float)Math.PI / 48f, (float)Math.PI / 2f);
					currentlyHighlightedEmptySlot = -1;
					if (otherInventoryForTrash[otherInventoryTrashItemIndex] != null && Utility.getTrashReclamationPrice(otherInventoryForTrash[otherInventoryTrashItemIndex], Game1.player) > 0)
					{
						hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
						hoverAmount = Utility.getTrashReclamationPrice(otherInventoryForTrash[otherInventoryTrashItemIndex], Game1.player);
						hoverTitle = otherInventoryForTrash[otherInventoryTrashItemIndex].DisplayName;
					}
					return;
				}
				holdingTrashCan = false;
				externalHoldingTrashCan = false;
			}
			if (currentlyStackingItem != -1 && (Math.Abs(x - dragX) >= deltaForDrag || Math.Abs(y - dragY) >= deltaForDrag))
			{
				if (getItemFromClickableComponent(inventory[currentlyStackingItem]) == null)
				{
					Item itemFromClickableComponent = getItemFromClickableComponent(inventory[currentlySelectedItem]);
					Item one = itemFromClickableComponent.getOne();
					Utility.addItemToInventory(one, currentlyStackingItem, actualInventory);
					itemFromClickableComponent.Stack--;
					return;
				}
				Vector2 vector = new Vector2(dragX, dragY);
				Vector2 vector2 = new Vector2(inventory[currentlySelectedItem].bounds.X + squareSide / 2, inventory[currentlySelectedItem].bounds.Y + squareSide / 2);
				Vector2 vector3 = new Vector2(x, y);
				double num = (vector - vector3).Length();
				double num2 = (vector2 - vector3).Length();
				int num3 = getItemFromClickableComponent(inventory[currentlyStackingItem]).Stack + getItemFromClickableComponent(inventory[currentlySelectedItem]).Stack;
				int num4 = (int)(num2 * (double)num3 / (num + num2));
				int stack = num3 - num4;
				getItemFromClickableComponent(inventory[currentlyStackingItem]).Stack = num4;
				getItemFromClickableComponent(inventory[currentlySelectedItem]).Stack = stack;
				return;
			}
			if (dragItem != -1)
			{
				dragX = x;
				dragY = y;
				if (dragScale < 2f)
				{
					dragScale *= 1.075f;
				}
				else if (dragScale > 2f)
				{
					dragScale = 2f;
				}
				highlightIfHoverOverSlot(x, y);
				if (trashCan.containsPoint(x, y))
				{
					holdingTrashCan = true;
				}
			}
			else if (inventoryItemHeld != -1)
			{
				if (Math.Abs(x - startDragX) >= deltaForDrag || Math.Abs(y - startDragY) >= deltaForDrag)
				{
					showItemInfo = false;
					dragItem = inventoryItemHeld;
					intializeDragItem(dragItem, x, y);
					heldTimer = 0f;
					currentlySelectedItem = dragItem;
				}
				else if (inventory[inventoryItemHeld].bounds.Contains(x, y))
				{
					heldTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
					if (heldTimer >= tapHoldTime && !showItemInfo)
					{
						Game1.playSound("smallSelect");
						currentlySelectedItem = inventoryItemHeld;
						if (actualInventory[currentlySelectedItem] != null && actualInventory[currentlySelectedItem].Stack > 1 && currentlyHeldStack <= 0)
						{
							currentlyHeldStack = 1;
							stackTimer = 0f;
							oldStackTimer = stackTimer;
							stackIncrementTime = 0.4f;
						}
						showItemInfo = true;
					}
					if (heldTimer >= startStackTime && currentlyHeldStack > 0 && actualInventory[inventoryItemHeld].Stack > 1 && actualInventory[inventoryItemHeld].maximumStackSize() > 1)
					{
						stackTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
						if (stackTimer > oldStackTimer + stackIncrementTime)
						{
							currentlyHeldStack = Math.Min(actualInventory[inventoryItemHeld].Stack, Math.Min(actualInventory[inventoryItemHeld].maximumStackSize(), currentlyHeldStack + 1));
							stackIncrementTime = Math.Max(0.02f, stackIncrementTime * 0.85f);
							oldStackTimer = stackTimer;
						}
					}
				}
				else
				{
					showItemInfo = false;
					dragItem = inventoryItemHeld;
					currentlyHighlightedEmptySlot = dragItem;
					intializeDragItem(dragItem, x, y);
					heldTimer = 0f;
				}
			}
			if (showOrganizeButton && organizeButton != null && holdingOrganizeButton && !organizeButton.containsPoint(x, y))
			{
				holdingOrganizeButton = false;
				organizeButton.drawShadow = true;
				organizeButton.bounds.X += 4;
				organizeButton.bounds.Y -= 4;
			}
			if (dragItem != -1 && trashCan.containsPoint(x, y))
			{
				if (trashCanLidRotation <= 0f)
				{
					Game1.playSound("trashcanlid");
				}
				trashCanLidRotation = Math.Min(trashCanLidRotation + (float)Math.PI / 48f, (float)Math.PI / 2f);
				if (Utility.getTrashReclamationPrice(actualInventory[dragItem], Game1.player) > 0)
				{
					hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
					hoverAmount = Utility.getTrashReclamationPrice(actualInventory[dragItem], Game1.player);
					hoverTitle = actualInventory[dragItem].DisplayName;
				}
			}
			else if (showTrash && holdingTrashCan && !trashCan.containsPoint(x, y))
			{
				holdingTrashCan = false;
				trashCan.bounds.X = trashX;
				trashCan.bounds.Y = trashY;
			}
			else
			{
				hoverAmount = -1;
			}
		}

		public Item selectItemAt(int x, int y, Item oldItem = null)
		{
			if (currentlyStackingItem != -1)
			{
				return null;
			}
			foreach (ClickableComponent item2 in inventory)
			{
				if (!item2.containsPoint(x, y))
				{
					continue;
				}
				Item item = getItemFromClickableComponent(item2);
				if (item != null)
				{
					int num = Convert.ToInt32(item2.name);
					if (num == currentlySelectedItem || !highlightMethod(item))
					{
						currentlySelectedItem = -1;
						item = null;
					}
					else if (highlightMethod(item))
					{
						currentlySelectedItem = num;
						currentlyHeldStack = -1;
					}
				}
				else
				{
					currentlySelectedItem = -1;
					currentlyHeldStack = -1;
				}
				return item;
			}
			return oldItem;
		}

		public Item GetItemAt(int i)
		{
			if (i < 0)
			{
				return null;
			}
			if (i >= actualInventory.Count)
			{
				return null;
			}
			return actualInventory[i];
		}

		public void SetItemAt(int i, Item item)
		{
			if (i >= 0)
			{
				actualInventory[i] = item;
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
		}

		public void GamePadShowInfoPanel()
		{
			if (currentlySelectedItem < 0 || currentlySelectedItem >= actualInventory.Count)
			{
				return;
			}
			ClickableComponent clickableComponent = inventory[currentlySelectedItem];
			actualItemSelected = getItemFromClickableComponent(clickableComponent);
			showItemInfo = true;
			Game1.playSound("smallSelect");
			if (currentlySelectedItem < inventory.Count)
			{
				Item item = actualInventory[currentlySelectedItem];
				if (item == null)
				{
					GamePadHideInfoPanel();
					return;
				}
				hoverText = item.getDescription();
				hoverTitle = item.DisplayName;
				infoPanelPosition = getPositionOfSellPanel(clickableComponent.bounds.X, clickableComponent.bounds.Y);
			}
		}

		public void GamePadHideInfoPanel()
		{
			inventoryItemHeld = -1;
			dragItem = -1;
			showItemInfo = false;
			actualItemSelected = null;
		}

		private void SetAccurateInfoPanelPosition(SpriteBatch b)
		{
			if (_infoPanelRect == Rectangle.Empty)
			{
				_infoPanelRect.Width = 1;
				float y = infoPanelPosition.Y;
				infoPanelPosition.Y = Game1.uiViewport.Height + 100;
				drawInfoPanel(b);
				_infoPanelRect = IClickableMenu.lastTextureBoxRect;
				infoPanelPosition.Y = y;
				infoPanelPosition.X = inventory[currentlySelectedItem].bounds.X - _infoPanelRect.Width - 40;
			}
		}

		public Vector2 getColorPositionOfItem(int index)
		{
			if (inventory[index] != null)
			{
				return new Vector2(inventory[index].bounds.X + 8, inventory[index].bounds.Y - squareSide / 2 + 16);
			}
			return new Vector2(-999f, -999f);
		}

		public void ClearSelection()
		{
			currentlySelectedItem = -1;
			currentlyStackingItem = -1;
			inventoryItemHeld = -1;
			dragItem = -1;
			showItemInfo = false;
			actualItemSelected = null;
		}
	}
}
