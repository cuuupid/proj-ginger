using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Locations;
using StardewValley.Tools;

namespace StardewValley.Menus
{
	public class TutorialManager : IClickableMenu
	{
		private static TutorialManager _instance;

		private List<TutorialItem> tutorials = new List<TutorialItem>();

		public bool showTheTutorials;

		public bool hasPickedUpSomething;

		public bool hasDoneTapAndHold;

		public bool hasWateringCanEverEmptied;

		public bool hasUsedWateringCan;

		public bool hasClosedMenu;

		public bool hasOpenedJournalEntry;

		public bool hasBeenInAShop;

		public bool hasBeenOutside;

		public bool collectionsHasBeenSeen;

		public bool inventoryhasBeenSeen;

		public bool mapHasBeenSeen;

		public bool hasSeenSaleTutorial;

		public bool hasSeenBuyTutorial;

		public bool hasClickedOnSaleTutorial;

		public bool skillsHasBeenSeen;

		public bool socialHasBeenSeen;

		public bool craftingHasBeenSeen;

		public bool hasMadeAttackChoice;

		public int numberOfThingsCleared;

		public int numberOfTilesHoed;

		public int numberOfSeedsSown;

		public List<TutorialShopLocation> shopLocationsVisited = new List<TutorialShopLocation>();

		public TutorialItem currentTutorial;

		public static bool tutorialsInitialized;

		public List<tutorialType> completedTutorials;

		public static bool menuUp;

		private bool pastIntro;

		public bool showAttackDialog;

		public DialogueBox attackDialog;

		private bool _gamePadHasBeenUsed;

		private bool _hasSelectedHoe;

		private bool _hasMovedYet;

		public static TutorialManager Instance => _instance;

		public bool gamePadHasBeenUsed
		{
			get
			{
				return _gamePadHasBeenUsed;
			}
			set
			{
				if (!_gamePadHasBeenUsed && value)
				{
					_gamePadHasBeenUsed = true;
					completeAllTutorials();
					showTutorials(toShow: false);
				}
				else
				{
					_gamePadHasBeenUsed = value;
				}
			}
		}

		public bool ShowingDialogueBox
		{
			get
			{
				if (attackDialog != null)
				{
					return true;
				}
				if (currentTutorial != null && currentTutorial.dialog != null)
				{
					return true;
				}
				return false;
			}
		}

		static TutorialManager()
		{
			_instance = new TutorialManager();
			tutorialsInitialized = false;
			menuUp = false;
		}

		private TutorialManager()
		{
		}

		public void resetBools()
		{
			_gamePadHasBeenUsed = false;
			showTheTutorials = false;
			hasPickedUpSomething = false;
			tutorialsInitialized = false;
			pastIntro = false;
			hasDoneTapAndHold = false;
			hasWateringCanEverEmptied = false;
			hasUsedWateringCan = false;
			hasClosedMenu = false;
			hasOpenedJournalEntry = false;
			hasBeenInAShop = false;
			hasBeenOutside = false;
			collectionsHasBeenSeen = false;
			inventoryhasBeenSeen = false;
			mapHasBeenSeen = false;
			hasSeenSaleTutorial = false;
			hasSeenBuyTutorial = false;
			hasClickedOnSaleTutorial = false;
			skillsHasBeenSeen = false;
			socialHasBeenSeen = false;
			craftingHasBeenSeen = false;
			hasMadeAttackChoice = false;
			numberOfThingsCleared = 0;
			numberOfTilesHoed = 0;
			numberOfSeedsSown = 0;
			attackDialog = null;
		}

		public void stopTutorialsTemporarily()
		{
			if (currentTutorial != null)
			{
				currentTutorial.unShow();
				currentTutorial = null;
			}
		}

		public void showTutorials(bool toShow)
		{
			if (Game1.skipTutorials)
			{
				showTheTutorials = false;
			}
			else
			{
				showTheTutorials = toShow;
			}
		}

