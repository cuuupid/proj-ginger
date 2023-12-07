using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class OptionsPage : IClickableMenu
	{
		public const int itemsPerPage = 7;

		public const int indexOfGraphicsPage = 6;

		private string descriptionText = "";

		private string hoverText = "";

		public int currentItemIndex;

		private ClickableTextureComponent upArrow;

		private ClickableTextureComponent downArrow;

		private ClickableTextureComponent scrollBar;

		private List<OptionsElement> options = new List<OptionsElement>();

		private bool scrolling;

		public static int currentScrollY = -99999;

		private const int X_PADDING = 40;

		private const int Y_SPACING = 20;

		private int oldxEdge;

		private MobileScrollbar newScrollbar;

		private MobileScrollbox scrollArea;

		public static bool drawScrollBar;

		private OptionsDropDown _selectedDropdown;

		private OptionsElement _optionsSliderMenuMargin;

		private OptionsElement _optionsSliderToolbarPadding;

		private OptionsElement _optionsSliderToolbarSlotSize;

		private OptionsElement _optionsSliderDateTimeScale;

		private OptionsElement _optionsSliderInvisibleButtonWidth;

		private OptionsElement _optionElementClickedOn;

		public OptionsButton optionsButtonAdjustControls;

		private int ContentHeight
		{
			get
			{
				int num = 20;
				for (int i = 0; i < options.Count; i++)
				{
					num += options[i].ItemHeight + 20;
				}
				return num;
			}
		}

		public OptionsPage(int x, int y, int width, int height, float widthMod = 1f, float heightMod = 1f)
			: base(x, y, width, height)
		{
			OptionsPage optionsPage = this;
			initializeUpperRightCloseButton();
			oldxEdge = Game1.xEdge;
			drawScrollBar = true;
			options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:ExitToTitle"), OnClickExitToTitle));
			options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:swap_saves"), OnClickSwapSave));
			if (Game1.CurrentEvent == null && !Game1.eventUp && Game1.currentMinigame == null && !(Game1.currentLocation.name == "BeachNightMarket") && !(Game1.currentLocation.name == "MermaidHouse") && !(Game1.currentLocation.name == "Submarine"))
			{
				OptionsButton item = new OptionsButton(Game1.content.LoadString("Strings\\UI:save_backup"), OnClickSaveBackup);
				options.Add(item);
			}
			OptionsDropDown optionsDropDown = new OptionsDropDown("Controls", 139);
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de)
			{
				optionsDropDown.AddHelpButton("https://de.stardewvalleywiki.com/Mobile_Steuerungen");
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es)
			{
				optionsDropDown.AddHelpButton("https://es.stardewvalleywiki.com/Controles_M%C3%B3viles");
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
			{
				optionsDropDown.AddHelpButton("https://ja.stardewvalleywiki.com/%E3%83%A2%E3%83%90%E3%82%A4%E3%83%AB%E6%93%8D%E4%BD%9C%E6%96%B9%E6%B3%95");
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt)
			{
				optionsDropDown.AddHelpButton("https://pt.stardewvalleywiki.com/Controles_m%C3%B3veis");
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
			{
				optionsDropDown.AddHelpButton("https://ru.stardewvalleywiki.com/%D0%A3%D0%BF%D1%80%D0%B0%D0%B2%D0%BB%D0%B5%D0%BD%D0%B8%D0%B5_%D0%B8%D0%B3%D1%80%D0%BE%D0%B9_(%D0%BC%D0%BE%D0%B1%D0%B8%D0%BB%D1%8C%D0%BD%D1%8B%D0%BC%D0%B8)");
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
			{
				optionsDropDown.AddHelpButton("https://zh.stardewvalleywiki.com/%E7%A7%BB%E5%8A%A8%E6%93%8D%E4%BD%9C%E6%96%B9%E5%BC%8F");
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				optionsDropDown.AddHelpButton("https://ko.stardewvalleywiki.com/%EB%AA%A8%EB%B0%94%EC%9D%BC_%EC%A1%B0%EC%9E%91");
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr)
			{
				optionsDropDown.AddHelpButton("https://tr.stardewvalleywiki.com/Mobil_Kontroller");
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it)
			{
				optionsDropDown.AddHelpButton("https://it.stardewvalleywiki.com/Controlli_Mobile");
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
			{
				optionsDropDown.AddHelpButton("https://fr.stardewvalleywiki.com/Contr%C3%B4les_Mobiles");
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.hu)
			{
				optionsDropDown.AddHelpButton("https://hu.stardewvalleywiki.com/Mobil%20vez%C3%A9rl%C5%91k");
			}
			else
			{
				optionsDropDown.AddHelpButton("https://stardewvalleywiki.com/Mobile_Controls");
			}
			optionsDropDown.bounds.Width = (int)((float)(width - 92) * 0.75f);
			options.Add(optionsDropDown);
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:show_controls_toggle_button"), 140));
			_optionsSliderInvisibleButtonWidth = new OptionsSlider(Game1.content.LoadString("Strings\\UI:invisible_button_width"), 146, -1, -1, width);
			options.Add(_optionsSliderInvisibleButtonWidth);
			optionsButtonAdjustControls = new OptionsButton(Game1.content.LoadString("Strings\\UI:adjust_joypad_controls"), OnClickAdjustJoypadControls);
			options.Add(optionsButtonAdjustControls);
			options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11242"), 1, -1, -1, width));
			options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11243"), 2, -1, -1, width));
			options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11244"), 20, -1, -1, width));
			options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11245"), 21, -1, -1, width));
			options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:BiteChime")));
			OptionsDropDown optionsDropDown2 = new OptionsDropDown(Game1.content.LoadString("Strings\\StringsFromCSFiles:BiteChime"), 42);
			options.Add(optionsDropDown2);
			optionsDropDown2.bounds.Width = (int)((float)(width - 92) * 0.75f);
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options_ToggleAnimalSounds"), 43));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:mobile_options_vertical_toolbar"), 135));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:mobile_options_green_squares"), 137));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11275.ps4"), 138));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:mobile_options_bigger_numbers"), 141));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:mobile_options_larger_font"), 151));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:Options_ShowAdvancedCraftingInformation"), 34));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:pinch_zoom"), 147));
			options.Add(new OptionsElement(" "));
			options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11272"), 24));
			options.Add(new OptionsElement(" "));
			if (Game1.graphics.PreferredBackBufferWidth > 1200)
			{
				_optionsSliderMenuMargin = new OptionsSlider(Game1.content.LoadString("Strings\\UI:mobile_options_menu_side_margin"), 133, -1, -1, width);
				options.Add(_optionsSliderMenuMargin);
			}
			else
			{
				Game1.options.xEdge = (Game1.xEdge = 0);
			}
			_optionsSliderToolbarPadding = new OptionsSlider(Game1.content.LoadString("Strings\\UI:mobile_options_toolbar_padding"), 134, -1, -1, width);
			((OptionsSlider)_optionsSliderToolbarPadding).sliderMaxValue = 160;
			((OptionsSlider)_optionsSliderToolbarPadding).value = Game1.options.toolbarPadding;
			options.Add(_optionsSliderToolbarPadding);
			_optionsSliderToolbarSlotSize = new OptionsSlider(Game1.content.LoadString("Strings\\UI:mobile_options_toolbar_slot_size"), 148, -1, -1, width);
			((OptionsSlider)_optionsSliderToolbarSlotSize).sliderMinValue = 32;
			((OptionsSlider)_optionsSliderToolbarSlotSize).sliderMaxValue = 200;
			((OptionsSlider)_optionsSliderToolbarSlotSize).value = Game1.options.toolbarSlotSize;
			options.Add(_optionsSliderToolbarSlotSize);
			_optionsSliderDateTimeScale = new OptionsSlider(Game1.content.LoadString("Strings\\UI:mobile_options_date_time_size"), 149, -1, -1, width);
			((OptionsSlider)_optionsSliderDateTimeScale).sliderMinValue = 50;
			((OptionsSlider)_optionsSliderDateTimeScale).sliderMaxValue = 200;
			((OptionsSlider)_optionsSliderDateTimeScale).value = (int)(Game1.options.dateTimeScale * 100f);
			options.Add(_optionsSliderDateTimeScale);
			if (Game1.game1.CanTakeScreenshots())
			{
				options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:OptionsPage_ScreenshotHeader")));
				int index = options.Count;
				if (!Game1.game1.CanZoomScreenshots())
				{
					Action callback = delegate
					{
						OptionsElement e = optionsPage.options[index];
						Game1.flashAlpha = 1f;
						Console.WriteLine("{0}.greyedOut = {1}", e.label, true);
						e.greyedOut = true;
						Action onDone = delegate
						{
							Console.WriteLine("{0}.greyedOut = {1}", e.label, false);
							e.greyedOut = false;
						};
						string text2 = Game1.game1.takeMapScreenshot(null, null, onDone);
						if (text2 != null)
						{
							Game1.addHUDMessage(new HUDMessage(text2, 6));
						}
						Game1.playSound("cameraNoise");
					};
					OptionsButton optionsButton = new OptionsButton(Game1.content.LoadString("Strings\\UI:OptionsPage_ScreenshotHeader").Replace(":", ""), callback);
					if (Game1.game1.ScreenshotBusy)
					{
						optionsButton.greyedOut = true;
					}
					options.Add(optionsButton);
				}
				else
				{
					options.Add(new OptionsPlusMinusButton(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11254"), 36, new List<string> { "25%", "50%", "75%", "100%" }, new List<string> { "25%", "50%", "75%", "100%" }, Game1.mouseCursors2, new Rectangle(72, 31, 18, 16), delegate(string selection)
					{
						Game1.flashAlpha = 1f;
						selection = selection.Substring(0, selection.Length - 1);
						int result = 25;
						if (!int.TryParse(selection, out result))
						{
							result = 25;
						}
						string text = Game1.game1.takeMapScreenshot((float)result / 100f, null, null);
						if (text != null)
						{
							Game1.addHUDMessage(new HUDMessage(text, 6));
						}
						Game1.playSound("cameraNoise");
					}));
				}
				if (Game1.game1.CanBrowseScreenshots())
				{
					options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:OptionsPage_OpenFolder"), delegate
					{
						Game1.game1.BrowseScreenshots();
					}));
				}
			}
			newScrollbar = new MobileScrollbar(x + width - 24 - 32, y + 16, 24, height - 32, 16, 16);
			int num = 50;
			int num2 = height - 16;
			scrollArea = new MobileScrollbox(x, y, width, num2, ContentHeight - num2 + num, new Rectangle(16, y + 16, Game1.uiViewport.Width - 32, height - 32), newScrollbar);
			if (currentScrollY != -99999)
			{
				scrollArea.setYOffsetForScroll(currentScrollY);
				newScrollbar.setPercentage(100f * (float)(-currentScrollY) / (float)scrollArea.getMaxYOffset());
			}
			updateContentPositions();
		}

		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return true;
		}

		public override bool readyToClose()
		{
			return base.readyToClose();
		}

		private void waitForServerConnection(Action onConnection)
		{
			if (Game1.server == null)
			{
				return;
			}
			if (Game1.server.connected())
			{
				onConnection();
				return;
			}
			IClickableMenu thisMenu = Game1.activeClickableMenu;
			ConfirmationDialog.behavior onClose = delegate
			{
				Game1.activeClickableMenu = thisMenu;
			};
			ConfirmationDialog.behavior onConfirm = delegate(Farmer who)
			{
				onClose(who);
				onConnection();
			};
			Game1.activeClickableMenu = new ServerConnectionDialog(onConfirm, onClose);
		}

		private void offerInvite()
		{
			waitForServerConnection(delegate
			{
				Game1.server.offerInvite();
			});
		}

		private void showInviteCode()
		{
			IClickableMenu thisMenu = Game1.activeClickableMenu;
			waitForServerConnection(delegate
			{
				ConfirmationDialog.behavior behavior = delegate
				{
					Game1.activeClickableMenu = thisMenu;
				};
				string inviteCode = Game1.server.getInviteCode();
			});
		}

		public override void snapToDefaultClickableComponent()
		{
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
		}

		private void setScrollBarToCurrentIndex()
		{
		}

		public override void snapCursorToCurrentSnappedComponent()
		{
			if (currentlySnappedComponent != null && currentlySnappedComponent.myID < options.Count)
			{
				if (options[currentlySnappedComponent.myID + currentItemIndex] is OptionsInputListener)
				{
					Game1.setMousePosition(currentlySnappedComponent.bounds.Right - 48, currentlySnappedComponent.bounds.Center.Y - 12);
				}
				else
				{
					Game1.setMousePosition(currentlySnappedComponent.bounds.Left + 48, currentlySnappedComponent.bounds.Center.Y - 12);
				}
			}
			else if (currentlySnappedComponent != null)
			{
				base.snapCursorToCurrentSnappedComponent();
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			bool flag = false;
			for (int i = 0; i < options.Count; i++)
			{
				if (options[i] is OptionsSlider && ((OptionsSlider)options[i]).isSliding)
				{
					flag = true;
				}
			}
			if (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y))
			{
				float num = newScrollbar.setY(y);
				int num2 = 50;
				int num3 = height - 16;
				scrollArea.setYOffsetForScroll((int)((0f - num) * (float)(ContentHeight - num3 + num2) / 100f));
			}
			else if (scrolling && !flag)
			{
				scrollArea.leftClickHeld(x, y);
			}
			if (_optionElementClickedOn != null)
			{
				_optionElementClickedOn.leftClickHeld(x, y);
				if (_optionElementClickedOn == _optionsSliderMenuMargin)
				{
					GameMenu.drawEdgeRect = true;
				}
				else if (_optionElementClickedOn == _optionsSliderToolbarPadding)
				{
					GameMenu.drawToolbarRect = true;
				}
				else if (_optionElementClickedOn == _optionsSliderInvisibleButtonWidth)
				{
					GameMenu.drawInvisibleButtonBox = true;
				}
				else if (_optionElementClickedOn != _optionsSliderToolbarSlotSize)
				{
					_ = _optionElementClickedOn;
					_ = _optionsSliderDateTimeScale;
				}
			}
		}

		public override ClickableComponent getCurrentlySnappedComponent()
		{
			return currentlySnappedComponent;
		}

		public override void setCurrentlySnappedComponentTo(int id)
		{
			currentlySnappedComponent = getComponentWithID(id);
			snapCursorToCurrentSnappedComponent();
		}

		public override void receiveKeyPress(Keys key)
		{
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if (!GameMenu.forcePreventClose)
			{
				base.receiveScrollWheelAction(direction);
				if ((Game1.options.gamepadControls && direction == -1) || direction == 1)
				{
					direction *= 100;
				}
				scrollArea.receiveScrollWheelAction(direction);
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			scrollArea.releaseLeftClick(x, y);
			if (_optionElementClickedOn != null)
			{
				_optionElementClickedOn.releaseLeftClick(x, y);
				_optionElementClickedOn = null;
			}
		}

		private void downArrowPressed()
		{
			downArrow.scale = downArrow.baseScale;
			currentItemIndex++;
			setScrollBarToCurrentIndex();
			Game1.playSound("shiny4");
		}

		private void upArrowPressed()
		{
			upArrow.scale = upArrow.baseScale;
			currentItemIndex--;
			setScrollBarToCurrentIndex();
			Game1.playSound("shiny4");
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (GameMenu.forcePreventClose)
			{
				return;
			}
			currentItemIndex = Math.Max(0, Math.Min(options.Count - 7, currentItemIndex));
			if (upperRightCloseButton.containsPoint(x, y))
			{
				SaveStartupPreferences();
				return;
			}
			scrollArea.receiveLeftClick(x, y);
			scrolling = scrollArea.panelScrolling;
			if (_selectedDropdown != null && _selectedDropdown.dropDownOpen)
			{
				_selectedDropdown.receiveLeftClick(x, y);
				_selectedDropdown = null;
				return;
			}
			for (int num = options.Count - 1; num >= 0; num--)
			{
				if (options[num].bounds.Contains(x, y))
				{
					_optionElementClickedOn = options[num];
					_optionElementClickedOn.receiveLeftClick(x, y);
					if (_optionElementClickedOn is OptionsDropDown)
					{
						_selectedDropdown = _optionElementClickedOn as OptionsDropDown;
					}
					else
					{
						_selectedDropdown = null;
					}
					break;
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			if (!GameMenu.forcePreventClose)
			{
				descriptionText = "";
				hoverText = "";
			}
		}

		public override void draw(SpriteBatch b)
		{
			IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
			newScrollbar.draw(b);
			scrollArea.setUpForScrollBoxDrawing(b);
			for (int num = options.Count - 1; num >= 0; num--)
			{
				options[currentItemIndex + num].draw(b, 0, 0);
			}
			scrollArea.finishScrollBoxDrawing(b);
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
		}

		public static void SaveStartupPreferences()
		{
			Game1.options.lastSeenBuildNumber = MainActivity.instance.GetBuild();
			StartupPreferences startupPreferences = new StartupPreferences();
			startupPreferences.loadPreferences(async: false, applyLanguage: false);
			startupPreferences.clientOptions = Game1.options;
			startupPreferences.savePreferences(async: false);
			Game1.xEdge = startupPreferences.clientOptions.xEdge;
			Game1.toolbarPaddingX = startupPreferences.clientOptions.toolbarPadding;
			Game1.toolbar.itemSlotSize = (Game1.maxItemSlotSize = startupPreferences.clientOptions.toolbarSlotSize);
		}

		private void OnClickExitToTitle()
		{
			Game1.playSound("bigDeSelect");
			SaveStartupPreferences();
			Game1.ExitToTitle();
		}

		private void OnClickEmergencyBackupLoad()
		{
			Game1.playSound("bigDeSelect");
			SaveStartupPreferences();
			Game1.loadTitleTexture();
			SaveGame.checkForAndLoadEmergencySave();
		}

		private void OnClickEmergencyBackupSave()
		{
			Game1.playSound("bigDeSelect");
			SaveStartupPreferences();
			Game1.emergencyBackup();
		}

		private void OnClickSwapSave()
		{
			Game1.playSound("bigDeSelect");
			SaveStartupPreferences();
			if (SaveGame.swapForOldSave())
			{
				exitThisMenu();
			}
		}

		private void OnClickCrash()
		{
			int[] array = new int[1];
			Console.WriteLine("crash: " + array[2]);
		}

		private void OnClickSaveBackup()
		{
			Game1.playSound("bigDeSelect");
			SaveStartupPreferences();
			Game1.saveWholeBackup();
		}

		private void OnClickAdjustJoypadControls()
		{
			Game1.playSound("bigDeSelect");
			Game1.exitActiveMenu();
			Game1.activeClickableMenu = Game1.virtualJoypad;
			Log.It("OptionsPage.OnClickAdjustJoypadControls -> Game1.virtualJoypad.adjustmentMode = true;");
			Game1.virtualJoypad.adjustmentMode = true;
		}

		public override void update(GameTime time)
		{
			scrollArea.update(time);
			updateContentPositions();
			if (Game1.options.weaponControl == 2 || Game1.options.weaponControl == 3 || Game1.options.weaponControl == 0)
			{
				optionsButtonAdjustControls.enabled = false;
			}
			else
			{
				optionsButtonAdjustControls.enabled = true;
			}
		}

		private void updateContentPositions()
		{
			int y = 32 + scrollArea.getYOffsetForScroll() + yPositionOnScreen;
			for (int i = 0; i < options.Count; i++)
			{
				options[i].bounds.X = Game1.xEdge + 40;
				options[i].bounds.Y = y;
				y = options[i].bounds.Y + options[i].bounds.Height + ((Game1.uiViewport.Height < 600) ? 8 : 20);
			}
			currentScrollY = scrollArea.getYOffsetForScroll();
		}
	}
}
