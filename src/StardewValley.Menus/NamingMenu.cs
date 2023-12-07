using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	public class NamingMenu : IClickableMenu
	{
		public delegate void doneNamingBehavior(string s);

		public const int region_okButton = 101;

		public const int region_doneNamingButton = 102;

		public const int region_randomButton = 103;

		public const int region_namingBox = 104;

		public ClickableTextureComponent doneNamingButton;

		public ClickableTextureComponent randomButton;

		protected TextBox textBox;

		public ClickableComponent textBoxCC;

		private TextBoxEvent e;

		private doneNamingBehavior doneNaming;

		private string title;

		protected int minLength = 1;

		public NamingMenu(doneNamingBehavior b, string title, string defaultName = null)
		{
			doneNaming = b;
			xPositionOnScreen = 0;
			yPositionOnScreen = 0;
			width = Game1.uiViewport.Width;
			height = Game1.uiViewport.Height;
			this.title = title;
			randomButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 51 + 64, Game1.uiViewport.Height / 2, 64, 64), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f);
			textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
			textBox.X = Game1.uiViewport.Width / 2 - 128;
			textBox.textLimit = 25;
			textBox.Y = Game1.uiViewport.Height / 2;
			textBox.Width = 256;
			textBox.Height = 192;
			e = textBoxEnter;
			textBox.OnEnterPressed += e;
			Game1.keyboardDispatcher.Subscriber = textBox;
			textBox.Text = ((defaultName != null) ? defaultName : Dialogue.randomName());
			textBox.Selected = true;
			randomButton = new ClickableTextureComponent(new Rectangle(textBox.X - 104, textBox.Y - 12, 80, 80), Game1.mobileSpriteSheet, new Rectangle(87, 22, 20, 20), 4f);
			doneNamingButton = new ClickableTextureComponent(new Rectangle(textBox.X + textBox.Width + 32 + 4, textBox.Y - 12, 80, 80), Game1.mobileSpriteSheet, new Rectangle(0, 0, 20, 20), 4f)
			{
				myID = 102,
				rightNeighborID = 103,
				leftNeighborID = 104
			};
			textBoxCC = new ClickableComponent(new Rectangle(textBox.X, textBox.Y, 192, 48), "")
			{
				myID = 104,
				rightNeighborID = 102
			};
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(104);
			snapCursorToCurrentSnappedComponent();
		}

		public void textBoxEnter(TextBox sender)
		{
			if (sender.Text.Length >= minLength)
			{
				if (doneNaming != null)
				{
					doneNaming(sender.Text);
					textBox.Selected = false;
				}
				else
				{
					Game1.exitActiveMenu();
				}
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			switch (b)
			{
			case Buttons.A:
				receiveLeftClick(doneNamingButton.bounds.X, doneNamingButton.bounds.Y);
				Game1.playSound("smallSelect");
				break;
			case Buttons.X:
				receiveLeftClick(randomButton.bounds.X, randomButton.bounds.Y);
				Game1.playSound("shwip");
				break;
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (!textBox.Selected && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
			{
				base.receiveKeyPress(key);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			textBox.Update();
			if (doneNamingButton.containsPoint(x, y))
			{
				textBoxEnter(textBox);
				Game1.playSound("smallSelect");
			}
			else if (randomButton.containsPoint(x, y))
			{
				textBox.Text = Dialogue.randomName();
				Game1.playSound("drumkit6");
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			SpriteText.drawStringWithScrollCenteredAt(b, title, Game1.uiViewport.Width / 2, textBox.Y - 128, title);
			textBox.Draw(b);
			doneNamingButton.draw(b);
			randomButton.draw(b);
			drawMouse(b);
		}
	}
}
