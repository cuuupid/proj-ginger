using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Minigames;

namespace StardewValley.Menus
{
	public class MobileFarmChooser : IClickableMenu
	{
		public List<ClickableTextureComponent> farmTypeButtons = new List<ClickableTextureComponent>();

		private float widthMod;

		private float heightMod;

		private int startX;

		private int buttonY;

		private int farmButtonSpacing;

		private int numFarmTypeButtons;

		private int farmButtonWidth;

		private int farmButtonHeight;

		private Rectangle nameBox;

		private Rectangle descBox;

		private Rectangle okPos;

		private Rectangle backPos;

		private string nameString;

		private string descString;

		private Vector2 nameSize;

		private Vector2 descSize;

		private bool isStandaloneScreen;

		private TextBox farmnameBox;

		private Rectangle farmBoxRect;

		private ClickableTextureComponent okButton;

		private ClickableTextureComponent backButton;

		private ClickableTextureComponent coopHelpButton;

		private ClickableTextureComponent coopHelpOkButton;

		private int farmNameSuffixLength;

		private string farmNameSuffix;

		private string farmMessage;

		private string coopHelpString;

		private string noneString;

		private string normalDiffString;

		private string toughDiffString;

		private string hardDiffString;

		private string superDiffString;

		private CharacterCustomization.Source source;

		private Rectangle leftSelectButtonPos;

		private Rectangle rightSelectButtonPos;

		private ClickableTextureComponent leftSelectButton;

		private ClickableTextureComponent rightSelectButton;

		public List<ClickableComponent> labels = new List<ClickableComponent>();

		public List<ClickableComponent> leftSelectionButtons = new List<ClickableComponent>();

		public List<ClickableComponent> rightSelectionButtons = new List<ClickableComponent>();

		private ClickableComponent startingCabinsLabel;

		private ClickableComponent cabinLayoutLabel;

		private ClickableComponent difficultyModifierLabel;

		public List<ClickableTextureComponent> cabinLayoutButtons = new List<ClickableTextureComponent>();

		public bool showingCoopHelp;

		private bool isHost;

		protected Dictionary<int, ClickableComponent> farmTypeButtonLookup = new Dictionary<int, ClickableComponent>();

