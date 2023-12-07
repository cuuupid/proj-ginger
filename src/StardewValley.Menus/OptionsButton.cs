using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class OptionsButton : OptionsElement
	{
		public ClickableComponent button;

		private Action _callback;

		private string _label;

		private const int paddingX = 32;

		private int paddingY = ((Game1.uiViewport.Height < 600) ? 4 : 16);

		private bool isHeldDown;

		private bool _enabled = true;

		public bool enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		public override int ItemHeight => 92;

		public OptionsButton(string label, Action callback, int x = -1, int y = -1)
			: base(label, x, y, Game1.uiViewport.Width / 2, OptionsElement.optionsItemHeight)
		{
			_label = label;
			_callback = callback;
			int num = (int)Game1.dialogueFont.MeasureString(_label).X + 64;
			int num2 = (int)Game1.dialogueFont.MeasureString(_label).Y + paddingY * 2;
			bounds = new Rectangle(x, y, num, num2);
			button = new ClickableComponent(bounds, "OptionsButton_" + _label);
		}

		public override void receiveLeftClick(int x, int y)
		{
			if (enabled && bounds.Contains(x, y))
			{
				isHeldDown = true;
				Game1.playSound("smallSelect");
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			isHeldDown = false;
			if (enabled && bounds.Contains(x, y) && _callback != null)
			{
				_callback();
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			if (enabled && !bounds.Contains(x, y))
			{
				isHeldDown = false;
			}
		}

		public override void draw(SpriteBatch b, int slotX, int slotY)
		{
			IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.dialogueFont, Game1.mouseCursors, new Rectangle(256, 256, 10, 10), null, new Rectangle(-1, -1, -1, -1), _label, slotX + bounds.X - (isHeldDown ? 4 : 0), slotY + bounds.Y + (isHeldDown ? 4 : 0), button.bounds.Width, button.bounds.Height, enabled ? Color.White : Color.Gray, 4f, !isHeldDown, iconLeft: true, isClickable: true, heldDown: false, drawIcon: true, reverseColors: false, bold: false);
		}
	}
}
