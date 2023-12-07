using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	public class GameMenu : IClickableMenu
	{
		public const int inventoryTab = 0;

		public const int skillsTab = 1;

		public const int socialTab = 2;

		public const int mapTab = 4;

		public const int craftingTab = 3;

		public const int collectionsTab = 5;

		public const int optionsTab = 6;

		public const int exitTab = 7;

		public const int region_inventoryTab = 12340;

		public const int region_skillsTab = 12341;

		public const int region_socialTab = 12342;

		public const int region_mapTab = 12343;

		public const int region_craftingTab = 12344;

		public const int region_collectionsTab = 12345;

		public const int region_optionsTab = 12346;

		public const int region_exitTab = 12347;

		public const int numberOfTabs = 7;

		public int currentTab;

		public int lastOpenedNonMapTab;

		public string hoverText = "";

		public string descriptionText = "";

		public List<ClickableComponent> tabs = new List<ClickableComponent>();

		public List<IClickableMenu> pages = new List<IClickableMenu>();

		public bool invisible;

		public static bool forcePreventClose;

		public static bool bundleItemHovered;

		private bool _showJunimoMenuNext;

		public ClickableTextureComponent junimoNoteIcon;

		public new int width;

		public new int height;

		public float widthMod;

		public float heightMod;

		public int tabWidth;

		public static int tabHeight;

		public int tabCollisionHeight;

		public int tabY;

		public int edgeX;

		public int edgeY;

		public RasterizerState _rasterizerState = new RasterizerState
		{
			ScissorTestEnable = true
		};

		private int oldxEdge;

		public static bool drawEdgeRect;

		public static bool drawToolbarRect;

		public static bool drawInvisibleButtonBox;

		private int junimoNotePulser;

		public GameMenu(bool standardTabs = true, bool optionsOnly = false)
			: base(0, 0, IClickableMenu.viewport.Width, IClickableMenu.viewport.Height, showUpperRightCloseButton: true)
		{
			oldxEdge = Game1.xEdge;
			setupMenus(standardTabs, optionsOnly);
			if (Game1.activeClickableMenu == null)
			{
				Game1.playSound("bigSelect");
			}
			if (Game1.player.hasOrWillReceiveMail("canReadJunimoText") && !Game1.player.hasOrWillReceiveMail("JojaMember"))
			{
				Game1.player.hasCompletedCommunityCenter();
			}
			forcePreventClose = false;
			if (pages.Count > 0)
			{
				pages[currentTab].populateClickableComponentList();
				AddTabsToClickableComponents(pages[currentTab]);
			}
			if (Game1.getLocationFromName("CommunityCenter") != null)
			{
				(Game1.getLocationFromName("CommunityCenter") as CommunityCenter).refreshBundlesIngredientsInfo();
			}
			if (Game1.options.SnappyMenus)
			{
				changeTab(0, playSound: false);
			}
		}

		public GameMenu(int startingTab, int extra = -1)
			: this()
		{
			oldxEdge = Game1.xEdge;
			changeTab(startingTab);
			if (startingTab == 6 && extra != -1)
			{
				(pages[6] as OptionsPage).currentItemIndex = extra;
			}
		}

		public override void automaticSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (GetCurrentPage() != null)
			{
				GetCurrentPage().automaticSnapBehavior(direction, oldRegion, oldID);
			}
			else
			{
				base.automaticSnapBehavior(direction, oldRegion, oldID);
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			if (pages.Count == 0)
			{
				return;
			}
			if (currentTab < 0)
			{
				currentTab = 0;
			}
			else if (currentTab >= pages.Count)
			{
				currentTab = pages.Count - 1;
			}
			IClickableMenu clickableMenu = pages[currentTab];
			if (clickableMenu != null)
			{
				clickableMenu.snapToDefaultClickableComponent();
				if (junimoNoteIcon != null && !clickableMenu.allClickableComponents.Contains(junimoNoteIcon))
				{
					clickableMenu.allClickableComponents.Add(junimoNoteIcon);
				}
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			switch (b)
			{
			case Buttons.LeftShoulder:
				if (currentTab == 4)
				{
					Game1.activeClickableMenu = new GameMenu(3);
				}
				else if (currentTab >= 0 && pages[currentTab].readyToClose())
				{
					if (junimoNoteIcon != null && currentTab == 0 && !_showJunimoMenuNext)
					{
						receiveLeftClick(junimoNoteIcon.bounds.X, junimoNoteIcon.bounds.Y);
						_showJunimoMenuNext = true;
					}
					else
					{
						_showJunimoMenuNext = false;
						changeTab(currentTab - 1);
					}
				}
				else if (junimoNoteIcon != null)
				{
					receiveLeftClick(junimoNoteIcon.bounds.X, junimoNoteIcon.bounds.Y);
				}
				return;
			case Buttons.RightShoulder:
				if (currentTab == 4)
				{
					Game1.activeClickableMenu = new GameMenu(5);
				}
				else if (currentTab < 7 && pages[currentTab].readyToClose())
				{
					if (junimoNoteIcon != null && currentTab == 6 && !_showJunimoMenuNext)
					{
						receiveLeftClick(junimoNoteIcon.bounds.X, junimoNoteIcon.bounds.Y);
						_showJunimoMenuNext = true;
					}
					else
					{
						_showJunimoMenuNext = false;
						changeTab(currentTab + 1);
					}
				}
				return;
			case Buttons.B:
				if (currentTab == 5 && ((pages[currentTab] as CollectionsPage).currentTab != 6 || (pages[currentTab] as CollectionsPage).letterviewerSubMenu == null))
				{
					Game1.playSound("bigDeSelect");
					exitThisMenu();
					return;
				}
				break;
			}
			if (b == Buttons.B && currentTab != 5)
			{
				Game1.playSound("bigDeSelect");
				exitThisMenu();
			}
			else
			{
				pages[currentTab].receiveGamePadButton(b);
			}
		}

		public override void setUpForGamePadMode()
		{
			base.setUpForGamePadMode();
			if (pages.Count > currentTab)
			{
				pages[currentTab].setUpForGamePadMode();
			}
		}

		public override ClickableComponent getCurrentlySnappedComponent()
		{
			return pages[currentTab].getCurrentlySnappedComponent();
		}

		public override void setCurrentlySnappedComponentTo(int id)
		{
			pages[currentTab].setCurrentlySnappedComponentTo(id);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!invisible && !forcePreventClose)
			{
				base.receiveLeftClick(x, y, playSound);
				for (int i = 0; i < tabs.Count; i++)
				{
					if (new Rectangle(tabs[i].bounds.X, yPositionOnScreen, tabWidth, tabCollisionHeight).Contains(x, y) && currentTab != i && pages[currentTab].readyToClose())
					{
						changeTab(getTabNumberFromName(tabs[i].name));
						return;
					}
				}
				if (junimoNoteIcon != null && junimoNoteIcon.containsPoint(x, y) && pages[currentTab].readyToClose())
				{
					Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true);
				}
			}
			pages[currentTab].receiveLeftClick(x, y);
		}

		public static string getLabelOfTabFromIndex(int index)
		{
			return index switch
			{
				0 => Game1.content.LoadString("Strings\\UI:GameMenu_Inventory"), 
				1 => Game1.content.LoadString("Strings\\UI:GameMenu_Skills"), 
				2 => Game1.content.LoadString("Strings\\UI:GameMenu_Social"), 
				4 => Game1.content.LoadString("Strings\\UI:GameMenu_Map"), 
				3 => Game1.content.LoadString("Strings\\UI:GameMenu_Crafting"), 
				5 => Game1.content.LoadString("Strings\\UI:GameMenu_Collections"), 
				6 => Game1.content.LoadString("Strings\\UI:GameMenu_Options"), 
				7 => Game1.content.LoadString("Strings\\UI:GameMenu_Exit"), 
				_ => "", 
			};
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			pages[currentTab].receiveRightClick(x, y);
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			pages[currentTab].receiveScrollWheelAction(direction);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			hoverText = "";
			if (currentTab >= 0 && currentTab < pages.Count)
			{
				pages[currentTab].performHoverAction(x, y);
			}
			if (junimoNoteIcon != null)
			{
				if (bundleItemHovered && currentTab == 0)
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

		public int getTabNumberFromName(string name)
		{
			int result = -1;
			if (name != null)
			{
				switch (name.Length)
				{
				case 6:
					switch (name[1])
					{
					case 'k':
						if (name == "skills")
						{
							result = 1;
						}
						break;
					case 'o':
						if (name == "social")
						{
							result = 2;
						}
						break;
					}
					break;
				case 9:
					if (name == "inventory")
					{
						result = 0;
					}
					break;
				case 3:
					if (name == "map")
					{
						result = 4;
					}
					break;
				case 8:
					if (name == "crafting")
					{
						result = 3;
					}
					break;
				case 11:
					if (name == "collections")
					{
						result = 5;
					}
					break;
				case 7:
					if (name == "options")
					{
						result = 6;
					}
					break;
				case 4:
					if (name == "exit")
					{
						result = 7;
					}
					break;
				}
			}
			return result;
		}

		public override void update(GameTime time)
		{
			drawEdgeRect = (drawToolbarRect = (drawInvisibleButtonBox = false));
			if (currentTab >= 0 && currentTab < pages.Count)
			{
				pages[currentTab].update(time);
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			pages[currentTab].releaseLeftClick(x, y);
			if (oldxEdge != Game1.options.xEdge && Game1.options.xEdge != -1)
			{
				Game1.xEdge = Game1.options.xEdge;
				setupMenus();
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			pages[currentTab].leftClickHeld(x, y);
		}

		public override bool readyToClose()
		{
			if (!forcePreventClose)
			{
				return pages[currentTab].readyToClose();
			}
			return false;
		}

		public void changeTab(int whichTab, bool playSound = true)
		{
			if (tabs.Count == 0)
			{
				return;
			}
			if (whichTab < 0)
			{
				whichTab = tabs.Count - 1;
			}
			else if (whichTab >= tabs.Count)
			{
				whichTab = 0;
			}
			if (currentTab == 6)
			{
				OptionsPage.SaveStartupPreferences();
			}
			currentTab = getTabNumberFromName(tabs[whichTab].name);
			if (currentTab == -1)
			{
				currentTab = 0;
				return;
			}
			_ = currentTab;
			_ = 4;
			if (currentTab == 3)
			{
				(pages[currentTab] as CraftingPageMobile).reset();
			}
			if (playSound)
			{
				Game1.playSound("smallSelect");
			}
			if (!Game1.options.SnappyMenus)
			{
				return;
			}
			pages[currentTab].populateClickableComponentList();
			pages[currentTab].allClickableComponents.AddRange(tabs);
			setTabNeighborsForCurrentPage();
			snapToDefaultClickableComponent();
			if (whichTab == 2)
			{
				pages[currentTab].currentlySnappedComponent = tabs[2];
				snapCursorToCurrentSnappedComponent();
			}
			if (currentTab == 0)
			{
				if (junimoNoteIcon != null)
				{
					junimoNoteIcon.leftNeighborID = 11;
					junimoNoteIcon.downNeighborID = 105;
				}
				pages[currentTab].allClickableComponents.Add((pages[currentTab] as InventoryPage).inventory.trashCan);
				pages[currentTab].allClickableComponents.Add((pages[currentTab] as InventoryPage).inventory.organizeButton);
			}
		}

		public void setTabNeighborsForCurrentPage()
		{
			switch (currentTab)
			{
			case 0:
			{
				for (int j = 0; j < tabs.Count; j++)
				{
					tabs[j].downNeighborID = j;
				}
				break;
			}
			case 7:
			{
				for (int k = 0; k < tabs.Count; k++)
				{
					tabs[k].downNeighborID = 535;
				}
				break;
			}
			default:
			{
				for (int i = 0; i < tabs.Count; i++)
				{
					tabs[i].downNeighborID = -99999;
				}
				break;
			}
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!invisible)
			{
				if (!Game1.options.showMenuBackground)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.8f);
				}
				pages[currentTab].draw(b);
				b.End();
				Rectangle scissorRectangle = b.GraphicsDevice.ScissorRectangle;
				b.GraphicsDevice.ScissorRectangle = new Rectangle(Math.Max(0, scissorRectangle.X), Math.Max(yPositionOnScreen, scissorRectangle.Y), Math.Min(scissorRectangle.Width, width), Math.Min(scissorRectangle.Height, tabHeight + 17));
				b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, _rasterizerState);
				if (!forcePreventClose)
				{
					foreach (ClickableComponent tab in tabs)
					{
						if (tabs.Count <= 1)
						{
							continue;
						}
						Rectangle value = new Rectangle(40, 22, 9, 12);
						switch (tab.name)
						{
						case "inventory":
							value = new Rectangle(40, 22, 9, 12);
							break;
						case "social":
							value = new Rectangle(49, 24, 9, 8);
							break;
						case "map":
							value = new Rectangle(59, 22, 8, 10);
							break;
						case "crafting":
							value = new Rectangle(64, 67, 8, 10);
							break;
						case "collections":
							value = new Rectangle(72, 67, 7, 10);
							break;
						case "options":
							value = new Rectangle(79, 67, 10, 10);
							break;
						}
						int tabNumberFromName = getTabNumberFromName(tab.name);
						drawTab(b, tab.bounds.X, tab.bounds.Y, tabWidth, tabHeight - 4, currentTab == tabNumberFromName, currentTab == 0 || currentTab == 5);
						if (tab.name == "inventory")
						{
							string text = ((LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko) ? (Game1.player.numberOfItemsInInventory<Item>() + "/" + Game1.player.MaxItems) : (Game1.player.numberOfItemsInInventory<Item>().ToString() ?? ""));
							b.Draw(Game1.mobileSpriteSheet, Utility.To4(new Vector2((float)tab.bounds.X + ((tabWidth > 140) ? (16f * widthMod) : ((float)((tabWidth - 32) / 2))), tab.bounds.Y + 20 + (12 - value.Height) * 4 / 2)), value, (currentTab == tabNumberFromName) ? Color.White : Color.DarkGray, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
							if (tabWidth > 140)
							{
								Vector2 vector = Game1.smallFont.MeasureString(text);
								if (currentTab == tabNumberFromName)
								{
									Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2((float)tab.bounds.X + 16f * widthMod + (float)value.Width + (((float)(tabWidth - value.Width) - 16f * widthMod) / 2f - vector.X / 2f), (float)(tab.bounds.Y + (tabHeight + 20) / 2) - vector.Y / 2f), Game1.textColor);
								}
								else
								{
									b.DrawString(Game1.smallFont, text, Utility.To4(new Vector2((float)tab.bounds.X + 16f * widthMod + (float)value.Width + (((float)(tabWidth - value.Width) - 16f * widthMod) / 2f - vector.X / 2f), (float)(tab.bounds.Y + (tabHeight + 20) / 2) - vector.Y / 2f)), Game1.textColor);
								}
							}
						}
						else if (tab.name != "skills")
						{
							b.Draw(Game1.mobileSpriteSheet, Utility.To4(new Vector2(tab.bounds.X + (tabWidth - value.Width * 4) / 2, tab.bounds.Y + 16 + (14 - value.Height) * 4 / 2)), value, (currentTab == tabNumberFromName) ? Color.White : Color.DarkGray, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
						}
						else
						{
							Game1.player.FarmerRenderer.drawMiniPortrat(b, Utility.To4(new Vector2(tab.bounds.X + (tabWidth - 40) / 2, tab.bounds.Y + 16)), 0.00011f, 3f, 2, Game1.player);
						}
					}
					b.GraphicsDevice.ScissorRectangle = scissorRectangle;
					b.End();
					b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
					if (junimoNoteIcon != null)
					{
						junimoNoteIcon.draw(b);
					}
					if (!hoverText.Equals(""))
					{
						IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
					}
				}
			}
			else
			{
				pages[currentTab].draw(b);
			}
			if (pages.Count > 1 && currentTab == 0)
			{
				(pages[currentTab] as InventoryPage).inventory.drawDragItem(b);
			}
			else if (currentTab == 3 && (pages[currentTab] as CraftingPageMobile).craftedItemTween != null)
			{
				(pages[currentTab] as CraftingPageMobile).craftedItemTween.draw(b);
			}
			if (!forcePreventClose)
			{
				base.draw(b);
			}
			if (drawEdgeRect)
			{
				b.Draw(Game1.mobileSpriteSheet, new Vector2(Game1.options.xEdge, 0f), new Rectangle(20, 96, 1, 20), Color.White, 0f, Vector2.Zero, new Vector2(8f, IClickableMenu.viewport.Height), SpriteEffects.None, 0.001f);
				b.Draw(Game1.mobileSpriteSheet, new Vector2(IClickableMenu.viewport.Width - Game1.options.xEdge, 0f), new Rectangle(20, 96, 1, 20), Color.White, 0f, Vector2.Zero, new Vector2(8f, IClickableMenu.viewport.Height), SpriteEffects.None, 0.001f);
			}
			if (drawToolbarRect)
			{
				b.Draw(Game1.mobileSpriteSheet, new Vector2(Game1.toolbarPaddingX, 0f), new Rectangle(20, 96, 1, 20), Color.White, 0f, Vector2.Zero, new Vector2(8f, IClickableMenu.viewport.Height), SpriteEffects.None, 0.001f);
			}
			if (drawInvisibleButtonBox)
			{
				b.Draw(Game1.mouseCursors, new Vector2(IClickableMenu.viewport.Width - Game1.options.invisibleButtonWidth - 4, IClickableMenu.viewport.Height / 2), new Rectangle(64, 205, 4, 4), Color.White, 0f, Vector2.Zero, new Vector2(1f, IClickableMenu.viewport.Height / 8), SpriteEffects.None, 0.001f);
				b.Draw(Game1.mouseCursors, new Vector2(IClickableMenu.viewport.Width - 2 * Game1.options.invisibleButtonWidth - 4, IClickableMenu.viewport.Height / 2), new Rectangle(64, 205, 4, 4), Color.White, 0f, Vector2.Zero, new Vector2(1f, IClickableMenu.viewport.Height / 8), SpriteEffects.None, 0.001f);
				b.Draw(Game1.mouseCursors, new Vector2(IClickableMenu.viewport.Width - 2 * Game1.options.invisibleButtonWidth - 4, IClickableMenu.viewport.Height / 2 - 4), new Rectangle(64, 205, 4, 4), Color.White, 0f, Vector2.Zero, new Vector2(2 * Game1.options.invisibleButtonWidth / 4, 1f), SpriteEffects.None, 0.001f);
			}
			if (!Game1.options.hardwareCursor && (currentTab != 3 || !Game1.options.SnappyMenus) && (currentTab != 5 || !Game1.options.SnappyMenus))
			{
				b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.gamepadControls ? 44 : 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
		}

		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return currentTab == 6;
		}

		public override bool areGamePadControlsImplemented()
		{
			return false;
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.menuButton.Contains(new InputButton(key)) && readyToClose())
			{
				Game1.exitActiveMenu();
				Game1.playSound("bigDeSelect");
			}
			pages[currentTab].receiveKeyPress(key);
		}

		public override void emergencyShutDown()
		{
			base.emergencyShutDown();
			pages[currentTab].emergencyShutDown();
		}

		private void setupMenus(bool standardTabs = true, bool optionsOnly = false)
		{
			oldxEdge = Game1.xEdge;
			drawEdgeRect = (drawToolbarRect = (drawInvisibleButtonBox = false));
			width = IClickableMenu.viewport.Width;
			height = IClickableMenu.viewport.Height;
			if (height > 1080)
			{
				height = (height = 1080);
				yPositionOnScreen = Utility.To4(yPositionOnScreen = (IClickableMenu.viewport.Height - height) / 2);
			}
			widthMod = (float)(width - Game1.xEdge * 2) / 1280f;
			heightMod = (float)height / 720f;
			tabHeight = 72;
			tabWidth = (IClickableMenu.viewport.Width - Game1.xEdge * 2 - 80) / 8;
			tabCollisionHeight = 90;
			edgeX = Game1.xEdge;
			edgeY = 0;
			tabY = 4;
			tabs.Clear();
			pages.Clear();
			initializeUpperRightCloseButton();
			if (standardTabs)
			{
				int num = height - tabHeight - edgeY * 2;
				tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + tabY, tabWidth, tabCollisionHeight), "inventory", Game1.content.LoadString("Strings\\UI:GameMenu_Inventory"))
				{
					myID = 12340,
					downNeighborID = 0,
					rightNeighborID = 12341,
					tryDefaultIfNoDownNeighborExists = true,
					fullyImmutable = true
				});
				pages.Add(new InventoryPage(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + tabHeight, width - edgeX * 2, num));
				tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + edgeX + tabWidth, yPositionOnScreen + tabY + edgeY, tabWidth, tabCollisionHeight), "skills", Game1.content.LoadString("Strings\\UI:GameMenu_Skills"))
				{
					myID = 12341,
					downNeighborID = 1,
					rightNeighborID = 12342,
					leftNeighborID = 12340,
					tryDefaultIfNoDownNeighborExists = true,
					fullyImmutable = true
				});
				pages.Add(new SkillsPage(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + tabHeight, width - edgeX * 2, height - edgeY - tabHeight, widthMod, heightMod));
				tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + edgeX + tabWidth * 2, yPositionOnScreen + tabY + edgeY, tabWidth, tabHeight), "social", Game1.content.LoadString("Strings\\UI:GameMenu_Social"))
				{
					myID = 12342,
					downNeighborID = 2,
					rightNeighborID = 12344,
					leftNeighborID = 12341,
					tryDefaultIfNoDownNeighborExists = true,
					fullyImmutable = true
				});
				pages.Add(new SocialPage(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + tabHeight, width - edgeX * 2, height - edgeY - tabHeight));
				tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + edgeX + tabWidth * 3, yPositionOnScreen + tabY + edgeY, tabWidth, tabCollisionHeight), "crafting", Game1.content.LoadString("Strings\\UI:GameMenu_Crafting"))
				{
					myID = 12344,
					downNeighborID = 4,
					rightNeighborID = 12343,
					leftNeighborID = 12342,
					tryDefaultIfNoDownNeighborExists = true,
					fullyImmutable = true
				});
				pages.Add(new CraftingPageMobile(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + tabHeight, width - edgeX * 2, height - edgeY - tabHeight, cooking: false, xPositionOnScreen + edgeX + tabWidth * 3));
				tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + edgeX + tabWidth * 4, yPositionOnScreen + tabY + edgeY, tabWidth, tabCollisionHeight), "map", Game1.content.LoadString("Strings\\UI:GameMenu_Map"))
				{
					myID = 12343,
					downNeighborID = 3,
					rightNeighborID = 12345,
					leftNeighborID = 12344,
					tryDefaultIfNoDownNeighborExists = true,
					fullyImmutable = true
				});
				pages.Add(new MapPage(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + tabHeight, width - edgeX * 2, height - edgeY - tabHeight, widthMod, heightMod));
				tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + edgeX + tabWidth * 5, yPositionOnScreen + tabY + edgeY, tabWidth, tabCollisionHeight), "collections", Game1.content.LoadString("Strings\\UI:GameMenu_Collections"))
				{
					myID = 12345,
					downNeighborID = 5,
					rightNeighborID = 12346,
					leftNeighborID = 12343,
					tryDefaultIfNoDownNeighborExists = true,
					fullyImmutable = true
				});
				pages.Add(new CollectionsPage(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + tabHeight, width - edgeX * 2, height - edgeY - tabHeight, widthMod, heightMod, xPositionOnScreen + edgeX + tabWidth * 5));
				tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + edgeX + tabWidth * 6, yPositionOnScreen + tabY + edgeY, tabWidth, tabCollisionHeight), "options", Game1.content.LoadString("Strings\\UI:GameMenu_Options"))
				{
					myID = 12346,
					downNeighborID = 6,
					rightNeighborID = 12347,
					leftNeighborID = 12345,
					tryDefaultIfNoDownNeighborExists = true,
					fullyImmutable = true
				});
				pages.Add(new OptionsPage(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + tabHeight, width - edgeX * 2, height - edgeY - tabHeight));
				if (Game1.player.hasOrWillReceiveMail("canReadJunimoText") && !Game1.player.hasOrWillReceiveMail("JojaMember") && !Game1.player.hasCompletedCommunityCenter())
				{
					junimoNoteIcon = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + edgeX + 16 + tabWidth * 7, yPositionOnScreen + 4 + tabY + edgeY, 64, 64), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), 4f);
				}
				TutorialManager.Instance.completeTutorial(tutorialType.TAP_GAME_MENU);
			}
			else if (optionsOnly)
			{
				int num2 = (int)((float)yPositionOnScreen + 720f * heightMod - (float)tabHeight - (float)(edgeY * 2));
				tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + tabY, tabWidth, tabCollisionHeight), "options", Game1.content.LoadString("Strings\\UI:GameMenu_Options")));
				pages.Add(new OptionsPage(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + tabHeight, width - edgeX * 2, height - edgeY - tabHeight));
			}
		}

		public void AddTabsToClickableComponents(IClickableMenu menu)
		{
			menu.allClickableComponents.AddRange(tabs);
		}

		public override Type getMenuType()
		{
			if (pages[currentTab] != null)
			{
				return pages[currentTab].getMenuType();
			}
			return GetType();
		}

		public void drawTab(SpriteBatch b, int x, int y, int width, int height, bool isSelected = false, bool leftSmooth = false)
		{
			width -= 8;
			Rectangle rectangle = new Rectangle(91, 80, 16, 19);
			float num = 4f;
			Texture2D mobileSpriteSheet = Game1.mobileSpriteSheet;
			int num2 = 4;
			Color color = (isSelected ? Color.White : Color.DarkGray);
			int num3 = (isSelected ? 16 : 0);
			b.Draw(mobileSpriteSheet, new Rectangle((int)((float)num2 * num) + x, (int)((float)num2 * num) + y, width - (int)((float)num2 * num * 2f) + 4, tabHeight - (int)((float)num2 * num) + num3), new Rectangle(num2 + rectangle.X, num2 + rectangle.Y, num2, 2), color, 0f, Vector2.Zero, SpriteEffects.None, 0.8f);
			b.Draw(mobileSpriteSheet, new Vector2(x, y), new Rectangle(rectangle.X, rectangle.Y, num2, num2), color, 0f, Vector2.Zero, num, SpriteEffects.None, 0.8f);
			b.Draw(mobileSpriteSheet, new Vector2(x + width - (int)((float)num2 * num), y), new Rectangle(rectangle.X + num2 * 2, rectangle.Y, num2 + 2, num2), color, 0f, Vector2.Zero, num, SpriteEffects.None, 0.8f);
			b.Draw(mobileSpriteSheet, new Rectangle(x + (int)((float)num2 * num), y, width - (int)((float)num2 * num) * 2, (int)((float)num2 * num)), new Rectangle(rectangle.X + num2, rectangle.Y, num2, num2), color, 0f, Vector2.Zero, SpriteEffects.None, 0.8f);
			if (!isSelected || !leftSmooth)
			{
				b.Draw(mobileSpriteSheet, new Rectangle(x, y + (int)((float)num2 * num), (int)((float)num2 * num), tabHeight - (int)((float)num2 * num)), new Rectangle(rectangle.X, num2 + rectangle.Y, num2, 2), color, 0f, Vector2.Zero, SpriteEffects.None, 0.8f);
			}
			else
			{
				b.Draw(mobileSpriteSheet, new Rectangle(x, y + (int)((float)num2 * num), (int)((float)num2 * num), tabHeight - (int)((float)num2 * num) + num3), new Rectangle(rectangle.X, num2 + rectangle.Y, num2, 2), color, 0f, Vector2.Zero, SpriteEffects.None, 0.8f);
			}
			b.Draw(mobileSpriteSheet, new Rectangle(x + width - (int)((float)num2 * num), y + (int)((float)num2 * num), (int)(6f * num), tabHeight - (int)((float)num2 * num)), new Rectangle(rectangle.X + num2 * 2, num2 + rectangle.Y, 6, 1), color, 0f, Vector2.Zero, SpriteEffects.None, 0.8f);
			if (isSelected)
			{
				b.Draw(mobileSpriteSheet, new Rectangle(x + width - 20, yPositionOnScreen + edgeY + tabHeight, 24, 20), new Rectangle(98, 92, 6, 5), color, 0f, Vector2.Zero, SpriteEffects.None, 0.8f);
				if (!leftSmooth)
				{
					b.Draw(mobileSpriteSheet, new Rectangle(x, yPositionOnScreen + edgeY + tabHeight, 24, 20), new Rectangle(91, 92, 6, 5), color, 0f, Vector2.Zero, SpriteEffects.None, 0.8f);
				}
			}
		}

		public IClickableMenu GetCurrentPage()
		{
			if (currentTab >= pages.Count || currentTab < 0)
			{
				return null;
			}
			return pages[currentTab];
		}
	}
}
