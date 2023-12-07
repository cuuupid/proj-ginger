using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class ButtonTutorialMenu : IClickableMenu
	{
		public const int move_run_check = 0;

		public const int useTool_menu = 1;

		public const float movementSpeed = 0.2f;

		public new const int width = 42;

		public new const int height = 109;

		private int timerToclose = 15000;

		private int which;

		internal static int current;

		private int myID;

		public ButtonTutorialMenu(int which)
			: base(-168, Game1.uiViewport.Height / 2 - 218, 168, 436)
		{
			this.which = which;
			current++;
			myID = current;
		}

		public override void update(GameTime time)
		{
		}

		public override void draw(SpriteBatch b)
		{
			_ = destroy;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}
	}
}
