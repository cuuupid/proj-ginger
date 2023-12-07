using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class NetworkModeMenu : IClickableMenu
	{
		public new const int width = 650;

		public new const int height = 260;

		public List<ClickableComponent> buttons = new List<ClickableComponent>();

		public bool OpenCoopMenu;

		public static bool SplitScreen;

		public NetworkModeMenu()
		{
			SetupButtons();
			SplitScreen = false;
		}

		private void SetupButtons()
		{
			Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(650, 260);
			buttons.Clear();
			int num = 83;
			int num2 = 16;
			string label = Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_Online");
			buttons.Add(new ClickableComponent(new Rectangle((int)topLeftPositionForCenteringOnScreen.X + 64, (int)topLeftPositionForCenteringOnScreen.Y + 32, 522, num), "net", label)
			{
				myID = 0,
				downNeighborID = 1
			});
			string label2 = Game1.content.LoadString("Strings\\UI:Split_Screen");
			buttons.Add(new ClickableComponent(new Rectangle((int)topLeftPositionForCenteringOnScreen.X + 64, (int)topLeftPositionForCenteringOnScreen.Y + 32 + (num + num2), 522, num), "split", label2)
			{
				myID = 1,
				upNeighborID = 0
			});
			if (Game1.options.SnappyMenus)
			{
				int id = ((currentlySnappedComponent != null) ? currentlySnappedComponent.myID : 0);
				populateClickableComponentList();
				currentlySnappedComponent = getComponentWithID(id);
				snapCursorToCurrentSnappedComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			foreach (ClickableComponent button in buttons)
			{
				if (button.containsPoint(x, y))
				{
					OpenCoopMenu = true;
					SplitScreen = button.myID == 1;
					(Game1.activeClickableMenu as TitleMenu).backButtonPressed();
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void draw(SpriteBatch b)
		{
			Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(650, 160);
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), (int)topLeftPositionForCenteringOnScreen.X + 32, (int)topLeftPositionForCenteringOnScreen.Y - 55, 586, 250, Color.White, 4f);
			foreach (ClickableComponent button in buttons)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), button.bounds.X, button.bounds.Y, button.bounds.Width, button.bounds.Height, (button.scale > 0f) ? Color.Wheat : Color.White, 4f);
				Utility.drawTextWithShadow(b, button.label, Game1.dialogueFont, new Vector2(button.bounds.Center.X, button.bounds.Center.Y + 4) - Game1.dialogueFont.MeasureString(button.label) / 2f, Game1.textColor, 1f, -1f, -1, -1, 0f);
			}
		}

		public override bool readyToClose()
		{
			return true;
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			SetupButtons();
		}
	}
}
