using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	public class CraftingPageMobile : IClickableMenu
	{
		public const int smallScreenY = 600;

		public int numInRow;

		public int numInCol = 4;

		public const int region_upArrow = 88;

		public const int region_downArrow = 89;

		public const int region_craftingSelectionArea = 8000;

		public const int region_craftingModifier = 200;

		private string descriptionText = "";

		private string hoverText = "";

		private Item hoverItem;

		private Item lastCookingHover;

		public List<Dictionary<ClickableTextureComponent, CraftingRecipe>> pagesOfCraftingRecipes = new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>();

		private int currentCraftingPage;

		private CraftingRecipe hoverRecipe;

		public ClickableTextureComponent upButton;

		public ClickableTextureComponent downButton;

		public ClickableTextureComponent selectedCraftingItem;

		public ClickableTextureComponent scrollBar;

		private bool cooking;

		private Rectangle scrollBarRunner;

		private float widthMod;

		private float heightMod;

		private Rectangle mainBox;

		private Rectangle sliderRunner;

		private Rectangle slider;

		private string headerText;

		private int upX;

		private int upY;

		private int downY;

		private Rectangle infoPanel;

		private Rectangle craftButton;

		private bool showCraftButton;

		private bool craftButtonHeld;

		private bool sliderVisible;

		private bool scrolling;

		private bool upButtonHeld;

		private bool downButtonHeld;

		private bool showQuantitySlider;

		private bool quantitySliderHeld;

		private ClickableTextureComponent[,] recipeImage;

		private ClickableComponent[,] recipeSquare;

		private CraftingRecipe[,] recipeActual;

		private int xSpace = 160;

		private int ySpace = 160;

		private int rows;

		private int firstRowShown;

		private int craftYWithSlider;

		private int craftYWithoutSlider;

		private int quantityWeCanMake;

		private int quantityToCraft;

		private MobileScrollbar newScrollbar;

		private MobileScrollbox scrollArea;

		private SliderBar quantitySlider;

		private TemporaryAnimatedSprite poof;

		private string inventoryFullText;

		private Vector2 inventoryFullTextSize;

		private string hoverTitle = "";

		public tweeningSprite craftedItemTween;

		protected List<Chest> _materialContainers;

		private int _selectedItemIndex;

		private float triggerPolling;

		private float triggerPollingAccel;

		private int selectedItemIndex
		{
			get
			{
				if (selectedCraftingItem != null)
				{
					int num = 0;
					int num2 = (cooking ? CraftingRecipe.cookingRecipes.Count : (recipeImage.Length / numInRow));
					for (int i = 0; i < num2; i++)
					{
						for (int j = 0; j < numInRow && recipeImage.Length > i * numInRow + j; j++)
						{
							if (recipeImage[j, i] != null && recipeImage[j, i] == selectedCraftingItem)
							{
								return num;
							}
							num++;
						}
					}
				}
				return 0;
			}
		}

		public CraftingPageMobile(int x, int y, int width, int height, bool cooking = false, int tabX = 300, List<Chest> material_containers = null)
		{
			_materialContainers = material_containers;
			if (!cooking && width > 0)
			{
				initialize(x, y, width, height);
			}
			else
			{
				x = Game1.xEdge;
				y = 0;
				initialize(Game1.xEdge, 0, Game1.uiViewport.Width - Game1.xEdge * 2, Game1.uiViewport.Height);
				if (Game1.gameMode == 3 && Game1.player != null && !Game1.eventUp)
				{
					Game1.player.Halt();
				}
			}
			inventoryFullText = Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588");
			inventoryFullTextSize = Game1.smallFont.MeasureString(inventoryFullText);
			mainBox.X = x;
			mainBox.Y = y;
			mainBox.Width = base.width;
			mainBox.Height = base.height;
			widthMod = (float)base.width / 1280f;
			heightMod = (float)base.height / (720f - (float)y);
			if (Game1.clientBounds.Height < 600)
			{
				infoPanel.X = tabX;
			}
			else
			{
				infoPanel.X = (int)(740f * widthMod);
			}
			infoPanel.Y = y;
			infoPanel.Height = base.height;
			infoPanel.Width = base.width - infoPanel.X;
			if (Game1.clientBounds.Height < 600)
			{
				craftButton.Width = infoPanel.Width / 3;
				craftButton.X = infoPanel.X + 32;
				craftButton.Height = Math.Min(80, (int)(80f * heightMod));
				craftButton.Y = (craftYWithoutSlider = infoPanel.Y + infoPanel.Height - 16 - (int)(16f * heightMod) - craftButton.Height);
				craftYWithSlider = craftButton.Y;
				quantitySlider = new SliderBar(infoPanel.X + infoPanel.Width - craftButton.Width - 32, craftYWithoutSlider - 8, 1);
				quantitySlider.bounds.Width = craftButton.Width - 32;
				quantitySlider.bounds.Height = Math.Min(100, (int)(100f * heightMod));
				showCraftButton = false;
				showQuantitySlider = false;
				quantitySliderHeld = false;
				numInCol = base.height / ySpace;
				numInRow = 2;
			}
			else
			{
				craftButton.Width = infoPanel.Width * 3 / 4;
				craftButton.X = infoPanel.X + xPositionOnScreen + (infoPanel.Width - craftButton.Width) / 2;
				craftButton.Height = Math.Min(80, (int)(80f * heightMod));
				craftButton.Y = (craftYWithoutSlider = infoPanel.Y + infoPanel.Height - 16 - (int)(16f * heightMod) - craftButton.Height);
				craftYWithSlider = craftYWithoutSlider - craftButton.Height - 16;
				quantitySlider = new SliderBar(craftButton.X + 16, craftYWithoutSlider, 1);
				quantitySlider.bounds.Width = craftButton.Width - 32;
				quantitySlider.bounds.Height = Math.Min(100, (int)(100f * heightMod));
				showCraftButton = false;
				showQuantitySlider = false;
				quantitySliderHeld = false;
				numInCol = base.height / ySpace;
				numInRow = Math.Max(4, (mainBox.Width - infoPanel.X) / 96);
			}
			if (cooking)
			{
				rows = CraftingRecipe.cookingRecipes.Count / numInRow;
				if (rows * numInRow < CraftingRecipe.cookingRecipes.Count)
				{
					rows++;
				}
				initializeUpperRightCloseButton();
			}
			else
			{
				rows = Game1.player.craftingRecipes.Count() / numInRow;
				if (rows * numInRow < Game1.player.craftingRecipes.Count())
				{
					rows++;
				}
				if (width == 0)
				{
					initializeUpperRightCloseButton();
				}
			}
			recipeImage = new ClickableTextureComponent[numInRow, rows];
			recipeSquare = new ClickableComponent[numInRow, rows];
			recipeActual = new CraftingRecipe[numInRow, rows];
			int length = recipeImage.Length;
			this.cooking = cooking;
			firstRowShown = 0;
			bool flag = false;
			headerText = (cooking ? Game1.content.LoadString("Strings\\UI:Collections_Cooking") : Game1.content.LoadString("Strings\\UI:GameMenu_Crafting"));
			if (rows > numInCol)
			{
				sliderVisible = true;
			}
			else
			{
				sliderVisible = false;
			}
			NetStringDictionary<int, NetInt> netStringDictionary = new NetStringDictionary<int, NetInt>();
			foreach (string key in CraftingRecipe.craftingRecipes.Keys)
			{
				if (Game1.player.craftingRecipes.ContainsKey(key))
				{
					netStringDictionary.Add(key, Game1.player.craftingRecipes[key]);
				}
			}
			List<string> list = new List<string>();
			if (!cooking)
			{
				foreach (string key2 in Game1.player.craftingRecipes.Keys)
				{
					list.Add(new string(key2.ToCharArray()));
				}
			}
			else
			{
				Game1.playSound("bigSelect");
				foreach (string key3 in CraftingRecipe.cookingRecipes.Keys)
				{
					list.Add(new string(key3.ToCharArray()));
				}
				list.Sort(delegate(string a, string b)
				{
					int num = -1;
					int value = -1;
					if (a != null && CraftingRecipe.cookingRecipes.ContainsKey(a))
					{
						string[] array = CraftingRecipe.cookingRecipes[a].Split('/');
						if (array.Length > 2 && int.TryParse(array[2], out var result))
						{
							num = result;
						}
					}
					if (b != null && CraftingRecipe.cookingRecipes.ContainsKey(b))
					{
						string[] array2 = CraftingRecipe.cookingRecipes[b].Split('/');
						if (array2.Length > 2 && int.TryParse(array2[2], out var result2))
						{
							value = result2;
						}
					}
					return num.CompareTo(value);
				});
			}
			setupRecipes(list);
			newScrollbar = new MobileScrollbar(xPositionOnScreen + infoPanel.X - 60, mainBox.Y + 16, 1, mainBox.Height - 32, 0, 40);
			scrollArea = new MobileScrollbox(clipRect: new Rectangle(mainBox.X + 8, mainBox.Y + 8, mainBox.Width - 16 - 15, mainBox.Height - 16), boxX: mainBox.X, boxY: mainBox.Y, boxWidth: mainBox.Width - infoPanel.Width - 32, boxHeight: mainBox.Height, boxContentHeight: (rows - numInCol) * ySpace + 32, scrollBar: newScrollbar);
			reset();
		}

		public void reset()
		{
			if (recipeImage[0, 0] == null)
			{
				return;
			}
			quantitySlider.value = 1;
			selectedCraftingItem = recipeImage[0, 0];
			hoverRecipe = recipeActual[0, 0];
			Item item = hoverRecipe.createItem();
			if (Game1.player.couldInventoryAcceptThisItem(item, message_if_full: false))
			{
				if (Game1.player.couldInventoryAcceptThisItem(item, message_if_full: false) && !recipeImage[0, 0].hoverText.Equals("ghosted") && recipeActual[0, 0].doesFarmerHaveIngredientsInInventory(getContainerContents()))
				{
					showCraftButton = true;
					if (item.maximumStackSize() > 1)
					{
						quantityWeCanMake = Math.Min(item.maximumStackSize() / recipeActual[0, 0].numberProducedPerCraft, recipeActual[0, 0].howManyCanWeMake(cooking ? getContainerContents() : null));
						if (quantityWeCanMake > 1)
						{
							showQuantitySlider = true;
							quantityToCraft = 1;
							craftButton.Y = craftYWithSlider;
						}
						else
						{
							showQuantitySlider = false;
							craftButton.Y = craftYWithoutSlider;
						}
					}
					else
					{
						showQuantitySlider = false;
						craftButton.Y = craftYWithoutSlider;
					}
				}
				else
				{
					showCraftButton = false;
				}
			}
			if (lastCookingHover == null || !lastCookingHover.Name.Equals(hoverRecipe.name))
			{
				lastCookingHover = hoverRecipe.createItem();
			}
		}

		protected virtual IList<Item> getContainerContents()
		{
			if (_materialContainers == null)
			{
				return null;
			}
			List<Item> list = new List<Item>();
			for (int i = 0; i < _materialContainers.Count; i++)
			{
				list.AddRange(_materialContainers[i].items);
			}
			return list;
		}

		private int craftingPageY()
		{
			return yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 16;
		}

		private void setupRecipes(List<string> playerRecipes)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if (!sliderVisible)
			{
				int num4 = numInRow * 64;
				xSpace = (infoPanel.X - 48 - (mainBox.X + 48 - Game1.xEdge) - num4) / (numInRow - 1);
			}
			else
			{
				int num4 = numInRow * 64;
				xSpace = (infoPanel.X - 48 - 24 - (mainBox.X + 48 - Game1.xEdge) - num4) / (numInRow - 1);
			}
			for (num2 = 0; num2 < rows; num2++)
			{
				for (num = 0; num < numInRow; num++)
				{
					if (playerRecipes.Count > num3)
					{
						CraftingRecipe craftingRecipe = new CraftingRecipe(playerRecipes[num3], cooking);
						recipeImage[num, num2] = new ClickableTextureComponent("", new Rectangle(mainBox.X + 48 + (64 + xSpace) * num, yPositionOnScreen + (craftingRecipe.bigCraftable ? 56 : 88) + num2 * ySpace, xSpace / 2, craftingRecipe.bigCraftable ? ySpace : (ySpace / 2)), null, (cooking && !Game1.player.cookingRecipes.ContainsKey(craftingRecipe.name)) ? "ghosted" : "", craftingRecipe.bigCraftable ? Game1.bigCraftableSpriteSheet : Game1.objectSpriteSheet, craftingRecipe.bigCraftable ? Game1.getArbitrarySourceRect(Game1.bigCraftableSpriteSheet, 16, 32, craftingRecipe.getIndexOfMenuView()) : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, craftingRecipe.getIndexOfMenuView(), 16, 16), 4f);
						recipeActual[num, num2] = craftingRecipe;
						recipeSquare[num, num2] = new ClickableComponent(new Rectangle(mainBox.X + 48 + (64 + xSpace) * num - 16, yPositionOnScreen + 96 + num2 * ySpace - 40, 96, 144), " ");
						num3++;
					}
				}
			}
		}

		private void drawRecipes(SpriteBatch b)
		{
			int num = ((!cooking) ? (recipeImage.Length / numInRow) : CraftingRecipe.cookingRecipes.Count);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < numInRow && recipeImage.Length > i * numInRow + j; j++)
				{
					if (recipeImage[j, i] == null)
					{
						continue;
					}
					recipeImage[j, i].bounds.Y = yPositionOnScreen + scrollArea.getYOffsetForScroll() + (recipeActual[j, i].bigCraftable ? 48 : 80) + i * ySpace;
					recipeSquare[j, i].bounds.Y = yPositionOnScreen + scrollArea.getYOffsetForScroll() + 80 + i * ySpace - 40;
					if (recipeImage[j, i].hoverText.Equals("ghosted"))
					{
						recipeImage[j, i].draw(b, Color.Black * 0.35f, 0.089f);
					}
					else if (!recipeActual[j, i].doesFarmerHaveIngredientsInInventory(getContainerContents()))
					{
						recipeImage[j, i].draw(b, Color.LightGray * 0.4f, 0.089f);
					}
					else
					{
						recipeImage[j, i].draw(b);
						if (recipeActual[j, i].numberProducedPerCraft > 1)
						{
							NumberSprite.draw(recipeActual[j, i].numberProducedPerCraft, b, new Vector2(recipeImage[j, i].bounds.X + 64 - 2, recipeImage[j, i].bounds.Y + 64 - 2), Color.White, 0.5f * (recipeImage[j, i].scale / 4f), 0.97f, 1f, 0);
						}
					}
					if (selectedCraftingItem == recipeImage[j, i])
					{
						IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(20, 96, 20, 20), recipeSquare[j, i].bounds.X, recipeSquare[j, i].bounds.Y, recipeSquare[j, i].bounds.Width, recipeSquare[j, i].bounds.Height, Color.White, 8f, drawShadow: false);
					}
				}
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			_selectedItemIndex = selectedItemIndex;
			triggerPolling = 0f;
			triggerPollingAccel = 0f;
			switch (b)
			{
			case Buttons.B:
				Game1.playSound("bigDeSelect");
				exitThisMenu();
				return;
			case Buttons.A:
				if (showCraftButton && selectedCraftingItem != null)
				{
					CraftSelectedRecipe();
					return;
				}
				break;
			}
			if (b == Buttons.DPadUp || b == Buttons.LeftThumbstickUp)
			{
				_selectedItemIndex -= numInRow;
			}
			else if (b == Buttons.DPadDown || b == Buttons.LeftThumbstickDown)
			{
				_selectedItemIndex += numInRow;
			}
			else if (b == Buttons.DPadLeft || b == Buttons.LeftThumbstickLeft)
			{
				_selectedItemIndex--;
			}
			else if (b == Buttons.DPadRight || b == Buttons.LeftThumbstickRight)
			{
				_selectedItemIndex++;
			}
			else
			{
				if ((b == Buttons.LeftTrigger || b == Buttons.X) && showQuantitySlider)
				{
					quantitySlider.changeValueBy(-1);
					quantityToCraft = Math.Max(quantityToCraft - 1, 1);
					quantitySlider.value = (int)((float)(quantityToCraft - 1) / (float)(quantityWeCanMake - 1) * 100f);
					return;
				}
				if ((b == Buttons.RightTrigger || b == Buttons.Y) && showQuantitySlider)
				{
					int num = 1;
					if (quantityToCraft == 0)
					{
						quantitySlider.changeValueBy(2);
						num = 2;
					}
					else
					{
						quantitySlider.changeValueBy(1);
					}
					quantityToCraft = Math.Min(quantityToCraft + num, quantityWeCanMake);
					quantitySlider.value = (int)((float)(quantityToCraft - 1) / (float)(quantityWeCanMake - 1) * 100f);
					return;
				}
			}
			int num2 = (cooking ? CraftingRecipe.cookingRecipes.Count : Game1.player.craftingRecipes.Keys.Count());
			if (_selectedItemIndex < 0)
			{
				_selectedItemIndex = 0;
			}
			else if (_selectedItemIndex >= num2)
			{
				_selectedItemIndex = num2 - 1;
			}
			int col = _selectedItemIndex % numInRow;
			int num3 = (int)Math.Floor((float)_selectedItemIndex / (float)numInRow);
			SelectRecipe(col, num3);
			int num4 = 160;
			int num5 = (int)Math.Floor((0f - (float)scrollArea.getYOffsetForScroll()) / (float)num4);
			int num6 = num5 + (int)Math.Floor((float)scrollArea.Bounds.Height / (float)num4);
			if (num3 >= num6)
			{
				int yOffsetForScroll = Math.Max(-scrollArea.maxYOffset, scrollArea.getYOffsetForScroll() - num4);
				scrollArea.setYOffsetForScroll(yOffsetForScroll);
			}
			else if (num3 <= num5)
			{
				int yOffsetForScroll2 = Math.Min(0, scrollArea.getYOffsetForScroll() + num4);
				scrollArea.setYOffsetForScroll(yOffsetForScroll2);
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if ((Game1.options.gamepadControls && direction == -1) || direction == 1)
			{
				direction *= 100;
			}
			base.receiveScrollWheelAction(direction);
			scrollArea.receiveScrollWheelAction(direction);
			if (selectedCraftingItem != null)
			{
				if (selectedCraftingItem.bounds.Top < scrollArea.Bounds.Top)
				{
					_selectedItemIndex += numInRow;
				}
				else if (selectedCraftingItem.bounds.Bottom > scrollArea.Bounds.Height)
				{
					_selectedItemIndex -= numInRow;
				}
				int col = _selectedItemIndex % numInRow;
				int row = (int)Math.Floor((float)_selectedItemIndex / (float)numInRow);
				SelectRecipe(col, row);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void releaseLeftClick(int x, int y)
		{
			scrolling = false;
			quantitySliderHeld = false;
			if (sliderVisible && upButtonHeld)
			{
				upButtonHeld = false;
			}
			if (sliderVisible && downButtonHeld)
			{
				downButtonHeld = false;
			}
			if (scrollArea == null || (scrollArea != null && !scrollArea.havePanelScrolled))
			{
				if (craftButtonHeld && showCraftButton && craftButton.Contains(x, y) && selectedCraftingItem != null)
				{
					CraftSelectedRecipe();
				}
				craftButtonHeld = false;
			}
			if (scrollArea != null)
			{
				scrollArea.releaseLeftClick(x, y);
			}
		}

		private void CraftSelectedRecipe()
		{
			clickCraftingRecipe(selectedCraftingItem);
			Item item = hoverRecipe.createItem();
			if (Game1.player.couldInventoryAcceptThisItem(item) && selectedCraftingItem != null && !selectedCraftingItem.hoverText.Equals("ghosted") && hoverRecipe != null && hoverRecipe.doesFarmerHaveIngredientsInInventory(getContainerContents()))
			{
				showCraftButton = true;
				if (item.maximumStackSize() > 1)
				{
					quantityWeCanMake = Math.Min(item.maximumStackSize() / hoverRecipe.numberProducedPerCraft, hoverRecipe.howManyCanWeMake(cooking ? getContainerContents() : null));
					if (quantityWeCanMake > 1)
					{
						showQuantitySlider = true;
						quantityToCraft = 1;
						craftButton.Y = craftYWithSlider;
					}
					else
					{
						showQuantitySlider = false;
						quantityToCraft = 1;
						craftButton.Y = craftYWithoutSlider;
					}
				}
				else
				{
					showQuantitySlider = false;
					quantityToCraft = 1;
					craftButton.Y = craftYWithoutSlider;
				}
			}
			else
			{
				showCraftButton = false;
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (quantitySliderHeld)
			{
				quantitySlider.click(x, y, wasAlreadyHeld: true);
				quantityToCraft = Math.Min(1 + (int)((float)(quantitySlider.value * quantityWeCanMake) / 100f), quantityWeCanMake);
			}
			if (scrollArea != null)
			{
				scrollArea.leftClickHeld(x, y);
			}
			if (craftButtonHeld && showCraftButton && !craftButton.Contains(x, y))
			{
				craftButtonHeld = false;
			}
			if (sliderVisible && scrolling && (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y)))
			{
				float num = newScrollbar.setY(y);
				scrollArea.setYOffsetForScroll((int)((0f - num) * (float)((rows - numInCol) * ySpace + 32) / 100f));
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.options.SnappyMenus)
			{
				return;
			}
			base.receiveLeftClick(x, y);
			int num = (cooking ? CraftingRecipe.cookingRecipes.Count : (recipeImage.Length / numInRow));
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < numInRow && recipeImage.Length > i * numInRow + j; j++)
				{
					if (recipeImage[j, i] != null && recipeSquare[j, i].containsPoint(x, y))
					{
						SelectRecipe(j, i);
					}
				}
			}
			if (scrollArea != null)
			{
				scrollArea.receiveLeftClick(x, y);
			}
			if (showCraftButton && craftButton.Contains(x, y))
			{
				craftButtonHeld = true;
			}
			if (showQuantitySlider && quantityWeCanMake > 1 && quantitySlider.bounds.Contains(x, y))
			{
				quantitySlider.click(x, y);
				quantitySliderHeld = true;
			}
			if (sliderVisible && (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y)))
			{
				scrolling = true;
			}
		}

		private void SelectRecipe(int col, int row)
		{
			Game1.playSound("smallSelect");
			selectedCraftingItem = recipeImage[col, row];
			if (recipeImage[col, row].hoverText.Equals("ghosted"))
			{
				hoverText = "???";
				hoverRecipe = null;
				showCraftButton = false;
				return;
			}
			hoverRecipe = recipeActual[col, row];
			Item item = hoverRecipe.createItem();
			if (Game1.player.couldInventoryAcceptThisItem(item))
			{
				if (!recipeImage[col, row].hoverText.Equals("ghosted") && recipeActual[col, row].doesFarmerHaveIngredientsInInventory(getContainerContents()))
				{
					showCraftButton = true;
					quantityToCraft = 1;
					if (item.maximumStackSize() > 1)
					{
						quantityWeCanMake = Math.Min(item.maximumStackSize() / hoverRecipe.numberProducedPerCraft, hoverRecipe.howManyCanWeMake(cooking ? getContainerContents() : null));
						if (quantityWeCanMake > 1)
						{
							showQuantitySlider = true;
							quantitySlider.value = 1;
							craftButton.Y = craftYWithSlider;
						}
						else
						{
							showQuantitySlider = false;
							craftButton.Y = craftYWithoutSlider;
						}
					}
					else
					{
						showQuantitySlider = false;
						craftButton.Y = craftYWithoutSlider;
					}
				}
				else
				{
					showCraftButton = false;
				}
				if (lastCookingHover == null || !lastCookingHover.Name.Equals(hoverRecipe.name))
				{
					lastCookingHover = hoverRecipe.createItem();
				}
			}
			else
			{
				showCraftButton = false;
			}
		}

		public override void update(GameTime time)
		{
			if (scrollArea != null)
			{
				scrollArea.update(time);
			}
			if (poof != null && poof.update(time))
			{
				poof = null;
			}
			if (craftedItemTween != null && craftedItemTween.tweening)
			{
				craftedItemTween.update(time);
				if (!craftedItemTween.tweening)
				{
					craftedItemTween = null;
				}
			}
			base.update(time);
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
					int num = (int)(triggerPollingAccel / 500f);
					quantitySlider.changeValueBy(-1 - num);
					quantityToCraft = Math.Max(quantityToCraft - 1 - num, 1);
					triggerPolling = 150f;
				}
			}
			else if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightTrigger))
			{
				triggerPolling += (float)time.ElapsedGameTime.TotalMilliseconds;
				triggerPollingAccel += (float)time.ElapsedGameTime.TotalMilliseconds;
				if (triggerPolling > 200f)
				{
					if (quantityToCraft == 0)
					{
						quantityToCraft = 1;
					}
					int num2 = (int)(triggerPollingAccel / 500f);
					quantitySlider.changeValueBy(1 + num2);
					quantityToCraft = Math.Min(quantityToCraft + 1 + num2, quantityWeCanMake);
					triggerPolling = 150f;
				}
			}
			quantitySlider.value = (int)((float)(quantityToCraft - 1) / (float)(quantityWeCanMake - 1) * 100f);
		}

		private void clickCraftingRecipe(ClickableTextureComponent itemToTween, bool playSound = true)
		{
			int num = Math.Max(1, quantityToCraft);
			quantityToCraft = Math.Max(hoverRecipe.numberProducedPerCraft, quantityToCraft * hoverRecipe.numberProducedPerCraft);
			Item item = hoverRecipe.createItem();
			List<KeyValuePair<int, int>> list = null;
			if (cooking && item is Object && (item as Object).Quality == 0)
			{
				list = new List<KeyValuePair<int, int>>();
				list.Add(new KeyValuePair<int, int>(917, 1));
				if (CraftingRecipe.DoesFarmerHaveAdditionalIngredientsInInventory(list, getContainerContents()))
				{
					(item as Object).Quality = 2;
				}
				else
				{
					list = null;
				}
			}
			item.Stack = quantityToCraft;
			Game1.player.checkForQuestComplete(null, -1, -1, item, null, 2);
			poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(craftButton.X + craftButton.Width / 2 - 32, craftButton.Y), flicker: false, flipped: false);
			Game1.playSound("fireball");
			craftedItemTween = new tweeningSprite(null, itemToTween, new Vector2(craftButton.X + craftButton.Width / 2 - 32, craftButton.Y), new Vector2(xPositionOnScreen + 8, yPositionOnScreen - 32), 500f);
			craftedItemTween.start();
			for (int i = 0; i < num; i++)
			{
				try
				{
					hoverRecipe.consumeIngredients(_materialContainers);
				}
				catch (Exception)
				{
				}
			}
			if (list != null)
			{
				if (playSound)
				{
					Game1.playSound("breathin");
				}
				CraftingRecipe.ConsumeAdditionalIngredients(list, _materialContainers);
				if (!CraftingRecipe.DoesFarmerHaveAdditionalIngredientsInInventory(list, getContainerContents()))
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Seasoning_UsedLast"));
				}
			}
			if (playSound)
			{
				Game1.playSound("coin");
			}
			if (!cooking && Game1.player.craftingRecipes.ContainsKey(hoverRecipe.name))
			{
				Game1.player.craftingRecipes[hoverRecipe.name] += hoverRecipe.numberProducedPerCraft * quantityToCraft;
			}
			if (cooking)
			{
				Game1.player.cookedRecipe(item.parentSheetIndex);
			}
			if (!cooking)
			{
				Game1.stats.checkForCraftingAchievements();
			}
			else
			{
				Game1.stats.checkForCookingAchievements();
			}
			if (item != null && Game1.player.couldInventoryAcceptThisItem(item))
			{
				Game1.player.addItemToInventoryBool(item);
				item = null;
			}
			quantitySlider.value = 1;
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
		}

		public override bool readyToClose()
		{
			return true;
		}

		public override void draw(SpriteBatch b)
		{
			if (!TutorialManager.Instance.craftingHasBeenSeen)
			{
				TutorialManager.Instance.craftingHasBeenSeen = true;
				TutorialManager.Instance.completeTutorial(tutorialType.DUMMY_CRAFTING);
			}
			IClickableMenu.drawTextureBox(b, mainBox.X, mainBox.Y, mainBox.Width, mainBox.Height, Color.White);
			IClickableMenu.drawTextureBox(b, mainBox.X, mainBox.Y, mainBox.Width - infoPanel.Width, infoPanel.Height, Color.White);
			scrollArea.setUpForScrollBoxDrawing(b);
			drawRecipes(b);
			scrollArea.finishScrollBoxDrawing(b);
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			base.draw(b);
			if (hoverRecipe != null)
			{
				IClickableMenu.drawMobileTextPanel(b, " ", Game1.smallFont, infoPanel.X + Game1.xEdge, infoPanel.Y + 16, infoPanel.Width - 32, infoPanel.Height - 32, 32, -1, hoverRecipe.DisplayName, -1, (cooking && lastCookingHover != null && Game1.objectInformation[(lastCookingHover as Object).parentSheetIndex].Split('/').Length > 7) ? Game1.objectInformation[(lastCookingHover as Object).parentSheetIndex].Split('/')[7].Split(' ') : null, lastCookingHover, 0, -1, -1, -1, -1, 1f, hoverRecipe, 0, inStockAndBuyable: true, drawBackgroundBox: false, avoidOffscreenCull: false, drawSmall: true, getContainerContents());
				if (showCraftButton)
				{
					if (showQuantitySlider)
					{
						quantitySlider.draw(b);
						IClickableMenu.drawButtonWithText(b, Game1.smallFont, Game1.content.LoadString("Strings\\UI:mobile_button_craft") + ": " + quantityToCraft, craftButton.X, craftButton.Y, craftButton.Width, craftButton.Height, Color.White, isClickable: true, craftButtonHeld);
					}
					else
					{
						IClickableMenu.drawButtonWithText(b, Game1.smallFont, Game1.content.LoadString("Strings\\UI:mobile_button_craft"), craftButton.X, craftButton.Y, craftButton.Width, craftButton.Height, Color.White, isClickable: true, craftButtonHeld);
					}
				}
				else if (Game1.player.MaxItems <= Game1.player.numberOfItemsInInventory<Item>())
				{
					Utility.drawTextWithShadow(b, inventoryFullText, Game1.smallFont, new Vector2((float)craftButton.X + ((float)craftButton.Width - inventoryFullTextSize.X) / 2f, (float)craftButton.Y + ((float)craftButton.Height - inventoryFullTextSize.Y) / 2f), Game1.textColor);
				}
			}
			if (sliderVisible)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, drawShadow: false);
				newScrollbar.draw(b);
			}
			if (Game1.activeClickableMenu.GetType() != typeof(GameMenu) && craftedItemTween != null)
			{
				craftedItemTween.draw(b);
			}
			if (poof != null)
			{
				poof.draw(b, localPosition: true);
			}
		}
	}
}
