using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace StardewValley
{
	public class HUDMessage
	{
		public const float defaultTime = 3500f;

		public const int achievement_type = 1;

		public const int newQuest_type = 2;

		public const int error_type = 3;

		public const int stamina_type = 4;

		public const int health_type = 5;

		public const int screenshot_type = 6;

		public string message;

		public string type;

		public Color color;

		public float timeLeft;

		public float transparency = 1f;

		public int number = -1;

		public int whatType;

		public bool add;

		public bool achievement;

		public bool fadeIn;

		public bool noIcon;

		protected Rectangle bounds = new Rectangle(-1, -1, 1, 1);

		private int newQuestIndex = -1;

		protected int textWidth;

		public Item messageSubject;

		public string Message
		{
			get
			{
				if (type != null)
				{
					if (type.Equals("Money"))
					{
						return (add ? "+ " : "- ") + number + "g";
					}
					return (add ? "+ " : "- ") + number + " " + type;
				}
				return message;
			}
			set
			{
				message = value;
			}
		}

		public HUDMessage(string message)
		{
			this.message = message;
			color = Color.SeaGreen;
			timeLeft = 3500f;
		}

		public HUDMessage(string message, bool achievement)
		{
			if (achievement)
			{
				this.message = Game1.content.LoadString("Strings\\StringsFromCSFiles:HUDMessage.cs.3824") + message;
				color = Color.OrangeRed;
				timeLeft = 5250f;
				this.achievement = true;
				whatType = 1;
			}
		}

		public HUDMessage(string message, int whatType)
		{
			this.message = message;
			color = Color.OrangeRed;
			timeLeft = 5250f;
			achievement = true;
			this.whatType = whatType;
		}

		public HUDMessage(string message, int whatType, int questIndex)
		{
			this.message = message;
			color = Color.OrangeRed;
			timeLeft = 5250f;
			achievement = true;
			this.whatType = whatType;
			if (whatType == 2)
			{
				Vector2 vector = Game1.dialogueFont.MeasureString(message);
				bounds.Width = (textWidth = (int)(vector.X + 32f));
				bounds.Height = Math.Max(60, (int)vector.Y + 32);
				newQuestIndex = questIndex;
			}
		}

		public HUDMessage(string type, int number, bool add, Color color, Item messageSubject = null)
		{
			this.type = type;
			this.add = add;
			this.color = color;
			timeLeft = 3500f;
			this.number = number;
			this.messageSubject = messageSubject;
		}

		public HUDMessage(string message, Color color, float timeLeft)
			: this(message, color, timeLeft, fadeIn: false)
		{
		}

		public HUDMessage(string message, string leaveMeNull)
		{
			this.message = Game1.parseText(message, Game1.dialogueFont, 384);
			timeLeft = 3500f;
			color = Game1.textColor;
			noIcon = true;
		}

		public HUDMessage(string message, Color color, float timeLeft, bool fadeIn)
		{
			this.message = message;
			this.color = color;
			this.timeLeft = timeLeft;
			this.fadeIn = fadeIn;
			if (fadeIn)
			{
				transparency = 0f;
			}
		}

		public virtual bool update(GameTime time)
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

		public virtual void draw(SpriteBatch b, int i)
		{
			Rectangle titleSafeArea = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
			if (noIcon)
			{
				int num = titleSafeArea.Left + 16;
				int num2 = ((Game1.uiViewport.Width < 1400) ? (-64) : 0) + titleSafeArea.Bottom - (i + 1) * 64 * 7 / 4 - 21 - (int)Game1.dialogueFont.MeasureString(message).Y;
				SpriteFont dialogueFont = Game1.dialogueFont;
				if (Game1.options.verticalToolbar)
				{
					num = Game1.xEdge + Toolbar.toolBarItemWidth + 20;
				}
				num2 = Game1.uiViewport.Height - (i + 1) * 112 - (int)(Game1.smallFont.MeasureString(message).Y + 40f);
				if (!Game1.options.verticalToolbar && !Game1.toolbar.alignTop)
				{
					num2 -= Game1.toolbar.itemSlotSize;
				}
				dialogueFont = Game1.smallFont;
				bounds.X = num;
				bounds.Y = num2;
				IClickableMenu.drawHoverText(b, message, dialogueFont, 0, 0, -1, null, -1, null, null, 0, -1, -1, num, num2, transparency);
				return;
			}
			Vector2 vector = new Vector2(titleSafeArea.Left + 16, titleSafeArea.Bottom - (i + 1) * 64 * 7 / 4 - 64);
			if (Game1.isOutdoorMapSmallerThanViewport())
			{
				vector.X = Math.Max(titleSafeArea.Left + 16, -Game1.uiViewport.X + 16);
			}
			if (Game1.options.verticalToolbar)
			{
				vector.X = Game1.xEdge + Toolbar.toolBarItemWidth + 20;
				bounds.X = (int)vector.X;
			}
			vector.Y = Game1.uiViewport.Height - (i + 1) * 100 - 20;
			if (!Game1.options.verticalToolbar && !Game1.toolbar.alignTop)
			{
				vector.Y -= Game1.toolbar.itemSlotSize;
			}
			if (vector.Y >= 0f)
			{
				bounds.Y = (int)vector.Y;
				b.Draw(Game1.mouseCursors, vector, (messageSubject != null && messageSubject is Object && (messageSubject as Object).sellToStorePrice(-1L) > 500) ? new Rectangle(163, 399, 26, 24) : new Rectangle(293, 360, 26, 24), Color.White * transparency, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				float x = Game1.smallFont.MeasureString((messageSubject != null && messageSubject.DisplayName != null) ? messageSubject.DisplayName : ((message == null) ? "" : message)).X;
				x += 10f;
				int num3 = 104;
				b.Draw(Game1.mouseCursors, new Vector2(vector.X + (float)num3, vector.Y), new Rectangle(319, 360, 1, 24), Color.White * transparency, 0f, Vector2.Zero, new Vector2(x, 4f), SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(vector.X + 104f + x, vector.Y), new Rectangle(323, 360, 6, 24), Color.White * transparency, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				vector.X += 16f;
				vector.Y += 16f;
				if (messageSubject == null)
				{
					switch (whatType)
					{
					case 1:
						b.Draw(Game1.mouseCursors, vector + new Vector2(8f, 8f) * 4f, new Rectangle(294, 392, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
						break;
					case 2:
						b.Draw(Game1.mouseCursors, vector + new Vector2(8f, 8f) * 4f, new Rectangle(403, 496, 5, 14), Color.White * transparency, 0f, new Vector2(3f, 7f), 4f + Math.Max(0f, (timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
						break;
					case 3:
						b.Draw(Game1.mouseCursors, vector + new Vector2(8f, 8f) * 4f, new Rectangle(268, 470, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
						break;
					case 4:
						b.Draw(Game1.mouseCursors, vector + new Vector2(8f, 8f) * 4f, new Rectangle(0, 411, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
						break;
					case 5:
						b.Draw(Game1.mouseCursors, vector + new Vector2(8f, 8f) * 4f, new Rectangle(16, 411, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
						break;
					case 6:
						b.Draw(Game1.mouseCursors2, vector + new Vector2(8f, 8f) * 4f, new Rectangle(96, 32, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
						break;
					}
				}
				else
				{
					messageSubject.drawInMenu(b, vector, 1f + Math.Max(0f, (timeLeft - 3000f) / 900f), transparency, 1f, StackDrawType.Hide);
				}
				vector.X += 51f;
				vector.Y += 51f;
				if (number > 1)
				{
					Utility.drawTinyDigits(number, b, vector, 3f, 1f, Color.White * transparency);
				}
				vector.X += 40f;
				vector.Y -= 33f;
				Utility.drawTextWithShadow(b, (messageSubject == null) ? message : messageSubject.DisplayName, Game1.smallFont, vector, Game1.textColor * transparency, 1f, 1f, -1, -1, transparency);
			}
			bounds.X = (int)vector.X;
			bounds.Y = (int)vector.Y;
		}
	}
}
