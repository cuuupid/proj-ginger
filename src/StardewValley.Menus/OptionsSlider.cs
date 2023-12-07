using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class OptionsSlider : OptionsElement
	{
		public const int sliderButtonWidth = 80;

		public const int scrollBarWidth = 24;

		private string _baseLabel;

		public ClickableTextureComponent buttonLeftArrow;

		public ClickableTextureComponent buttonRightArrow;

		private float _percent;

		private int _buttonStartX = 40;

		private bool tappedLeftOrRight;

		public static Rectangle sliderBGSource = new Rectangle(403, 383, 6, 6);

		public static Rectangle sliderButtonRect = new Rectangle(420, 441, 10, 6);

		public int sliderMinValue;

		public int sliderMaxValue = 100;

		public bool isSliding;

		private int _value;

		public override int ItemHeight => 122;

		public int value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
				_percent = Math.Max(0f, Math.Min(1f, (float)(_value - sliderMinValue) / (float)(sliderMaxValue - sliderMinValue)));
				label = _baseLabel + ": " + _value;
			}
		}

		public OptionsSlider(string label, int whichOption, int x = -1, int y = -1, int width = -1)
			: base(label, x, y, (int)((float)(((width != -1) ? width : (Game1.uiViewport.Width - 2 * Game1.xEdge)) - 24 - 78) * 0.75f), 130, whichOption)
		{
			buttonLeftArrow = new ClickableTextureComponent(new Rectangle(x, y, 80, 80), Game1.mobileSpriteSheet, new Rectangle(80, 0, 20, 20), 4f);
			buttonRightArrow = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width - Game1.xEdge - 80 - 24, y, 80, 80), Game1.mobileSpriteSheet, new Rectangle(100, 0, 20, 20), 4f);
			_baseLabel = label;
			label = _baseLabel + ": " + _value;
			Game1.options.setSliderToProperValue(this);
		}

		public override void leftClickHeld(int x, int y)
		{
			if (greyedOut)
			{
				return;
			}
			base.leftClickHeld(x, y);
			if (!tappedLeftOrRight && y >= bounds.Y && y <= bounds.Y + bounds.Height)
			{
				x -= 40;
				int num = bounds.X + 100;
				int num2 = num + bounds.Width - 200 - 80;
				if (x >= num - 40 && x <= num2 + 40)
				{
					_percent = ((float)x - (float)num) / ((float)num2 - (float)num);
					_percent = Math.Max(0f, Math.Min(_percent, 1f));
					value = (int)(_percent * (float)(sliderMaxValue - sliderMinValue) + (float)sliderMinValue);
					Game1.options.changeSliderOption(whichOption, _value);
				}
			}
		}

		public override void receiveLeftClick(int x, int y)
		{
			if (!greyedOut)
			{
				base.receiveLeftClick(x, y);
				if (tappedLeftOrRight)
				{
					return;
				}
				if (buttonLeftArrow.containsPoint(x, y))
				{
					tappedLeftOrRight = true;
					if (_value > sliderMinValue)
					{
						value = _value - 1;
					}
				}
				else if (buttonRightArrow.containsPoint(x, y))
				{
					tappedLeftOrRight = true;
					if (_value < sliderMaxValue)
					{
						value = _value + 1;
					}
				}
				else
				{
					isSliding = true;
					leftClickHeld(x, y);
				}
			}
			Game1.options.changeSliderOption(whichOption, value);
		}

		public override void releaseLeftClick(int x, int y)
		{
			isSliding = false;
			tappedLeftOrRight = false;
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
				{
					value = Math.Min(value + 10, sliderMaxValue);
					Game1.options.changeSliderOption(whichOption, value);
				}
				else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
				{
					value = Math.Max(value - 10, sliderMinValue);
					Game1.options.changeSliderOption(whichOption, value);
				}
			}
		}

		public override void draw(SpriteBatch b, int slotX, int slotY)
		{
			base.draw(b, slotX, slotY);
			int num = 46;
			buttonLeftArrow.bounds = new Rectangle(slotX + bounds.X, slotY + bounds.Y + 50, 80, 80);
			buttonLeftArrow.draw(b);
			buttonRightArrow.bounds = new Rectangle(slotX + bounds.X + bounds.Width - 80, slotY + bounds.Y + 50, 80, 80);
			buttonRightArrow.draw(b);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, sliderBGSource, slotX + bounds.X + 100, slotY + bounds.Y + num + 20, bounds.Width - 200, 40, Color.White, 4f, drawShadow: false);
			int num2 = bounds.X + 100;
			int num3 = num2 + bounds.Width - 200 - 80;
			_buttonStartX = num2 + (int)(_percent * (float)(num3 - num2));
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(256, 256, 10, 10), _buttonStartX, slotY + bounds.Y + num, 80, 80, Color.White, 4f, drawShadow: false);
		}
	}
}
