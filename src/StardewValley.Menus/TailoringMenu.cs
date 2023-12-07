using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Crafting;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	public class TailoringMenu : MenuWithInventory
	{
		public enum CraftState
		{
			MissingIngredients,
			Valid,
			InvalidRecipe,
			NotDyeable
		}

		protected int _timeUntilCraft;

		public const int region_leftIngredient = 998;

		public const int region_rightIngredient = 997;

		public const int region_startButton = 996;

		public const int region_resultItem = 995;

		public ClickableTextureComponent needleSprite;

		public ClickableTextureComponent presserSprite;

		public ClickableTextureComponent craftResultDisplay;

		public Vector2 needlePosition;

		public Vector2 presserPosition;

		public Vector2 leftIngredientStartSpot;

		public Vector2 leftIngredientEndSpot;

		protected float _rightItemOffset;

		public ClickableTextureComponent leftIngredientSpot;

		public ClickableTextureComponent rightIngredientSpot;

		public ClickableTextureComponent blankLeftIngredientSpot;

		public ClickableTextureComponent blankRightIngredientSpot;

		public ClickableTextureComponent startTailoringButton;

		public const int region_shirt = 108;

		public const int region_pants = 109;

		public const int region_hat = 101;

		public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

		public const int CRAFT_TIME = 1500;

		public Texture2D tailoringTextures;

		public List<TailorItemRecipe> _tailoringRecipes;

		private ICue _sewingSound;

		protected Dictionary<Item, bool> _highlightDictionary;

		protected Dictionary<string, Item> _lastValidEquippedItems;

		protected bool _shouldPrismaticDye;

		protected bool _heldItemIsEquipped;

		protected bool _isDyeCraft;

		protected bool _isMultipleResultCraft;

		protected string displayedDescription = "";

		protected CraftState _craftState;

		public Vector2 questionMarkOffset;

		private const int item_hat = 0;

		private const int item_shirt = 1;

		private const int item_pants = 2;

		private int dragEquippedItem = -1;

		private int selectedEquppedItem = -1;

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

		private int dyePanelCrop;

		private int red = 50;

		private int green = 160;

		private int blue = 255;

		private float dyePanelRatioWH = 1.75f;

		private float dyePanelWidthRatio;

		private float dyePanelHeightRatio;

		public TailoringMenu()
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
			dyePanelX = Game1.xEdge + (int)(128f * widthMod);
			dyePanelY = yPositionOnScreen;
			dyePanelHeight = height / 2;
			dyePanelWidth = (int)((float)dyePanelHeight * dyePanelRatioWH);
			dyePanelWidthRatio = (float)(dyePanelWidth - 32) / 140f;
			dyePanelHeightRatio = (float)(dyePanelHeight - 40) / 80f;
			int num = height - dyePanelHeight;
			infoBox = new Rectangle(dyePanelWidth + dyePanelX, dyePanelY, width - dyePanelWidth - dyePanelX + xPositionOnScreen, dyePanelHeight);
			bottomInv = new Rectangle(xPositionOnScreen, dyePanelY + dyePanelHeight, width, num);
			inventory.movePosition(0, (dyePanelY + dyePanelHeight - inventory.getInvY()) / 2);
			inventory.highlightMethod = HighlightItems;
			tailoringTextures = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\tailoring");
			_tailoringRecipes = Game1.temporaryContent.Load<List<TailorItemRecipe>>("Data\\TailoringRecipes");
			dyePanelRect = new Rectangle(dyePanelX + 18, dyePanelY + 16, dyePanelWidth - 32, dyePanelHeight - 40);
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
			_ValidateCraft();
		}

		protected void _CreateButtons()
		{
			leftIngredientSpot = new ClickableTextureComponent(new Rectangle(dyePanelRect.Left + (int)(3f * dyePanelWidthRatio), dyePanelRect.Top + (int)(50f * dyePanelHeightRatio), (int)(24f * dyePanelWidthRatio), (int)(24f * dyePanelHeightRatio)), tailoringTextures, new Rectangle(0, 156, 24, 24), dyePanelHeightRatio)
			{
				myID = 998,
				downNeighborID = -99998,
				leftNeighborID = 109,
				rightNeighborID = 996,
				upNeighborID = 997,
				item = ((leftIngredientSpot != null) ? leftIngredientSpot.item : null)
			};
			leftIngredientStartSpot = new Vector2(leftIngredientSpot.bounds.X, leftIngredientSpot.bounds.Y);
			leftIngredientEndSpot = leftIngredientStartSpot + new Vector2(64f * dyePanelWidthRatio, 0f);
			needleSprite = new ClickableTextureComponent(new Rectangle(dyePanelRect.Left + (int)(37f * dyePanelWidthRatio) + 8 - (int)(32f * widthMod), dyePanelRect.Top + (int)(49f * dyePanelHeightRatio) + 16 - (int)(64f * widthMod), (int)(96f * widthMod), (int)(96f * widthMod)), tailoringTextures, new Rectangle(64, 80, 16, 32), 4f * widthMod);
			presserSprite = new ClickableTextureComponent(new Rectangle(dyePanelRect.Left + (int)(38f * dyePanelWidthRatio) - (int)(32f * widthMod), dyePanelRect.Top + (int)(52f * dyePanelHeightRatio) - (int)(64f * widthMod), (int)(96f * widthMod), (int)(96f * widthMod)), tailoringTextures, new Rectangle(48, 80, 16, 32), 4f * widthMod);
			needlePosition = new Vector2(needleSprite.bounds.X, needleSprite.bounds.Y);
			presserPosition = new Vector2(presserSprite.bounds.X, presserSprite.bounds.Y);
			rightIngredientSpot = new ClickableTextureComponent(new Rectangle(dyePanelRect.Right - (int)(40f * dyePanelWidthRatio), dyePanelRect.Top + (int)(2f * dyePanelHeightRatio), (int)(24f * dyePanelWidthRatio), (int)(24f * dyePanelHeightRatio)), tailoringTextures, new Rectangle(0, 180, 24, 24), dyePanelHeightRatio)
			{
				myID = 997,
				downNeighborID = 996,
				leftNeighborID = 998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				item = ((rightIngredientSpot != null) ? rightIngredientSpot.item : null),
				fullyImmutable = true
			};
			blankRightIngredientSpot = new ClickableTextureComponent(new Rectangle(rightIngredientSpot.bounds.X, rightIngredientSpot.bounds.Y, (int)(24f * dyePanelHeightRatio), (int)(24f * dyePanelWidthRatio)), tailoringTextures, new Rectangle(0, 128, 24, 24), dyePanelHeightRatio);
			blankLeftIngredientSpot = new ClickableTextureComponent(new Rectangle(leftIngredientSpot.bounds.X, leftIngredientSpot.bounds.Y, (int)(24f * dyePanelHeightRatio), (int)(24f * dyePanelWidthRatio)), tailoringTextures, new Rectangle(0, 128, 24, 24), dyePanelHeightRatio);
			startTailoringButton = new ClickableTextureComponent(new Rectangle(dyePanelRect.Right - (int)(28f * dyePanelWidthRatio), dyePanelRect.Top + (int)(34f * dyePanelHeightRatio), (int)(24f * dyePanelWidthRatio), (int)(24f * dyePanelHeightRatio)), tailoringTextures, new Rectangle(24, 80, 24, 24), dyePanelWidthRatio)
			{
				myID = 996,
				downNeighborID = -99998,
				leftNeighborID = 998,
				rightNeighborID = 995,
				upNeighborID = 997,
				item = ((startTailoringButton != null) ? startTailoringButton.item : null),
				fullyImmutable = true
			};
			if (inventory.inventory != null && inventory.inventory.Count >= 12)
			{
				for (int i = 0; i < 12; i++)
				{
					if (inventory.inventory[i] != null)
					{
						inventory.inventory[i].upNeighborID = -99998;
					}
				}
			}
			equipmentIcons = new List<ClickableComponent>();
			equipmentIcons.Add(new ClickableComponent(new Rectangle(0, 0, (int)(64f * widthMod), (int)(64f * widthMod)), "Hat")
			{
				myID = 101,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborID = -99998
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(0, 0, (int)(64f * widthMod), (int)(64f * widthMod)), "Shirt")
			{
				myID = 108,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = -99998,
				leftNeighborID = -99998
			});
			equipmentIcons.Add(new ClickableComponent(new Rectangle(0, 0, (int)(64f * widthMod), (int)(64f * widthMod)), "Pants")
			{
				myID = 109,
				upNeighborID = -99998,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998
			});
			for (int j = 0; j < equipmentIcons.Count; j++)
			{
				equipmentIcons[j].bounds.X = xPositionOnScreen + (int)(64f * widthMod / 2f) + 9;
				equipmentIcons[j].bounds.Y = yPositionOnScreen + (int)(((float)(dyePanelHeight / 3) - 64f * widthMod) / 2f) + j * ((int)(2f * (((float)(dyePanelHeight / 3) - 64f * widthMod) / 2f)) + (int)(64f * widthMod));
			}
			craftResultDisplay = new ClickableTextureComponent(new Rectangle(infoBox.X + (infoBox.Width - 64) / 2, infoBox.Y + (infoBox.Height - 64) / 2, 64, 64), tailoringTextures, new Rectangle(0, 208, 16, 16), 4f)
			{
				item = ((craftResultDisplay != null) ? craftResultDisplay.item : null)
			};
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
			if (i != null && !IsValidCraftIngredient(i))
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
			return _highlightDictionary[i];
		}

		public void GenerateHighlightDictionary()
		{
			_highlightDictionary = new Dictionary<Item, bool>();
			List<Item> list = new List<Item>(inventory.actualInventory);
			if (Game1.player.pantsItem.Value != null)
			{
				list.Add(Game1.player.pantsItem.Value);
			}
			if (Game1.player.shirtItem.Value != null)
			{
				list.Add(Game1.player.shirtItem.Value);
			}
			if (Game1.player.hat.Value != null)
			{
				list.Add(Game1.player.hat.Value);
			}
			foreach (Item item in list)
			{
				if (item != null)
				{
					if (leftIngredientSpot.item == null && rightIngredientSpot.item == null)
					{
						_highlightDictionary[item] = true;
					}
					else if (leftIngredientSpot.item != null && rightIngredientSpot.item != null)
					{
						_highlightDictionary[item] = false;
					}
					else if (leftIngredientSpot.item != null)
					{
						_highlightDictionary[item] = IsValidCraft(leftIngredientSpot.item, item);
					}
					else
					{
						_highlightDictionary[item] = IsValidCraft(item, rightIngredientSpot.item);
					}
				}
			}
		}

		private void _leftIngredientSpotClicked()
		{
			Item item = leftIngredientSpot.item;
			leftIngredientSpot.item = null;
			int num = -1;
			if (inventory.dragItem != -1)
			{
				num = inventory.dragItem;
			}
			else if (inventory.currentlySelectedItem != -1)
			{
				num = inventory.currentlySelectedItem;
			}
			if (num != -1)
			{
				if (num != -1)
				{
					leftIngredientSpot.item = inventory.actualInventory[num].getOne();
					inventory.actualInventory[num].Stack--;
					if (inventory.actualInventory[num].Stack <= 0 || !inventory.actualInventory[num].canStackWith(inventory.actualInventory[num]))
					{
						Utility.removeItemFromInventory(num, inventory.actualInventory);
					}
					inventory.currentlySelectedItem = -1;
					inventory.dragItem = -1;
					heldItem = null;
				}
			}
			else if (heldItem != null && Game1.player.IsEquippedItem(heldItem))
			{
				leftIngredientSpot.item = heldItem;
				heldItem = null;
			}
			if (item != null && !Game1.player.IsEquippedItem(item))
			{
				item = inventory.tryToAddItem(item);
				if (item != null)
				{
					Game1.playSound("throwDownITem");
					Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
				}
			}
			_highlightDictionary = null;
			_ValidateCraft();
		}

		public bool IsValidCraftIngredient(Item item)
		{
			if (item.HasContextTag("item_lucky_purple_shorts"))
			{
				return true;
			}
			if (!item.canBeTrashed())
			{
				return false;
			}
			return true;
		}

		private void _rightIngredientSpotClicked()
		{
			Item item = rightIngredientSpot.item;
			rightIngredientSpot.item = null;
			int num = -1;
			if (inventory.dragItem != -1)
			{
				num = inventory.dragItem;
			}
			else if (inventory.currentlySelectedItem != -1)
			{
				num = inventory.currentlySelectedItem;
			}
			if (num != -1 && num != -1)
			{
				rightIngredientSpot.item = inventory.actualInventory[num].getOne();
				inventory.actualInventory[num].Stack--;
				if (inventory.actualInventory[num].Stack <= 0 || !inventory.actualInventory[num].canStackWith(inventory.actualInventory[num]))
				{
					Utility.removeItemFromInventory(num, inventory.actualInventory);
				}
				inventory.currentlySelectedItem = -1;
				inventory.dragItem = -1;
				heldItem = null;
			}
			if (item != null)
			{
				item = inventory.tryToAddItem(item);
				if (item != null)
				{
					Game1.playSound("throwDownITem");
					Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
				}
			}
			_highlightDictionary = null;
			_ValidateCraft();
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

		public bool IsHoldingEquippedItem()
		{
			if (heldItem == null)
			{
				return false;
			}
			return Game1.player.IsEquippedItem(heldItem);
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (IsBusy())
			{
				return;
			}
			if (leftIngredientSpot.containsPoint(x, y))
			{
				_leftIngredientSpotClicked();
			}
			else if (rightIngredientSpot.containsPoint(x, y))
			{
				_rightIngredientSpotClicked();
			}
			else if (startTailoringButton.containsPoint(x, y))
			{
				if (heldItem == null)
				{
					bool flag = false;
					if (!CanFitCraftedItem())
					{
						Game1.playSound("cancel");
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
						_timeUntilCraft = 0;
						flag = true;
					}
					if (!flag && IsValidCraft(leftIngredientSpot.item, rightIngredientSpot.item))
					{
						Game1.playSound("bigSelect");
						_sewingSound = Game1.soundBank.GetCue("sewing_loop");
						_sewingSound.Play();
						startTailoringButton.scale = startTailoringButton.baseScale;
						_timeUntilCraft = 1500;
						_UpdateDescriptionText();
					}
					else
					{
						Game1.playSound("sell");
					}
				}
				else
				{
					Game1.playSound("sell");
				}
			}
			else if (Game1.player.IsEquippedItem(heldItem))
			{
				if (inventory.isWithinBounds(x, y))
				{
					Item item = heldItem;
					if (inventory.addItemAt(heldItem, x, y, allowStack: false))
					{
						heldItem = null;
					}
					else
					{
						heldItem = null;
						item = null;
					}
					if (heldItem != item)
					{
						if (item == Game1.player.hat.Value)
						{
							Game1.player.hat.Value = null;
							_highlightDictionary = null;
						}
						else if (item == Game1.player.shirtItem.Value)
						{
							Game1.player.shirtItem.Value = null;
							_highlightDictionary = null;
						}
						else if (item == Game1.player.pantsItem.Value)
						{
							Game1.player.pantsItem.Value = null;
							_highlightDictionary = null;
						}
					}
				}
				else
				{
					heldItem = null;
				}
			}
			else if (inventory.dragItem != -1)
			{
				foreach (ClickableComponent equipmentIcon in equipmentIcons)
				{
					if (!equipmentIcon.containsPoint(x, y))
					{
						continue;
					}
					switch (equipmentIcon.name)
					{
					case "Hat":
					{
						Item item5 = Utility.PerformSpecialItemPlaceReplacement(heldItem);
						if (item5 is Hat)
						{
							Item value2 = Game1.player.hat.Value;
							value2 = Utility.PerformSpecialItemGrabReplacement(value2);
							if (value2 == heldItem)
							{
								value2 = null;
							}
							Game1.player.hat.Value = item5 as Hat;
							if (inventory.inventoryItemHeld != -1)
							{
								inventory.SetItemAt(inventory.inventoryItemHeld, value2);
								inventory.currentlySelectedItem = -1;
								inventory.inventoryItemHeld = -1;
								value2 = null;
							}
							heldItem = value2;
							Game1.playSound("grassyStep");
							_highlightDictionary = null;
							_ValidateCraft();
						}
						break;
					}
					case "Shirt":
					{
						Item item3 = Utility.PerformSpecialItemPlaceReplacement(heldItem);
						if (heldItem is Clothing && (heldItem as Clothing).clothesType.Value == 0)
						{
							Item item4 = (Clothing)Game1.player.shirtItem;
							item4 = Utility.PerformSpecialItemGrabReplacement(item4);
							if (item4 == heldItem)
							{
								item4 = null;
							}
							Game1.player.shirtItem.Value = item3 as Clothing;
							if (inventory.inventoryItemHeld != -1)
							{
								inventory.SetItemAt(inventory.inventoryItemHeld, item4);
								inventory.currentlySelectedItem = -1;
								inventory.inventoryItemHeld = -1;
								item4 = null;
							}
							heldItem = item4;
							Game1.playSound("sandyStep");
							_highlightDictionary = null;
							_ValidateCraft();
						}
						break;
					}
					case "Pants":
					{
						Item item2 = Utility.PerformSpecialItemPlaceReplacement(heldItem);
						if (item2 is Clothing && (item2 as Clothing).clothesType.Value == 1)
						{
							Item value = Game1.player.pantsItem.Value;
							value = Utility.PerformSpecialItemGrabReplacement(value);
							if (value == heldItem)
							{
								value = null;
							}
							Game1.player.pantsItem.Value = item2 as Clothing;
							if (inventory.inventoryItemHeld != -1)
							{
								inventory.SetItemAt(inventory.inventoryItemHeld, value);
								inventory.currentlySelectedItem = -1;
								inventory.inventoryItemHeld = -1;
								value = null;
							}
							heldItem = value;
							Game1.playSound("sandyStep");
							_highlightDictionary = null;
							_ValidateCraft();
						}
						break;
					}
					}
				}
			}
			base.releaseLeftClick(x, y);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			Item item = heldItem;
			bool flag = Game1.player.IsEquippedItem(item);
			if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
			{
				_OnCloseMenu();
			}
			base.receiveLeftClick(x, y, playSound: true);
			if (flag && heldItem != item)
			{
				if (item == Game1.player.hat.Value)
				{
					Game1.player.hat.Value = null;
					_highlightDictionary = null;
				}
				else if (item == Game1.player.shirtItem.Value)
				{
					Game1.player.shirtItem.Value = null;
					_highlightDictionary = null;
				}
				else if (item == Game1.player.pantsItem.Value)
				{
					Game1.player.pantsItem.Value = null;
					_highlightDictionary = null;
				}
			}
			foreach (ClickableComponent equipmentIcon in equipmentIcons)
			{
				if (!equipmentIcon.containsPoint(x, y))
				{
					continue;
				}
				switch (equipmentIcon.name)
				{
				case "Hat":
				{
					Item item5 = Utility.PerformSpecialItemPlaceReplacement(heldItem);
					if (heldItem == null)
					{
						if (HighlightItems((Hat)Game1.player.hat))
						{
							heldItem = Utility.PerformSpecialItemGrabReplacement((Hat)Game1.player.hat);
							Game1.playSound("dwop");
							if (!(heldItem is Hat))
							{
								Game1.player.hat.Value = null;
							}
							_highlightDictionary = null;
							_ValidateCraft();
						}
					}
					else if (item5 is Hat)
					{
						Item value2 = Game1.player.hat.Value;
						value2 = Utility.PerformSpecialItemGrabReplacement(value2);
						if (value2 == heldItem)
						{
							value2 = null;
						}
						Game1.player.hat.Value = item5 as Hat;
						heldItem = value2;
						if (heldItem == null)
						{
							inventory.SetItemAt(inventory.currentlySelectedItem, value2);
							inventory.currentlySelectedItem = -1;
							inventory.inventoryItemHeld = -1;
							value2 = null;
						}
						else
						{
							inventory.SetItemAt(inventory.currentlySelectedItem, heldItem);
							inventory.currentlySelectedItem = -1;
							inventory.inventoryItemHeld = -1;
							heldItem = null;
							value2 = null;
						}
						Game1.playSound("grassyStep");
						_highlightDictionary = null;
						_ValidateCraft();
					}
					break;
				}
				case "Shirt":
				{
					Item item3 = Utility.PerformSpecialItemPlaceReplacement(heldItem);
					if (heldItem == null)
					{
						if (HighlightItems((Clothing)Game1.player.shirtItem))
						{
							heldItem = Utility.PerformSpecialItemGrabReplacement((Clothing)Game1.player.shirtItem);
							Game1.playSound("dwop");
							if (!(heldItem is Clothing))
							{
								Game1.player.shirtItem.Value = null;
							}
							_highlightDictionary = null;
							_ValidateCraft();
						}
					}
					else if (heldItem is Clothing && (heldItem as Clothing).clothesType.Value == 0)
					{
						Item item4 = (Clothing)Game1.player.shirtItem;
						item4 = Utility.PerformSpecialItemGrabReplacement(item4);
						if (item4 == heldItem)
						{
							item4 = null;
						}
						Game1.player.shirtItem.Value = item3 as Clothing;
						heldItem = item4;
						if (heldItem == null)
						{
							inventory.SetItemAt(inventory.currentlySelectedItem, item4);
							inventory.currentlySelectedItem = -1;
							inventory.inventoryItemHeld = -1;
							item4 = null;
						}
						else
						{
							inventory.SetItemAt(inventory.currentlySelectedItem, heldItem);
							inventory.currentlySelectedItem = -1;
							inventory.inventoryItemHeld = -1;
							heldItem = null;
							item4 = null;
						}
						Game1.playSound("sandyStep");
						_highlightDictionary = null;
						_ValidateCraft();
					}
					break;
				}
				case "Pants":
				{
					Item item2 = Utility.PerformSpecialItemPlaceReplacement(heldItem);
					if (heldItem == null)
					{
						if (HighlightItems((Clothing)Game1.player.pantsItem))
						{
							heldItem = Utility.PerformSpecialItemGrabReplacement((Clothing)Game1.player.pantsItem);
							if (!(heldItem is Clothing))
							{
								Game1.player.pantsItem.Value = null;
							}
							Game1.playSound("dwop");
							_highlightDictionary = null;
							_ValidateCraft();
						}
					}
					else if (item2 is Clothing && (item2 as Clothing).clothesType.Value == 1)
					{
						Item value = Game1.player.pantsItem.Value;
						value = Utility.PerformSpecialItemGrabReplacement(value);
						if (value == heldItem)
						{
							value = null;
						}
						Game1.player.pantsItem.Value = item2 as Clothing;
						heldItem = value;
						if (heldItem == null)
						{
							inventory.SetItemAt(inventory.currentlySelectedItem, value);
							inventory.currentlySelectedItem = -1;
							inventory.inventoryItemHeld = -1;
							value = null;
						}
						else
						{
							inventory.SetItemAt(inventory.currentlySelectedItem, heldItem);
							inventory.currentlySelectedItem = -1;
							inventory.inventoryItemHeld = -1;
							heldItem = null;
							value = null;
						}
						Game1.playSound("sandyStep");
						_highlightDictionary = null;
						_ValidateCraft();
					}
					break;
				}
				}
				return;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && item != heldItem && heldItem != null)
			{
				if (heldItem.Name == "Cloth" || (heldItem is Clothing && (bool)(heldItem as Clothing).dyeable))
				{
					_leftIngredientSpotClicked();
				}
				else
				{
					_rightIngredientSpotClicked();
				}
			}
			if (IsBusy())
			{
				return;
			}
			if (leftIngredientSpot.containsPoint(x, y))
			{
				_leftIngredientSpotClicked();
				if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && heldItem != null)
				{
					if (Game1.player.IsEquippedItem(heldItem))
					{
						heldItem = null;
					}
					else
					{
						heldItem = inventory.tryToAddItem(heldItem, "");
					}
				}
			}
			else if (rightIngredientSpot.containsPoint(x, y))
			{
				_rightIngredientSpotClicked();
				if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) && heldItem != null)
				{
					if (Game1.player.IsEquippedItem(heldItem))
					{
						heldItem = null;
					}
					else
					{
						heldItem = inventory.tryToAddItem(heldItem, "");
					}
				}
			}
			else
			{
				if (!startTailoringButton.containsPoint(x, y))
				{
					return;
				}
				if (heldItem == null)
				{
					if (CanFitCraftedItem())
					{
						if (IsValidCraft(leftIngredientSpot.item, rightIngredientSpot.item))
						{
							Game1.playSound("bigSelect");
							startTailoringButton.scale = startTailoringButton.baseScale;
							_timeUntilCraft = 1500;
							_UpdateDescriptionText();
						}
						else
						{
							Game1.playSound("sell");
						}
					}
				}
				else
				{
					Game1.playSound("sell");
				}
			}
		}

		protected virtual bool CheckHeldItem(Func<Item, bool> f = null)
		{
			return f?.Invoke(heldItem) ?? (heldItem != null);
		}

		protected void _ValidateCraft()
		{
			Item item = leftIngredientSpot.item;
			Item item2 = rightIngredientSpot.item;
			if (item == null || item2 == null)
			{
				_craftState = CraftState.MissingIngredients;
			}
			else if (item is Clothing && !(item as Clothing).dyeable)
			{
				_craftState = CraftState.NotDyeable;
			}
			else if (IsValidCraft(item, item2))
			{
				_craftState = CraftState.Valid;
				bool shouldPrismaticDye = _shouldPrismaticDye;
				Item one = item.getOne();
				if (IsMultipleResultCraft(item, item2))
				{
					_isMultipleResultCraft = true;
				}
				else
				{
					_isMultipleResultCraft = false;
				}
				craftResultDisplay.item = CraftItem(one, item2.getOne());
				if (craftResultDisplay.item == one)
				{
					_isDyeCraft = true;
				}
				else
				{
					_isDyeCraft = false;
				}
				_shouldPrismaticDye = shouldPrismaticDye;
			}
			else
			{
				_craftState = CraftState.InvalidRecipe;
			}
			_UpdateDescriptionText();
		}

		protected void _UpdateDescriptionText()
		{
			if (IsBusy())
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_Busy");
			}
			else if (_craftState == CraftState.NotDyeable)
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_NotDyeable");
			}
			else if (_craftState == CraftState.MissingIngredients)
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_MissingIngredients");
			}
			else if (_craftState == CraftState.Valid)
			{
				if (!CanFitCraftedItem())
				{
					displayedDescription = Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588");
				}
				else
				{
					displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_Valid");
				}
			}
			else if (_craftState == CraftState.InvalidRecipe)
			{
				displayedDescription = Game1.content.LoadString("Strings\\UI:Tailor_InvalidRecipe");
			}
			else
			{
				displayedDescription = "";
			}
		}

		public static Color? GetDyeColor(Item dye_object)
		{
			if (dye_object != null)
			{
				if (dye_object.Name == "Prismatic Shard")
				{
					return Color.White;
				}
				if (dye_object is ColoredObject)
				{
					return (dye_object as ColoredObject).color;
				}
				Dictionary<string, Color> dictionary = new Dictionary<string, Color>();
				dictionary["black"] = new Color(45, 45, 45);
				dictionary["gray"] = Color.Gray;
				dictionary["white"] = Color.White;
				dictionary["pink"] = new Color(255, 163, 186);
				dictionary["red"] = new Color(220, 0, 0);
				dictionary["orange"] = new Color(255, 128, 0);
				dictionary["yellow"] = new Color(255, 230, 0);
				dictionary["green"] = new Color(10, 143, 0);
				dictionary["blue"] = new Color(46, 85, 183);
				dictionary["purple"] = new Color(115, 41, 181);
				dictionary["brown"] = new Color(130, 73, 37);
				dictionary["light_cyan"] = new Color(180, 255, 255);
				dictionary["cyan"] = Color.Cyan;
				dictionary["aquamarine"] = Color.Aquamarine;
				dictionary["sea_green"] = Color.SeaGreen;
				dictionary["lime"] = Color.Lime;
				dictionary["yellow_green"] = Color.GreenYellow;
				dictionary["pale_violet_red"] = Color.PaleVioletRed;
				dictionary["salmon"] = new Color(255, 85, 95);
				dictionary["jade"] = new Color(130, 158, 93);
				dictionary["sand"] = Color.NavajoWhite;
				dictionary["poppyseed"] = new Color(82, 47, 153);
				dictionary["dark_red"] = Color.DarkRed;
				dictionary["dark_orange"] = Color.DarkOrange;
				dictionary["dark_yellow"] = Color.DarkGoldenrod;
				dictionary["dark_green"] = Color.DarkGreen;
				dictionary["dark_blue"] = Color.DarkBlue;
				dictionary["dark_purple"] = Color.DarkViolet;
				dictionary["dark_pink"] = Color.DeepPink;
				dictionary["dark_cyan"] = Color.DarkCyan;
				dictionary["dark_gray"] = Color.DarkGray;
				dictionary["dark_brown"] = Color.SaddleBrown;
				dictionary["gold"] = Color.Gold;
				dictionary["copper"] = new Color(179, 85, 0);
				dictionary["iron"] = new Color(197, 213, 224);
				dictionary["iridium"] = new Color(105, 15, 255);
				foreach (string key in dictionary.Keys)
				{
					if (dye_object.HasContextTag("color_" + key))
					{
						return dictionary[key];
					}
				}
			}
			return null;
		}

		public bool DyeItems(Clothing clothing, Item dye_object, float dye_strength_override = -1f)
		{
			if (dye_object.Name == "Prismatic Shard")
			{
				clothing.Dye(Color.White, 1f);
				clothing.isPrismatic.Set(newValue: true);
				return true;
			}
			Color? dyeColor = GetDyeColor(dye_object);
			if (dyeColor.HasValue)
			{
				float strength = 0.25f;
				if (dye_object.HasContextTag("dye_medium"))
				{
					strength = 0.5f;
				}
				if (dye_object.HasContextTag("dye_strong"))
				{
					strength = 1f;
				}
				if (dye_strength_override >= 0f)
				{
					strength = dye_strength_override;
				}
				clothing.Dye(dyeColor.Value, strength);
				if (clothing == Game1.player.shirtItem.Value || clothing == Game1.player.pantsItem.Value)
				{
					Game1.player.FarmerRenderer.MarkSpriteDirty();
				}
				return true;
			}
			return false;
		}

		public TailorItemRecipe GetRecipeForItems(Item left_item, Item right_item)
		{
			foreach (TailorItemRecipe tailoringRecipe in _tailoringRecipes)
			{
				bool flag = false;
				if (tailoringRecipe.FirstItemTags != null && tailoringRecipe.FirstItemTags.Count > 0)
				{
					if (left_item == null)
					{
						continue;
					}
					foreach (string firstItemTag in tailoringRecipe.FirstItemTags)
					{
						if (!left_item.HasContextTag(firstItemTag))
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					continue;
				}
				if (tailoringRecipe.SecondItemTags != null && tailoringRecipe.SecondItemTags.Count > 0)
				{
					if (right_item == null)
					{
						continue;
					}
					foreach (string secondItemTag in tailoringRecipe.SecondItemTags)
					{
						if (!right_item.HasContextTag(secondItemTag))
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					return tailoringRecipe;
				}
			}
			return null;
		}

		public bool IsValidCraft(Item left_item, Item right_item)
		{
			if (left_item == null || right_item == null)
			{
				return false;
			}
			if (left_item is Boots && right_item is Boots)
			{
				return true;
			}
			if (left_item is Clothing && (left_item as Clothing).dyeable.Value)
			{
				if (right_item.HasContextTag("color_prismatic"))
				{
					return true;
				}
				if (GetDyeColor(right_item).HasValue)
				{
					return true;
				}
			}
			TailorItemRecipe recipeForItems = GetRecipeForItems(left_item, right_item);
			if (recipeForItems != null)
			{
				return true;
			}
			return false;
		}

		public bool IsMultipleResultCraft(Item left_item, Item right_item)
		{
			TailorItemRecipe recipeForItems = GetRecipeForItems(left_item, right_item);
			if (recipeForItems != null && recipeForItems.CraftedItemIDs != null && recipeForItems.CraftedItemIDs.Count > 0)
			{
				return true;
			}
			return false;
		}

		public Item CraftItem(Item left_item, Item right_item)
		{
			if (left_item == null || right_item == null)
			{
				return null;
			}
			if (left_item is Boots && left_item is Boots)
			{
				(left_item as Boots).applyStats(right_item as Boots);
				return left_item;
			}
			if (left_item is Clothing && (left_item as Clothing).dyeable.Value)
			{
				if (right_item.HasContextTag("color_prismatic"))
				{
					_shouldPrismaticDye = true;
					return left_item;
				}
				if (DyeItems(left_item as Clothing, right_item))
				{
					return left_item;
				}
			}
			TailorItemRecipe recipeForItems = GetRecipeForItems(left_item, right_item);
			if (recipeForItems != null)
			{
				int num = recipeForItems.CraftedItemID;
				if (recipeForItems != null && recipeForItems.CraftedItemIDs != null && recipeForItems.CraftedItemIDs.Count > 0)
				{
					num = int.Parse(Utility.GetRandom(recipeForItems.CraftedItemIDs));
				}
				Item item = null;
				item = ((num < 0) ? new Object(-num, 1) : ((num < 2000 || num >= 3000) ? ((Item)new Clothing(num)) : ((Item)new Hat(num - 2000))));
				if (item != null && item is Clothing)
				{
					DyeItems(item as Clothing, right_item, 1f);
				}
				return item;
			}
			return null;
		}

		public void SpendRightItem()
		{
			if (rightIngredientSpot.item != null)
			{
				rightIngredientSpot.item.Stack--;
				if (rightIngredientSpot.item.Stack <= 0 || rightIngredientSpot.item.maximumStackSize() == 1)
				{
					rightIngredientSpot.item = null;
				}
			}
		}

		public void SpendLeftItem()
		{
			if (leftIngredientSpot.item != null)
			{
				leftIngredientSpot.item.Stack--;
				if (leftIngredientSpot.item.Stack <= 0)
				{
					leftIngredientSpot.item = null;
				}
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
			if (IsBusy())
			{
				return;
			}
			hoveredItem = null;
			base.performHoverAction(x, y);
			hoverText = "";
			for (int i = 0; i < equipmentIcons.Count; i++)
			{
				if (equipmentIcons[i].containsPoint(x, y))
				{
					if (equipmentIcons[i].name == "Shirt")
					{
						hoveredItem = Game1.player.shirtItem.Value;
					}
					else if (equipmentIcons[i].name == "Hat")
					{
						hoveredItem = Game1.player.hat.Value;
					}
					else if (equipmentIcons[i].name == "Pants")
					{
						hoveredItem = Game1.player.pantsItem.Value;
					}
				}
			}
			if (craftResultDisplay.visible && craftResultDisplay.containsPoint(x, y) && craftResultDisplay.item != null)
			{
				if (_isDyeCraft || Game1.player.HasTailoredThisItem(craftResultDisplay.item))
				{
					hoveredItem = craftResultDisplay.item;
				}
				else
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Tailor_MakeResultUnknown");
				}
			}
			if (leftIngredientSpot.containsPoint(x, y))
			{
				if (leftIngredientSpot.item != null)
				{
					hoveredItem = leftIngredientSpot.item;
				}
				else
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Tailor_Feed");
				}
			}
			rightIngredientSpot.tryHover(x, y);
			leftIngredientSpot.tryHover(x, y);
			if (_craftState == CraftState.Valid)
			{
				startTailoringButton.tryHover(x, y, 0.33f);
			}
			else
			{
				startTailoringButton.tryHover(-999, -999);
			}
			if (rightIngredientSpot.containsPoint(x, y) && rightIngredientSpot.item == null)
			{
				hoverText = Game1.content.LoadString("Strings\\UI:Tailor_Spool");
			}
			rightIngredientSpot.tryHover(x, y);
			leftIngredientSpot.tryHover(x, y);
			if (_craftState == CraftState.Valid && CanFitCraftedItem())
			{
				startTailoringButton.tryHover(x, y, 0.33f);
			}
			else
			{
				startTailoringButton.tryHover(-999, -999);
			}
		}

		public bool CanFitCraftedItem()
		{
			if (craftResultDisplay.item != null && !Utility.canItemBeAddedToThisInventoryList(craftResultDisplay.item, inventory.actualInventory))
			{
				return false;
			}
			return true;
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			string[] obj = new string[10] { "meow:", null, null, null, null, null, null, null, null, null };
			Rectangle rectangle = oldBounds;
			obj[1] = rectangle.ToString();
			obj[2] = " ";
			rectangle = newBounds;
			obj[3] = rectangle.ToString();
			obj[4] = " ";
			obj[5] = width.ToString();
			obj[6] = " ";
			obj[7] = height.ToString();
			obj[8] = " ";
			obj[9] = yPositionOnScreen.ToString();
			Console.WriteLine(string.Concat(obj));
			base.gameWindowSizeChanged(oldBounds, newBounds);
			Console.WriteLine("meow2:" + yPositionOnScreen);
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
			questionMarkOffset.X = (float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.5) * 4f;
			questionMarkOffset.Y = (float)Math.Cos(time.TotalGameTime.TotalSeconds * 5.0) * -4f;
			bool flag = CanFitCraftedItem();
			if (_craftState == CraftState.Valid && flag)
			{
				startTailoringButton.sourceRect.Y = 104;
			}
			else
			{
				startTailoringButton.sourceRect.Y = 80;
			}
			if (_craftState == CraftState.Valid && !IsBusy() && flag)
			{
				craftResultDisplay.visible = true;
			}
			else
			{
				craftResultDisplay.visible = false;
			}
			if (_timeUntilCraft > 0)
			{
				startTailoringButton.tryHover(startTailoringButton.bounds.Center.X, startTailoringButton.bounds.Center.Y, 0.33f);
				Vector2 vector = new Vector2(0f, 0f);
				vector.X = Utility.Lerp(leftIngredientEndSpot.X, leftIngredientStartSpot.X, (float)_timeUntilCraft / 1500f);
				vector.Y = Utility.Lerp(leftIngredientEndSpot.Y, leftIngredientStartSpot.Y, (float)_timeUntilCraft / 1500f);
				leftIngredientSpot.bounds.X = (int)vector.X;
				leftIngredientSpot.bounds.Y = (int)vector.Y;
				_timeUntilCraft -= time.ElapsedGameTime.Milliseconds;
				needleSprite.bounds.Location = new Point((int)needlePosition.X, (int)(needlePosition.Y - 2f * ((float)_timeUntilCraft % 25f) / 25f * 4f));
				presserSprite.bounds.Location = new Point((int)presserPosition.X, (int)(presserPosition.Y - 1f * ((float)_timeUntilCraft % 50f) / 50f * 4f));
				_rightItemOffset = (float)Math.Sin(time.TotalGameTime.TotalMilliseconds * 2.0 * Math.PI / 180.0) * 2f;
				if (_timeUntilCraft > 0)
				{
					return;
				}
				TailorItemRecipe recipeForItems = GetRecipeForItems(leftIngredientSpot.item, rightIngredientSpot.item);
				_shouldPrismaticDye = false;
				Item item = CraftItem(leftIngredientSpot.item, rightIngredientSpot.item);
				if (leftIngredientSpot.item == item)
				{
					leftIngredientSpot.item = null;
				}
				else
				{
					SpendLeftItem();
				}
				if ((recipeForItems == null || recipeForItems.SpendRightItem) && (readyToClose() || !_shouldPrismaticDye))
				{
					SpendRightItem();
				}
				if (recipeForItems != null)
				{
					Game1.player.MarkItemAsTailored(item);
				}
				if (_sewingSound != null && _sewingSound.IsPlaying)
				{
					_sewingSound.Stop(AudioStopOptions.Immediate);
				}
				Game1.playSound("coin");
				heldItem = item;
				_timeUntilCraft = 0;
				_ValidateCraft();
				if (_shouldPrismaticDye)
				{
					Item item2 = heldItem;
					heldItem = null;
					if (readyToClose())
					{
						exitThisMenuNoSound();
						Game1.activeClickableMenu = new CharacterCustomization(item as Clothing);
						return;
					}
					heldItem = item2;
				}
				if (heldItem != null)
				{
					if (IsHoldingEquippedItem())
					{
						if (heldItem is Hat)
						{
							Game1.player.hat.Value = null;
						}
						else if ((heldItem as Clothing).clothesType.Value == 0)
						{
							Game1.player.shirtItem.Value = null;
						}
						else if ((heldItem as Clothing).clothesType.Value == 1)
						{
							Game1.player.pantsItem.Value = null;
						}
					}
					heldItem = inventory.tryToAddItem(heldItem);
					if (heldItem != null)
					{
						Game1.playSound("throwDownITem");
						Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
					}
					heldItem = null;
					_highlightDictionary = null;
				}
			}
			_rightItemOffset = 0f;
			leftIngredientSpot.bounds.X = (int)leftIngredientStartSpot.X;
			leftIngredientSpot.bounds.Y = (int)leftIngredientStartSpot.Y;
			needleSprite.bounds.Location = new Point((int)needlePosition.X, (int)needlePosition.Y);
			presserSprite.bounds.Location = new Point((int)presserPosition.X, (int)presserPosition.Y);
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.75f);
			Game1.DrawBox(xPositionOnScreen + 16, yPositionOnScreen + 16, dyePanelX, dyePanelHeight + 96, new Color(50, 160, 255));
			int num = yPositionOnScreen;
			int num2 = xPositionOnScreen;
			yPositionOnScreen = -80;
			xPositionOnScreen -= 16;
			Game1.drawDialogueBox(dyePanelX - 16, -80, dyePanelWidth + 28, dyePanelHeight + 96, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe: true, red, green, blue);
			Game1.drawDialogueBox(infoBox.X - 28, -80, infoBox.Width + 44, infoBox.Height + 96, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe: true, red, green, blue);
			Game1.drawDialogueBox(bottomInv.X - 16, bottomInv.Y - 102, bottomInv.Width + 32, bottomInv.Height + 121, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe: true, red, green, blue);
			int num3 = base.width;
			base.width += 32;
			base.width = num3;
			drawVerticalUpperIntersectingPartition(b, dyePanelX - 21, bottomInv.Y - 32 + 2, red, green, blue);
			drawVerticalUpperIntersectingPartition(b, infoBox.X - 38, bottomInv.Y - 32 + 2, red, green, blue);
			yPositionOnScreen = num;
			xPositionOnScreen = num2;
			b.Draw(tailoringTextures, dyePanelRect, new Rectangle(0, 0, 142, 80), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.087f);
			base.draw(b, drawUpperPortion: true, drawDescriptionArea: true, red, green, blue);
			startTailoringButton.draw(b, Color.White, 0.096f);
			startTailoringButton.drawItem(b, 16, 16);
			presserSprite.draw(b, Color.White, 0.099f);
			needleSprite.draw(b, Color.White, 0.097f);
			Point point = new Point(0, 0);
			if (!IsBusy())
			{
				if (leftIngredientSpot.item != null)
				{
					blankLeftIngredientSpot.draw(b, Color.White, 0.087f);
				}
				else
				{
					leftIngredientSpot.draw(b, Color.White, 0.087f, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000 / 200);
				}
			}
			else
			{
				point.X = Game1.random.Next(-1, 2);
				point.Y = Game1.random.Next(-1, 2);
			}
			leftIngredientSpot.drawItem(b, (4 + point.X) * 4, (4 + point.Y) * 4);
			if (craftResultDisplay.visible)
			{
				string text = Game1.content.LoadString("Strings\\UI:Tailor_MakeResult");
				Utility.drawTextWithColoredShadow(position: new Vector2((float)craftResultDisplay.bounds.Center.X - Game1.smallFont.MeasureString(text).X / 2f, (float)craftResultDisplay.bounds.Top - Game1.smallFont.MeasureString(text).Y), b: b, text: text, font: Game1.smallFont, color: Game1.textColor * 0.75f, shadowColor: Color.Black * 0.2f, scale: 1f, layerDepth: 0.08f);
				craftResultDisplay.draw(b);
				if (craftResultDisplay.item != null)
				{
					if (_isMultipleResultCraft)
					{
						Rectangle bounds = craftResultDisplay.bounds;
						bounds.X += 6;
						bounds.Y -= 8 + (int)questionMarkOffset.Y;
						b.Draw(tailoringTextures, bounds, new Rectangle(112, 208, 16, 16), Color.White, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0.08f);
					}
					else if (_isDyeCraft || Game1.player.HasTailoredThisItem(craftResultDisplay.item))
					{
						craftResultDisplay.drawItem(b);
					}
					else
					{
						if (craftResultDisplay.item is Hat)
						{
							b.Draw(tailoringTextures, craftResultDisplay.bounds, new Rectangle(96, 208, 16, 16), Color.White, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0.08f);
						}
						else if (craftResultDisplay.item is Clothing)
						{
							if ((craftResultDisplay.item as Clothing).clothesType.Value == 1)
							{
								b.Draw(tailoringTextures, craftResultDisplay.bounds, new Rectangle(64, 208, 16, 16), Color.White, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0.08f);
							}
							else if ((craftResultDisplay.item as Clothing).clothesType.Value == 0)
							{
								b.Draw(tailoringTextures, craftResultDisplay.bounds, new Rectangle(80, 208, 16, 16), Color.White, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0.08f);
							}
						}
						else if (craftResultDisplay.item is Object item && Utility.IsNormalObjectAtParentSheetIndex(item, 71))
						{
							b.Draw(tailoringTextures, craftResultDisplay.bounds, new Rectangle(64, 208, 16, 16), Color.White);
						}
						Rectangle bounds2 = craftResultDisplay.bounds;
						bounds2.X += 24;
						bounds2.Y += 12 + (int)questionMarkOffset.Y;
						b.Draw(tailoringTextures, bounds2, new Rectangle(112, 208, 16, 16), Color.White, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0.08f);
					}
				}
			}
			else if (descriptionText != "")
			{
				Utility.drawMultiLineTextWithShadow(position: new Vector2(infoBox.X + 12, infoBox.Y + 12), b: b, text: displayedDescription, font: Game1.smallFont, width: infoBox.Width - 64, height: infoBox.Height - 24, col: Game1.textColor);
			}
			foreach (ClickableComponent equipmentIcon in equipmentIcons)
			{
				switch (equipmentIcon.name)
				{
				case "Hat":
					if (Game1.player.hat.Value != null)
					{
						b.Draw(tailoringTextures, equipmentIcon.bounds, new Rectangle(0, 208, 16, 16), Color.White);
						float transparency3 = 1f;
						if (!HighlightItems((Hat)Game1.player.hat))
						{
							transparency3 = 0.5f;
						}
						if (Game1.player.hat.Value == heldItem)
						{
							transparency3 = 0.5f;
						}
						Game1.player.hat.Value.drawInMenu(b, new Vector2(equipmentIcon.bounds.X, equipmentIcon.bounds.Y), equipmentIcon.scale * widthMod, transparency3, 0.866f, StackDrawType.Hide);
					}
					else
					{
						b.Draw(tailoringTextures, equipmentIcon.bounds, new Rectangle(48, 208, 16, 16), Color.White);
					}
					break;
				case "Shirt":
					if (Game1.player.shirtItem.Value != null)
					{
						b.Draw(tailoringTextures, equipmentIcon.bounds, new Rectangle(0, 208, 16, 16), Color.White);
						float transparency2 = 1f;
						if (!HighlightItems((Clothing)Game1.player.shirtItem))
						{
							transparency2 = 0.5f;
						}
						if (Game1.player.shirtItem.Value == heldItem)
						{
							transparency2 = 0.5f;
						}
						Game1.player.shirtItem.Value.drawInMenu(b, new Vector2(equipmentIcon.bounds.X, equipmentIcon.bounds.Y), equipmentIcon.scale * widthMod, transparency2, 0.866f);
					}
					else
					{
						b.Draw(tailoringTextures, equipmentIcon.bounds, new Rectangle(32, 208, 16, 16), Color.White);
					}
					break;
				case "Pants":
					if (Game1.player.pantsItem.Value != null)
					{
						b.Draw(tailoringTextures, equipmentIcon.bounds, new Rectangle(0, 208, 16, 16), Color.White);
						float transparency = 1f;
						if (!HighlightItems((Clothing)Game1.player.pantsItem))
						{
							transparency = 0.5f;
						}
						if (Game1.player.pantsItem.Value == heldItem)
						{
							transparency = 0.5f;
						}
						Game1.player.pantsItem.Value.drawInMenu(b, new Vector2(equipmentIcon.bounds.X, equipmentIcon.bounds.Y), equipmentIcon.scale * widthMod, transparency, 0.866f);
					}
					else
					{
						b.Draw(tailoringTextures, equipmentIcon.bounds, new Rectangle(16, 208, 16, 16), Color.White);
					}
					break;
				}
			}
			if (!IsBusy())
			{
				if (rightIngredientSpot.item != null)
				{
					blankRightIngredientSpot.draw(b, Color.White, 0.087f);
				}
				else
				{
					rightIngredientSpot.draw(b, Color.White, 0.087f, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000 / 200);
				}
			}
			rightIngredientSpot.drawItem(b, 16, (4 + (int)_rightItemOffset) * 4);
			if (heldItem != null && Game1.player.IsEquippedItem(heldItem))
			{
				Vector2 location = new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY() - 128);
				heldItem.drawInMenu(b, location, 2f, 0.5f, 0.0865f);
			}
			inventory.drawDragItem(b);
			inventory.drawInfoPanel(b, force: true);
			_ = Game1.options.hardwareCursor;
		}

		protected override void cleanupBeforeExit()
		{
			_OnCloseMenu();
		}

		protected void _OnCloseMenu()
		{
			if (!Game1.player.IsEquippedItem(heldItem))
			{
				Utility.CollectOrDrop(heldItem);
			}
			if (!Game1.player.IsEquippedItem(leftIngredientSpot.item))
			{
				Utility.CollectOrDrop(leftIngredientSpot.item);
			}
			if (!Game1.player.IsEquippedItem(rightIngredientSpot.item))
			{
				Utility.CollectOrDrop(rightIngredientSpot.item);
			}
			if (!Game1.player.IsEquippedItem(startTailoringButton.item))
			{
				Utility.CollectOrDrop(startTailoringButton.item);
			}
			heldItem = null;
			leftIngredientSpot.item = null;
			rightIngredientSpot.item = null;
			startTailoringButton.item = null;
		}
	}
}
