using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class NumberSelectionMenu : IClickableMenu
	{
		public delegate void behaviorOnNumberSelect(int number, int price, Farmer who);

		public const int region_leftButton = 101;

		public const int region_rightButton = 102;

		public const int region_okButton = 103;

		public const int region_cancelButton = 104;

		private string message;

		protected int price;

		protected int minValue;

		protected int maxValue;

		protected int currentValue;

		protected int priceShake;

		protected int heldTimer;

		private behaviorOnNumberSelect behaviorFunction;

		protected TextBox numberSelectedBox;

		public ClickableTextureComponent leftButton;

		public ClickableTextureComponent rightButton;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent cancelButton;

		private int buttonY;

		private int buttonY2;

		private Vector2 textSize;

		private SliderBar quantitySlider;

		private bool quantitySliderHeld;

		private ClickableTextureComponent _selectedButton;

		protected virtual Vector2 centerPosition => new Vector2(Game1.uiViewport.Width / 2, Game1.uiViewport.Height / 2);

		public NumberSelectionMenu(string message, behaviorOnNumberSelect behaviorOnSelection, int price = -1, int minValue = 0, int maxValue = 99, int defaultNumber = 0)
		{
			Vector2 vector = Game1.dialogueFont.MeasureString(message);
			int num = Math.Max((int)vector.X, 600) + IClickableMenu.borderWidth * 2;
			int num2 = (int)vector.Y + IClickableMenu.borderWidth * 2 + 160;
			int x = Game1.uiViewport.Width / 2 - num / 2;
			int y = Game1.uiViewport.Height / 2 - num2 / 2;
			initialize(x, y, num, num2);
			this.message = message;
			this.price = price;
			this.minValue = minValue;
			this.maxValue = maxValue;
			currentValue = defaultNumber;
			behaviorFunction = behaviorOnSelection;
			buttonY = yPositionOnScreen + 96;
			textSize = Game1.dialogueFont.MeasureString(maxValue.ToString() ?? "");
			leftButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth, buttonY, 80, 76), Game1.mobileSpriteSheet, new Rectangle(80, 0, 20, 19), 4f, drawShadow: true);
			rightButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth + 80 + 120, buttonY, 80, 76), Game1.mobileSpriteSheet, new Rectangle(100, 0, 20, 19), 4f, drawShadow: true);
			numberSelectedBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor, drawBackground: false, centerText: true)
			{
				X = leftButton.bounds.X + leftButton.bounds.Width,
				Y = buttonY + 4,
				Width = 120,
				Height = (int)textSize.Y,
				Text = (currentValue.ToString() ?? ""),
				numbersOnly = true,
				textLimit = (maxValue.ToString() ?? "").Length
			};
			quantitySlider = new SliderBar(leftButton.bounds.X, leftButton.bounds.Y + leftButton.bounds.Height + 32, currentValue);
			quantitySlider.bounds.Width = rightButton.bounds.X + rightButton.bounds.Width - leftButton.bounds.X;
			quantitySlider.bounds.Height = 100;
			buttonY2 = quantitySlider.bounds.Y;
			cancelButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 160, buttonY2, 80, 80), null, null, Game1.mobileSpriteSheet, new Rectangle(20, 0, 20, 20), 4f, drawShadow: true);
			okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 80 + 16, buttonY2, 80, 80), null, null, Game1.mobileSpriteSheet, new Rectangle(0, 0, 20, 20), 4f, drawShadow: true);
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(102);
			snapCursorToCurrentSnappedComponent();
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			switch (b)
			{
			case Buttons.DPadUp:
			case Buttons.LeftThumbstickUp:
				if (_selectedButton == null || _selectedButton == cancelButton || _selectedButton == okButton)
				{
					_selectedButton = rightButton;
				}
				break;
			case Buttons.DPadDown:
			case Buttons.LeftThumbstickDown:
				if (_selectedButton == null || _selectedButton == leftButton || _selectedButton == rightButton)
				{
					_selectedButton = cancelButton;
				}
				break;
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
				if (_selectedButton == okButton)
				{
					_selectedButton = cancelButton;
					break;
				}
				if (_selectedButton == cancelButton)
				{
					_selectedButton = okButton;
					break;
				}
				if (_selectedButton == leftButton)
				{
					OnClickLeftArrowButton();
				}
				_selectedButton = leftButton;
				break;
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickRight:
				if (_selectedButton == okButton)
				{
					_selectedButton = cancelButton;
					break;
				}
				if (_selectedButton == cancelButton)
				{
					_selectedButton = okButton;
					break;
				}
				if (_selectedButton == rightButton)
				{
					OnClickRightArrowButton();
				}
				_selectedButton = rightButton;
				break;
			case Buttons.A:
				if (_selectedButton == okButton)
				{
					OnClickOKButton();
				}
				else if (_selectedButton == cancelButton)
				{
					CloseMenu();
				}
				else if (_selectedButton == leftButton)
				{
					OnClickLeftArrowButton();
				}
				else if (_selectedButton == rightButton)
				{
					OnClickRightArrowButton();
				}
				break;
			}
		}

		private void OnClickLeftArrowButton()
		{
			currentValue = Math.Max(minValue, currentValue - 1);
			if (currentValue == 0 || maxValue == 0)
			{
				quantitySlider.value = 0;
			}
			else
			{
				quantitySlider.value = currentValue * 100 / maxValue;
			}
			numberSelectedBox.Text = currentValue.ToString();
			Game1.playSound("smallSelect");
		}

		private void OnClickRightArrowButton()
		{
			currentValue = Math.Min(maxValue, currentValue + 1);
			if (currentValue == 0 || maxValue == 0)
			{
				quantitySlider.value = 0;
			}
			else
			{
				quantitySlider.value = currentValue * 100 / maxValue;
			}
			numberSelectedBox.Text = currentValue.ToString();
			Game1.playSound("smallSelect");
		}

		public override void gamePadButtonHeld(Buttons b)
		{
			base.gamePadButtonHeld(b);
			if (b != Buttons.A || currentlySnappedComponent == null)
			{
				return;
			}
			heldTimer += Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			if (heldTimer <= 300)
			{
				return;
			}
			if (currentlySnappedComponent.myID == 102)
			{
				int num = currentValue + 1;
				if (num <= maxValue && (price == -1 || num * price <= Game1.player.Money))
				{
					rightButton.scale = rightButton.baseScale;
					currentValue = num;
					numberSelectedBox.Text = currentValue.ToString() ?? "";
				}
			}
			else if (currentlySnappedComponent.myID == 101)
			{
				int num2 = currentValue - 1;
				if (num2 >= minValue)
				{
					leftButton.scale = leftButton.baseScale;
					currentValue = num2;
					numberSelectedBox.Text = currentValue.ToString() ?? "";
				}
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (leftButton.containsPoint(x, y))
			{
				int num = currentValue - 1;
				if (num >= minValue)
				{
					currentValue = num;
					numberSelectedBox.Text = currentValue.ToString() ?? "";
					quantitySlider.value = currentValue * 100 / maxValue;
					Game1.playSound("smallSelect");
				}
			}
			if (rightButton.containsPoint(x, y))
			{
				int num2 = currentValue + 1;
				if (num2 <= maxValue && (price == -1 || num2 * price <= Game1.player.Money))
				{
					currentValue = num2;
					quantitySlider.value = currentValue * 100 / maxValue;
					numberSelectedBox.Text = currentValue.ToString() ?? "";
					Game1.playSound("smallSelect");
				}
			}
			if (maxValue > 1 && quantitySlider.bounds.Contains(x, y))
			{
				quantitySlider.click(x, y);
				quantitySliderHeld = true;
			}
		}

		public void resetButtons()
		{
			leftButton.bounds.X = xPositionOnScreen + IClickableMenu.borderWidth;
			leftButton.bounds.Y = (rightButton.bounds.Y = buttonY);
			okButton.bounds.Y = (cancelButton.bounds.Y = buttonY2);
			rightButton.bounds.X = xPositionOnScreen + IClickableMenu.borderWidth + 80 + 120;
			okButton.bounds.X = xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 80 + 16;
			cancelButton.bounds.X = xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 160;
			leftButton.drawShadow = (rightButton.drawShadow = (okButton.drawShadow = (cancelButton.drawShadow = true)));
		}

		private void OnClickOKButton()
		{
			if (currentValue > maxValue || currentValue < minValue)
			{
				currentValue = Math.Max(minValue, Math.Min(maxValue, currentValue));
				numberSelectedBox.Text = currentValue.ToString() ?? "";
			}
			else if (behaviorFunction != null)
			{
				behaviorFunction(currentValue, price, Game1.player);
			}
			Game1.playSound("smallSelect");
			Game1.player.forceCanMove();
		}

		public override void releaseLeftClick(int x, int y)
		{
			quantitySliderHeld = false;
			resetButtons();
			if (okButton.containsPoint(x, y))
			{
				OnClickOKButton();
			}
			if (cancelButton.containsPoint(x, y))
			{
				CloseMenu();
			}
		}

		public void CloseMenu()
		{
			Game1.exitActiveMenu();
			Game1.playSound("bigDeSelect");
			Game1.player.forceCanMove();
		}

		public override void leftClickHeld(int x, int y)
		{
			resetButtons();
			if (leftButton.containsPoint(x, y))
			{
				leftButton.bounds.X -= 4;
				leftButton.bounds.Y += 4;
				leftButton.drawShadow = false;
			}
			else if (rightButton.containsPoint(x, y))
			{
				rightButton.bounds.X -= 4;
				rightButton.bounds.Y += 4;
				rightButton.drawShadow = false;
			}
			else if (okButton.containsPoint(x, y))
			{
				okButton.bounds.X -= 4;
				okButton.bounds.Y += 4;
				okButton.drawShadow = false;
			}
			else if (cancelButton.containsPoint(x, y))
			{
				cancelButton.bounds.X -= 4;
				cancelButton.bounds.Y += 4;
				cancelButton.drawShadow = false;
			}
			if (quantitySliderHeld)
			{
				quantitySlider.click(x, y, wasAlreadyHeld: true);
				currentValue = Math.Min((int)((float)(quantitySlider.value * maxValue) / 100f), maxValue);
				currentValue = Math.Max(minValue, Math.Min(maxValue, currentValue));
				numberSelectedBox.Text = currentValue.ToString() ?? "";
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (key == Keys.Enter)
			{
				receiveLeftClick(okButton.bounds.Center.X, okButton.bounds.Center.Y);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			currentValue = 0;
			if (numberSelectedBox.Text != null)
			{
				int.TryParse(numberSelectedBox.Text, out currentValue);
			}
			if (priceShake > 0)
			{
				priceShake -= time.ElapsedGameTime.Milliseconds;
			}
			if (Game1.options.SnappyMenus)
			{
				_ = Game1.oldPadState;
				if (!Game1.oldPadState.IsButtonDown(Buttons.A))
				{
					heldTimer = 0;
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.75f);
			IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, quantitySlider.bounds.Y + quantitySlider.bounds.Height - yPositionOnScreen + 48, Color.White);
			Utility.drawTextWithShadow(b, message, Game1.dialogueFont, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen + 32), Game1.textColor);
			if (price != -1)
			{
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", price * currentValue), new Vector2(xPositionOnScreen + IClickableMenu.borderWidth + 80 + 216 + ((priceShake > 0) ? Game1.random.Next(-1, 2) : 0), buttonY + 16 + ((priceShake > 0) ? Game1.random.Next(-1, 2) : 0)), (currentValue * price > Game1.player.Money) ? Color.Red : Game1.textColor);
			}
			quantitySlider.draw(b);
			Game1.dayTimeMoneyBox.drawMoneyBox(b, Game1.uiViewport.Width / 2 - 130, 0, oldGFX: true);
			leftButton.draw(b);
			rightButton.draw(b);
			numberSelectedBox.Draw(b);
			okButton.draw(b);
			cancelButton.draw(b);
			if (_selectedButton != null)
			{
				IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(20, 96, 20, 20), _selectedButton.bounds.X - 4, _selectedButton.bounds.Y - 4, _selectedButton.bounds.Width + 8, _selectedButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}
	}
}
