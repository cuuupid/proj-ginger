using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	public class JunimoNoteMenu : IClickableMenu
	{
		public const int region_ingredientSlotModifier = 250;

		public const int region_ingredientListModifier = 1000;

		public const int region_bundleModifier = 5000;

		public const int region_areaNextButton = 101;

		public const int region_areaBackButton = 102;

		public const int region_backButton = 103;

		public const int region_purchaseButton = 104;

		public const int region_presentButton = 105;

		public const string noteTextureName = "LooseSprites\\JunimoNote";

		public Texture2D noteTexture;

		private bool specificBundlePage;

		public const int baseWidth = 320;

		public const int baseHeight = 180;

		public InventoryMenu inventory;

		public Item partialDonationItem;

		public List<Item> partialDonationComponents = new List<Item>();

		public BundleIngredientDescription? currentPartialIngredientDescription;

		public int currentPartialIngredientDescriptionIndex = -1;

		private Item heldItem;

		private Item hoveredItem;

		public static bool canClick = true;

		private int whichArea;

		public bool bundlesChanged;

		public static ScreenSwipe screenSwipe;

		public static string hoverText = "";

		public List<Bundle> bundles = new List<Bundle>();

		public static List<TemporaryAnimatedSprite> tempSprites = new List<TemporaryAnimatedSprite>();

		public List<ClickableTextureComponent> ingredientSlots = new List<ClickableTextureComponent>();

		public List<ClickableTextureComponent> ingredientList = new List<ClickableTextureComponent>();

		public List<ClickableTextureComponent> otherClickableComponents = new List<ClickableTextureComponent>();

		public bool fromGameMenu;

		public bool fromThisMenu;

		public bool scrambledText;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent purchaseButton;

		public ClickableTextureComponent areaNextButton;

		public ClickableTextureComponent areaBackButton;

		public ClickableAnimatedComponent presentButton;

		private Bundle currentPageBundle;

		private Texture2D mobileBackground;

		private int areaBackX;

		private int areaBackY;

		private int forwardX;

		private int forwardY;

		private int startX;

		private int backX;

		private int backY;

		private int leftX;

		private int rightX;

		private int centX;

		private int textY;

		private float widthMod;

		private float heightMod;

		private Rectangle inventoryRect;

		private int goldX;

		private int goldY;

		private int highlightedBundle;

		private int _selectedItemIndex = -1;

		private bool highlightPurchaseButton;

		private bool pressedOnBundleSpecificPage;

		public JunimoNoteMenu(bool fromGameMenu, int area = 1, bool fromThisMenu = false)
		{
			CommunityCenter communityCenter = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
			hoverText = "";
			if (fromGameMenu && !fromThisMenu)
			{
				for (int i = 0; i < communityCenter.areasComplete.Count; i++)
				{
					if (communityCenter.shouldNoteAppearInArea(i) && !communityCenter.areasComplete[i])
					{
						area = i;
						whichArea = area;
						break;
					}
				}
				if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible") && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
				{
					area = 6;
				}
			}
			setUpMenu(area, communityCenter.bundlesDict());
			Game1.player.forceCanMove();
			pressedOnBundleSpecificPage = false;
			highlightedBundle = -1;
			areaNextButton = new ClickableTextureComponent(new Rectangle(forwardX, forwardY, 80, 76), Game1.mobileSpriteSheet, new Rectangle(100, 0, 20, 19), 4f, drawShadow: true)
			{
				visible = false
			};
			areaBackButton = new ClickableTextureComponent(new Rectangle(areaBackX, areaBackY, 80, 76), Game1.mobileSpriteSheet, new Rectangle(80, 0, 20, 19), 4f, drawShadow: true)
			{
				visible = false
			};
			int num = 6;
			for (int j = 0; j < num; j++)
			{
				if (j != area && communityCenter.shouldNoteAppearInArea(j))
				{
					areaNextButton.visible = true;
					areaBackButton.visible = true;
					break;
				}
			}
			this.fromGameMenu = fromGameMenu;
			this.fromThisMenu = fromThisMenu;
			foreach (Bundle bundle in bundles)
			{
				bundle.depositsAllowed = false;
			}
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public JunimoNoteMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
		{
			highlightedBundle = -1;
			setUpMenu(whichArea, bundlesComplete);
		}

		public override void snapToDefaultClickableComponent()
		{
			if (specificBundlePage)
			{
				currentlySnappedComponent = getComponentWithID(0);
			}
			else
			{
				currentlySnappedComponent = getComponentWithID(5000);
			}
			snapCursorToCurrentSnappedComponent();
		}

		protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
		{
			if (specificBundlePage)
			{
				return false;
			}
			return true;
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (!Game1.player.hasOrWillReceiveMail("canReadJunimoText") || oldID - 5000 < 0 || oldID - 5000 >= 10 || currentlySnappedComponent == null)
			{
				return;
			}
			int num = -1;
			int num2 = 999999;
			Point center = currentlySnappedComponent.bounds.Center;
			for (int i = 0; i < bundles.Count; i++)
			{
				if (bundles[i].myID == oldID)
				{
					continue;
				}
				int num3 = 999999;
				Point center2 = bundles[i].bounds.Center;
				switch (direction)
				{
				case 3:
					if (center2.X < center.X)
					{
						num3 = center.X - center2.X + Math.Abs(center.Y - center2.Y) * 3;
					}
					break;
				case 0:
					if (center2.Y < center.Y)
					{
						num3 = center.Y - center2.Y + Math.Abs(center.X - center2.X) * 3;
					}
					break;
				case 1:
					if (center2.X > center.X)
					{
						num3 = center2.X - center.X + Math.Abs(center.Y - center2.Y) * 3;
					}
					break;
				case 2:
					if (center2.Y > center.Y)
					{
						num3 = center2.Y - center.Y + Math.Abs(center.X - center2.X) * 3;
					}
					break;
				}
				if (num3 < 10000 && num3 < num2)
				{
					num2 = num3;
					num = i;
				}
			}
			if (num != -1)
			{
				currentlySnappedComponent = getComponentWithID(num + 5000);
				snapCursorToCurrentSnappedComponent();
				return;
			}
			switch (direction)
			{
			case 2:
				if (presentButton != null)
				{
					currentlySnappedComponent = presentButton;
					snapCursorToCurrentSnappedComponent();
					presentButton.upNeighborID = oldID;
				}
				break;
			case 3:
				if (areaBackButton != null && areaBackButton.visible)
				{
					currentlySnappedComponent = areaBackButton;
					snapCursorToCurrentSnappedComponent();
					areaBackButton.rightNeighborID = oldID;
				}
				break;
			case 1:
				if (areaNextButton != null && areaNextButton.visible)
				{
					currentlySnappedComponent = areaNextButton;
					snapCursorToCurrentSnappedComponent();
					areaNextButton.leftNeighborID = oldID;
				}
				break;
			}
		}

		public void setUpMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
		{
			noteTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JunimoNote");
			if (!Game1.player.hasOrWillReceiveMail("seenJunimoNote"))
			{
				Game1.player.removeQuest(26);
				Game1.player.mailReceived.Add("seenJunimoNote");
			}
			if (!Game1.player.hasOrWillReceiveMail("wizardJunimoNote"))
			{
				Game1.addMailForTomorrow("wizardJunimoNote");
			}
			if (!Game1.player.hasOrWillReceiveMail("hasSeenAbandonedJunimoNote") && whichArea == 6)
			{
				Game1.player.mailReceived.Add("hasSeenAbandonedJunimoNote");
			}
			scrambledText = !Game1.player.hasOrWillReceiveMail("canReadJunimoText");
			tempSprites.Clear();
			this.whichArea = whichArea;
			highlightedBundle = -1;
			goldX = 517 * Game1.uiViewport.Width / 1280;
			goldY = 0;
			initializeUpperRightCloseButton();
			mobileBackground = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JunimoNoteMobile");
			width = Game1.uiViewport.Width - Game1.xEdge * 2;
			height = Game1.uiViewport.Height;
			widthMod = (float)width / 1280f;
			heightMod = (float)height / 720f;
			startX = Game1.xEdge;
			backX = Game1.xEdge + 16;
			backY = Game1.uiViewport.Height - 80 - 16;
			areaBackX = startX + (int)((float)(8 + IClickableMenu.borderWidth) * 2f * widthMod);
			areaBackY = Game1.uiViewport.Height / 2 - 40;
			forwardX = startX + width - (int)((float)(8 + IClickableMenu.borderWidth) * 2f * widthMod) - 80;
			forwardY = areaBackY;
			inventoryRect = new Rectangle(startX + (int)(88f * widthMod), (int)(64f * heightMod), Math.Min((int)(132f * widthMod * 4f) - 32, width / 2), (int)(124f * heightMod * 4f));
			inventory = new InventoryMenu(0, 0, playerInventory: true, null, Utility.highlightSmallObjects, 36, 6, 0, 0, drawSlots: true, width / 2, height / 2, showTrash: false, showOrganizeButton: false, 0, drawHeldItem: true, inventoryRect.X + 20, 0, Math.Min((inventoryRect.Width - 36) / 6, (inventoryRect.Height - 36) / 6));
			inventoryRect.Y = (int)(80f * heightMod);
			int y = inventoryRect.Y;
			inventory.movePosition(0, inventoryRect.Y + 24 - inventory.yPositionOnScreen);
			inventoryRect.Width = inventory.squareSide * 6 + inventory.hGap * 5 + 48;
			inventoryRect.Height = inventory.squareSide * 6 + inventory.verticalGap * 6 + 96;
			leftX = inventoryRect.X + inventoryRect.Width;
			rightX = startX + (int)(widthMod * 4f * 302f);
			centX = leftX + (rightX - leftX) / 2;
			textY = (int)(heightMod * 22f * 4f) - 16 + ((height < 700) ? 40 : 160);
			for (int i = 0; i < inventory.inventory.Count; i++)
			{
				if (i >= inventory.actualInventory.Count)
				{
					inventory.inventory[i].visible = false;
				}
			}
			foreach (ClickableComponent item in inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
			{
				item.downNeighborID = -99998;
			}
			foreach (ClickableComponent item2 in inventory.GetBorder(InventoryMenu.BorderSide.Right))
			{
				item2.rightNeighborID = -99998;
			}
			inventory.dropItemInvisibleButton.visible = false;
			Dictionary<string, string> bundleData = Game1.netWorldState.Value.BundleData;
			string areaNameFromNumber = CommunityCenter.getAreaNameFromNumber(whichArea);
			int num = 0;
			foreach (string key in bundleData.Keys)
			{
				if (key.Contains(areaNameFromNumber))
				{
					int num2 = Convert.ToInt32(key.Split('/')[1]);
					bundles.Add(new Bundle(num2, bundleData[key], bundlesComplete[num2], getBundleLocationFromNumber(num), "LooseSprites\\JunimoNote", this)
					{
						myID = num + 5000,
						rightNeighborID = -7777,
						leftNeighborID = -7777,
						upNeighborID = -7777,
						downNeighborID = -7777,
						fullyImmutable = true
					});
					num++;
				}
			}
			backButton = new ClickableTextureComponent("Back", new Rectangle(Game1.xEdge + 12, Game1.uiViewport.Height / 2 - 40, 80, 76), null, null, Game1.mobileSpriteSheet, new Rectangle(80, 0, 20, 19), 4f);
			checkForRewards();
			canClick = true;
			Game1.playSound("shwip");
			bool flag = false;
			foreach (Bundle bundle in bundles)
			{
				if (!bundle.complete && !bundle.Equals(currentPageBundle))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).markAreaAsComplete(whichArea);
				exitFunction = restoreAreaOnExit;
				((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areaCompleteReward(whichArea);
			}
		}

		public virtual bool HighlightObjects(Item item)
		{
			if (partialDonationItem != null && currentPageBundle != null && currentPartialIngredientDescriptionIndex >= 0)
			{
				return currentPageBundle.IsValidItemForThisIngredientDescription(item, currentPageBundle.ingredients[currentPartialIngredientDescriptionIndex]);
			}
			return Utility.highlightSmallObjects(item);
		}

		public override bool readyToClose()
		{
			if (!specificBundlePage)
			{
				return isReadyToCloseMenuOrBundle();
			}
			return false;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!canClick)
			{
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			if (scrambledText)
			{
				return;
			}
			if (specificBundlePage)
			{
				if (backButton.containsPoint(x, y) && heldItem == null)
				{
					closeBundlePage();
				}
				if (partialDonationItem != null)
				{
					if (heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
					{
						for (int i = 0; i < ingredientSlots.Count; i++)
						{
							if (ingredientSlots[i].item == partialDonationItem)
							{
								HandlePartialDonation(heldItem, ingredientSlots[i]);
							}
						}
					}
					else
					{
						for (int j = 0; j < ingredientSlots.Count; j++)
						{
							if (ingredientSlots[j].containsPoint(x, y) && ingredientSlots[j].item == partialDonationItem)
							{
								if (heldItem != null)
								{
									HandlePartialDonation(heldItem, ingredientSlots[j]);
									return;
								}
								bool flag = true;
								ReturnPartialDonations(!flag);
								return;
							}
						}
					}
				}
				else if (heldItem != null)
				{
					if (Game1.oldKBState.IsKeyDown(Keys.LeftShift))
					{
						for (int k = 0; k < ingredientSlots.Count; k++)
						{
							if (currentPageBundle.canAcceptThisItem(heldItem, ingredientSlots[k]))
							{
								if (ingredientSlots[k].item == null)
								{
									heldItem = currentPageBundle.tryToDepositThisItem(heldItem, ingredientSlots[k], "LooseSprites\\JunimoNote");
									checkIfBundleIsComplete();
									return;
								}
							}
							else if (ingredientSlots[k].item == null)
							{
								HandlePartialDonation(heldItem, ingredientSlots[k]);
							}
						}
					}
					for (int l = 0; l < ingredientSlots.Count; l++)
					{
						if (ingredientSlots[l].containsPoint(x, y))
						{
							heldItem = currentPageBundle.tryToDepositThisItem(heldItem, ingredientSlots[l], "LooseSprites\\JunimoNote");
							if (heldItem == null || heldItem.Stack <= 0)
							{
								Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
								inventory.currentlySelectedItem = -1;
								heldItem = null;
							}
							checkIfBundleIsComplete();
							return;
						}
						if (ingredientSlots[l].item == null && inventory.getItemAt(x, y) == heldItem)
						{
							HandlePartialDonation(heldItem, ingredientSlots[l]);
						}
					}
				}
				inventory.receiveLeftClick(x, y, playSound);
				if (inventory.dragItem == -1)
				{
					heldItem = inventory.selectItemAt(x, y);
				}
				else
				{
					heldItem = inventory.actualInventory[inventory.dragItem];
				}
				if (upperRightCloseButton != null && isReadyToCloseMenuOrBundle() && upperRightCloseButton.containsPoint(x, y))
				{
					closeBundlePage();
				}
			}
			else
			{
				pressedOnBundleSpecificPage = true;
			}
		}

		public virtual void ReturnPartialDonation(Item item, bool play_sound = true)
		{
			List<Item> list = new List<Item>();
			Item item2 = Game1.player.addItemToInventory(item, list);
			foreach (Item item3 in list)
			{
				inventory.ShakeItem(item3);
			}
			if (item2 != null)
			{
				Utility.CollectOrDrop(item2);
				inventory.ShakeItem(item2);
			}
			if (play_sound)
			{
				Game1.playSound("coin");
			}
		}

		public virtual void ReturnPartialDonations(bool to_hand = true)
		{
			if (partialDonationComponents.Count > 0)
			{
				bool play_sound = true;
				foreach (Item partialDonationComponent in partialDonationComponents)
				{
					if (heldItem == null && to_hand)
					{
						Game1.playSound("dwop");
						heldItem = partialDonationComponent;
					}
					else
					{
						ReturnPartialDonation(partialDonationComponent, play_sound);
						play_sound = false;
					}
				}
			}
			ResetPartialDonation();
		}

		public virtual void ResetPartialDonation()
		{
			partialDonationComponents.Clear();
			currentPartialIngredientDescription = null;
			currentPartialIngredientDescriptionIndex = -1;
			foreach (ClickableTextureComponent ingredientSlot in ingredientSlots)
			{
				if (ingredientSlot.item == partialDonationItem)
				{
					ingredientSlot.item = null;
				}
			}
			partialDonationItem = null;
		}

		public virtual bool CanBePartiallyOrFullyDonated(Item item)
		{
			if (currentPageBundle == null)
			{
				return false;
			}
			int bundleIngredientDescriptionIndexForItem = currentPageBundle.GetBundleIngredientDescriptionIndexForItem(item);
			if (bundleIngredientDescriptionIndexForItem < 0)
			{
				return false;
			}
			BundleIngredientDescription ingredient = currentPageBundle.ingredients[bundleIngredientDescriptionIndexForItem];
			int num = 0;
			foreach (Item item2 in Game1.player.items)
			{
				if (currentPageBundle.IsValidItemForThisIngredientDescription(item2, ingredient))
				{
					num += item2.Stack;
				}
			}
			if (bundleIngredientDescriptionIndexForItem == currentPartialIngredientDescriptionIndex && partialDonationItem != null)
			{
				num += partialDonationItem.Stack;
			}
			return num >= ingredient.stack;
		}

		public virtual void HandlePartialDonation(Item item, ClickableTextureComponent slot)
		{
			if ((currentPageBundle != null && !currentPageBundle.depositsAllowed) || (partialDonationItem != null && slot.item != partialDonationItem) || !CanBePartiallyOrFullyDonated(item))
			{
				return;
			}
			if (!currentPartialIngredientDescription.HasValue)
			{
				currentPartialIngredientDescriptionIndex = currentPageBundle.GetBundleIngredientDescriptionIndexForItem(item);
				if (currentPartialIngredientDescriptionIndex != -1)
				{
					currentPartialIngredientDescription = currentPageBundle.ingredients[currentPartialIngredientDescriptionIndex];
				}
			}
			if (!currentPartialIngredientDescription.HasValue || !currentPageBundle.IsValidItemForThisIngredientDescription(item, currentPartialIngredientDescription.Value))
			{
				return;
			}
			bool flag = true;
			int num = 0;
			if (slot.item == null)
			{
				Game1.playSound("sell");
				flag = false;
				partialDonationItem = item.getOne();
				num = Math.Min(currentPartialIngredientDescription.Value.stack, item.Stack);
				partialDonationItem.Stack = num;
				item.Stack -= num;
				if (partialDonationItem is Object)
				{
					(partialDonationItem as Object).Quality = currentPartialIngredientDescription.Value.quality;
				}
				slot.item = partialDonationItem;
				slot.sourceRect.X = 512;
				slot.sourceRect.Y = 244;
			}
			else if (partialDonationItem != null)
			{
				num = Math.Min(currentPartialIngredientDescription.Value.stack - partialDonationItem.Stack, item.Stack);
				partialDonationItem.Stack += num;
				item.Stack -= num;
			}
			if (num > 0)
			{
				Item one = heldItem.getOne();
				one.Stack = num;
				foreach (Item partialDonationComponent in partialDonationComponents)
				{
					if (partialDonationComponent.canStackWith(heldItem))
					{
						one.Stack = partialDonationComponent.addToStack(one);
					}
				}
				if (one.Stack > 0)
				{
					partialDonationComponents.Add(one);
				}
				partialDonationComponents.Sort((Item a, Item b) => b.Stack.CompareTo(a.Stack));
			}
			if (item.Stack <= 0 && item == heldItem)
			{
				Game1.player.removeItemFromInventory(heldItem);
				heldItem = null;
			}
			if (partialDonationItem != null && partialDonationItem.Stack >= currentPartialIngredientDescription.Value.stack)
			{
				slot.item = null;
				partialDonationItem = currentPageBundle.tryToDepositThisItem(partialDonationItem, slot, "LooseSprites\\JunimoNote");
				if (partialDonationItem != null && partialDonationItem.Stack > 0)
				{
					ReturnPartialDonation(partialDonationItem);
				}
				partialDonationItem = null;
				ResetPartialDonation();
				checkIfBundleIsComplete();
			}
			else if (num > 0 && flag)
			{
				Game1.playSound("sell");
			}
		}

		public bool isReadyToCloseMenuOrBundle()
		{
			if (specificBundlePage && currentPageBundle != null && currentPageBundle.completionTimer > 0)
			{
				return false;
			}
			if (heldItem != null)
			{
				return false;
			}
			return true;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			hoverText = "";
			if (!fromGameMenu || specificBundlePage || !doFromGameMenuJoystick(b))
			{
				if (!specificBundlePage)
				{
					doNonSpecificBundlePageJoystick(b);
				}
				else
				{
					doSpecificBundlePageJoystick(b);
				}
			}
		}

		public void SwapPage(int direction)
		{
			if ((direction > 0 && !areaNextButton.visible) || (direction < 0 && !areaBackButton.visible))
			{
				return;
			}
			CommunityCenter communityCenter = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
			int num = whichArea;
			int num2 = 6;
			for (int i = 0; i < num2; i++)
			{
				num += direction;
				if (num < 0)
				{
					num += num2;
				}
				if (num >= num2)
				{
					num -= num2;
				}
				if (communityCenter.shouldNoteAppearInArea(num))
				{
					int num3 = -1;
					if (currentlySnappedComponent != null && (currentlySnappedComponent.myID >= 5000 || currentlySnappedComponent.myID == 101 || currentlySnappedComponent.myID == 102))
					{
						num3 = currentlySnappedComponent.myID;
					}
					JunimoNoteMenu junimoNoteMenu = (JunimoNoteMenu)(Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true, num, fromThisMenu: true));
					if (num3 >= 0)
					{
						junimoNoteMenu.currentlySnappedComponent = junimoNoteMenu.getComponentWithID(currentlySnappedComponent.myID);
						junimoNoteMenu.snapCursorToCurrentSnappedComponent();
					}
					if (junimoNoteMenu.getComponentWithID(areaNextButton.leftNeighborID) != null)
					{
						junimoNoteMenu.areaNextButton.leftNeighborID = areaNextButton.leftNeighborID;
					}
					else
					{
						junimoNoteMenu.areaNextButton.leftNeighborID = junimoNoteMenu.areaBackButton.myID;
					}
					junimoNoteMenu.areaNextButton.rightNeighborID = areaNextButton.rightNeighborID;
					junimoNoteMenu.areaNextButton.upNeighborID = areaNextButton.upNeighborID;
					junimoNoteMenu.areaNextButton.downNeighborID = areaNextButton.downNeighborID;
					if (junimoNoteMenu.getComponentWithID(areaBackButton.rightNeighborID) != null)
					{
						junimoNoteMenu.areaBackButton.leftNeighborID = areaBackButton.leftNeighborID;
					}
					else
					{
						junimoNoteMenu.areaBackButton.leftNeighborID = junimoNoteMenu.areaNextButton.myID;
					}
					junimoNoteMenu.areaBackButton.rightNeighborID = areaBackButton.rightNeighborID;
					junimoNoteMenu.areaBackButton.upNeighborID = areaBackButton.upNeighborID;
					junimoNoteMenu.areaBackButton.downNeighborID = areaBackButton.downNeighborID;
					break;
				}
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (!Game1.options.gamepadControls)
			{
				base.receiveKeyPress(key);
				if (key.Equals(Keys.Delete) && heldItem != null && heldItem.canBeTrashed())
				{
					Utility.trashItem(heldItem);
					heldItem = null;
				}
			}
		}

		private void closeBundlePage()
		{
			if (partialDonationItem != null)
			{
				ReturnPartialDonations(to_hand: false);
			}
			else if (specificBundlePage)
			{
				hoveredItem = null;
				inventory.descriptionText = "";
				if (heldItem == null)
				{
					takeDownBundleSpecificPage(currentPageBundle);
					Game1.playSound("shwip");
				}
				else
				{
					heldItem = inventory.tryToAddItem(heldItem);
				}
			}
		}

		private void reOpenThisMenu()
		{
			bool flag = specificBundlePage;
			JunimoNoteMenu junimoNoteMenu = ((!fromGameMenu && !fromThisMenu) ? new JunimoNoteMenu(whichArea, ((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundlesDict()) : new JunimoNoteMenu(fromGameMenu, whichArea, fromThisMenu));
			if (flag)
			{
				foreach (Bundle bundle in junimoNoteMenu.bundles)
				{
					if (bundle.bundleIndex == currentPageBundle.bundleIndex)
					{
						junimoNoteMenu.setUpBundleSpecificPage(bundle);
						break;
					}
				}
			}
			Game1.activeClickableMenu = junimoNoteMenu;
		}

		private void updateIngredientSlots()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			int num = 0;
			for (int i = 0; i < currentPageBundle.ingredients.Count; i++)
			{
				if (currentPageBundle.ingredients[i].completed && num < ingredientSlots.Count)
				{
					int num2 = currentPageBundle.ingredients[i].index;
					if (num2 < 0)
					{
						num2 = GetObjectOrCategoryIndex(num2);
					}
					ingredientSlots[num].item = new Object(num2, currentPageBundle.ingredients[i].stack, isRecipe: false, -1, currentPageBundle.ingredients[i].quality);
					currentPageBundle.ingredientDepositAnimation(ingredientSlots[num], "LooseSprites\\JunimoNote", skipAnimation: true);
					num++;
				}
			}
		}

		public static int GetObjectOrCategoryIndex(int category)
		{
			if (category < 0)
			{
				foreach (int key in Game1.objectInformation.Keys)
				{
					string text = Game1.objectInformation[key];
					if (text != null)
					{
						string[] array = text.Split('/');
						if (array.Length > 3 && array[3].EndsWith(category.ToString()))
						{
							return key;
						}
					}
				}
				return category;
			}
			return category;
		}

		public static void GetBundleRewards(int area, List<Item> rewards)
		{
			Dictionary<string, string> bundleData = Game1.netWorldState.Value.BundleData;
			foreach (string key in bundleData.Keys)
			{
				if (key.Contains(CommunityCenter.getAreaNameFromNumber(area)))
				{
					int num = Convert.ToInt32(key.Split('/')[1]);
					if (((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards[num])
					{
						Item itemFromStandardTextDescription = Utility.getItemFromStandardTextDescription(bundleData[key].Split('/')[1], Game1.player);
						itemFromStandardTextDescription.SpecialVariable = num;
						rewards.Add(itemFromStandardTextDescription);
					}
				}
			}
		}

		private void openRewardsMenu()
		{
			Game1.playSound("smallSelect");
			List<Item> rewards = new List<Item>();
			GetBundleRewards(whichArea, rewards);
			Game1.activeClickableMenu = new ItemGrabMenu(rewards, reverseGrab: false, showReceivingMenu: true, null, null, null, rewardGrabbed, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 0, null, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, this);
			Game1.activeClickableMenu.exitFunction = ((exitFunction != null) ? exitFunction : new onExit(reOpenThisMenu));
		}

		private void rewardGrabbed(Item item, Farmer who)
		{
			((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards[item.SpecialVariable] = false;
		}

		private void checkIfBundleIsComplete()
		{
			if (!specificBundlePage || currentPageBundle == null)
			{
				return;
			}
			int num = 0;
			foreach (ClickableTextureComponent ingredientSlot in ingredientSlots)
			{
				if (ingredientSlot.item != null && ingredientSlot.item != partialDonationItem)
				{
					num++;
				}
			}
			if (num < currentPageBundle.numberOfIngredientSlots)
			{
				return;
			}
			if (heldItem != null)
			{
				heldItem = null;
			}
			for (int i = 0; i < ((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundles[currentPageBundle.bundleIndex].Length; i++)
			{
				((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundles.FieldDict[currentPageBundle.bundleIndex][i] = true;
			}
			((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).checkForNewJunimoNotes();
			screenSwipe = new ScreenSwipe(0, -1f, -1, width, height);
			currentPageBundle.completionAnimation(this, playSound: true, 400);
			canClick = false;
			((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards[currentPageBundle.bundleIndex] = true;
			Game1.multiplayer.globalChatInfoMessage("Bundle");
			bool flag = false;
			foreach (Bundle bundle in bundles)
			{
				if (!bundle.complete && !bundle.Equals(currentPageBundle))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (whichArea == 6)
				{
					exitFunction = restoreaAreaOnExit_AbandonedJojaMart;
				}
				else
				{
					((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).markAreaAsComplete(whichArea);
					exitFunction = restoreAreaOnExit;
					((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areaCompleteReward(whichArea);
				}
			}
			else
			{
				((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).getJunimoForArea(whichArea)?.bringBundleBackToHut(Bundle.getColorFromColorIndex(currentPageBundle.bundleColor), Game1.getLocationFromName("CommunityCenter"));
			}
			checkForRewards();
		}

		private void restoreaAreaOnExit_AbandonedJojaMart()
		{
			((AbandonedJojaMart)Game1.getLocationFromName("AbandonedJojaMart")).restoreAreaCutscene();
		}

		private void restoreAreaOnExit()
		{
			if (!fromGameMenu)
			{
				((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).restoreAreaCutscene(whichArea);
			}
		}

		public void checkForRewards()
		{
			Dictionary<string, string> bundleData = Game1.netWorldState.Value.BundleData;
			foreach (string key2 in bundleData.Keys)
			{
				if (!key2.Contains(CommunityCenter.getAreaNameFromNumber(whichArea)) || bundleData[key2].Split('/')[1].Length <= 1)
				{
					continue;
				}
				int key = Convert.ToInt32(key2.Split('/')[1]);
				if (!((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards[key])
				{
					continue;
				}
				presentButton = new ClickableAnimatedComponent(new Rectangle(startX + (int)(widthMod * 148f * 4f), (int)((float)yPositionOnScreen + 480f * heightMod), 72, 72), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10783"), new TemporaryAnimatedSprite("LooseSprites\\JunimoNote", new Rectangle(548, 262, 18, 20), 70f, 4, 99999, new Vector2(-64f, -64f), flicker: false, flipped: false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true));
				if (currentPageBundle != null)
				{
					for (int i = 0; i < ((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundles[currentPageBundle.bundleIndex].Length; i++)
					{
						((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundles.FieldDict[currentPageBundle.bundleIndex][i] = true;
					}
				}
				break;
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (!canClick)
			{
				return;
			}
			if (specificBundlePage)
			{
				heldItem = inventory.rightClick(x, y, heldItem);
				if (partialDonationItem != null)
				{
					for (int i = 0; i < ingredientSlots.Count; i++)
					{
						if (!ingredientSlots[i].containsPoint(x, y) || ingredientSlots[i].item != partialDonationItem)
						{
							continue;
						}
						if (partialDonationComponents.Count <= 0)
						{
							break;
						}
						Item one = partialDonationComponents[0].getOne();
						bool flag = false;
						if (heldItem == null)
						{
							heldItem = one;
							Game1.playSound("dwop");
							flag = true;
						}
						else if (heldItem.canStackWith(one))
						{
							heldItem.addToStack(one);
							Game1.playSound("dwop");
							flag = true;
						}
						if (!flag)
						{
							break;
						}
						partialDonationComponents[0].Stack--;
						if (partialDonationComponents[0].Stack <= 0)
						{
							partialDonationComponents.RemoveAt(0);
						}
						int num = 0;
						foreach (Item partialDonationComponent in partialDonationComponents)
						{
							num += partialDonationComponent.Stack;
						}
						if (partialDonationItem != null)
						{
							partialDonationItem.Stack = num;
						}
						if (partialDonationComponents.Count == 0)
						{
							ResetPartialDonation();
						}
						break;
					}
				}
			}
			if (!specificBundlePage && isReadyToCloseMenuOrBundle())
			{
				exitThisMenu();
			}
		}

		public override void update(GameTime time)
		{
			if (specificBundlePage && currentPageBundle != null && currentPageBundle.completionTimer <= 0 && isReadyToCloseMenuOrBundle() && currentPageBundle.complete)
			{
				takeDownBundleSpecificPage(currentPageBundle);
			}
			foreach (Bundle bundle in bundles)
			{
				bundle.update(time);
			}
			for (int num = tempSprites.Count - 1; num >= 0; num--)
			{
				if (tempSprites[num].update(time))
				{
					tempSprites.RemoveAt(num);
				}
			}
			if (presentButton != null)
			{
				presentButton.update(time);
			}
			if (screenSwipe != null)
			{
				canClick = false;
				if (screenSwipe.update(time))
				{
					screenSwipe = null;
					canClick = true;
				}
			}
			if (bundlesChanged && fromGameMenu)
			{
				reOpenThisMenu();
			}
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void draw(SpriteBatch b)
		{
			upperRightCloseButton.bounds.X = Game1.uiViewport.Width - 68 - Game1.xEdge;
			if (Game1.options.showMenuBackground)
			{
				base.drawBackground(b);
			}
			else
			{
				b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
			}
			if (!specificBundlePage)
			{
				b.Draw(noteTexture, new Vector2(startX, yPositionOnScreen), new Rectangle(0, 0, 320, 180), Color.White, 0f, Vector2.Zero, new Vector2(widthMod * 4f, heightMod * 4f), SpriteEffects.None, 0.08f);
				SpriteText.drawStringHorizontallyCenteredAt(b, scrambledText ? CommunityCenter.getAreaEnglishDisplayNameFromNumber(whichArea) : CommunityCenter.getAreaDisplayNameFromNumber(whichArea), Game1.uiViewport.Width / 2, (int)((float)yPositionOnScreen + 20f * heightMod), 999999, -1, 99999, 0.88f, 0.088f, scrambledText);
				if (scrambledText)
				{
					SpriteText.drawString(b, LocalizedContentManager.CurrentLanguageLatin ? Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786") : Game1.content.LoadBaseString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786"), startX + (int)(192f * widthMod / 2f), (int)((float)yPositionOnScreen + 192f * heightMod / 2f), 999999, width - (int)(192f * widthMod), 99999, 0.88f, 0.088f, junimoText: true);
					base.draw(b);
					return;
				}
				SpriteText.drawStringWithScrollCenteredAt(b, getRewardNameForArea(whichArea), startX + width / 2, (int)(560f * heightMod));
				for (int i = 0; i < bundles.Count; i++)
				{
					if (i == highlightedBundle)
					{
						bundles[i].sprite.scale = 6f;
					}
					else
					{
						bundles[i].sprite.scale = 4f;
					}
				}
				foreach (Bundle bundle in bundles)
				{
					bundle.draw(b);
				}
				if (presentButton != null)
				{
					presentButton.draw(b);
				}
				foreach (TemporaryAnimatedSprite tempSprite in tempSprites)
				{
					tempSprite.draw(b, localPosition: true);
				}
				if (fromGameMenu)
				{
					if (areaNextButton.visible)
					{
						areaNextButton.draw(b);
					}
					if (areaBackButton.visible)
					{
						areaBackButton.draw(b);
					}
				}
			}
			else
			{
				b.Draw(mobileBackground, new Vector2(startX, yPositionOnScreen), new Rectangle(0, 0, 320, 180), Color.White, 0f, Vector2.Zero, new Vector2(widthMod * 4f, heightMod * 4f), SpriteEffects.None, 0.08f);
				IClickableMenu.drawTextureBox(b, inventoryRect.X, inventoryRect.Y, inventoryRect.Width, inventoryRect.Height, Color.White);
				if (currentPageBundle != null)
				{
					int num = currentPageBundle.bundleIndex;
					Texture2D bundleTextureOverride = noteTexture;
					int num2 = 180;
					if (currentPageBundle.bundleTextureIndexOverride >= 0)
					{
						num = currentPageBundle.bundleTextureIndexOverride;
					}
					if (currentPageBundle.bundleTextureOverride != null)
					{
						bundleTextureOverride = currentPageBundle.bundleTextureOverride;
						num2 = 0;
					}
					if (height >= 700)
					{
						b.Draw(noteTexture, new Vector2(centX - 80, yPositionOnScreen + (int)(heightMod * 22f * 4f) - 16), new Rectangle(534, 18, 40, 40), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.085f);
						b.Draw(bundleTextureOverride, new Vector2(centX - 80 + 16, yPositionOnScreen + (int)(heightMod * 22f * 4f)), new Rectangle(num * 16 * 2 % bundleTextureOverride.Width, num2 + 32 * (num * 16 * 2 / bundleTextureOverride.Width), 32, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.085f);
						SpriteFont spriteFont = ((Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label).Length > 23) ? Game1.smallFont : Game1.dialogueFont);
						float x = spriteFont.MeasureString((!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label)).X;
						b.DrawString(spriteFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)centX - x / 2f, textY) + new Vector2(2f, 2f), Game1.textShadowColor);
						b.DrawString(spriteFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)centX - x / 2f, textY) + new Vector2(0f, 2f), Game1.textShadowColor);
						b.DrawString(spriteFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)centX - x / 2f, textY) + new Vector2(2f, 0f), Game1.textShadowColor);
						b.DrawString(spriteFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)centX - x / 2f, textY), Game1.textColor * 0.9f);
					}
					else if (ingredientSlots.Count <= 0)
					{
						b.Draw(noteTexture, new Vector2(centX - 80, yPositionOnScreen + (int)(heightMod * 22f * 4f) - 16), new Rectangle(534, 18, 40, 40), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.085f);
						b.Draw(bundleTextureOverride, new Vector2(centX - 80 + 16, yPositionOnScreen + (int)(heightMod * 22f * 4f)), new Rectangle(num * 16 * 2 % bundleTextureOverride.Width, num2 + 32 * (num * 16 * 2 / bundleTextureOverride.Width), 32, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.085f);
						SpriteFont spriteFont2 = ((Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label).Length > 23) ? Game1.smallFont : Game1.dialogueFont);
						float x2 = spriteFont2.MeasureString((!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label)).X;
						b.DrawString(spriteFont2, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)centX - x2 / 2f, Game1.uiViewport.Height / 2) + new Vector2(2f, 2f), Game1.textShadowColor);
						b.DrawString(spriteFont2, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)centX - x2 / 2f, Game1.uiViewport.Height / 2) + new Vector2(0f, 2f), Game1.textShadowColor);
						b.DrawString(spriteFont2, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)centX - x2 / 2f, Game1.uiViewport.Height / 2) + new Vector2(2f, 0f), Game1.textShadowColor);
						b.DrawString(spriteFont2, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)centX - x2 / 2f, Game1.uiViewport.Height / 2), Game1.textColor * 0.9f);
					}
					else
					{
						b.Draw(noteTexture, new Vector2(centX - 60, yPositionOnScreen + (int)(heightMod * 22f * 4f) - 16), new Rectangle(534, 18, 40, 40), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.085f);
						b.Draw(bundleTextureOverride, new Vector2(centX - 60 + 12, yPositionOnScreen + (int)(heightMod * 22f * 4f - 4f)), new Rectangle(num * 16 * 2 % bundleTextureOverride.Width, num2 + 32 * (num * 16 * 2 / bundleTextureOverride.Width), 32, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.085f);
						SpriteFont smallFont = Game1.smallFont;
						int num3 = ingredientSlots[ingredientSlots.Count - 1].bounds.Y + ingredientSlots[ingredientSlots.Count - 1].bounds.Height + 16;
						float x3 = smallFont.MeasureString((!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label)).X;
						b.DrawString(smallFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)centX - x3 / 2f, num3) + new Vector2(2f, 2f), Game1.textShadowColor);
						b.DrawString(smallFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)centX - x3 / 2f, num3) + new Vector2(0f, 2f), Game1.textShadowColor);
						b.DrawString(smallFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)centX - x3 / 2f, num3) + new Vector2(2f, 0f), Game1.textShadowColor);
						b.DrawString(smallFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)centX - x3 / 2f, num3), Game1.textColor * 0.9f);
					}
				}
				if (width > 1000 && height >= 600)
				{
					SpriteText.drawStringWithScrollCenteredAt(b, getRewardNameForArea(whichArea), startX + width / 2, inventoryRect.Y + inventoryRect.Height + 8 + (Game1.uiViewport.Height - (inventoryRect.Y + inventoryRect.Height - 8) - 72) / 2);
				}
				else
				{
					SpriteText.shrinkFont(shrink: true);
					SpriteText.drawStringWithScrollCenteredAt(b, getRewardNameForArea(whichArea), startX + width / 2, inventoryRect.Y + inventoryRect.Height + 16 + (Game1.uiViewport.Height - (inventoryRect.Y + inventoryRect.Height - 16) - 72) / 2);
					SpriteText.shrinkFont(shrink: false);
				}
				backButton.draw(b);
				if (purchaseButton != null)
				{
					purchaseButton.draw(b);
					Game1.dayTimeMoneyBox.drawMoneyBox(b, goldX, goldY, oldGFX: true);
					if (highlightPurchaseButton)
					{
						IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(20, 96, 20, 20), purchaseButton.bounds.X - 4, purchaseButton.bounds.Y - 4, purchaseButton.bounds.Width + 8, purchaseButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
					}
				}
				float extraAlpha = 1f;
				if (partialDonationItem != null)
				{
					extraAlpha = 0.25f;
				}
				foreach (TemporaryAnimatedSprite tempSprite2 in tempSprites)
				{
					tempSprite2.draw(b, localPosition: true, 0, 0, extraAlpha);
				}
				foreach (ClickableTextureComponent ingredientSlot in ingredientSlots)
				{
					float num4 = 1f;
					if (partialDonationItem != null && ingredientSlot.item != partialDonationItem)
					{
						num4 = 0.25f;
					}
					if (ingredientSlot.item == null || (partialDonationItem != null && ingredientSlot.item == partialDonationItem))
					{
						ingredientSlot.draw(b, (fromGameMenu ? (Color.LightGray * 0.5f) : Color.White) * num4, 0.89f);
					}
					ingredientSlot.drawItem(b, 4, 4, num4);
				}
				for (int j = 0; j < ingredientList.Count; j++)
				{
					float num5 = 1f;
					if (currentPartialIngredientDescriptionIndex >= 0 && currentPartialIngredientDescriptionIndex != j)
					{
						num5 = 0.25f;
					}
					ClickableTextureComponent clickableTextureComponent = ingredientList[j];
					bool flag = false;
					int bundleColor = currentPageBundle.bundleColor;
					if (currentPageBundle != null && currentPageBundle.ingredients != null && j < currentPageBundle.ingredients.Count && currentPageBundle.ingredients[j].completed)
					{
						flag = true;
					}
					if (!flag)
					{
						b.Draw(Game1.shadowTexture, new Vector2(clickableTextureComponent.bounds.Center.X - Game1.shadowTexture.Bounds.Width * 4 / 2 - 4, clickableTextureComponent.bounds.Center.Y + 4), Game1.shadowTexture.Bounds, Color.White * num5, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
					}
					if (clickableTextureComponent.item != null && clickableTextureComponent.visible)
					{
						clickableTextureComponent.item.drawInMenu(b, new Vector2(clickableTextureComponent.bounds.X, clickableTextureComponent.bounds.Y), clickableTextureComponent.scale / 4f, 1f, 0.9f, StackDrawType.Draw, Color.White * (flag ? 0.25f : num5), drawShadow: false);
					}
				}
				inventory.draw(b);
			}
			base.draw(b);
			Game1.mouseCursorTransparency = 1f;
			if (heldItem != null && inventory.dragItem != -1)
			{
				inventory.drawDragItem(b);
			}
			if (inventory.descriptionText.Length > 0)
			{
				if (hoveredItem != null)
				{
					IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);
				}
			}
			else if (!specificBundlePage && highlightedBundle != -1)
			{
				string text = Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", bundles[highlightedBundle].label);
				Vector2 vector = Game1.dialogueFont.MeasureString(text) + new Vector2(32f, 32f);
				IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.dialogueFont, Game1.menuTexture, new Rectangle(0, 256, 60, 60), null, new Rectangle(0, 0, 1, 1), text, bundles[highlightedBundle].bounds.X - (int)(vector.X / 2f) + bundles[highlightedBundle].bounds.Width * 3 / 4, bundles[highlightedBundle].bounds.Y - (int)vector.Y, (int)vector.X, (int)vector.Y, Color.White, 1f, drawShadow: true, iconLeft: false, isClickable: true, heldDown: false, drawIcon: false);
			}
			else
			{
				IClickableMenu.drawHoverText(b, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText") && hoverText.Length > 0) ? "???" : hoverText, Game1.dialogueFont, -64, (int)(-164.0 * Math.Max(1.0, Math.Min(heightMod, 1.3))));
			}
			if (screenSwipe != null)
			{
				screenSwipe.draw(b);
			}
		}

		public string getRewardNameForArea(int whichArea)
		{
			return whichArea switch
			{
				3 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBoiler"), 
				5 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBulletin"), 
				1 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardCrafts"), 
				0 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardPantry"), 
				4 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardVault"), 
				2 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardFishTank"), 
				_ => "???", 
			};
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
		}

		private void setUpBundleSpecificPage(Bundle b)
		{
			tempSprites.Clear();
			currentPageBundle = b;
			specificBundlePage = true;
			if (whichArea == 4)
			{
				if (!fromGameMenu)
				{
					purchaseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + (int)(800f * widthMod), yPositionOnScreen + (int)(504f * heightMod), 260, 72), noteTexture, new Rectangle(517, 286, 65, 20), 4f)
					{
						myID = 797,
						leftNeighborID = 103
					};
					if (Game1.options.SnappyMenus)
					{
						currentlySnappedComponent = purchaseButton;
						snapCursorToCurrentSnappedComponent();
					}
				}
				return;
			}
			List<Rectangle> list = new List<Rectangle>();
			ingredientList.Clear();
			addRectangleRowsToList(list, currentPageBundle.ingredients.Count, centX, textY + 128, canShrink: true);
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				int num2 = b.ingredients[i].index;
				if (num2 < 0)
				{
					num2 = GetObjectOrCategoryIndex(num2);
				}
				if (!Game1.objectInformation.ContainsKey(num2))
				{
					continue;
				}
				string[] array = Game1.objectInformation[num2].Split('/');
				string text = array[4];
				if (b.ingredients[i].index < 0)
				{
					if (b.ingredients[i].index == -2)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569");
					}
					else if (b.ingredients[i].index == -75)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570");
					}
					else if (b.ingredients[i].index == -4)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571");
					}
					else if (b.ingredients[i].index == -5)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572");
					}
					else if (b.ingredients[i].index == -6)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573");
					}
				}
				ingredientList.Add(new ClickableTextureComponent("ingredient_list_slot", list[i], "", text, Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, currentPageBundle.ingredients[i].index, 16, 16), 4f)
				{
					item = new Object(num2, currentPageBundle.ingredients[i].stack, isRecipe: false, -1, currentPageBundle.ingredients[i].quality)
				});
				if (list[i].Y + list[i].Height > num)
				{
					num = list[i].Y + list[i].Height;
				}
			}
			int numberOfIngredientSlots = currentPageBundle.numberOfIngredientSlots;
			List<Rectangle> list2 = new List<Rectangle>();
			addRectangleRowsToList(list2, numberOfIngredientSlots, centX, num + 96);
			ingredientSlots.Clear();
			for (int j = 0; j < list2.Count; j++)
			{
				ingredientSlots.Add(new ClickableTextureComponent(list2[j], noteTexture, new Rectangle(512, 244, 18, 18), 4f));
			}
			updateIngredientSlots();
		}

		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (currentPartialIngredientDescriptionIndex >= 0)
			{
				if (ingredientSlots.Contains(b) && b.item != partialDonationItem)
				{
					return false;
				}
				if (ingredientList.Contains(b) && ingredientList.IndexOf(b as ClickableTextureComponent) != currentPartialIngredientDescriptionIndex)
				{
					return false;
				}
			}
			return (a.myID >= 5000 || a.myID == 101 || a.myID == 102) == (b.myID >= 5000 || b.myID == 101 || b.myID == 102);
		}

		private void addRectangleRowsToList(List<Rectangle> toAddTo, int numberOfItems, int centerX, int centerY, bool canShrink = false)
		{
			switch (numberOfItems)
			{
			case 1:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 1, 72, 72, 12, canShrink));
				break;
			case 2:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 2, 72, 72, 12, canShrink));
				break;
			case 3:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 3, 72, 72, 12, canShrink));
				break;
			case 4:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 4, 72, 72, 12, canShrink));
				break;
			case 5:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12, canShrink));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 2, 72, 72, 12, canShrink));
				break;
			case 6:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12, canShrink));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12, canShrink));
				break;
			case 7:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12, canShrink));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12, canShrink));
				break;
			case 8:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12, canShrink));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12, canShrink));
				break;
			case 9:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12, canShrink));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12, canShrink));
				break;
			case 10:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12, canShrink));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12, canShrink));
				break;
			case 11:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12, canShrink));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12, canShrink));
				break;
			case 12:
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12, canShrink));
				toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 6, 72, 72, 12, canShrink));
				break;
			}
		}

		private List<Rectangle> createRowOfBoxesCenteredAt(int xStart, int yStart, int numBoxes, int boxWidth, int boxHeight, int horizontalGap, bool canShrink = false)
		{
			if (width > 1200)
			{
				horizontalGap = (int)((float)horizontalGap * widthMod);
			}
			else
			{
				horizontalGap = 8;
				if (canShrink)
				{
					boxWidth = (int)((float)boxWidth * widthMod);
					boxHeight = (int)((float)boxHeight * heightMod);
				}
			}
			List<Rectangle> list = new List<Rectangle>();
			int num = xStart - numBoxes * (boxWidth + horizontalGap) / 2;
			int y = yStart - boxHeight / 2;
			for (int i = 0; i < numBoxes; i++)
			{
				list.Add(new Rectangle(num + i * (boxWidth + horizontalGap), y, boxWidth, boxHeight));
			}
			return list;
		}

		public void takeDownBundleSpecificPage(Bundle b = null)
		{
			if (!isReadyToCloseMenuOrBundle())
			{
				return;
			}
			ReturnPartialDonations(to_hand: false);
			hoveredItem = null;
			if (!specificBundlePage)
			{
				return;
			}
			if (b == null)
			{
				b = currentPageBundle;
			}
			specificBundlePage = false;
			ingredientSlots.Clear();
			ingredientList.Clear();
			tempSprites.Clear();
			purchaseButton = null;
			if (Game1.options.SnappyMenus)
			{
				if (currentPageBundle != null)
				{
					currentlySnappedComponent = currentPageBundle;
					snapCursorToCurrentSnappedComponent();
				}
				else
				{
					snapToDefaultClickableComponent();
				}
			}
		}

		private Point getBundleLocationFromNumber(int whichBundle)
		{
			Point result = new Point(xPositionOnScreen, yPositionOnScreen);
			switch (whichBundle)
			{
			case 0:
				result.X += 592;
				result.Y += 136;
				break;
			case 1:
				result.X += 392;
				result.Y += 384;
				break;
			case 2:
				result.X += 784;
				result.Y += 388;
				break;
			case 5:
				result.X += 588;
				result.Y += 276;
				break;
			case 6:
				result.X += 588;
				result.Y += 380;
				break;
			case 3:
				result.X += 304;
				result.Y += 252;
				break;
			case 4:
				result.X += 892;
				result.Y += 252;
				break;
			case 7:
				result.X += 440;
				result.Y += 164;
				break;
			case 8:
				result.X += 776;
				result.Y += 164;
				break;
			}
			result.X = startX + (int)((float)result.X * widthMod);
			result.Y = (int)((float)result.Y * heightMod);
			return result;
		}

		private void resetButtons()
		{
			if (backButton != null)
			{
				backButton.bounds.X = backX;
				backButton.bounds.Y = backY;
				backButton.drawShadow = true;
			}
			if (areaNextButton != null)
			{
				areaNextButton.bounds.X = forwardX;
				areaNextButton.bounds.Y = forwardY;
				areaNextButton.drawShadow = true;
			}
			if (areaBackButton != null)
			{
				areaBackButton.bounds.X = areaBackX;
				areaBackButton.bounds.Y = areaBackY;
				areaBackButton.drawShadow = true;
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			highlightedBundle = -1;
			if (scrambledText)
			{
				return;
			}
			if (specificBundlePage)
			{
				if (backButton.containsPoint(x, y))
				{
					backButton.bounds.X = backX - 4;
					backButton.bounds.Y = backY + 4;
					backButton.drawShadow = false;
				}
				else
				{
					backButton.bounds.X = backX;
					backButton.bounds.Y = backY;
					backButton.drawShadow = true;
				}
				inventory.leftClickHeld(x, y);
				backButton.tryHover(x, y);
				hoveredItem = inventory.hover(x, y, heldItem);
				hoverText = "";
				foreach (ClickableTextureComponent ingredient in ingredientList)
				{
					if (ingredient.bounds.Contains(x, y))
					{
						hoverText = ingredient.hoverText;
						break;
					}
				}
				if (purchaseButton != null)
				{
					if (purchaseButton.containsPoint(x, y))
					{
						purchaseButton.scale = purchaseButton.baseScale * 0.75f;
					}
					else
					{
						purchaseButton.scale = purchaseButton.baseScale;
					}
				}
				if (heldItem == null && inventory.dragItem != -1)
				{
					heldItem = inventory.actualInventory[inventory.dragItem];
				}
				if (heldItem != null)
				{
					foreach (ClickableTextureComponent ingredientSlot in ingredientSlots)
					{
						if (ingredientSlot.bounds.Contains(x, y) && CanBePartiallyOrFullyDonated(heldItem) && (partialDonationItem == null || ingredientSlot.item == partialDonationItem))
						{
							ingredientSlot.sourceRect.X = 530;
							ingredientSlot.sourceRect.Y = 262;
						}
						else
						{
							ingredientSlot.sourceRect.X = 512;
							ingredientSlot.sourceRect.Y = 244;
						}
					}
				}
				if (inventory.dragItem == -1)
				{
					return;
				}
				{
					foreach (ClickableTextureComponent ingredientSlot2 in ingredientSlots)
					{
						if (ingredientSlot2.bounds.Contains(x, y) && CanBePartiallyOrFullyDonated(inventory.actualInventory[inventory.dragItem]) && (partialDonationItem == null || ingredientSlot2.item == partialDonationItem))
						{
							ingredientSlot2.sourceRect.X = 530;
							ingredientSlot2.sourceRect.Y = 262;
						}
						else
						{
							ingredientSlot2.sourceRect.X = 512;
							ingredientSlot2.sourceRect.Y = 244;
						}
					}
					return;
				}
			}
			if (presentButton != null)
			{
				hoverText = presentButton.tryHover(x, y);
			}
			foreach (Bundle bundle in bundles)
			{
				bundle.tryHoverAction(x, y);
			}
			if (areaNextButton != null)
			{
				if (areaNextButton.containsPoint(x, y) && areaNextButton.visible)
				{
					areaNextButton.bounds.X = forwardX - 4;
					areaNextButton.bounds.Y = forwardY + 4;
					areaNextButton.drawShadow = false;
				}
				else
				{
					areaNextButton.bounds.X = forwardX;
					areaNextButton.bounds.Y = forwardY;
					areaNextButton.drawShadow = true;
				}
			}
			if (areaBackButton != null && areaBackButton.visible)
			{
				if (areaBackButton.containsPoint(x, y))
				{
					areaBackButton.bounds.X = areaBackX - 4;
					areaBackButton.bounds.Y = areaBackY + 4;
					areaBackButton.drawShadow = false;
				}
				else
				{
					areaBackButton.bounds.X = areaBackX;
					areaBackButton.bounds.Y = areaBackY;
					areaBackButton.drawShadow = true;
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			if (scrambledText)
			{
				return;
			}
			resetButtons();
			hoveredItem = null;
			hoverText = "";
			int num = 6;
			if ((areaBackButton != null && areaBackButton.visible && areaBackButton.containsPoint(x, y)) || (areaNextButton != null && areaNextButton.visible && areaNextButton.containsPoint(x, y)))
			{
				Game1.playSound("smallSelect");
			}
			if (!specificBundlePage && pressedOnBundleSpecificPage)
			{
				foreach (Bundle bundle in bundles)
				{
					if (bundle.canBeClicked() && bundle.containsPoint(x, y))
					{
						setUpBundleSpecificPage(bundle);
						Game1.playSound("shwip");
						return;
					}
				}
				if (presentButton != null && presentButton.containsPoint(x, y) && !fromGameMenu && !fromThisMenu)
				{
					openRewardsMenu();
				}
				if (!fromGameMenu)
				{
					return;
				}
				CommunityCenter communityCenter = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
				if (areaNextButton != null && areaNextButton.visible && areaNextButton.containsPoint(x, y))
				{
					for (int i = 1; i < num + 1; i++)
					{
						if (communityCenter.shouldNoteAppearInArea((whichArea + i) % num))
						{
							Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true, (whichArea + i) % num, fromThisMenu: true);
							break;
						}
					}
				}
				else
				{
					if (areaBackButton == null || !areaBackButton.visible || !areaBackButton.containsPoint(x, y))
					{
						return;
					}
					int num2 = whichArea;
					for (int j = 1; j < num + 1; j++)
					{
						num2--;
						if (num2 == -1)
						{
							num2 = num;
						}
						if (communityCenter.shouldNoteAppearInArea(num2))
						{
							Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true, num2, fromThisMenu: true);
							break;
						}
					}
				}
				return;
			}
			if (inventory.dragItem != -1)
			{
				for (int k = 0; k < ingredientSlots.Count; k++)
				{
					if (!ingredientSlots[k].containsPoint(x, y))
					{
						continue;
					}
					if (currentPageBundle.canAcceptThisItem(inventory.actualInventory[inventory.dragItem], ingredientSlots[k]))
					{
						inventory.actualInventory[inventory.dragItem] = currentPageBundle.tryToDepositThisItem(heldItem, ingredientSlots[k], "LooseSprites\\JunimoNote");
						if (inventory.actualInventory[inventory.dragItem] == null)
						{
							inventory.currentlySelectedItem = -1;
							inventory.dragItem = -1;
						}
						checkIfBundleIsComplete();
					}
					else
					{
						HandlePartialDonation(inventory.actualInventory[inventory.dragItem], ingredientSlots[k]);
					}
				}
			}
			else if (heldItem != null)
			{
				for (int l = 0; l < ingredientSlots.Count; l++)
				{
					if (!ingredientSlots[l].containsPoint(x, y))
					{
						continue;
					}
					if (currentPageBundle.canAcceptThisItem(heldItem, ingredientSlots[l]))
					{
						heldItem = currentPageBundle.tryToDepositThisItem(heldItem, ingredientSlots[l], "LooseSprites\\JunimoNote");
						if (heldItem == null)
						{
							Utility.removeItemFromInventory(inventory.currentlySelectedItem, inventory.actualInventory);
							inventory.currentlySelectedItem = -1;
						}
						checkIfBundleIsComplete();
					}
					else if (ingredientSlots[l].item == null)
					{
						HandlePartialDonation(heldItem, ingredientSlots[l]);
					}
				}
			}
			inventory.releaseLeftClick(x, y);
			if (purchaseButton == null || !purchaseButton.containsPoint(x, y))
			{
				return;
			}
			int stack = currentPageBundle.ingredients.Last().stack;
			if (Game1.player.Money >= stack)
			{
				Game1.player.Money -= stack;
				Game1.playSound("select");
				currentPageBundle.completionAnimation(this);
				if (purchaseButton != null)
				{
					purchaseButton.scale = purchaseButton.baseScale;
				}
				((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards[currentPageBundle.bundleIndex] = true;
				(Game1.getLocationFromName("CommunityCenter") as CommunityCenter).bundles[currentPageBundle.bundleIndex][0] = true;
				checkForRewards();
				bool flag = false;
				foreach (Bundle bundle2 in bundles)
				{
					if (!bundle2.complete && !bundle2.Equals(currentPageBundle))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).markAreaAsComplete(whichArea);
					exitFunction = restoreAreaOnExit;
					((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).areaCompleteReward(whichArea);
				}
				else
				{
					((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).getJunimoForArea(whichArea)?.bringBundleBackToHut(Bundle.getColorFromColorIndex(currentPageBundle.bundleColor), Game1.getLocationFromName("CommunityCenter"));
				}
			}
			else
			{
				Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
			}
		}

		public void showTestBanner()
		{
			screenSwipe = new ScreenSwipe(0, -1f, -1, width, height);
		}

		public void tryDepositItem()
		{
			if (inventory.currentlySelectedItem == -1)
			{
				return;
			}
			heldItem = inventory.actualInventory[inventory.currentlySelectedItem];
			for (int i = 0; i < ingredientSlots.Count; i++)
			{
				if (ingredientSlots[i] != null && ingredientSlots[i].item == null)
				{
					releaseLeftClick(ingredientSlots[i].bounds.X, ingredientSlots[i].bounds.Y);
					break;
				}
			}
		}

		public void doNonSpecificBundlePageJoystick(Buttons b)
		{
			if (presentButton != null && !fromGameMenu && !fromThisMenu)
			{
				if (b == Buttons.A)
				{
					openRewardsMenu();
					highlightedBundle = -1;
				}
				return;
			}
			if (b == Buttons.A && highlightedBundle != -1 && bundles[highlightedBundle].canBeClicked())
			{
				setUpBundleSpecificPage(bundles[highlightedBundle]);
				bundles[highlightedBundle].sprite.reset();
				bundles[highlightedBundle].sprite.paused = true;
				highlightedBundle = -1;
				Game1.playSound("smallSelect");
				return;
			}
			if (highlightedBundle == -1)
			{
				highlightedBundle = 0;
			}
			switch (b)
			{
			case Buttons.DPadUp:
			case Buttons.LeftThumbstickUp:
				highlightedBundle--;
				if (highlightedBundle < 0)
				{
					highlightedBundle = bundles.Count - 1;
				}
				Game1.playSound("shwip");
				break;
			case Buttons.DPadDown:
			case Buttons.LeftThumbstickDown:
				highlightedBundle++;
				if (highlightedBundle >= bundles.Count)
				{
					highlightedBundle = 0;
				}
				Game1.playSound("shwip");
				break;
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
				highlightedBundle--;
				if (highlightedBundle < 0)
				{
					highlightedBundle = bundles.Count - 1;
				}
				Game1.playSound("shwip");
				break;
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickRight:
				highlightedBundle++;
				if (highlightedBundle >= bundles.Count)
				{
					highlightedBundle = 0;
				}
				Game1.playSound("shwip");
				break;
			}
			for (int i = 0; i < bundles.Count; i++)
			{
				if (i != highlightedBundle)
				{
					bundles[i].sprite.reset();
					bundles[i].sprite.paused = true;
				}
			}
			bundles[highlightedBundle].tryHoverAction(bundles[highlightedBundle].bounds.X, bundles[highlightedBundle].bounds.Y);
		}

		public bool doFromGameMenuJoystick(Buttons b)
		{
			CommunityCenter communityCenter = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
			switch (b)
			{
			case Buttons.RightShoulder:
			case Buttons.RightTrigger:
			{
				for (int j = 1; j < 7; j++)
				{
					if (communityCenter.shouldNoteAppearInArea((whichArea + j) % 6))
					{
						Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true, (whichArea + j) % 6, fromThisMenu: true);
						return true;
					}
				}
				break;
			}
			case Buttons.LeftShoulder:
			case Buttons.LeftTrigger:
			{
				int num = whichArea;
				for (int i = 1; i < 7; i++)
				{
					num--;
					if (num == -1)
					{
						num = 5;
					}
					if (communityCenter.shouldNoteAppearInArea(num))
					{
						Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true, num, fromThisMenu: true);
						return true;
					}
				}
				break;
			}
			}
			return false;
		}

		public void doSpecificBundlePageJoystick(Buttons b)
		{
			if (b == Buttons.LeftTrigger || b == Buttons.LeftShoulder || b == Buttons.B)
			{
				heldItem = null;
				takeDownBundleSpecificPage(currentPageBundle);
				Game1.playSound("shwip");
				return;
			}
			if (purchaseButton != null)
			{
				if (highlightPurchaseButton)
				{
					if (b == Buttons.A)
					{
						releaseLeftClick(purchaseButton.bounds.X, purchaseButton.bounds.Y);
					}
					highlightPurchaseButton = false;
				}
				else
				{
					highlightPurchaseButton = true;
				}
				return;
			}
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
				_selectedItemIndex = Math.Max(0, _selectedItemIndex - 6);
				break;
			case Buttons.DPadDown:
			case Buttons.LeftThumbstickDown:
				_selectedItemIndex = Math.Min(_selectedItemIndex + 6, inventory.inventory.Count - 1);
				break;
			}
			inventory.currentlySelectedItem = _selectedItemIndex;
			if (b == Buttons.X)
			{
				tryDepositItem();
			}
		}

		public void unsetRewardGrabbed(Item item, Farmer who)
		{
			((CommunityCenter)Game1.getLocationFromName("CommunityCenter")).bundleRewards[item.SpecialVariable] = true;
		}
	}
}
