using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.Menus
{
	public class ShopMenu : IClickableMenu
	{
		public new int width;

		public new int height;

		public int edge;

		public int separator;

		public float widthMod;

		public float heightMod;

		public int invX;

		public int invY;

		public int invHeight;

		public int invWidth;

		public int goldX;

		public int goldY;

		public int notesX;

		public int notesY;

		public int notesWidth;

		public int notesHeight;

		public int portraitX;

		public int portraitY;

		public int portraitWidth;

		public int portraitHeight;

		public int itemsX;

		public int itemsY;

		public int itemsWidth;

		public int itemsHeight;

		public int itemsXoff;

		public int itemsYoff;

		public int priceX;

		public int priceY;

		public int priceWidth;

		public int priceHeight;

		public int scrollbarWidth;

		public int scrollbarHeight;

		public int currentlySelectedItem;

		public int priceItem;

		public int buyX;

		public int buyY;

		public int buyYWithSlider;

		public int buyWidth;

		public int buyHeight;

		public int maxBuyable;

		public int quantityToBuy;

		public int numUsedSlots;

		public int numOfCurrentItem;

		public int sellPanelWidth;

		public int sellPanelHeight;

		public int salePrice;

		public int savedInventoryX;

		public int savedInventoryY;

		public int itemButtonHeight;

		public int quantityToSell;

		public string descItem;

		public string nameItem;

		public ISalable currentItem;

		private Item itemPlayerIsSelling;

		public Vector2 sellPanelPosition;

		public Vector2 sellPanelTextSize;

		public ClickableComponent inventoryButton;

		public ClickableComponent buyButton;

		public ClickableComponent sellButton;

		public int baseItemButtonHeight;

		public bool inventoryButtonisHeld;

		public bool buyButtonisHeld;

		public bool sellButtonisHeld;

		public bool quantitySliderHeld;

		public bool sellQuantitySliderHeld;

		public bool inventoryVisible;

		public bool scrollBarVisible;

		public SliderBar quantitySlider;

		public SliderBar sellQuantitySlider;

		public Rectangle fadeRect;

		private string personName;

		private MobileScrollbox scrollArea;

		private tweeningSprite boughtItemTween;

		private MobileScrollbar newScrollbar;

		private bool clickReceived;

		public const int region_shopButtonModifier = 3546;

		public const int region_upArrow = 97865;

		public const int region_downArrow = 97866;

		public const int region_tabStartIndex = 99999;

		public const int howManyRecipesFitOnPage = 28;

		public const int infiniteStock = 2147483647;

		public const int salePriceIndex = 0;

		public const int stockIndex = 1;

		public const int extraTradeItemIndex = 2;

		public const int extraTradeItemCountIndex = 3;

		public int itemsPerPage = 4;

		public const int numberRequiredForExtraItemTrade = 5;

		private string descriptionText = "";

		private string hoverText = "";

		private string boldTitleText = "";

		public string purchaseSound = "purchaseClick";

		public string purchaseRepeatSound = "purchaseRepeat";

		public string storeContext = "";

		public InventoryMenu inventory;

		public ISalable heldItem;

		public ISalable hoveredItem;

		private Texture2D wallpapers;

		private Texture2D floors;

		private int lastWallpaperFloorPrice;

		private TemporaryAnimatedSprite poof;

		private Rectangle scrollBarRunner;

		public List<ISalable> forSale = new List<ISalable>();

		public List<ClickableComponent> forSaleButtons = new List<ClickableComponent>();

		public List<int> categoriesToSellHere = new List<int>();

		public Dictionary<ISalable, int[]> itemPriceAndStock = new Dictionary<ISalable, int[]>();

		private float sellPercentage = 1f;

		private List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();

		public int hoverPrice = -1;

		public int currency;

		public int currentItemIndex;

		public NPC portraitPerson;

		public string potraitPersonDialogue;

		public object source;

		private bool scrolling;

		public Func<ISalable, Farmer, int, bool> onPurchase;

		public Func<ISalable, bool> onSell;

		public Func<int, bool> canPurchaseCheck;

		public List<ClickableTextureComponent> tabButtons = new List<ClickableTextureComponent>();

		protected int currentTab;

		protected bool _isStorageShop;

		protected bool _isCatalogue;

		public bool readOnly;

		public HashSet<ISalable> buyBackItems = new HashSet<ISalable>();

		public Dictionary<ISalable, ISalable> buyBackItemsToResellTomorrow = new Dictionary<ISalable, ISalable>();

		private bool inventoryWasVisible;

		private float AButtonPolling;

		private float triggerPolling;

		private float triggerPollingAccel;

		private float aButtonPollingAccel;

		public ShopMenu(Dictionary<ISalable, int[]> itemPriceAndStock, int currency = 0, string who = null, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
			: this(itemPriceAndStock.Keys.ToList(), currency, who, on_purchase, on_sell, context)
		{
			this.itemPriceAndStock = itemPriceAndStock;
			if (potraitPersonDialogue == null)
			{
				setUpShopOwner(who);
			}
			if (currentlySelectedItem >= 0)
			{
				setCurrentItem(currentlySelectedItem);
			}
			updateNumberOfUsedInventorySlots();
			if (potraitPersonDialogue != null && portraitPerson == null)
			{
				notesY = portraitY;
				notesHeight = itemsHeight;
			}
		}

		public ShopMenu(List<ISalable> itemsForSale, int currency = 0, string who = null, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
			: base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1000 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
		{
			this.currency = currency;
			upperRightCloseButton.bounds.X = Game1.uiViewport.Width - 68 - Game1.xEdge;
			if (Game1.uiViewport.Width < 1500)
			{
				xPositionOnScreen = 32;
			}
			Game1.player.forceCanMove();
			Game1.playSound("dwop");
			inventory = new InventoryMenu(xPositionOnScreen + width, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 + 40, playerInventory: true, null, highlightItemToSell, -1, 3, 0, 0, drawSlots: true, 0, 0, showTrash: false, showOrganizeButton: false)
			{
				showGrayedOutSlots = true
			};
			inventory.movePosition(-inventory.width - 32, 0);
			this.currency = currency;
			int num = xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
			int num2 = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder;
			onPurchase = on_purchase;
			onSell = on_sell;
			inventory.canMoveItems = false;
			personName = who;
			clickReceived = false;
			inventoryButton = new ClickableComponent(new Rectangle(invX, invY, invWidth, invHeight), "inventoryButton");
			if (context == null)
			{
				context = Game1.currentLocation.name;
			}
			storeContext = context;
			setUpStoreForContext();
			if (tabButtons.Count > 0)
			{
				foreach (ClickableComponent forSaleButton in forSaleButtons)
				{
					forSaleButton.leftNeighborID = -99998;
				}
			}
			applyTab();
			initialiseMobileLayout();
			itemsPerPage = (itemsHeight - itemsYoff * 2) / (baseItemButtonHeight + 8);
			itemButtonHeight = baseItemButtonHeight;
			int num3;
			if (itemsForSale.Count > itemsPerPage)
			{
				scrollBarVisible = true;
				num3 = 44;
			}
			else
			{
				scrollBarVisible = false;
				num3 = itemsXoff + 4;
			}
			for (int i = 0; i < itemsForSale.Count; i++)
			{
				int num4 = itemsY + itemsYoff + i * (itemsHeight / itemsPerPage);
				forSaleButtons.Add(new ClickableComponent(new Rectangle(itemsX + itemsXoff, itemsY + itemsYoff + i * (itemButtonHeight + 8), itemsWidth - itemsXoff - num3, itemButtonHeight), i.ToString() ?? ""));
			}
			inventory = new InventoryMenu(portraitX, portraitY, playerInventory: true, null, highlightItemToSell, -1, 3, 0, 0, drawSlots: true, width - edge * 2, height / 2, showTrash: false, showOrganizeButton: false)
			{
				showGrayedOutSlots = true
			};
			fadeRect = new Rectangle(0, portraitY + 1, Game1.uiViewport.Width, Game1.uiViewport.Height - portraitY + 1);
			updateNumberOfUsedInventorySlots();
			newScrollbar = new MobileScrollbar(itemsX + itemsWidth - scrollbarWidth - 4, itemsY + itemsYoff, 44, itemsHeight - 32, 0, 32);
			scrollArea = new MobileScrollbox(itemsX, itemsY + itemsYoff, itemsWidth - scrollbarWidth - 4, itemsHeight, (forSaleButtons.Count - itemsPerPage) * (itemButtonHeight + 8), new Rectangle(itemsX + 16, itemsY + 16, itemsWidth - 36, itemsHeight - 32), newScrollbar);
			foreach (ISalable item in itemsForSale)
			{
				if (item is Object && (bool)(item as Object).isRecipe)
				{
					if (Game1.player.knowsRecipe(item.Name))
					{
						continue;
					}
					item.Stack = 1;
				}
				forSale.Add(item);
				itemPriceAndStock.Add(item, new int[2]
				{
					item.salePrice(),
					item.Stack
				});
			}
			if (itemPriceAndStock.Count >= 2)
			{
				setUpShopOwner(who);
			}
			switch ((string)Game1.currentLocation.name)
			{
			case "SeedShop":
				categoriesToSellHere.AddRange(new int[14]
				{
					-81, -75, -79, -80, -74, -17, -18, -6, -26, -5,
					-14, -19, -7, -25
				});
				break;
			case "Blacksmith":
				categoriesToSellHere.AddRange(new int[3] { -12, -2, -15 });
				break;
			case "ScienceHouse":
				categoriesToSellHere.AddRange(new int[1] { -16 });
				break;
			case "AnimalShop":
				categoriesToSellHere.AddRange(new int[4] { -18, -6, -5, -14 });
				break;
			case "FishShop":
				categoriesToSellHere.AddRange(new int[4] { -4, -23, -21, -22 });
				break;
			case "AdventureGuild":
				categoriesToSellHere.AddRange(new int[4] { -28, -98, -97, -96 });
				break;
			}
			checkForTutorial();
			if (forSaleButtons != null && forSaleButtons.Count > 0 && forSaleButtons[0] != null)
			{
				setCurrentItem(0);
			}
			if (currency == 4)
			{
				Game1.specialCurrencyDisplay.ShowCurrency("qiGems");
			}
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public void initialiseMobileLayout()
		{
			edge = Game1.xEdge;
			width = Game1.uiViewport.Width;
			height = Game1.uiViewport.Height;
			widthMod = (float)width / 1280f;
			heightMod = (float)height / 720f;
			separator = (int)(4f * widthMod);
			invX = edge + 4;
			invY = separator;
			invWidth = (int)(384f * widthMod);
			invHeight = (int)(79f * heightMod);
			goldX = (int)(517f * widthMod);
			portraitX = edge;
			portraitY = (int)(90f * heightMod);
			baseItemButtonHeight = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 148 : 112);
			if (personName == null || width - edge * 2 < 1100 || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				portraitWidth = 0;
				personName = null;
				portraitPerson = null;
			}
			else
			{
				portraitWidth = 300;
			}
			portraitHeight = 300;
			notesX = edge;
			notesY = portraitY + portraitHeight;
			notesWidth = portraitWidth;
			itemsWidth = Math.Max(500, (int)(554f * widthMod) + (300 - ((portraitWidth == 0) ? 300 : portraitWidth)));
			itemsX = portraitX + portraitWidth + separator;
			itemsY = portraitY;
			if (_isStorageShop)
			{
				itemsX = (width - itemsWidth) / 2;
			}
			else if (_isCatalogue && tabButtons.Count > 0)
			{
				itemsX = edge + tabButtons[0].bounds.Width;
			}
			repositionTabs();
			itemsHeight = height - itemsY;
			itemsXoff = 16;
			itemsYoff = 16;
			notesHeight = itemsHeight - portraitHeight;
			priceX = itemsX + itemsWidth + separator;
			priceY = portraitY;
			priceWidth = width - priceX - edge;
			priceHeight = itemsHeight;
			inventoryButton.bounds = new Rectangle(invX, invY, invWidth, invHeight);
			scrollbarWidth = 55;
			scrollbarHeight = (int)(35f * heightMod);
			currentlySelectedItem = -1;
			if (Game1.clientBounds.Height <= 640 || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				buyWidth = priceWidth * 4 / 9;
				buyX = priceX + 32;
				buyHeight = Math.Min(80, (int)(80f * heightMod));
				buyY = priceY + priceHeight - buyHeight * 3 / 2;
				buyYWithSlider = buyY;
				quantitySlider = new SliderBar(priceX + priceWidth - buyWidth, buyY - 8, 1);
				quantitySlider.bounds.Width = buyWidth - 32;
				quantitySlider.bounds.Height = Math.Min(100, (int)(100f * heightMod));
			}
			else
			{
				buyWidth = priceWidth * 7 / 8;
				buyX = priceX + (priceWidth - buyWidth) / 2;
				buyHeight = Math.Min(80, (int)(80f * heightMod));
				buyY = priceY + priceHeight - buyHeight * 3 / 2;
				buyYWithSlider = priceY + priceHeight - buyHeight * 2 - 28;
				quantitySlider = new SliderBar(buyX + 32, buyYWithSlider + buyHeight + 12, 1);
				quantitySlider.bounds.Width = buyWidth - 64;
			}
			numOfCurrentItem = 0;
			sellPanelPosition.X = (sellPanelPosition.Y = -1f);
			sellPanelWidth = 0;
			sellPanelHeight = (int)(250f * Math.Max(1.3f, heightMod));
			sellQuantitySliderHeld = false;
			sellQuantitySlider = new SliderBar(-1, -1, 1);
			buyButton = new ClickableComponent(new Rectangle(buyX, buyY, buyWidth, buyHeight), "");
			sellButton = new ClickableComponent(new Rectangle(0, 0, 0, 0), "");
			inventoryButtonisHeld = false;
		}

		private void updateItemButtons()
		{
			for (int i = 0; i < forSaleButtons.Count; i++)
			{
				forSaleButtons[i].bounds.Y = scrollArea.getYOffsetForScroll() + itemsY + itemsYoff + i * (itemButtonHeight + 8);
			}
		}

		public bool checkForItemsToSell()
		{
			foreach (Item item in inventory.actualInventory)
			{
				if (item != null && categoriesToSellHere != null && categoriesToSellHere.Contains(item.category))
				{
					return true;
				}
			}
			return false;
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			switch (direction)
			{
			case 2:
			{
				if (currentItemIndex < Math.Max(0, forSale.Count - itemsPerPage))
				{
					downArrowPressed();
					break;
				}
				int num = -1;
				for (int i = 0; i < 12; i++)
				{
					inventory.inventory[i].upNeighborID = oldID;
					if (num == -1 && heldItem != null && inventory.actualInventory != null && inventory.actualInventory.Count > i && inventory.actualInventory[i] == null)
					{
						num = i;
					}
				}
				currentlySnappedComponent = getComponentWithID((num != -1) ? num : 0);
				snapCursorToCurrentSnappedComponent();
				break;
			}
			case 0:
				if (currentItemIndex > 0)
				{
					upArrowPressed();
					currentlySnappedComponent = getComponentWithID(3546);
					snapCursorToCurrentSnappedComponent();
				}
				break;
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(3546);
			snapCursorToCurrentSnappedComponent();
		}

		public void setUpShopOwner(string who)
		{
			if (who == null)
			{
				return;
			}
			Random random = new Random((int)(Game1.uniqueIDForThisGame + Game1.stats.DaysPlayed));
			string text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11457");
			if (who != null)
			{
				switch (who.Length)
				{
				case 11:
					switch (who[0])
					{
					case 'D':
						if (who == "DesertTrade")
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:DesertTrader" + (random.Next(2) + 1));
							if (random.NextDouble() < 0.2)
							{
								int num = random.Next(2) + 3;
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:DesertTrader" + num + ((num == 4) ? ("_" + (Game1.player.isMale ? "male" : "female")) : ""));
							}
						}
						break;
					case 'C':
						if (who == "Concessions")
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheaterConcessions" + Game1.random.Next(5));
						}
						break;
					}
					break;
				case 5:
					switch (who[0])
					{
					case 'R':
						if (who == "Robin")
						{
							portraitPerson = Game1.getCharacterFromName("Robin");
							switch (Game1.random.Next(5))
							{
							case 0:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11460");
								break;
							case 1:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11461");
								break;
							case 2:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11462");
								break;
							case 3:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11463");
								break;
							case 4:
							{
								string displayName = itemPriceAndStock.ElementAt(Game1.random.Next(2, itemPriceAndStock.Count)).Key.DisplayName;
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11464", displayName, Lexicon.getRandomPositiveAdjectiveForEventOrPerson(), Lexicon.getProperArticleForWord(displayName));
								break;
							}
							}
						}
						break;
					case 'C':
						if (who == "Clint")
						{
							portraitPerson = Game1.getCharacterFromName("Clint");
							switch (Game1.random.Next(3))
							{
							case 0:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11469");
								break;
							case 1:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11470");
								break;
							case 2:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11471");
								break;
							}
						}
						break;
					case 'W':
						if (who == "Willy")
						{
							portraitPerson = Game1.getCharacterFromName("Willy");
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11477");
							if (Game1.random.NextDouble() < 0.05)
							{
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11478");
							}
						}
						break;
					case 'D':
						if (who == "Dwarf")
						{
							portraitPerson = Game1.getCharacterFromName("Dwarf");
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11492");
						}
						break;
					case 'S':
						if (who == "Sandy")
						{
							portraitPerson = Game1.getCharacterFromName("Sandy");
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11524");
							if (random.NextDouble() < 0.0001)
							{
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11525");
							}
						}
						break;
					}
					break;
				case 6:
					switch (who[3])
					{
					case 'r':
						if (who == "Pierre")
						{
							portraitPerson = Game1.getCharacterFromName("Pierre");
							switch (Game1.dayOfMonth % 7)
							{
							case 1:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11481");
								break;
							case 2:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11482");
								break;
							case 3:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11483");
								break;
							case 4:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11484");
								break;
							case 5:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11485");
								break;
							case 6:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11486");
								break;
							case 0:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11487");
								break;
							}
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11488") + text;
							if (Game1.dayOfMonth == 28)
							{
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11489");
							}
						}
						break;
					case 'b':
						if (who == "Krobus")
						{
							portraitPerson = Game1.getCharacterFromName("Krobus");
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11497");
						}
						break;
					case 'n':
						if (who == "Marnie")
						{
							portraitPerson = Game1.getCharacterFromName("Marnie");
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11507");
							if (random.NextDouble() < 0.0001)
							{
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11508");
							}
						}
						break;
					case 'l':
						if (who == "Marlon")
						{
							portraitPerson = Game1.getCharacterFromName("Marlon");
							switch (random.Next(4))
							{
							case 0:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11517");
								break;
							case 1:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11518");
								break;
							case 2:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11519");
								break;
							case 3:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11520");
								break;
							}
							if (random.NextDouble() < 0.001)
							{
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11521");
							}
						}
						break;
					}
					break;
				case 8:
					switch (who[0])
					{
					case 'H':
						if (who == "HatMouse")
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11494");
						}
						break;
					case 'B':
						if (who == "BlueBoat")
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.blueboat");
						}
						break;
					case 'T':
						if (who == "Traveler")
						{
							switch (random.Next(5))
							{
							case 0:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11499");
								break;
							case 1:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11500");
								break;
							case 2:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11501");
								break;
							case 3:
								text = ((itemPriceAndStock.Count <= 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11504") : Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11502", itemPriceAndStock.ElementAt(random.Next(itemPriceAndStock.Count)).Key.DisplayName));
								break;
							case 4:
								text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11504");
								break;
							}
						}
						break;
					}
					break;
				case 9:
					if (who == "boxOffice")
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheaterBoxOffice");
					}
					break;
				case 12:
					if (who == "ClintUpgrade")
					{
						portraitPerson = Game1.getCharacterFromName("Clint");
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11474");
					}
					break;
				case 13:
					if (who == "magicBoatShop")
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.magicBoat");
					}
					break;
				case 10:
					if (who == "KrobusGone")
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:KrobusShopGone");
					}
					break;
				case 19:
					if (who == "TravelerNightMarket")
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.travelernightmarket");
					}
					break;
				case 3:
					if (who == "Gus")
					{
						portraitPerson = Game1.getCharacterFromName("Gus");
						switch (Game1.random.Next(4))
						{
						case 0:
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11511");
							break;
						case 1:
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11512", itemPriceAndStock.ElementAt(random.Next(itemPriceAndStock.Count)).Key.DisplayName);
							break;
						case 2:
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11513");
							break;
						case 3:
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11514");
							break;
						}
					}
					break;
				case 15:
					if (who == "Marlon_Recovery")
					{
						portraitPerson = Game1.getCharacterFromName("Marlon");
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ItemRecovery_Description");
					}
					break;
				}
			}
			potraitPersonDialogue = Game1.parseText(text, Game1.dialogueFont, 304);
		}

		public bool highlightItemToSell(Item i)
		{
			if (i != null && categoriesToSellHere.Contains(i.Category))
			{
				return true;
			}
			return false;
		}

		public static int getPlayerCurrencyAmount(Farmer who, int currencyType)
		{
			return currencyType switch
			{
				0 => who.Money, 
				1 => who.festivalScore, 
				2 => who.clubCoins, 
				4 => who.QiGems, 
				_ => 0, 
			};
		}

		public void doGamePadButtonInventory(Buttons b)
		{
			switch (b)
			{
			case Buttons.B:
			case Buttons.Y:
				inventoryVisible = false;
				return;
			case Buttons.X:
				if (!_isStorageShop)
				{
					if (itemPlayerIsSelling == null)
					{
						InventoryReceiveLeftClick(inventory.inventory[inventory.currentlySelectedItem].bounds.X, inventory.inventory[inventory.currentlySelectedItem].bounds.Y);
					}
					else
					{
						InventoryReceiveLeftClick(0, 0);
					}
					return;
				}
				break;
			}
			if (b == Buttons.A)
			{
				if (_isStorageShop)
				{
					if (inventory.currentlySelectedItem >= 0)
					{
						itemPlayerIsSelling = inventory.actualInventory[inventory.currentlySelectedItem];
					}
					if (itemPlayerIsSelling != null)
					{
						Item item = itemPlayerIsSelling;
						sellButtonisHeld = true;
						clickReceived = true;
						inventory.leftClick(inventory.inventory[inventory.currentlySelectedItem].bounds.X, inventory.inventory[inventory.currentlySelectedItem].bounds.Y, null, playSound: false);
						sellButtonisHeld = false;
						itemPlayerIsSelling = null;
						inventory.currentlySelectedItem = -1;
						if (item != null && onSell != null && onSell(item))
						{
							item = null;
							rebuildSaleButtons();
							switchTab(0);
						}
						return;
					}
				}
				sellButtonisHeld = true;
				clickReceived = true;
				int inventoryPositionOfClick = inventory.getInventoryPositionOfClick(Game1.getMouseX(), Game1.getMouseY());
				if (inventoryPositionOfClick >= 0 && inventoryPositionOfClick < inventory.capacity && highlightItemToSell(inventory.actualInventory[inventoryPositionOfClick]))
				{
					itemPlayerIsSelling = inventory.actualInventory[inventoryPositionOfClick];
				}
				sellButton.bounds.Width = 1;
				sellButton.bounds.Height = 1;
				quantityToSell = 1;
				savedInventoryX = Game1.getMouseX();
				savedInventoryY = Game1.getMouseY();
				releaseLeftClick(sellButton.bounds.X, sellButton.bounds.Y);
				sellButtonisHeld = false;
				return;
			}
			if (itemPlayerIsSelling != null && b == Buttons.LeftTrigger)
			{
				sellQuantitySlider.changeValueBy(-10);
				quantityToSell = Math.Max(1 + (int)((float)(sellQuantitySlider.value * itemPlayerIsSelling.Stack) / 100f), 1);
				salePrice = (int)((itemPlayerIsSelling is Object) ? ((float)(itemPlayerIsSelling as Object).sellToStorePrice(-1L) * sellPercentage) : ((float)(itemPlayerIsSelling.salePrice() / 2) * sellPercentage)) * quantityToSell;
				return;
			}
			if (itemPlayerIsSelling != null && b == Buttons.RightTrigger)
			{
				if (quantityToSell == 0)
				{
					quantityToSell = 1;
				}
				sellQuantitySlider.changeValueBy(10);
				quantityToSell = Math.Min(1 + (int)((float)(sellQuantitySlider.value * itemPlayerIsSelling.Stack) / 100f), itemPlayerIsSelling.Stack);
				salePrice = (int)((itemPlayerIsSelling is Object) ? ((float)(itemPlayerIsSelling as Object).sellToStorePrice(-1L) * sellPercentage) : ((float)(itemPlayerIsSelling.salePrice() / 2) * sellPercentage)) * quantityToSell;
				return;
			}
			if (b == Buttons.DPadDown || b == Buttons.DPadUp || b == Buttons.DPadLeft || b == Buttons.DPadRight || b == Buttons.LeftThumbstickDown || b == Buttons.LeftThumbstickUp || b == Buttons.LeftThumbstickLeft || b == Buttons.LeftThumbstickRight)
			{
				if (itemPlayerIsSelling != null)
				{
					InventoryReceiveLeftClick(0, 0);
				}
				inventory.receiveKeyPress(Utility.mapGamePadButtonToKey(b));
				snapCursorToCurrentSnappedComponent();
			}
			inventory.receiveGamePadButton(b);
		}

		public override void receiveGamePadButton(Buttons b)
		{
			triggerPolling = 0f;
			triggerPollingAccel = 0f;
			AButtonPolling = 0f;
			aButtonPollingAccel = 0f;
			if (inventoryVisible)
			{
				inventoryWasVisible = true;
				doGamePadButtonInventory(b);
				return;
			}
			inventoryWasVisible = false;
			if (b == Buttons.Y)
			{
				inventoryVisible = true;
				Game1.playSound("smallSelect");
				setCurrentlySnappedComponentTo(0);
				snapCursorToCurrentSnappedComponent();
				return;
			}
			base.receiveGamePadButton(b);
			switch (b)
			{
			case Buttons.DPadUp:
			case Buttons.LeftThumbstickUp:
				if (currentlySelectedItem == 0)
				{
					inventoryVisible = true;
					Game1.playSound("smallSelect");
					setCurrentlySnappedComponentTo(0);
					snapCursorToCurrentSnappedComponent();
					return;
				}
				setCurrentItem(Math.Max(0, currentlySelectedItem - 1));
				if (scrollBarVisible)
				{
					float num = currentlySelectedItem * 100 / forSaleButtons.Count;
					int yOffsetForScroll = scrollArea.getYOffsetForScroll();
					if (yOffsetForScroll < -(itemButtonHeight + 8) * currentItemIndex)
					{
						scrollArea.setYOffsetForScroll(yOffsetForScroll + (itemButtonHeight + 8));
					}
					updateItemButtons();
				}
				return;
			case Buttons.DPadDown:
			case Buttons.LeftThumbstickDown:
				setCurrentItem(Math.Min(forSaleButtons.Count - 1, currentlySelectedItem + 1));
				if (scrollBarVisible)
				{
					float num2 = currentlySelectedItem * 100 / forSaleButtons.Count;
					scrollArea.setYOffsetForScroll(-(int)(num2 * (float)(forSaleButtons.Count - itemsPerPage) * (float)(itemButtonHeight + 8) / 100f));
					int yOffsetForScroll2 = scrollArea.getYOffsetForScroll();
					if (itemsHeight - yOffsetForScroll2 < (itemButtonHeight + 8) * (currentlySelectedItem + 1))
					{
						scrollArea.setYOffsetForScroll(yOffsetForScroll2 - (itemButtonHeight + 8));
					}
					updateItemButtons();
				}
				return;
			case Buttons.LeftShoulder:
			case Buttons.RightShoulder:
				if (!_isStorageShop && !_isCatalogue)
				{
					break;
				}
				switch (b)
				{
				case Buttons.LeftShoulder:
					currentTab--;
					if (currentTab < 0)
					{
						currentTab = tabButtons.Count - 1;
					}
					switchTab(currentTab);
					break;
				case Buttons.RightShoulder:
					currentTab++;
					if (currentTab >= tabButtons.Count)
					{
						currentTab = 0;
					}
					switchTab(currentTab);
					break;
				}
				return;
			}
			if (_isStorageShop && b == Buttons.A)
			{
				if (currentItem != null && currentlySelectedItem > -1)
				{
					Game1.player.addItemToInventory(currentItem as Item);
					Game1.player.fakeAddItemToInventoryBool(currentItem as Item);
					if (onPurchase != null)
					{
						onPurchase(currentItem, Game1.player, 1);
					}
					itemPriceAndStock.Remove(forSale[currentlySelectedItem]);
					forSale.RemoveAt(currentlySelectedItem);
				}
				return;
			}
			switch (b)
			{
			case Buttons.LeftTrigger:
				quantitySlider.changeValueBy(-1);
				quantityToBuy = Math.Max(quantityToBuy - 1, 1);
				quantitySlider.value = (int)((float)(quantityToBuy - 1) / (float)(maxBuyable - 1) * 100f);
				break;
			case Buttons.RightTrigger:
			{
				int num3 = 1;
				if (quantityToBuy == 0)
				{
					quantitySlider.changeValueBy(2);
					num3 = 2;
				}
				else
				{
					quantitySlider.changeValueBy(1);
				}
				quantityToBuy = Math.Min(quantityToBuy + num3, maxBuyable);
				quantitySlider.value = (int)((float)(quantityToBuy - 1) / (float)(maxBuyable - 1) * 100f);
				break;
			}
			case Buttons.A:
				if (currentlySelectedItem > -1)
				{
					OnTapBuy();
				}
				break;
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.options.SnappyMenus)
			{
				return;
			}
			if (upperRightCloseButton.containsPoint(x, y))
			{
				Game1.currentLocation.tapToMove.ResetRotatingFurniture();
			}
			base.receiveLeftClick(x, y);
			if (Game1.activeClickableMenu == null)
			{
				return;
			}
			Vector2 vector = inventory.snapToClickableComponent(x, y);
			clickReceived = true;
			for (int i = 0; i < tabButtons.Count; i++)
			{
				if (tabButtons[i].containsPoint(x, y))
				{
					switchTab(i);
				}
			}
			if (inventoryButton.bounds.Contains(x, y))
			{
				inventoryButtonisHeld = true;
				Game1.playSound("smallSelect");
				TutorialManager.Instance.SaleTutorialCheck();
			}
			else
			{
				inventoryButtonisHeld = false;
			}
			if (!inventoryVisible && scrollBarVisible)
			{
				scrollArea.receiveLeftClick(x, y);
				if (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y))
				{
					scrolling = true;
				}
			}
			currentItemIndex = Math.Max(0, Math.Min(forSale.Count - itemsPerPage, currentItemIndex));
			InventoryReceiveLeftClick(x, y);
			if (!inventoryVisible && maxBuyable > 1 && priceItem > 0 && quantitySlider.bounds.Contains(x, y))
			{
				quantitySlider.click(x, y);
				quantitySliderHeld = true;
			}
			if (!inventoryVisible && !_isStorageShop && buyButton.containsPoint(x, y) && currentlySelectedItem != -1)
			{
				buyButtonisHeld = true;
				Game1.playSound("smallSelect");
			}
		}

		private void InventoryReceiveLeftClick(int x, int y)
		{
			if (readOnly || !inventoryVisible || (itemPlayerIsSelling != null && sellButton.bounds.Contains(x, y)))
			{
				return;
			}
			Item item = null;
			if (!new Rectangle((int)sellPanelPosition.X, (int)sellPanelPosition.Y, sellPanelWidth, sellPanelHeight).Contains(x, y))
			{
				inventory.receiveLeftClick(x, y);
				item = inventory.getItemAt(x, y);
			}
			if (itemPlayerIsSelling != null && !new Rectangle((int)sellPanelPosition.X, (int)sellPanelPosition.Y, sellPanelWidth, sellPanelHeight).Contains(x, y))
			{
				itemPlayerIsSelling = null;
				sellPanelPosition.X = -100f;
				sellPanelPosition.Y = -100f;
				sellPanelWidth = 0;
				sellPanelHeight = 0;
				Game1.playSound("smallSelect");
			}
			if (item != null && item != itemPlayerIsSelling && highlightItemToSell(item))
			{
				Game1.playSound("smallSelect");
				itemPlayerIsSelling = item;
				quantityToSell = 1;
				if (!_isStorageShop)
				{
					sellPanelTextSize = Game1.dialogueFont.MeasureString(itemPlayerIsSelling.DisplayName);
					salePrice = (int)((itemPlayerIsSelling is Object) ? ((float)(itemPlayerIsSelling as Object).sellToStorePrice(-1L) * sellPercentage) : ((float)(itemPlayerIsSelling.salePrice() / 2) * sellPercentage)) * quantityToSell;
					sellPanelHeight = (int)(250f * Math.Max(1.3f, heightMod));
					sellPanelWidth = Math.Max(300, (int)sellPanelTextSize.X + itemsXoff * 4);
					sellPanelPosition = inventory.getPositionOfSellPanel(x, y);
					if (sellPanelPosition.X + (float)sellPanelWidth > (float)(width - edge * 2))
					{
						sellPanelPosition.X -= sellPanelPosition.X + (float)sellPanelWidth - (float)(width - edge * 2);
					}
					sellButton.bounds.X = (int)sellPanelPosition.X + (int)((double)sellPanelWidth * 0.125);
					sellButton.bounds.Y = (int)sellPanelPosition.Y + sellPanelHeight - buyHeight * 2;
					sellButton.bounds.Width = (int)((double)sellPanelWidth * 0.75);
					sellButton.bounds.Height = buyHeight;
					if (itemPlayerIsSelling.Stack > 1)
					{
						sellQuantitySlider.bounds.X = sellButton.bounds.X + 16;
						sellQuantitySlider.bounds.Y = sellButton.bounds.Y + sellButton.bounds.Height + 32;
						sellQuantitySlider.bounds.Width = sellButton.bounds.Width - 32;
						sellQuantitySlider.bounds.Height = sellButton.bounds.Height + 16;
						sellPanelHeight += sellButton.bounds.Height;
						sellQuantitySlider.value = quantityToSell;
					}
				}
				sellButtonisHeld = false;
				savedInventoryX = x;
				savedInventoryY = y;
			}
			if (sellQuantitySlider != null && sellQuantitySlider.bounds.Contains(x, y))
			{
				sellQuantitySlider.click(x, y);
				sellQuantitySliderHeld = true;
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (inventoryVisible && itemPlayerIsSelling == null)
			{
				inventory.leftClickHeld(x, y);
				return;
			}
			if (!inventoryVisible)
			{
				if (scrollArea.panelScrolling)
				{
					updateItemButtons();
				}
				scrollArea.leftClickHeld(x, y);
				if (quantitySliderHeld)
				{
					quantitySlider.click(x, y, wasAlreadyHeld: true);
					quantityToBuy = Math.Min(1 + (int)((float)(quantitySlider.value * maxBuyable) / 100f), maxBuyable);
				}
				if (buyButtonisHeld && !buyButton.containsPoint(x, y))
				{
					buyButtonisHeld = false;
				}
				if (scrollBarVisible && scrolling && (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y)))
				{
					float num = newScrollbar.setY(y);
					scrollArea.setYOffsetForScroll(-(int)(num * (float)(forSale.Count - itemsPerPage) * (float)(itemButtonHeight + 8) / 100f));
					updateItemButtons();
				}
			}
			if (inventoryVisible && sellQuantitySliderHeld)
			{
				sellQuantitySlider.click(x, y, wasAlreadyHeld: true);
				quantityToSell = Math.Min(1 + (int)((float)(sellQuantitySlider.value * itemPlayerIsSelling.Stack) / 100f), itemPlayerIsSelling.Stack);
				salePrice = (int)((itemPlayerIsSelling is Object) ? ((float)(itemPlayerIsSelling as Object).sellToStorePrice(-1L) * sellPercentage) : ((float)(itemPlayerIsSelling.salePrice() / 2) * sellPercentage)) * quantityToSell;
			}
			if (!inventoryButton.containsPoint(x, y))
			{
				inventoryButtonisHeld = false;
			}
			if (sellButton.containsPoint(x, y))
			{
				sellButtonisHeld = true;
			}
			else
			{
				sellButtonisHeld = false;
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (!clickReceived)
			{
				return;
			}
			base.releaseLeftClick(x, y);
			quantitySliderHeld = (sellQuantitySliderHeld = false);
			if (!inventoryVisible && !scrollArea.havePanelScrolled)
			{
				for (int i = 0; i < forSaleButtons.Count; i++)
				{
					if (currentItemIndex + i >= forSale.Count || !forSaleButtons[i].containsPoint(x, y))
					{
						continue;
					}
					int index = currentItemIndex + i;
					setCurrentItem(index);
					Game1.playSound("smallSelect");
					if (_isStorageShop && currentItem != null && Game1.player.couldInventoryAcceptThisItem(currentItem as Item))
					{
						Item one = (currentItem as Item).getOne();
						one.Stack = (currentItem as Item).Stack;
						Item item = Game1.player.addItemToInventory(one);
						Game1.player.fakeAddItemToInventoryBool(one);
						if (item != null)
						{
							currentItem.Stack = item.Stack;
						}
						else
						{
							currentItem.Stack = 0;
						}
						if (onPurchase != null)
						{
							onPurchase(currentItem, Game1.player, 1);
						}
						itemPriceAndStock.Remove(forSale[index]);
						forSale.RemoveAt(index);
					}
				}
			}
			if (inventoryButtonisHeld && inventoryButton.containsPoint(x, y))
			{
				inventoryButtonisHeld = false;
				inventoryVisible = !inventoryVisible;
				Game1.playSound("smallSelect");
			}
			if (inventoryVisible)
			{
				int num = 0;
				inventory.releaseLeftClick(x, y);
				if (itemPlayerIsSelling != null && _isStorageShop)
				{
					Item item2 = inventory.leftClick(x, y, null, playSound: false);
					if (item2 != null && onSell != null && onSell(item2))
					{
						item2 = null;
						rebuildSaleButtons();
						switchTab(0);
					}
					itemPlayerIsSelling = null;
					return;
				}
				if (itemPlayerIsSelling != null && sellButton.containsPoint(x, y))
				{
					if (itemPlayerIsSelling.Stack > 1)
					{
						itemPlayerIsSelling.Stack -= quantityToSell;
						if (itemPlayerIsSelling.Stack <= 0)
						{
							inventory.leftClick(savedInventoryX, savedInventoryY, null, playSound: false);
						}
						num = quantityToSell;
					}
					else
					{
						num = itemPlayerIsSelling.Stack;
						inventory.leftClick(savedInventoryX, savedInventoryY, null, playSound: false);
					}
					updateNumberOfUsedInventorySlots();
					chargePlayer(Game1.player, currency, -((int)((itemPlayerIsSelling is Object) ? ((float)(itemPlayerIsSelling as Object).sellToStorePrice(-1L) * sellPercentage) : ((float)(itemPlayerIsSelling.salePrice() / 2) * sellPercentage)) * num));
					int num2 = itemPlayerIsSelling.Stack / 8 + 2;
					if (itemPlayerIsSelling is Object && (int)(itemPlayerIsSelling as Object).edibility != -300)
					{
						for (int j = 0; j < num; j++)
						{
							if (Game1.random.NextDouble() < 0.03999999910593033)
							{
								(Game1.getLocationFromName("SeedShop") as SeedShop).itemsToStartSellingTomorrow.Add(itemPlayerIsSelling.getOne());
							}
						}
					}
					sellPanelPosition.X = -100f;
					sellPanelPosition.Y = -100f;
					sellPanelWidth = 0;
					sellPanelHeight = 0;
					itemPlayerIsSelling = null;
					Game1.playSound("sell");
				}
			}
			else if (buyButtonisHeld && currentlySelectedItem != -1 && buyButton.containsPoint(x, y) && !_isStorageShop)
			{
				OnTapBuy();
				return;
			}
			if (!inventoryVisible)
			{
				scrollArea.releaseLeftClick(x, y);
			}
			scrolling = false;
		}

		private void OnTapBuy()
		{
			buyButtonisHeld = false;
			Game1.playSound("smallSelect");
			if (forSale[currentlySelectedItem] != null)
			{
				int num = quantityToBuy;
				if (num == -1)
				{
					num = 1;
				}
				if (num > 0 && tryToPurchaseItem(forSale[currentlySelectedItem], heldItem, num, 0, 0, currentlySelectedItem))
				{
					itemPriceAndStock.Remove(forSale[currentlySelectedItem]);
					forSale.RemoveAt(currentlySelectedItem);
					currentlySelectedItem = -1;
				}
				else if (num <= 0)
				{
					Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
					Game1.playSound("cancel");
				}
			}
			if (currentlySelectedItem >= 0)
			{
				setCurrentItem(currentlySelectedItem);
			}
			currentItemIndex = Math.Max(0, Math.Min(forSale.Count - itemsPerPage, currentItemIndex));
		}

		private void setScrollBarToCurrentIndex()
		{
			if (forSale.Count > 0)
			{
				int num = Math.Max(1, forSale.Count - itemsPerPage + 1) * currentItemIndex;
				newScrollbar.setPercentage(num);
				int num2 = (forSale.Count - itemsPerPage) * (itemButtonHeight + 8);
				scrollArea.setMaxYOffset(num2);
				scrollArea.setYOffsetForScroll(-(int)((float)(num * num2) / 100f));
				updateItemButtons();
			}
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			if (direction > 0 && currentItemIndex > 0)
			{
				currentItemIndex--;
				setScrollBarToCurrentIndex();
			}
			else if (direction < 0 && currentItemIndex < forSale.Count - itemsPerPage)
			{
				currentItemIndex++;
				setScrollBarToCurrentIndex();
			}
		}

		private void downArrowPressed()
		{
		}

		private void upArrowPressed()
		{
		}

		private void setCurrentItem(int index)
		{
			currentlySelectedItem = Math.Max(0, index);
			currentlySelectedItem = Math.Min(currentlySelectedItem, forSale.Count - 1);
			if (currentlySelectedItem >= 0)
			{
				currentItem = forSale[currentlySelectedItem];
				descItem = currentItem.getDescription().Replace("\n", "").Replace("\r", "");
				nameItem = currentItem.DisplayName;
				priceItem = itemPriceAndStock[currentItem][0];
				if (itemPriceAndStock[currentItem][1] == 2147483647)
				{
					maxBuyable = Math.Max(0, forSale[currentlySelectedItem].maximumStackSize());
				}
				else
				{
					maxBuyable = Math.Max(0, itemPriceAndStock[currentItem][1]);
				}
				if (currentItem is Object && (bool)(currentItem as Object).isRecipe)
				{
					maxBuyable = Math.Min(1, maxBuyable);
				}
				if (itemPriceAndStock[forSale[currentlySelectedItem]][0] != 0)
				{
					maxBuyable = Math.Min(maxBuyable, getPlayerCurrencyAmount(Game1.player, currency) / itemPriceAndStock[forSale[currentlySelectedItem]][0]);
				}
				quantityToBuy = 1;
				if (maxBuyable > 1 && priceItem > 0)
				{
					buyButton.bounds.Y = buyYWithSlider;
					quantitySlider.value = 0;
				}
				else
				{
					buyButton.bounds.Y = buyY;
				}
				numOfCurrentItem = getPlayerNumberOfItem(currentItem);
				hoveredItem = currentItem;
			}
		}

		private void checkForTutorial()
		{
			if (!TutorialManager.Instance.hasSeenBuyTutorial)
			{
				TutorialManager.Instance.hasSeenBuyTutorial = true;
				TutorialManager.Instance.completeTutorial(tutorialType.INTERACT_SHOP);
			}
			if (!TutorialManager.Instance.hasSeenSaleTutorial && checkForItemsToSell())
			{
				TutorialManager.Instance.hasSeenSaleTutorial = true;
				TutorialManager.Instance.completeTutorial(tutorialType.DUMMY_SELL_SHOP);
			}
		}

		private int updateNumberOfUsedInventorySlots()
		{
			numUsedSlots = 0;
			for (int i = 0; i < Game1.player.items.Count; i++)
			{
				if (Game1.player.items[i] != null)
				{
					numUsedSlots++;
				}
			}
			return numUsedSlots;
		}

		private int getPlayerNumberOfItem(ISalable item)
		{
			int num = 0;
			for (int i = 0; i < Game1.player.items.Count; i++)
			{
				if (Game1.player.items[i] != null && Game1.player.items[i].Name == item.Name && Game1.player.items[i].Stack < 1000)
				{
					num += Game1.player.items[i].Stack;
				}
			}
			return num;
		}

		private bool isTrashCan(ISalable item)
		{
			if (item is GenericTool && item.Name.Contains("Trash Can"))
			{
				return true;
			}
			return false;
		}

		public virtual bool CanBuyback()
		{
			return true;
		}

		public virtual void BuyBuybackItem(ISalable bought_item, int price, int stack)
		{
			Game1.player.totalMoneyEarned -= (uint)price;
			if (Game1.player.useSeparateWallets)
			{
				Game1.player.stats.IndividualMoneyEarned -= (uint)price;
			}
			if (buyBackItemsToResellTomorrow.ContainsKey(bought_item))
			{
				ISalable salable = buyBackItemsToResellTomorrow[bought_item];
				salable.Stack -= stack;
				if (salable.Stack <= 0)
				{
					buyBackItemsToResellTomorrow.Remove(bought_item);
					(Game1.currentLocation as ShopLocation).itemsToStartSellingTomorrow.Remove(salable as Item);
				}
			}
		}

		public virtual ISalable AddBuybackItem(ISalable sold_item, int sell_unit_price, int stack)
		{
			ISalable salable = null;
			while (stack > 0)
			{
				salable = null;
				foreach (ISalable buyBackItem in buyBackItems)
				{
					if (buyBackItem.canStackWith(sold_item) && buyBackItem.Stack < buyBackItem.maximumStackSize())
					{
						salable = buyBackItem;
						break;
					}
				}
				if (salable == null)
				{
					salable = sold_item.GetSalableInstance();
					int num = Math.Min(stack, salable.maximumStackSize());
					buyBackItems.Add(salable);
					itemPriceAndStock.Add(salable, new int[2] { sell_unit_price, num });
					salable.Stack = num;
					stack -= num;
				}
				else
				{
					int num2 = Math.Min(stack, salable.maximumStackSize() - salable.Stack);
					int[] array = itemPriceAndStock[salable];
					array[1] += num2;
					itemPriceAndStock[salable] = array;
					salable.Stack = array[1];
					stack -= num2;
				}
			}
			forSale = itemPriceAndStock.Keys.ToList();
			return salable;
		}

		public override bool readyToClose()
		{
			if (heldItem == null && animations.Count == 0)
			{
				if (Game1.options.SnappyMenus)
				{
					return !inventoryWasVisible;
				}
				return true;
			}
			return false;
		}

		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			if (heldItem != null)
			{
				Game1.player.addItemToInventoryBool(heldItem as Item);
				Game1.playSound("coin");
			}
		}

		public static void chargePlayer(Farmer who, int currencyType, int amount)
		{
			switch (currencyType)
			{
			case 0:
				who.Money -= amount;
				break;
			case 1:
				who.festivalScore -= amount;
				break;
			case 2:
				who.clubCoins -= amount;
				break;
			case 4:
				who.QiGems -= amount;
				break;
			case 3:
				break;
			}
		}

		private bool tryToPurchaseItem(ISalable item, ISalable held_item, int numberToBuy, int x, int y, int indexInForSaleList)
		{
			if (readOnly)
			{
				return false;
			}
			if (held_item == null)
			{
				if (itemPriceAndStock[item][1] == 0)
				{
					hoveredItem = null;
					return true;
				}
				if (item.GetSalableInstance().maximumStackSize() < numberToBuy)
				{
					numberToBuy = Math.Max(1, item.GetSalableInstance().maximumStackSize());
				}
				int num = itemPriceAndStock[item][0] * numberToBuy;
				int num2 = -1;
				int num3 = 5;
				if (itemPriceAndStock[item].Length > 2)
				{
					num2 = itemPriceAndStock[item][2];
					if (itemPriceAndStock[item].Length > 3)
					{
						num3 = itemPriceAndStock[item][3];
					}
					num3 *= numberToBuy;
				}
				if (getPlayerCurrencyAmount(Game1.player, currency) >= num && (num2 == -1 || Game1.player.hasItemInInventory(num2, num3)))
				{
					heldItem = item.GetSalableInstance();
					heldItem.Stack = numberToBuy;
					if (storeContext == "QiGemShop" || storeContext == "StardewFair")
					{
						heldItem.Stack *= item.Stack;
					}
					else if (itemPriceAndStock[item][1] == 2147483647 && item.Stack != 2147483647)
					{
						heldItem.Stack *= item.Stack;
					}
					if (!heldItem.CanBuyItem(Game1.player) && !item.IsInfiniteStock() && (!(heldItem is Object) || !(heldItem as Object).isRecipe))
					{
						Game1.playSound("smallSelect");
						heldItem = null;
						updateNumberOfUsedInventorySlots();
						return false;
					}
					if (itemPriceAndStock[item][1] != 2147483647 && !item.IsInfiniteStock())
					{
						itemPriceAndStock[item][1] -= numberToBuy;
						forSale[indexInForSaleList].Stack -= numberToBuy;
					}
					if (CanBuyback() && buyBackItems.Contains(item))
					{
						BuyBuybackItem(item, num, numberToBuy);
					}
					chargePlayer(Game1.player, currency, num);
					if (heldItem is Item i)
					{
						Game1.playSound("sell");
						boughtItemTween = new tweeningSprite(i, null, new Vector2(buyButton.bounds.X + buyButton.bounds.Width / 2 - 32, buyButton.bounds.Y), new Vector2((float)(invX + invWidth) - 80f * widthMod, (float)invY + 10f * heightMod), 500f);
						boughtItemTween.start();
					}
					if (num2 != -1)
					{
						Game1.player.removeItemsFromInventory(num2, num3);
					}
					if (!_isStorageShop && item.actionWhenPurchased())
					{
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
								Game1.playSound("newRecipe");
							}
							catch (Exception)
							{
							}
						}
						held_item = null;
						heldItem = null;
					}
					else
					{
						if (heldItem != null && heldItem is Object && (heldItem as Object).ParentSheetIndex == 858)
						{
							Game1.player.team.addQiGemsToTeam.Fire(heldItem.Stack);
							heldItem = null;
						}
						if (Game1.mouseClickPolling > 300)
						{
							if (purchaseRepeatSound != null)
							{
								Game1.playSound(purchaseRepeatSound);
							}
						}
						else if (purchaseSound != null)
						{
							Game1.playSound(purchaseSound);
						}
					}
					if (onPurchase != null && onPurchase(item, Game1.player, numberToBuy))
					{
						exitThisMenu();
					}
				}
				else
				{
					if (num > 0)
					{
						Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
					}
					Game1.playSound("cancel");
				}
			}
			else if (held_item.canStackWith(item))
			{
				numberToBuy = Math.Min(numberToBuy, held_item.maximumStackSize() - held_item.Stack);
				if (numberToBuy > 0)
				{
					int num4 = itemPriceAndStock[item][0] * numberToBuy;
					int num5 = -1;
					int num6 = 5;
					if (itemPriceAndStock[item].Length > 2)
					{
						num5 = itemPriceAndStock[item][2];
						if (itemPriceAndStock[item].Length > 3)
						{
							num6 = itemPriceAndStock[item][3];
						}
						num6 *= numberToBuy;
					}
					int stack = item.Stack;
					item.Stack = numberToBuy + heldItem.Stack;
					if (!item.CanBuyItem(Game1.player))
					{
						item.Stack = stack;
						Game1.playSound("cancel");
						return false;
					}
					item.Stack = stack;
					if (getPlayerCurrencyAmount(Game1.player, currency) >= num4 && (num5 == -1 || Game1.player.hasItemInInventory(num5, num6)))
					{
						int num7 = numberToBuy;
						if (itemPriceAndStock[item][1] == 2147483647 && item.Stack != 2147483647)
						{
							num7 *= item.Stack;
						}
						heldItem.Stack += num7;
						if (itemPriceAndStock[item][1] != 2147483647 && !item.IsInfiniteStock())
						{
							itemPriceAndStock[item][1] -= numberToBuy;
							forSale[indexInForSaleList].Stack -= numberToBuy;
						}
						if (CanBuyback() && buyBackItems.Contains(item))
						{
							BuyBuybackItem(item, num4, num7);
						}
						chargePlayer(Game1.player, currency, num4);
						if (heldItem is Item i2)
						{
							boughtItemTween = new tweeningSprite(i2, null, new Vector2(buyButton.bounds.X + buyButton.bounds.Width / 2 - 32, buyButton.bounds.Y), new Vector2(-32f, -32f), 500f);
							boughtItemTween.start();
						}
						Game1.playSound("purchaseClick");
						if (num5 != -1)
						{
							Game1.player.removeItemsFromInventory(num5, num6);
						}
						if (!_isStorageShop && item.actionWhenPurchased())
						{
							heldItem = null;
						}
						if (onPurchase != null && onPurchase(item, Game1.player, numberToBuy))
						{
							exitThisMenu();
						}
					}
					else
					{
						if (num4 > 0)
						{
							Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
						}
						Game1.playSound("cancel");
					}
				}
			}
			try
			{
				if (heldItem != null)
				{
					Game1.player.addItemToInventory(heldItem as Item);
					Game1.player.fakeAddItemToInventoryBool(heldItem as Item);
					heldItem = null;
				}
			}
			catch (Exception exception)
			{
				Log.Exception(exception, "K");
				return true;
			}
			if (itemPriceAndStock[item][1] <= 0)
			{
				if (buyBackItems.Contains(item))
				{
					buyBackItems.Remove(item);
				}
				numOfCurrentItem = getPlayerNumberOfItem(currentItem);
				updateNumberOfUsedInventorySlots();
				hoveredItem = null;
				return true;
			}
			try
			{
				numOfCurrentItem = getPlayerNumberOfItem(currentItem);
				updateNumberOfUsedInventorySlots();
			}
			catch (Exception exception2)
			{
				Log.Exception(exception2, "M");
				return true;
			}
			return false;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			Vector2 vector = inventory.snapToClickableComponent(x, y);
			if (heldItem == null && !readOnly)
			{
				Item item = inventory.rightClick(x, y, null, playSound: false);
				if (item != null)
				{
					chargePlayer(Game1.player, currency, -((int)((item is Object) ? ((float)(item as Object).sellToStorePrice(-1L) * sellPercentage) : ((float)(item.salePrice() / 2) * sellPercentage)) * item.Stack));
					item = null;
					if (Game1.mouseClickPolling > 300)
					{
						Game1.playSound("purchaseRepeat");
					}
					else
					{
						Game1.playSound("purchaseClick");
					}
					int num = 2;
					for (int i = 0; i < num; i++)
					{
						animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, vector + new Vector2(32f, 32f), flicker: false, flipped: false)
						{
							alphaFade = 0.025f,
							motion = new Vector2(Game1.random.Next(-3, 4), -4f),
							acceleration = new Vector2(0f, 0.5f),
							delayBeforeAnimationStart = i * 25,
							scale = 2f
						});
						animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, vector + new Vector2(32f, 32f), flicker: false, flipped: false)
						{
							scale = 4f,
							alphaFade = 0.025f,
							delayBeforeAnimationStart = i * 50,
							motion = Utility.getVelocityTowardPoint(new Point((int)vector.X + 32, (int)vector.Y + 32), new Vector2(xPositionOnScreen - 36, yPositionOnScreen + height - inventory.height - 16), 8f),
							acceleration = Utility.getVelocityTowardPoint(new Point((int)vector.X + 32, (int)vector.Y + 32), new Vector2(xPositionOnScreen - 36, yPositionOnScreen + height - inventory.height - 16), 0.5f)
						});
					}
					if (item is Object && (int)(item as Object).edibility != -300 && Game1.random.NextDouble() < 0.03999999910593033)
					{
						(Game1.getLocationFromName("SeedShop") as SeedShop).itemsToStartSellingTomorrow.Add(item.getOne());
					}
					if (inventory.getItemAt(x, y) == null)
					{
						Game1.playSound("sell");
						animations.Add(new TemporaryAnimatedSprite(5, vector + new Vector2(32f, 32f), Color.White)
						{
							motion = new Vector2(0f, -0.5f)
						});
					}
				}
			}
			else
			{
				heldItem = inventory.rightClick(x, y, heldItem as Item);
			}
			for (int j = 0; j < forSaleButtons.Count; j++)
			{
				if (currentItemIndex + j >= forSale.Count || !forSaleButtons[j].containsPoint(x, y))
				{
					continue;
				}
				int num2 = currentItemIndex + j;
				if (forSale[num2] != null)
				{
					int num3 = ((!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : Math.Min(Math.Min(5, getPlayerCurrencyAmount(Game1.player, currency) / itemPriceAndStock[forSale[num2]][0]), itemPriceAndStock[forSale[num2]][1]));
					if (num3 > 0 && tryToPurchaseItem(forSale[num2], heldItem, num3, 0, 0, num2))
					{
						itemPriceAndStock.Remove(forSale[num2]);
						forSale.RemoveAt(num2);
						currentlySelectedItem = -1;
					}
					if (heldItem != null && Game1.options.SnappyMenus && Game1.activeClickableMenu != null && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(heldItem as Item))
					{
						heldItem = null;
						DelayedAction.playSoundAfterDelay("coin", 100);
					}
				}
				break;
			}
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (poof != null && poof.update(time))
			{
				poof = null;
			}
			bool scrollingWithMomentum = scrollArea.scrollingWithMomentum;
			scrollArea.update(time);
			if (scrollArea.scrollingWithMomentum || scrollingWithMomentum)
			{
				updateItemButtons();
			}
			if (boughtItemTween != null && boughtItemTween.tweening)
			{
				boughtItemTween.update(time);
				if (!boughtItemTween.tweening)
				{
					boughtItemTween = null;
				}
			}
			if (!Game1.options.SnappyMenus)
			{
				return;
			}
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.LeftTrigger))
			{
				triggerPolling += (float)time.ElapsedGameTime.TotalMilliseconds;
				triggerPollingAccel += (float)time.ElapsedGameTime.TotalMilliseconds;
				if (triggerPolling > 200f)
				{
					int num = (int)(triggerPollingAccel / 800f);
					quantitySlider.changeValueBy(-1 - num);
					quantityToBuy = Math.Max(quantityToBuy - 1 - num, 1);
				}
			}
			else if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightTrigger))
			{
				triggerPolling += (float)time.ElapsedGameTime.TotalMilliseconds;
				triggerPollingAccel += (float)time.ElapsedGameTime.TotalMilliseconds;
				if (triggerPolling > 200f)
				{
					if (quantityToBuy == 0)
					{
						quantityToBuy = 1;
					}
					int num2 = (int)(triggerPollingAccel / 800f);
					quantitySlider.changeValueBy(1 + num2);
					quantityToBuy = Math.Min(quantityToBuy + 1 + num2, maxBuyable);
				}
			}
			quantitySlider.value = (int)((float)(quantityToBuy - 1) / (float)(maxBuyable - 1) * 100f);
			if (inventoryVisible && Game1.input.GetGamePadState().IsButtonDown(Buttons.A))
			{
				AButtonPolling += (float)time.ElapsedGameTime.TotalMilliseconds;
				aButtonPollingAccel += (float)time.ElapsedGameTime.TotalMilliseconds;
				if (AButtonPolling > 400f)
				{
					doGamePadButtonInventory(Buttons.A);
					AButtonPolling = 300 + (int)aButtonPollingAccel / 200 * 10;
				}
			}
		}

		private int getHoveredItemExtraItemIndex()
		{
			if (itemPriceAndStock != null && hoveredItem != null && itemPriceAndStock.ContainsKey(hoveredItem) && itemPriceAndStock[hoveredItem].Length > 2)
			{
				return itemPriceAndStock[hoveredItem][2];
			}
			return -1;
		}

		private int getHoveredItemExtraItemAmount()
		{
			if (itemPriceAndStock != null && hoveredItem != null && itemPriceAndStock.ContainsKey(hoveredItem) && itemPriceAndStock[hoveredItem].Length > 3)
			{
				return itemPriceAndStock[hoveredItem][3];
			}
			return 5;
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			initialiseMobileLayout();
		}

		public void setItemPriceAndStock(Dictionary<ISalable, int[]> new_stock)
		{
			itemPriceAndStock = new_stock;
			forSale = itemPriceAndStock.Keys.ToList();
		}

		public void drawSellPanel(SpriteBatch b)
		{
			if (itemPlayerIsSelling != null)
			{
				Vector2 position = new Vector2(sellPanelPosition.X + (float)(itemsXoff * 2), sellPanelPosition.Y + (float)(itemsYoff * 2));
				IClickableMenu.drawTextureBox(b, (int)sellPanelPosition.X, (int)sellPanelPosition.Y, sellPanelWidth, sellPanelHeight, Color.White);
				Utility.drawTextWithShadow(b, itemPlayerIsSelling.DisplayName, Game1.dialogueFont, position, Game1.textColor);
				position.Y += sellPanelTextSize.Y * 1.2f;
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AnimalQuery_Sell", salePrice), Game1.smallFont, position, Game1.textColor);
				IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.dialogueFont, Game1.mouseCursors, new Rectangle(256, 256, 10, 10), Game1.mouseCursors, new Rectangle(193 + currency * 9, 373, 9, 10), (itemPlayerIsSelling.Stack > 1) ? (quantityToSell + ":" + salePrice) : (salePrice.ToString() ?? ""), sellButton.bounds.X, sellButton.bounds.Y, sellButton.bounds.Width, sellButton.bounds.Height, Color.White, 4f, drawShadow: true, iconLeft: false, isClickable: true, sellButtonisHeld);
			}
		}

		public void drawCurrency(SpriteBatch b)
		{
			if (currency != 0)
			{
				_ = 1;
			}
			else
			{
				Game1.dayTimeMoneyBox.drawMoneyBox(b, goldX, goldY, oldGFX: true);
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			drawCurrency(b);
			IClickableMenu.drawTextureBox(b, itemsX, itemsY, itemsWidth, itemsHeight, Color.White);
			IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.smallFont, Game1.mobileSpriteSheet, new Rectangle(107, 80, 15, 15), Game1.mobileSpriteSheet, new Rectangle(40, 22, 9, 12), Game1.content.LoadString("Strings\\UI:GameMenu_Inventory") + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? "" : (" (" + numUsedSlots + "/" + Game1.player.MaxItems + ")")), inventoryButton.bounds.X, inventoryButton.bounds.Y, inventoryButton.bounds.Width, inventoryButton.bounds.Height, Color.Wheat, 4f, drawShadow: true, iconLeft: true, isClickable: true, inventoryButtonisHeld || inventoryVisible, drawIcon: true, reverseColors: false, bold: false);
			if (currency == 2)
			{
				int num = Game1.uiViewport.Width * 2 / 3;
				int num2 = 16;
				string text = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? "     " : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? "\u3000   \u3000" : "  "));
				SpriteText.drawStringWithScrollBackground(b, text + Game1.player.clubCoins, num, num2);
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(num, num2 + 4), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			}
			if (currency == 1)
			{
				int num3 = Game1.uiViewport.Width * 2 / 3;
				int num4 = -8;
				b.Draw(Game1.fadeToBlackRect, new Rectangle(num3 + 16, num4 + 16, 128 + ((Game1.player.festivalScore > 999) ? 16 : 0), 64), Color.Black * 0.75f);
				b.Draw(Game1.mouseCursors, new Vector2(num3 + 32, num4 + 32), new Rectangle(338, 400, 8, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				Game1.drawWithBorder(Game1.player.festivalScore.ToString() ?? "", Color.Black, Color.White, new Vector2(num3 + 32 + 40, num4 + 21 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8))), 0f, 1f, 1f, tiny: false);
			}
			base.draw(b);
			scrollArea.setUpForScrollBoxDrawing(b);
			int yOffsetForScroll = scrollArea.getYOffsetForScroll();
			for (int i = 0; i < forSaleButtons.Count; i++)
			{
				if (forSaleButtons[i].bounds.Y <= -forSaleButtons[i].bounds.Height || forSaleButtons[i].bounds.Y >= scrollArea.Bounds.Y + scrollArea.Bounds.Height)
				{
					continue;
				}
				int num5 = forSaleButtons[i].bounds.Width - 270;
				if (currentItemIndex + i >= forSale.Count)
				{
					continue;
				}
				bool flag = false;
				if (canPurchaseCheck != null && !canPurchaseCheck(currentItemIndex + i))
				{
					flag = true;
				}
				int num6 = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 18 : 0);
				IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(72, 101, 15, 15), forSaleButtons[i].bounds.X, forSaleButtons[i].bounds.Y, forSaleButtons[i].bounds.Width, forSaleButtons[i].bounds.Height, (currentItemIndex + i == currentlySelectedItem) ? Color.Wheat : Color.White, 4f, drawShadow: false);
				if (!(forSale[i] is HouseRenovation))
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), forSaleButtons[i].bounds.X + 32 - 8, forSaleButtons[i].bounds.Y + num6 + 24 - 8, 80, 80, new Color(133, 54, 5), 4f, drawShadow: false);
					b.Draw(Game1.mouseCursors, new Vector2(forSaleButtons[i].bounds.X + 32 - 4, forSaleButtons[i].bounds.Y + num6 + 24 - 4), new Rectangle(296, 363, 18, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
				ISalable salable = forSale[currentItemIndex + i];
				bool flag2 = salable.Stack > 1 && salable.Stack != 2147483647 && itemPriceAndStock[salable][1] == 2147483647;
				StackDrawType drawStackNumber;
				if (itemPriceAndStock[salable][1] == 2147483647)
				{
					drawStackNumber = StackDrawType.Hide;
				}
				else
				{
					drawStackNumber = StackDrawType.Draw_OneInclusive;
					if (_isStorageShop)
					{
						drawStackNumber = StackDrawType.Draw;
					}
				}
				forSale[currentItemIndex + i].drawInMenu(b, new Vector2(forSaleButtons[i].bounds.X + 32, forSaleButtons[i].bounds.Y + num6 + 24), 1f, 1f, 0.086f, drawStackNumber, Color.White * ((!flag) ? 1f : 0.25f), drawShadow: true);
				Vector2 position = ((!(forSale[i] is HouseRenovation)) ? new Vector2(forSaleButtons[i].bounds.X + 96 + 16, forSaleButtons[i].bounds.Y + num6) : new Vector2(forSaleButtons[i].bounds.X + forSaleButtons[i].bounds.Height / 3, forSaleButtons[i].bounds.Y + num6));
				if (Game1.options.bigFonts)
				{
					position.Y += 4f;
				}
				Utility.drawMultiLineTextWithShadow(b, forSale[currentItemIndex + i].DisplayName, Game1.smallFont, position, num5, forSaleButtons[i].bounds.Height, Game1.textColor, centreY: true, actuallyDrawIt: true, drawShadows: true, centerX: false, bold: false, close: true, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 0.75f : 1f);
				if (_isStorageShop)
				{
					continue;
				}
				if (itemPriceAndStock[forSale[currentItemIndex + i]][0] > 0)
				{
					Utility.drawTextWithShadow(b, itemPriceAndStock[forSale[currentItemIndex + i]][0] + " ", Game1.smallFont, new Vector2((float)forSaleButtons[i].bounds.Right - Game1.smallFont.MeasureString(itemPriceAndStock[forSale[currentItemIndex + i]][0] + " ").X - 60f, forSaleButtons[i].bounds.Y + num6 + 40), Game1.textColor);
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(forSaleButtons[i].bounds.Right - 52, forSaleButtons[i].bounds.Y + num6 + 40 - 4), new Rectangle(193 + currency * 9, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				}
				else if (itemPriceAndStock[forSale[currentItemIndex + i]].Length > 2)
				{
					int quantity = 5;
					int num7 = itemPriceAndStock[forSale[currentItemIndex + i]][2];
					if (itemPriceAndStock[forSale[currentItemIndex + i]].Length > 3)
					{
						quantity = itemPriceAndStock[forSale[currentItemIndex + i]][3];
					}
					bool flag3 = Game1.player.hasItemInInventory(num7, quantity);
					if (canPurchaseCheck != null && !canPurchaseCheck(currentItemIndex + i))
					{
						flag3 = false;
					}
					float x = Game1.smallFont.MeasureString("x" + quantity).X;
					Utility.drawWithShadow(b, Game1.objectSpriteSheet, new Vector2((float)(forSaleButtons[i].bounds.Right - 88) - x, forSaleButtons[i].bounds.Y + 28 - 4), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, num7, 16, 16), Color.White * (flag3 ? 1f : 0.25f), 0f, Vector2.Zero, -1f, flipped: false, -1f, -1, -1, flag3 ? 0.35f : 0f);
					Color textColor = Game1.textColor;
					textColor.A = (byte)((float)(int)textColor.A * (flag3 ? 1f : 0.25f));
					Utility.drawTextWithShadow(b, "x" + quantity, Game1.smallFont, new Vector2((float)forSaleButtons[i].bounds.Right - x - 16f, forSaleButtons[i].bounds.Y + num6 + 40), textColor);
				}
			}
			scrollArea.finishScrollBoxDrawing(b);
			for (int num8 = animations.Count - 1; num8 >= 0; num8--)
			{
				if (animations[num8].update(Game1.currentGameTime))
				{
					animations.RemoveAt(num8);
				}
				else
				{
					animations[num8].draw(b, localPosition: true);
				}
			}
			if (scrollBarVisible && newScrollbar != null)
			{
				newScrollbar.draw(b);
			}
			if (heldItem != null)
			{
				heldItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: true);
			}
			if (storeContext != "Dresser")
			{
				if (portraitWidth > 0)
				{
					if (portraitPerson != null)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(portraitX, portraitY), new Rectangle(603, 414, 74, 74), Color.White, 0f, Vector2.Zero, -1f, flipped: false, 0.91f);
						if (portraitPerson.Portrait != null)
						{
							b.Draw(portraitPerson.Portrait, new Vector2(portraitX + 20, portraitY + 20), new Rectangle(0, 0, 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.92f);
						}
					}
					if (potraitPersonDialogue != null)
					{
						IClickableMenu.drawTextureBox(b, notesX, notesY, notesWidth, notesHeight, Color.White);
						Utility.drawMultiLineTextWithShadow(b, potraitPersonDialogue, Game1.smallFont, new Vector2(notesX + 16, notesY + 16), notesWidth - 32, notesHeight - 32, Game1.textColor);
					}
				}
				if (!_isStorageShop)
				{
					IClickableMenu.drawTextureBox(b, priceX, priceY, priceWidth, priceHeight, Color.White);
				}
				if (currentlySelectedItem != -1 && currentItem != null && !_isStorageShop)
				{
					if (forSale.Count > currentlySelectedItem)
					{
						IClickableMenu.drawMobileToolTip(b, priceX + 16, priceY + 16, priceWidth - 32, priceHeight - 32, 34, descItem, nameItem, currentItem as Item, heldItem != null, -1, currency, getHoveredItemExtraItemIndex(), getHoveredItemExtraItemAmount(), null, priceItem, currency, getPlayerCurrencyAmount(Game1.player, currency) >= itemPriceAndStock[forSale[currentlySelectedItem]][0], drawSmall: true);
					}
					if (priceItem > -1)
					{
						int hoveredItemExtraItemIndex = getHoveredItemExtraItemIndex();
						if (maxBuyable > 1 && priceItem > 0)
						{
							quantitySlider.draw(b);
							IClickableMenu.drawTextureBoxWithIconAndText(b, (Game1.clientBounds.Height <= 640) ? Game1.smallFont : Game1.dialogueFont, Game1.mouseCursors, new Rectangle(256, 256, 10, 10), Game1.mouseCursors, new Rectangle(193 + currency * 9, 373, 9, 10), "x" + quantityToBuy + ": " + priceItem * quantityToBuy, buyX, buyYWithSlider, buyWidth, buyHeight, Color.White, 4f, drawShadow: true, iconLeft: false, isClickable: true, buyButtonisHeld);
						}
						else if (priceItem <= 0 && hoveredItemExtraItemIndex > -1)
						{
							IClickableMenu.drawTextureBoxWithIconAndText(b, (Game1.clientBounds.Height <= 640) ? Game1.smallFont : Game1.dialogueFont, Game1.mouseCursors, new Rectangle(256, 256, 10, 10), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, hoveredItemExtraItemIndex, 16, 16), getHoveredItemExtraItemAmount().ToString() ?? "", buyX, buyY, buyWidth, buyHeight, Color.White, 4f, drawShadow: true, iconLeft: false, isClickable: true, buyButtonisHeld);
						}
						else
						{
							IClickableMenu.drawTextureBoxWithIconAndText(b, (Game1.clientBounds.Height <= 640) ? Game1.smallFont : Game1.dialogueFont, Game1.mouseCursors, new Rectangle(256, 256, 10, 10), Game1.mouseCursors, new Rectangle(193 + currency * 9, 373, 9, 10), priceItem.ToString() ?? "", buyX, buyY, buyWidth, buyHeight, Color.White, 4f, drawShadow: true, iconLeft: false, isClickable: true, buyButtonisHeld);
						}
						if (numOfCurrentItem > 0 && !inventoryVisible && (boughtItemTween == null || (boughtItemTween != null && !boughtItemTween.tweening)))
						{
							(currentItem as Item).drawInMenu(b, new Vector2((float)(invX + invWidth) - 80f * widthMod, (float)invY + 10f * heightMod), 1f, 1f, 0.008f, StackDrawType.Hide);
							Utility.drawTinyDigits(numOfCurrentItem, b, new Vector2((float)(invX + invWidth) - 80f * widthMod, invY) + new Vector2(64 - Utility.getWidthOfTinyDigitString(numOfCurrentItem, 3f) + 3, 48f), 3f, 1f, Color.White);
						}
					}
				}
			}
			for (int j = 0; j < tabButtons.Count; j++)
			{
				tabButtons[j].draw(b);
			}
			if (inventoryVisible)
			{
				if (!Game1.options.showMenuBackground)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				}
				IClickableMenu.drawTextureBox(b, portraitX, portraitY, width - edge * 2, height / 2, Color.White);
				inventory.draw(b);
				if (sellPanelPosition.X > -1f)
				{
					drawSellPanel(b);
				}
				if (sellQuantitySlider != null && itemPlayerIsSelling != null && itemPlayerIsSelling.Stack > 1)
				{
					sellQuantitySlider.draw(b);
				}
				if (upperRightCloseButton != null)
				{
					upperRightCloseButton.draw(b);
				}
			}
			if (boughtItemTween != null && boughtItemTween.tweening)
			{
				boughtItemTween.draw(b);
			}
			if (poof != null)
			{
				poof.draw(b);
			}
			if (!Game1.options.SnappyMenus || inventoryVisible)
			{
				drawMouse(b);
			}
		}

		public virtual void setUpStoreForContext()
		{
			tabButtons = new List<ClickableTextureComponent>();
			string text = storeContext;
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			case 8:
				switch (text[0])
				{
				case 'S':
					if (text == "SeedShop")
					{
						categoriesToSellHere.AddRange(new int[14]
						{
							-81, -75, -79, -80, -74, -17, -18, -6, -26, -5,
							-14, -19, -7, -25
						});
					}
					break;
				case 'F':
					if (!(text == "FishShop"))
					{
						if (text == "FishTank")
						{
							_isStorageShop = true;
						}
					}
					else
					{
						categoriesToSellHere.AddRange(new int[4] { -4, -23, -21, -22 });
					}
					break;
				}
				break;
			case 10:
				switch (text[0])
				{
				default:
					return;
				case 'B':
					break;
				case 'A':
					if (text == "AnimalShop")
					{
						categoriesToSellHere.AddRange(new int[4] { -18, -6, -5, -14 });
					}
					return;
				}
				if (!(text == "Blacksmith"))
				{
					break;
				}
				goto IL_016b;
			case 11:
				if (!(text == "VolcanoShop"))
				{
					break;
				}
				goto IL_016b;
			case 12:
				if (text == "ScienceHouse")
				{
					categoriesToSellHere.AddRange(new int[1] { -16 });
				}
				break;
			case 14:
				if (text == "AdventureGuild")
				{
					categoriesToSellHere.AddRange(new int[4] { -28, -98, -97, -96 });
				}
				break;
			case 19:
				if (text == "Furniture Catalogue")
				{
					ClickableTextureComponent clickableTextureComponent3 = null;
					_isCatalogue = true;
					clickableTextureComponent3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 48, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent3);
					clickableTextureComponent3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(80, 48, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent3);
					clickableTextureComponent3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 48, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent3);
					clickableTextureComponent3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 64, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent3);
					clickableTextureComponent3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 64, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent3);
					clickableTextureComponent3 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(80, 64, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent3);
				}
				break;
			case 9:
				if (text == "Catalogue")
				{
					ClickableTextureComponent clickableTextureComponent2 = null;
					_isCatalogue = true;
					clickableTextureComponent2 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 48, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent2);
					clickableTextureComponent2 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(48, 64, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent2);
					clickableTextureComponent2 = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(32, 64, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent2);
				}
				break;
			case 17:
				if (text == "ReturnedDonations")
				{
					_isStorageShop = true;
				}
				break;
			case 7:
				if (text == "Dresser")
				{
					categoriesToSellHere.AddRange(new int[4] { -95, -100, -97, -96 });
					_isStorageShop = true;
					ClickableTextureComponent clickableTextureComponent = null;
					clickableTextureComponent = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 48, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent);
					clickableTextureComponent = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 48, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent);
					clickableTextureComponent = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(32, 48, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent);
					clickableTextureComponent = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(48, 48, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent);
					clickableTextureComponent = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 64, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent);
					clickableTextureComponent = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 64, 16, 16), 4f)
					{
						myID = 99999 + tabButtons.Count,
						upNeighborID = -99998,
						downNeighborID = -99998,
						rightNeighborID = 3546
					};
					tabButtons.Add(clickableTextureComponent);
				}
				break;
			case 13:
			case 15:
			case 16:
			case 18:
				break;
				IL_016b:
				categoriesToSellHere.AddRange(new int[3] { -12, -2, -15 });
				break;
			}
		}

		public void repositionTabs()
		{
			if (!_isCatalogue && !_isStorageShop)
			{
				return;
			}
			for (int i = 0; i < tabButtons.Count; i++)
			{
				if (i == currentTab)
				{
					tabButtons[i].bounds.X = itemsX - tabButtons[i].bounds.Width + 12;
				}
				else
				{
					tabButtons[i].bounds.X = itemsX - tabButtons[i].bounds.Width;
				}
				tabButtons[i].bounds.Y = itemsY + i * tabButtons[i].bounds.Height;
			}
		}

		protected override void cleanupBeforeExit()
		{
			if (currency == 4)
			{
				Game1.specialCurrencyDisplay.ShowCurrency(null);
			}
			base.cleanupBeforeExit();
		}

		private void rebuildSaleButtons()
		{
			forSaleButtons.Clear();
			int num;
			if (itemPriceAndStock.Count > itemsPerPage)
			{
				scrollBarVisible = true;
				num = 44;
			}
			else
			{
				scrollBarVisible = false;
				num = itemsXoff + 4;
			}
			for (int i = 0; i < itemPriceAndStock.Count; i++)
			{
				int num2 = itemsY + itemsYoff + i * (itemsHeight / itemsPerPage);
				forSaleButtons.Add(new ClickableComponent(new Rectangle(itemsX + itemsXoff, itemsY + itemsYoff + i * (itemButtonHeight + 8), itemsWidth - itemsXoff - num, itemButtonHeight), i.ToString() ?? ""));
			}
		}

		public virtual void switchTab(int new_tab)
		{
			currentTab = new_tab;
			Game1.playSound("shwip");
			applyTab();
			repositionTabs();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				snapCursorToCurrentSnappedComponent();
			}
		}

		public virtual void applyTab()
		{
			if (storeContext == "Dresser")
			{
				if (currentTab == 0)
				{
					forSale = itemPriceAndStock.Keys.ToList();
				}
				else
				{
					forSale.Clear();
					foreach (ISalable key in itemPriceAndStock.Keys)
					{
						if (!(key is Item item))
						{
							continue;
						}
						if (currentTab == 1)
						{
							if (item.Category == -95)
							{
								forSale.Add(item);
							}
						}
						else if (currentTab == 2)
						{
							if (item is Clothing && (item as Clothing).clothesType.Value == 0)
							{
								forSale.Add(item);
							}
						}
						else if (currentTab == 3)
						{
							if (item is Clothing && (item as Clothing).clothesType.Value == 1)
							{
								forSale.Add(item);
							}
						}
						else if (currentTab == 4)
						{
							if (item.Category == -97)
							{
								forSale.Add(item);
							}
						}
						else if (currentTab == 5 && item.Category == -96)
						{
							forSale.Add(item);
						}
					}
				}
			}
			else if (storeContext == "Catalogue")
			{
				if (currentTab == 0)
				{
					forSale = itemPriceAndStock.Keys.ToList();
				}
				else
				{
					forSale.Clear();
					foreach (ISalable key2 in itemPriceAndStock.Keys)
					{
						if (!(key2 is Item item2))
						{
							continue;
						}
						if (currentTab == 1)
						{
							if (item2 is Wallpaper && (item2 as Wallpaper).isFloor.Value)
							{
								forSale.Add(item2);
							}
						}
						else if (currentTab == 2 && item2 is Wallpaper && !(item2 as Wallpaper).isFloor.Value)
						{
							forSale.Add(item2);
						}
					}
				}
			}
			else if (storeContext == "Furniture Catalogue")
			{
				if (currentTab == 0)
				{
					forSale = itemPriceAndStock.Keys.ToList();
				}
				else
				{
					forSale.Clear();
					foreach (ISalable key3 in itemPriceAndStock.Keys)
					{
						if (!(key3 is Item item3))
						{
							continue;
						}
						if (currentTab == 1)
						{
							if (item3 is Furniture && ((item3 as Furniture).furniture_type.Value == 5 || (item3 as Furniture).furniture_type.Value == 4 || (item3 as Furniture).furniture_type.Value == 11))
							{
								forSale.Add(item3);
							}
						}
						else if (currentTab == 2)
						{
							if (item3 is Furniture && ((item3 as Furniture).furniture_type.Value == 0 || (item3 as Furniture).furniture_type.Value == 1 || (item3 as Furniture).furniture_type.Value == 2 || (item3 as Furniture).furniture_type.Value == 3))
							{
								forSale.Add(item3);
							}
						}
						else if (currentTab == 3)
						{
							if (item3 is Furniture && ((item3 as Furniture).furniture_type.Value == 6 || (item3 as Furniture).furniture_type.Value == 13))
							{
								forSale.Add(item3);
							}
						}
						else if (currentTab == 4)
						{
							if (item3 is Furniture && (item3 as Furniture).furniture_type.Value == 12)
							{
								forSale.Add(item3);
							}
						}
						else if (currentTab == 5 && item3 is Furniture && ((item3 as Furniture).furniture_type.Value == 7 || (item3 as Furniture).furniture_type.Value == 10 || (item3 as Furniture).furniture_type.Value == 8 || (item3 as Furniture).furniture_type.Value == 9 || (item3 as Furniture).furniture_type.Value == 14))
						{
							forSale.Add(item3);
						}
					}
				}
			}
			currentItemIndex = 0;
			setCurrentItem(currentItemIndex);
			setScrollBarToCurrentIndex();
		}
	}
}
