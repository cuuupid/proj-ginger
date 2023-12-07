using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class SkillsPage : IClickableMenu
	{
		public const int region_special1 = 10201;

		public const int region_special2 = 10202;

		public const int region_special3 = 10203;

		public const int region_special4 = 10204;

		public const int region_special5 = 10205;

		public const int region_special6 = 10206;

		public const int region_special7 = 10207;

		public const int region_special8 = 10208;

		public const int region_special9 = 10209;

		public const int region_skillArea1 = 0;

		public const int region_skillArea2 = 1;

		public const int region_skillArea3 = 2;

		public const int region_skillArea4 = 3;

		public const int region_skillArea5 = 4;

		public List<ClickableTextureComponent> skillBars = new List<ClickableTextureComponent>();

		public List<ClickableTextureComponent> skillAreas = new List<ClickableTextureComponent>();

		public List<ClickableTextureComponent> specialItems = new List<ClickableTextureComponent>();

		private string hoverText = "";

		private string hoverTitle = "";

		private int professionImage = -1;

		private int playerPanelIndex;

		private int playerPanelTimer;

		private Rectangle playerPanel;

		private int[] playerPanelFrames = new int[4] { 0, 1, 0, 2 };

		private int portraitX = 100;

		private const int portraitY = 80;

		private const int walletX = 30;

		private const int walletY = 480;

		private const int walletHeight = 130;

		private const int iconsX = 600;

		private const int iconsY = 90;

		private const int portraitTextXAddon = 64;

		private const int portraitTextYAddon = 192;

		private const int offset = 16;

		private float widthMod;

		private float heightMod;

		private string walletText;

		private string headerText;

		private string specialText;

		private Vector2 specialTextSize;

		private Vector2 walletTextSize;

		private ClickableTextureComponent hoverItem;

		private ClickableTextureComponent currentSkillbar;

		private float hoverTime;

		private bool showTooltip;

		private bool showProfession;

		private bool showProficiency;

		private Rectangle hoverBox;

		private int _selectedSpecialItemIndex = -1;

		public SkillsPage(int x, int y, int width, int height, float wMod, float hMod)
			: base(x, y, width, height)
		{
			widthMod = wMod;
			heightMod = hMod;
			walletText = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11610");
			walletTextSize = Game1.dialogueFont.MeasureString(walletText);
			int num = xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 80;
			int num2 = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + (int)((float)height / 2f) + 80;
			int num3 = 10;
			float num4 = 48f;
			playerPanel = new Rectangle(xPositionOnScreen + 64, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder, 128, 192);
			if (width < 1200)
			{
				portraitX = 48;
			}
			num2 = Utility.To4((int)((float)yPositionOnScreen + 480f * heightMod) + (int)(66f * heightMod / 2f));
			int num5 = 704;
			int num6 = width - (int)(96f * widthMod);
			int num7 = (num6 - num5) / 10 + 64;
			num = xPositionOnScreen + (int)(48f * widthMod);
			showTooltip = (showProfession = false);
			hoverItem = null;
			hoverTime = 0f;
			headerText = Game1.content.LoadString("Strings\\UI:GameMenu_Skills");
			num4 *= widthMod;
			if (Game1.player.canUnderstandDwarves)
			{
				specialItems.Add(new ClickableTextureComponent("", new Rectangle(num, num2, 64, 128), null, Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11587"), Game1.mouseCursors, new Rectangle(129, 320, 16, 16), 4f, drawShadow: true));
			}
			if (Game1.player.hasRustyKey)
			{
				specialItems.Add(new ClickableTextureComponent("", new Rectangle(num + num7, num2, 64, 128), null, Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11588"), Game1.mouseCursors, new Rectangle(145, 320, 16, 16), 4f, drawShadow: true));
			}
			if (Game1.player.hasClubCard)
			{
				specialItems.Add(new ClickableTextureComponent("", new Rectangle(num + 2 * num7, num2, 64, 128), null, Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11589"), Game1.mouseCursors, new Rectangle(161, 320, 16, 16), 4f, drawShadow: true));
			}
			if (Game1.player.eventsSeen.Contains(2120303))
			{
				specialItems.Add(new ClickableTextureComponent("", new Rectangle(num + 3 * num7, num2, 64, 64), null, Game1.content.LoadString("Strings\\Objects:BearPaw"), Game1.mouseCursors, new Rectangle(192, 336, 16, 16), 4f, drawShadow: true));
			}
			if (Game1.player.eventsSeen.Contains(3910979))
			{
				specialItems.Add(new ClickableTextureComponent("", new Rectangle(num + 4 * num7, num2, 64, 64), null, Game1.content.LoadString("Strings\\Objects:SpringOnionBugs"), Game1.mouseCursors, new Rectangle(208, 336, 16, 16), 4f, drawShadow: true));
			}
			if (Game1.player.hasSpecialCharm)
			{
				specialItems.Add(new ClickableTextureComponent("", new Rectangle(num + 5 * num7, num2, 64, 128), null, Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11590"), Game1.mouseCursors, new Rectangle(177, 320, 15, 16), 4f, drawShadow: true));
			}
			if (Game1.player.hasSkullKey)
			{
				specialItems.Add(new ClickableTextureComponent("", new Rectangle(num + 6 * num7, num2, 64, 128), null, Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11591"), Game1.mouseCursors, new Rectangle(193, 320, 15, 16), 4f, drawShadow: true));
			}
			if (Game1.player.hasDarkTalisman)
			{
				specialItems.Add(new ClickableTextureComponent("", new Rectangle(num + 7 * num7, num2, 64, 128), null, Game1.content.LoadString("Strings\\Objects:DarkTalisman"), Game1.mouseCursors, new Rectangle(225, 320, 16, 16), 4f, drawShadow: true));
			}
			if (Game1.player.hasMagicInk)
			{
				specialItems.Add(new ClickableTextureComponent("", new Rectangle(num + 8 * num7, num2, 64, 128), null, Game1.content.LoadString("Strings\\Objects:MagicInk"), Game1.mouseCursors, new Rectangle(241, 320, 16, 16), 4f, drawShadow: true));
			}
			if (Game1.player.hasMagnifyingGlass)
			{
				specialItems.Add(new ClickableTextureComponent("", new Rectangle(num + 9 * num7, num2, 64, 64), null, Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.magnifyingglass"), Game1.mouseCursors, new Rectangle(208, 320, 16, 16), 4f, drawShadow: true));
			}
			if (Game1.player.HasTownKey)
			{
				specialItems.Add(new ClickableTextureComponent("", new Rectangle(num + 10 * num7, num2, 64, 64), null, Game1.content.LoadString("Strings\\StringsFromCSFiles:KeyToTheTown"), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 912, 16, 16), 4f, drawShadow: true));
			}
			int num8 = 0;
			int num9 = (int)((float)yPositionOnScreen + 90f * heightMod) - 4;
			int num10 = (int)((float)xPositionOnScreen + 600f * widthMod) + 4;
			for (int i = 4; i < 10; i += 5)
			{
				for (int j = 0; j < 5; j++)
				{
					string professionBlurb = "";
					string professionTitle = "";
					bool flag = false;
					int whichProfession = -1;
					switch (j)
					{
					case 0:
						flag = Game1.player.FarmingLevel > i;
						whichProfession = Game1.player.getProfessionForSkill(0, i + 1);
						parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(whichProfession));
						break;
					case 1:
						flag = Game1.player.MiningLevel > i;
						whichProfession = Game1.player.getProfessionForSkill(3, i + 1);
						parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(whichProfession));
						break;
					case 2:
						flag = Game1.player.ForagingLevel > i;
						whichProfession = Game1.player.getProfessionForSkill(2, i + 1);
						parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(whichProfession));
						break;
					case 3:
						flag = Game1.player.FishingLevel > i;
						whichProfession = Game1.player.getProfessionForSkill(1, i + 1);
						parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(whichProfession));
						break;
					case 4:
						flag = Game1.player.CombatLevel > i;
						whichProfession = Game1.player.getProfessionForSkill(4, i + 1);
						parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(whichProfession));
						break;
					case 5:
						flag = Game1.player.LuckLevel > i;
						whichProfession = Game1.player.getProfessionForSkill(5, i + 1);
						parseProfessionDescription(ref professionBlurb, ref professionTitle, LevelUpMenu.getProfessionDescription(whichProfession));
						break;
					}
					if (flag && (i + 1) % 5 == 0)
					{
						skillBars.Add(new ClickableTextureComponent(whichProfession.ToString() ?? "", new Rectangle(num8 + num10 - 4 + i * 36, (int)((float)num9 + (float)j * heightMod * 72f), 56, 36), null, professionBlurb, Game1.mouseCursors, new Rectangle(159, 338, 14, 9), 4f, drawShadow: true));
					}
				}
				num8 += 24;
			}
			for (int k = 0; k < 5; k++)
			{
				int num11 = k;
				switch (num11)
				{
				case 1:
					num11 = 3;
					break;
				case 3:
					num11 = 1;
					break;
				}
				string text = "";
				switch (num11)
				{
				case 0:
					if (Game1.player.FarmingLevel > 0)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11592", Game1.player.FarmingLevel) + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11594", Game1.player.FarmingLevel);
					}
					break;
				case 2:
					if (Game1.player.ForagingLevel > 0)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11596", Game1.player.ForagingLevel);
					}
					break;
				case 1:
					if (Game1.player.FishingLevel > 0)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11598", Game1.player.FishingLevel);
					}
					break;
				case 3:
					if (Game1.player.MiningLevel > 0)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11600", Game1.player.MiningLevel);
					}
					break;
				case 4:
					if (Game1.player.CombatLevel > 0)
					{
						text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11602", Game1.player.CombatLevel * 5);
					}
					break;
				}
				skillAreas.Add(new ClickableTextureComponent(num11.ToString() ?? "", new Rectangle(num10 - 192, num9 + (int)((float)k * heightMod * 72f), 212, 36), num11.ToString() ?? "", text, null, Rectangle.Empty, 1f)
				{
					myID = k,
					downNeighborID = ((k < 4) ? (k + 1) : 10201),
					upNeighborID = ((k > 0) ? (k - 1) : 12341),
					rightNeighborID = 100 + k
				});
			}
		}

		private void parseProfessionDescription(ref string professionBlurb, ref string professionTitle, List<string> professionDescription)
		{
			if (professionDescription.Count <= 0)
			{
				return;
			}
			professionTitle = professionDescription[0];
			for (int i = 1; i < professionDescription.Count; i++)
			{
				professionBlurb += professionDescription[i];
				if (i < professionDescription.Count - 1)
				{
					professionBlurb += Environment.NewLine;
				}
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = ((skillAreas.Count > 0) ? getComponentWithID(0) : null);
			if (currentlySnappedComponent != null && Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				currentlySnappedComponent.snapMouseCursorToCenter();
			}
			if (currentlySnappedComponent != null && Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				currentlySnappedComponent.snapMouseCursorToCenter();
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableTextureComponent specialItem in specialItems)
			{
				if (specialItem.bounds.Contains(x, y))
				{
					SetSpecialItemTooltip(specialItem);
					break;
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void draw(SpriteBatch b)
		{
			if (!TutorialManager.Instance.skillsHasBeenSeen)
			{
				TutorialManager.Instance.skillsHasBeenSeen = true;
				TutorialManager.Instance.completeTutorial(tutorialType.DUMMY_SKILLS);
			}
			IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
			int num = (int)((float)xPositionOnScreen + (float)portraitX * widthMod);
			int num2 = (int)((float)yPositionOnScreen + 80f * heightMod);
			b.Draw((Game1.timeOfDay >= 1900) ? Game1.nightbg : Game1.daybg, new Vector2(num, num2), Color.White);
			bool value = Game1.player.swimming;
			Game1.player.swimming.Value = false;
			FarmerRenderer.isDrawingForUI = true;
			Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, Game1.player.bathingClothes ? 108 : 0, secondaryArm: false, flip: false), Game1.player.bathingClothes ? 108 : 0, new Rectangle(0, Game1.player.bathingClothes ? 576 : 0, 16, 32), new Vector2(num + 32, num2 + 32), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, Game1.player);
			if (Game1.timeOfDay >= 1900)
			{
				Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, 0, secondaryArm: false, flip: false), 0, new Rectangle(0, 0, 16, 32), new Vector2(num + 32, num2 + 32), Vector2.Zero, 0.8f, 2, Color.DarkBlue * 0.3f, 0f, 1f, Game1.player);
			}
			FarmerRenderer.isDrawingForUI = false;
			Game1.player.swimming.Value = value;
			SpriteFont dialogueFont = Game1.dialogueFont;
			dialogueFont = Game1.smallFont;
			num = xPositionOnScreen + (int)((float)portraitX * widthMod + 64f - dialogueFont.MeasureString(Game1.player.name).X / 2f);
			num2 = (int)((float)yPositionOnScreen + 80f * heightMod + 192f);
			Utility.drawTextWithShadow(b, Game1.player.name, dialogueFont, new Vector2(num, num2), Game1.textColor);
			string title = Game1.player.getTitle();
			num = xPositionOnScreen + (int)((float)portraitX * widthMod + 64f - dialogueFont.MeasureString(title).X / 2f);
			num2 = (int)((float)yPositionOnScreen + dialogueFont.MeasureString(Game1.player.name).Y + 80f * heightMod + 192f);
			Utility.drawTextWithShadow(b, title, dialogueFont, new Vector2(num, num2), Game1.textColor);
			num2 += 56;
			if ((int)Game1.netWorldState.Value.GoldenWalnuts > 0)
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2(num, num2), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 73, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
				num += 48;
				b.DrawString(Game1.smallFont, Game1.netWorldState.Value.GoldenWalnuts?.ToString() ?? "", new Vector2(num, num2), Game1.textColor);
				num += 64;
			}
			if (Game1.player.QiGems > 0)
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2(num, num2), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
				num += 48;
				b.DrawString(Game1.smallFont, Game1.player.QiGems.ToString() ?? "", new Vector2(num, num2), Game1.textColor);
				num += 64;
			}
			num2 = (int)((float)yPositionOnScreen + 90f * heightMod);
			num = (int)((float)xPositionOnScreen + 600f * widthMod);
			int num3 = 0;
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					bool flag = false;
					bool flag2 = false;
					string text = "";
					int num4 = 0;
					Rectangle value2 = Rectangle.Empty;
					switch (j)
					{
					case 0:
						flag = Game1.player.FarmingLevel > i;
						if (i == 0)
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11604");
						}
						num4 = Game1.player.FarmingLevel;
						flag2 = (int)Game1.player.addedFarmingLevel > 0;
						value2 = new Rectangle(10, 428, 10, 10);
						break;
					case 1:
						flag = Game1.player.MiningLevel > i;
						if (i == 0)
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11605");
						}
						num4 = Game1.player.MiningLevel;
						flag2 = (int)Game1.player.addedMiningLevel > 0;
						value2 = new Rectangle(30, 428, 10, 10);
						break;
					case 2:
						flag = Game1.player.ForagingLevel > i;
						if (i == 0)
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11606");
						}
						num4 = Game1.player.ForagingLevel;
						flag2 = (int)Game1.player.addedForagingLevel > 0;
						value2 = new Rectangle(60, 428, 10, 10);
						break;
					case 3:
						flag = Game1.player.FishingLevel > i;
						if (i == 0)
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11607");
						}
						num4 = Game1.player.FishingLevel;
						flag2 = (int)Game1.player.addedFishingLevel > 0;
						value2 = new Rectangle(20, 428, 10, 10);
						break;
					case 4:
						flag = Game1.player.CombatLevel > i;
						if (i == 0)
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11608");
						}
						num4 = Game1.player.CombatLevel;
						flag2 = (int)Game1.player.addedCombatLevel > 0;
						value2 = new Rectangle(120, 428, 10, 10);
						break;
					case 5:
						flag = Game1.player.LuckLevel > i;
						if (i == 0)
						{
							text = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11609");
						}
						num4 = Game1.player.LuckLevel;
						flag2 = (int)Game1.player.addedLuckLevel > 0;
						value2 = new Rectangle(50, 428, 10, 10);
						break;
					}
					if (!text.Equals(""))
					{
						Utility.drawTextWithShadow(b, text, Game1.dialogueFont, new Vector2((float)num - Game1.dialogueFont.MeasureString(text).X + 4f - 64f, (float)(num2 - 4) + (float)j * heightMod * 72f), Game1.textColor);
						b.Draw(Game1.mouseCursors, new Vector2(num - 56, (float)num2 + (float)j * heightMod * 72f), value2, Color.Black * 0.3f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.85f);
						b.Draw(Game1.mouseCursors, new Vector2(num - 52, (float)(num2 - 4) + (float)j * heightMod * 72f), value2, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
					}
					if (!flag && (i + 1) % 5 == 0)
					{
						b.Draw(Game1.mouseCursors, new Vector2(num3 + num - 4 + i * 36, (float)num2 + (float)j * heightMod * 72f), new Rectangle(145, 338, 14, 9), Color.Black * 0.35f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
						b.Draw(Game1.mouseCursors, new Vector2(num3 + num + i * 36, (float)(num2 - 4) + (float)j * heightMod * 72f), new Rectangle(145 + (flag ? 14 : 0), 338, 14, 9), Color.White * (flag ? 1f : 0.65f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
					}
					else if ((i + 1) % 5 != 0)
					{
						b.Draw(Game1.mouseCursors, new Vector2(num3 + num - 4 + i * 36, (float)num2 + (float)j * heightMod * 72f), new Rectangle(129, 338, 8, 9), Color.Black * 0.35f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.85f);
						b.Draw(Game1.mouseCursors, new Vector2(num3 + num + i * 36, (float)(num2 - 4) + (float)j * heightMod * 72f), new Rectangle(129 + (flag ? 8 : 0), 338, 8, 9), Color.White * (flag ? 1f : 0.65f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
					}
					if (i == 9)
					{
						NumberSprite.draw(num4, b, new Vector2(num3 + num + (i + 2) * 36 + 12 + ((num4 >= 10) ? 12 : 0), (float)(num2 + 16) + (float)j * heightMod * 72f), Color.Black * 0.35f, 1f, 0.85f, 1f, 0);
						NumberSprite.draw(num4, b, new Vector2(num3 + num + (i + 2) * 36 + 16 + ((num4 >= 10) ? 12 : 0), (float)(num2 + 12) + (float)j * heightMod * 72f), (flag2 ? Color.LightGreen : Color.SandyBrown) * ((num4 == 0) ? 0.75f : 1f), 1f, 0.87f, 1f, 0);
					}
				}
				if ((i + 1) % 5 == 0)
				{
					num3 += 24;
				}
			}
			foreach (ClickableTextureComponent skillBar in skillBars)
			{
				skillBar.draw(b);
			}
			IClickableMenu.drawTextureBox(b, xPositionOnScreen + (int)(30f * widthMod), yPositionOnScreen + (int)(480f * heightMod), (int)((float)width - 30f * widthMod * 2f), (int)(130f * heightMod), Color.White);
			if (height > 600)
			{
				IClickableMenu.drawTextureBox(b, xPositionOnScreen + (int)(30f * widthMod * 2f), yPositionOnScreen + (int)((double)(480f * heightMod) - (double)walletTextSize.Y * 0.75 - 16.0), (int)((double)walletTextSize.X * 1.5 * (double)widthMod), (int)((double)walletTextSize.Y * 1.5), Color.White);
				int num5 = xPositionOnScreen + (int)(30f * widthMod * 2f) + (int)(((double)walletTextSize.X * 1.5 * (double)widthMod - (double)walletTextSize.X) / 2.0);
				Utility.drawTextWithShadow(b, walletText, Game1.dialogueFont, new Vector2(num5, yPositionOnScreen + (int)(480f * heightMod - walletTextSize.Y / 2f) - 16), Game1.textColor);
			}
			foreach (ClickableTextureComponent specialItem in specialItems)
			{
				specialItem.draw(b);
			}
			if (showTooltip)
			{
				IClickableMenu.drawTextureBox(b, hoverBox.X, hoverBox.Y, hoverBox.Width, hoverBox.Height, Color.White);
				Utility.drawTextWithShadow(b, specialText, Game1.smallFont, new Vector2(hoverBox.X + (int)(0.15f * specialTextSize.X), hoverBox.Y + (int)(specialTextSize.Y * 0.5f) + 4), Game1.textColor);
			}
			else if (showProfession)
			{
				int num6 = IClickableMenu.drawMobileTextPanel(b, specialText, Game1.dialogueFont, hoverBox.X, hoverBox.Y, hoverBox.Width, hoverBox.Height, 34, -1, hoverTitle, -1, null, null, 0, -1, -1, -1, -1, 1f, null, 0, inStockAndBuyable: false, drawBackgroundBox: true);
				if (num6 <= hoverBox.Y + (hoverBox.Height - 96))
				{
					b.Draw(Game1.mouseCursors, new Vector2(hoverBox.X + (hoverBox.Width - 64) / 2, hoverBox.Y + (hoverBox.Height - 96)), new Rectangle(professionImage % 6 * 16, 624 + professionImage / 6 * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
			}
			else if (showProficiency)
			{
				IClickableMenu.drawMobileTextPanel(b, hoverText, Game1.dialogueFont, hoverBox.X, hoverBox.Y, hoverBox.Width, hoverBox.Height, 34, -1, hoverTitle, -1, null, null, 0, -1, -1, -1, -1, 1f, null, 0, inStockAndBuyable: false, drawBackgroundBox: true);
			}
		}

		private void SetSpecialItemTooltip(ClickableTextureComponent c)
		{
			showTooltip = true;
			showProficiency = (showProfession = false);
			hoverItem = c;
			hoverTime = 0f;
			specialText = c.hoverText;
			specialTextSize = Game1.smallFont.MeasureString(specialText);
			hoverBox.X = hoverItem.bounds.X;
			if ((float)hoverBox.X + specialTextSize.X >= (float)(width - 64))
			{
				hoverBox.X -= (int)(specialTextSize.X * 3f / 2f);
			}
			hoverBox.Y = hoverItem.bounds.Y - 120;
			hoverBox.Width = (int)((double)specialTextSize.X * 1.3);
			hoverBox.Height = (int)(specialTextSize.Y * 2f);
		}

		private void SetSkillBarTooltip(ClickableTextureComponent c)
		{
			currentSkillbar = c;
			showTooltip = (showProficiency = false);
			showProfession = true;
			hoverTitle = LevelUpMenu.getProfessionTitleFromNumber(Convert.ToInt32(c.name));
			professionImage = Convert.ToInt32(c.name);
			hoverItem = c;
			hoverTime = 0f;
			specialText = c.hoverText;
			specialTextSize = Game1.smallFont.MeasureString(specialText);
			hoverBox.X = (int)((float)xPositionOnScreen + (float)portraitX * widthMod);
			hoverBox.Y = (int)((float)yPositionOnScreen + 80f * heightMod);
			hoverBox.Width = (int)(400f * widthMod);
			hoverBox.Height = (int)(300f * Math.Min(1f, heightMod));
		}

		private void SetSkillAreaTooltip(ClickableTextureComponent c)
		{
			showTooltip = (showProfession = false);
			showProficiency = true;
			hoverText = c.hoverText;
			hoverTitle = Farmer.getSkillDisplayNameFromIndex(Convert.ToInt32(c.name));
			hoverBox.X = (int)((float)xPositionOnScreen + (float)portraitX * widthMod);
			hoverBox.Y = (int)((float)yPositionOnScreen + 80f * heightMod);
			hoverBox.Width = (int)(400f * widthMod);
			hoverBox.Height = (int)(300f * Math.Min(1f, heightMod));
		}

		private void HideTooltip()
		{
			showTooltip = (showProfession = (showProficiency = false));
			currentSkillbar = null;
			hoverItem = null;
			hoverTime = 0f;
			specialText = "";
			foreach (ClickableTextureComponent skillBar in skillBars)
			{
				skillBar.scale = 4f;
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			currentSkillbar = null;
			showProfession = false;
			foreach (ClickableTextureComponent skillBar in skillBars)
			{
				if (skillBar.bounds.Contains(x, y) && !skillBar.name.Equals("-1"))
				{
					SetSkillBarTooltip(skillBar);
					return;
				}
			}
			foreach (ClickableTextureComponent skillArea in skillAreas)
			{
				if (skillArea.containsPoint(x, y) && skillArea.hoverText.Length > 0)
				{
					SetSkillAreaTooltip(skillArea);
					break;
				}
			}
			if (hoverItem != null)
			{
				if (hoverItem.bounds.Contains(x, y))
				{
					showTooltip = true;
					return;
				}
				showTooltip = false;
				hoverItem = null;
				specialText = "";
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			HideTooltip();
		}

		public override void receiveGamePadButton(Buttons b)
		{
			int num = skillBars.Count + skillAreas.Count + specialItems.Count;
			switch (b)
			{
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
				_selectedSpecialItemIndex--;
				if (_selectedSpecialItemIndex < 0)
				{
					_selectedSpecialItemIndex = num - 1;
				}
				break;
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickRight:
				_selectedSpecialItemIndex++;
				if (_selectedSpecialItemIndex > num - 1)
				{
					_selectedSpecialItemIndex = 0;
				}
				break;
			}
			if (_selectedSpecialItemIndex > num - 1)
			{
				_selectedSpecialItemIndex = -1;
			}
			HideTooltip();
			if (_selectedSpecialItemIndex > -1)
			{
				if (_selectedSpecialItemIndex < skillBars.Count)
				{
					SetSkillBarTooltip(skillBars[_selectedSpecialItemIndex]);
				}
				else if (_selectedSpecialItemIndex < skillBars.Count + skillAreas.Count)
				{
					int index = _selectedSpecialItemIndex - skillBars.Count;
					SetSkillAreaTooltip(skillAreas[index]);
				}
				else
				{
					int index2 = _selectedSpecialItemIndex - skillBars.Count - skillAreas.Count;
					SetSpecialItemTooltip(specialItems[index2]);
				}
			}
		}
	}
}