		public bool isInDialogBounds(int x, int y)
		{
			if (currentTutorial != null)
			{
				if (menuUp)
				{
					return currentTutorial.isInDialogBounds(x, y);
				}
				return false;
			}
			return false;
		}

		public void addTutorial(TutorialItem tut)
		{
			tutorials.Add(tut);
		}

		public bool hasTutorialBeenShown(tutorialType tut)
		{
			if (!showTheTutorials)
			{
				return false;
			}
			foreach (TutorialItem tutorial in tutorials)
			{
				if (tutorial.tType == tut)
				{
					return tutorial.hasBeenShown();
				}
			}
			return false;
		}

		public void loadCompletedTutorials(List<tutorialType> tuts)
		{
			initializeStartTutorials();
			initializeTutorials();
			completedTutorials = tuts;
			if (tuts != null && tutorials != null)
			{
				foreach (TutorialItem tutorial in tutorials)
				{
					if (tuts.Contains(tutorial.tType))
					{
						tutorial.setComplete();
					}
				}
			}
			if (Game1.dayOfMonth >= 3 || Game1.currentSeason != "spring" || Game1.year > 1)
			{
				completeAllBasicTutorials();
			}
			if (Game1.isGamePadConnected())
			{
				gamePadHasBeenUsed = true;
			}
			else
			{
				_gamePadHasBeenUsed = false;
			}
		}

		public bool isTutorialComplete(tutorialType tut)
		{
			if (!showTheTutorials)
			{
				return false;
			}
			foreach (TutorialItem tutorial in tutorials)
			{
				if (tutorial.tType == tut)
				{
					return tutorial.isComplete();
				}
			}
			return false;
		}

		public void completeAllTutorials()
		{
			if (!showTheTutorials)
			{
				return;
			}
			completedTutorials.Clear();
			foreach (TutorialItem tutorial in tutorials)
			{
				tutorial.setComplete();
				completedTutorials.Add(tutorial.tType);
			}
			currentTutorial = null;
		}

		public void completeAllBasicTutorials()
		{
			for (tutorialType tutorialType = tutorialType.TUTORIAL_NONE; tutorialType < tutorialType.DUMMY_START_ATTACK; tutorialType++)
			{
				foreach (TutorialItem tutorial in tutorials)
				{
					if (tutorial.tType == tutorialType)
					{
						tutorial.setComplete();
						completedTutorials.Add(tutorial.tType);
					}
				}
			}
		}

		public bool completeTutorial(tutorialType tut)
		{
			if (!showTheTutorials)
			{
				return false;
			}
			foreach (TutorialItem tutorial in tutorials)
			{
				if (tutorial.tType == tut && (tutorial.location == "" || (Game1.currentLocation != null && tutorial.location == Game1.currentLocation.name && Game1.activeClickableMenu == null) || (tutorial.menuType != null && Game1.activeClickableMenu != null && tutorial.menuType == Game1.activeClickableMenu.getMenuType()) || tutorial.tType == tutorialType.TAP_LEAVE_HOUSE))
				{
					if (currentTutorial != null && tutorial.tType == currentTutorial.tType)
					{
						currentTutorial = null;
					}
					tutorial.setComplete();
					if (!completedTutorials.Contains(tutorial.tType))
					{
						completedTutorials.Add(tutorial.tType);
					}
					if (tutorial.tType == tutorialType.DUMMY_FINISH_CUSTOMIZE)
					{
						initializeTutorials();
					}
					return true;
				}
			}
			return false;
		}

		public bool dontAllowExit()
		{
			if (currentTutorial != null)
			{
				return currentTutorial.dontAllowExit();
			}
			return false;
		}

