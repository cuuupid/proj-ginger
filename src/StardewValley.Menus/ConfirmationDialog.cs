using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class ConfirmationDialog : IClickableMenu
	{
		public delegate void behavior(Farmer who);

		public const int region_okButton = 101;

		public const int region_cancelButton = 102;

		protected string message;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent cancelButton;

		protected behavior onConfirm;

		protected behavior onCancel;

		private bool active = true;

		private Rectangle box;

		private bool okHeld;

		private bool cancelHeld;

		private ClickableTextureComponent _selectedButton;

		public ConfirmationDialog(string message, behavior onConfirm, behavior onCancel = null)
			: base(Game1.uiViewport.Width / 2 - (int)Game1.dialogueFont.MeasureString(message).X / 2 - IClickableMenu.borderWidth, Game1.uiViewport.Height / 2 - (int)Game1.dialogueFont.MeasureString(message).Y / 2, (int)Game1.dialogueFont.MeasureString(message).X + IClickableMenu.borderWidth * 2, (int)Game1.dialogueFont.MeasureString(message).Y + IClickableMenu.borderWidth * 2 + 160)
		{
			initialize(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height, showUpperRightCloseButton: true);
			if (onCancel == null)
			{
				onCancel = closeDialog;
			}
			else
			{
				this.onCancel = onCancel;
			}
			this.onConfirm = onConfirm;
			message = Game1.parseText(width: Math.Min(Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Width - 64, width), text: message, whichFont: Game1.dialogueFont);
			this.message = message;
			box = new Rectangle(Game1.xEdge, Game1.uiViewport.Height / 2 - 160, Game1.uiViewport.Width - Game1.xEdge * 2, 320);
			Rectangle bounds = new Rectangle(box.X + box.Width / 2 + 16, box.Y + box.Height - 32 - 80, 80, 80);
			Rectangle bounds2 = new Rectangle(box.X + (box.Width / 2 - 16 - 80), box.Y + box.Height - 32 - 80, 80, 80);
			okButton = new ClickableTextureComponent("OK", bounds, null, null, Game1.mobileSpriteSheet, new Rectangle(0, 0, 20, 20), 4f, drawShadow: true);
			cancelButton = new ClickableTextureComponent("Cancel", bounds2, null, null, Game1.mobileSpriteSheet, new Rectangle(20, 0, 20, 20), 4f, drawShadow: true);
			okHeld = (cancelHeld = false);
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public virtual void closeDialog(Farmer who)
		{
			if (Game1.activeClickableMenu is TitleMenu)
			{
				(Game1.activeClickableMenu as TitleMenu).backButtonPressed();
			}
			else
			{
				Game1.exitActiveMenu();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(102);
			snapCursorToCurrentSnappedComponent();
		}

		public void confirm()
		{
			if (onConfirm != null)
			{
				onConfirm(Game1.player);
			}
			if (active)
			{
				Game1.playSound("smallSelect");
			}
			active = false;
		}

		public void cancel()
		{
			if (onCancel != null)
			{
				onCancel(Game1.player);
			}
			else
			{
				closeDialog(Game1.player);
			}
			Game1.playSound("bigDeSelect");
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!active)
			{
				return;
			}
			if (upperRightCloseButton.containsPoint(x, y))
			{
				if (onCancel != null)
				{
					onCancel(Game1.player);
				}
				else
				{
					Game1.exitActiveMenu();
				}
				Game1.playSound("bigDeSelect");
				active = false;
				return;
			}
			if (okButton.containsPoint(x, y))
			{
				okHeld = true;
				Game1.playSound("smallSelect");
			}
			if (cancelButton.containsPoint(x, y))
			{
				cancelHeld = true;
				Game1.playSound("smallSelect");
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (active && Game1.activeClickableMenu == null && onCancel != null)
			{
				onCancel(Game1.player);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void draw(SpriteBatch b)
		{
			if (active)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				IClickableMenu.drawTextureBox(b, box.X, box.Y, box.Width, box.Height, Color.White);
				Utility.drawMultiLineTextWithShadow(b, message, Game1.dialogueFont, new Vector2(box.X + 32, box.Y + 32), box.Width - 64, box.Height - 80 - 32 - 16, Game1.textColor);
				if (_selectedButton != null)
				{
					IClickableMenu.drawTextureBox(b, Game1.mobileSpriteSheet, new Rectangle(20, 96, 20, 20), _selectedButton.bounds.X - 4, _selectedButton.bounds.Y - 4, _selectedButton.bounds.Width + 8, _selectedButton.bounds.Height + 8, Color.White, 4f, drawShadow: false);
				}
				Rectangle bounds = okButton.bounds;
				if (okHeld)
				{
					okButton.drawShadow = false;
					okButton.bounds.X -= 4;
					okButton.bounds.Y += 4;
				}
				okButton.draw(b, Color.White, 0.08f);
				okButton.drawShadow = true;
				okButton.bounds = bounds;
				bounds = cancelButton.bounds;
				if (cancelHeld)
				{
					cancelButton.drawShadow = false;
					cancelButton.bounds.X -= 4;
					cancelButton.bounds.Y += 4;
				}
				cancelButton.draw(b, Color.White, 0.08f);
				cancelButton.drawShadow = true;
				cancelButton.bounds = bounds;
				drawMouse(b);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (active)
			{
				if (okHeld && !okButton.containsPoint(x, y))
				{
					okHeld = false;
				}
				if (cancelHeld && !cancelButton.containsPoint(x, y))
				{
					cancelHeld = false;
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (active)
			{
				if (okHeld && okButton.containsPoint(x, y))
				{
					if (onConfirm != null)
					{
						onConfirm(Game1.player);
					}
					Game1.playSound("smallSelect");
					active = false;
				}
				if (cancelHeld && cancelButton.containsPoint(x, y))
				{
					if (onCancel != null)
					{
						onCancel(Game1.player);
					}
					else
					{
						Game1.exitActiveMenu();
					}
					Game1.playSound("bigDeSelect");
				}
			}
			okHeld = (cancelHeld = false);
		}

		public override void receiveGamePadButton(Buttons b)
		{
			switch (b)
			{
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
				if (_selectedButton == null)
				{
					_selectedButton = cancelButton;
				}
				else if (_selectedButton == cancelButton)
				{
					_selectedButton = okButton;
				}
				else if (_selectedButton == okButton)
				{
					_selectedButton = cancelButton;
				}
				break;
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickRight:
				if (_selectedButton == null)
				{
					_selectedButton = okButton;
				}
				else if (_selectedButton == cancelButton)
				{
					_selectedButton = okButton;
				}
				else if (_selectedButton == okButton)
				{
					_selectedButton = cancelButton;
				}
				break;
			case Buttons.A:
			case Buttons.B:
				if (_selectedButton == okButton || (_selectedButton == null && b == Buttons.B))
				{
					Game1.playSound("smallSelect");
					if (onConfirm != null)
					{
						onConfirm(Game1.player);
					}
					active = false;
				}
				else if (_selectedButton == cancelButton || (_selectedButton == null && b == Buttons.A))
				{
					Game1.playSound("bigDeSelect");
					if (onCancel != null)
					{
						onCancel(Game1.player);
					}
					else
					{
						Game1.exitActiveMenu();
					}
				}
				break;
			}
		}
	}
}
