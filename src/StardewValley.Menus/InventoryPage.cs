using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.Menus
{
	public class InventoryPage : IClickableMenu
	{
		public const int region_inventory = 100;

		public const int region_hat = 101;

		public const int region_ring1 = 102;

		public const int region_ring2 = 103;

		public const int region_boots = 104;

		public const int region_trashCan = 105;

		public const int region_organizeButton = 106;

		public const int region_accessory = 107;

		public const int region_shirt = 108;

		public const int region_pants = 109;

		public const int region_shoes = 110;

		public InventoryMenu inventory;

		private string descriptionText = "";

		private string hoverText = "";

		private string descriptionTitle = "";

		private string hoverTitle = "";

		private Item heldItem;

		private Item hoveredItem;

		public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

		public ClickableComponent portrait;

		public ClickableTextureComponent trashCan;

		public ClickableTextureComponent organizeButton;

		private float trashCanLidRotation;

		public ClickableTextureComponent junimoNoteIcon;

		private int junimoNotePulser;

		private string headerText = "";

		private int offset = 8;

		private int portraitX = 160;

		private int portraitY = 464;

		private int portraitHeight;

		private int portraitWidth;

		private int equipmentIconSize;

		private int bottomBoxY;

		private int bottomBoxHeight;

		private float widthMod;

		private float heightMod;

		private float scaleFactor;

		private int highlightEquipmentIcon = -1;

		public InventoryPage(int x, int y, int width, int height)
			: base(x, y, width, height)
		{
			widthMod = (float)width / 1280f;
			heightMod = (float)height / (720f - (float)GameMenu.tabHeight);
			scaleFactor = Math.Min(widthMod, heightMod);
			headerText = Game1.content.LoadString("Strings\\UI:GameMenu_Inventory");
			portraitX = (int)((float)x + 200f * widthMod);
			bottomBoxY = yPositionOnScreen + height / 2;
			bottomBoxHeight = height - bottomBoxY + GameMenu.tabHeight;
			portraitHeight = 192;
			portraitWidth = 128;
			portraitY = Math.Max(bottomBoxY + 28, bottomBoxY + (height / 2 - portraitHeight) / 2);
			inventory = new InventoryMenu(xPositionOnScreen, (int)((double)yPositionOnScreen + (double)offset * 1.5), playerInventory: true, null, null, -1, 3, 0, 0, drawSlots: true, width, height / 2 - offset)
			{
				showGrayedOutSlots = true
			};
			equipmentIconSize = 64;
			equipmentIcons.Add(new ClickableComponent(new Rectangle(portraitX - (int)((float)equipmentIconSize + (float)(equipmentIconSize / 2) * widthMod), portraitY, equipmentIconSize, equipmentIconSize), "Hat")
			{
				myID = 101,
				downNeighborID = 108,
				rightNeighborID = 102,
				upNeighborID = 1 + (Game1.player.MaxItems - 1) / 12 * 12,
				upNeighborImmutable = true
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(portraitX - (int)((float)equipmentIconSize + (float)(equipmentIconSize / 2) * widthMod), portraitY + portraitHeight - equipmentIconSize, equipmentIconSize, equipmentIconSize), "Left Ring")
			{
				myID = 103,
				upNeighborID = 108,
				rightNeighborID = 104
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(portraitX + portraitWidth + (int)((double)equipmentIconSize * 0.5 * (double)widthMod), portraitY, equipmentIconSize, equipmentIconSize), "Right Ring")
			{
				myID = 102,
				downNeighborID = 109,
				leftNeighborID = 101,
				upNeighborID = 4 + (Game1.player.MaxItems - 1) / 12 * 12,
				upNeighborImmutable = true
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(portraitX + portraitWidth + (int)((double)equipmentIconSize * 0.5 * (double)widthMod), portraitY + portraitHeight - equipmentIconSize, equipmentIconSize, equipmentIconSize), "Boots")
			{
				myID = 104,
				leftNeighborID = 103,
				upNeighborID = 109
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(portraitX - (int)((float)equipmentIconSize + (float)(equipmentIconSize / 2) * widthMod), portraitY + equipmentIconSize, equipmentIconSize, equipmentIconSize), "Shirt")
			{
				myID = 108,
				upNeighborID = 101,
				downNeighborID = 103,
				rightNeighborID = 109
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(portraitX + portraitWidth + (int)((double)equipmentIconSize * 0.5 * (double)widthMod), portraitY + equipmentIconSize, equipmentIconSize, equipmentIconSize), "Pants")
			{
				myID = 109,
				downNeighborID = 104,
				leftNeighborID = 108,
				upNeighborID = 102
			});
			portrait = new ClickableComponent(new Rectangle(portraitX, portraitY, 64, 96), "32");
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.options.doesInputListContain(Game1.options.inventorySlot1, key))
			{
				Game1.player.CurrentToolIndex = 0;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot2, key))
			{
				Game1.player.CurrentToolIndex = 1;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot3, key))
			{
				Game1.player.CurrentToolIndex = 2;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot4, key))
			{
				Game1.player.CurrentToolIndex = 3;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot5, key))
			{
				Game1.player.CurrentToolIndex = 4;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot6, key))
			{
				Game1.player.CurrentToolIndex = 5;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot7, key))
			{
				Game1.player.CurrentToolIndex = 6;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot8, key))
			{
				Game1.player.CurrentToolIndex = 7;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot9, key))
			{
				Game1.player.CurrentToolIndex = 8;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot10, key))
			{
				Game1.player.CurrentToolIndex = 9;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot11, key))
			{
				Game1.player.CurrentToolIndex = 10;
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.inventorySlot12, key))
			{
				Game1.player.CurrentToolIndex = 11;
				Game1.playSound("toolSwap");
			}
		}

		public override void setUpForGamePadMode()
		{
			base.setUpForGamePadMode();
			if (inventory != null)
			{
				inventory.setUpForGamePadMode();
			}
			currentRegion = 100;
		}

		public override void releaseLeftClick(int x, int y)
		{
			highlightEquipmentIcon = -1;
			bool flag = false;
			if (inventory.dragItem != -1)
			{
				foreach (ClickableComponent equipmentIcon in equipmentIcons)
				{
					if (equipmentIcon.containsPoint(x, y))
					{
						receiveLeftClick(x, y);
						flag = true;
					}
				}
				if (!flag && y > bottomBoxY + 64 && x > width / 2 && heldItem != null && heldItem.canBeTrashed())
				{
					Game1.playSound("throwDownITem");
					Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
					inventory.actualInventory.RemoveAt(inventory.dragItem);
					heldItem = null;
					inventory.dragItem = -1;
					Game1.exitActiveMenu();
				}
			}
			inventory.releaseLeftClick(x, y);
		}

		public override void leftClickHeld(int x, int y)
		{
			inventory.leftClickHeld(x, y);
			highlightEquipmentIcon = -1;
			int num = 0;
			if (heldItem == null)
			{
				return;
			}
			foreach (ClickableComponent equipmentIcon in equipmentIcons)
			{
				if (equipmentIcon.containsPoint(x, y) && inventory.dragItem != -1 && ((equipmentIcon.name == "Hat" && heldItem is Hat) || ((equipmentIcon.name == "Left Ring" || equipmentIcon.name == "Right Ring") && heldItem is Ring) || (equipmentIcon.name == "Boots" && heldItem is Boots) || (equipmentIcon.name == "Shirt" && heldItem is Clothing && (heldItem as Clothing).clothesType.Value == 0) || (equipmentIcon.name == "Pants" && heldItem is Clothing && (heldItem as Clothing).clothesType.Value == 1) || (heldItem is Object && (int)heldItem.parentSheetIndex == 71)))
				{
					highlightEquipmentIcon = num;
				}
				num++;
			}
		}

		public override void update(GameTime time)
		{
			if (!TutorialManager.Instance.inventoryhasBeenSeen)
			{
				TutorialManager.Instance.inventoryhasBeenSeen = true;
				TutorialManager.Instance.completeTutorial(tutorialType.DUMMY_INVENTORY);
			}
			inventory.update(time);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			inventory.receiveLeftClick(x, y, playSound);
			foreach (ClickableComponent equipmentIcon in equipmentIcons)
			{
				if (!equipmentIcon.containsPoint(x, y))
				{
					continue;
				}
				if (inventory.currentlySelectedItem != -1)
				{
					heldItem = inventory.getItemFromClickableComponent(inventory.inventory[inventory.currentlySelectedItem]);
				}
				else
				{
					heldItem = null;
				}
				bool flag = heldItem == null;
				switch (equipmentIcon.name)
				{
				case "Hat":
				{
					if (heldItem != null && !(heldItem is Hat) && !(heldItem is Pan))
					{
						break;
					}
					Hat value5 = ((heldItem is Pan) ? new Hat(71) : ((Hat)heldItem));
					heldItem = (Hat)Game1.player.hat;
					heldItem = Utility.PerformSpecialItemGrabReplacement(heldItem);
					Game1.player.hat.Value = value5;
					if (Game1.player.hat.Value != null)
					{
						Game1.playSound("grassyStep");
						Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
						inventory.currentlySelectedItem = -1;
						inventory.dragItem = -1;
					}
					if (heldItem != null)
					{
						Game1.playSound("dwop");
						if (Utility.ifPossibleAddItemToThisInventoryList(heldItem, inventory.actualInventory, Game1.player.MaxItems))
						{
							heldItem = null;
							inventory.currentlySelectedItem = -1;
							inventory.dragItem = -1;
						}
						else
						{
							Game1.player.hat.Value = ((heldItem is Pan) ? new Hat(71) : ((Hat)heldItem));
						}
					}
					break;
				}
				case "Left Ring":
				{
					if (heldItem != null && !(heldItem is Ring))
					{
						break;
					}
					Ring value4 = (Ring)heldItem;
					heldItem = (Ring)Game1.player.leftRing;
					Game1.player.leftRing.Value = value4;
					if (heldItem != null)
					{
						(heldItem as Ring).onUnequip(Game1.player, Game1.currentLocation);
					}
					if (Game1.player.leftRing.Value != null)
					{
						Game1.player.leftRing.Value.onEquip(Game1.player, Game1.currentLocation);
						Game1.playSound("crit");
						Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
						inventory.currentlySelectedItem = -1;
					}
					if (heldItem != null)
					{
						Game1.playSound("dwop");
						if (Utility.ifPossibleAddItemToThisInventoryList(heldItem, inventory.actualInventory, Game1.player.MaxItems))
						{
							heldItem = null;
							inventory.currentlySelectedItem = -1;
							inventory.dragItem = -1;
						}
						else
						{
							Game1.player.leftRing.Value = (Ring)heldItem;
						}
					}
					break;
				}
				case "Right Ring":
				{
					if (heldItem != null && !(heldItem is Ring))
					{
						break;
					}
					Ring value6 = (Ring)heldItem;
					heldItem = (Ring)Game1.player.rightRing;
					Game1.player.rightRing.Value = value6;
					if (heldItem != null)
					{
						(heldItem as Ring).onUnequip(Game1.player, Game1.currentLocation);
					}
					if (Game1.player.rightRing.Value != null)
					{
						Game1.player.rightRing.Value.onEquip(Game1.player, Game1.currentLocation);
						Game1.playSound("crit");
						Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
						inventory.currentlySelectedItem = -1;
					}
					if (heldItem != null)
					{
						Game1.playSound("dwop");
						if (Utility.ifPossibleAddItemToThisInventoryList(heldItem, inventory.actualInventory, Game1.player.MaxItems))
						{
							heldItem = null;
							inventory.currentlySelectedItem = -1;
							inventory.dragItem = -1;
						}
						else
						{
							Game1.player.rightRing.Value = (Ring)heldItem;
						}
					}
					break;
				}
				case "Boots":
				{
					if (heldItem != null && !(heldItem is Boots))
					{
						break;
					}
					Boots value2 = (Boots)heldItem;
					heldItem = (Boots)Game1.player.boots;
					Game1.player.boots.Value = value2;
					if (heldItem != null)
					{
						(heldItem as Boots).onUnequip();
					}
					if (Game1.player.boots.Value != null)
					{
						Game1.player.boots.Value.onEquip();
						Game1.playSound("sandyStep");
						DelayedAction.playSoundAfterDelay("sandyStep", 150);
						Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
						inventory.currentlySelectedItem = -1;
					}
					if (heldItem != null)
					{
						Game1.playSound("dwop");
						if (Utility.ifPossibleAddItemToThisInventoryList(heldItem, inventory.actualInventory, Game1.player.MaxItems))
						{
							heldItem = null;
							inventory.currentlySelectedItem = -1;
							inventory.dragItem = -1;
						}
						else
						{
							Game1.player.boots.Value = (Boots)heldItem;
						}
					}
					break;
				}
				case "Shirt":
				{
					if (heldItem != null && (!(heldItem is Clothing) || (heldItem as Clothing).clothesType.Value != 0))
					{
						break;
					}
					Clothing value3 = (Clothing)heldItem;
					heldItem = (Clothing)Game1.player.shirtItem;
					heldItem = Utility.PerformSpecialItemGrabReplacement(heldItem);
					Game1.player.shirtItem.Value = value3;
					if (Game1.player.shirtItem.Value != null)
					{
						Game1.playSound("sandyStep");
						Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
						inventory.currentlySelectedItem = -1;
					}
					if (heldItem != null)
					{
						Game1.playSound("dwop");
						if (Utility.ifPossibleAddItemToThisInventoryList(heldItem, inventory.actualInventory, Game1.player.MaxItems))
						{
							heldItem = null;
							inventory.currentlySelectedItem = -1;
							inventory.dragItem = -1;
						}
						else
						{
							Game1.player.shirtItem.Value = (Clothing)heldItem;
						}
					}
					break;
				}
				case "Pants":
				{
					if (heldItem != null && (!(heldItem is Clothing) || (heldItem as Clothing).clothesType.Value != 1) && (!(heldItem is Object) || (int)heldItem.parentSheetIndex != 71))
					{
						break;
					}
					Clothing value = ((heldItem is Object && (int)heldItem.parentSheetIndex == 71) ? new Clothing(15) : ((Clothing)heldItem));
					heldItem = (Clothing)Game1.player.pantsItem;
					heldItem = Utility.PerformSpecialItemGrabReplacement(heldItem);
					Game1.player.pantsItem.Value = value;
					if (Game1.player.pantsItem.Value != null)
					{
						Game1.playSound("sandyStep");
						Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
						inventory.currentlySelectedItem = -1;
					}
					if (heldItem != null)
					{
						Game1.playSound("dwop");
						if (Utility.ifPossibleAddItemToThisInventoryList(heldItem, inventory.actualInventory, Game1.player.MaxItems))
						{
							heldItem = null;
							inventory.currentlySelectedItem = -1;
							inventory.dragItem = -1;
						}
						else
						{
							Game1.player.pantsItem.Value = (Clothing)heldItem;
						}
					}
					break;
				}
				}
				if (!flag || heldItem == null || !Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					continue;
				}
				for (int i = 0; i < Game1.player.items.Count; i++)
				{
					if (Game1.player.items[i] == null || Game1.player.items[i].canStackWith(heldItem))
					{
						if (Game1.player.CurrentToolIndex == i && heldItem != null)
						{
							heldItem.actionWhenBeingHeld(Game1.player);
						}
						heldItem = Utility.addItemToInventory(heldItem, i, inventory.actualInventory);
						if (Game1.player.CurrentToolIndex == i && heldItem != null)
						{
							heldItem.actionWhenStopBeingHeld(Game1.player);
						}
						Game1.playSound("stoneStep");
						return;
					}
				}
			}
			heldItem = inventory.selectItemAt(x, y);
			if (heldItem != null && Utility.IsNormalObjectAtParentSheetIndex(heldItem, 434))
			{
				Game1.playSound("smallSelect");
				Game1.player.eatObject(heldItem as Object, overrideFullness: true);
				Game1.player.removeItemFromInventory(inventory.getInventoryPositionOfClick(x, y));
				heldItem = null;
				Game1.exitActiveMenu();
			}
			else if (heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
			{
				if (heldItem is Ring)
				{
					if (Game1.player.leftRing.Value == null)
					{
						Game1.player.leftRing.Value = heldItem as Ring;
						(heldItem as Ring).onEquip(Game1.player, Game1.currentLocation);
						heldItem = null;
						Game1.playSound("crit");
						return;
					}
					if (Game1.player.rightRing.Value == null)
					{
						Game1.player.rightRing.Value = heldItem as Ring;
						(heldItem as Ring).onEquip(Game1.player, Game1.currentLocation);
						heldItem = null;
						Game1.playSound("crit");
						return;
					}
				}
				else if (heldItem is Hat)
				{
					if (Game1.player.hat.Value == null)
					{
						Game1.player.hat.Value = heldItem as Hat;
						Game1.playSound("grassyStep");
						heldItem = null;
						return;
					}
				}
				else if (heldItem is Boots && Game1.player.boots.Value == null)
				{
					Game1.player.boots.Value = heldItem as Boots;
					(heldItem as Boots).onEquip();
					Game1.playSound("sandyStep");
					DelayedAction.playSoundAfterDelay("sandyStep", 150);
					heldItem = null;
					return;
				}
				if (inventory.getInventoryPositionOfClick(x, y) >= 12)
				{
					for (int j = 0; j < 12; j++)
					{
						if (Game1.player.items[j] == null || Game1.player.items[j].canStackWith(heldItem))
						{
							if (Game1.player.CurrentToolIndex == j && heldItem != null)
							{
								heldItem.actionWhenBeingHeld(Game1.player);
							}
							heldItem = Utility.addItemToInventory(heldItem, j, inventory.actualInventory);
							if (heldItem != null)
							{
								heldItem.actionWhenStopBeingHeld(Game1.player);
							}
							Game1.playSound("stoneStep");
							return;
						}
					}
				}
			}
			if (portrait.containsPoint(x, y))
			{
				portrait.name = (portrait.name.Equals("32") ? "8" : "32");
			}
			if (organizeButton != null && organizeButton.containsPoint(x, y))
			{
				ItemGrabMenu.organizeItemsInList(Game1.player.items);
				Game1.playSound("Ship");
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			Game1.options.snappyMenus = true;
			inventory.receiveGamePadButton(b);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			heldItem = inventory.rightClick(x, y, heldItem);
		}

		public override void performHoverAction(int x, int y)
		{
			descriptionText = "";
			descriptionTitle = "";
			hoveredItem = inventory.hover(x, y, heldItem);
			hoverText = inventory.hoverText;
			hoverTitle = inventory.hoverTitle;
			foreach (ClickableComponent equipmentIcon in equipmentIcons)
			{
				if (!equipmentIcon.containsPoint(x, y))
				{
					continue;
				}
				switch (equipmentIcon.name)
				{
				case "Hat":
					if (Game1.player.hat.Value != null)
					{
						hoveredItem = (Hat)Game1.player.hat;
						hoverText = Game1.player.hat.Value.getDescription();
						hoverTitle = Game1.player.hat.Value.DisplayName;
					}
					break;
				case "Right Ring":
					if (Game1.player.rightRing.Value != null)
					{
						hoveredItem = (Ring)Game1.player.rightRing;
						hoverText = Game1.player.rightRing.Value.getDescription();
						hoverTitle = Game1.player.rightRing.Value.DisplayName;
					}
					break;
				case "Left Ring":
					if (Game1.player.leftRing.Value != null)
					{
						hoveredItem = (Ring)Game1.player.leftRing;
						hoverText = Game1.player.leftRing.Value.getDescription();
						hoverTitle = Game1.player.leftRing.Value.DisplayName;
					}
					break;
				case "Boots":
					if (Game1.player.boots.Value != null)
					{
						hoveredItem = (Boots)Game1.player.boots;
						hoverText = Game1.player.boots.Value.getDescription();
						hoverTitle = Game1.player.boots.Value.DisplayName;
					}
					break;
				}
			}
			if (portrait.containsPoint(x, y))
			{
				portrait.scale += 0.2f;
				hoverText = Game1.content.LoadString("Strings\\UI:Inventory_PortraitHover_Level", Game1.player.Level) + Environment.NewLine + Game1.player.getTitle();
			}
			else
			{
				portrait.scale = 0f;
			}
			if (organizeButton != null)
			{
				organizeButton.tryHover(x, y);
				if (organizeButton.containsPoint(x, y))
				{
					hoverText = organizeButton.hoverText;
				}
			}
			if (junimoNoteIcon != null)
			{
				junimoNoteIcon.tryHover(x, y);
				if (junimoNoteIcon.containsPoint(x, y))
				{
					hoverText = junimoNoteIcon.hoverText;
				}
				if (GameMenu.bundleItemHovered)
				{
					junimoNoteIcon.scale = junimoNoteIcon.baseScale + (float)Math.Sin((float)junimoNotePulser / 100f) / 4f;
					junimoNotePulser += (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				}
				else
				{
					junimoNotePulser = 0;
					junimoNoteIcon.scale = junimoNoteIcon.baseScale;
				}
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override bool readyToClose()
		{
			return inventory.dragItem == -1;
		}

		public override void draw(SpriteBatch b)
		{
			string text = "";
			if (Game1.player.horseName.Value != null && Game1.player.horseName.Value != "")
			{
				text = Game1.player.horseName.Value;
			}
			IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
			drawMobileHorizontalPartition(b, xPositionOnScreen, bottomBoxY, width);
			b.Draw((Game1.timeOfDay >= 1900) ? Game1.nightbg : Game1.daybg, new Vector2(portrait.bounds.X, portrait.bounds.Y), Color.White);
			bool value = Game1.player.swimming;
			Game1.player.swimming.Value = false;
			FarmerRenderer.isDrawingForUI = true;
			Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, Game1.player.bathingClothes ? 108 : 0, secondaryArm: false, flip: false), Game1.player.bathingClothes ? 108 : 0, new Rectangle(0, Game1.player.bathingClothes ? 576 : 0, 16, 32), new Vector2((float)portrait.bounds.X + 32f, portrait.bounds.Y + 64 - 32), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, Game1.player);
			if (Game1.timeOfDay >= 1900)
			{
				Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, 0, secondaryArm: false, flip: false), 0, new Rectangle(0, 0, 16, 32), new Vector2((float)portrait.bounds.X + 32f, portraitY * 64 - 32), Vector2.Zero, 0.8f, 2, Color.DarkBlue * 0.3f, 0f, 1f, Game1.player);
			}
			FarmerRenderer.isDrawingForUI = false;
			Game1.player.swimming.Value = value;
			int num = 0;
			foreach (ClickableComponent equipmentIcon in equipmentIcons)
			{
				switch (equipmentIcon.name)
				{
				case "Hat":
					if (Game1.player.hat.Value != null)
					{
						b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
						Game1.player.hat.Value.itemSlotSize = 64;
						Game1.player.hat.Value.drawInMenu(b, new Vector2(equipmentIcon.bounds.X, equipmentIcon.bounds.Y), (float)equipmentIcon.bounds.Width / 64f, 1f, 0.866f, StackDrawType.Hide);
						Game1.player.hat.Value.itemSlotSize = 64;
					}
					else
					{
						b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 42), Color.White);
					}
					break;
				case "Right Ring":
					if (Game1.player.rightRing.Value != null)
					{
						b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
						Game1.player.rightRing.Value.itemSlotSize = 64;
						Game1.player.rightRing.Value.drawInMenu(b, new Vector2(equipmentIcon.bounds.X, equipmentIcon.bounds.Y), (float)equipmentIcon.bounds.Width / 64f, 1f, 0.866f, StackDrawType.Hide);
						Game1.player.rightRing.Value.itemSlotSize = 64;
					}
					else
					{
						b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41), Color.White);
					}
					break;
				case "Left Ring":
					if (Game1.player.leftRing.Value != null)
					{
						b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
						Game1.player.leftRing.Value.drawInMenu(b, new Vector2(equipmentIcon.bounds.X, equipmentIcon.bounds.Y), (float)equipmentIcon.bounds.Width / 64f, 1f, 0.866f, StackDrawType.Hide);
					}
					else
					{
						b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41), Color.White);
					}
					break;
				case "Boots":
					if (Game1.player.boots.Value != null)
					{
						b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
						Game1.player.boots.Value.itemSlotSize = 64;
						Game1.player.boots.Value.drawInMenu(b, new Vector2(equipmentIcon.bounds.X, equipmentIcon.bounds.Y), (float)equipmentIcon.bounds.Width / 64f, 1f, 0.866f, StackDrawType.Hide);
						Game1.player.boots.Value.itemSlotSize = 64;
					}
					else
					{
						b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 40), Color.White);
					}
					break;
				case "Shirt":
					if (Game1.player.shirtItem.Value != null)
					{
						b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
						Game1.player.shirtItem.Value.itemSlotSize = 64;
						Game1.player.shirtItem.Value.drawInMenu(b, new Vector2(equipmentIcon.bounds.X, equipmentIcon.bounds.Y), (float)equipmentIcon.bounds.Width / 64f, 1f, 0.866f, StackDrawType.Hide);
						Game1.player.shirtItem.Value.itemSlotSize = 64;
					}
					else
					{
						b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 69), Color.White);
					}
					break;
				case "Pants":
					if (Game1.player.pantsItem.Value != null)
					{
						b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
						Game1.player.pantsItem.Value.itemSlotSize = 64;
						Game1.player.pantsItem.Value.drawInMenu(b, new Vector2(equipmentIcon.bounds.X, equipmentIcon.bounds.Y), (float)equipmentIcon.bounds.Width / 64f, 1f, 0.866f, StackDrawType.Hide);
						Game1.player.pantsItem.Value.itemSlotSize = 64;
					}
					else
					{
						b.Draw(Game1.menuTexture, equipmentIcon.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 68), Color.White);
					}
					break;
				}
				if (num == highlightEquipmentIcon)
				{
					b.Draw(Game1.mouseCursors, equipmentIcon.bounds, new Rectangle(194, 388, 16, 16), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
				}
				num++;
			}
			SpriteFont spriteFont = ((heightMod < 0.8f) ? Game1.smallFont : Game1.dialogueFont);
			int num2 = (int)((double)(portraitX + portraitWidth) + (double)equipmentIconSize * 1.5);
			int num3 = bottomBoxY + 32 + ((!Game1.player.hasPet() && text.Length <= 0) ? 32 : 0);
			Vector2 vector;
			if (width > 1200)
			{
				vector = spriteFont.MeasureString(string.Concat(Game1.player.name, ": ", Game1.content.LoadString("Strings\\UI:Inventory_PortraitHover_Level", Game1.player.Level), " ", Game1.player.getTitle()));
				Utility.drawTextWithShadow(b, string.Concat(Game1.player.name, ": ", Game1.content.LoadString("Strings\\UI:Inventory_PortraitHover_Level", Game1.player.Level), " ", Game1.player.getTitle()), spriteFont, new Vector2((float)(num2 + (width - num2) / 2) - vector.X / 2f, num3), Game1.textColor);
			}
			else
			{
				vector = spriteFont.MeasureString(string.Concat(Game1.player.name, ":Lvl ", Game1.player.Level.ToString(), " ", Game1.player.getTitle()));
				Utility.drawTextWithShadow(b, string.Concat(Game1.player.name, ":Lvl ", Game1.player.Level.ToString(), " ", Game1.player.getTitle()), spriteFont, new Vector2((float)(num2 + (width - num2) / 2) - vector.X / 2f, num3), Game1.textColor);
			}
			drawMobileHorizontalPartition(b, (int)((float)(num2 + (width - num2) / 2) - vector.X / 2f), (int)((float)num3 + vector.Y) - 32, (int)vector.X, small: true);
			string text2 = Game1.content.LoadString("Strings\\UI:Inventory_FarmName", Game1.player.farmName);
			Utility.drawTextWithShadow(b, text2, spriteFont, new Vector2((float)(num2 + (width - num2) / 2) - spriteFont.MeasureString(text2).X / 2f, (float)num3 + 64f * heightMod), Game1.textColor);
			string text3 = Game1.content.LoadString("Strings\\UI:Inventory_CurrentFunds", Utility.getNumberWithCommas(Game1.player.Money));
			Utility.drawTextWithShadow(b, text3, spriteFont, new Vector2((float)(num2 + (width - num2) / 2) - spriteFont.MeasureString(text3).X / 2f, (float)num3 + 112f * heightMod), Game1.textColor);
			string text4 = Game1.content.LoadString("Strings\\UI:Inventory_TotalEarnings", Utility.getNumberWithCommas((int)Game1.player.totalMoneyEarned));
			Utility.drawTextWithShadow(b, text4, spriteFont, new Vector2((float)(num2 + (width - num2) / 2) - spriteFont.MeasureString(text4).X / 2f, (float)num3 + 160f * heightMod), Game1.textColor);
			if (Game1.player.hasPet())
			{
				string petDisplayName = Game1.player.getPetDisplayName();
				if (text.Length > 0)
				{
					Utility.drawTextWithShadow(b, petDisplayName, spriteFont, new Vector2((float)(num2 + (width - num2) / 2) - spriteFont.MeasureString(petDisplayName).X - 96f, (float)(((heightMod < 0.8f) ? (-8) : 0) + num3) + 224f * heightMod), Game1.textColor);
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num2 + (width - num2) / 2 - 96, (float)(((heightMod < 0.8f) ? (-16) : 0) + num3) + 211.2f * heightMod), new Rectangle(160 + ((!Game1.MasterPlayer.catPerson) ? 48 : 0) + Game1.MasterPlayer.whichPetBreed * 16, 208, 16, 16), Color.White, 0f, Vector2.Zero, 4f);
				}
				else
				{
					Utility.drawTextWithShadow(b, petDisplayName, spriteFont, new Vector2((float)(num2 + (width - num2) / 2) + 32f - spriteFont.MeasureString(petDisplayName).X / 2f, (float)(((heightMod < 0.8f) ? (-8) : 0) + num3) + 224f * heightMod), Game1.textColor);
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(num2 + (width - num2) / 2) - 32f - spriteFont.MeasureString(petDisplayName).X / 2f, (float)(((heightMod < 0.8f) ? (-16) : 0) + num3) + 211.2f * heightMod), new Rectangle(160 + ((!Game1.MasterPlayer.catPerson) ? 48 : 0) + Game1.MasterPlayer.whichPetBreed * 16, 208, 16, 16), Color.White, 0f, Vector2.Zero, 4f);
				}
			}
			if (text.Length > 0)
			{
				if (Game1.player.hasPet())
				{
					Utility.drawTextWithShadow(b, text, spriteFont, new Vector2(num2 + (width - num2) / 2 + 96, (float)(((heightMod < 0.8f) ? (-8) : 0) + num3) + 224f * heightMod), Game1.textColor);
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num2 + (width - num2) / 2 + 32, (float)(((heightMod < 0.8f) ? (-16) : 0) + num3) + 211.2f * heightMod), new Rectangle(193, 192, 16, 16), Color.White, 0f, Vector2.Zero, 4f);
				}
				else
				{
					Utility.drawTextWithShadow(b, text, spriteFont, new Vector2((float)(num2 + (width - num2) / 2) + 32f - spriteFont.MeasureString(text).X / 2f, (float)(((heightMod < 0.8f) ? (-8) : 0) + num3) + 224f * heightMod), Game1.textColor);
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2((float)(num2 + (width - num2) / 2) - 32f - spriteFont.MeasureString(text).X / 2f, (float)(((heightMod < 0.8f) ? (-16) : 0) + num3) + 211.2f * heightMod), new Rectangle(193, 192, 16, 16), Color.White, 0f, Vector2.Zero, 4f);
				}
			}
			inventory.draw(b);
		}
	}
}
