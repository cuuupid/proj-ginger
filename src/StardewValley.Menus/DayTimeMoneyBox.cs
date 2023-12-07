using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Mobile;
using StardewValley.Tools;

namespace StardewValley.Menus
{
	public class DayTimeMoneyBox : IClickableMenu
	{
		public static bool DEBUG_CONSOLE_ENABLED = true;

		public const int HIT_AREA_WIDTH = 80;

		public const int HIT_AREA_HEIGHT = 280;

		public ClickableTextureComponent buttonGameMenu;

		public ClickableTextureComponent buttonF8;

		public Game1 game1;

		private int paddingX = 12;

		private int paddingY = 12;

		private int spacing = 12;

		private bool drawingJustTheMenuButton;

		private bool _buttonGameMenuDown;

		private bool _buttonJournalDown;

		public Vector2 position;

		private Rectangle sourceRect;

		public MoneyDial moneyDial = new MoneyDial(8);

		public int timeShakeTimer;

		public int moneyShakeTimer;

		public int questPulseTimer;

		public int whenToPulseTimer;

		public ClickableTextureComponent questButton;

		public ClickableTextureComponent zoomOutButton;

		public ClickableTextureComponent zoomInButton;

		private StringBuilder _hoverText = new StringBuilder();

		private StringBuilder _timeText = new StringBuilder();

		private StringBuilder _dateText = new StringBuilder();

		private StringBuilder _hours = new StringBuilder();

		private StringBuilder _padZeros = new StringBuilder();

		private StringBuilder _temp = new StringBuilder();

		private int _lastDayOfMonth = -1;

		private string _lastDayOfMonthString;

		private string _amString;

		private string _pmString;

		private LocalizedContentManager.LanguageCode _languageCode = (LocalizedContentManager.LanguageCode)(-1);

		public bool questsDirty;

		public int questPingTimer;

		private Vector2 _datePosition = new Vector2(20f, 15f);

		private Vector2 _timePosition = new Vector2(196f, 15f);

		private int scaledViewportWidth => (int)((float)Game1.uiViewport.Width / Game1.DateTimeScale);

		private bool ShowingTutorial
		{
			get
			{
				if (TutorialManager.Instance.currentTutorial != null && (TutorialManager.Instance.currentTutorial.tType == tutorialType.TAP_JOURNAL || TutorialManager.Instance.currentTutorial.tType == tutorialType.TAP_GAME_MENU))
				{
					return false;
				}
				return TutorialManager.Instance.ShowingDialogueBox;
			}
		}

		public bool buttonGameMenuVisible
		{
			get
			{
				if (Game1.displayHUD || (Game1.activeClickableMenu == null && Game1.CurrentEvent != null && Game1.pauseTime <= 0f && Game1.CurrentEvent.playerControlSequence && Game1.CurrentEvent.playerControlSequenceID != null && (Game1.CurrentEvent.playerControlSequenceID == "fair" || Game1.CurrentEvent.playerControlSequenceID == "iceFestival" || Game1.CurrentEvent.playerControlSequenceID == "eggFestival" || Game1.CurrentEvent.playerControlSequenceID == "halloween" || Game1.CurrentEvent.playerControlSequenceID == "eggHunt" || Game1.CurrentEvent.playerControlSequenceID == "flowerFestival" || Game1.CurrentEvent.playerControlSequenceID == "luau" || Game1.CurrentEvent.playerControlSequenceID == "jellies" || Game1.CurrentEvent.playerControlSequenceID == "christmas" || (Game1.CurrentEvent.eventCommands != null && Game1.CurrentEvent.eventCommands.Length > Game1.CurrentEvent.CurrentCommand && Game1.CurrentEvent.eventCommands[Game1.CurrentEvent.CurrentCommand].Equals("playerControl christmas2")))))
				{
					return true;
				}
				return false;
			}
		}

		public DayTimeMoneyBox()
			: base(Game1.uiViewport.Width - 80, 0, 80, 280)
		{
			initialize(Game1.uiViewport.Width - 80, 0, 80, 280);
			paddingX = Math.Max(12, Game1.xEdge);
			sourceRect = new Rectangle(0, 153, 92, 29);
			SetPositionTopRight();
			buttonF8 = new ClickableTextureComponent(new Rectangle((int)position.X, (int)position.Y, sourceRect.Width * 4, sourceRect.Height * 4), Game1.mobileSpriteSheet, sourceRect, 4f);
			questButton = new ClickableTextureComponent(new Rectangle(scaledViewportWidth - 64 - paddingX, (int)position.Y + sourceRect.Height * 4 + spacing, 64, 64), Game1.mobileSpriteSheet, new Rectangle(0, 44, 16, 16), 4f);
			buttonGameMenu = new ClickableTextureComponent(new Rectangle(scaledViewportWidth - 64 - paddingX, questButton.bounds.Y + questButton.bounds.Height + spacing, 64, 64), Game1.mobileSpriteSheet, new Rectangle(16, 44, 16, 16), 4f);
		}

