using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	public class EmoteMenu : IClickableMenu
	{
		public Texture2D menuBackgroundTexture;

		public List<string> emotes;

		protected Point _mouseStartPosition;

		public bool _hasSelectedEmote;

		protected List<ClickableTextureComponent> _emoteButtons;

		protected string _selectedEmote;

		protected int _selectedIndex = -1;

		protected int _oldSelection;

		protected int _selectedTime;

		protected float _alpha;

		protected int _menuCloseGracePeriod = -1;

		protected int _age;

		public bool gamepadMode;

		protected int _expandTime = 200;

		protected int _expandedButtonRadius = 24;

		protected int _buttonRadius;

		public Vector2 _oldDrawPosition;

		public EmoteMenu()
		{
			menuBackgroundTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\EmoteMenu");
			width = 256;
			height = 256;
			xPositionOnScreen = (int)((float)(Game1.viewport.Width / 2) - (float)width / 2f);
			yPositionOnScreen = (int)((float)(Game1.viewport.Height / 2) - (float)height / 2f);
			emotes = new List<string>();
			foreach (string emoteFavorite in Game1.player.GetEmoteFavorites())
			{
				emotes.Add(emoteFavorite);
			}
			_mouseStartPosition = Game1.getMousePosition(ui_scale: false);
			_alpha = 0f;
			_menuCloseGracePeriod = 300;
			_CreateEmoteButtons();
			_SnapToPlayerPosition();
		}

		protected void _CreateEmoteButtons()
		{
			_emoteButtons = new List<ClickableTextureComponent>();
			for (int i = 0; i < emotes.Count; i++)
			{
				int emote_index = -1;
				for (int j = 0; j < Farmer.EMOTES.Count(); j++)
				{
					if (Farmer.EMOTES[j].emoteString == emotes[i])
					{
						emote_index = j;
						break;
					}
				}
				ClickableTextureComponent item = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), menuBackgroundTexture, GetEmoteNonBubbleSpriteRect(emote_index), 4f);
				_emoteButtons.Add(item);
			}
			_RepositionButtons();
		}

		public static Rectangle GetEmoteSpriteRect(int emote_index)
		{
			if (emote_index <= 0)
			{
				return new Rectangle(48, 0, 16, 16);
			}
			return new Rectangle(emote_index % 4 * 16 + 48, emote_index / 4 * 16, 16, 16);
		}

		public static Rectangle GetEmoteNonBubbleSpriteRect(int emote_index)
		{
			return new Rectangle(emote_index % 4 * 16, emote_index / 4 * 16, 16, 16);
		}

		public override void applyMovementKey(int direction)
		{
		}

		protected override void cleanupBeforeExit()
		{
			if (Game1.emoteMenu != null)
			{
				Game1.emoteMenu = null;
			}
			Game1.oldMouseState = Game1.input.GetMouseState();
			base.cleanupBeforeExit();
		}

		public override void performHoverAction(int x, int y)
		{
			x = (int)Utility.ModifyCoordinateFromUIScale(x);
			y = (int)Utility.ModifyCoordinateFromUIScale(y);
			if (gamepadMode)
			{
				return;
			}
			for (int i = 0; i < _emoteButtons.Count; i++)
			{
				ClickableTextureComponent clickableTextureComponent = _emoteButtons[i];
				if (clickableTextureComponent.containsPoint(x, y))
				{
					_selectedEmote = emotes[i];
					_selectedIndex = i;
					if (_selectedIndex != _oldSelection)
					{
						_selectedTime = 0;
					}
					return;
				}
			}
			_selectedEmote = null;
			_selectedIndex = -1;
		}

		protected void _RepositionButtons()
		{
			for (int i = 0; i < _emoteButtons.Count; i++)
			{
				ClickableTextureComponent clickableTextureComponent = _emoteButtons[i];
				float num = Utility.Lerp(0f, (float)Math.PI * 2f, (float)i / (float)_emoteButtons.Count);
				clickableTextureComponent.bounds.X = (int)((float)(xPositionOnScreen + width / 2 + (int)(Math.Cos(num) * (double)_buttonRadius) * 4) - (float)clickableTextureComponent.bounds.Width / 2f);
				clickableTextureComponent.bounds.Y = (int)((float)(yPositionOnScreen + height / 2 + (int)((0.0 - Math.Sin(num)) * (double)_buttonRadius) * 4) - (float)clickableTextureComponent.bounds.Height / 2f);
			}
		}

		protected void _SnapToPlayerPosition()
		{
			if (Game1.player != null)
			{
				Vector2 vector = Game1.player.getLocalPosition(Game1.viewport) + new Vector2((float)(-width) / 2f, (float)(-height) / 2f);
				xPositionOnScreen = (int)vector.X + 32;
				yPositionOnScreen = (int)vector.Y - 64;
				if (xPositionOnScreen + width > Game1.viewport.Width)
				{
					xPositionOnScreen -= xPositionOnScreen + width - Game1.viewport.Width;
				}
				if (xPositionOnScreen < 0)
				{
					xPositionOnScreen -= xPositionOnScreen;
				}
				if (yPositionOnScreen + height > Game1.viewport.Height)
				{
					yPositionOnScreen -= yPositionOnScreen + height - Game1.viewport.Height;
				}
				if (yPositionOnScreen < 0)
				{
					yPositionOnScreen -= yPositionOnScreen;
				}
				_RepositionButtons();
			}
		}

		public override void update(GameTime time)
		{
			_age += time.ElapsedGameTime.Milliseconds;
			if (_age > _expandTime)
			{
				_age = _expandTime;
			}
			if (!gamepadMode && Game1.options.gamepadControls && (Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.X) > 0.5f || Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.Y) > 0.5f))
			{
				gamepadMode = true;
			}
			_alpha = (float)_age / (float)_expandTime;
			_buttonRadius = (int)((float)_age / (float)_expandTime * (float)_expandedButtonRadius);
			_SnapToPlayerPosition();
			Vector2 value = default(Vector2);
			if (gamepadMode)
			{
				_mouseStartPosition = Game1.getMousePosition(ui_scale: false);
				if (Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.X) > 0.5f || Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.Y) > 0.5f)
				{
					_hasSelectedEmote = true;
					value = new Vector2(Game1.input.GetGamePadState().ThumbSticks.Right.X, Game1.input.GetGamePadState().ThumbSticks.Right.Y);
					value.Y *= -1f;
					value.Normalize();
					float num = -1f;
					for (int i = 0; i < _emoteButtons.Count; i++)
					{
						float num2 = Vector2.Dot(value2: new Vector2((float)_emoteButtons[i].bounds.Center.X - ((float)xPositionOnScreen + (float)width / 2f), (float)_emoteButtons[i].bounds.Center.Y - ((float)yPositionOnScreen + (float)height / 2f)), value1: value);
						if (num2 > num)
						{
							num = num2;
							_selectedEmote = emotes[i];
							_selectedIndex = i;
						}
					}
					_menuCloseGracePeriod = 100;
					if (Game1.input.GetGamePadState().IsButtonDown(Buttons.Back) && _selectedIndex >= 0)
					{
						Game1.activeClickableMenu = new EmoteSelector(_selectedIndex, emotes[_selectedIndex]);
						exitThisMenuNoSound();
						return;
					}
				}
				else
				{
					if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick) && _menuCloseGracePeriod < 100)
					{
						_menuCloseGracePeriod = 100;
					}
					if (_menuCloseGracePeriod >= 0)
					{
						_menuCloseGracePeriod -= time.ElapsedGameTime.Milliseconds;
					}
					if (_menuCloseGracePeriod <= 0 && !Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick))
					{
						ConfirmSelection();
					}
				}
			}
			for (int j = 0; j < _emoteButtons.Count; j++)
			{
				if (_emoteButtons[j].scale > 4f)
				{
					_emoteButtons[j].scale = Utility.MoveTowards(_emoteButtons[j].scale, 4f, (float)time.ElapsedGameTime.Milliseconds / 1000f * 10f);
				}
			}
			if (_selectedEmote != null && _selectedIndex > -1)
			{
				_emoteButtons[_selectedIndex].scale = 5f;
			}
			if (_oldSelection != _selectedIndex)
			{
				_oldSelection = _selectedIndex;
				_selectedTime = 0;
			}
			_selectedTime += time.ElapsedGameTime.Milliseconds;
			base.update(time);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			x = (int)Utility.ModifyCoordinateFromUIScale(x);
			y = (int)Utility.ModifyCoordinateFromUIScale(y);
			for (int i = 0; i < _emoteButtons.Count; i++)
			{
				if (_emoteButtons[i].containsPoint(x, y) && Game1.activeClickableMenu == null)
				{
					Game1.activeClickableMenu = new EmoteSelector(i, emotes[i]);
					exitThisMenuNoSound();
					return;
				}
			}
			base.receiveLeftClick(x, y, playSound);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			x = (int)Utility.ModifyCoordinateFromUIScale(x);
			y = (int)Utility.ModifyCoordinateFromUIScale(y);
			ConfirmSelection();
			base.receiveLeftClick(x, y, playSound);
		}

		public void ConfirmSelection()
		{
			if (_selectedEmote != null)
			{
				Game1.chatBox.textBoxEnter("/emote " + _selectedEmote);
			}
			exitThisMenu(playSound: false);
		}

		public override void draw(SpriteBatch b)
		{
			Game1.StartWorldDrawInUI(b);
			Color white = Color.White;
			white.A = (byte)Utility.Lerp(0f, 255f, _alpha);
			foreach (ClickableTextureComponent emoteButton in _emoteButtons)
			{
				emoteButton.draw(b, white, 0.86f);
			}
			if (_selectedEmote != null)
			{
				Farmer.EmoteType[] eMOTES = Farmer.EMOTES;
				foreach (Farmer.EmoteType emoteType in eMOTES)
				{
					if (emoteType.emoteString == _selectedEmote)
					{
						SpriteText.drawStringWithScrollCenteredAt(b, emoteType.displayName, xPositionOnScreen + width / 2, yPositionOnScreen + height);
						break;
					}
				}
			}
			if (_selectedIndex >= 0 && _selectedTime >= 250)
			{
				ClickableTextureComponent clickableTextureComponent = _emoteButtons[_selectedIndex];
				Vector2 position = Utility.PointToVector2(clickableTextureComponent.bounds.Center);
				position.X += 16f;
				if (!gamepadMode)
				{
					position = Utility.PointToVector2(Game1.getMousePosition(ui_scale: false)) + new Vector2(32f, 32f);
					b.Draw(menuBackgroundTexture, position, new Rectangle(64, 0, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.99f);
				}
				else
				{
					b.Draw(Game1.controllerMaps, position, Utility.controllerMapSourceRect(new Rectangle(625, 260, 28, 28)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
				}
				position.X += 32f;
				b.Draw(menuBackgroundTexture, position, new Rectangle(64, 16, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.99f);
			}
			Game1.EndWorldDrawInUI(b);
		}
	}
}
