using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;

namespace StardewValley.Menus
{
	public class PondQueryMenu : IClickableMenu
	{
		public const int region_okButton = 101;

		public const int region_emptyButton = 103;

		public const int region_noButton = 105;

		public const int region_nettingButton = 106;

		public new static int width = 384;

		public new static int height = 512;

		public const int unresolved_needs_extra_height = 116;

		protected FishPond _pond;

		protected Object _fishItem;

		protected string _statusText = "";

		public ClickableTextureComponent emptyButton;

		public ClickableTextureComponent yesButton;

		public ClickableTextureComponent noButton;

		public ClickableTextureComponent changeNettingButton;

		private bool confirmingEmpty;

		protected Rectangle _confirmationBoxRectangle;

		protected string _confirmationText;

		protected float _age;

		private string hoverText = "";

		public bool yesButtonHeld;

		public bool noButtonHeld;

		public bool changeNettingButtonHeld;

		public bool emptyButtonHeld;

		public PondQueryMenu(FishPond fish_pond)
		{
			xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2 - 128;
			initializeUpperRightCloseButton();
			Game1.player.Halt();
			width = 384;
			height = 512;
			_pond = fish_pond;
			_fishItem = new Object(_pond.fishType.Value, 1);
			UpdateState();
			yPositionOnScreen = Game1.uiViewport.Height / 2 - measureTotalHeight() / 2 - 64;
			_confirmationBoxRectangle = new Rectangle(0, 0, 400, 100);
			_confirmationBoxRectangle.X = Game1.uiViewport.Width / 2 - _confirmationBoxRectangle.Width / 2;
			_confirmationText = Game1.content.LoadString("Strings\\UI:PondQuery_ConfirmEmpty");
			_confirmationText = Game1.parseText(_confirmationText, Game1.smallFont, _confirmationBoxRectangle.Width);
			Vector2 vector = Game1.smallFont.MeasureString(_confirmationText);
			_confirmationBoxRectangle.Height = (int)vector.Y;
			_confirmationBoxRectangle.Y = Game1.uiViewport.Height / 2 - _confirmationBoxRectangle.Height / 2;
			emptyButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + measureTotalHeight() / 2 - 16, 64, 64), Game1.mouseCursors, new Rectangle(32, 384, 16, 16), 4f)
			{
				myID = 103,
				downNeighborID = -99998
			};
			changeNettingButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + measureTotalHeight() / 2 + 64, 64, 64), Game1.mouseCursors, new Rectangle(48, 384, 16, 16), 4f)
			{
				myID = 106,
				downNeighborID = -99998,
				upNeighborID = -99998
			};
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(101);
			snapCursorToCurrentSnappedComponent();
		}

		public void textBoxEnter(TextBox sender)
		{
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.globalFade)
			{
				return;
			}
			if (Game1.options.menuButton.Contains(new InputButton(key)))
			{
				Game1.playSound("smallSelect");
				if (readyToClose())
				{
					Game1.exitActiveMenu();
				}
			}
			else if (Game1.options.SnappyMenus && !Game1.options.menuButton.Contains(new InputButton(key)))
			{
				base.receiveKeyPress(key);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			_age += (float)time.ElapsedGameTime.TotalSeconds;
		}

		public void finishedPlacingAnimal()
		{
			Game1.exitActiveMenu();
			Game1.currentLocation = Game1.player.currentLocation;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear();
			Game1.displayHUD = true;
			Game1.viewportFreeze = false;
			Game1.displayFarmer = true;
			Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:AnimalQuery_Moving_HomeChanged"), Color.LimeGreen, 3500f));
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (Game1.globalFade)
			{
				return;
			}
			base.releaseLeftClick(x, y);
			if (confirmingEmpty)
			{
				if (yesButtonHeld && yesButton.containsPoint(x, y))
				{
					Game1.playSound("fishSlap");
					_pond.ClearPond();
					exitThisMenu();
					yesButtonHeld = (noButtonHeld = (changeNettingButtonHeld = (emptyButtonHeld = false)));
					return;
				}
				if (noButtonHeld && noButton.containsPoint(x, y))
				{
					confirmingEmpty = false;
					Game1.playSound("smallSelect");
					if (Game1.options.SnappyMenus)
					{
						currentlySnappedComponent = getComponentWithID(103);
						snapCursorToCurrentSnappedComponent();
					}
				}
			}
			else
			{
				if (upperRightCloseButton != null && upperRightCloseButton.containsPoint(x, y) && readyToClose())
				{
					Game1.exitActiveMenu();
					Game1.playSound("smallSelect");
				}
				if (changeNettingButtonHeld && changeNettingButton.containsPoint(x, y))
				{
					Game1.playSound("drumkit6");
					_pond.nettingStyle.Value++;
					_pond.nettingStyle.Value %= 4;
				}
				else if (emptyButtonHeld && emptyButton.containsPoint(x, y))
				{
					confirmingEmpty = true;
					yesButton = new ClickableTextureComponent(new Rectangle(_confirmationBoxRectangle.Center.X - 100, _confirmationBoxRectangle.Bottom + 40, 80, 80), Game1.mobileSpriteSheet, new Rectangle(0, 0, 20, 20), 4f)
					{
						myID = 111,
						rightNeighborID = 105
					};
					noButton = new ClickableTextureComponent(new Rectangle(_confirmationBoxRectangle.Center.X + 20, _confirmationBoxRectangle.Bottom + 40, 80, 80), Game1.mobileSpriteSheet, new Rectangle(20, 0, 20, 20), 4f)
					{
						myID = 105,
						leftNeighborID = 111
					};
					Game1.playSound("smallSelect");
					if (Game1.options.SnappyMenus)
					{
						populateClickableComponentList();
						currentlySnappedComponent = noButton;
						snapCursorToCurrentSnappedComponent();
					}
					yesButtonHeld = (noButtonHeld = (changeNettingButtonHeld = (emptyButtonHeld = false)));
					return;
				}
			}
			yesButtonHeld = (noButtonHeld = (changeNettingButtonHeld = (emptyButtonHeld = false)));
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (!Game1.globalFade)
			{
				if (yesButton != null && yesButton.containsPoint(x, y))
				{
					yesButtonHeld = true;
				}
				else if (noButton != null && noButton.containsPoint(x, y))
				{
					noButtonHeld = true;
				}
				else if (changeNettingButton.containsPoint(x, y))
				{
					changeNettingButtonHeld = true;
				}
				else if (emptyButton.containsPoint(x, y))
				{
					emptyButtonHeld = true;
				}
			}
		}

		public override bool readyToClose()
		{
			if (base.readyToClose())
			{
				return !Game1.globalFade;
			}
			return false;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			if (!Game1.globalFade && readyToClose())
			{
				Game1.exitActiveMenu();
				Game1.playSound("smallSelect");
			}
		}

		public override void performHoverAction(int x, int y)
		{
			hoverText = "";
			if (emptyButton != null)
			{
				if (emptyButton.containsPoint(x, y))
				{
					emptyButton.scale = Math.Min(4.1f, emptyButton.scale + 0.05f);
					hoverText = Game1.content.LoadString("Strings\\UI:PondQuery_EmptyPond", 10);
				}
				else
				{
					emptyButton.scale = Math.Max(4f, emptyButton.scale - 0.05f);
				}
			}
			if (changeNettingButton != null)
			{
				if (changeNettingButton.containsPoint(x, y))
				{
					changeNettingButton.scale = Math.Min(4.1f, changeNettingButton.scale + 0.05f);
					hoverText = Game1.content.LoadString("Strings\\UI:PondQuery_ChangeNetting", 10);
				}
				else
				{
					changeNettingButton.scale = Math.Max(4f, emptyButton.scale - 0.05f);
				}
			}
			if (yesButton != null)
			{
				if (yesButton.containsPoint(x, y))
				{
					yesButton.scale = Math.Min(4.4f, yesButton.scale + 0.05f);
				}
				else
				{
					yesButton.scale = Math.Max(4f, yesButton.scale - 0.05f);
				}
			}
			if (noButton != null)
			{
				if (noButton.containsPoint(x, y))
				{
					noButton.scale = Math.Min(4.4f, noButton.scale + 0.05f);
				}
				else
				{
					noButton.scale = Math.Max(4f, noButton.scale - 0.05f);
				}
			}
		}

		public static string getCompletedRequestString(FishPond pond, Object fishItem, Random r)
		{
			if (fishItem != null && fishItem.GetContextTagList().Contains("fish_talk_rude"))
			{
				return Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestComplete_Rude" + r.Next(3), pond.neededItem.Value.DisplayName));
			}
			if (fishItem != null && fishItem.GetContextTagList().Contains("fish_talk_stiff"))
			{
				return Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestComplete_Stiff" + r.Next(3), pond.neededItem.Value.DisplayName));
			}
			if (fishItem != null && fishItem.GetContextTagList().Contains("fish_talk_demanding"))
			{
				return Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestComplete_Demanding" + r.Next(3), pond.neededItem.Value.DisplayName));
			}
			if (fishItem != null && fishItem.GetContextTagList().Contains("fish_carnivorous"))
			{
				return Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestComplete_Carnivore" + r.Next(3), pond.neededItem.Value.DisplayName));
			}
			return Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestComplete" + r.Next(7), pond.neededItem.Value.DisplayName);
		}

		public void UpdateState()
		{
			Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)_pond.seedOffset);
			if (_pond.currentOccupants.Value <= 0)
			{
				_statusText = Game1.content.LoadString("Strings\\UI:PondQuery_StatusNoFish");
				return;
			}
			if (_pond.neededItem.Value != null)
			{
				if ((bool)_pond.hasCompletedRequest)
				{
					_statusText = getCompletedRequestString(_pond, _fishItem, random);
					return;
				}
				if (_pond.HasUnresolvedNeeds())
				{
					string text = _pond.neededItemCount.Value.ToString() ?? "";
					if (_pond.neededItemCount.Value <= 1)
					{
						text = Lexicon.getProperArticleForWord(_pond.neededItem.Value.DisplayName);
						if (text == "")
						{
							text = Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestOneCount");
						}
					}
					if (_fishItem != null && _fishItem.GetContextTagList().Contains("fish_talk_rude"))
					{
						_statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending_Rude" + random.Next(3) + "_" + (Game1.player.isMale ? "Male" : "Female"), Lexicon.makePlural(_pond.neededItem.Value.DisplayName, _pond.neededItemCount.Value == 1), text, _pond.neededItem.Value.DisplayName));
					}
					else if (_fishItem != null && _fishItem.GetContextTagList().Contains("fish_talk_stiff"))
					{
						_statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending_Stiff" + random.Next(3), Lexicon.makePlural(_pond.neededItem.Value.DisplayName, _pond.neededItemCount.Value == 1), text, _pond.neededItem.Value.DisplayName));
					}
					else if (_fishItem != null && _fishItem.GetContextTagList().Contains("fish_talk_demanding"))
					{
						_statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending_Demanding" + random.Next(3), Lexicon.makePlural(_pond.neededItem.Value.DisplayName, _pond.neededItemCount.Value == 1), text, _pond.neededItem.Value.DisplayName));
					}
					else if (_fishItem != null && _fishItem.GetContextTagList().Contains("fish_carnivorous"))
					{
						_statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending_Carnivore" + random.Next(3), Lexicon.makePlural(_pond.neededItem.Value.DisplayName, _pond.neededItemCount.Value == 1), text, _pond.neededItem.Value.DisplayName));
					}
					else
					{
						_statusText = Lexicon.capitalize(Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequestPending" + random.Next(7), Lexicon.makePlural(_pond.neededItem.Value.DisplayName, _pond.neededItemCount.Value == 1), text, _pond.neededItem.Value.DisplayName));
					}
					return;
				}
			}
			if (_fishItem != null && ((int)_fishItem.parentSheetIndex == 397 || (int)_fishItem.parentSheetIndex == 393))
			{
				_statusText = Game1.content.LoadString("Strings\\UI:PondQuery_StatusOk_Coral", _fishItem.DisplayName);
			}
			else
			{
				_statusText = Game1.content.LoadString("Strings\\UI:PondQuery_StatusOk" + random.Next(7));
			}
		}

		private int measureTotalHeight()
		{
			return 644 + measureExtraTextHeight(getDisplayedText());
		}

		private int measureExtraTextHeight(string displayed_text)
		{
			return Math.Max(0, (int)Game1.smallFont.MeasureString(displayed_text).Y - 90) + 4;
		}

		private string getDisplayedText()
		{
			return Game1.parseText(_statusText, Game1.smallFont, width - IClickableMenu.spaceToClearSideBorder * 2 - 64);
		}

		public override void draw(SpriteBatch b)
		{
			if (!Game1.globalFade)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				bool flag = _pond.neededItem.Value != null && _pond.HasUnresolvedNeeds() && !_pond.hasCompletedRequest;
				string text = Game1.content.LoadString("Strings\\UI:PondQuery_Name", _fishItem.DisplayName);
				Vector2 vector = Game1.smallFont.MeasureString(text);
				SpriteText.drawStringWithScrollCenteredAt(b, text, Game1.uiViewport.Width / 2, yPositionOnScreen + 16 + 128);
				string displayedText = getDisplayedText();
				int num = 0;
				if (flag)
				{
					num += 116;
				}
				int num2 = measureExtraTextHeight(displayedText);
				Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen + 128, width, height - 128 + num + num2, speaker: false, drawOnlyBox: true);
				string text2 = Game1.content.LoadString("Strings\\UI:PondQuery_Population", _pond.FishCount.ToString() ?? "", _pond.maxOccupants);
				vector = Game1.smallFont.MeasureString(text2);
				Utility.drawTextWithShadow(b, text2, Game1.smallFont, new Vector2((float)(xPositionOnScreen + width / 2) - vector.X * 0.5f, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16 + 128 - 16), Game1.textColor);
				int num3 = _pond.maxOccupants;
				float num4 = 13f;
				int num5 = 0;
				int num6 = 0;
				for (int i = 0; i < num3; i++)
				{
					float num7 = (float)Math.Sin(_age * 1f + (float)num5 * 0.75f + (float)num6 * 0.25f) * 2f;
					if (i < _pond.FishCount)
					{
						_fishItem.drawInMenu(b, new Vector2((float)(xPositionOnScreen + width / 2) - num4 * (float)Math.Min(num3, 5) * 4f * 0.5f + num4 * 4f * (float)num5 - 12f, (float)(yPositionOnScreen + (int)(num7 * 4f)) + (float)(num6 * 4) * num4 + 275.2f), 0.75f, 1f, 0.089f, StackDrawType.Hide, Color.White, drawShadow: false);
					}
					else
					{
						_fishItem.drawInMenu(b, new Vector2((float)(xPositionOnScreen + width / 2) - num4 * (float)Math.Min(num3, 5) * 4f * 0.5f + num4 * 4f * (float)num5 - 12f, (float)(yPositionOnScreen + (int)(num7 * 4f)) + (float)(num6 * 4) * num4 + 275.2f), 0.75f, 0.35f, 0.089f, StackDrawType.Hide, Color.Black, drawShadow: false);
					}
					num5++;
					if (num5 == 5)
					{
						num5 = 0;
						num6++;
					}
				}
				vector = Game1.smallFont.MeasureString(displayedText);
				Utility.drawTextWithShadow(b, displayedText, Game1.smallFont, new Vector2((float)(xPositionOnScreen + width / 2) - vector.X * 0.5f, (float)(yPositionOnScreen + height + num2 - (flag ? 32 : 48)) - vector.Y), Game1.textColor);
				if (flag)
				{
					string text3 = Game1.content.LoadString("Strings\\UI:PondQuery_StatusRequest_Bring");
					vector = Game1.smallFont.MeasureString(text3);
					int num8 = xPositionOnScreen + 88;
					float num9 = num8;
					float num10 = num9 + vector.X + 4f;
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr)
					{
						num10 = num8 - 8;
						num9 = num8 + 76;
					}
					Utility.drawTextWithShadow(b, text3, Game1.smallFont, new Vector2(num9, yPositionOnScreen + height + num2 + 24), Game1.textColor);
					b.Draw(Game1.objectSpriteSheet, new Vector2(num10, yPositionOnScreen + height + num2 + 4), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, _pond.neededItem.Value.parentSheetIndex, 16, 16), Color.Black * 0.4f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.089f);
					b.Draw(Game1.objectSpriteSheet, new Vector2(num10 + 4f, yPositionOnScreen + height + num2), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, _pond.neededItem.Value.parentSheetIndex, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.089f);
					if ((int)_pond.neededItemCount > 1)
					{
						Utility.drawTinyDigits(_pond.neededItemCount, b, new Vector2(num10 + 48f, yPositionOnScreen + height + num2 + 48), 3f, 0.088f, Color.White);
					}
				}
				emptyButton.draw(b);
				changeNettingButton.draw(b);
				if (confirmingEmpty)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
					int num11 = 16;
					_confirmationBoxRectangle.Width += num11;
					_confirmationBoxRectangle.Height += num11;
					_confirmationBoxRectangle.X -= num11 / 2;
					_confirmationBoxRectangle.Y -= num11 / 2;
					Game1.DrawBox(_confirmationBoxRectangle.X, _confirmationBoxRectangle.Y, _confirmationBoxRectangle.Width, _confirmationBoxRectangle.Height);
					_confirmationBoxRectangle.Width -= num11;
					_confirmationBoxRectangle.Height -= num11;
					_confirmationBoxRectangle.X += num11 / 2;
					_confirmationBoxRectangle.Y += num11 / 2;
					b.DrawString(Game1.smallFont, _confirmationText, new Vector2(_confirmationBoxRectangle.X, _confirmationBoxRectangle.Y), Game1.textColor);
					yesButton.draw(b);
					noButton.draw(b);
				}
			}
			base.draw(b);
		}
	}
}
