using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class TextBox : IKeyboardSubscriber
	{
		protected Texture2D _textBoxTexture;

		protected Texture2D _caretTexture;

		protected SpriteFont _font;

		public bool drawBackground;

		public bool centerText;

		protected Color _textColor;

		public bool numbersOnly;

		public int textLimit = -1;

		public bool limitWidth = true;

		private string _text = "";

		protected bool _showKeyboard;

		private bool _selected;

		public bool isScroll { get; set; }

		public SpriteFont Font => _font;

		public Color TextColor => _textColor;

		public int X { get; set; }

		public int Y { get; set; }

		public int Width { get; set; }

		public int Height { get; set; }

		public bool PasswordBox { get; set; }

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				string text = _text;
				_text = value;
				if (_text == null)
				{
					_text = "";
				}
				if (!(_text != ""))
				{
					return;
				}
				string text2 = "";
				for (int i = 0; i < value.Length; i++)
				{
					char c = value[i];
					if (_font.Characters.Contains(c) && c != '\n' && c != '\r')
					{
						text2 += c;
					}
				}
				_text = text2;
				if ((limitWidth && _font.MeasureString(_text).X > (float)(Width - 21)) || (isScroll && limitWidth && _font.MeasureString(_text).X > (float)(Width - 48 - 21)))
				{
					Text = _text.Substring(0, _text.Length - 1);
				}
				if (Text.Length > textLimit && textLimit > 0)
				{
					Text = Text.Substring(0, textLimit);
				}
			}
		}

		public string TitleText { get; set; }

		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				if (_selected == value)
				{
					return;
				}
				_selected = value;
				if (_selected)
				{
					Game1.keyboardDispatcher.Subscriber = this;
					_showKeyboard = true;
					return;
				}
				_showKeyboard = false;
				if (Game1.keyboardDispatcher.Subscriber == this)
				{
					Game1.keyboardDispatcher.Subscriber = null;
				}
			}
		}

		public event TextBoxEvent OnEnterPressed;

		public event TextBoxEvent OnTabPressed;

		public event TextBoxEvent OnBackspacePressed;

		public void setTextColor(Color newCol)
		{
			_textColor = newCol;
		}

		public TextBox(Texture2D textBoxTexture, Texture2D caretTexture, SpriteFont font, Color textColor, bool drawBackground = true, bool centerText = false)
		{
			this.drawBackground = drawBackground;
			this.centerText = centerText;
			_textBoxTexture = textBoxTexture;
			if (textBoxTexture != null)
			{
				Width = textBoxTexture.Width;
				Height = textBoxTexture.Height;
			}
			_caretTexture = caretTexture;
			_font = font;
			_textColor = textColor;
		}

		public void SelectMe()
		{
			Selected = true;
		}

		public virtual void Update()
		{
			Point value = new Point(Game1.getMouseX(), Game1.getMouseY());
			Selected = new Rectangle(X, Y, Width, Height).Contains(value);
			if (_showKeyboard)
			{
				ShowAndroidKeyboard();
				_showKeyboard = false;
			}
			else
			{
				HideStatusBar();
			}
		}

		protected virtual void ShowAndroidKeyboard()
		{
			Task<string> task = KeyboardInput.Show("", "", Text);
			task.ContinueWith((Task<string> s) => Text = s.Result);
			Selected = false;
		}

		public void HideStatusBar()
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat && Game.Activity.Window.DecorView.SystemUiVisibility != (StatusBarVisibility)5894)
			{
				Game.Activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)5894;
			}
		}

		public virtual void Draw(SpriteBatch spriteBatch, bool drawShadow = true)
		{
			bool flag = true;
			flag = DateTime.UtcNow.Millisecond % 1000 >= 500;
			string text = Text;
			if (PasswordBox)
			{
				text = "";
				for (int i = 0; i < Text.Length; i++)
				{
					text += "•";
				}
			}
			if (_textBoxTexture != null)
			{
				spriteBatch.Draw(_textBoxTexture, new Rectangle(X, Y, 16, Height), new Rectangle(0, 0, 16, Height), Color.White);
				spriteBatch.Draw(_textBoxTexture, new Rectangle(X + 16, Y, Width - 32, Height), new Rectangle(16, 0, 4, Height), Color.White);
				spriteBatch.Draw(_textBoxTexture, new Rectangle(X + Width - 16, Y, 16, Height), new Rectangle(_textBoxTexture.Bounds.Width - 16, 0, 16, Height), Color.White);
			}
			else if (isScroll)
			{
				spriteBatch.Draw(Game1.mouseCursors, new Rectangle(X, Y, 48, 72), new Rectangle(325, 318, 12, 18), Color.White);
				spriteBatch.Draw(Game1.mouseCursors, new Rectangle(X + 48, Y, Width - 96, 72), new Rectangle(337, 318, 1, 18), Color.White);
				spriteBatch.Draw(Game1.mouseCursors, new Rectangle(X + Width - 48, Y, 48, 72), new Rectangle(338, 318, 12, 18), Color.White);
			}
			if (drawBackground && !isScroll)
			{
				IClickableMenu.drawTextureBox(spriteBatch, X, Y - 12, Width + 16, 80, Color.White);
			}
			Vector2 vector = _font.MeasureString(text);
			while (vector.X > (float)Width)
			{
				text = text.Substring(1);
				vector = _font.MeasureString(text);
			}
			int num = ((_font == Game1.smallFont) ? 8 : 0);
			int num2 = (int)((float)Width - _font.MeasureString(text).X - 48f) / 2;
			if (flag && Selected)
			{
				if (isScroll)
				{
					spriteBatch.Draw(Game1.staminaRect, new Rectangle(X + num2 + 16 + (int)vector.X + 2, Y + 18, 4, 32), _textColor);
					Utility.drawTextWithShadow(spriteBatch, text, _font, new Vector2(X + num2 + 16, Y + 4 + ((_textBoxTexture != null) ? 12 : 8) + num), _textColor);
				}
				else
				{
					spriteBatch.Draw(Game1.staminaRect, new Rectangle(X + 16 + (int)vector.X + 2, Y + 16, 4, 32), _textColor);
					Utility.drawTextWithShadow(spriteBatch, text, _font, new Vector2(X + 16, Y + 4 + ((_textBoxTexture != null) ? 12 : 8) + num), _textColor);
				}
			}
			else if (isScroll || centerText)
			{
				Utility.drawTextWithShadow(spriteBatch, text, _font, new Vector2(X + num2 + 16, Y + 4 + ((_textBoxTexture != null) ? 12 : 8) + num), _textColor);
			}
			else
			{
				Utility.drawTextWithShadow(spriteBatch, text, _font, new Vector2(X + 16, Y + 4 + ((_textBoxTexture != null) ? 12 : 8) + num), _textColor, 1f, 1f);
			}
		}

		public virtual void RecieveTextInput(char inputChar)
		{
			if (!Selected || (numbersOnly && !char.IsDigit(inputChar)) || (textLimit != -1 && Text.Length >= textLimit))
			{
				return;
			}
			if (Game1.gameMode != 3)
			{
				switch (inputChar)
				{
				case '+':
					Game1.playSound("slimeHit");
					break;
				case '*':
					Game1.playSound("hammer");
					break;
				case '=':
					Game1.playSound("coin");
					break;
				case '<':
					Game1.playSound("crystal");
					break;
				case '$':
					Game1.playSound("money");
					break;
				case '"':
					return;
				default:
					Game1.playSound("cowboy_monsterhit");
					break;
				}
			}
			Text += inputChar;
		}

		public virtual void RecieveTextInput(string text)
		{
			int result = -1;
			if (Selected && (!numbersOnly || int.TryParse(text, out result)) && (textLimit == -1 || Text.Length < textLimit - text.Length))
			{
				Text += text;
			}
		}

		public virtual void RecieveCommandInput(char command)
		{
			if (!Selected)
			{
				return;
			}
			switch (command)
			{
			case '\b':
				if (Text.Length <= 0)
				{
					break;
				}
				if (this.OnBackspacePressed != null)
				{
					this.OnBackspacePressed(this);
					break;
				}
				Text = Text.Substring(0, Text.Length - 1);
				if (Game1.gameMode != 3)
				{
					Game1.playSound("tinyWhip");
				}
				break;
			case '\r':
				if (this.OnEnterPressed != null)
				{
					_text = Utility.RemoveDodgyChars(_text);
					this.OnEnterPressed(this);
				}
				break;
			case '\t':
				if (this.OnTabPressed != null)
				{
					this.OnTabPressed(this);
				}
				break;
			}
		}

		public void PressEnter()
		{
			if (this.OnEnterPressed != null)
			{
				_text = Utility.RemoveDodgyChars(_text);
				this.OnEnterPressed(this);
			}
		}

		public void RecieveSpecialInput(Keys key)
		{
		}

		public void Hover(int x, int y)
		{
			if (x > X && x < X + Width && y > Y && y < Y + Height)
			{
				Game1.SetFreeCursorDrag();
			}
		}
	}
}