		public bool checkPrerequisites(TutorialItem t)
		{
			if (!showTheTutorials)
			{
				return false;
			}
			foreach (tutorialType prerequisite in t.getPrerequisites())
			{
				foreach (TutorialItem tutorial in tutorials)
				{
					if (tutorial.tType == prerequisite && !tutorial.isComplete())
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool checkIgnores(TutorialItem t)
		{
			if (!showTheTutorials)
			{
				return false;
			}
			foreach (tutorialType item in t.getIgnoreIfThere())
			{
				foreach (TutorialItem tutorial in tutorials)
				{
					if (tutorial.tType == item && tutorial.isComplete())
					{
						return false;
					}
				}
			}
			return true;
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			if (!shouldShowAttackDialog() || attackDialog == null)
			{
				return;
			}
			Toolbar.toolbarPressed = false;
			int selectedResponse = attackDialog.getSelectedResponse();
			if (selectedResponse == -1)
			{
				return;
			}
			showAttackDialog = false;
			if (selectedResponse == 0)
			{
				if (Game1.options.weaponControl != 0)
				{
					Game1.options.weaponControl = 0;
					OptionsPage.SaveStartupPreferences();
				}
			}
			else if (Game1.options.weaponControl != 6)
			{
				Game1.options.weaponControl = 6;
				Game1.virtualJoypad.OnClickSetToDefaults();
				OptionsPage.SaveStartupPreferences();
			}
			showAttackDialog = false;
			attackDialog.releaseLeftClick(x, y);
			attackDialog = null;
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:tutorial_auto_attack"));
		}

		public override void leftClickHeld(int x, int y)
		{
			if (shouldShowAttackDialog() && attackDialog != null && !Game1.dialogueUp && !Game1.eventUp)
			{
				attackDialog.leftClickHeld(x, y);
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (shouldShowAttackDialog() && attackDialog != null && !Game1.dialogueUp && !Game1.eventUp)
			{
				attackDialog.receiveLeftClick(x, y, playSound);
			}
			else if (currentTutorial != null)
			{
				currentTutorial.receiveLeftClick(x, y, playSound);
			}
		}

		public override void update(GameTime time)
		{
			if (shouldShowAttackDialog() && attackDialog != null && !Game1.dialogueUp && !Game1.eventUp)
			{
				attackDialog.update(time);
			}
			if (!showTheTutorials)
			{
				return;
			}
			if (Game1.isGamePadConnected())
			{
				completeAllTutorials();
				showTheTutorials = false;
				return;
			}
			try
			{
				if (Game1.currentLocation == null && Game1.activeClickableMenu == null)
				{
					return;
				}
				string text = "";
				if (Game1.currentLocation != null)
				{
					try
					{
						text = Game1.currentLocation.name;
					}
					catch (Exception ex)
					{
						Log.It("TutorialManager.update ERROR " + ex.ToString());
						text = "";
					}
				}
				if (currentTutorial != null && !currentTutorial.willTimeout() && ((currentTutorial.location != "" && text != currentTutorial.location) || (currentTutorial.location != "" && Game1.activeClickableMenu != null && Game1.activeClickableMenu.getMenuType() != typeof(DialogueBox)) || (currentTutorial.menuType != null && (Game1.activeClickableMenu == null || (currentTutorial.menuType != Game1.activeClickableMenu.getMenuType() && TitleMenu.subMenu != null && currentTutorial.menuType != TitleMenu.subMenu.GetType())))))
				{
					currentTutorial.unShow();
					completeTutorial(currentTutorial.tType);
					currentTutorial = null;
				}
				foreach (TutorialItem tutorial in tutorials)
				{
					if (((!Game1.game1.IsSaving && (tutorial.location == "" || tutorial.location == text) && (Game1.activeClickableMenu == null || Game1.activeClickableMenu.getMenuType() == typeof(DialogueBox))) || (tutorial.menuType != null && Game1.activeClickableMenu != null && (tutorial.menuType == Game1.activeClickableMenu.getMenuType() || (TitleMenu.subMenu != null && tutorial.menuType == TitleMenu.subMenu.GetType())))) && !tutorial.isShowing() && !tutorial.isComplete() && (tutorial.hasPointer || tutorial.text != "") && checkPrerequisites(tutorial) && checkIgnores(tutorial))
					{
						currentTutorial = tutorial;
						currentTutorial.show();
					}
				}
				if (currentTutorial != null)
				{
					currentTutorial.update(time);
				}
			}
			catch (Exception)
			{
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (showTheTutorials && currentTutorial != null && !currentTutorial.isComplete())
			{
				currentTutorial.draw(b);
			}
		}

		public void drawUI(SpriteBatch b)
		{
			if (shouldShowAttackDialog() && attackDialog != null && !Game1.dialogueUp && !Game1.eventUp)
			{
				attackDialog.draw(b);
			}
			else if (showTheTutorials && currentTutorial != null && !currentTutorial.isComplete())
			{
				currentTutorial.drawDialogueBox(b);
				currentTutorial.drawHandForUI(b);
			}
		}

		public void drawButtonHands(SpriteBatch b)
		{
			if (showTheTutorials && currentTutorial != null && !currentTutorial.isComplete())
			{
				currentTutorial.drawButtonHands(b);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void initializeStartTutorials()
		{
			tutorials = new List<TutorialItem>();
			resetBools();
			completedTutorials = new List<tutorialType>();
			currentTutorial = null;
			if (!tutorialsInitialized)
			{
				tutorialsInitialized = true;
				currentTutorial = null;
				float num = (float)Game1.viewport.Width / 1280f;
				float num2 = (float)Game1.viewport.Height / 720f;
				int num3 = Math.Max(12, Game1.toolbarPaddingX);
				int num4 = Game1.viewport.Width - 64 - num3 + 32;
				TutorialItem item = new TutorialItem("", tutorialType.CUSTOMIZE, hasPointer: false, typeof(CharacterCustomization), -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial1"), timeOut: true);
				tutorials.Add(item);
				item = new TutorialItem("", tutorialType.DUMMY_FINISH_CUSTOMIZE, hasPointer: false, null, -1, -1, -1, -1, -1, "", timeOut: true);
				item.addPrerequisite(tutorialType.CUSTOMIZE);
				tutorials.Add(item);
				if (Game1.isGamePadConnected())
				{
					gamePadHasBeenUsed = true;
				}
				else
				{
					_gamePadHasBeenUsed = false;
				}
			}
		}

		public void initializeTutorials()
		{
			float num = (float)Game1.viewport.Width / 1280f;
			float num2 = (float)Game1.viewport.Height / 720f;
			int num3 = Math.Max(12, Game1.toolbarPaddingX);
			int num4 = Game1.viewport.Width - 64 - num3 + 32;
			currentTutorial = null;
			TutorialItem item = new TutorialItem("FarmHouse", tutorialType.GOOD_MORNING, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial2"), timeOut: true);
			tutorials.Add(item);
			int targetX;
			int targetY;
			int targetX2;
			int targetY2;
			int targetX3;
			int targetY3;
			switch (Game1.whichFarm)
			{
			case 0:
				targetX = 3;
				targetY = 7;
				targetX2 = 61;
				targetY2 = 18;
				targetX3 = 3;
				targetY3 = 11;
				break;
			case 1:
				targetX = 4;
				targetY = 7;
				targetX2 = 67;
				targetY2 = 19;
				targetX3 = 3;
				targetY3 = 10;
				break;
			case 2:
				targetX = 4;
				targetY = 7;
				targetX2 = 61;
				targetY2 = 18;
				targetX3 = 3;
				targetY3 = 10;
				break;
			case 3:
				targetX = 2;
				targetY = 9;
				targetX2 = 56;
				targetY2 = 19;
				targetX3 = 3;
				targetY3 = 10;
				break;
			default:
				targetX = 4;
				targetY = 7;
				targetX2 = 58;
				targetY2 = 18;
				targetX3 = 3;
				targetY3 = 10;
				break;
			}
			item = new TutorialItem("FarmHouse", tutorialType.GET_PARSNIPS, hasPointer: true, null, 1, targetX, targetY, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial3"));
			item.addPrerequisite(tutorialType.GOOD_MORNING);
			tutorials.Add(item);
			item = new TutorialItem("FarmHouse", tutorialType.TAP_JOURNAL, hasPointer: true, null, 0, 0, 0, -1, -1, "", timeOut: false, Game1.dayTimeMoneyBox.questButton);
			item.addPrerequisite(tutorialType.GET_PARSNIPS);
			item.addDelay(4000f);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.JOURNAL_INFO, hasPointer: false, typeof(QuestLog), -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial4"));
			item.addPrerequisite(tutorialType.TAP_JOURNAL);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.CLOSE_JOURNAL, hasPointer: true, typeof(QuestLog), 0, 0, 0, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial5"), timeOut: false, null, "closeButton");
			item.addDelay(2000f);
			item.addPrerequisite(tutorialType.JOURNAL_INFO);
			tutorials.Add(item);
			item = new TutorialItem("FarmHouse", tutorialType.TAP_LEAVE_HOUSE, hasPointer: true, null, 1, targetX3, targetY3, -1, -1, "");
			item.addPrerequisite(tutorialType.CLOSE_JOURNAL);
			tutorials.Add(item);
			item = new TutorialItem("Farm", tutorialType.SEE_FARM, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial6"), timeOut: true);
			item.addPrerequisite(tutorialType.GOOD_MORNING);
			tutorials.Add(item);
			item = new TutorialItem("Farm", tutorialType.TAP_FARM, hasPointer: true, null, 1, targetX2, targetY2, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial7"));
			item.addPrerequisite(tutorialType.SEE_FARM);
			tutorials.Add(item);
			item = new TutorialItem("Farm", tutorialType.TAP_FARM2, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial8"));
			item.addPrerequisite(tutorialType.TAP_FARM);
			tutorials.Add(item);
			item = new TutorialItem("Farm", tutorialType.SELECT_TOOL, hasPointer: true, null, 0, (int)Game1.toolbar.getIconPosition(1).X, (int)Game1.toolbar.getIconPosition(1).Y, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial9"), timeOut: false, null, "Hoe");
			item.addPrerequisite(tutorialType.TAP_FARM2);
			item.addIgnoreIfThere(tutorialType.USE_HOE);
			tutorials.Add(item);
			item = new TutorialItem("Farm", tutorialType.USE_HOE, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial10"));
			item.addPrerequisite(tutorialType.SELECT_TOOL);
			item.addPrerequisite(tutorialType.TAP_FARM2);
			tutorials.Add(item);
			item = new TutorialItem("Farm", tutorialType.PLANT_SEEDS, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial11"));
			item.addPrerequisite(tutorialType.USE_HOE);
			item.addPrerequisite(tutorialType.GET_PARSNIPS);
			tutorials.Add(item);
			item = new TutorialItem("Farm", tutorialType.GO_BACK_FARMHOUSE, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial26"));
			item.addPrerequisite(tutorialType.USE_HOE);
			item.addIgnoreIfThere(tutorialType.GET_PARSNIPS);
			tutorials.Add(item);
			item = new TutorialItem("Farm", tutorialType.WATER_GROUND, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial12"));
			item.addPrerequisite(tutorialType.PLANT_SEEDS);
			tutorials.Add(item);
			item = new TutorialItem("Farm", tutorialType.WATER_REMINDER, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial13"), timeOut: true);
			item.addPrerequisite(tutorialType.WATER_GROUND);
			tutorials.Add(item);
			item = new TutorialItem("Farm", tutorialType.GO_EXPLORE, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial14"), timeOut: true);
			item.addPrerequisite(tutorialType.WATER_REMINDER);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.DUMMY_REFILL_WATER, hasPointer: false, null, -1, -1, -1, -1, -1, "");
			item.addPrerequisite(tutorialType.WATER_GROUND);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.REFILL_WATER, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial15"));
			item.addPrerequisite(tutorialType.DUMMY_REFILL_WATER);
			tutorials.Add(item);
			item = new TutorialItem("BusStop", tutorialType.TAP_HOLD_MOVE, hasPointer: true, null, 1, 12, 23, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial16"));
			item.addPrerequisite(tutorialType.GOOD_MORNING);
			tutorials.Add(item);
			item = new TutorialItem("BusStop", tutorialType.TAP_HOLD2, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial17"), timeOut: true);
			item.addPrerequisite(tutorialType.TAP_HOLD_MOVE);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.TAP_GAME_MENU, hasPointer: true, null, 0, 0, 0, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial18"), timeOut: false, Game1.dayTimeMoneyBox.buttonGameMenu);
			item.addPrerequisite(tutorialType.TAP_HOLD2);
			item.addDelay(30000f);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.DUMMY_INVENTORY, hasPointer: false, null, -1, -1, -1, -1, -1, "");
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.INVENTORY, hasPointer: false, typeof(InventoryPage), -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial19"), timeOut: true);
			item.addPrerequisite(tutorialType.DUMMY_INVENTORY);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.DUMMY_SKILLS, hasPointer: false, null, -1, -1, -1, -1, -1, "");
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.SKILLS, hasPointer: false, typeof(SkillsPage), -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial20"), timeOut: true);
			item.addPrerequisite(tutorialType.DUMMY_SKILLS);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.DUMMY_SOCIAL, hasPointer: false, null, -1, -1, -1, -1, -1, "");
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.SOCIAL, hasPointer: false, typeof(SocialPage), -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial21"), timeOut: true);
			item.addPrerequisite(tutorialType.DUMMY_SOCIAL);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.DUMMY_CRAFTING, hasPointer: false, null, -1, -1, -1, -1, -1, "");
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.CRAFTING, hasPointer: false, typeof(CraftingPageMobile), -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial22"), timeOut: true);
			item.addPrerequisite(tutorialType.DUMMY_CRAFTING);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.DUMMY_MAP, hasPointer: false, null, -1, -1, -1, -1, -1, "");
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.MAP, hasPointer: false, typeof(MapPage), -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial23"), timeOut: true);
			item.addPrerequisite(tutorialType.DUMMY_MAP);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.DUMMY_COLLECTIONS, hasPointer: false, null, -1, -1, -1, -1, -1, "");
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.COLLECTIONS, hasPointer: false, typeof(CollectionsPage), -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial24"), timeOut: true);
			item.addPrerequisite(tutorialType.DUMMY_COLLECTIONS);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.DUMMY_INTERACT_SHOP, hasPointer: false, null, -1, -1, -1, -1, -1, "");
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.INTERACT_SHOP, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial25"), timeOut: true);
			item.addPrerequisite(tutorialType.DUMMY_INTERACT_SHOP);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.DUMMY_START_ATTACK, hasPointer: false, null, -1, -1, -1, -1, -1, "");
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.START_ATTACK, hasPointer: false, null, -1, -1, -1, -1, -1, Game1.content.LoadString("Strings\\UI:tutorial_auto_attack"), timeOut: true);
			item.addPrerequisite(tutorialType.DUMMY_START_ATTACK);
			tutorials.Add(item);
			item = new TutorialItem("", tutorialType.DUMMY_PAST_INTRO, hasPointer: false);
			tutorials.Add(item);
			foreach (TutorialItem tutorial in tutorials)
			{
				if (tutorial.tType != tutorialType.CUSTOMIZE)
				{
					tutorial.addPrerequisite(tutorialType.DUMMY_PAST_INTRO);
				}
			}
			if (Game1.isGamePadConnected())
			{
				gamePadHasBeenUsed = true;
			}
			else
			{
				_gamePadHasBeenUsed = false;
			}
		}

		public void TestForHoeSelected()
		{
			if (!_hasSelectedHoe && Game1.player.CurrentTool != null && Game1.player.CurrentTool is Hoe && isTutorialComplete(tutorialType.TAP_FARM2) && completeTutorial(tutorialType.SELECT_TOOL))
			{
				_hasSelectedHoe = true;
			}
		}

		public void CheckHasMovedYet()
		{
			if (!_hasMovedYet)
			{
				_hasMovedYet = true;
				completeTutorial(tutorialType.TAP_MOVE);
			}
		}

		public void CheckTapAndHold()
		{
			if (!hasDoneTapAndHold && hasTutorialBeenShown(tutorialType.TAP_HOLD_MOVE) && completeTutorial(tutorialType.TAP_HOLD_MOVE))
			{
				hasDoneTapAndHold = true;
			}
		}

		public void SeenShop(TutorialShopLocation shopLocation)
		{
			if (!shopLocationsVisited.Contains(shopLocation))
			{
				shopLocationsVisited.Add(shopLocation);
			}
		}

		public bool shouldShowAttackDialog()
		{
			if (showAttackDialog)
			{
				if (Game1.activeClickableMenu == null)
				{
					return true;
				}
				if (Game1.activeClickableMenu is ItemGrabMenu)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (!showAttackDialog || attackDialog == null)
			{
				return;
			}
			attackDialog.receiveGamePadButton(b);
			if (b != Buttons.A)
			{
				return;
			}
			Toolbar.toolbarPressed = false;
			int selectedResponse = attackDialog.getSelectedResponse();
			if (selectedResponse == -1)
			{
				return;
			}
			showAttackDialog = false;
			if (selectedResponse == 0)
			{
				if (Game1.options.weaponControl != 0)
				{
					Game1.options.weaponControl = 0;
					OptionsPage.SaveStartupPreferences();
				}
			}
			else if (Game1.options.weaponControl != 6)
			{
				Game1.options.weaponControl = 6;
				Game1.virtualJoypad.OnClickSetToDefaults();
				OptionsPage.SaveStartupPreferences();
			}
			showAttackDialog = false;
		}

		public void triggerAttackChoice()
		{
			if (!hasMadeAttackChoice && !Game1.isGamePadConnected())
			{
				List<Response> list = new List<Response>();
				list.Add(new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")));
				list.Add(new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")));
				attackDialog = new DialogueBox(Game1.content.LoadString("Strings\\UI:question_auto_attack"), list, DialogueBox.GetWidth());
				showAttackDialog = true;
			}
		}

		public void SaleTutorialCheck()
		{
			if (!hasClickedOnSaleTutorial && isTutorialComplete(tutorialType.DUMMY_SELL_SHOP))
			{
				hasClickedOnSaleTutorial = true;
				completeTutorial(tutorialType.SELL_SHOP);
			}
		}

		public void MeleeWeaponCheck()
		{
			if (numberOfThingsCleared < 8)
			{
				if (numberOfThingsCleared < 1)
				{
					completeTutorial(tutorialType.TAP_FARM);
				}
				numberOfThingsCleared++;
				if (numberOfThingsCleared >= 8)
				{
					completeTutorial(tutorialType.TAP_FARM2);
				}
			}
		}

		public void TapLeaveHouseCheck()
		{
			if (!hasBeenOutside && Game1.player != null && Game1.player.currentLocation != null && !(Game1.player.currentLocation is FarmHouse) && isTutorialComplete(tutorialType.GOOD_MORNING))
			{
				hasBeenOutside = true;
				completeTutorial(tutorialType.TAP_LEAVE_HOUSE);
			}
		}

		public void DummyInteractShopCheck()
		{
			try
			{
				if (!hasBeenInAShop && Game1.currentLocation != null && Game1.currentLocation.name != null)
				{
					switch ((string)Game1.currentLocation.name)
					{
					case "SeedShop":
					case "JojaMart":
					case "Saloon":
					case "Blacksmith":
					case "AnimalShop":
					case "FishShop":
						hasBeenInAShop = true;
						completeTutorial(tutorialType.DUMMY_INTERACT_SHOP);
						break;
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
