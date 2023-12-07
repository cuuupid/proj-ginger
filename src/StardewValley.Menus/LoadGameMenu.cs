using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class LoadGameMenu : IClickableMenu, IDisposable
	{
		public abstract class MenuSlot : IDisposable
		{
			public int ActivateDelay;

			protected LoadGameMenu menu;

			public SpriteFont mainSlotFont;

			public MenuSlot(LoadGameMenu menu)
			{
				this.menu = menu;
				if (Game1.viewport.Width - Game1.xEdge * 2 < 1200)
				{
					mainSlotFont = Game1.smallFont;
				}
				else
				{
					mainSlotFont = Game1.dialogueFont;
				}
			}

			public void setFont(SpriteFont font)
			{
				mainSlotFont = font;
			}

			public virtual bool isLabelledSlot()
			{
				return false;
			}

			public abstract void Activate();

			public abstract void Draw(SpriteBatch b, int i);

			public virtual void Dispose()
			{
			}
		}

		public class SaveFileSlot : MenuSlot
		{
			public Farmer Farmer;

			public double redTimer;

			public int versionComparison;

			private int xBinOffset = -64 - Game1.xEdge * 2;

			private Vector2 _position;

			public SaveFileSlot(LoadGameMenu menu, Farmer farmer)
				: base(menu)
			{
				ActivateDelay = 2150;
				Farmer = farmer;
				versionComparison = Utility.CompareGameVersions(Game1.version, farmer.gameVersion, ignore_platform_specific: true, major_only: true);
			}

			public override void Activate()
			{
				SaveGame.Load(Farmer.slotName);
				Game1.exitActiveMenu();
			}

			protected virtual void drawSlotSaveNumber(SpriteBatch b, int i)
			{
				string s = menu.currentItemIndex + i + 1 + ".";
				int x = menu.slotButtons[i].bounds.X + 28 + 32 - SpriteText.getWidthOfString(menu.currentItemIndex + i + 1 + ".") / 2;
				int y = menu.slotButtons[i].bounds.Y + 36;
				SpriteText.drawString(b, s, x, y);
			}

			protected virtual string slotName()
			{
				return Farmer.Name;
			}

			protected virtual void drawSlotName(SpriteBatch b, int i)
			{
				SpriteText.drawString(b, slotName(), menu.slotButtons[i].bounds.X + 128 + 36, menu.slotButtons[i].bounds.Y + 36);
			}

			protected virtual void drawSlotShadow(SpriteBatch b, int i)
			{
				Vector2 vector = portraitOffset();
				b.Draw(Game1.shadowTexture, new Vector2((float)menu.slotButtons[i].bounds.X + vector.X + 32f, menu.slotButtons[i].bounds.Y + 128 + 16), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.8f);
			}

			protected virtual Vector2 portraitOffset()
			{
				return new Vector2(92f, 20f);
			}

			protected virtual void drawSlotFarmer(SpriteBatch b, int i)
			{
				Vector2 vector = portraitOffset();
				FarmerRenderer.isDrawingForUI = true;
				Farmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, 0, secondaryArm: false, flip: false), 0, new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 32), new Vector2((float)menu.slotButtons[i].bounds.X + vector.X, (float)menu.slotButtons[i].bounds.Y + vector.Y), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, Farmer);
				FarmerRenderer.isDrawingForUI = false;
			}

			protected virtual void drawSlotDate(SpriteBatch b, int i)
			{
				string text = ((!Farmer.dayOfMonthForSaveGame.HasValue || !Farmer.seasonForSaveGame.HasValue || !Farmer.yearForSaveGame.HasValue) ? Farmer.dateStringForSaveGame : Utility.getDateStringFor(Farmer.dayOfMonthForSaveGame.Value, Farmer.seasonForSaveGame.Value, Farmer.yearForSaveGame.Value));
				Utility.drawTextWithShadow(b, text, mainSlotFont, new Vector2(menu.slotButtons[i].bounds.X + 128 + 36, menu.slotButtons[i].bounds.Y + 64 + 40), Game1.textColor);
			}

			protected virtual string slotSubName()
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11019", Farmer.farmName);
			}

			protected virtual void drawSlotSubName(SpriteBatch b, int i)
			{
				string text = slotSubName();
				_position = new Vector2((float)(menu.slotButtons[i].bounds.X + xBinOffset + menu.width) - Game1.dialogueFont.MeasureString(text).X - 98f, menu.slotButtons[i].bounds.Y + 38);
				Utility.drawTextWithShadow(b, text, Game1.dialogueFont, _position, Game1.textColor);
			}

			protected virtual void drawSlotMoney(SpriteBatch b, int i)
			{
				string text = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", Utility.getNumberWithCommas(Farmer.Money));
				if (Farmer.Money == 1 && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt)
				{
					text = text.Substring(0, text.Length - 1);
				}
				int num = (int)mainSlotFont.MeasureString(text).X;
				_position.X -= num + 100;
				Utility.drawWithShadow(b, Game1.mouseCursors, _position, new Microsoft.Xna.Framework.Rectangle(193, 373, 9, 9), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				_position.X += 40f;
				if (LocalizedContentManager.CurrentLanguageCode != 0)
				{
					_position.Y += 5f;
				}
				_position.Y -= 4f;
				Utility.drawTextWithShadow(b, text, mainSlotFont, _position, Game1.textColor);
			}

			protected virtual void drawSlotTimer(SpriteBatch b, int i)
			{
				string hoursMinutesStringFromMilliseconds = Utility.getHoursMinutesStringFromMilliseconds(Farmer.millisecondsPlayed);
				int num = (int)mainSlotFont.MeasureString(hoursMinutesStringFromMilliseconds).X;
				Vector2 position = new Vector2(xBinOffset + menu.slotButtons[i].bounds.X + menu.width - num - 136, menu.slotButtons[i].bounds.Y + 64 + 36);
				Utility.drawWithShadow(b, Game1.mouseCursors, position, new Microsoft.Xna.Framework.Rectangle(595, 1748, 9, 11), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				position.X += 40f;
				position.Y += 8f;
				if (LocalizedContentManager.CurrentLanguageCode != 0)
				{
					position.Y += 5f;
				}
				position.Y -= 4f;
				Utility.drawTextWithShadow(b, hoursMinutesStringFromMilliseconds, mainSlotFont, position, Game1.textColor);
				position.Y += 4f;
				_position = position;
			}

			public virtual void drawVersionMismatchSlot(SpriteBatch b, int i)
			{
				drawSlotSaveNumber(b, i);
				drawSlotName(b, i);
				drawSlotSubName(b, i);
				string text = Farmer.gameVersion;
				if (text == "-1")
				{
					text = "<1.4";
				}
				string text2 = Game1.content.LoadString("Strings\\UI:VersionMismatch", text);
				Color color = Game1.textColor;
				if (Game1.currentGameTime.TotalGameTime.TotalSeconds < redTimer)
				{
					double num = redTimer - Game1.currentGameTime.TotalGameTime.TotalSeconds;
					if ((int)(num / 0.25) % 2 == 1)
					{
						color = Color.Red;
					}
				}
				Utility.drawTextWithShadow(b, text2, Game1.dialogueFont, new Vector2(menu.slotButtons[i].bounds.X + 128 + 36, menu.slotButtons[i].bounds.Y + 64 + 40), color);
			}

			public override void Draw(SpriteBatch b, int i)
			{
				if (i >= 0 && i < menu.slotButtons.Count)
				{
					if (versionComparison < 0)
					{
						drawVersionMismatchSlot(b, i);
						return;
					}
					drawSlotSaveNumber(b, i);
					drawSlotName(b, i);
					drawSlotShadow(b, i);
					drawSlotFarmer(b, i);
					drawSlotDate(b, i);
					drawSlotSubName(b, i);
					drawSlotTimer(b, i);
					drawSlotMoney(b, i);
				}
			}

			public new void Dispose()
			{
				Farmer.unload();
			}
		}

		public MobileScrollbox scrollArea;

		public MobileScrollbar newScrollbar;

		private Microsoft.Xna.Framework.Rectangle mainBox;

		private Microsoft.Xna.Framework.Rectangle clipBox;

		private Microsoft.Xna.Framework.Rectangle outerBox;

		private new int width;

		private new int height;

		private int windowInset;

		private float widthMod;

		private float heightMod;

		private bool sliderVisible;

		private bool _okSelected;

		private bool _cancelSelected;

		private int itemsPerPage = 4;

		private int storedSaves;

		private int itemHeight = 200;

		private SpriteFont mainFont;

		private ConfirmationDialog confirmBox;

		private ConfirmationDialog backupBox;

		private int _joypadSelectedItemIndex = -1;

		public const int region_upArrow = 800;

		public const int region_downArrow = 801;

		public const int region_okDelete = 802;

		public const int region_cancelDelete = 803;

		public const int region_slots = 900;

		public const int region_deleteButtons = 901;

		public const int region_navigationButtons = 902;

		public const int region_deleteConfirmations = 903;

		public List<ClickableComponent> slotButtons = new List<ClickableComponent>();

		public List<ClickableTextureComponent> deleteButtons = new List<ClickableTextureComponent>();

		protected int currentItemIndex;

		protected int timerToLoad;

		protected int selected = -1;

		protected int selectedForDelete = -1;

		public ClickableTextureComponent upArrow;

		public ClickableTextureComponent downArrow;

		public ClickableTextureComponent scrollBar;

		public ClickableTextureComponent okDeleteButton;

		public ClickableTextureComponent cancelDeleteButton;

		public ClickableComponent backButton;

		public bool scrolling;

		public bool deleteConfirmationScreen;

		protected List<MenuSlot> menuSlots = new List<MenuSlot>();

		private Microsoft.Xna.Framework.Rectangle scrollBarRunner;

		private string hoverText = "";

		protected bool loading;

		protected bool drawn;

		private bool deleting;

		private int _updatesSinceLastDeleteConfirmScreen;

		private Task<List<Farmer>> _initTask;

		private Task _deleteTask;

		private bool disposedValue;

		protected virtual List<MenuSlot> MenuSlots
		{
			get
			{
				return menuSlots;
			}
			set
			{
				menuSlots = value;
			}
		}

		public bool IsDoingTask()
		{
			if (_initTask == null && _deleteTask == null && !loading)
			{
				return deleting;
			}
			return true;
		}

		public override bool readyToClose()
		{
			if (_initTask == null && _deleteTask == null && !loading && !deleting)
			{
				return _updatesSinceLastDeleteConfirmScreen > 1;
			}
			return false;
		}

		public LoadGameMenu(int yTopOffset = 0)
			: base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height, showUpperRightCloseButton: true)
		{
			initializeUpperRightCloseButton();
			resetButtonStatus();
			width = Game1.uiViewport.Width;
			xPositionOnScreen = Game1.xEdge;
			yPositionOnScreen = upperRightCloseButton.bounds.Y + upperRightCloseButton.bounds.Height;
			height = Game1.uiViewport.Height;
			widthMod = (float)width / 1280f;
			heightMod = (float)height / 720f;
			scrolling = false;
			outerBox = new Microsoft.Xna.Framework.Rectangle(Game1.xEdge, yPositionOnScreen, width - 2 * Game1.xEdge, height - yPositionOnScreen);
			mainBox = new Microsoft.Xna.Framework.Rectangle(Game1.xEdge, yPositionOnScreen + yTopOffset, width - 2 * Game1.xEdge, height - yPositionOnScreen - yTopOffset);
			clipBox = new Microsoft.Xna.Framework.Rectangle(mainBox.X + 16, mainBox.Y + 16, mainBox.Width - 32, mainBox.Height - 32);
			newScrollbar = new MobileScrollbar(mainBox.X + mainBox.Width - 60, clipBox.Y, 55, clipBox.Height - 4, 0, 24);
			scrollArea = new MobileScrollbox(mainBox.X, mainBox.Y, mainBox.Width, mainBox.Height, 1000, clipBox, newScrollbar);
			itemsPerPage = clipBox.Height / itemHeight;
			positionChildren();
			MainActivity.instance.PromptForPermissionsIfNecessary(startListPopulation);
		}

		protected virtual bool hasDeleteButtons()
		{
			return true;
		}

		protected virtual void startListPopulation()
		{
			_initTask = new Task<List<Farmer>>(delegate
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				return FindSaveGames();
			});
			_initTask.Start();
		}

		protected virtual void addSaveFiles(List<Farmer> files)
		{
			MenuSlots.AddRange(((IEnumerable<Farmer>)files).Select((Func<Farmer, MenuSlot>)((Farmer file) => new SaveFileSlot(this, file))));
		}

		private static List<Farmer> FindSaveGames()
		{
			List<Farmer> list = new List<Farmer>();
			string savesPath = Game1.savesPath;
			if (Directory.Exists(savesPath))
			{
				string[] directories = Directory.GetDirectories(savesPath);
				string[] array = directories;
				foreach (string text in array)
				{
					string text2 = Path.Combine(savesPath, text, "SaveGameInfo");
					if (!File.Exists(text2))
					{
						continue;
					}
					Farmer farmer = null;
					try
					{
						using FileStream stream = File.OpenRead(text2);
						farmer = (Farmer)SaveGame.farmerSerializer.Deserialize(stream);
						SaveGame.loadDataToFarmer(farmer);
						farmer.slotName = text.Split(Path.DirectorySeparatorChar).Last();
						list.Add(farmer);
					}
					catch (Exception ex)
					{
						Console.WriteLine("Exception occured trying to access file '{0}'", text2);
						Console.WriteLine(ex.GetBaseException().ToString());
						farmer?.unload();
					}
				}
			}
			list.Sort();
			return list;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (confirmBox != null)
			{
				confirmBox.receiveGamePadButton(b);
				return;
			}
			if (backupBox != null)
			{
				backupBox.receiveGamePadButton(b);
				return;
			}
			if (_joypadSelectedItemIndex == -1 && (b == Buttons.DPadUp || b == Buttons.LeftThumbstickUp || b == Buttons.DPadDown || b == Buttons.LeftThumbstickDown))
			{
				_joypadSelectedItemIndex = currentItemIndex;
				Game1.playSound("shwip");
				return;
			}
			switch (b)
			{
			case Buttons.DPadUp:
			case Buttons.LeftThumbstickUp:
				_joypadSelectedItemIndex--;
				if (_joypadSelectedItemIndex < 0)
				{
					_joypadSelectedItemIndex = 0;
					break;
				}
				scrollArea.setYOffsetForScroll(-_joypadSelectedItemIndex * itemHeight);
				Game1.playSound("shwip");
				break;
			case Buttons.DPadDown:
			case Buttons.LeftThumbstickDown:
				_joypadSelectedItemIndex++;
				if (_joypadSelectedItemIndex >= MenuSlots.Count)
				{
					_joypadSelectedItemIndex = MenuSlots.Count - 1;
					break;
				}
				if (_joypadSelectedItemIndex > 1 && _joypadSelectedItemIndex < MenuSlots.Count - 2)
				{
					scrollArea.setYOffsetForScroll(-_joypadSelectedItemIndex * itemHeight);
				}
				Game1.playSound("shwip");
				break;
			default:
				_ = 4096;
				break;
			}
			if (b == Buttons.B && upperRightCloseButton != null)
			{
				receiveLeftClick(upperRightCloseButton.bounds.X, upperRightCloseButton.bounds.Y);
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(_joypadSelectedItemIndex);
			snapCursorToCurrentSnappedComponent();
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
		}

		public override void gameWindowSizeChanged(Microsoft.Xna.Framework.Rectangle oldBounds, Microsoft.Xna.Framework.Rectangle newBounds)
		{
			positionChildren();
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (confirmBox != null)
			{
				confirmBox.leftClickHeld(x, y);
			}
			else if (backupBox != null)
			{
				backupBox.leftClickHeld(x, y);
			}
			else if (MenuSlots.Count > itemsPerPage)
			{
				scrollArea.leftClickHeld(x, y);
				if (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y))
				{
					float num = newScrollbar.setY(y);
					scrollArea.setYOffsetForScroll((int)((0f - num) * (float)(MenuSlots.Count - itemsPerPage) * (float)itemHeight / 100f));
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (confirmBox != null)
			{
				confirmBox.releaseLeftClick(x, y);
			}
			if (backupBox != null)
			{
				backupBox.releaseLeftClick(x, y);
			}
			scrolling = false;
			if (scrollArea == null || (scrollArea != null && !scrollArea.havePanelScrolled))
			{
				if (timerToLoad > 0 || loading || deleting)
				{
					return;
				}
				if (!deleteConfirmationScreen)
				{
					for (int i = 0; i < slotButtons.Count; i++)
					{
						if (slotButtons[i].containsPoint(x, y) && i < MenuSlots.Count)
						{
							if (!(MenuSlots[currentItemIndex + i] is SaveFileSlot saveFileSlot) || saveFileSlot.versionComparison > -1)
							{
								timerToLoad = 2150;
								loading = true;
								Game1.playSound("select");
								selected = i;
								return;
							}
							saveFileSlot.redTimer = Game1.currentGameTime.TotalGameTime.TotalSeconds + 1.0;
							Game1.playSound("cancel");
						}
					}
				}
				if (deleteConfirmationScreen)
				{
					if (_cancelSelected)
					{
						deleteConfirmationScreen = false;
						selectedForDelete = -1;
						Game1.playSound("smallSelect");
						if (Game1.options.snappyMenus && Game1.options.gamepadControls)
						{
							currentlySnappedComponent = getComponentWithID(0);
							snapCursorToCurrentSnappedComponent();
						}
					}
					else if (_okSelected)
					{
						deleting = true;
						_deleteTask = new Task(delegate
						{
							Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
							deleteFile(selectedForDelete);
						});
						_deleteTask.Start();
						deleteConfirmationScreen = false;
						if (Game1.options.snappyMenus && Game1.options.gamepadControls)
						{
							currentlySnappedComponent = getComponentWithID(0);
							snapCursorToCurrentSnappedComponent();
						}
						Game1.playSound("trashcan");
					}
					return;
				}
			}
			if (scrollArea != null)
			{
				scrollArea.releaseLeftClick(x, y);
			}
			currentItemIndex = Math.Max(0, Math.Min(MenuSlots.Count - itemsPerPage, currentItemIndex));
			scrolling = false;
		}

		protected void setScrollBarToCurrentIndex()
		{
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			scrollArea.receiveScrollWheelAction(direction);
		}

		private void downArrowPressed()
		{
			downArrow.scale = downArrow.baseScale;
			currentItemIndex++;
			Game1.playSound("shwip");
			setScrollBarToCurrentIndex();
		}

		private void upArrowPressed()
		{
			upArrow.scale = upArrow.baseScale;
			currentItemIndex--;
			Game1.playSound("shwip");
			setScrollBarToCurrentIndex();
		}

		private void deleteFile(int which)
		{
			if (MenuSlots[which] is SaveFileSlot saveFileSlot)
			{
				Farmer farmer = saveFileSlot.Farmer;
				string slotName = farmer.slotName;
				string text = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), slotName));
				string savesPath = Game1.savesPath;
				string text2 = Path.Combine(savesPath, slotName);
				string text3 = Path.Combine(text2, "SaveGameInfo");
				string text4 = Path.Combine(text2, slotName);
				Log.It("LoadGameMenu.deleteFile saveDir:" + text2 + " saveGameInfoPath:" + text3 + " dataPath:" + text4);
				File.Delete(text3);
				File.Delete(text4);
				Directory.Delete(text2, recursive: true);
				SaveGame.deleteEmergencySaveIfCalled(slotName);
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (confirmBox != null)
			{
				confirmBox.receiveLeftClick(x, y);
				return;
			}
			if (backupBox != null)
			{
				backupBox.receiveLeftClick(x, y);
				return;
			}
			if (MenuSlots.Count > itemsPerPage)
			{
				scrollArea.receiveLeftClick(x, y);
				if (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y))
				{
					scrolling = true;
				}
			}
			base.receiveLeftClick(x, y, playSound);
			if (MenuSlots == null || selected != -1)
			{
				return;
			}
			for (int i = 0; i < deleteButtons.Count; i++)
			{
				if (deleteButtons[i].containsPoint(x, y) && i < MenuSlots.Count && !deleteConfirmationScreen)
				{
					deleteConfirmationScreen = true;
					Game1.playSound("drumkit6");
					selectedForDelete = currentItemIndex + i;
					confirmBox = new ConfirmationDialog(Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11023", (MenuSlots[selectedForDelete] as SaveFileSlot).Farmer.Name), okSelected, cancelSelected);
					resetButtonStatus();
					if (Game1.options.snappyMenus && Game1.options.gamepadControls)
					{
						currentlySnappedComponent = getComponentWithID(803);
						snapCursorToCurrentSnappedComponent();
					}
					break;
				}
			}
		}

		protected virtual void saveFileScanComplete()
		{
		}

		protected virtual bool checkListPopulation()
		{
			if (!deleteConfirmationScreen)
			{
				_updatesSinceLastDeleteConfirmScreen++;
			}
			else
			{
				_updatesSinceLastDeleteConfirmScreen = 0;
			}
			if (_initTask != null)
			{
				if (_initTask.IsCanceled || _initTask.IsCompleted || _initTask.IsFaulted)
				{
					if (_initTask.IsCompleted)
					{
						addSaveFiles(_initTask.Result);
						saveFileScanComplete();
					}
					_initTask = null;
				}
				return true;
			}
			return false;
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (storedSaves != MenuSlots.Count)
			{
				positionChildren();
				storedSaves = MenuSlots.Count;
			}
			if (scrollArea != null)
			{
				recalculateSlots();
				scrollArea.update(time);
				bool havePanelScrolled = scrollArea.havePanelScrolled;
				scrollArea.setMaxYOffset((MenuSlots.Count - itemsPerPage) * itemHeight);
				scrollArea.havePanelScrolled = havePanelScrolled;
				int yOffsetForScroll = scrollArea.getYOffsetForScroll();
				if (deleteButtons.Count > itemsPerPage)
				{
					for (int i = 0; i < deleteButtons.Count; i++)
					{
						deleteButtons[i].bounds.X = clipBox.X + clipBox.Width - 96 - 24;
						deleteButtons[i].bounds.Y = i * itemHeight + clipBox.Y + (itemHeight - 102) / 2 + yOffsetForScroll;
					}
				}
			}
			if (checkListPopulation())
			{
				return;
			}
			if (_deleteTask != null)
			{
				if (_deleteTask.IsCanceled || _deleteTask.IsCompleted || _deleteTask.IsFaulted)
				{
					if (!_deleteTask.IsCompleted)
					{
						selectedForDelete = -1;
					}
					_deleteTask = null;
					deleting = false;
				}
				return;
			}
			if (selectedForDelete >= 0 && selectedForDelete < MenuSlots.Count && !deleteConfirmationScreen && !deleting && MenuSlots[selectedForDelete] is SaveFileSlot saveFileSlot)
			{
				Farmer farmer = saveFileSlot.Farmer;
				farmer.unload();
				MenuSlots.RemoveAt(selectedForDelete);
				selectedForDelete = -1;
				positionChildren();
			}
			if (timerToLoad <= 0)
			{
				return;
			}
			timerToLoad -= time.ElapsedGameTime.Milliseconds;
			if (timerToLoad <= 0 && selected >= 0 && selected < MenuSlots.Count)
			{
				if (Game1.options.autoSave && SaveGame.newerBackUpExists((MenuSlots[selected] as SaveFileSlot).Farmer.slotName) != null)
				{
					backupBox = new ConfirmationDialog(Game1.content.LoadString("Strings\\UI:question_restore_backup"), backupSelected, mainSelected);
				}
				else
				{
					MenuSlots[selected].Activate();
				}
			}
		}

		protected virtual string getStatusText()
		{
			if (_initTask != null)
			{
				return Game1.content.LoadString("Strings\\UI:LoadGameMenu_LookingForSavedGames");
			}
			if (deleting)
			{
				return Game1.content.LoadString("Strings\\UI:LoadGameMenu_Deleting");
			}
			if (MenuSlots.Count == 0)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11022");
			}
			return null;
		}

		protected virtual void drawExtra(SpriteBatch b)
		{
		}

		protected virtual void drawSlotBackground(SpriteBatch b, int i, MenuSlot slot)
		{
			Color color = Color.White;
			if (((currentItemIndex + i == selected && timerToLoad % 150 > 75 && timerToLoad > 1000) || (selected == -1 && slotButtons[i].scale > 1f && !scrolling && !deleteConfirmationScreen)) && (deleteButtons.Count <= i || !deleteButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())))
			{
				color = Color.Wheat;
			}
			if (_joypadSelectedItemIndex == i)
			{
				color = Color.Wheat;
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(384, 396, 15, 15), slotButtons[i].bounds.X, slotButtons[i].bounds.Y, slotButtons[i].bounds.Width, slotButtons[i].bounds.Height, color, 4f, drawShadow: false);
		}

		protected virtual void drawBefore(SpriteBatch b)
		{
		}

		protected virtual void drawStatusText(SpriteBatch b)
		{
			string statusText = getStatusText();
			if (statusText != null)
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, statusText, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.X, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.Y);
			}
		}

		public override void draw(SpriteBatch b)
		{
			IClickableMenu.drawTextureBox(b, outerBox.X, outerBox.Y, outerBox.Width, outerBox.Height, Color.White);
			if (outerBox.Y != mainBox.Y)
			{
				drawMobileHorizontalPartition(b, mainBox.X + 8, mainBox.Y - 8, mainBox.Width - 16);
			}
			if (scrollArea != null)
			{
				scrollArea.setUpForScrollBoxDrawing(b);
			}
			if (selectedForDelete == -1 || !deleting || deleteConfirmationScreen)
			{
				for (int i = 0; i < slotButtons.Count; i++)
				{
					if (currentItemIndex + i < MenuSlots.Count)
					{
						drawSlotBackground(b, i, MenuSlots[currentItemIndex + i]);
						MenuSlots[currentItemIndex + i].Draw(b, i);
						if (deleteButtons.Count > i && !MenuSlots[currentItemIndex + i].isLabelledSlot())
						{
							deleteButtons[i].draw(b, Color.White * 0.75f, 0.08f);
							b.Draw(Game1.mouseCursors, deleteButtons[i].getVector2() + new Vector2(-1f, 0f) * 4f, new Microsoft.Xna.Framework.Rectangle(564, 129, 18, 10), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.081f);
						}
					}
				}
			}
			if (scrollArea != null)
			{
				scrollArea.finishScrollBoxDrawing(b);
			}
			drawStatusText(b);
			if (MenuSlots.Count > itemsPerPage)
			{
				newScrollbar.draw(b);
			}
			if (deleteConfirmationScreen && MenuSlots[selectedForDelete] is SaveFileSlot && confirmBox != null)
			{
				confirmBox.draw(b);
			}
			base.draw(b);
			if (backupBox != null)
			{
				backupBox.draw(b);
			}
			drawn = true;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		private void recalculateSlots()
		{
			int num = clipBox.Width - ((MenuSlots.Count > itemsPerPage) ? 32 : 0);
			int num2 = itemHeight;
			int x = clipBox.X;
			int num3 = clipBox.Y;
			deleteButtons.Clear();
			slotButtons.Clear();
			if (scrollArea != null)
			{
				num3 += scrollArea.getYOffsetForScroll();
				num3 = Utility.To4(num3);
			}
			for (int i = 0; i < MenuSlots.Count; i++)
			{
				Microsoft.Xna.Framework.Rectangle rectangle = default(Microsoft.Xna.Framework.Rectangle);
				rectangle.X = x;
				rectangle.Y = num3;
				rectangle.Width = num;
				rectangle.Height = num2;
				Microsoft.Xna.Framework.Rectangle bounds = rectangle;
				slotButtons.Add(new ClickableComponent(bounds, i.ToString() ?? "")
				{
					myID = i,
					region = 900,
					downNeighborID = ((i < itemsPerPage - 1) ? (-99998) : (-7777)),
					upNeighborID = ((i > 0) ? (-99998) : (-7777)),
					rightNeighborID = -99998,
					fullyImmutable = true
				});
				deleteButtons.Add(new ClickableTextureComponent("", new Microsoft.Xna.Framework.Rectangle(x + num - 96, Utility.To4(num3 + (num2 - 52) / 2), 64, 104), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10994"), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(564, 102, 18, 26), 4f)
				{
					myID = i + 100,
					region = 901,
					leftNeighborID = -99998,
					downNeighborImmutable = true,
					downNeighborID = -99998,
					upNeighborImmutable = true,
					upNeighborID = ((i > 0) ? (-99998) : (-1)),
					rightNeighborID = -99998
				});
				num3 += num2;
			}
		}

		private void positionChildren()
		{
			int num = clipBox.Width;
			int num2 = clipBox.Height;
			xTile.Dimensions.Rectangle uiViewport = Game1.uiViewport;
			int num3 = (uiViewport.Width - num) / 2;
			int num4 = (uiViewport.Height - num2) / 2;
			scrollArea.setYOffsetForScroll(0);
			newScrollbar.setPercentage(0f);
			int num5 = 12;
			new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen, yPositionOnScreen, width, height).Inflate(-num5, -num5);
			recalculateSlots();
		}

		public void backupSelected(Farmer who)
		{
			backupBox = null;
			SaveGame.Load((MenuSlots[selected] as SaveFileSlot).Farmer.slotName, loadEmergencySave: false, loadBackupSave: true);
			Game1.exitActiveMenu();
		}

		public void mainSelected(Farmer who)
		{
			backupBox = null;
			MenuSlots[selected].Activate();
		}

		public void okSelected(Farmer who)
		{
			_okSelected = true;
			confirmBox = null;
		}

		public void cancelSelected(Farmer who)
		{
			_cancelSelected = true;
			confirmBox = null;
		}

		public void resetButtonStatus()
		{
			_okSelected = (_cancelSelected = false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposedValue)
			{
				return;
			}
			if (disposing)
			{
				if (MenuSlots != null)
				{
					foreach (MenuSlot menuSlot in MenuSlots)
					{
						menuSlot.Dispose();
					}
					MenuSlots.Clear();
					MenuSlots = null;
				}
				if (_initTask != null)
				{
					_initTask = null;
				}
				if (_deleteTask != null)
				{
					_deleteTask = null;
				}
			}
			disposedValue = true;
		}

		~LoadGameMenu()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
