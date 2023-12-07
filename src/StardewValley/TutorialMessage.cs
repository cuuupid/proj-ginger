using System;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace StardewValley
{
	public class TutorialMessage : IClickableMenu
	{
		public const float defaultTime = 5000f;

		public string message;

		public string type;

		public Color color;

		public float timeLeft;

		public float transparency = 1f;

		public int number = -1;

		public int whatType;

		public bool add;

		public bool fadeIn;

		private Rectangle bounds;

		private float widthMod;

		private float heightMod;

		private DialogueBox tMenu;

		public string Message
		{
			get
			{
				return message;
			}
			set
			{
				message = value;
			}
		}

		public TutorialMessage(string message, int x = -1, int y = -1, int maxWidth = -1, int maxHeight = -1)
		{
			widthMod = (float)Game1.viewport.Width / 1280f;
			heightMod = (float)Game1.viewport.Height / 720f;
			if (x != -1)
			{
				bounds = new Rectangle(x, y, maxWidth, maxHeight);
			}
			else
			{
				bounds = new Rectangle(Toolbar.toolBarItemWidth + 40 + Game1.toolbarPaddingX, Game1.viewport.Height - 256, Game1.viewport.Width - Toolbar.toolBarItemWidth - 40 - Game1.toolbarPaddingX, 128);
			}
			this.message = message;
			color = Color.SeaGreen;
			timeLeft = 5000f;
			tMenu = new DialogueBox(message);
		}

		public new bool update(GameTime time)
		{
			timeLeft -= time.ElapsedGameTime.Milliseconds;
			if (timeLeft < 0f)
			{
				transparency -= 0.02f;
				if (transparency < 0f)
				{
					return true;
				}
			}
			else if (fadeIn)
			{
				transparency = Math.Min(transparency + 0.02f, 1f);
			}
			return false;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			throw new NotImplementedException();
		}
	}
}