		public override bool isWithinBounds(int x, int y)
		{
			x = (int)((float)x / Game1.DateTimeScale);
			y = (int)((float)y / Game1.DateTimeScale);
			if (Game1.options.zoomButtons && ((zoomInButton != null && zoomInButton.containsPoint(x, y)) || (zoomOutButton != null && zoomOutButton.containsPoint(x, y))))
			{
				return true;
			}
			if (Game1.player.visibleQuestCount > 0 && questButton.containsPoint(x, y))
			{
				return true;
			}
			return false;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!Game1.virtualJoypad.joystickHeld && (Game1.currentLocation == null || !Game1.currentLocation.tapToMove.TapHoldActive) && !(Game1.activeClickableMenu is MuseumMenu) && !(Game1.currentLocation is MermaidHouse))
			{
				if (!drawingJustTheMenuButton)
				{
					updatePosition();
				}
				x = (int)((float)x / Game1.DateTimeScale);
				y = (int)((float)y / Game1.DateTimeScale);
				if (buttonGameMenu.containsPoint(x, y) && ((buttonGameMenuVisible && !Game1.player.isEating && Game1.player.CanMove && (!Game1.player.usingTool || !(Game1.player.CurrentTool is FishingRod))) || drawingJustTheMenuButton) && !PinchZoom.Instance.Pinching && !ShowingTutorial)
				{
					OnTapGameMenuButton();
				}
				if (Game1.player.visibleQuestCount > 0 && questButton.containsPoint(x, y) && !Game1.player.isEating && Game1.player.CanMove && (!Game1.player.usingTool || !(Game1.player.CurrentTool is FishingRod)) && !Game1.eventUp && !PinchZoom.Instance.Pinching && !ShowingTutorial)
				{
					OnTapJournalButton();
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			updatePosition();
		}

		public void questIconPulse()
		{
			questPulseTimer = 2000;
		}

		public override void performHoverAction(int x, int y)
		{
			updatePosition();
		}

		public void drawJustTheGameMenuButton(SpriteBatch b)
		{
			drawingJustTheMenuButton = false;
			if (buttonGameMenuVisible && Game1.ShowJustTheMinimalButtons)
			{
				buttonGameMenu.bounds.X = scaledViewportWidth - 64 - paddingX;
				buttonGameMenu.bounds.Y = 12;
				buttonGameMenu.draw(b, Color.White, 1E-07f);
				Game1.virtualJoypad.drawJustToggleShowJoypadButton(b);
				drawingJustTheMenuButton = true;
			}
		}

		public void drawMoneyBox(SpriteBatch b, int overrideX = -1, int overrideY = -1, bool oldGFX = false)
		{
			updatePosition();
			if (oldGFX)
			{
				b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? position.X : ((float)overrideX), overrideY - 164) : position) + new Vector2(28 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0), 164 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), new Rectangle(340, 472, 65, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-07f);
				moneyDial.draw(b, ((overrideY != -1) ? new Vector2((overrideX == -1) ? position.X : ((float)overrideX), overrideY - 164) : position) + new Vector2(68 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0), 188 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), Game1.player.Money);
			}
			else
			{
				moneyDial.draw(b, ((overrideY != -1) ? new Vector2((overrideX == -1) ? position.X : ((float)overrideX), overrideY - 164) : position) + new Vector2(164 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0), 72 + ((moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), Game1.player.Money);
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (Game1.displayHUD)
			{
				Game1.virtualJoypad.drawAndUpdateToggleShowJoypadButton(b);
			}
			if (moneyShakeTimer > 0)
			{
				moneyShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (_languageCode != LocalizedContentManager.CurrentLanguageCode)
			{
				_languageCode = LocalizedContentManager.CurrentLanguageCode;
				_amString = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370");
				_pmString = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371");
			}
			if (questPingTimer > 0)
			{
				questPingTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (questPingTimer < 0)
			{
				questPingTimer = 0;
			}
			if (questsDirty)
			{
				if (Game1.player.hasPendingCompletedQuests)
				{
					PingQuestLog();
				}
				questsDirty = false;
			}
		}

		public virtual void PingQuestLog()
		{
			questPingTimer = 6000;
		}

		public virtual void DismissQuestPing()
		{
			questPingTimer = 0;
		}

		public override void draw(SpriteBatch b)
		{
			drawingJustTheMenuButton = false;
			updatePosition();
			if (timeShakeTimer > 0)
			{
				timeShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			if (questPulseTimer > 0)
			{
				questPulseTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			}
			if (whenToPulseTimer >= 0)
			{
				whenToPulseTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
				if (whenToPulseTimer <= 0)
				{
					whenToPulseTimer = 3000;
					if (Game1.player.hasNewQuestActivity())
					{
						questPulseTimer = 1000;
					}
				}
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(Game1.DateTimeScale));
			b.Draw(Game1.mobileSpriteSheet, position, sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-07f);
			buttonF8.bounds = new Rectangle((int)position.X, (int)position.Y, sourceRect.Width * 4, sourceRect.Height * 4);
			string text = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja) ? (Game1.dayOfMonth + "日 (" + Game1.shortDayDisplayNameFromDayOfSeason(Game1.dayOfMonth) + ")") : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? (Game1.shortDayDisplayNameFromDayOfSeason(Game1.dayOfMonth) + " " + Game1.dayOfMonth + "日") : (Game1.shortDayDisplayNameFromDayOfSeason(Game1.dayOfMonth) + ". " + Game1.dayOfMonth)));
			Vector2 vector = Game1.dialogueFont.MeasureString(text);
			switch (LocalizedContentManager.CurrentLanguageCode)
			{
			case LocalizedContentManager.LanguageCode.ru:
			case LocalizedContentManager.LanguageCode.de:
				_datePosition.X = 22f;
				_datePosition.Y = 14f;
				break;
			case LocalizedContentManager.LanguageCode.ja:
				_datePosition.X = 20f;
				_datePosition.Y = 24f;
				break;
			default:
				_datePosition.X = 20f;
				_datePosition.Y = 15f;
				break;
			}
			Utility.drawTextWithShadow(b, text, Game1.dialogueFont, position + _datePosition, Game1.textColor);
			string text2 = ((Game1.timeOfDay % 100 == 0) ? "0" : "");
			string text3 = ((Game1.timeOfDay / 100 % 12 == 0) ? "12" : ((Game1.timeOfDay / 100 % 12).ToString() ?? ""));
			switch (LocalizedContentManager.CurrentLanguageCode)
			{
			case LocalizedContentManager.LanguageCode.en:
			case LocalizedContentManager.LanguageCode.it:
				text3 = ((Game1.timeOfDay / 100 % 12 == 0) ? "12" : ((Game1.timeOfDay / 100 % 12).ToString() ?? ""));
				break;
			case LocalizedContentManager.LanguageCode.ja:
				text3 = ((Game1.timeOfDay / 100 % 12 == 0) ? "0" : ((Game1.timeOfDay / 100 % 12).ToString() ?? ""));
				break;
			case LocalizedContentManager.LanguageCode.zh:
				text3 = ((Game1.timeOfDay / 100 % 24 == 0) ? "00" : ((Game1.timeOfDay / 100 % 12 == 0) ? "12" : ((Game1.timeOfDay / 100 % 12).ToString() ?? "")));
				break;
			case LocalizedContentManager.LanguageCode.ru:
			case LocalizedContentManager.LanguageCode.pt:
			case LocalizedContentManager.LanguageCode.es:
			case LocalizedContentManager.LanguageCode.de:
			case LocalizedContentManager.LanguageCode.th:
			case LocalizedContentManager.LanguageCode.fr:
			case LocalizedContentManager.LanguageCode.ko:
			case LocalizedContentManager.LanguageCode.tr:
			case LocalizedContentManager.LanguageCode.hu:
				text3 = (Game1.timeOfDay / 100 % 24).ToString() ?? "";
				text3 = ((Game1.timeOfDay / 100 % 24 <= 9) ? ("0" + text3) : text3);
				break;
			}
			string text4 = text3 + ":" + Game1.timeOfDay % 100 + text2;
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it)
			{
				text4 = text4 + " " + ((Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") : Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371"));
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
			{
				text4 = ((Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") + " " + text4) : (Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371") + " " + text4));
			}
			else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
			{
				text4 = ((Game1.timeOfDay < 600 || Game1.timeOfDay >= 2400) ? ("凌晨 " + text4) : ((Game1.timeOfDay < 1200) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370") + " " + text4) : ((Game1.timeOfDay < 1300) ? ("中午  " + text4) : ((Game1.timeOfDay < 1900) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371") + " " + text4) : ("晚上  " + text4)))));
			}
			Vector2 vector2 = Game1.dialogueFont.MeasureString(text4);
			bool flag = Game1.shouldTimePass() || Game1.fadeToBlack || Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0 > 1000.0;
			switch (LocalizedContentManager.CurrentLanguageCode)
			{
			case LocalizedContentManager.LanguageCode.ru:
			case LocalizedContentManager.LanguageCode.de:
				_timePosition.X = 198f;
				_timePosition.Y = 14f;
				break;
			case LocalizedContentManager.LanguageCode.ja:
				_timePosition.X = 196f;
				_timePosition.Y = 24f;
				break;
			default:
				_timePosition.X = 196f;
				_timePosition.Y = 15f;
				break;
			}
			Utility.drawTextWithShadow(b, text4, Game1.dialogueFont, position + _timePosition, (Game1.timeOfDay >= 2400) ? Color.Red : (Game1.textColor * (flag ? 1f : 0.5f)));
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(Game1.DateTimeScale));
			int num = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
			if (Game1.player.visibleQuestCount > 0 && !ShowingTutorial)
			{
				questButton.draw(b, Color.White, 1E-07f);
				if (questPulseTimer > 0)
				{
					float num2 = 1f / (Math.Max(300f, Math.Abs(questPulseTimer % 1000 - 500)) / 500f);
					b.Draw(Game1.mobileSpriteSheet, new Vector2(questButton.bounds.X + 32, questButton.bounds.Y + 32) + ((num2 > 1f) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(6, 47, 4, 10), Color.White, 0f, new Vector2(2f, 5f), 4f * num2, SpriteEffects.None, 1E-07f);
				}
			}
			if (!ShowingTutorial)
			{
				buttonGameMenu.draw(b, Color.White, 1E-07f);
			}
			drawMoneyBox(b);
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		}

		private void updatePosition()
		{
			bool flag = false;
			Point center = Game1.player.GetBoundingBox().Center;
			Vector2 vector = Game1.GlobalToLocal(globalPosition: new Vector2(center.X, center.Y), viewport: Game1.viewport);
			if (Game1.options.pinToolbarToggle)
			{
				flag = false;
			}
			else
			{
				flag = vector.Y > (float)(Game1.uiViewport.Height / 2 + 64);
			}
			paddingX = Math.Max(12, Game1.xEdge);
			SetPositionTopRight();
			questButton.bounds = new Rectangle(scaledViewportWidth - 64 - paddingX, (int)position.Y + sourceRect.Height * 4 + spacing, 64, 64);
			if (drawingJustTheMenuButton)
			{
				buttonGameMenu.bounds = new Rectangle(scaledViewportWidth - 64 - paddingX, 12, 64, 64);
			}
			else
			{
				buttonGameMenu.bounds.X = scaledViewportWidth - 64 - paddingX;
				if (Game1.player.visibleQuestCount > 0)
				{
					buttonGameMenu.bounds.Y = questButton.bounds.Y + questButton.bounds.Height + spacing;
				}
				else
				{
					buttonGameMenu.bounds.Y = questButton.bounds.Y;
				}
			}
			xPositionOnScreen = buttonGameMenu.bounds.X;
			yPositionOnScreen = 0;
			if (_buttonGameMenuDown)
			{
				buttonGameMenu.bounds.X -= 4;
				buttonGameMenu.bounds.Y += 4;
			}
			if (_buttonJournalDown)
			{
				questButton.bounds.X -= 4;
				questButton.bounds.Y += 4;
			}
		}

		private void SetPositionTopRight()
		{
			position = new Vector2((int)((float)scaledViewportWidth - (float)sourceRect.Width * 4f - (float)paddingX), paddingY);
		}

		public bool testToOpenDebugConsole(int x, int y)
		{
			return false;
		}

		public override void leftClickHeld(int x, int y)
		{
			x = (int)((float)x / Game1.DateTimeScale);
			y = (int)((float)y / Game1.DateTimeScale);
			if (_buttonGameMenuDown && !buttonGameMenu.containsPoint(x, y))
			{
				_buttonGameMenuDown = false;
				Toolbar.toolbarPressed = false;
			}
			if (_buttonJournalDown && !questButton.containsPoint(x, y))
			{
				_buttonJournalDown = false;
				Toolbar.toolbarPressed = false;
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			_buttonGameMenuDown = false;
			_buttonJournalDown = false;
		}

		private void OnTapGameMenuButton()
		{
			_buttonGameMenuDown = true;
			Game1.currentLocation.tapToMove.Reset();
			Toolbar.toolbarPressed = true;
			Game1.activeClickableMenu = new GameMenu(0);
		}

		private void OnTapJournalButton()
		{
			_buttonJournalDown = true;
			Game1.currentLocation.tapToMove.Reset();
			Toolbar.toolbarPressed = true;
			Game1.activeClickableMenu = new QuestLog();
		}
	}
}
