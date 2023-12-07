using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class TitleTextInputMenu : NamingMenu
	{
		public ClickableTextureComponent pasteButton;

		public const int region_pasteButton = 105;

		public string context = "";

		public TitleTextInputMenu(string title, doneNamingBehavior b, string default_text = "", string context = "")
			: base(b, title, "")
		{
			this.context = context;
			textBox.limitWidth = false;
			textBox.Width = 512;
			textBox.X -= 128;
			randomButton.visible = false;
			pasteButton = new ClickableTextureComponent(new Rectangle(textBox.X + textBox.Width + 32 + 4 + 64, Game1.viewport.Height / 2 - 8, 64, 64), Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 4f)
			{
				myID = 105,
				leftNeighborID = 102
			};
			doneNamingButton.rightNeighborID = 105;
			doneNamingButton.bounds.X += 128;
			minLength = 0;
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
			textBox.Text = default_text;
		}

		public override void performHoverAction(int x, int y)
		{
			if (pasteButton != null)
			{
				pasteButton.tryHover(x, y);
			}
			base.performHoverAction(x, y);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (pasteButton != null && pasteButton.containsPoint(x, y))
			{
				string text = "";
			}
			base.receiveLeftClick(x, y, playSound);
		}

		public override void update(GameTime time)
		{
			GamePadState state = Game1.input.GetGamePadState();
			KeyboardState state2 = Game1.GetKeyboardState();
			if (Game1.IsPressEvent(ref state, Buttons.B) || Game1.IsPressEvent(ref state2, Keys.Escape))
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
			base.update(time);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (pasteButton != null)
			{
				pasteButton.draw(b);
			}
		}
	}
}
