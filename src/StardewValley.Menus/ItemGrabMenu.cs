using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	public class ItemGrabMenu : MenuWithInventory
	{
		public delegate bool behaviorOnMobileItemChange(Item i, int position, Item old, ItemGrabMenu container, bool onRemoval = false);

		public delegate void behaviorOnItemSelect(Item item, Farmer who);

		public delegate void behaviorOnItemAddtoItemsToGrab(Item item, Farmer who);

		public delegate void behaviorOnTapClose(Item item, Farmer who);

		public const int region_organizationButtons = 15923;

		public const int region_itemsToGrabMenuModifier = 53910;

		public const int region_fillStacksButton = 12952;

		public const int region_organizeButton = 106;

		public const int region_colorPickToggle = 27346;

		public const int region_specialButton = 12485;

		public const int region_lastShippedHolder = 12598;

		public const int source_none = 0;

		public const int source_chest = 1;

		public const int source_gift = 2;

		public const int source_fishingChest = 3;

		public const int specialButton_junimotoggle = 1;

		public InventoryMenu ItemsToGrabMenu;

		private TemporaryAnimatedSprite poof;

		public bool reverseGrab;

		public bool showReceivingMenu = true;

		public bool drawBG = true;

		public bool destroyItemOnClick;

		public bool canExitOnKey;

		public bool playRightClickSound;

		public bool allowRightClick;

		public bool shippingBin;

		private string message;

		private behaviorOnItemSelect behaviorFunction;

		public behaviorOnItemSelect behaviorOnItemGrab;

		public behaviorOnItemAddtoItemsToGrab behaviorOnAddtoItemsToGrab;

		public behaviorOnTapClose behaviorOnClose;

		private bool rearrangeGrangeOnExit;

		private InventoryMenu.highlightThisItem highlightFunction;

		protected List<TransferredItemSprite> _transferredItemSprites = new List<TransferredItemSprite>();

		private Item hoverItem;

		private Item sourceItem;

		public ClickableTextureComponent fillStacksButton;

		public ClickableTextureComponent organizeButton;

		public ClickableTextureComponent colorPickerToggleButton;

		public ClickableTextureComponent specialButton;

		public ClickableTextureComponent lastShippedHolder;

		public List<ClickableComponent> discreteColorPickerCC;

		public int source;

		public int whichSpecialButton;

		public object context;

		private bool snappedtoBottom;

		public DiscreteColorPicker chestColorPicker;

		private bool essential;

		private bool holdingFillStacksButton;

		private Rectangle topInv;

		private Rectangle bottomInv;

		private float widthMod;

		private float heightMod;

		private new int width;

		private new int height;

		private int lastShippedTextWidth;

		private Rectangle crateBounds;

		private Vector2 lastShippedTextPos;

		private string lastShippedText;

		private ClickableTextureComponent clickableShippingCrate;

		private ClickableTextureComponent clickableCrateLid;

		private bool lastControlWasWithJoystick;

		private string shippingInstructionText = Game1.content.LoadString("Strings\\UI:shippingInstructionText");

		private behaviorOnMobileItemChange itemChangeBehavior;

		private bool allowStack = true;

		private bool hoverOverShippingBin;

		public bool justGrabbing;

		protected bool _sourceItemInCurrentLocation;

		public ClickableTextureComponent junimoNoteIcon;

		private int junimoNotePulser;

		private int _selectedItemIndex = -1;

		private bool _movingItem;

		private bool _showTooltip;

		public bool enableGamePadControls;

		public ItemGrabMenu(IList<Item> inventory, object context = null)
			: base(null, okButton: true, trashCan: true, Game1.xEdge)
		{
			lastControlWasWithJoystick = false;
			this.context = context;
			justGrabbing = true;
			xPositionOnScreen = Game1.xEdge;
			yPositionOnScreen = 0;
			width = Game1.uiViewport.Width - Game1.xEdge * 2;
			height = Game1.uiViewport.Height - yPositionOnScreen;
			height = Game1.uiViewport.Height - yPositionOnScreen;
			if (height > 1080)
			{
				height = (height = 1080);
				yPositionOnScreen = (Game1.uiViewport.Height - height) / 2;
			}
			widthMod = (float)width / 1280f;
			heightMod = (float)height / 720f;
			topInv = new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height / 2);
			bottomInv = new Rectangle(xPositionOnScreen, yPositionOnScreen + height / 2, width, height / 2);
			int count = inventory.Count;
			int num = ((inventory.Count <= 3) ? 1 : ((inventory.Count > 6) ? 3 : 2));
			int num2 = count / num + ((count % num > 0) ? 1 : 0);
			topInv.Width = base.inventory.squareSide * num2 + base.inventory.hGap * (num2 - 1) + 64 + Math.Min(32, (num2 - 1) * 64 / 4);
			if (source == 1)
			{
				topInv.Width = Game1.uiViewport.Width - Game1.xEdge * 2;
			}
			topInv.X = Game1.uiViewport.Width / 2 - topInv.Width / 2;
			int xOff = 32;
			topInv.Height = base.inventory.squareSide * num + base.inventory.verticalGap * (num - 1) + base.inventory.yOffset + Math.Min(16, (num - 1) * 64 / 8);
			topInv.Y = yPositionOnScreen + (height / 2 - topInv.Height);
			int yOff = base.inventory.yOffset / 2;
			int x = topInv.X;
			int y = topInv.Y;
			int num3 = topInv.Width;
			int num4 = topInv.Height;
			int squareSide = base.inventory.squareSide;
			ItemsToGrabMenu = new InventoryMenu(x, y, playerInventory: false, inventory, null, count, num, 0, 0, drawSlots: true, num3, num4, showTrash: false, showOrganizeButton: false, 0, drawHeldItem: false, xOff, yOff, squareSide);
			ItemsToGrabMenu.populateClickableComponentList();
			for (int i = 0; i < ItemsToGrabMenu.inventory.Count; i++)
			{
				if (ItemsToGrabMenu.inventory[i] != null)
				{
					ItemsToGrabMenu.inventory[i].myID += 53910;
					ItemsToGrabMenu.inventory[i].upNeighborID += 53910;
					ItemsToGrabMenu.inventory[i].rightNeighborID += 53910;
					ItemsToGrabMenu.inventory[i].downNeighborID = -7777;
					ItemsToGrabMenu.inventory[i].leftNeighborID += 53910;
					ItemsToGrabMenu.inventory[i].fullyImmutable = true;
				}
			}
			for (int j = 0; j < GetColumnCount(); j++)
			{
				if (base.inventory != null && base.inventory.inventory != null && base.inventory.inventory.Count >= GetColumnCount())
				{
					base.inventory.inventory[j].upNeighborID = (shippingBin ? 12598 : (-7777));
				}
			}
			if (!shippingBin)
			{
				for (int k = 0; k < GetColumnCount() * 3; k++)
				{
					if (base.inventory != null && base.inventory.inventory != null && base.inventory.inventory.Count > k)
					{
						base.inventory.inventory[k].upNeighborID = -7777;
						base.inventory.inventory[k].upNeighborImmutable = true;
					}
				}
			}
			if (okButton != null)
			{
				okButton.leftNeighborID = 11;
			}
			populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				snapToDefaultClickableComponent();
			}
			base.inventory.showGrayedOutSlots = true;
		}

		public virtual void DropRemainingItems()
		{
			if (ItemsToGrabMenu == null || ItemsToGrabMenu.actualInventory == null)
			{
				return;
			}
			foreach (Item item in ItemsToGrabMenu.actualInventory)
			{
				if (item != null)
				{
					Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
				}
			}
			ItemsToGrabMenu.actualInventory.Clear();
		}

		public override bool readyToClose()
		{
			return base.readyToClose();
		}

		public ItemGrabMenu(IList<Item> inventory, bool reverseGrab, bool showReceivingMenu, InventoryMenu.highlightThisItem highlightFunction, behaviorOnItemSelect behaviorOnItemSelectFunction, string message, behaviorOnItemSelect behaviorOnItemGrab = null, bool snapToBottom = false, bool canBeExitedWithKey = false, bool playRightClickSound = true, bool allowRightClick = true, bool showOrganizeButton = false, int source = 0, Item sourceItem = null, int whichSpecialButton = -1, object specialObject = null, int storageCapacity = -1, int numRows = 3, behaviorOnMobileItemChange itemChangeBehavior = null, bool allowStack = true, behaviorOnItemAddtoItemsToGrab behaviorOnAddtoTop = null, bool rearrangeGrangeOnExit = false, behaviorOnTapClose behaviorOnTapClose = null, object context = null)
			: base(highlightFunction, okButton: true, trashCan: true, Game1.xEdge)
		{
			this.source = source;
			this.message = message;
			this.reverseGrab = reverseGrab;
			this.showReceivingMenu = showReceivingMenu;
			this.playRightClickSound = playRightClickSound;
			this.allowRightClick = allowRightClick;
			base.inventory.showGrayedOutSlots = true;
			this.sourceItem = sourceItem;
			this.context = context;
			this.highlightFunction = highlightFunction;
			behaviorOnClose = behaviorOnTapClose;
			this.rearrangeGrangeOnExit = rearrangeGrangeOnExit;
			this.allowStack = allowStack;
			xPositionOnScreen = Game1.xEdge;
			yPositionOnScreen = 0;
			width = Game1.uiViewport.Width - Game1.xEdge * 2;
			height = Game1.uiViewport.Height - yPositionOnScreen;
			if (height > 1080)
			{
				height = (height = 1080);
				yPositionOnScreen = (yPositionOnScreen = (Game1.uiViewport.Height - height) / 2);
			}
			widthMod = (float)width / 1280f;
			heightMod = (float)height / 720f;
			crateBounds = new Rectangle(0, 0, 120, 88);
			topInv = new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height / 2);
			bottomInv = new Rectangle(xPositionOnScreen, yPositionOnScreen + height / 2, width, height / 2);
			this.itemChangeBehavior = itemChangeBehavior;
			behaviorOnAddtoItemsToGrab = behaviorOnAddtoTop;
			if (sourceItem != null && Game1.currentLocation.objects.Values.Contains(sourceItem))
			{
				_sourceItemInCurrentLocation = true;
			}
			else
			{
				_sourceItemInCurrentLocation = false;
			}
			this.whichSpecialButton = whichSpecialButton;
			if (snapToBottom)
			{
				movePosition(0, Game1.uiViewport.Height - (yPositionOnScreen + height - IClickableMenu.spaceToClearTopBorder));
				snappedtoBottom = true;
			}
			if (source == 1 && sourceItem != null && sourceItem is Chest && (sourceItem as Chest).GetActualCapacity() != 36)
			{
				int actualCapacity = (sourceItem as Chest).GetActualCapacity();
				int num = 3;
				if (actualCapacity < 9)
				{
					num = 1;
				}
				for (int i = 0; i < actualCapacity; i++)
				{
					if (inventory.Count <= i)
					{
						inventory.Add(null);
					}
				}
				int num2 = actualCapacity / num;
				topInv.Width = base.inventory.squareSide * num2 + base.inventory.hGap * (num2 - 1) + 64 + Math.Min(32, (num2 - 1) * 64 / 4);
				topInv.X = Game1.uiViewport.Width / 2 - topInv.Width / 2;
				int xOff = 32;
				topInv.Height = base.inventory.squareSide * num + base.inventory.verticalGap * (num - 1) + base.inventory.yOffset * 2;
				topInv.Y = yPositionOnScreen + (height / 2 - topInv.Height);
				int yOffset = base.inventory.yOffset;
				int x = topInv.X;
				int y = topInv.Y;
				IList<Item> actualInventory = inventory;
				int rows = num;
				int num3 = topInv.Width;
				int num4 = topInv.Height;
				int squareSide = base.inventory.squareSide;
				ItemsToGrabMenu = new InventoryMenu(x, y, playerInventory: false, actualInventory, highlightFunction, actualCapacity, rows, 0, 0, drawSlots: true, num3, num4, showTrash: false, showOrganizeButton: false, 0, drawHeldItem: false, xOff, yOffset, squareSide);
				if ((sourceItem as Chest).SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
				{
					base.inventory.moveItemSound = "Ship";
				}
			}
			else
			{
				if (inventory == null)
				{
					inventory = new List<Item>();
				}
				if (storageCapacity == -1)
				{
					storageCapacity = ((inventory.Count <= 0) ? 1 : inventory.Count);
				}
				int num5 = ((storageCapacity == 1) ? 1 : 3);
				if (storageCapacity % 2 == 0 && storageCapacity < 12)
				{
					num5 = 2;
				}
				int num6 = (int)Math.Ceiling((double)storageCapacity / (double)num5);
				for (int j = 0; j < storageCapacity; j++)
				{
					if (inventory.Count <= j)
					{
						inventory.Add(null);
					}
				}
				topInv.Width = base.inventory.squareSide * num6 + base.inventory.hGap * (num6 - 1) + 64 + Math.Min(32, (num6 - 1) * 64 / 4);
				if (source == 1)
				{
					topInv.Width = Game1.uiViewport.Width - Game1.xEdge * 2;
				}
				topInv.X = Game1.uiViewport.Width / 2 - topInv.Width / 2;
				int xOff2 = 32;
				topInv.Height = base.inventory.squareSide * num5 + base.inventory.verticalGap * (num5 - 1) + base.inventory.yOffset + Math.Min(16, (num5 - 1) * 64 / 8);
				topInv.Y = yPositionOnScreen + (height / 2 - topInv.Height);
				int yOff = base.inventory.yOffset / 2;
				int x2 = topInv.X;
				int y2 = topInv.Y;
				IList<Item> actualInventory2 = inventory;
				int capacity = storageCapacity;
				int rows2 = num5;
				int num7 = topInv.Width;
				int num8 = topInv.Height;
				int squareSide = base.inventory.squareSide;
				ItemsToGrabMenu = new InventoryMenu(x2, y2, playerInventory: false, actualInventory2, highlightFunction, capacity, rows2, 0, 0, drawSlots: true, num7, num8, showTrash: false, showOrganizeButton, 0, drawHeldItem: false, xOff2, yOff, squareSide);
			}
			if (whichSpecialButton == 1)
			{
				specialButton = new ClickableTextureComponent(new Rectangle(ItemsToGrabMenu.orgX, topInv.Y + topInv.Height / 4, 64, 64), Game1.mouseCursors, new Rectangle(108, 491, 16, 16), 4f)
				{
					myID = 12485,
					downNeighborID = (showOrganizeButton ? 12952 : 5948),
					region = 15923,
					leftNeighborID = 53921
				};
				if (context != null && context is JunimoHut)
				{
					specialButton.sourceRect.X = ((context as JunimoHut).noHarvest ? 124 : 108);
				}
			}
			if (source == 1 && sourceItem != null && sourceItem is Chest && (sourceItem as Chest).SpecialChestType == Chest.SpecialChestTypes.None)
			{
				chestColorPicker = new DiscreteColorPicker(topInv.X + 16, topInv.Y + topInv.Height / 8, 0, new Chest(playerChest: true), 3, topInv.Width * 5 / 6, topInv.Height * 3 / 4);
				chestColorPicker.visible = false;
				Game1.player.showChestColorPicker = false;
				chestColorPicker.colorSelection = chestColorPicker.getSelectionFromColor((sourceItem as Chest).playerChoiceColor);
				(chestColorPicker.itemToDrawColored as Chest).playerChoiceColor.Value = chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
				colorPickerToggleButton = new ClickableTextureComponent(new Rectangle(ItemsToGrabMenu.orgX, topInv.Y + topInv.Height - topInv.Height / 8 - 64, 64, 64), Game1.mouseCursors, new Rectangle(119, 469, 16, 16), 4f, drawShadow: true)
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker"),
					myID = 27346,
					downNeighborID = (showOrganizeButton ? 106 : 5948),
					leftNeighborID = 11
				};
			}
			ItemsToGrabMenu.orgY = topInv.Y + topInv.Height / 8;
			if (showOrganizeButton && ItemsToGrabMenu.organizeButton != null)
			{
				ItemsToGrabMenu.organizeButton.bounds.X = ItemsToGrabMenu.orgX;
				ItemsToGrabMenu.organizeButton.bounds.Y = ItemsToGrabMenu.orgY;
				fillStacksButton = new ClickableTextureComponent("", new Rectangle(ItemsToGrabMenu.organizeButton.bounds.X, ItemsToGrabMenu.getInvY() + ItemsToGrabMenu.getInvHeight() / 2 - 32, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_FillStacks"), Game1.mouseCursors, new Rectangle(103, 469, 16, 16), 4f)
				{
					myID = 12952,
					upNeighborID = ((colorPickerToggleButton != null) ? 27346 : ((specialButton != null) ? 12485 : (-500))),
					downNeighborID = 106,
					leftNeighborID = 53921,
					region = 15923
				};
			}
			ItemsToGrabMenu.populateClickableComponentList();
			for (int k = 0; k < ItemsToGrabMenu.inventory.Count; k++)
			{
				if (ItemsToGrabMenu.inventory[k] != null)
				{
					ItemsToGrabMenu.inventory[k].myID += 53910;
					ItemsToGrabMenu.inventory[k].upNeighborID += 53910;
					ItemsToGrabMenu.inventory[k].rightNeighborID += 53910;
					ItemsToGrabMenu.inventory[k].downNeighborID = -7777;
					ItemsToGrabMenu.inventory[k].leftNeighborID += 53910;
					ItemsToGrabMenu.inventory[k].fullyImmutable = true;
				}
			}
			behaviorFunction = behaviorOnItemSelectFunction;
			this.behaviorOnItemGrab = behaviorOnItemGrab;
			canExitOnKey = canBeExitedWithKey;
			if ((Game1.isAnyGamePadButtonBeingPressed() || !Game1.lastCursorMotionWasMouse) && ItemsToGrabMenu.actualInventory.Count > 0 && Game1.activeClickableMenu == null)
			{
				Game1.setMousePosition(base.inventory.inventory[0].bounds.Center);
			}
			if (chestColorPicker != null)
			{
				discreteColorPickerCC = new List<ClickableComponent>();
				for (int l = 0; l < chestColorPicker.totalColors; l++)
				{
					discreteColorPickerCC.Add(new ClickableComponent(new Rectangle(chestColorPicker.xPositionOnScreen + IClickableMenu.borderWidth / 2 + l * (DiscreteColorPicker.sizeOfEachSwatch + 2) * 4, chestColorPicker.yPositionOnScreen + IClickableMenu.borderWidth / 2, (DiscreteColorPicker.sizeOfEachSwatch + 2) * 4, DiscreteColorPicker.sizeOfEachSwatch * 4), "")
					{
						myID = l + 4343,
						rightNeighborID = ((l < chestColorPicker.totalColors - 1) ? (l + 4343 + 1) : (-1)),
						leftNeighborID = ((l > 0) ? (l + 4343 - 1) : (-1)),
						downNeighborID = ((ItemsToGrabMenu != null && ItemsToGrabMenu.inventory.Count > 0) ? 53910 : 0)
					});
				}
			}
			for (int m = 0; m < GetColumnCount(); m++)
			{
				if (base.inventory != null && base.inventory.inventory != null && base.inventory.inventory.Count >= 12)
				{
					base.inventory.inventory[m].upNeighborID = (shippingBin ? 12598 : ((discreteColorPickerCC != null && ItemsToGrabMenu != null && ItemsToGrabMenu.inventory.Count <= m && Game1.player.showChestColorPicker) ? 4343 : ((ItemsToGrabMenu.inventory.Count > m) ? (53910 + m) : 53910)));
				}
				if (discreteColorPickerCC != null && ItemsToGrabMenu != null && ItemsToGrabMenu.inventory.Count > m && Game1.player.showChestColorPicker)
				{
					ItemsToGrabMenu.inventory[m].upNeighborID = 4343;
				}
				else
				{
					ItemsToGrabMenu.inventory[m].upNeighborID = -1;
				}
			}
			if (!shippingBin)
			{
				for (int n = 0; n < 36; n++)
				{
					if (base.inventory != null && base.inventory.inventory != null && base.inventory.inventory.Count > n)
					{
						base.inventory.inventory[n].upNeighborID = -7777;
						base.inventory.inventory[n].upNeighborImmutable = true;
					}
				}
			}
			if (okButton != null)
			{
				okButton.leftNeighborID = 11;
			}
			populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				snapToDefaultClickableComponent();
			}
		}

		public virtual int GetColumnCount()
		{
			return ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows;
		}

		public ItemGrabMenu setEssential(bool essential)
		{
			this.essential = essential;
			return this;
		}

		public void initializeShippingBin()
		{
			shippingBin = true;
			topInv.Width = width / 2;
			topInv.X = xPositionOnScreen + topInv.Width / 2;
			topInv.Y = yPositionOnScreen;
			topInv.Height = bottomInv.Y - topInv.Y;
			crateBounds = new Rectangle(topInv.X + topInv.Width / 6, topInv.Y + (topInv.Height - 140) / 2, 120, 88);
			clickableShippingCrate = new ClickableTextureComponent(crateBounds, Game1.mouseCursors, new Rectangle(526, 218, 30, 22), 4f, drawShadow: true);
			clickableShippingCrate.bounds.Y += 52;
			clickableCrateLid = new ClickableTextureComponent(crateBounds, Game1.mouseCursors, new Rectangle(494, 226, 30, 13), 4f, drawShadow: true);
			lastShippedHolder = new ClickableTextureComponent("", new Rectangle(topInv.X + topInv.Width * 5 / 6 - 96, crateBounds.Y, 96, 96), "", Game1.content.LoadString("Strings\\UI:ShippingBin_LastItem"), Game1.mouseCursors, new Rectangle(293, 360, 24, 24), 4f)
			{
				myID = 12598,
				region = 12598
			};
			lastShippedText = Game1.content.LoadString("Strings\\UI:ShippingBin_LastItem");
			lastShippedTextPos = new Vector2(lastShippedHolder.bounds.X + lastShippedHolder.bounds.Width / 2, lastShippedHolder.bounds.Y + lastShippedHolder.bounds.Height + 16);
			for (int i = 0; i < GetColumnCount(); i++)
			{
				if (inventory != null && inventory.inventory != null && inventory.inventory.Count >= GetColumnCount())
				{
					inventory.inventory[i].upNeighborID = -7777;
					if (i == 11)
					{
						inventory.inventory[i].rightNeighborID = 5948;
					}
				}
			}
			populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				snapToDefaultClickableComponent();
			}
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			switch (direction)
			{
			case 2:
			{
				for (int j = 0; j < 12; j++)
				{
					if (inventory != null && inventory.inventory != null && inventory.inventory.Count >= GetColumnCount() && shippingBin)
					{
						inventory.inventory[j].upNeighborID = (shippingBin ? 12598 : (Math.Min(j, ItemsToGrabMenu.inventory.Count - 1) + 53910));
					}
				}
				if (!shippingBin && oldID >= 53910)
				{
					int num2 = oldID - 53910;
					if (num2 + GetColumnCount() <= ItemsToGrabMenu.inventory.Count - 1)
					{
						currentlySnappedComponent = getComponentWithID(num2 + GetColumnCount() + 53910);
						snapCursorToCurrentSnappedComponent();
						break;
					}
				}
				currentlySnappedComponent = getComponentWithID((oldRegion != 12598) ? ((oldID - 53910) % GetColumnCount()) : 0);
				snapCursorToCurrentSnappedComponent();
				break;
			}
			case 0:
			{
				if (shippingBin && oldID < 12)
				{
					currentlySnappedComponent = getComponentWithID(12598);
					currentlySnappedComponent.downNeighborID = oldID;
					snapCursorToCurrentSnappedComponent();
					break;
				}
				if (oldID < 53910 && oldID >= 12)
				{
					currentlySnappedComponent = getComponentWithID(oldID - 12);
					break;
				}
				int num = oldID + GetColumnCount() * 2;
				for (int i = 0; i < 3; i++)
				{
					if (ItemsToGrabMenu.inventory.Count > num)
					{
						break;
					}
					num -= GetColumnCount();
				}
				if (showReceivingMenu)
				{
					if (num < 0)
					{
						if (ItemsToGrabMenu.inventory.Count > 0)
						{
							currentlySnappedComponent = getComponentWithID(53910 + ItemsToGrabMenu.inventory.Count - 1);
						}
						else if (discreteColorPickerCC != null)
						{
							currentlySnappedComponent = getComponentWithID(4343);
						}
					}
					else
					{
						currentlySnappedComponent = getComponentWithID(num + 53910);
						if (currentlySnappedComponent == null)
						{
							currentlySnappedComponent = getComponentWithID(53910);
						}
					}
				}
				snapCursorToCurrentSnappedComponent();
				break;
			}
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			if (shippingBin)
			{
				currentlySnappedComponent = getComponentWithID(0);
			}
			else if (source == 1 && sourceItem != null && sourceItem is Chest && (sourceItem as Chest).SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
			{
				currentlySnappedComponent = getComponentWithID(0);
			}
			else
			{
				currentlySnappedComponent = getComponentWithID((ItemsToGrabMenu.inventory.Count > 0 && showReceivingMenu) ? 53910 : 0);
			}
			snapCursorToCurrentSnappedComponent();
		}

		public void setSourceItem(Item item)
		{
			sourceItem = item;
			chestColorPicker = null;
			colorPickerToggleButton = null;
			if (source == 1 && sourceItem != null && sourceItem is Chest && (sourceItem as Chest).SpecialChestType == Chest.SpecialChestTypes.None)
			{
				chestColorPicker = new DiscreteColorPicker(xPositionOnScreen, yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, 0, new Chest(playerChest: true, sourceItem.ParentSheetIndex));
				chestColorPicker.colorSelection = chestColorPicker.getSelectionFromColor((sourceItem as Chest).playerChoiceColor);
				(chestColorPicker.itemToDrawColored as Chest).playerChoiceColor.Value = chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
				colorPickerToggleButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width, yPositionOnScreen + 64 + 20, 64, 64), Game1.mouseCursors, new Rectangle(119, 469, 16, 16), 4f)
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker")
				};
			}
		}

		public void setBackgroundTransparency(bool b)
		{
			drawBG = b;
		}

		public void setDestroyItemOnClick(bool b)
		{
			destroyItemOnClick = b;
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			if (snappedtoBottom)
			{
				movePosition((newBounds.Width - oldBounds.Width) / 2, Game1.uiViewport.Height - (yPositionOnScreen + height - IClickableMenu.spaceToClearTopBorder));
			}
			if (ItemsToGrabMenu != null)
			{
				ItemsToGrabMenu.gameWindowSizeChanged(oldBounds, newBounds);
			}
			if (source == 1 && sourceItem != null && sourceItem is Chest)
			{
				chestColorPicker = new DiscreteColorPicker(xPositionOnScreen, yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2);
				chestColorPicker.colorSelection = chestColorPicker.getSelectionFromColor((sourceItem as Chest).playerChoiceColor);
			}
		}

		private Item GetItemAt(int index)
		{
			if (index < 0)
			{
				index = 0;
			}
			else if (index > ItemsToGrabMenu.actualInventory.Count + inventory.totalItemSlots)
			{
				index = ItemsToGrabMenu.actualInventory.Count + inventory.totalItemSlots - 1;
			}
			if (index < ItemsToGrabMenu.actualInventory.Count)
			{
				return ItemsToGrabMenu.GetItemAt(index);
			}
			return inventory.GetItemAt(index - ItemsToGrabMenu.actualInventory.Count);
		}

		private void SetItemAt(int index, Item item)
		{
			if (index < 0)
			{
				index = 0;
			}
			else if (index > ItemsToGrabMenu.actualInventory.Count + inventory.totalItemSlots)
			{
				index = ItemsToGrabMenu.actualInventory.Count + inventory.totalItemSlots - 1;
			}
			Log.It("ItemsToGrabMenu.actualInventory.Count:" + ItemsToGrabMenu.actualInventory.Count + ", inventory.totalItemSlots:" + inventory.totalItemSlots + ", index:" + index);
			if (item != null)
			{
				Log.It("SetItemAt index:" + index + ", item:" + item.Name);
			}
			else
			{
				Log.It("SetItemAt index:" + index);
			}
			if (index < ItemsToGrabMenu.actualInventory.Count)
			{
				ItemsToGrabMenu.SetItemAt(index, item);
				Log.It("SetItemAt ItemsToGrabMenu index:" + index);
			}
			else
			{
				inventory.SetItemAt(index - ItemsToGrabMenu.actualInventory.Count, item);
				Log.It("SetItemAt inventory index:" + (index - ItemsToGrabMenu.actualInventory.Count));
			}
		}

		public void receiveGamePadButtonGrabbingItems(Buttons b)
		{
			switch (b)
			{
			case Buttons.DPadUp:
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
			case Buttons.LeftThumbstickUp:
				_selectedItemIndex = Math.Max(0, _selectedItemIndex - 1);
				break;
			case Buttons.DPadDown:
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickDown:
			case Buttons.LeftThumbstickRight:
				_selectedItemIndex = Math.Min(_selectedItemIndex + 1, ItemsToGrabMenu.actualInventory.Count - 1);
				break;
			case Buttons.A:
				_showTooltip = !_showTooltip;
				if (!_showTooltip)
				{
					ItemsToGrabMenu.GamePadHideInfoPanel();
					inventory.GamePadHideInfoPanel();
				}
				break;
			case Buttons.X:
			{
				if (_selectedItemIndex <= -1)
				{
					break;
				}
				Item itemAt = GetItemAt(_selectedItemIndex);
				if (itemAt == null)
				{
					break;
				}
				if (_selectedItemIndex < ItemsToGrabMenu.actualInventory.Count && behaviorOnItemGrab != null)
				{
					behaviorOnItemGrab(itemAt, Game1.player);
				}
				if (Utility.IsNormalObjectAtParentSheetIndex(itemAt, 326))
				{
					try
					{
						heldItem = null;
						Game1.player.canUnderstandDwarves = true;
						Game1.playSound("fireball");
						Utility.removeItemFromInventory(_selectedItemIndex, ItemsToGrabMenu.actualInventory);
						ItemsToGrabMenu.currentlyStackingItem = -1;
						ItemsToGrabMenu.currentlySelectedItem = -1;
						ItemsToGrabMenu.dragItem = -1;
						return;
					}
					catch (Exception exception)
					{
						Log.Exception(exception, "U1");
						return;
					}
				}
				if (itemAt is Object && (int)(itemAt as Object).parentSheetIndex == 102)
				{
					try
					{
						heldItem = null;
						Game1.player.foundArtifact(102, 1);
						Game1.playSound("fireball");
						Utility.removeItemFromInventory(_selectedItemIndex, ItemsToGrabMenu.actualInventory);
						ItemsToGrabMenu.currentlyStackingItem = -1;
						ItemsToGrabMenu.currentlySelectedItem = -1;
						ItemsToGrabMenu.dragItem = -1;
						return;
					}
					catch (Exception exception2)
					{
						Log.Exception(exception2, "V1");
						return;
					}
				}
				if (itemAt is Object && (bool)(itemAt as Object).isRecipe)
				{
					try
					{
						string key = itemAt.Name.Substring(0, itemAt.Name.IndexOf("Recipe") - 1);
						if ((itemAt as Object).Category == -7)
						{
							if (!Game1.player.cookingRecipes.ContainsKey(key))
							{
								Game1.player.cookingRecipes.Add(key, 0);
							}
						}
						else if (!Game1.player.craftingRecipes.ContainsKey(key))
						{
							Game1.player.craftingRecipes.Add(key, 0);
						}
						Game1.playSound("newRecipe");
						Utility.removeItemFromInventory(_selectedItemIndex, ItemsToGrabMenu.actualInventory);
						ItemsToGrabMenu.currentlyStackingItem = -1;
						ItemsToGrabMenu.currentlySelectedItem = -1;
						ItemsToGrabMenu.dragItem = -1;
						heldItem = null;
					}
					catch (Exception exception3)
					{
						Log.Exception(exception3, "W1");
						return;
					}
				}
				for (int i = 0; i < Math.Min(Game1.player.MaxItems, inventory.inventory.Count); i++)
				{
					if (inventory.GetItemAt(i) == null)
					{
						inventory.SetItemAt(i, itemAt);
						SetItemAt(_selectedItemIndex, null);
						_selectedItemIndex = -1;
						break;
					}
				}
				break;
			}
			}
			if (_selectedItemIndex > -1)
			{
				ItemsToGrabMenu.currentlySelectedItem = _selectedItemIndex;
				if (_showTooltip)
				{
					ItemsToGrabMenu.GamePadShowInfoPanel();
					inventory.GamePadHideInfoPanel();
				}
			}
		}

		public void receiveGamePadButtonShippingBin(Buttons b)
		{
			switch (b)
			{
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
				_selectedItemIndex = Math.Max(0, _selectedItemIndex - 1);
				break;
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickRight:
				_selectedItemIndex = Math.Min(_selectedItemIndex + 1, inventory.inventory.Count - 1);
				break;
			case Buttons.DPadUp:
			case Buttons.LeftThumbstickUp:
				_selectedItemIndex = Math.Max(0, _selectedItemIndex - 12);
				break;
			case Buttons.DPadDown:
			case Buttons.LeftThumbstickDown:
				_selectedItemIndex = Math.Min(_selectedItemIndex + 12, inventory.inventory.Count - 1);
				break;
			case Buttons.A:
				_showTooltip = !_showTooltip;
				if (!_showTooltip)
				{
					ItemsToGrabMenu.GamePadHideInfoPanel();
					inventory.GamePadHideInfoPanel();
				}
				break;
			case Buttons.X:
				if (_selectedItemIndex > -1)
				{
					Item itemAt = inventory.GetItemAt(_selectedItemIndex);
					if (itemAt != null && inventory.highlightMethod(itemAt))
					{
						Game1.getFarm().getShippingBin(Game1.player).Add(itemAt);
						Game1.getFarm().lastItemShipped = itemAt;
						Game1.playSound("coin");
						Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
					}
				}
				break;
			case Buttons.Y:
			{
				Item lastItemShipped = Game1.getFarm().lastItemShipped;
				if (lastItemShipped == null || !Game1.player.addItemToInventoryBool(lastItemShipped))
				{
					break;
				}
				Game1.playSound("coin");
				Game1.getFarm().getShippingBin(Game1.player).Remove(lastItemShipped);
				Game1.getFarm().lastItemShipped = null;
				if (Game1.player.ActiveObject != null)
				{
					Game1.player.showCarrying();
					Game1.player.Halt();
				}
				for (int i = 0; i < inventory.inventory.Count; i++)
				{
					if (inventory.actualInventory[i] == lastItemShipped)
					{
						_selectedItemIndex = i;
						break;
					}
				}
				break;
			}
			}
			if (_selectedItemIndex > -1)
			{
				inventory.currentlySelectedItem = _selectedItemIndex;
				if (_showTooltip)
				{
					inventory.GamePadShowInfoPanel();
					ItemsToGrabMenu.GamePadHideInfoPanel();
				}
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.B)
			{
				if (heldItem != null)
				{
					heldItem = null;
				}
				if (upperRightCloseButton != null && readyToClose())
				{
					OnTapUpperRightCloseButton();
				}
			}
		}

		private void checkBehavior(Item movingItem, Item swapItem, int newMovingItemIndex, int newSwapItemIndex)
		{
			if (behaviorOnAddtoItemsToGrab != null && newMovingItemIndex < ItemsToGrabMenu.actualInventory.Count && newSwapItemIndex > ItemsToGrabMenu.actualInventory.Count)
			{
				behaviorOnAddtoItemsToGrab(movingItem, Game1.player);
			}
		}

		private void HighlightSelectedItemInChest()
		{
			if (_selectedItemIndex <= -1)
			{
				return;
			}
			if (_selectedItemIndex < ItemsToGrabMenu.actualInventory.Count)
			{
				ItemsToGrabMenu.currentlySelectedItem = _selectedItemIndex;
				inventory.currentlySelectedItem = -1;
				if (_showTooltip)
				{
					ItemsToGrabMenu.GamePadShowInfoPanel();
					inventory.GamePadHideInfoPanel();
				}
			}
			else
			{
				inventory.currentlySelectedItem = _selectedItemIndex - ItemsToGrabMenu.actualInventory.Count;
				ItemsToGrabMenu.currentlySelectedItem = -1;
				if (_showTooltip)
				{
					inventory.GamePadShowInfoPanel();
					ItemsToGrabMenu.GamePadHideInfoPanel();
				}
			}
		}

		private void OnTapUpperRightCloseButton()
		{
			try
			{
				if (behaviorOnClose != null)
				{
					behaviorOnClose(heldItem, Game1.player);
				}
				if (rearrangeGrangeOnExit)
				{
					rearrangeGrange();
				}
				if (essential)
				{
					foreach (Item item2 in ItemsToGrabMenu.actualInventory)
					{
						if (item2 == null)
						{
							continue;
						}
						if ((int)item2.parentSheetIndex == 102)
						{
							Game1.player.addItemToInventoryBool(item2);
							continue;
						}
						Item item = Game1.player.addItemToInventory(item2);
						if (item != null)
						{
							Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
						}
					}
				}
				if (context is JunimoNoteMenu)
				{
					foreach (Item item3 in ItemsToGrabMenu.actualInventory)
					{
						if (item3 != null)
						{
							((JunimoNoteMenu)context).unsetRewardGrabbed(item3, Game1.player);
						}
					}
				}
				exitThisMenu();
				if (Game1.currentLocation.currentEvent != null)
				{
					Game1.currentLocation.currentEvent.CurrentCommand++;
				}
			}
			catch (Exception exception)
			{
				Log.Exception(exception, "upperRightCloseButton");
			}
		}

		private void UndoLastShippedItem()
		{
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			lastControlWasWithJoystick = false;
			_selectedItemIndex = -1;
			_movingItem = false;
			Item item = heldItem;
			if (upperRightCloseButton != null && upperRightCloseButton.containsPoint(x, y))
			{
				OnTapUpperRightCloseButton();
				return;
			}
			if (shippingBin && topInv.Contains(x, y))
			{
				if (Game1.getFarm().lastItemShipped != null)
				{
					try
					{
						if (lastShippedHolder.containsPoint(x, y) && inventory.dragItem == -1)
						{
							if (Game1.player.addItemToInventoryBool(Game1.getFarm().lastItemShipped))
							{
								Game1.playSound("coin");
								Game1.getFarm().getShippingBin(Game1.player).Remove(Game1.getFarm().lastItemShipped);
								Game1.getFarm().lastItemShipped = null;
								if (Game1.player.ActiveObject != null)
								{
									Game1.player.showCarrying();
									Game1.player.Halt();
								}
							}
							ItemsToGrabMenu.currentlySelectedItem = -1;
							ItemsToGrabMenu.currentlyStackingItem = -1;
							inventory.currentlySelectedItem = -1;
							inventory.currentlyStackingItem = -1;
							inventory.dragItem = -1;
							ItemsToGrabMenu.dragItem = -1;
							inventory.inventoryItemHeld = -1;
							ItemsToGrabMenu.inventoryItemHeld = -1;
							return;
						}
					}
					catch (Exception exception)
					{
						Log.Exception(exception, "B");
						return;
					}
				}
				if (heldItem != null && inventory != null && inventory.currentlySelectedItem != -1 && heldItem is Object && ((Object)heldItem).canBeShipped())
				{
					if (inventory.currentlyHeldStack != -1)
					{
						try
						{
							Item one = heldItem.getOne();
							one.Stack = inventory.currentlyHeldStack;
							Game1.getFarm().getShippingBin(Game1.player).Add(one);
							Game1.getFarm().lastItemShipped = one;
							Game1.playSound("coin");
							heldItem.Stack -= inventory.currentlyHeldStack;
							inventory.currentlyHeldStack = -1;
							if (heldItem.Stack <= 0)
							{
								Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
							}
							heldItem = null;
							ItemsToGrabMenu.currentlySelectedItem = -1;
							ItemsToGrabMenu.currentlyStackingItem = -1;
							inventory.currentlySelectedItem = -1;
							inventory.currentlyStackingItem = -1;
							inventory.dragItem = -1;
							ItemsToGrabMenu.dragItem = -1;
							inventory.inventoryItemHeld = -1;
							ItemsToGrabMenu.inventoryItemHeld = -1;
							return;
						}
						catch (Exception exception2)
						{
							Log.Exception(exception2, "C");
							return;
						}
					}
					try
					{
						Game1.getFarm().getShippingBin(Game1.player).Add(heldItem);
						Game1.getFarm().lastItemShipped = heldItem;
						Game1.playSound("coin");
						Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
						heldItem = null;
						ItemsToGrabMenu.currentlySelectedItem = -1;
						ItemsToGrabMenu.currentlyStackingItem = -1;
						inventory.currentlySelectedItem = -1;
						inventory.currentlyStackingItem = -1;
						inventory.dragItem = -1;
						ItemsToGrabMenu.dragItem = -1;
						inventory.inventoryItemHeld = -1;
						ItemsToGrabMenu.inventoryItemHeld = -1;
						return;
					}
					catch (Exception exception3)
					{
						Log.Exception(exception3, "D");
						return;
					}
				}
			}
			if (chestColorPicker == null || (chestColorPicker != null && !chestColorPicker.visible))
			{
				if (ItemsToGrabMenu.isWithinBounds(x, y))
				{
					int currentlySelectedItem = ItemsToGrabMenu.currentlySelectedItem;
					if (ItemsToGrabMenu.currentlySelectedItem == -1 && inventory.currentlySelectedItem == -1)
					{
						ItemsToGrabMenu.ClearSelection();
						heldItem = null;
					}
					ItemsToGrabMenu.receiveLeftClick(x, y);
					if (ItemsToGrabMenu.currentlySelectedItem == -1 && inventory.currentlySelectedItem == -1)
					{
						heldItem = ItemsToGrabMenu.selectItemAt(x, y);
					}
					if (currentlySelectedItem != -1 && ItemsToGrabMenu.currentlySelectedItem == currentlySelectedItem && ItemsToGrabMenu.inventoryItemHeld == currentlySelectedItem)
					{
						ItemsToGrabMenu.ClearSelection();
						heldItem = null;
						return;
					}
					if (ItemsToGrabMenu.inventoryItemHeld != -1 && inventory.currentlySelectedItem == -1)
					{
						ItemsToGrabMenu.currentlySelectedItem = ItemsToGrabMenu.inventoryItemHeld;
						heldItem = ItemsToGrabMenu.actualInventory[ItemsToGrabMenu.currentlySelectedItem];
					}
					if (inventory.currentlySelectedItem == -1 && performSpecialContextChecks(heldItem, x, y))
					{
						return;
					}
				}
				if (heldItem != null && ItemsToGrabMenu.currentlySelectedItem != -1)
				{
					if (inventory.isWithinBounds(x, y))
					{
						int stack = heldItem.Stack;
						Item item2;
						if (ItemsToGrabMenu.currentlyHeldStack != -1)
						{
							try
							{
								Item one2 = heldItem.getOne();
								one2.Stack = ItemsToGrabMenu.currentlyHeldStack;
								item2 = ((ItemsToGrabMenu.currentlyHighlightedEmptySlot == -1) ? inventory.tryToAddItemAt(one2, x, y) : inventory.tryToAddItemToSlotNumber(one2, inventory.currentlyHighlightedEmptySlot, allowStack));
							}
							catch (Exception exception4)
							{
								Log.Exception(exception4, "E");
								return;
							}
						}
						else
						{
							try
							{
								if (behaviorOnItemGrab != null)
								{
									behaviorOnItemGrab(heldItem, Game1.player);
								}
								item2 = ((inventory.currentlyHighlightedEmptySlot == -1) ? inventory.tryToAddItemAt(heldItem, x, y) : inventory.tryToAddItemToSlotNumber(heldItem, inventory.currentlyHighlightedEmptySlot, allowStack));
							}
							catch (Exception exception5)
							{
								Log.Exception(exception5, "F");
								return;
							}
						}
						if (item2 == null)
						{
							try
							{
								if (ItemsToGrabMenu.currentlyHeldStack != -1)
								{
									heldItem.Stack -= ItemsToGrabMenu.currentlyHeldStack;
									if (heldItem.Stack <= 0)
									{
										Utility.removeItemFromInventory(ItemsToGrabMenu.currentlySelectedItem, ItemsToGrabMenu.actualInventory);
									}
								}
								else
								{
									Utility.removeItemFromInventory(ItemsToGrabMenu.currentlySelectedItem, ItemsToGrabMenu.actualInventory);
								}
								if (itemChangeBehavior != null)
								{
									itemChangeBehavior(null, ItemsToGrabMenu.currentlySelectedItem, heldItem, this, onRemoval: true);
								}
								ItemsToGrabMenu.currentlyHeldStack = -1;
								inventory.currentlyHeldStack = -1;
								heldItem = null;
								ItemsToGrabMenu.currentlySelectedItem = -1;
								ItemsToGrabMenu.currentlyStackingItem = -1;
								inventory.currentlySelectedItem = -1;
								inventory.currentlyStackingItem = -1;
								inventory.dragItem = -1;
								ItemsToGrabMenu.dragItem = -1;
								inventory.inventoryItemHeld = -1;
								ItemsToGrabMenu.inventoryItemHeld = -1;
								Game1.playSound("coin");
								return;
							}
							catch (Exception exception6)
							{
								Log.Exception(exception6, "G");
								return;
							}
						}
						if (item2 != null && item2.Stack != stack)
						{
							try
							{
								if (ItemsToGrabMenu.currentlyHeldStack != -1)
								{
									heldItem.Stack = heldItem.Stack - ItemsToGrabMenu.currentlyHeldStack + item2.Stack;
								}
								else
								{
									ItemsToGrabMenu.tryToAddItemToSlotNumber(item2, ItemsToGrabMenu.currentlySelectedItem, allowStack);
								}
								inventory.currentlyHeldStack = -1;
								ItemsToGrabMenu.currentlyHeldStack = -1;
								heldItem = null;
								ItemsToGrabMenu.currentlySelectedItem = -1;
								ItemsToGrabMenu.currentlyStackingItem = -1;
								inventory.currentlySelectedItem = -1;
								inventory.currentlyStackingItem = -1;
								inventory.dragItem = -1;
								ItemsToGrabMenu.dragItem = -1;
								inventory.inventoryItemHeld = -1;
								ItemsToGrabMenu.inventoryItemHeld = -1;
								Game1.playSound("coin");
								return;
							}
							catch (Exception exception7)
							{
								Log.Exception(exception7, "H");
								return;
							}
						}
						if (inventory.tryToAddItem(heldItem) == null)
						{
							try
							{
								Utility.removeItemFromInventory(ItemsToGrabMenu.currentlySelectedItem, ItemsToGrabMenu.actualInventory);
								if (itemChangeBehavior != null)
								{
									itemChangeBehavior(null, ItemsToGrabMenu.currentlySelectedItem, heldItem, this, onRemoval: true);
								}
								inventory.currentlyHeldStack = -1;
								ItemsToGrabMenu.currentlyHeldStack = -1;
								heldItem = null;
								ItemsToGrabMenu.currentlySelectedItem = -1;
								ItemsToGrabMenu.currentlyStackingItem = -1;
								inventory.currentlySelectedItem = -1;
								inventory.currentlyStackingItem = -1;
								inventory.dragItem = -1;
								ItemsToGrabMenu.dragItem = -1;
								inventory.inventoryItemHeld = -1;
								ItemsToGrabMenu.inventoryItemHeld = -1;
								return;
							}
							catch (Exception exception8)
							{
								Log.Exception(exception8, "I");
								return;
							}
						}
						try
						{
							inventory.currentlyHeldStack = -1;
							ItemsToGrabMenu.currentlyHeldStack = -1;
							heldItem = null;
							ItemsToGrabMenu.currentlySelectedItem = -1;
							ItemsToGrabMenu.currentlyStackingItem = -1;
							inventory.currentlySelectedItem = -1;
							inventory.currentlyStackingItem = -1;
							inventory.dragItem = -1;
							ItemsToGrabMenu.dragItem = -1;
							inventory.inventoryItemHeld = -1;
							ItemsToGrabMenu.inventoryItemHeld = -1;
							return;
						}
						catch (Exception exception9)
						{
							Log.Exception(exception9, "J");
							return;
						}
					}
				}
				else if (heldItem != null && ItemsToGrabMenu.currentlySelectedItem == -1 && inventory.currentlySelectedItem != -1 && (reverseGrab || behaviorFunction != null) && !shippingBin && ItemsToGrabMenu.isWithinBounds(x, y))
				{
					int stack2 = heldItem.Stack;
					Item item3;
					if (allowStack && inventory.currentlyHeldStack != -1)
					{
						try
						{
							Item one3 = heldItem.getOne();
							one3.Stack = inventory.currentlyHeldStack;
							item3 = ((ItemsToGrabMenu.currentlyHighlightedEmptySlot == -1) ? ItemsToGrabMenu.tryToAddItemAt(one3, x, y) : ItemsToGrabMenu.tryToAddItemToSlotNumber(one3, ItemsToGrabMenu.currentlyHighlightedEmptySlot, allowStack));
							if (item3 == one3 && !Utility.canItemBeAddedToThisInventoryList(item3, ItemsToGrabMenu.actualInventory, ItemsToGrabMenu.actualInventory.Count))
							{
								item3.Stack = stack2;
							}
						}
						catch (Exception exception10)
						{
							Log.Exception(exception10, "K");
							return;
						}
					}
					else
					{
						try
						{
							item3 = ((ItemsToGrabMenu.currentlyHighlightedEmptySlot == -1) ? ItemsToGrabMenu.tryToAddItemAt(heldItem, x, y, allowStack) : ItemsToGrabMenu.tryToAddItemToSlotNumber(heldItem, ItemsToGrabMenu.currentlyHighlightedEmptySlot, allowStack));
						}
						catch (Exception exception11)
						{
							Log.Exception(exception11, "L");
							return;
						}
					}
					if (item3 == null)
					{
						try
						{
							if (itemChangeBehavior != null)
							{
								itemChangeBehavior(ItemsToGrabMenu.getItemAt(x, y), ItemsToGrabMenu.getInventoryPositionOfClick(x, y), null, this);
							}
							if (inventory.currentlyHeldStack != -1)
							{
								heldItem.Stack -= inventory.currentlyHeldStack;
								if (heldItem.Stack <= 0)
								{
									Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
								}
							}
							else
							{
								Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
							}
							ItemsToGrabMenu.currentlyHeldStack = -1;
							inventory.currentlyHeldStack = -1;
							ItemsToGrabMenu.currentlySelectedItem = -1;
							ItemsToGrabMenu.currentlyStackingItem = -1;
							inventory.currentlySelectedItem = -1;
							inventory.currentlyStackingItem = -1;
							inventory.dragItem = -1;
							ItemsToGrabMenu.dragItem = -1;
							inventory.inventoryItemHeld = -1;
							ItemsToGrabMenu.inventoryItemHeld = -1;
							Game1.playSound("coin");
							if (reverseGrab && behaviorFunction != null)
							{
								behaviorFunction(heldItem, Game1.player);
							}
							if (itemChangeBehavior != null)
							{
								itemChangeBehavior(ItemsToGrabMenu.getItemAt(x, y), ItemsToGrabMenu.getInventoryPositionOfClick(x, y), null, this);
							}
							if (behaviorOnAddtoItemsToGrab != null)
							{
								behaviorOnAddtoItemsToGrab(heldItem, Game1.player);
							}
							heldItem = null;
							return;
						}
						catch (Exception exception12)
						{
							Log.Exception(exception12, "M");
							return;
						}
					}
					if (allowStack && item3 != null && item3.Stack != stack2)
					{
						try
						{
							if (inventory.currentlyHeldStack != -1)
							{
								heldItem.Stack = heldItem.Stack - inventory.currentlyHeldStack + item3.Stack;
							}
							inventory.currentlyHeldStack = -1;
							ItemsToGrabMenu.currentlyHeldStack = -1;
							ItemsToGrabMenu.currentlySelectedItem = -1;
							ItemsToGrabMenu.currentlyStackingItem = -1;
							inventory.currentlySelectedItem = -1;
							inventory.currentlyStackingItem = -1;
							inventory.dragItem = -1;
							ItemsToGrabMenu.dragItem = -1;
							inventory.inventoryItemHeld = -1;
							ItemsToGrabMenu.inventoryItemHeld = -1;
							Game1.playSound("coin");
							if (behaviorFunction != null)
							{
								behaviorFunction(heldItem, Game1.player);
							}
							if (itemChangeBehavior != null)
							{
								itemChangeBehavior(ItemsToGrabMenu.getItemAt(x, y), ItemsToGrabMenu.getInventoryPositionOfClick(x, y), null, this);
							}
							if (behaviorOnAddtoItemsToGrab != null)
							{
								behaviorOnAddtoItemsToGrab(heldItem, Game1.player);
							}
							heldItem = null;
							return;
						}
						catch (Exception exception13)
						{
							Log.Exception(exception13, "N");
							return;
						}
					}
					if (!allowStack && item3 != null && item3.Stack != stack2)
					{
						try
						{
							inventory.currentlyHeldStack = -1;
							ItemsToGrabMenu.currentlyHeldStack = -1;
							ItemsToGrabMenu.currentlySelectedItem = -1;
							ItemsToGrabMenu.currentlyStackingItem = -1;
							inventory.currentlySelectedItem = -1;
							inventory.currentlyStackingItem = -1;
							inventory.dragItem = -1;
							ItemsToGrabMenu.dragItem = -1;
							inventory.inventoryItemHeld = -1;
							ItemsToGrabMenu.inventoryItemHeld = -1;
							Game1.playSound("coin");
							if (behaviorFunction != null)
							{
								behaviorFunction(heldItem, Game1.player);
							}
							if (itemChangeBehavior != null)
							{
								itemChangeBehavior(ItemsToGrabMenu.getItemAt(x, y), ItemsToGrabMenu.getInventoryPositionOfClick(x, y), null, this);
							}
							if (behaviorOnAddtoItemsToGrab != null)
							{
								behaviorOnAddtoItemsToGrab(heldItem, Game1.player);
							}
							heldItem = null;
							return;
						}
						catch (Exception exception14)
						{
							Log.Exception(exception14, "O");
							return;
						}
					}
					if (allowStack && ItemsToGrabMenu.getInventoryPositionOfClick(x, y) != -1 && ItemsToGrabMenu.tryToAddItem(heldItem) == null)
					{
						try
						{
							Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
							inventory.currentlyHeldStack = -1;
							ItemsToGrabMenu.currentlyHeldStack = -1;
							heldItem = null;
							ItemsToGrabMenu.currentlySelectedItem = -1;
							ItemsToGrabMenu.currentlyStackingItem = -1;
							inventory.currentlySelectedItem = -1;
							inventory.currentlyStackingItem = -1;
							inventory.dragItem = -1;
							ItemsToGrabMenu.dragItem = -1;
							inventory.inventoryItemHeld = -1;
							ItemsToGrabMenu.inventoryItemHeld = -1;
							return;
						}
						catch (Exception exception15)
						{
							Log.Exception(exception15, "P");
							return;
						}
					}
					if (!allowStack && item3 != null)
					{
						try
						{
							inventory.currentlyHeldStack = -1;
							ItemsToGrabMenu.currentlyHeldStack = -1;
							ItemsToGrabMenu.currentlySelectedItem = -1;
							ItemsToGrabMenu.currentlyStackingItem = -1;
							inventory.currentlySelectedItem = -1;
							inventory.currentlyStackingItem = -1;
							inventory.dragItem = -1;
							ItemsToGrabMenu.dragItem = -1;
							inventory.inventoryItemHeld = -1;
							ItemsToGrabMenu.inventoryItemHeld = -1;
							Game1.playSound("coin");
							if (behaviorFunction != null)
							{
								behaviorFunction(heldItem, Game1.player);
							}
							if (itemChangeBehavior != null)
							{
								itemChangeBehavior(ItemsToGrabMenu.getItemAt(x, y), ItemsToGrabMenu.getInventoryPositionOfClick(x, y), null, this);
							}
							heldItem = null;
							return;
						}
						catch (Exception exception16)
						{
							Log.Exception(exception16, "Q");
							return;
						}
					}
				}
				base.receiveLeftClick(x, y, (!destroyItemOnClick) ? true : false);
			}
			if (chestColorPicker != null)
			{
				chestColorPicker.receiveLeftClick(x, y);
				if (sourceItem != null && sourceItem is Chest)
				{
					(sourceItem as Chest).playerChoiceColor.Value = chestColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
				}
			}
			if (colorPickerToggleButton != null && colorPickerToggleButton.containsPoint(x, y))
			{
				try
				{
					Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
					if (Game1.player.showChestColorPicker)
					{
						colorPickerToggleButton.bounds.X -= 4;
						colorPickerToggleButton.bounds.Y += 4;
						colorPickerToggleButton.drawShadow = false;
					}
					else
					{
						colorPickerToggleButton.bounds.X += 4;
						colorPickerToggleButton.bounds.Y -= 4;
						colorPickerToggleButton.drawShadow = true;
					}
					chestColorPicker.visible = Game1.player.showChestColorPicker;
					try
					{
						Game1.playSound("drumkit6");
					}
					catch (Exception)
					{
					}
				}
				catch (Exception exception17)
				{
					Log.Exception(exception17, "R");
					return;
				}
			}
			if (whichSpecialButton != -1 && specialButton != null && specialButton.containsPoint(x, y))
			{
				try
				{
					Game1.playSound("drumkit6");
					int num = whichSpecialButton;
					if (num == 1 && context != null && context is JunimoHut)
					{
						(context as JunimoHut).noHarvest.Value = !(context as JunimoHut).noHarvest;
						specialButton.sourceRect.X = ((context as JunimoHut).noHarvest ? 124 : 108);
					}
				}
				catch (Exception exception18)
				{
					Log.Exception(exception18, "S");
					return;
				}
			}
			if (fillStacksButton != null && fillStacksButton.containsPoint(x, y))
			{
				holdingFillStacksButton = true;
				fillStacksButton.drawShadow = false;
				Game1.playSound("smallSelect");
				fillStacksButton.bounds.X -= 4;
				fillStacksButton.bounds.Y += 4;
				inventory.ClearSelection();
				ItemsToGrabMenu.ClearSelection();
				return;
			}
			if (heldItem == null && showReceivingMenu && (chestColorPicker == null || (chestColorPicker != null && !chestColorPicker.visible)))
			{
				try
				{
					if (ItemsToGrabMenu.isWithinBounds(x, y))
					{
						heldItem = ItemsToGrabMenu.selectItemAt(x, y);
						if (heldItem != null)
						{
							inventory.currentlySelectedItem = -1;
							inventory.currentlyStackingItem = -1;
							inventory.dragItem = -1;
						}
					}
					if (heldItem != null)
					{
						_ = behaviorOnItemGrab;
					}
				}
				catch (Exception exception19)
				{
					Log.Exception(exception19, "T");
					return;
				}
				if (Utility.IsNormalObjectAtParentSheetIndex(heldItem, 326))
				{
					try
					{
						heldItem = null;
						Game1.player.canUnderstandDwarves = true;
						Game1.playSound("fireball");
						Rectangle iconBoundsAt = ItemsToGrabMenu.getIconBoundsAt(x, y);
						int num2 = iconBoundsAt.X + (iconBoundsAt.Width - 64) / 2;
						int num3 = iconBoundsAt.Y + (iconBoundsAt.Height - 64) / 2;
						poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(num2, num3), flicker: false, flipped: false);
						Utility.removeItemFromInventory(ItemsToGrabMenu.currentlySelectedItem, ItemsToGrabMenu.actualInventory);
						ItemsToGrabMenu.currentlySelectedItem = -1;
						ItemsToGrabMenu.currentlyStackingItem = -1;
						ItemsToGrabMenu.dragItem = -1;
					}
					catch (Exception exception20)
					{
						Log.Exception(exception20, "U");
						return;
					}
				}
				else if (heldItem is Object && (int)(heldItem as Object).parentSheetIndex == 102)
				{
					try
					{
						heldItem = null;
						Game1.player.foundArtifact(102, 1);
						Game1.playSound("fireball");
						Rectangle iconBoundsAt2 = ItemsToGrabMenu.getIconBoundsAt(x, y);
						int num4 = iconBoundsAt2.X + (iconBoundsAt2.Width - 64) / 2;
						int num5 = iconBoundsAt2.Y + (iconBoundsAt2.Height - 64) / 2;
						poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(num4, num5), flicker: false, flipped: false);
						Utility.removeItemFromInventory(ItemsToGrabMenu.currentlySelectedItem, ItemsToGrabMenu.actualInventory);
						ItemsToGrabMenu.currentlySelectedItem = -1;
						ItemsToGrabMenu.currentlyStackingItem = -1;
						ItemsToGrabMenu.dragItem = -1;
					}
					catch (Exception exception21)
					{
						Log.Exception(exception21, "V");
						return;
					}
				}
				else if (heldItem is Object && (bool)(heldItem as Object).isRecipe)
				{
					try
					{
						string key = heldItem.Name.Substring(0, heldItem.Name.IndexOf("Recipe") - 1);
						if ((heldItem as Object).Category == -7)
						{
							if (!Game1.player.cookingRecipes.ContainsKey(key))
							{
								Game1.player.cookingRecipes.Add(key, 0);
							}
						}
						else if (!Game1.player.craftingRecipes.ContainsKey(key))
						{
							Game1.player.craftingRecipes.Add(key, 0);
						}
						Game1.playSound("newRecipe");
						Rectangle iconBoundsAt3 = ItemsToGrabMenu.getIconBoundsAt(x, y);
						int num6 = iconBoundsAt3.X + (iconBoundsAt3.Width - 64) / 2;
						int num7 = iconBoundsAt3.Y + (iconBoundsAt3.Height - 64) / 2;
						poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(num6, num7), flicker: false, flipped: false);
						Utility.removeItemFromInventory(ItemsToGrabMenu.currentlySelectedItem, ItemsToGrabMenu.actualInventory);
						ItemsToGrabMenu.currentlySelectedItem = -1;
						ItemsToGrabMenu.currentlyStackingItem = -1;
						ItemsToGrabMenu.dragItem = -1;
						heldItem = null;
					}
					catch (Exception exception22)
					{
						Log.Exception(exception22, "W");
						return;
					}
				}
			}
			try
			{
				if (inventory.currentlySelectedItem == -1 && heldItem != null && ItemsToGrabMenu.currentlySelectedItem != -1 && inventory.addItemAt(heldItem, x, y))
				{
					Utility.removeItemFromInventory(ItemsToGrabMenu.currentlySelectedItem, ItemsToGrabMenu.actualInventory);
					ItemsToGrabMenu.currentlySelectedItem = -1;
					ItemsToGrabMenu.currentlyStackingItem = -1;
				}
				if (inventory.currentlySelectedItem != -1)
				{
					ItemsToGrabMenu.currentlySelectedItem = -1;
					ItemsToGrabMenu.currentlyStackingItem = -1;
				}
			}
			catch (Exception exception23)
			{
				Log.Exception(exception23, "X");
			}
		}

		public void FillOutStacks()
		{
			for (int i = 0; i < ItemsToGrabMenu.actualInventory.Count; i++)
			{
				Item item = ItemsToGrabMenu.actualInventory[i];
				if (item == null || item.maximumStackSize() <= 1)
				{
					continue;
				}
				for (int j = 0; j < inventory.actualInventory.Count; j++)
				{
					Item item2 = inventory.actualInventory[j];
					if (item2 == null || !item.canStackWith(item2))
					{
						continue;
					}
					TransferredItemSprite item3 = new TransferredItemSprite(item2.getOne(), inventory.inventory[j].bounds.X, inventory.inventory[j].bounds.Y);
					_transferredItemSprites.Add(item3);
					int stack = item2.Stack;
					if (item.getRemainingStackSpace() > 0)
					{
						stack = item.addToStack(item2);
						ItemsToGrabMenu.ShakeItem(item);
					}
					item2.Stack = stack;
					while (item2.Stack > 0)
					{
						Item item4 = null;
						if (!Utility.canItemBeAddedToThisInventoryList(item.getOne(), ItemsToGrabMenu.actualInventory, ItemsToGrabMenu.capacity))
						{
							break;
						}
						if (item4 == null)
						{
							for (int k = 0; k < ItemsToGrabMenu.actualInventory.Count; k++)
							{
								if (ItemsToGrabMenu.actualInventory[k] != null && ItemsToGrabMenu.actualInventory[k].canStackWith(item) && ItemsToGrabMenu.actualInventory[k].getRemainingStackSpace() > 0)
								{
									item4 = ItemsToGrabMenu.actualInventory[k];
									break;
								}
							}
						}
						if (item4 == null)
						{
							for (int l = 0; l < ItemsToGrabMenu.actualInventory.Count; l++)
							{
								if (ItemsToGrabMenu.actualInventory[l] == null)
								{
									Item item5 = (ItemsToGrabMenu.actualInventory[l] = item.getOne());
									item4 = item5;
									item4.Stack = 0;
									break;
								}
							}
						}
						if (item4 == null && ItemsToGrabMenu.actualInventory.Count < ItemsToGrabMenu.capacity)
						{
							item4 = item.getOne();
							item4.Stack = 0;
							ItemsToGrabMenu.actualInventory.Add(item4);
						}
						if (item4 == null)
						{
							break;
						}
						stack = item4.addToStack(item2);
						ItemsToGrabMenu.ShakeItem(item4);
						item2.Stack = stack;
					}
					if (item2.Stack == 0)
					{
						inventory.actualInventory[j] = null;
					}
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			_ = allowRightClick;
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				applyMovementKey(key);
			}
			if ((canExitOnKey || areAllItemsTaken()) && Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
			{
				exitThisMenu();
				if (Game1.currentLocation.currentEvent != null)
				{
					Game1.currentLocation.currentEvent.CurrentCommand++;
				}
			}
			else if (Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				_ = heldItem;
			}
			if (key == Keys.Delete && heldItem != null && heldItem.canBeTrashed())
			{
				if (heldItem is Object && Game1.player.specialItems.Contains((heldItem as Object).parentSheetIndex))
				{
					Game1.player.specialItems.Remove((heldItem as Object).parentSheetIndex);
				}
				heldItem = null;
				Game1.playSound("trashcan");
			}
		}

		public static void organizeItemsInList(IList<Item> items)
		{
			List<Item> list = new List<Item>(items);
			List<Item> list2 = new List<Item>();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == null)
				{
					list.RemoveAt(i);
					i--;
				}
				else if (list[i] is Tool)
				{
					list2.Add(list[i]);
					list.RemoveAt(i);
					i--;
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				Item item = list[j];
				if (item.getRemainingStackSpace() <= 0)
				{
					continue;
				}
				for (int k = j + 1; k < list.Count; k++)
				{
					Item item2 = list[k];
					if (item.canStackWith(item2))
					{
						item2.Stack = item.addToStack(item2);
						if (item2.Stack == 0)
						{
							list.RemoveAt(k);
							k--;
						}
					}
				}
			}
			list.Sort();
			list.InsertRange(0, list2);
			for (int l = 0; l < items.Count; l++)
			{
				items[l] = null;
			}
			for (int m = 0; m < list.Count; m++)
			{
				items[m] = list[m];
			}
		}

		public bool areAllItemsTaken()
		{
			for (int i = 0; i < ItemsToGrabMenu.actualInventory.Count; i++)
			{
				if (ItemsToGrabMenu.actualInventory[i] != null)
				{
					return false;
				}
			}
			return true;
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (poof != null && poof.update(time))
			{
				poof = null;
			}
			if (chestColorPicker != null)
			{
				chestColorPicker.update(time);
			}
			if (sourceItem != null && sourceItem is Chest && _sourceItemInCurrentLocation)
			{
				Vector2 vector = (sourceItem as Object).tileLocation;
				if (vector != Vector2.Zero && !Game1.currentLocation.objects.ContainsKey(vector))
				{
					if (Game1.activeClickableMenu != null)
					{
						Game1.activeClickableMenu.emergencyShutDown();
					}
					Game1.exitActiveMenu();
				}
			}
			for (int i = 0; i < _transferredItemSprites.Count; i++)
			{
				TransferredItemSprite transferredItemSprite = _transferredItemSprites[i];
				if (transferredItemSprite.Update(time))
				{
					_transferredItemSprites.RemoveAt(i);
					i--;
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
			if (shippingBin)
			{
				hoverText = null;
			}
			if (chestColorPicker != null)
			{
				chestColorPicker.performHoverAction(x, y);
			}
		}

		private bool performSpecialContextChecks(Item item, int tap_x, int tap_y)
		{
			if (item != null && (context is JunimoNoteMenu || context is CommunityCenter || context is LibraryMuseum) && inventory.dragItem == -1 && ItemsToGrabMenu.dragItem == -1)
			{
				if (item is Object && (bool)(item as Object).isRecipe)
				{
					string key = item.Name.Substring(0, item.Name.IndexOf("Recipe") - 1);
					if ((item as Object).Category == -7)
					{
						if (!Game1.player.cookingRecipes.ContainsKey(key))
						{
							Game1.player.cookingRecipes.Add(key, 0);
						}
					}
					else if (!Game1.player.craftingRecipes.ContainsKey(key))
					{
						Game1.player.craftingRecipes.Add(key, 0);
					}
					Game1.playSound("newRecipe");
					Rectangle iconBoundsAt = ItemsToGrabMenu.getIconBoundsAt(tap_x, tap_y);
					int num = iconBoundsAt.X + (iconBoundsAt.Width - 64) / 2;
					int num2 = iconBoundsAt.Y + (iconBoundsAt.Height - 64) / 2;
					poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(num, num2), flicker: false, flipped: false);
					if (ItemsToGrabMenu.inventoryItemHeld != -1)
					{
						Utility.removeItemFromInventory(ItemsToGrabMenu.inventoryItemHeld, ItemsToGrabMenu.actualInventory);
					}
					else if (ItemsToGrabMenu.currentlySelectedItem != -1)
					{
						Utility.removeItemFromInventory(ItemsToGrabMenu.currentlySelectedItem, ItemsToGrabMenu.actualInventory);
					}
					ItemsToGrabMenu.currentlySelectedItem = -1;
					ItemsToGrabMenu.currentlyStackingItem = -1;
					ItemsToGrabMenu.dragItem = -1;
					behaviorOnItemGrab?.Invoke(item, Game1.player);
					return true;
				}
				if (Utility.IsNormalObjectAtParentSheetIndex(item, 326))
				{
					try
					{
						behaviorOnItemGrab?.Invoke(item, Game1.player);
						heldItem = null;
						Game1.player.canUnderstandDwarves = true;
						Game1.playSound("fireball");
						Rectangle iconBoundsAt2 = ItemsToGrabMenu.getIconBoundsAt(tap_x, tap_y);
						int num3 = iconBoundsAt2.X + (iconBoundsAt2.Width - 64) / 2;
						int num4 = iconBoundsAt2.Y + (iconBoundsAt2.Height - 64) / 2;
						poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(num3, num4), flicker: false, flipped: false);
						if (ItemsToGrabMenu.currentlySelectedItem != -1)
						{
							Utility.removeItemFromInventory(ItemsToGrabMenu.currentlySelectedItem, ItemsToGrabMenu.actualInventory);
						}
						else if (ItemsToGrabMenu.actualInventory.IndexOf(item) != -1)
						{
							ItemsToGrabMenu.actualInventory[ItemsToGrabMenu.actualInventory.IndexOf(item)] = null;
						}
						ItemsToGrabMenu.currentlySelectedItem = -1;
						ItemsToGrabMenu.currentlyStackingItem = -1;
						ItemsToGrabMenu.dragItem = -1;
						return true;
					}
					catch (Exception exception)
					{
						Log.Exception(exception, "U");
						return false;
					}
				}
				if (item is Object && Utility.IsNormalObjectAtParentSheetIndex(item, 434))
				{
					Object @object = item as Object;
					behaviorOnItemGrab?.Invoke(@object, Game1.player);
					item = null;
					exitThisMenu(playSound: false);
					Game1.player.eatObject(@object, overrideFullness: true);
					return true;
				}
				if (item != null && Utility.canItemBeAddedToThisInventoryList(item, inventory.actualInventory))
				{
					behaviorOnItemGrab?.Invoke(item, Game1.player);
					Utility.addItemToThisInventoryList(item, inventory.actualInventory);
					Game1.player.fakeAddItemToInventoryBool(item);
					Utility.removeItemFromInventory(ItemsToGrabMenu.inventoryItemHeld, ItemsToGrabMenu.actualInventory);
					heldItem = null;
					inventory.ClearSelection();
					ItemsToGrabMenu.ClearSelection();
				}
			}
			return false;
		}

		public override void leftClickHeld(int x, int y)
		{
			Item itemAt = ItemsToGrabMenu.GetItemAt(ItemsToGrabMenu.inventoryItemHeld);
			if (ItemsToGrabMenu.inventoryItemHeld != -1 && performSpecialContextChecks(itemAt, x, y))
			{
				return;
			}
			ItemsToGrabMenu.leftClickHeld(x, y);
			inventory.leftClickHeld(x, y);
			if (ItemsToGrabMenu.dragItem != -1)
			{
				if (inventory.trashCan != null && inventory.trashCan.bounds.Contains(x, y))
				{
					inventory.doOpenTrashCan(ItemsToGrabMenu.actualInventory, ItemsToGrabMenu.dragItem, ItemsToGrabMenu.currentlyHeldStack);
					return;
				}
				heldItem = ItemsToGrabMenu.actualInventory[ItemsToGrabMenu.dragItem];
				if (heldItem is Object && (bool)(heldItem as Object).isRecipe)
				{
					ItemsToGrabMenu.dragItem = -1;
				}
				inventory.highlightIfHoverOverSlot(x, y, itemFromOtherInventory: true, heldItem, allowStack);
			}
			else if (inventory.dragItem != -1 && (reverseGrab || behaviorFunction != null) && ItemsToGrabMenu.isWithinBounds(x, y))
			{
				heldItem = inventory.actualInventory[inventory.dragItem];
				ItemsToGrabMenu.highlightIfHoverOverSlot(x, y, itemFromOtherInventory: true, heldItem, allowStack);
			}
			if (shippingBin)
			{
				if (topInv.Contains(x, y) && inventory.dragItem != -1)
				{
					hoverOverShippingBin = true;
				}
				else
				{
					hoverOverShippingBin = false;
				}
			}
			if (fillStacksButton != null && holdingFillStacksButton && !fillStacksButton.containsPoint(x, y))
			{
				holdingFillStacksButton = false;
				fillStacksButton.drawShadow = true;
				fillStacksButton.bounds.X += 4;
				fillStacksButton.bounds.Y -= 4;
			}
			base.leftClickHeld(x, y);
		}

		public override void releaseLeftClick(int x, int y)
		{
			hoverOverShippingBin = false;
			if (ItemsToGrabMenu.dragItem != -1)
			{
				if (inventory.trashCan.containsPoint(x, y))
				{
					inventory.releaseLeftClick(x, y);
					ItemsToGrabMenu.currentlyHeldStack = -1;
					ItemsToGrabMenu.currentlyHighlightedEmptySlot = -1;
					ItemsToGrabMenu.inventoryItemHeld = -1;
					ItemsToGrabMenu.currentlySelectedItem = -1;
					ItemsToGrabMenu.currentlyStackingItem = -1;
					ItemsToGrabMenu.dragItem = -1;
					return;
				}
				receiveLeftClick(x, y);
			}
			else if (inventory.dragItem != -1)
			{
				receiveLeftClick(x, y);
			}
			base.releaseLeftClick(x, y);
			if (fillStacksButton != null && holdingFillStacksButton && fillStacksButton.containsPoint(x, y))
			{
				holdingFillStacksButton = false;
				fillStacksButton.drawShadow = true;
				fillStacksButton.bounds.X = ItemsToGrabMenu.orgX;
				fillStacksButton.bounds.Y = ItemsToGrabMenu.getInvY() + ItemsToGrabMenu.getInvHeight() / 2 - 32;
				FillOutStacks();
				Game1.playSound("Ship");
			}
			if (!shippingBin)
			{
				ItemsToGrabMenu.releaseLeftClick(x, y);
				ItemsToGrabMenu.dragItem = -1;
				ItemsToGrabMenu.showItemInfo = false;
				ItemsToGrabMenu.currentlyHighlightedEmptySlot = -1;
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (upperRightCloseButton != null)
			{
				upperRightCloseButton.bounds.X = IClickableMenu.viewport.Width - 68 - Game1.xEdge;
			}
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			IClickableMenu.drawTextureBox(b, bottomInv.X, bottomInv.Y, bottomInv.Width, bottomInv.Height, Color.White);
			base.draw(b, drawUpperPortion: false, drawDescriptionArea: false);
			if (showReceivingMenu)
			{
				IClickableMenu.drawTextureBox(b, topInv.X, topInv.Y, topInv.Width, topInv.Height, Color.White);
				ItemsToGrabMenu.draw(b);
			}
			else if (message != null)
			{
				Game1.drawDialogueBox(Game1.uiViewport.Width / 2, ItemsToGrabMenu.yPositionOnScreen + ItemsToGrabMenu.height / 2, speaker: false, drawOnlyBox: false, message);
			}
			if (upperRightCloseButton != null)
			{
				upperRightCloseButton.draw(b, Color.White, -1f);
			}
			if (poof != null)
			{
				poof.draw(b, localPosition: true);
			}
			foreach (TransferredItemSprite transferredItemSprite in _transferredItemSprites)
			{
				transferredItemSprite.Draw(b);
			}
			if (shippingBin)
			{
				IClickableMenu.drawTextureBox(b, topInv.X, topInv.Y, topInv.Width, topInv.Height, hoverOverShippingBin ? Color.Wheat : Color.White);
				if (!showReceivingMenu)
				{
					base.draw(b, drawUpperPortion: false, drawDescriptionArea: false);
				}
				clickableShippingCrate.draw(b);
				clickableCrateLid.draw(b);
				if (Game1.getFarm().lastItemShipped != null)
				{
					lastShippedHolder.draw(b);
					Game1.getFarm().lastItemShipped.drawInMenu(b, new Vector2(lastShippedHolder.bounds.X + 16, lastShippedHolder.bounds.Y + 16), 1f);
					b.Draw(Game1.mouseCursors, new Vector2(lastShippedHolder.bounds.X + -8, lastShippedHolder.bounds.Bottom - 100), new Rectangle(325, 448, 5, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					b.Draw(Game1.mouseCursors, new Vector2(lastShippedHolder.bounds.X + 84, lastShippedHolder.bounds.Bottom - 100), new Rectangle(325, 448, 5, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					b.Draw(Game1.mouseCursors, new Vector2(lastShippedHolder.bounds.X + -8, lastShippedHolder.bounds.Bottom - 44), new Rectangle(325, 452, 5, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					b.Draw(Game1.mouseCursors, new Vector2(lastShippedHolder.bounds.X + 84, lastShippedHolder.bounds.Bottom - 44), new Rectangle(325, 452, 5, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					Utility.drawMultiLineTextWithShadow(b, lastShippedText, Game1.smallFont, new Vector2(topInv.X, topInv.Y + topInv.Height - 96), topInv.Width, 80, Game1.textColor, centreY: true, actuallyDrawIt: true, drawShadows: true, centerX: true, bold: false, close: true);
				}
				else
				{
					Utility.drawMultiLineTextWithShadow(b, shippingInstructionText, Game1.smallFont, new Vector2(topInv.X + topInv.Width / 2, topInv.Y + 16), topInv.Width / 2 - 32, topInv.Height - 32, Game1.textColor);
				}
			}
			if (colorPickerToggleButton != null)
			{
				colorPickerToggleButton.draw(b);
			}
			else if (specialButton != null)
			{
				specialButton.draw(b);
			}
			if (fillStacksButton != null)
			{
				fillStacksButton.draw(b);
			}
			if (chestColorPicker != null)
			{
				chestColorPicker.draw(b);
			}
			if (organizeButton != null)
			{
				organizeButton.draw(b);
			}
			if (hoverText != null && (hoveredItem == null || hoveredItem == null || ItemsToGrabMenu == null))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
			if (hoveredItem != null)
			{
				IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem, heldItem != null);
			}
			else if (hoveredItem != null && ItemsToGrabMenu != null)
			{
				IClickableMenu.drawToolTip(b, ItemsToGrabMenu.descriptionText, ItemsToGrabMenu.descriptionTitle, hoveredItem, heldItem != null);
			}
			Game1.mouseCursorTransparency = 1f;
			inventory.drawDragItem(b);
			ItemsToGrabMenu.drawDragItem(b);
			inventory.drawInfoPanel(b, force: true);
			ItemsToGrabMenu.drawInfoPanel(b, force: true);
			drawMouse(b);
		}

		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			Console.WriteLine("ItemGrabMenu.emergencyShutDown");
			if (heldItem != null)
			{
				Console.WriteLine("Taking " + heldItem.Name);
				heldItem = Game1.player.addItemToInventory(heldItem);
			}
			if (heldItem != null)
			{
				Game1.playSound("throwDownITem");
				Console.WriteLine("Dropping " + heldItem.Name);
				Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
				heldItem = null;
			}
			if (essential)
			{
				Console.WriteLine("essential");
				{
					foreach (Item item2 in ItemsToGrabMenu.actualInventory)
					{
						if (item2 != null)
						{
							Console.WriteLine("Taking " + item2.Name);
							Item item = Game1.player.addItemToInventory(item2);
							if (item != null)
							{
								Console.WriteLine("Dropping " + item.Name);
								Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
							}
						}
					}
					return;
				}
			}
			Console.WriteLine("essential");
		}

		private void rearrangeGrange()
		{
			if (ItemsToGrabMenu.inventory.Count != 9)
			{
				return;
			}
			Game1.player.team.grangeDisplay.Clear();
			while (Game1.player.team.grangeDisplay.Count < 9)
			{
				Game1.player.team.grangeDisplay.Add(null);
			}
			for (int i = 0; i < ItemsToGrabMenu.inventory.Count; i++)
			{
				Item itemFromClickableComponent = ItemsToGrabMenu.getItemFromClickableComponent(ItemsToGrabMenu.inventory[i]);
				if (itemFromClickableComponent != null)
				{
					Game1.player.team.grangeDisplay[i] = itemFromClickableComponent;
				}
			}
		}

		private void rearrange(int rows, int columns)
		{
			if (rows < 3 || columns < 12)
			{
				int num = yPositionOnScreen;
				if (columns < 12)
				{
					topInv.Width = ItemsToGrabMenu.squareSide * columns + ItemsToGrabMenu.hGap * (columns - 1) + ItemsToGrabMenu.xOffset * 2;
					topInv.X = xPositionOnScreen + (width - topInv.Width) / 2;
				}
				if (rows < 3)
				{
					topInv.Height = ItemsToGrabMenu.squareSide * rows + ItemsToGrabMenu.verticalGap * (rows - 1) + ItemsToGrabMenu.yOffset * 2;
					topInv.Y = yPositionOnScreen + (height / 2 - topInv.Height);
				}
				ItemsToGrabMenu.movePosition(topInv.X - Game1.xEdge, topInv.Y - num);
			}
		}
	}
}
