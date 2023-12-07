using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class OptionsDropDown : OptionsElement
	{
		public static Rectangle dropDownBGSource = new Rectangle(433, 451, 3, 3);

		public static Rectangle dropDownButtonSource = new Rectangle(437, 450, 10, 11);

		public static OptionsDropDown selected;

		public ClickableTextureComponent buttonHelp;

		public string url;

		private bool _buttonHelpClicked;

		public List<string> dropDownOptions = new List<string>();

		public List<string> dropDownDisplayOptions = new List<string>();

		public int selectedOption;

		public int recentSlotY;

		public int startingSelected;

		public bool dropDownOpen;

		private Rectangle dropDownBounds;

		public override int ItemHeight => 64;

		public OptionsDropDown(string label, int whichOption, int x = -1, int y = -1)
			: base(label, x, y, Game1.uiViewport.Width - 100, 52, whichOption)
		{
			Game1.options.setDropDownToProperValue(this);
			dropDownBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height * Math.Max(1, dropDownOptions.Count));
		}

		public void AddHelpButton(string url)
		{
			this.url = url;
			buttonHelp = new ClickableTextureComponent(new Rectangle(dropDownBounds.X + dropDownBounds.Width + 12, dropDownBounds.Y, 64, 64), Game1.mouseCursors, new Rectangle(240, 192, 16, 16), 4f);
		}

		public virtual void RecalculateBounds()
		{
			foreach (string dropDownDisplayOption in dropDownDisplayOptions)
			{
				float x = Game1.smallFont.MeasureString(dropDownDisplayOption).X;
				if (x >= (float)(bounds.Width - 48))
				{
					bounds.Width = (int)(x + 64f);
				}
			}
			dropDownBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width - 48, bounds.Height * dropDownOptions.Count);
		}

		public override void leftClickHeld(int x, int y)
		{
		}

		public override void receiveLeftClick(int x, int y)
		{
			dropDownBounds.X = bounds.X;
			dropDownBounds.Y = bounds.Y;
			if (dropDownOpen)
			{
				dropDownOpen = false;
				if (dropDownBounds.Contains(x, y))
				{
					selectedOption = (int)Math.Max(Math.Min((float)(y - dropDownBounds.Y) / (float)bounds.Height, dropDownOptions.Count - 1), 0f);
					Game1.options.changeDropDownOption(whichOption, dropDownOptions[selectedOption]);
				}
				selected = null;
			}
			else if (dropDownBounds.Contains(x, y))
			{
				dropDownOpen = true;
				Game1.playSound("shwip");
				selected = this;
			}
			if (buttonHelp != null && !_buttonHelpClicked && buttonHelp.containsPoint(x, y))
			{
				Game1.playSound("bigSelect");
				try
				{
					Intent intent = new Intent("android.intent.action.VIEW", Android.Net.Uri.Parse(url));
					intent.AddFlags(ActivityFlags.NewTask);
					Application.Context.StartActivity(intent);
				}
				catch (Exception)
				{
				}
			}
		}

		public override void leftClickReleased(int x, int y)
		{
			_buttonHelpClicked = false;
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
			{
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
			{
				selectedOption++;
				if (selectedOption >= dropDownOptions.Count)
				{
					selectedOption = 0;
				}
				Game1.options.changeDropDownOption(whichOption, dropDownOptions[selectedOption]);
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
			{
				selectedOption--;
				if (selectedOption < 0)
				{
					selectedOption = dropDownOptions.Count - 1;
				}
				Game1.options.changeDropDownOption(whichOption, dropDownOptions[selectedOption]);
			}
		}

		public override void draw(SpriteBatch b, int slotX, int slotY)
		{
			recentSlotY = slotY;
			base.draw(b, slotX, slotY);
			float num = (greyedOut ? 0.33f : 1f);
			if (buttonHelp != null)
			{
				buttonHelp.bounds.X = bounds.X + bounds.Width - buttonHelp.bounds.Width;
				buttonHelp.bounds.Y = bounds.Y;
				buttonHelp.draw(b, Color.White, 0.87f);
				dropDownBounds.Width = bounds.Width - 12 - buttonHelp.bounds.Width;
			}
			else
			{
				dropDownBounds.Width = bounds.Width;
			}
			if (dropDownOpen)
			{
				dropDownBounds.X = bounds.X;
				dropDownBounds.Y = bounds.Y;
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, dropDownBGSource, slotX + dropDownBounds.X, slotY + dropDownBounds.Y, dropDownBounds.Width - 44, dropDownBounds.Height, Color.White * num, 4f, drawShadow: false);
				for (int i = 0; i < dropDownDisplayOptions.Count; i++)
				{
					if (i == selectedOption)
					{
						b.Draw(Game1.staminaRect, new Rectangle(slotX + dropDownBounds.X + 4, slotY + 4 + dropDownBounds.Y + i * bounds.Height, dropDownBounds.Width - 52, bounds.Height - 8), new Rectangle(0, 0, 1, 1), Color.Wheat, 0f, Vector2.Zero, SpriteEffects.None, 0.975f);
					}
					b.DrawString(Game1.smallFont, dropDownDisplayOptions[i], new Vector2(slotX + dropDownBounds.X + 8, slotY + dropDownBounds.Y + (Game1.options.bigFonts ? 4 : 12) + bounds.Height * i), Game1.textColor * num, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.98f);
				}
				b.Draw(Game1.mouseCursors, new Vector2(slotX + bounds.X + dropDownBounds.Width - 48, slotY + bounds.Y), dropDownButtonSource, Color.Wheat * num, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.981f);
			}
			else
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, dropDownBGSource, slotX + bounds.X, slotY + bounds.Y, dropDownBounds.Width - 44, bounds.Height, Color.White * num, 4f, drawShadow: false);
				b.DrawString(Game1.smallFont, (selectedOption < dropDownDisplayOptions.Count && selectedOption >= 0) ? dropDownDisplayOptions[selectedOption] : "", new Vector2(slotX + bounds.X + 8, slotY + bounds.Y + (Game1.options.bigFonts ? 4 : 12)), Game1.textColor * num, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.88f);
				b.Draw(Game1.mouseCursors, new Vector2(slotX + bounds.X + dropDownBounds.Width - 48, slotY + bounds.Y), dropDownButtonSource, Color.White * num, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			}
		}
	}
}
