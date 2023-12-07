using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	public class JojaCDMenu : IClickableMenu
	{
		public new const int width = 1280;

		public new const int height = 576;

		public const int buttonWidth = 147;

		public const int buttonHeight = 30;

		private Texture2D noteTexture;

		public List<ClickableComponent> checkboxes = new List<ClickableComponent>();

		private string hoverText;

		private bool boughtSomething;

		private Rectangle bottomBox;

		private Rectangle topBox;

		private float widthMod;

		private float heightMod;

		private int drawScale;

		private ClickableComponent buyButton;

		private Rectangle buyButtonBounds;

		private bool buyButtonIsHeld;

		private bool buyButtonShown;

		private int currentlySelectedBox;

		private string buyText;

		private int exitTimer = -1;

		private int _selectedItemIndex = -1;

		public JojaCDMenu(Texture2D noteTexture)
		{
			this.noteTexture = noteTexture;
			setUpMobileMenu();
			exitFunction = onExitFunction;
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
				Game1.mouseCursorTransparency = 1f;
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		private void onExitFunction()
		{
			if (boughtSomething)
			{
				try
				{
					JojaMart.Morris.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Morris_JojaCDConfirm"));
					Game1.drawDialogue(JojaMart.Morris);
				}
				catch (Exception)
				{
				}
			}
			Game1.player.forceCanMove();
		}

		public override void receiveGamePadButton(Buttons b)
		{
			switch (b)
			{
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
				currentlySelectedBox = Math.Max(0, currentlySelectedBox - 1);
				break;
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickRight:
				currentlySelectedBox = Math.Min(4, currentlySelectedBox + 1);
				break;
			case Buttons.DPadUp:
			case Buttons.LeftThumbstickUp:
				currentlySelectedBox = Math.Max(0, currentlySelectedBox - 2);
				break;
			case Buttons.DPadDown:
			case Buttons.LeftThumbstickDown:
				currentlySelectedBox = Math.Min(4, currentlySelectedBox + 2);
				break;
			case Buttons.A:
				if (buyButtonShown && currentlySelectedBox > -1)
				{
					OnClickBuyButton();
				}
				break;
			}
			base.receiveGamePadButton(b);
		}

		public override bool readyToClose()
		{
			return true;
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (exitTimer >= 0)
			{
				exitTimer -= time.ElapsedGameTime.Milliseconds;
				if (exitTimer <= 0)
				{
					exitThisMenu();
				}
			}
			Game1.mouseCursorTransparency = 1f;
		}

		public int getPriceFromButtonNumber(int buttonNumber)
		{
			return buttonNumber switch
			{
				0 => 40000, 
				1 => 15000, 
				2 => 25000, 
				3 => 35000, 
				4 => 20000, 
				_ => -1, 
			};
		}

		public string getDescriptionFromButtonNumber(int buttonNumber)
		{
			return Game1.content.LoadString("Strings\\UI:JojaCDMenu_Hover" + buttonNumber);
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			setUpMobileMenu();
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, 1280, 576), Color.Black * 0.75f);
			b.Draw(noteTexture, new Vector2(topBox.X, topBox.Y), new Rectangle(0, 0, 320, 144), Color.White, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0.0079f);
			IClickableMenu.drawTextureBox(b, bottomBox.X, bottomBox.Y, bottomBox.Width, bottomBox.Height, Color.White);
			if (buyButtonShown)
			{
				try
				{
					IClickableMenu.drawButtonWithText(b, Game1.dialogueFont, buyText, buyButton.bounds.X, buyButton.bounds.Y, buyButton.bounds.Width, buyButton.bounds.Height, Color.White, isClickable: true, buyButtonIsHeld);
				}
				catch (Exception)
				{
					IClickableMenu.drawButtonWithText(b, Game1.dialogueFont, "Buy", buyButton.bounds.X, buyButton.bounds.Y, buyButton.bounds.Width, buyButton.bounds.Height, Color.White, isClickable: true, buyButtonIsHeld);
				}
			}
			if (currentlySelectedBox != -1)
			{
				IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(20, 96, 20, 20), checkboxes[currentlySelectedBox].bounds.X, checkboxes[currentlySelectedBox].bounds.Y, checkboxes[currentlySelectedBox].bounds.Width, checkboxes[currentlySelectedBox].bounds.Height - drawScale, Color.White, 2 * drawScale, drawShadow: false);
				Utility.drawMultiLineTextWithShadow(b, getDescriptionFromButtonNumber(currentlySelectedBox), Game1.dialogueFont, new Vector2(bottomBox.X + 32, bottomBox.Y + 16), bottomBox.Width - 64 - (buyButtonShown ? (32 + buyButton.bounds.Width) : 0), bottomBox.Height - 32, Game1.textColor);
			}
			base.draw(b);
			foreach (ClickableComponent checkbox in checkboxes)
			{
				if (checkbox.name.Equals("complete"))
				{
					b.Draw(noteTexture, new Vector2(checkbox.bounds.Left + 4 * drawScale, checkbox.bounds.Y + 4 * drawScale), new Rectangle(0, 144, 16, 16), Color.White, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0.008f);
				}
			}
			Game1.dayTimeMoneyBox.drawMoneyBox(b, (int)((float)(bottomBox.Width / 2) - 158f), 0, oldGFX: true);
			Game1.mouseCursorTransparency = 1f;
			if (hoverText != null && !hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont);
			}
		}

		public void setUpMobileMenu()
		{
			int num = 72;
			widthMod = (float)(Game1.uiViewport.Width - Game1.xEdge * 2) / 1280f;
			heightMod = (float)(Game1.uiViewport.Height - 2 * num) / 720f;
			drawScale = (int)(4f * Math.Min(widthMod, heightMod));
			initializeUpperRightCloseButton();
			buyButtonBounds.Width = 200;
			buyButtonBounds.Height = 80;
			buyText = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1654");
			topBox.Width = (int)((float)drawScale * 320f);
			topBox.Height = (int)((float)drawScale * 144f);
			topBox.X = (Game1.uiViewport.Width - Game1.xEdge * 2 - topBox.Width) / 2 + Game1.xEdge;
			topBox.Y = num;
			bottomBox.X = Game1.xEdge;
			bottomBox.Y = topBox.Y + topBox.Height + 4;
			bottomBox.Width = Game1.uiViewport.Width - 2 * Game1.xEdge;
			bottomBox.Height = Game1.uiViewport.Height - bottomBox.Y;
			int num2 = topBox.X + drawScale;
			int num3 = topBox.Y + 52 * drawScale;
			checkboxes.Clear();
			for (int i = 0; i < 5; i++)
			{
				checkboxes.Add(new ClickableComponent(new Rectangle(num2, num3, 147 * drawScale, 30 * drawScale), i.ToString() ?? ""));
				num2 += 148 * drawScale;
				if (num2 > topBox.X + 148 * drawScale * 2)
				{
					num2 = topBox.X + drawScale;
					num3 += 30 * drawScale;
				}
			}
			checkboxes[4].bounds.Height = 32 * drawScale;
			checkboxes[1].bounds.Width = (checkboxes[3].bounds.Width = 170 * drawScale);
			buyButtonBounds.X = bottomBox.X + bottomBox.Width - 16 - 32 - buyButtonBounds.Width;
			buyButtonBounds.Y = bottomBox.Y + (bottomBox.Height - buyButtonBounds.Height) / 2;
			buyButton = new ClickableComponent(buyButtonBounds, "");
			buyButtonIsHeld = false;
			if (Game1.player.hasOrWillReceiveMail("ccVault"))
			{
				checkboxes[0].name = "complete";
			}
			if (Game1.player.hasOrWillReceiveMail("ccBoilerRoom"))
			{
				checkboxes[1].name = "complete";
			}
			if (Game1.player.hasOrWillReceiveMail("ccCraftsRoom"))
			{
				checkboxes[2].name = "complete";
			}
			if (Game1.player.hasOrWillReceiveMail("ccPantry"))
			{
				checkboxes[3].name = "complete";
			}
			if (Game1.player.hasOrWillReceiveMail("ccFishTank"))
			{
				checkboxes[4].name = "complete";
			}
			currentlySelectedBox = 0;
			buyButtonShown = Game1.player.Money >= getPriceFromButtonNumber(currentlySelectedBox) && !checkboxes[currentlySelectedBox].name.Equals("complete");
		}

		private void OnClickBuyButton()
		{
			Game1.playSound("coin");
			int priceFromButtonNumber = getPriceFromButtonNumber(currentlySelectedBox);
			if (Game1.player.Money >= priceFromButtonNumber)
			{
				Game1.playSound("coin");
				Game1.player.Money -= priceFromButtonNumber;
				Game1.playSound("reward");
				checkboxes[currentlySelectedBox].name = "complete";
				boughtSomething = true;
				switch (currentlySelectedBox)
				{
				case 0:
					Game1.addMailForTomorrow("jojaVault", noLetter: true, sendToEveryone: true);
					Game1.addMailForTomorrow("ccVault", noLetter: true, sendToEveryone: true);
					break;
				case 1:
					Game1.addMailForTomorrow("jojaBoilerRoom", noLetter: true, sendToEveryone: true);
					Game1.addMailForTomorrow("ccBoilerRoom", noLetter: true, sendToEveryone: true);
					break;
				case 2:
					Game1.addMailForTomorrow("jojaCraftsRoom", noLetter: true, sendToEveryone: true);
					Game1.addMailForTomorrow("ccCraftsRoom", noLetter: true, sendToEveryone: true);
					break;
				case 3:
					Game1.addMailForTomorrow("jojaPantry", noLetter: true, sendToEveryone: true);
					Game1.addMailForTomorrow("ccPantry", noLetter: true, sendToEveryone: true);
					break;
				case 4:
					Game1.addMailForTomorrow("jojaFishTank", noLetter: true, sendToEveryone: true);
					Game1.addMailForTomorrow("ccFishTank", noLetter: true, sendToEveryone: true);
					break;
				}
				currentlySelectedBox = -1;
				buyButtonShown = false;
				exitTimer = 1000;
			}
			else
			{
				Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (exitTimer >= 0)
			{
				return;
			}
			base.receiveLeftClick(x, y);
			if (buyButtonShown && buyButton.containsPoint(x, y))
			{
				Game1.playSound("smallSelect");
				return;
			}
			for (int i = 0; i < 5; i++)
			{
				if (checkboxes[i].containsPoint(x, y))
				{
					Game1.playSound("smallSelect");
					currentlySelectedBox = i;
					if (checkboxes[i].name.Equals("complete"))
					{
						buyButtonShown = false;
					}
					else
					{
						buyButtonShown = Game1.player.Money >= getPriceFromButtonNumber(i);
					}
				}
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (buyButtonShown && buyButton.containsPoint(x, y))
			{
				buyButton.bounds.X = buyButtonBounds.X - 4;
				buyButton.bounds.Y = buyButtonBounds.Y + 4;
				buyButtonIsHeld = true;
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			if (buyButtonShown && buyButtonIsHeld && buyButton.containsPoint(x, y) && currentlySelectedBox != -1)
			{
				OnClickBuyButton();
			}
			buyButton.bounds = buyButtonBounds;
			buyButtonIsHeld = false;
		}
	}
}
