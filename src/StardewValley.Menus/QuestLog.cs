using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Quests;

namespace StardewValley.Menus
{
	public class QuestLog : IClickableMenu
	{
		public int questsPerPage = 6;

		public const int region_forwardButton = 101;

		public const int region_backButton = 102;

		public const int region_rewardBox = 103;

		public const int region_cancelQuestButton = 104;

		private List<List<IQuest>> pages;

		public List<ClickableComponent> questLogButtons;

		private int currentPage;

		private int questPage = -1;

		public ClickableTextureComponent forwardButton;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent rewardBox;

		public ClickableTextureComponent cancelQuestButton;

		private float widthMod;

		private float heightMod;

		private Rectangle clipBox;

		private int boxHeight;

		private int boxWidth;

		private int entryHeight;

		private int entryX;

		private int entryWidth;

		private int expandedEntryYAddon;

		private int extraY;

		private int currentEntry;

		public MobileScrollbar newScrollbar;

		public MobileScrollbox scrollArea;

		private List<IQuest> quests;

		private bool scrollbarVisible;

		private bool ignoreClickRelease;

		private bool cancelButtonHeld;

		private bool scrollBarClicked;

		private string cancelQuestText;

		private int cancelQuestLength;

		private int _currentSelectedQuestIndex = -1;

