using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class ScreenSizeAdjustMenu : IClickableMenu
	{
		public ScreenSizeAdjustMenu()
		{
			Game1.shouldDrawSafeAreaBounds = true;
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.B)
			{
				exitThisMenu();
			}
			else
			{
				base.receiveGamePadButton(b);
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.options.moveUpButton.Contains(new InputButton(key)))
			{
				Game1.AdjustScreenScale(-0.01f);
			}
			else if (Game1.options.moveDownButton.Contains(new InputButton(key)))
			{
				Game1.AdjustScreenScale(0.01f);
			}
			if (key == Keys.Escape)
			{
				exitThisMenu();
			}
		}

		protected override void cleanupBeforeExit()
		{
			Game1.shouldDrawSafeAreaBounds = false;
			base.cleanupBeforeExit();
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black * 0.75f);
			Vector2 vector = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height / 2);
			SpriteFont smallFont = Game1.smallFont;
			string text = Game1.content.LoadString("Strings\\UI:DisplayAdjustmentText");
			Vector2 vector2 = smallFont.MeasureString(text);
			vector2.X += 32f;
			vector -= vector2 / 2f;
			int num = 32;
			int num2 = Math.Max((int)vector2.Y, 32);
			Game1.DrawBox((int)vector.X - num, (int)vector.Y, (int)vector2.X + num * 2, num2);
			b.DrawString(smallFont, text, vector + new Vector2(4f, 4f), Game1.textShadowColor);
			b.DrawString(smallFont, text, vector, Game1.textColor);
			string text2 = Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel");
			vector.Y -= vector2.Y / 2f;
			vector.Y += num2;
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko)
			{
				vector.Y += 48f;
			}
			else
			{
				vector.Y += 32f;
			}
			vector.X += vector2.X + (float)num;
			vector.X -= smallFont.MeasureString(text2).X;
			b.DrawString(smallFont, text2, vector, Color.White);
			vector.X -= smallFont.MeasureString("XX").X;
			vector += smallFont.MeasureString("X") / 2f;
			b.Draw(Game1.controllerMaps, vector, Utility.controllerMapSourceRect(new Rectangle(569, 260, 28, 28)), Color.White, 0f, new Vector2(14f, 14f), 1f, SpriteEffects.None, 0.99f);
		}
	}
}