		public MobileFarmChooser(int x, int y, int width, int height, CharacterCustomization.Source source, bool isStandaloneScreen = true)
			: base(x, y, width, height)
		{
			this.source = source;
			widthMod = (float)width / 1280f;
			heightMod = (float)height / 720f;
			farmTypeButtons.Clear();
			numFarmTypeButtons = 6;
			farmButtonWidth = 76;
			this.isStandaloneScreen = isStandaloneScreen;
			if (isStandaloneScreen)
			{
				_ = 3;
				isHost = true;
				farmNameSuffix = Game1.content.LoadString("Strings\\UI:Character_FarmNameSuffix");
				farmNameSuffixLength = (int)Game1.dialogueFont.MeasureString(farmNameSuffix).X;
				farmButtonSpacing = 32;
				farmButtonHeight = 80;
				startX = x + (width - numFarmTypeButtons * farmButtonWidth - (numFarmTypeButtons - 1) * farmButtonSpacing) / 2;
				buttonY = (int)(124f * heightMod);
				nameBox = new Rectangle(isHost ? (width / 8) : (startX - 64), (int)(252f * heightMod), isHost ? (width * 3 / 4) : (numFarmTypeButtons * farmButtonWidth + (numFarmTypeButtons - 1) * farmButtonSpacing + 128), (int)(64f * heightMod));
				descBox = new Rectangle(nameBox.X, (int)(336f * heightMod), nameBox.Width, (int)(180f * heightMod));
				farmMessage = Game1.content.LoadString("Strings\\UI:Character_Farm").Replace("\n", " ");
				if (Game1.player.farmName == "")
				{
					Game1.player.farmName.Value = farmMessage;
				}
				farmBoxRect = new Rectangle(width / 2 - 180, descBox.Y + descBox.Height + 16, 360 - farmNameSuffixLength, 64);
				farmnameBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor, drawBackground: false)
				{
					X = farmBoxRect.X,
					Y = farmBoxRect.Y,
					Width = farmBoxRect.Width,
					Height = farmBoxRect.Height,
					Text = Game1.player.farmName,
					textLimit = 9,
					TitleText = Game1.content.LoadString("Strings\\UI:Character_Farm").Replace("\n", " ")
				};
				Rectangle rectangle = new Rectangle((int)(148f * widthMod) + xPositionOnScreen, (int)(572f * heightMod), width - (int)(296f * widthMod), (int)(120f * heightMod));
				okPos = new Rectangle(rectangle.X + rectangle.Width + (int)(12f * widthMod), rectangle.Y + (rectangle.Height - 80) / 2, 80, 80);
				okButton = new ClickableTextureComponent("OK", okPos, null, null, Game1.mobileSpriteSheet, new Rectangle(0, 0, 20, 20), 4f, drawShadow: true);
				backPos = new Rectangle(xPositionOnScreen + 32, okButton.bounds.Y, 80, 76);
				backButton = new ClickableTextureComponent("Back", backPos, null, null, Game1.mobileSpriteSheet, new Rectangle(80, 0, 20, 19), 4f, drawShadow: true);
				_ = 3;
				Game1.startingCabins = 0;
				Game1.player.difficultyModifier = 1f;
				labels.Add(startingCabinsLabel = new ClickableComponent(new Rectangle(descBox.X + 32, descBox.Y + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_StartingCabins")));
				leftSelectionButtons.Add(new ClickableTextureComponent("Cabins", new Rectangle(descBox.X, descBox.Y + 56, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f));
				rightSelectionButtons.Add(new ClickableTextureComponent("Cabins", new Rectangle(descBox.X + (int)Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:Character_StartingCabins")).X, descBox.Y + 56, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f));
				labels.Add(cabinLayoutLabel = new ClickableComponent(new Rectangle(descBox.X + (descBox.Width - (int)Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:Character_CabinLayout")).X) / 2, descBox.Y + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_CabinLayout")));
				cabinLayoutButtons.Add(new ClickableTextureComponent("Close", new Rectangle(descBox.X + (descBox.Width + 64) / 2, descBox.Y + 56, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_Close"), Game1.mouseCursors, new Rectangle(208, 192, 16, 16), 4f));
				cabinLayoutButtons.Add(new ClickableTextureComponent("Separate", new Rectangle(descBox.X + (descBox.Width - 172) / 2, descBox.Y + 56, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_Separate"), Game1.mouseCursors, new Rectangle(224, 192, 16, 16), 4f));
				labels.Add(difficultyModifierLabel = new ClickableComponent(new Rectangle(descBox.X + descBox.Width - 32 - (int)Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:Character_Difficulty")).X, descBox.Y + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Difficulty")));
				leftSelectionButtons.Add(new ClickableTextureComponent("Difficulty", new Rectangle(descBox.X + descBox.Width - 64 - (int)Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:Character_Difficulty")).X, descBox.Y + 56, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f));
				rightSelectionButtons.Add(new ClickableTextureComponent("Difficulty", new Rectangle(descBox.X + descBox.Width - 64, descBox.Y + 56, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f));
				noneString = Game1.content.LoadString("Strings\\UI:Character_none");
				normalDiffString = Game1.content.LoadString("Strings\\UI:Character_Normal");
				toughDiffString = Game1.content.LoadString("Strings\\UI:Character_Tough");
				hardDiffString = Game1.content.LoadString("Strings\\UI:Character_Hard");
				superDiffString = Game1.content.LoadString("Strings\\UI:Character_Super");
			}
			else
			{
				farmButtonSpacing = 12;
				farmButtonHeight = 76;
				startX = x + (width - numFarmTypeButtons * farmButtonWidth - (numFarmTypeButtons - 1) * farmButtonSpacing) / 2;
				buttonY = y + 32;
				nameBox = new Rectangle(x + 16, buttonY + farmButtonHeight + 16, width - 32, 64);
				descBox = new Rectangle(x + 16, buttonY + farmButtonHeight + 56, nameBox.Width, height - (farmButtonHeight + 56));
				int num = 76;
				int num2 = xPositionOnScreen + width / 2;
				leftSelectButtonPos = new Rectangle(num2 - num - 70, buttonY, 80, 76);
				leftSelectButton = new ClickableTextureComponent("BottomL", leftSelectButtonPos, null, "", Game1.mobileSpriteSheet, new Rectangle(80, 0, 20, 19), 4f, drawShadow: true);
				rightSelectButtonPos = new Rectangle(num2 + num, buttonY, 80, 76);
				rightSelectButton = new ClickableTextureComponent("BottomR", rightSelectButtonPos, null, "", Game1.mobileSpriteSheet, new Rectangle(100, 0, 20, 19), 4f, drawShadow: true);
			}
			farmTypeButtons.Add(new ClickableTextureComponent("Standard", new Rectangle(startX, buttonY, 76, 76), null, Game1.content.LoadString("Strings\\UI:Character_FarmStandard"), Game1.mouseCursors, new Rectangle(0, 324, 22, 20), 4f));
			farmTypeButtonLookup[0] = farmTypeButtons[farmTypeButtons.Count - 1];
			farmTypeButtons.Add(new ClickableTextureComponent("Fishing", new Rectangle(startX + farmButtonWidth + farmButtonSpacing, buttonY, 76, 76), null, Game1.content.LoadString("Strings\\UI:Character_FarmFishing"), Game1.mouseCursors, new Rectangle(22, 324, 22, 20), 4f));
			farmTypeButtonLookup[1] = farmTypeButtons[farmTypeButtons.Count - 1];
			farmTypeButtons.Add(new ClickableTextureComponent("Foraging", new Rectangle(startX + 2 * (farmButtonWidth + farmButtonSpacing), buttonY, 76, 76), null, Game1.content.LoadString("Strings\\UI:Character_FarmForaging"), Game1.mouseCursors, new Rectangle(44, 324, 22, 20), 4f));
			farmTypeButtonLookup[2] = farmTypeButtons[farmTypeButtons.Count - 1];
			farmTypeButtons.Add(new ClickableTextureComponent("Mining", new Rectangle(startX + 3 * (farmButtonWidth + farmButtonSpacing), buttonY, 76, 76), null, Game1.content.LoadString("Strings\\UI:Character_FarmMining"), Game1.mouseCursors, new Rectangle(66, 324, 22, 20), 4f));
			farmTypeButtonLookup[3] = farmTypeButtons[farmTypeButtons.Count - 1];
			farmTypeButtons.Add(new ClickableTextureComponent("Combat", new Rectangle(startX + 4 * (farmButtonWidth + farmButtonSpacing), buttonY, 76, 76), null, Game1.content.LoadString("Strings\\UI:Character_FarmCombat"), Game1.mouseCursors, new Rectangle(88, 324, 22, 20), 4f));
			farmTypeButtonLookup[4] = farmTypeButtons[farmTypeButtons.Count - 1];
			farmTypeButtons.Add(new ClickableTextureComponent("FourCorners", new Rectangle(startX + 5 * (farmButtonWidth + farmButtonSpacing), buttonY, 76, 76), null, Game1.content.LoadString("Strings\\UI:Character_FarmFourCorners"), Game1.mouseCursors, new Rectangle(0, 345, 22, 20), 4f));
			farmTypeButtonLookup[5] = farmTypeButtons[farmTypeButtons.Count - 1];
			farmTypeButtons.Add(new ClickableTextureComponent("Beach", new Rectangle(startX + 6 * (farmButtonWidth + farmButtonSpacing), buttonY, 76, 76), null, Game1.content.LoadString("Strings\\UI:Character_FarmBeach"), Game1.mouseCursors, new Rectangle(22, 345, 22, 20), 4f));
			farmTypeButtonLookup[6] = farmTypeButtons[farmTypeButtons.Count - 1];
			if (farmTypeButtonLookup.ContainsKey(Game1.whichFarm))
			{
				optionButtonClick(farmTypeButtonLookup[Game1.whichFarm].name);
			}
		}

		private string getNameOfDifficulty()
		{
			if (Game1.player.difficultyModifier < 0.5f)
			{
				return superDiffString;
			}
			if (Game1.player.difficultyModifier < 0.75f)
			{
				return hardDiffString;
			}
			if (Game1.player.difficultyModifier < 1f)
			{
				return toughDiffString;
			}
			return normalDiffString;
		}

		private void optionButtonClick(string name)
		{
			switch (name)
			{
			case "OK":
			{
				int num2 = 15;
				Game1.playSound("bigSelect");
				Game1.player.farmName.Value = farmnameBox.Text.Trim();
				if (Game1.player.farmName.Length > num2)
				{
					Game1.player.farmName.Value = Game1.player.farmName.Value.Substring(0, num2);
				}
				if (Game1.activeClickableMenu is TitleMenu)
				{
					TutorialManager.Instance.completeTutorial(tutorialType.DUMMY_FINISH_CUSTOMIZE);
					(Game1.activeClickableMenu as TitleMenu).createdNewCharacter(skipIntro: false);
					break;
				}
				TutorialManager.Instance.completeTutorial(tutorialType.DUMMY_FINISH_CUSTOMIZE);
				Game1.exitActiveMenu();
				if (Game1.currentMinigame != null && Game1.currentMinigame is Intro)
				{
					(Game1.currentMinigame as Intro).doneCreatingCharacter();
				}
				break;
			}
			case "Back":
				if (Game1.activeClickableMenu is TitleMenu)
				{
					TitleMenu.subMenu = new CharacterCustomization(source);
					Game1.playSound("shwip");
				}
				break;
			case "Close":
				Game1.cabinsSeparate = false;
				break;
			case "Separate":
				Game1.cabinsSeparate = true;
				break;
			default:
				foreach (ClickableTextureComponent farmTypeButton in farmTypeButtons)
				{
					if (!(farmTypeButton.name == name))
					{
						continue;
					}
					int num = -1;
					foreach (KeyValuePair<int, ClickableComponent> item in farmTypeButtonLookup)
					{
						if (item.Value == farmTypeButton)
						{
							num = item.Key;
							break;
						}
					}
					if (num >= 0)
					{
						Game1.whichFarm = num;
						if (num == 4)
						{
							Game1.spawnMonstersAtNight = true;
						}
						else
						{
							Game1.spawnMonstersAtNight = false;
						}
						nameString = farmTypeButton.hoverText.Split('_')[0];
						descString = farmTypeButton.hoverText.Split('_')[1];
						Game1.playSound("coin");
					}
					break;
				}
				break;
			}
			nameSize = Game1.dialogueFont.MeasureString(nameString);
			descSize = Game1.dialogueFont.MeasureString(descString);
		}

		private void selectionClick(string name, int change)
		{
			if (!(name == "Cabins"))
			{
				if (name == "Difficulty")
				{
					if (Game1.player.difficultyModifier < 1f && change < 0)
					{
						Game1.playSound("breathout");
						Game1.player.difficultyModifier += 0.25f;
					}
					else if (Game1.player.difficultyModifier > 0.25f && change > 0)
					{
						Game1.playSound("batFlap");
						Game1.player.difficultyModifier -= 0.25f;
					}
				}
			}
			else
			{
				if ((Game1.startingCabins != 0 || change >= 0) && (Game1.startingCabins != 3 || change <= 0))
				{
					Game1.playSound("axchop");
				}
				Game1.startingCabins += change;
				Game1.startingCabins = Math.Max(0, Math.Min(3, Game1.startingCabins));
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (isStandaloneScreen)
			{
				foreach (ClickableTextureComponent farmTypeButton in farmTypeButtons)
				{
					if (farmTypeButton.containsPoint(x, y) && !farmTypeButton.name.Contains("Gray"))
					{
						optionButtonClick(farmTypeButton.name);
					}
				}
			}
			else
			{
				if (leftSelectButton.containsPoint(x, y))
				{
					Game1.whichFarm--;
					if (Game1.whichFarm < 0)
					{
						Game1.whichFarm = farmTypeButtons.Count - 1;
					}
					optionButtonClick(farmTypeButtons[Game1.whichFarm].name);
				}
				if (rightSelectButton.containsPoint(x, y))
				{
					Game1.whichFarm++;
					if (Game1.whichFarm >= farmTypeButtons.Count)
					{
						Game1.whichFarm = 0;
					}
					optionButtonClick(farmTypeButtons[Game1.whichFarm].name);
				}
			}
			if (isStandaloneScreen)
			{
				if (farmnameBox != null)
				{
					farmnameBox.Update();
				}
				if (okButton.containsPoint(x, y) && canLeaveMenu())
				{
					Game1.playSound("smallSelect");
				}
				if (backButton.containsPoint(x, y))
				{
					Game1.playSound("smallSelect");
				}
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			if (isStandaloneScreen)
			{
				okButton.drawShadow = true;
				okButton.bounds.X = okPos.X;
				okButton.bounds.Y = okPos.Y;
				backButton.drawShadow = true;
				backButton.bounds.X = backPos.X;
				backButton.bounds.Y = backPos.Y;
				if (backButton.bounds.Contains(x, y))
				{
					backButton.drawShadow = false;
					backButton.bounds.X = backPos.X - 4;
					backButton.bounds.Y = backPos.Y + 4;
				}
				if (okButton.bounds.Contains(x, y) && canLeaveMenu())
				{
					okButton.drawShadow = false;
					okButton.bounds.X = okPos.X - 4;
					okButton.bounds.Y = okPos.Y + 4;
				}
			}
			else
			{
				leftSelectButton.drawShadow = true;
				leftSelectButton.bounds.X = leftSelectButtonPos.X;
				leftSelectButton.bounds.Y = leftSelectButtonPos.Y;
				rightSelectButton.drawShadow = true;
				rightSelectButton.bounds.X = rightSelectButtonPos.X;
				rightSelectButton.bounds.Y = rightSelectButtonPos.Y;
				if (leftSelectButton.containsPoint(x, y))
				{
					leftSelectButton.drawShadow = false;
					leftSelectButton.bounds.X = leftSelectButtonPos.X - 4;
					leftSelectButton.bounds.Y = leftSelectButtonPos.Y + 4;
				}
				if (rightSelectButton.containsPoint(x, y))
				{
					rightSelectButton.drawShadow = false;
					rightSelectButton.bounds.X = rightSelectButtonPos.X - 4;
					rightSelectButton.bounds.Y = rightSelectButtonPos.Y + 4;
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			if (isStandaloneScreen)
			{
				if (okButton.containsPoint(x, y) && canLeaveMenu())
				{
					optionButtonClick(okButton.name);
				}
				if (backButton.containsPoint(x, y))
				{
					optionButtonClick(backButton.name);
				}
				okButton.drawShadow = true;
				okButton.bounds.X = okPos.X;
				okButton.bounds.Y = okPos.Y;
				backButton.drawShadow = true;
				backButton.bounds.X = backPos.X;
				backButton.bounds.Y = backPos.Y;
				if (!isHost)
				{
					return;
				}
				foreach (ClickableTextureComponent rightSelectionButton in rightSelectionButtons)
				{
					if (rightSelectionButton.containsPoint(x, y))
					{
						selectionClick(rightSelectionButton.name, 1);
					}
				}
				foreach (ClickableTextureComponent leftSelectionButton in leftSelectionButtons)
				{
					if (leftSelectionButton.containsPoint(x, y))
					{
						selectionClick(leftSelectionButton.name, -1);
					}
				}
				{
					foreach (ClickableTextureComponent cabinLayoutButton in cabinLayoutButtons)
					{
						if (cabinLayoutButton.containsPoint(x, y))
						{
							optionButtonClick(cabinLayoutButton.name);
						}
					}
					return;
				}
			}
			leftSelectButton.drawShadow = true;
			leftSelectButton.bounds.X = leftSelectButtonPos.X;
			leftSelectButton.bounds.Y = leftSelectButtonPos.Y;
			rightSelectButton.drawShadow = true;
			rightSelectButton.bounds.X = rightSelectButtonPos.X;
			rightSelectButton.bounds.Y = rightSelectButtonPos.Y;
		}

		public override void update(GameTime time)
		{
		}

		public override void draw(SpriteBatch b)
		{
			if (isStandaloneScreen)
			{
				IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
				foreach (ClickableTextureComponent farmTypeButton in farmTypeButtons)
				{
					farmTypeButton.draw(b);
				}
				int x = startX - 8;
				if (farmTypeButtonLookup.ContainsKey(Game1.whichFarm))
				{
					ClickableComponent clickableComponent = farmTypeButtonLookup[Game1.whichFarm];
					x = clickableComponent.bounds.X - 8;
				}
				IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(20, 96, 20, 20), x, buttonY - 8, 92, 92, Color.White, 4f, drawShadow: false);
			}
			else
			{
				ClickableTextureComponent clickableTextureComponent = farmTypeButtons[Game1.whichFarm];
				clickableTextureComponent.bounds.X = xPositionOnScreen + width / 2 - clickableTextureComponent.bounds.Width / 2;
				clickableTextureComponent.draw(b);
				leftSelectButton.draw(b);
				rightSelectButton.draw(b);
			}
			if (isStandaloneScreen)
			{
				IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(0, 96, 20, 20), nameBox.X, nameBox.Y, nameBox.Width, nameBox.Height, Color.White, 4f, drawShadow: false);
				Utility.drawTextWithShadow(b, nameString, Game1.dialogueFont, new Vector2((float)nameBox.X + ((float)nameBox.Width - nameSize.X) / 2f, (float)(nameBox.Y + 4) + ((float)nameBox.Height - nameSize.Y) / 2f), Game1.textColor);
				IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(0, 96, 20, 20), descBox.X, descBox.Y, descBox.Width, descBox.Height, Color.White, 4f, drawShadow: false);
				if (farmnameBox != null)
				{
					IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 320, 60, 60), farmBoxRect.X, farmBoxRect.Y, farmBoxRect.Width, farmBoxRect.Height, farmnameBox.Selected ? Color.White : Color.Wheat, 1f, drawShadow: false);
					Game1.player.farmName.Value = farmnameBox.Text;
					if (Game1.player.farmName == farmMessage)
					{
						farmnameBox.setTextColor(Color.Red);
					}
					else
					{
						farmnameBox.setTextColor(Game1.textColor);
					}
					farmnameBox.Draw(b);
					Utility.drawTextWithShadow(b, farmNameSuffix, Game1.dialogueFont, new Vector2(farmnameBox.X + farmnameBox.Width + 16, farmnameBox.Y + 12), Game1.textColor);
					if (okButton != null)
					{
						if (canLeaveMenu())
						{
							okButton.draw(b, Color.White, 0.75f);
						}
						else
						{
							okButton.draw(b, Color.White, 0.75f);
							okButton.draw(b, Color.LightGray * 0.5f, 0.751f);
						}
					}
					if (backButton != null)
					{
						backButton.draw(b);
					}
					if (isHost)
					{
						foreach (ClickableTextureComponent rightSelectionButton in rightSelectionButtons)
						{
							rightSelectionButton.draw(b);
						}
						foreach (ClickableTextureComponent leftSelectionButton in leftSelectionButtons)
						{
							leftSelectionButton.draw(b);
						}
						foreach (ClickableTextureComponent rightSelectionButton2 in rightSelectionButtons)
						{
							rightSelectionButton2.draw(b);
						}
						foreach (ClickableComponent label in labels)
						{
							string text = "";
							float num = 24f;
							Color textColor = Game1.textColor;
							if (label == startingCabinsLabel)
							{
								text = ((Game1.startingCabins == 0 && noneString != null) ? noneString : (Game1.startingCabins.ToString() ?? ""));
							}
							else if (label == difficultyModifierLabel)
							{
								text = getNameOfDifficulty();
							}
							else
							{
								textColor = Game1.textColor;
							}
							Utility.drawBoldText(b, label.name, Game1.smallFont, new Vector2(label.bounds.X, label.bounds.Y), textColor);
							if (text.Length > 0)
							{
								Utility.drawTextWithShadow(b, text, Game1.smallFont, new Vector2((float)label.bounds.X + (Game1.smallFont.MeasureString(label.name).X - Game1.smallFont.MeasureString(text).X) / 2f, (float)(label.bounds.Y + 32) + num), textColor);
							}
						}
						foreach (ClickableTextureComponent cabinLayoutButton in cabinLayoutButtons)
						{
							cabinLayoutButton.draw(b, Color.White * ((Game1.startingCabins > 0) ? 1f : 0.5f), 0.9f);
							if (Game1.startingCabins > 0 && ((cabinLayoutButton.name.Equals("Close") && !Game1.cabinsSeparate) || (cabinLayoutButton.name.Equals("Separate") && Game1.cabinsSeparate)))
							{
								b.Draw(Game1.mobileSpriteSheet, new Rectangle(cabinLayoutButton.bounds.X - 4, cabinLayoutButton.bounds.Y - 4, cabinLayoutButton.bounds.Width + 8, cabinLayoutButton.bounds.Height + 8), new Rectangle(20, 96, 20, 20), Color.White);
							}
						}
					}
					else
					{
						Utility.drawMultiLineTextWithShadow(b, descString, Game1.smallFont, new Vector2(descBox.X + 16, descBox.Y), descBox.Width - 32, descBox.Height, Game1.textColor);
					}
				}
			}
			else
			{
				Utility.drawTextWithShadow(b, nameString, Game1.dialogueFont, new Vector2((float)nameBox.X + ((float)nameBox.Width - nameSize.X) / 2f, nameBox.Y), Game1.textColor);
				Utility.drawMultiLineTextWithShadow(b, descString, Game1.smallFont, new Vector2(descBox.X, descBox.Y), descBox.Width, descBox.Height, Game1.textColor, centreY: false);
			}
			base.draw(b);
		}

		public bool canLeaveMenu()
		{
			if (farmnameBox != null)
			{
				if (Game1.player.farmName.Length > 0)
				{
					return Game1.player.farmName != farmMessage;
				}
				return false;
			}
			return false;
		}
	}
}