		public QuestLog(int showQuestIndex = -1)
			: base(0, 0, 0, 0, showUpperRightCloseButton: true)
		{
			Game1.playSound("bigSelect");
			questLogButtons = new List<ClickableComponent>();
			initializeBounds(showQuestIndex);
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (quests.Count == 0)
			{
				return;
			}
			if (_currentSelectedQuestIndex == -1)
			{
				if (b == Buttons.DPadUp || b == Buttons.LeftThumbstickUp || b == Buttons.DPadDown || b == Buttons.LeftThumbstickDown)
				{
					_currentSelectedQuestIndex = Math.Min(0, quests.Count - 1);
				}
			}
			else if (b == Buttons.A || (b == Buttons.X && _currentSelectedQuestIndex > 0))
			{
				if (_currentSelectedQuestIndex > -1 && !quests[_currentSelectedQuestIndex].ShouldDisplayAsComplete() && quests[_currentSelectedQuestIndex].CanBeCancelled())
				{
					return;
				}
				if (_currentSelectedQuestIndex > -1 && quests[_currentSelectedQuestIndex].ShouldDisplayAsComplete() && quests[_currentSelectedQuestIndex].HasMoneyReward())
				{
					Game1.player.Money += quests[_currentSelectedQuestIndex].GetMoneyReward();
					Game1.playSound("purchaseRepeat");
					quests[_currentSelectedQuestIndex].OnMoneyRewardClaimed();
					currentEntry = (_currentSelectedQuestIndex = -1);
					setUpQuests();
					recalculateButtonPositions();
					return;
				}
			}
			else
			{
				switch (b)
				{
				case Buttons.DPadUp:
				case Buttons.LeftThumbstickUp:
					_currentSelectedQuestIndex--;
					if (_currentSelectedQuestIndex < 0)
					{
						_currentSelectedQuestIndex = Math.Min(0, quests.Count - 1);
					}
					break;
				case Buttons.DPadDown:
				case Buttons.LeftThumbstickDown:
					_currentSelectedQuestIndex++;
					if (_currentSelectedQuestIndex >= questLogButtons.Count)
					{
						_currentSelectedQuestIndex = questLogButtons.Count - 1;
					}
					break;
				}
			}
			recalculateButtonPositions();
			if (_currentSelectedQuestIndex < 0)
			{
				_currentSelectedQuestIndex = Math.Min(0, quests.Count - 1);
			}
			if (_currentSelectedQuestIndex > -1)
			{
				ToggleShowQuest(_currentSelectedQuestIndex);
			}
			currentEntry = _currentSelectedQuestIndex;
			if (scrollbarVisible)
			{
				scrollArea.setYOffsetForScroll(-currentEntry * entryHeight);
			}
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if ((Game1.options.gamepadControls && direction == -1) || direction == 1)
			{
				direction *= 100;
			}
			base.receiveScrollWheelAction(direction);
			scrollArea.receiveScrollWheelAction(direction);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void leftClickHeld(int x, int y)
		{
			if (Game1.options.SnappyMenus || GameMenu.forcePreventClose)
			{
				return;
			}
			base.leftClickHeld(x, y);
			_currentSelectedQuestIndex = -1;
			int num = 0;
			if (scrollbarVisible)
			{
				num = scrollArea.getYOffsetForScroll();
			}
			if (currentEntry > -1 && !quests[currentEntry].ShouldDisplayAsComplete() && quests[currentEntry].CanBeCancelled() && cancelQuestButton.containsPoint(x, y - num))
			{
				cancelButtonHeld = true;
				return;
			}
			cancelButtonHeld = false;
			if (scrollBarClicked && (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y)))
			{
				SetScrollAreaPosition(y);
			}
			scrollArea.leftClickHeld(x, y);
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (Game1.options.SnappyMenus)
			{
				return;
			}
			scrollBarClicked = true;
			cancelButtonHeld = false;
			if (ignoreClickRelease)
			{
				ignoreClickRelease = false;
				return;
			}
			base.releaseLeftClick(x, y);
			if (currentEntry > -1)
			{
				int num = 0;
				if (scrollbarVisible)
				{
					num = scrollArea.getYOffsetForScroll();
				}
				if (!quests[currentEntry].ShouldDisplayAsComplete() && quests[currentEntry].CanBeCancelled() && cancelQuestButton.containsPoint(x, y - num))
				{
					Quest quest = quests[currentEntry] as Quest;
					quest.accepted.Value = false;
					quest.destroy.Value = true;
					currentEntry = -1;
					Game1.playSound("trashcan");
					setUpQuests();
					recalculateButtonPositions();
					return;
				}
			}
			if (!scrollArea.havePanelScrolled)
			{
				int num2 = 0;
				if (scrollbarVisible)
				{
					num2 = scrollArea.getYOffsetForScroll();
				}
				for (int i = 0; i < questLogButtons.Count; i++)
				{
					if (questLogButtons[i].containsPoint(x, y - num2))
					{
						_currentSelectedQuestIndex = i;
						ToggleShowQuest(i);
						return;
					}
				}
			}
			scrollArea.releaseLeftClick(x, y);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.options.SnappyMenus)
			{
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			_currentSelectedQuestIndex = -1;
			if (Game1.activeClickableMenu == null)
			{
				return;
			}
			ignoreClickRelease = false;
			int num = 0;
			if (scrollbarVisible)
			{
				num = scrollArea.getYOffsetForScroll();
				if (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y))
				{
					scrollBarClicked = true;
				}
			}
			if (currentEntry > -1 && !quests[currentEntry].ShouldDisplayAsComplete() && quests[currentEntry].CanBeCancelled() && cancelQuestButton.containsPoint(x, y - num))
			{
				cancelButtonHeld = true;
				Game1.playSound("smallSelect");
			}
			else if (currentEntry > -1 && rewardBox.containsPoint(x, y) && quests[currentEntry].ShouldDisplayAsComplete() && quests[currentEntry].HasMoneyReward())
			{
				Game1.player.Money += quests[currentEntry].GetMoneyReward();
				Game1.playSound("purchaseRepeat");
				quests[currentEntry].OnMoneyRewardClaimed();
				initializeBounds();
				recalculateButtonPositions();
				ignoreClickRelease = true;
			}
			else
			{
				scrollArea.receiveLeftClick(x, y);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			scrollArea.update(time);
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11373"), Game1.uiViewport.Width / 2, yPositionOnScreen / 2 - 24);
			int num = (scrollbarVisible ? scrollArea.getYOffsetForScroll() : 0);
			IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, boxWidth, boxHeight, Color.White);
			scrollArea.setUpForScrollBoxDrawing(b);
			for (int i = 0; i < questLogButtons.Count; i++)
			{
				if (questLogButtons[i].bounds.Y + questLogButtons[i].bounds.Height + num <= 0 || questLogButtons[i].bounds.Y + num >= Game1.uiViewport.Height)
				{
					continue;
				}
				Color color = Color.White;
				if (_currentSelectedQuestIndex == i || (_currentSelectedQuestIndex == -1 && questLogButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY() - num)))
				{
					color = Color.Wheat;
				}
				IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(72, 101, 15, 15), questLogButtons[i].bounds.X, questLogButtons[i].bounds.Y + num, questLogButtons[i].bounds.Width, questLogButtons[i].bounds.Height - 8, color, 4f, drawShadow: false);
				if (quests[i].ShouldDisplayAsNew() || quests[i].ShouldDisplayAsComplete())
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(questLogButtons[i].bounds.X + 64 + 20, questLogButtons[i].bounds.Y + 44 + num), new Rectangle(quests[i].ShouldDisplayAsComplete() ? 341 : 317, 410, 23, 9), Color.White, 0f, new Vector2(11f, 4f), 4f + Game1.dialogueButtonScale * 10f / 250f, flipped: false, 0.99f);
				}
				else
				{
					Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(questLogButtons[i].bounds.X + 32 + 16, questLogButtons[i].bounds.Y + 28 + num), quests[i].IsTimedQuest() ? new Rectangle(410, 501, 9, 9) : new Rectangle(395 + (quests[i].IsTimedQuest() ? 3 : 0), 497, 3, 8), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.99f);
					if (quests[i].GetDaysLeft() > 0)
					{
						if ((double)widthMod >= 0.9)
						{
							Utility.drawTextWithShadow(b, (quests[i].GetDaysLeft() > 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11374", quests[i].GetDaysLeft()) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest_FinalDay"), Game1.dialogueFont, new Vector2(questLogButtons[i].bounds.X + 96, questLogButtons[i].bounds.Y + 24 + num), Game1.textColor);
						}
						else
						{
							Utility.drawTextWithShadow(b, quests[i].GetDaysLeft().ToString() ?? "", Game1.dialogueFont, new Vector2(questLogButtons[i].bounds.X + 96, questLogButtons[i].bounds.Y + 24 + num), Game1.textColor);
						}
					}
				}
				SpriteText.drawStringHorizontallyCenteredAt(b, quests[i].GetName(), questLogButtons[i].bounds.X + questLogButtons[i].bounds.Width / 2, questLogButtons[i].bounds.Y + 24 + num);
				if (i != currentEntry)
				{
					continue;
				}
				int num2 = questLogButtons[i].bounds.Y + 96 + num;
				num2 = Utility.drawMultiLineTextWithShadow(b, quests[i].GetDescription(), Game1.smallFont, new Vector2(questLogButtons[i].bounds.X + 32, num2), questLogButtons[i].bounds.Width - 64, 128, Game1.textColor, centreY: false);
				if (quests[i].ShouldDisplayAsComplete())
				{
					string s = Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11376") + " " + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", quests[i].GetMoneyReward());
					SpriteText.drawStringHorizontallyCenteredAt(b, s, questLogButtons[i].bounds.X + questLogButtons[i].bounds.Width / 2, num2);
					rewardBox.bounds.X = questLogButtons[i].bounds.X + (questLogButtons[i].bounds.Width - rewardBox.bounds.Width) / 2;
					rewardBox.bounds.Y = num2 + 48;
					if (quests[i].HasMoneyReward())
					{
						rewardBox.draw(b);
						b.Draw(Game1.mouseCursors, new Vector2(rewardBox.bounds.X + 16, (float)(rewardBox.bounds.Y + 16) - Game1.dialogueButtonScale / 2f), new Rectangle(280, 410, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					}
					continue;
				}
				SpriteFont spriteFont = ((!(widthMod > 1f)) ? Game1.smallFont : Game1.dialogueFont);
				IQuest quest = quests[i];
				ClickableComponent clickableComponent = questLogButtons[i];
				List<string> objectiveDescriptions = quest.GetObjectiveDescriptions();
				for (int j = 0; j < objectiveDescriptions.Count; j++)
				{
					bool flag = false;
					if (quest is SpecialOrder)
					{
						flag = true;
					}
					string text = objectiveDescriptions[j];
					int num3 = clickableComponent.bounds.Width - 192;
					string text2 = Game1.parseText(text, spriteFont, num3);
					bool flag2 = false;
					if (quest is SpecialOrder)
					{
						OrderObjective orderObjective = (quest as SpecialOrder).objectives[j];
						flag2 = orderObjective.IsComplete();
					}
					Vector2 vector = spriteFont.MeasureString(text2);
					int num4 = (int)vector.X + 16;
					if (!flag2)
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(clickableComponent.bounds.X + (clickableComponent.bounds.Width - num4) / 2, num2 + 12), new Rectangle(412, 495, 5, 4), Color.White, (float)Math.PI / 2f, Vector2.Zero);
					}
					Color color2 = Color.DarkBlue;
					if (flag2)
					{
						color2 = Game1.unselectedOptionColor;
					}
					num2 += ((spriteFont == Game1.smallFont) ? 8 : 0);
					Utility.drawTextWithShadow(b, text2, spriteFont, new Vector2(clickableComponent.bounds.X + (clickableComponent.bounds.Width - num4) / 2 + 16, num2), color2);
					num2 += (int)vector.Y;
					if (!(quest is SpecialOrder))
					{
						continue;
					}
					OrderObjective orderObjective2 = (quest as SpecialOrder).objectives[j];
					if (orderObjective2.GetMaxCount() > 1 && orderObjective2.ShouldShowProgress())
					{
						Color color3 = Color.DarkRed;
						Color color4 = Color.Red;
						if (orderObjective2.GetCount() >= orderObjective2.GetMaxCount())
						{
							color4 = Color.LimeGreen;
							color3 = Color.Green;
						}
						int num5 = 128;
						int num6 = 160;
						int num7 = 4;
						Rectangle rectangle = new Rectangle(0, 224, 47, 12);
						Rectangle value = new Rectangle(47, 224, 1, 12);
						int num8 = 3;
						int num9 = 3;
						int num10 = 5;
						string text3 = orderObjective2.GetCount() + "/" + orderObjective2.GetMaxCount();
						int num11 = (int)spriteFont.MeasureString(orderObjective2.GetMaxCount() + "/" + orderObjective2.GetMaxCount()).X;
						Vector2 vector2 = spriteFont.MeasureString(text3);
						int num12 = (int)vector2.X;
						Rectangle bounds = clickableComponent.bounds;
						int num13 = bounds.Width - num5 * 2 - num6;
						int num14 = bounds.X + bounds.Width - num5 - num11;
						int num15 = 6;
						Rectangle rectangle2 = new Rectangle(bounds.X + num15 + (bounds.Width - num13) / 2, num2 + 2, num13, rectangle.Height * 4);
						Utility.drawTextWithShadow(b, text3, spriteFont, new Vector2(rectangle2.X + num13 + num15, rectangle2.Y + (int)(vector2.Y / 4f)), Color.DarkBlue);
						if (rectangle2.Right > num14 - 16)
						{
							int num16 = rectangle2.Right - (num14 - 16);
							rectangle2.Width -= num16;
						}
						b.Draw(Game1.mouseCursors2, new Rectangle(rectangle2.X, rectangle2.Y, num10 * 4, rectangle2.Height), new Rectangle(rectangle.X, rectangle.Y, num10, rectangle.Height), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
						b.Draw(Game1.mouseCursors2, new Rectangle(rectangle2.X + num10 * 4, rectangle2.Y, rectangle2.Width - 2 * num10 * 4, rectangle2.Height), new Rectangle(rectangle.X + num10, rectangle.Y, rectangle.Width - 2 * num10, rectangle.Height), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
						b.Draw(Game1.mouseCursors2, new Rectangle(rectangle2.Right - num10 * 4, rectangle2.Y, num10 * 4, rectangle2.Height), new Rectangle(rectangle.Right - num10, rectangle.Y, num10, rectangle.Height), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
						float num17 = (float)orderObjective2.GetCount() / (float)orderObjective2.GetMaxCount();
						if (orderObjective2.GetMaxCount() < num7)
						{
							num7 = orderObjective2.GetMaxCount();
						}
						rectangle2.X += 4 * num8;
						rectangle2.Width -= 4 * num8 * 2;
						for (int k = 1; k < num7; k++)
						{
							b.Draw(Game1.mouseCursors2, new Vector2((float)rectangle2.X + (float)rectangle2.Width * ((float)k / (float)num7), rectangle2.Y), value, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
						}
						rectangle2.Y += 4 * num9;
						rectangle2.Height -= 4 * num9 * 2;
						Rectangle destinationRectangle = new Rectangle(rectangle2.X, rectangle2.Y, (int)((float)rectangle2.Width * num17) - 4, rectangle2.Height);
						b.Draw(Game1.staminaRect, destinationRectangle, null, color4, 0f, Vector2.Zero, SpriteEffects.None, (float)destinationRectangle.Y / 10000f);
						destinationRectangle.X = destinationRectangle.Right;
						destinationRectangle.Width = 4;
						b.Draw(Game1.staminaRect, destinationRectangle, null, color3, 0f, Vector2.Zero, SpriteEffects.None, (float)destinationRectangle.Y / 10000f);
						num2 += (rectangle.Height + 2) * 4;
					}
				}
				if (quests[i].CanBeCancelled())
				{
					IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.smallFont, Game1.mouseCursors, new Rectangle(256, 256, 10, 10), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), cancelQuestText, cancelQuestButton.bounds.X - (cancelButtonHeld ? 4 : 0), cancelQuestButton.bounds.Y + num + (cancelButtonHeld ? 4 : 0), cancelQuestButton.bounds.Width, cancelQuestButton.bounds.Height, Color.White, 4f, !cancelButtonHeld);
				}
			}
			scrollArea.finishScrollBoxDrawing(b);
			if (scrollbarVisible)
			{
				newScrollbar.draw(b);
			}
			base.draw(b);
			Game1.mouseCursorTransparency = 1f;
			if (!Game1.options.SnappyMenus)
			{
				drawMouse(b);
			}
		}

		private void makeDetailForEntry(int entry)
		{
			int num = -1;
			bool flag = currentEntry != entry;
			if (currentEntry == entry)
			{
				num = entry;
				entry = -1;
				if (scrollbarVisible && quests.Count <= questsPerPage - 2)
				{
					scrollbarVisible = false;
					recalculateButtonPositions();
				}
			}
			else if ((!scrollbarVisible && quests.Count > questsPerPage - 2) || (quests.Count > questsPerPage - 3 && currentEntry != -1))
			{
				scrollbarVisible = true;
				recalculateButtonPositions();
			}
			currentEntry = entry;
			for (int i = 0; i < questLogButtons.Count; i++)
			{
				questLogButtons[i].bounds.Y = yPositionOnScreen + 32 + i * entryHeight;
				questLogButtons[i].bounds.Height = entryHeight;
			}
			if (entry > -1)
			{
				extraY = Utility.drawMultiLineTextWithShadow(null, quests[entry].GetDescription(), Game1.smallFont, new Vector2(0f, 0f), questLogButtons[entry].bounds.Width - 64, 128, Game1.textColor, centreY: false, actuallyDrawIt: false);
				SpriteFont spriteFont = ((widthMod > 1f) ? Game1.dialogueFont : Game1.smallFont);
				Vector2 vector = spriteFont.MeasureString("W");
				if (quests[entry] is SpecialOrder specialOrder)
				{
					extraY += specialOrder.GetObjectiveDescriptions().Count * ((int)vector.Y + 8);
					foreach (OrderObjective objective in specialOrder.objectives)
					{
						if (objective.GetMaxCount() > 1 && objective.ShouldShowProgress())
						{
							extraY += 56;
						}
					}
				}
				else
				{
					extraY += (int)vector.Y;
					if (!quests[entry].ShouldDisplayAsComplete() && quests[entry].CanBeCancelled())
					{
						extraY += cancelQuestButton.bounds.Height + 16;
					}
					else if (quests[entry].ShouldDisplayAsComplete() && quests[entry].HasMoneyReward())
					{
						extraY += rewardBox.bounds.Height + 16;
					}
				}
				extraY += 42;
				for (int j = entry + 1; j < questLogButtons.Count; j++)
				{
					questLogButtons[j].bounds.Y += extraY;
				}
				questLogButtons[entry].bounds.Height += extraY;
				int num2 = cancelQuestLength + 32;
				cancelQuestButton.bounds.X = questLogButtons[entry].bounds.X + (questLogButtons[entry].bounds.Width - ((!scrollbarVisible) ? 24 : 0) - num2 - 16);
				cancelQuestButton.bounds.Width = num2;
				cancelQuestButton.bounds.Height = 60;
				cancelQuestButton.bounds.Y = questLogButtons[entry].bounds.Y + questLogButtons[entry].bounds.Height - 84;
			}
			scrollArea.setMaxYOffset((quests.Count - questsPerPage) * entryHeight + 32 + ((currentEntry > -1) ? extraY : 0));
			if (currentEntry >= questLogButtons.Count - 1)
			{
				scrollArea.setYOffsetForScroll(-scrollArea.getMaxYOffset());
			}
			else if (flag && num != -1 && questLogButtons[num] != null)
			{
				SetScrollAreaPosition(questLogButtons[num].bounds.Y);
			}
		}

		private void SetScrollAreaPosition(int y)
		{
			if (scrollbarVisible)
			{
				float num = newScrollbar.setY(y);
				int yOffsetForScroll = (int)((0f - num) * (float)((quests.Count - questsPerPage) * entryHeight + 32 + ((currentEntry > -1) ? extraY : 0)) / 100f);
				scrollArea.setYOffsetForScroll(yOffsetForScroll);
			}
		}

		private void initializeBounds(int showQuestIndex = -1)
		{
			widthMod = (float)Game1.uiViewport.Width / 1280f;
			heightMod = (float)Game1.uiViewport.Height / 720f;
			xPositionOnScreen = (int)(56f * widthMod) + Game1.xEdge;
			yPositionOnScreen = (int)(100f * heightMod);
			boxWidth = (int)(1176f * widthMod) - Game1.xEdge * 2;
			boxHeight = (int)(608f * heightMod);
			entryHeight = 100;
			expandedEntryYAddon = 212;
			extraY = 0;
			currentEntry = -1;
			entryX = xPositionOnScreen + 32;
			questLogButtons.Clear();
			setUpQuests(showQuestIndex);
			cancelButtonHeld = false;
			questsPerPage = boxHeight / entryHeight;
			if (quests.Count > questsPerPage - 2)
			{
				scrollbarVisible = true;
			}
			else
			{
				scrollbarVisible = false;
			}
			entryWidth = boxWidth - 88 + ((!scrollbarVisible) ? 24 : 0);
			initializeUpperRightCloseButton();
			scrollBarClicked = false;
			for (int i = 0; i < quests.Count; i++)
			{
				questLogButtons.Add(new ClickableComponent(new Rectangle(entryX, yPositionOnScreen + 32 + i * entryHeight, entryWidth, entryHeight), i.ToString() ?? ""));
			}
			rewardBox = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width / 2 - 80, yPositionOnScreen + height - 32 - 96, 96, 96), Game1.mouseCursors, new Rectangle(293, 360, 24, 24), 4f, drawShadow: true);
			cancelQuestButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 4, yPositionOnScreen + height + 4, 400, 120), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 4f, drawShadow: true);
			clipBox = new Rectangle(xPositionOnScreen, yPositionOnScreen + 16, boxWidth, boxHeight - 32);
			newScrollbar = new MobileScrollbar(xPositionOnScreen + boxWidth - 60, clipBox.Y, 55, clipBox.Height - 4, 0, 32);
			scrollArea = new MobileScrollbox(entryX, yPositionOnScreen, entryWidth, boxHeight, (quests.Count - questsPerPage) * entryHeight + 32, clipBox, newScrollbar);
			cancelQuestText = Game1.content.LoadString("Strings\\StringsFromCSFiles:QuestLog.cs.11364");
			cancelQuestLength = (int)Game1.dialogueFont.MeasureString(cancelQuestText).X;
			scrollArea.setMaxYOffset((quests.Count - questsPerPage) * entryHeight + 32 + ((currentEntry > -1) ? (expandedEntryYAddon + extraY) : 0));
			scrollArea.setYOffsetForScroll(0);
		}

		private void setUpQuests(int showQuestIndex = -1)
		{
			currentEntry = -1;
			if (quests == null)
			{
				quests = new List<IQuest>();
			}
			else
			{
				quests.Clear();
			}
			for (int num = Game1.player.team.specialOrders.Count - 1; num >= 0; num--)
			{
				if (!Game1.player.team.specialOrders[num].IsHidden())
				{
					quests.Add(Game1.player.team.specialOrders[num]);
				}
			}
			for (int num2 = Game1.player.questLog.Count - 1; num2 >= 0; num2--)
			{
				if (Game1.player.questLog[num2] == null || (bool)Game1.player.questLog[num2].destroy)
				{
					Game1.player.questLog.RemoveAt(num2);
					if (questLogButtons != null && questLogButtons.Count > num2 && questLogButtons[num2] != null)
					{
						questLogButtons.RemoveAt(num2);
					}
				}
				else if (Game1.player.questLog[num2] == null || !Game1.player.questLog[num2].isSecretQuest())
				{
					int num3 = Game1.player.questLog.Count - 1 - num2;
					quests.Add(Game1.player.questLog[num2]);
				}
			}
			if (quests != null)
			{
				if (quests.Count >= 1)
				{
					_ = quests[0];
				}
				if (quests.Count >= 2)
				{
					_ = quests[1];
				}
			}
			TutorialManager.Instance.completeTutorial(tutorialType.TAP_JOURNAL);
		}

		public void recalculateButtonPositions()
		{
			if (quests.Count > questsPerPage - 2 || (quests.Count > questsPerPage - 3 && currentEntry != -1))
			{
				scrollbarVisible = true;
			}
			else
			{
				scrollbarVisible = false;
			}
			for (int i = 0; i < quests.Count; i++)
			{
				entryWidth = boxWidth - 88 + ((!scrollbarVisible) ? 24 : 0);
				questLogButtons[i].bounds.Width = entryWidth;
				questLogButtons[i].bounds.Y = yPositionOnScreen + 32 + i * entryHeight;
				questLogButtons[i].bounds.Height = entryHeight;
			}
			currentEntry = -1;
		}

		private void ToggleShowQuest(int i)
		{
			if (i >= 0 && i < quests.Count)
			{
				Game1.playSound("smallSelect");
				quests[i].MarkAsViewed();
				makeDetailForEntry(i);
				if (!TutorialManager.Instance.hasOpenedJournalEntry)
				{
					TutorialManager.Instance.hasOpenedJournalEntry = true;
					TutorialManager.Instance.completeTutorial(tutorialType.JOURNAL_INFO);
				}
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			initializeBounds();
		}
	}
}
