using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class LevelUpMenu : IClickableMenu
	{
		public const int region_okButton = 101;

		public const int region_leftProfession = 102;

		public const int region_rightProfession = 103;

		public const int basewidth = 768;

		public const int baseheight = 512;

		public bool informationUp;

		public bool isActive;

		public bool isProfessionChooser;

		public bool hasUpdatedProfessions;

		private int currentLevel;

		private int currentSkill;

		private int timerBeforeStart;

		private float scale;

		private Color leftProfessionColor = Game1.textColor;

		private Color rightProfessionColor = Game1.textColor;

		private MouseState oldMouseState;

		public ClickableTextureComponent starIcon;

		public ClickableTextureComponent okButton;

		public ClickableComponent leftProfession;

		public ClickableComponent rightProfession;

		private List<CraftingRecipe> newCraftingRecipes = new List<CraftingRecipe>();

		private List<string> extraInfoForLevel = new List<string>();

		private List<string> leftProfessionDescription = new List<string>();

		private List<string> rightProfessionDescription = new List<string>();

		private Rectangle sourceRectForLevelIcon;

		private string title;

		private List<int> professionsToChoose = new List<int>();

		private List<TemporaryAnimatedSprite> littleStars = new List<TemporaryAnimatedSprite>();

		private bool okButtonHeld;

		private bool dropTitle;

		public LevelUpMenu()
			: base(Game1.uiViewport.Width / 2 - 384, Game1.uiViewport.Height / 2 - 256, 768, 512)
		{
			Game1.player.team.endOfNightStatus.UpdateState("level");
			width = 768;
			height = 512;
			width = 896;
			if (width > Game1.uiViewport.Width - Game1.xEdge * 2)
			{
				width = Game1.uiViewport.Width - Game1.xEdge * 2;
			}
			xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
			height = 960;
			dropTitle = false;
			okButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 2 - 40, yPositionOnScreen + height - 80 - 32, 80, 80), Game1.mobileSpriteSheet, new Rectangle(0, 0, 20, 20), 4f, drawShadow: true);
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public LevelUpMenu(int skill, int level)
			: base(Game1.uiViewport.Width / 2 - 384, Game1.uiViewport.Height / 2 - 256, 768, 512)
		{
			Game1.player.team.endOfNightStatus.UpdateState("level");
			initialize(Game1.uiViewport.Width / 2 - 384, Game1.uiViewport.Height / 2 - 256, 768, 512);
			timerBeforeStart = 250;
			isActive = true;
			width = 960;
			height = 512;
			width = 896;
			if (width > Game1.uiViewport.Width - Game1.xEdge * 2)
			{
				width = Game1.uiViewport.Width - Game1.xEdge * 2;
			}
			xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
			okButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 2 - 40, yPositionOnScreen + 512 - 80 - 16, 80, 80), Game1.mobileSpriteSheet, new Rectangle(0, 0, 20, 20), 4f, drawShadow: true);
			newCraftingRecipes.Clear();
			extraInfoForLevel.Clear();
			Game1.player.completelyStopAnimatingOrDoingAction();
			informationUp = true;
			isProfessionChooser = false;
			currentLevel = level;
			currentSkill = skill;
			if (level == 10)
			{
				Game1.getSteamAchievement("Achievement_SingularTalent");
				if ((int)Game1.player.farmingLevel == 10 && (int)Game1.player.miningLevel == 10 && (int)Game1.player.fishingLevel == 10 && (int)Game1.player.foragingLevel == 10 && (int)Game1.player.combatLevel == 10)
				{
					Game1.getSteamAchievement("Achievement_MasterOfTheFiveWays");
				}
				if (skill == 0)
				{
					Game1.addMailForTomorrow("marnieAutoGrabber");
				}
			}
			title = Game1.content.LoadString("Strings\\UI:LevelUp_Title", currentLevel, Farmer.getSkillDisplayNameFromIndex(currentSkill));
			extraInfoForLevel = getExtraInfoForLevel(currentSkill, currentLevel);
			switch (currentSkill)
			{
			case 0:
				sourceRectForLevelIcon = new Rectangle(0, 0, 16, 16);
				break;
			case 1:
				sourceRectForLevelIcon = new Rectangle(16, 0, 16, 16);
				break;
			case 3:
				sourceRectForLevelIcon = new Rectangle(32, 0, 16, 16);
				break;
			case 2:
				sourceRectForLevelIcon = new Rectangle(80, 0, 16, 16);
				break;
			case 4:
				sourceRectForLevelIcon = new Rectangle(128, 16, 16, 16);
				break;
			case 5:
				sourceRectForLevelIcon = new Rectangle(64, 0, 16, 16);
				break;
			}
			if ((currentLevel == 5 || currentLevel == 10) && currentSkill != 5)
			{
				professionsToChoose.Clear();
				isProfessionChooser = true;
				leftProfession = new ClickableComponent(new Rectangle(xPositionOnScreen, yPositionOnScreen + 128, width / 2, height), "")
				{
					myID = 102,
					rightNeighborID = 103
				};
				rightProfession = new ClickableComponent(new Rectangle(width / 2 + xPositionOnScreen, yPositionOnScreen + 128, width / 2, height), "")
				{
					myID = 103,
					leftNeighborID = 102
				};
				if (Game1.clientBounds.Height < 720)
				{
					dropTitle = true;
				}
			}
			int num = 0;
			foreach (KeyValuePair<string, string> craftingRecipe in CraftingRecipe.craftingRecipes)
			{
				string text = craftingRecipe.Value.Split('/')[4];
				if (text.Contains(Farmer.getSkillNameFromIndex(currentSkill)) && text.Contains(currentLevel.ToString() ?? ""))
				{
					newCraftingRecipes.Add(new CraftingRecipe(craftingRecipe.Key, isCookingRecipe: false));
					if (!Game1.player.craftingRecipes.ContainsKey(craftingRecipe.Key))
					{
						Game1.player.craftingRecipes.Add(craftingRecipe.Key, 0);
					}
					num += 32;
				}
			}
			foreach (KeyValuePair<string, string> cookingRecipe in CraftingRecipe.cookingRecipes)
			{
				string text2 = cookingRecipe.Value.Split('/')[3];
				if (!text2.Contains(Farmer.getSkillNameFromIndex(currentSkill)) || !text2.Contains(currentLevel.ToString() ?? ""))
				{
					continue;
				}
				newCraftingRecipes.Add(new CraftingRecipe(cookingRecipe.Key, isCookingRecipe: true));
				if (!Game1.player.cookingRecipes.ContainsKey(cookingRecipe.Key))
				{
					Game1.player.cookingRecipes.Add(cookingRecipe.Key, 0);
					if (!Game1.player.hasOrWillReceiveMail("robinKitchenLetter"))
					{
						Game1.mailbox.Add("robinKitchenLetter");
					}
				}
				num = ((Game1.uiViewport.Height <= 700) ? (num + 32) : (num + (newCraftingRecipes.Last().bigCraftable ? 128 : 64)));
			}
			height = num + 256 + extraInfoForLevel.Count * 64 * 3 / 4 + 16 + 80;
			if (isProfessionChooser)
			{
				height = Math.Max(512, height);
				leftProfession.bounds.X = xPositionOnScreen + 32;
				leftProfession.bounds.Width = (rightProfession.bounds.Width = Game1.uiViewport.Width / 2 - leftProfession.bounds.X - 8);
				leftProfession.bounds.Y = (rightProfession.bounds.Y = yPositionOnScreen + 192 + 40);
				leftProfession.bounds.Height = (rightProfession.bounds.Height = height - 192 - 72);
				rightProfession.bounds.X = Game1.uiViewport.Width / 2 + 8;
			}
			Game1.player.freezePause = 100;
			updatePositions();
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			if (isProfessionChooser)
			{
				currentlySnappedComponent = getComponentWithID(103);
				Game1.setMousePosition(xPositionOnScreen + width + 64, yPositionOnScreen + height + 64);
			}
			else
			{
				currentlySnappedComponent = getComponentWithID(101);
				snapCursorToCurrentSnappedComponent();
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
		}

		private void SetOKButtonBounds()
		{
			okButton.bounds = new Rectangle(xPositionOnScreen + width / 2 - 40, dropTitle ? (height - 80 - 16) : (yPositionOnScreen + height - 80 - 48), 80, 80);
			okButton.drawShadow = !okButtonHeld;
			if (okButtonHeld)
			{
				okButton.bounds.X += 4;
				okButton.bounds.Y += 4;
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			if (currentLevel != 5 && currentLevel != 10 && okButton.containsPoint(x, y) && okButtonHeld && isActive && informationUp)
			{
				okButtonClicked();
			}
			okButtonHeld = false;
			SetOKButtonBounds();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (okButton.containsPoint(x, y))
			{
				okButtonHeld = true;
				Game1.playSound("smallSelect");
			}
			if (leftProfession != null && leftProfession.containsPoint(x, y))
			{
				leftProfessionColor = Color.Green;
				Game1.player.professions.Add(professionsToChoose[0]);
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary["name"] = getProfessionName(professionsToChoose[0]);
				getImmediateProfessionPerk(professionsToChoose[0]);
				RemoveLevelFromLevelList();
				isActive = false;
				informationUp = false;
				isProfessionChooser = false;
			}
			else if (rightProfession != null && rightProfession.containsPoint(x, y))
			{
				rightProfessionColor = Color.Green;
				Game1.player.professions.Add(professionsToChoose[1]);
				getImmediateProfessionPerk(professionsToChoose[1]);
				RemoveLevelFromLevelList();
				isActive = false;
				informationUp = false;
				isProfessionChooser = false;
			}
			if (leftProfession == null && rightProfession == null)
			{
				okButtonClicked();
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			if (!okButton.containsPoint(x, y) && okButtonHeld)
			{
				okButtonHeld = false;
			}
			SetOKButtonBounds();
		}

		public List<string> getExtraInfoForLevel(int whichSkill, int whichLevel)
		{
			List<string> list = new List<string>();
			switch (whichSkill)
			{
			case 0:
				list.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Farming1"));
				list.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Farming2"));
				break;
			case 3:
				list.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Mining"));
				break;
			case 1:
				list.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Fishing"));
				break;
			case 2:
				list.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging1"));
				if (whichLevel == 1)
				{
					list.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging2"));
				}
				if (whichLevel == 4 || whichLevel == 8)
				{
					list.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Foraging3"));
				}
				break;
			case 4:
				list.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Combat"));
				break;
			case 5:
				list.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ExtraInfo_Luck"));
				break;
			}
			return list;
		}

		private static void addProfessionDescriptions(List<string> descriptions, string professionName)
		{
			descriptions.Add(Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionName_" + professionName));
			descriptions.AddRange(Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionDescription_" + professionName).Split('\n'));
		}

		private static string getProfessionName(int whichProfession)
		{
			return whichProfession switch
			{
				0 => "Rancher", 
				1 => "Tiller", 
				2 => "Coopmaster", 
				3 => "Shepherd", 
				4 => "Artisan", 
				5 => "Agriculturist", 
				6 => "Fisher", 
				7 => "Trapper", 
				8 => "Angler", 
				9 => "Pirate", 
				10 => "Mariner", 
				11 => "Luremaster", 
				12 => "Forester", 
				13 => "Gatherer", 
				14 => "Lumberjack", 
				15 => "Tapper", 
				16 => "Botanist", 
				17 => "Tracker", 
				18 => "Miner", 
				19 => "Geologist", 
				20 => "Blacksmith", 
				21 => "Prospector", 
				22 => "Excavator", 
				23 => "Gemologist", 
				24 => "Fighter", 
				25 => "Scout", 
				26 => "Brute", 
				27 => "Defender", 
				28 => "Acrobat", 
				_ => "Desperado", 
			};
		}

		public static List<string> getProfessionDescription(int whichProfession)
		{
			List<string> list = new List<string>();
			addProfessionDescriptions(list, getProfessionName(whichProfession));
			return list;
		}

		public static string getProfessionTitleFromNumber(int whichProfession)
		{
			return Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionName_" + getProfessionName(whichProfession));
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (b == Buttons.A && !isProfessionChooser && isActive)
			{
				okButtonClicked();
			}
			else
			{
				if (!isProfessionChooser || !isActive)
				{
					return;
				}
				if (b == Buttons.DPadLeft || (b == Buttons.LeftThumbstickLeft && leftProfession != null))
				{
					receiveLeftClick(leftProfession.bounds.X, leftProfession.bounds.Y);
				}
				switch (b)
				{
				case Buttons.RightThumbstickRight:
					if (rightProfession == null)
					{
						break;
					}
					goto case Buttons.DPadRight;
				case Buttons.DPadRight:
					receiveLeftClick(rightProfession.bounds.X, rightProfession.bounds.Y);
					break;
				}
			}
		}

		public static void AddMissedProfessionChoices(Farmer farmer)
		{
			int[] array = new int[5] { 0, 1, 2, 3, 4 };
			foreach (int num in array)
			{
				if (farmer.GetUnmodifiedSkillLevel(num) >= 5 && !farmer.newLevels.Contains(new Point(num, 5)) && farmer.getProfessionForSkill(num, 5) == -1)
				{
					farmer.newLevels.Add(new Point(num, 5));
				}
				if (farmer.GetUnmodifiedSkillLevel(num) >= 10 && !farmer.newLevels.Contains(new Point(num, 10)) && farmer.getProfessionForSkill(num, 10) == -1)
				{
					farmer.newLevels.Add(new Point(num, 10));
				}
			}
		}

		public static void AddMissedLevelRecipes(Farmer farmer)
		{
			int[] array = new int[5] { 0, 1, 2, 3, 4 };
			foreach (int num in array)
			{
				for (int j = 0; j <= farmer.GetUnmodifiedSkillLevel(num); j++)
				{
					if (farmer.newLevels.Contains(new Point(num, j)))
					{
						continue;
					}
					foreach (KeyValuePair<string, string> craftingRecipe in CraftingRecipe.craftingRecipes)
					{
						string text = craftingRecipe.Value.Split('/')[4];
						if (text.Contains(Farmer.getSkillNameFromIndex(num)) && text.Contains(j.ToString() ?? "") && !farmer.craftingRecipes.ContainsKey(craftingRecipe.Key))
						{
							Console.WriteLine(farmer.Name + " was missing recipe " + craftingRecipe.Key + " from skill level up.");
							farmer.craftingRecipes.Add(craftingRecipe.Key, 0);
						}
					}
					foreach (KeyValuePair<string, string> cookingRecipe in CraftingRecipe.cookingRecipes)
					{
						string text2 = cookingRecipe.Value.Split('/')[3];
						if (text2.Contains(Farmer.getSkillNameFromIndex(num)) && text2.Contains(j.ToString() ?? "") && !farmer.cookingRecipes.ContainsKey(cookingRecipe.Key))
						{
							Console.WriteLine(farmer.Name + " was missing recipe " + cookingRecipe.Key + " from skill level up.");
							farmer.cookingRecipes.Add(cookingRecipe.Key, 0);
						}
					}
				}
			}
		}

		public static void removeImmediateProfessionPerk(int whichProfession)
		{
			switch (whichProfession)
			{
			case 24:
				Game1.player.maxHealth -= 15;
				break;
			case 27:
				Game1.player.maxHealth -= 25;
				break;
			}
			if (Game1.player.health > Game1.player.maxHealth)
			{
				Game1.player.health = Game1.player.maxHealth;
			}
		}

		public void getImmediateProfessionPerk(int whichProfession)
		{
			switch (whichProfession)
			{
			case 24:
				Game1.player.maxHealth += 15;
				break;
			case 27:
				Game1.player.maxHealth += 25;
				break;
			}
		}

		public static void RevalidateHealth(Farmer farmer)
		{
			int maxHealth = farmer.maxHealth;
			int num = 100;
			if (farmer.mailReceived.Contains("qiCave"))
			{
				num += 25;
			}
			for (int i = 1; i <= farmer.GetUnmodifiedSkillLevel(4); i++)
			{
				if (!farmer.newLevels.Contains(new Point(4, i)) && i != 5 && i != 10)
				{
					num += 5;
				}
			}
			if (farmer.professions.Contains(24))
			{
				num += 15;
			}
			if (farmer.professions.Contains(27))
			{
				num += 25;
			}
			if (farmer.maxHealth < num)
			{
				Console.WriteLine("Fixing max health of: " + farmer.Name + " was " + farmer.maxHealth + " (expected: " + num + ")");
				int num2 = num - farmer.maxHealth;
				farmer.maxHealth = num;
				farmer.health += num2;
			}
		}

		public override void update(GameTime time)
		{
			if (!isActive)
			{
				exitThisMenu();
				return;
			}
			if (isProfessionChooser && !hasUpdatedProfessions)
			{
				if (currentLevel == 5)
				{
					professionsToChoose.Add(currentSkill * 6);
					professionsToChoose.Add(currentSkill * 6 + 1);
				}
				else if (Game1.player.professions.Contains(currentSkill * 6))
				{
					professionsToChoose.Add(currentSkill * 6 + 2);
					professionsToChoose.Add(currentSkill * 6 + 3);
				}
				else
				{
					professionsToChoose.Add(currentSkill * 6 + 4);
					professionsToChoose.Add(currentSkill * 6 + 5);
				}
				leftProfessionDescription = getProfessionDescription(professionsToChoose[0]);
				rightProfessionDescription = getProfessionDescription(professionsToChoose[1]);
				hasUpdatedProfessions = true;
			}
			for (int num = littleStars.Count - 1; num >= 0; num--)
			{
				if (littleStars[num].update(time))
				{
					littleStars.RemoveAt(num);
				}
			}
			if (Game1.random.NextDouble() < 0.03)
			{
				Vector2 position = new Vector2(0f, Game1.random.Next(yPositionOnScreen - 128, yPositionOnScreen - 4) / 20 * 4 * 5 + 32);
				if (Game1.random.NextDouble() < 0.5)
				{
					position.X = Game1.random.Next(xPositionOnScreen + width / 2 - 228, xPositionOnScreen + width / 2 - 132);
				}
				else
				{
					position.X = Game1.random.Next(xPositionOnScreen + width / 2 + 116, xPositionOnScreen + width - 160);
				}
				if (position.Y < (float)(yPositionOnScreen - 64 - 8))
				{
					position.X = Game1.random.Next(xPositionOnScreen + width / 2 - 116, xPositionOnScreen + width / 2 + 116);
				}
				position.X = position.X / 20f * 4f * 5f;
				littleStars.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(364, 79, 5, 5), 80f, 7, 1, position, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					local = true
				});
			}
			if (timerBeforeStart > 0)
			{
				timerBeforeStart -= time.ElapsedGameTime.Milliseconds;
				return;
			}
			oldMouseState = Game1.input.GetMouseState();
			if (isActive && !informationUp && starIcon != null)
			{
				if (starIcon.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
				{
					starIcon.sourceRect.X = 294;
				}
				else
				{
					starIcon.sourceRect.X = 310;
				}
			}
			if (isActive && starIcon != null && !informationUp && (oldMouseState.LeftButton == ButtonState.Pressed || (Game1.options.gamepadControls && Game1.oldPadState.IsButtonDown(Buttons.A))) && starIcon.containsPoint(oldMouseState.X, oldMouseState.Y))
			{
				newCraftingRecipes.Clear();
				extraInfoForLevel.Clear();
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.playSound("bigSelect");
				informationUp = true;
				isProfessionChooser = false;
				RemoveLevelFromLevelList();
				currentLevel = Game1.player.newLevels.First().Y;
				currentSkill = Game1.player.newLevels.First().X;
				title = Game1.content.LoadString("Strings\\UI:LevelUp_Title", currentLevel, Farmer.getSkillDisplayNameFromIndex(currentSkill));
				extraInfoForLevel = getExtraInfoForLevel(currentSkill, currentLevel);
				switch (currentSkill)
				{
				case 0:
					sourceRectForLevelIcon = new Rectangle(0, 0, 16, 16);
					break;
				case 1:
					sourceRectForLevelIcon = new Rectangle(16, 0, 16, 16);
					break;
				case 3:
					sourceRectForLevelIcon = new Rectangle(32, 0, 16, 16);
					break;
				case 2:
					sourceRectForLevelIcon = new Rectangle(80, 0, 16, 16);
					break;
				case 4:
					sourceRectForLevelIcon = new Rectangle(128, 16, 16, 16);
					break;
				case 5:
					sourceRectForLevelIcon = new Rectangle(64, 0, 16, 16);
					break;
				}
				if ((currentLevel == 5 || currentLevel == 10) && currentSkill != 5)
				{
					professionsToChoose.Clear();
					isProfessionChooser = true;
					if (currentLevel == 5)
					{
						professionsToChoose.Add(currentSkill * 6);
						professionsToChoose.Add(currentSkill * 6 + 1);
					}
					else if (Game1.player.professions.Contains(currentSkill * 6))
					{
						professionsToChoose.Add(currentSkill * 6 + 2);
						professionsToChoose.Add(currentSkill * 6 + 3);
					}
					else
					{
						professionsToChoose.Add(currentSkill * 6 + 4);
						professionsToChoose.Add(currentSkill * 6 + 5);
					}
					leftProfessionDescription = getProfessionDescription(professionsToChoose[0]);
					rightProfessionDescription = getProfessionDescription(professionsToChoose[1]);
				}
				int num2 = 0;
				foreach (KeyValuePair<string, string> craftingRecipe in CraftingRecipe.craftingRecipes)
				{
					string text = craftingRecipe.Value.Split('/')[4];
					if (text.Contains(Farmer.getSkillNameFromIndex(currentSkill)) && text.Contains(currentLevel.ToString() ?? ""))
					{
						newCraftingRecipes.Add(new CraftingRecipe(craftingRecipe.Key, isCookingRecipe: false));
						if (!Game1.player.craftingRecipes.ContainsKey(craftingRecipe.Key))
						{
							Game1.player.craftingRecipes.Add(craftingRecipe.Key, 0);
						}
						num2 += (newCraftingRecipes.Last().bigCraftable ? 128 : 64);
					}
				}
				foreach (KeyValuePair<string, string> cookingRecipe in CraftingRecipe.cookingRecipes)
				{
					string text2 = cookingRecipe.Value.Split('/')[3];
					if (text2.Contains(Farmer.getSkillNameFromIndex(currentSkill)) && text2.Contains(currentLevel.ToString() ?? ""))
					{
						newCraftingRecipes.Add(new CraftingRecipe(cookingRecipe.Key, isCookingRecipe: true));
						if (!Game1.player.cookingRecipes.ContainsKey(cookingRecipe.Key))
						{
							Game1.player.cookingRecipes.Add(cookingRecipe.Key, 0);
						}
						num2 += (newCraftingRecipes.Last().bigCraftable ? 128 : 64);
					}
				}
				height = num2 + 256 + extraInfoForLevel.Count * 64 * 3 / 4;
				Log.It("LevelUpMenu.update height:" + height);
				Game1.player.freezePause = 100;
			}
			if (isActive && informationUp)
			{
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.player.freezePause = 100;
			}
		}

		public void okButtonClicked()
		{
			getLevelPerk(currentSkill, currentLevel);
			RemoveLevelFromLevelList();
			isActive = false;
			informationUp = false;
		}

		public virtual void RemoveLevelFromLevelList()
		{
			for (int i = 0; i < Game1.player.newLevels.Count; i++)
			{
				Point point = Game1.player.newLevels[i];
				if (point.X == currentSkill && point.Y == currentLevel)
				{
					Game1.player.newLevels.RemoveAt(i);
					i--;
				}
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.SnappyMenus && ((!Game1.options.doesInputListContain(Game1.options.cancelButton, key) && !Game1.options.doesInputListContain(Game1.options.menuButton, key)) || !isProfessionChooser))
			{
				base.receiveKeyPress(key);
			}
		}

		public void getLevelPerk(int skill, int level)
		{
			switch (skill)
			{
			case 4:
				Game1.player.maxHealth += 5;
				break;
			case 1:
				switch (level)
				{
				case 2:
					if (!Game1.player.hasOrWillReceiveMail("fishing2"))
					{
						Game1.addMailForTomorrow("fishing2");
					}
					break;
				case 6:
					if (!Game1.player.hasOrWillReceiveMail("fishing6"))
					{
						Game1.addMailForTomorrow("fishing6");
					}
					break;
				}
				break;
			}
			Game1.player.health = Game1.player.maxHealth;
			Game1.player.Stamina = (int)Game1.player.maxStamina;
		}

		public override void draw(SpriteBatch b)
		{
			if (timerBeforeStart > 0)
			{
				return;
			}
			updatePositions();
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			if (!dropTitle)
			{
				foreach (TemporaryAnimatedSprite littleStar in littleStars)
				{
					littleStar.draw(b);
				}
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + width / 2 - 116, yPositionOnScreen - 32 + 12), new Rectangle(363, 87, 58, 22), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
			}
			if (!informationUp && isActive && starIcon != null)
			{
				starIcon.draw(b);
			}
			else
			{
				if (!informationUp)
				{
					return;
				}
				if (isProfessionChooser)
				{
					int num = ((!dropTitle) ? yPositionOnScreen : 0);
					Game1.drawDialogueBox(xPositionOnScreen, num, width, height, speaker: false, drawOnlyBox: true);
					drawHorizontalPartition(b, num + 192);
					drawMobileVerticalIntersectingPartition(b, xPositionOnScreen + width / 2 - 32, num + 192, yPositionOnScreen - num);
					Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, num + IClickableMenu.spaceToClearTopBorder + 16), sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.088f);
					Utility.drawTextWithShadow(b, title, Game1.dialogueFont, new Vector2((float)(xPositionOnScreen + width / 2) - Game1.dialogueFont.MeasureString(title).X / 2f, num + IClickableMenu.spaceToClearTopBorder + 16), Game1.textColor);
					Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2(xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 64, num + IClickableMenu.spaceToClearTopBorder + 16), sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.088f);
					string text = Game1.content.LoadString("Strings\\UI:LevelUp_ChooseProfession");
					b.DrawString(Game1.smallFont, text, new Vector2((float)(xPositionOnScreen + width / 2) - Game1.smallFont.MeasureString(text).X / 2f, num + 64 + IClickableMenu.spaceToClearTopBorder), Game1.textColor);
					Utility.drawTextWithShadow(b, leftProfessionDescription[0], Game1.dialogueFont, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, num + IClickableMenu.spaceToClearTopBorder + 160), leftProfessionColor);
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + width / 2 - 112, num + IClickableMenu.spaceToClearTopBorder + 160 - 16), new Rectangle(professionsToChoose[0] % 6 * 16, 624 + professionsToChoose[0] / 6 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
					for (int i = 1; i < leftProfessionDescription.Count; i++)
					{
						Utility.drawTextWithShadow(b, Game1.parseText(leftProfessionDescription[i], Game1.smallFont, width / 2 - 64), Game1.smallFont, new Vector2(-4 + xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 32, num + IClickableMenu.spaceToClearTopBorder + 64 + 16 + 64 * (i + 1)), leftProfessionColor);
					}
					Utility.drawTextWithShadow(b, rightProfessionDescription[0], Game1.dialogueFont, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + width / 2, num + IClickableMenu.spaceToClearTopBorder + 160), rightProfessionColor);
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + width - 128, num + IClickableMenu.spaceToClearTopBorder + 160 - 16), new Rectangle(professionsToChoose[1] % 6 * 16, 624 + professionsToChoose[1] / 6 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
					for (int j = 1; j < rightProfessionDescription.Count; j++)
					{
						Utility.drawTextWithShadow(b, Game1.parseText(rightProfessionDescription[j], Game1.smallFont, width / 2 - 48), Game1.smallFont, new Vector2(-4 + xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + width / 2, num + IClickableMenu.spaceToClearTopBorder + 64 + 16 + 64 * (j + 1)), rightProfessionColor);
					}
					return;
				}
				if (dropTitle)
				{
					IClickableMenu.drawTextureBox(b, xPositionOnScreen, 0, width, height, Color.White);
				}
				else
				{
					Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
				}
				Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.088f);
				Utility.drawTextWithShadow(b, title, Game1.dialogueFont, new Vector2((float)(xPositionOnScreen + width / 2) - Game1.dialogueFont.MeasureString(title).X / 2f, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), Game1.textColor);
				Utility.drawWithShadow(b, Game1.buffsIcons, new Vector2(xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 64, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16), sourceRectForLevelIcon, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.088f);
				int num2 = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 80;
				foreach (string item in extraInfoForLevel)
				{
					Utility.drawTextWithShadow(b, item, Game1.smallFont, new Vector2((float)(xPositionOnScreen + width / 2) - Game1.smallFont.MeasureString(item).X / 2f, num2), Game1.textColor);
					num2 += 48;
				}
				foreach (CraftingRecipe newCraftingRecipe in newCraftingRecipes)
				{
					string sub = Game1.content.LoadString("Strings\\UI:LearnedRecipe_" + (newCraftingRecipe.isCookingRecipe ? "cooking" : "crafting"));
					string text2 = Game1.content.LoadString("Strings\\UI:LevelUp_NewRecipe", sub, newCraftingRecipe.DisplayName);
					if (num2 < okButton.bounds.Y - 36)
					{
						Utility.drawTextWithShadow(b, text2, Game1.smallFont, new Vector2((float)(xPositionOnScreen + width / 2) - Game1.smallFont.MeasureString(text2).X / 2f, num2), Game1.textColor);
						num2 += 32;
					}
				}
				okButton.draw(b);
			}
		}

		private void updatePositions()
		{
			if (width > Game1.uiViewport.Width - Game1.xEdge * 2)
			{
				width = Game1.uiViewport.Width - Game1.xEdge * 2;
			}
			if (height > Game1.uiViewport.Height)
			{
				height = Game1.uiViewport.Height;
				dropTitle = true;
				yPositionOnScreen = -104;
				okButton.bounds = new Rectangle(xPositionOnScreen + width / 2 - 40, height - 80 - 16, 80, 80);
			}
			else
			{
				yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2;
				okButton.bounds = new Rectangle(xPositionOnScreen + width / 2 - 40, yPositionOnScreen + height - 80 - 48, 80, 80);
			}
			xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
		}
	}
}
