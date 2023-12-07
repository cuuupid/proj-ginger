using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	public class SpecialCurrencyDisplay
	{
		public class CurrencyDisplayType
		{
			public string key;

			public NetIntDelta netIntDelta;

			public Action<int> playSound;

			public Action<SpriteBatch, Vector2> drawSprite;
		}

		protected MoneyDial _moneyDial;

		protected float currentPosition;

		protected CurrencyDisplayType _currentDisplayedCurrency;

		public Dictionary<string, CurrencyDisplayType> registeredCurrencyDisplays;

		public float timeToLive;

		public Action<SpriteBatch, Vector2> drawSprite;

		public CurrencyDisplayType forcedOnscreenCurrencyType;

		public SpecialCurrencyDisplay()
		{
			_moneyDial = new MoneyDial(3);
			_moneyDial.onPlaySound = null;
			drawSprite = null;
			registeredCurrencyDisplays = new Dictionary<string, CurrencyDisplayType>();
		}

		public virtual void Register(string key, NetIntDelta net_int_delta, Action<int> sound_function = null, Action<SpriteBatch, Vector2> draw_function = null)
		{
			if (registeredCurrencyDisplays.ContainsKey(key))
			{
				Unregister(key);
			}
			CurrencyDisplayType currencyDisplayType = new CurrencyDisplayType();
			currencyDisplayType.key = key;
			currencyDisplayType.netIntDelta = net_int_delta;
			currencyDisplayType.playSound = sound_function;
			currencyDisplayType.drawSprite = draw_function;
			registeredCurrencyDisplays[key] = currencyDisplayType;
			registeredCurrencyDisplays[key].netIntDelta.fieldChangeVisibleEvent += OnCurrencyChange;
		}

		public virtual void ShowCurrency(string currency_type)
		{
			if (currency_type == null || !registeredCurrencyDisplays.ContainsKey(currency_type))
			{
				forcedOnscreenCurrencyType = null;
				return;
			}
			forcedOnscreenCurrencyType = registeredCurrencyDisplays[currency_type];
			SetDisplayedCurrency(forcedOnscreenCurrencyType);
		}

		public virtual void OnCurrencyChange(NetIntDelta field, int old_value, int new_value)
		{
			if (Game1.gameMode != 3)
			{
				return;
			}
			string text = null;
			foreach (string key in registeredCurrencyDisplays.Keys)
			{
				if (registeredCurrencyDisplays[key].netIntDelta == field)
				{
					text = key;
					break;
				}
			}
			if (text == null)
			{
				return;
			}
			SetDisplayedCurrency(text);
			if (_currentDisplayedCurrency != null)
			{
				_moneyDial.currentValue = old_value;
				if (_moneyDial.onPlaySound != null)
				{
					_moneyDial.onPlaySound(new_value - old_value);
				}
			}
			timeToLive = 5f;
		}

		public virtual void SetDisplayedCurrency(CurrencyDisplayType currency_type)
		{
			if (currency_type == _currentDisplayedCurrency || (forcedOnscreenCurrencyType != null && forcedOnscreenCurrencyType != currency_type))
			{
				return;
			}
			_moneyDial.onPlaySound = null;
			drawSprite = null;
			_currentDisplayedCurrency = currency_type;
			if (currency_type != null)
			{
				_moneyDial.currentValue = _currentDisplayedCurrency.netIntDelta.Value;
				_moneyDial.previousTargetValue = _moneyDial.currentValue;
				if (currency_type.playSound != null)
				{
					_moneyDial.onPlaySound = currency_type.playSound;
				}
				else
				{
					_moneyDial.onPlaySound = DefaultPlaySound;
				}
				if (currency_type.drawSprite != null)
				{
					drawSprite = currency_type.drawSprite;
				}
				else
				{
					drawSprite = DefaultDrawSprite;
				}
			}
		}

		public virtual void SetDisplayedCurrency(string key)
		{
			if (registeredCurrencyDisplays.ContainsKey(key))
			{
				CurrencyDisplayType displayedCurrency = registeredCurrencyDisplays[key];
				SetDisplayedCurrency(displayedCurrency);
			}
		}

		public virtual void Unregister(string key)
		{
			if (registeredCurrencyDisplays.ContainsKey(key))
			{
				if (_currentDisplayedCurrency == registeredCurrencyDisplays[key])
				{
					SetDisplayedCurrency((CurrencyDisplayType)null);
				}
				registeredCurrencyDisplays[key].netIntDelta.fieldChangeVisibleEvent -= OnCurrencyChange;
				registeredCurrencyDisplays.Remove(key);
			}
		}

		public virtual void Cleanup()
		{
			List<string> list = new List<string>(registeredCurrencyDisplays.Keys);
			foreach (string item in list)
			{
				Unregister(item);
			}
		}

		public virtual void DefaultDrawSprite(SpriteBatch b, Vector2 position)
		{
			if (_currentDisplayedCurrency != null)
			{
				if (_currentDisplayedCurrency.key == "walnuts")
				{
					b.Draw(Game1.objectSpriteSheet, position, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 73, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				}
				else if (_currentDisplayedCurrency.key == "qiGems")
				{
					b.Draw(Game1.objectSpriteSheet, position, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				}
			}
		}

		public virtual void DefaultPlaySound(int direction)
		{
			if (_currentDisplayedCurrency != null)
			{
				if (direction < 0 && _currentDisplayedCurrency.key == "walnuts")
				{
					Game1.playSound("goldenWalnut");
				}
				if (direction > 0 && _currentDisplayedCurrency.key == "walnuts")
				{
					Game1.playSound("goldenWalnut");
				}
			}
		}

		public virtual void Update(GameTime time)
		{
			if (timeToLive > 0f)
			{
				timeToLive -= (float)time.ElapsedGameTime.TotalSeconds;
				if (timeToLive < 0f)
				{
					timeToLive = 0f;
				}
			}
			if (timeToLive > 0f || forcedOnscreenCurrencyType != null)
			{
				currentPosition += (float)time.ElapsedGameTime.TotalSeconds / 0.5f;
			}
			else
			{
				currentPosition -= (float)time.ElapsedGameTime.TotalSeconds / 0.5f;
			}
			currentPosition = Utility.Clamp(currentPosition, 0f, 1f);
		}

		public Vector2 GetUpperLeft()
		{
			return new Vector2((Game1.activeClickableMenu != null) ? ((float)Game1.uiViewport.Width * 0.45f) : ((float)(Toolbar.toolbarWidth + 12)), (int)Utility.Lerp(-26f, 0f, currentPosition) * 4);
		}

		public virtual void Draw(SpriteBatch b)
		{
			if (_currentDisplayedCurrency != null && !(currentPosition <= 0f))
			{
				Vector2 upperLeft = GetUpperLeft();
				b.Draw(Game1.mouseCursors2, upperLeft, new Rectangle(48, 176, 52, 26), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				int target = _currentDisplayedCurrency.netIntDelta.Value;
				if (currentPosition < 0.5f)
				{
					target = _moneyDial.previousTargetValue;
				}
				_moneyDial.draw(b, upperLeft + new Vector2(108f, 40f), target);
				if (drawSprite != null)
				{
					drawSprite(b, upperLeft + new Vector2(4f, 6f) * 4f);
				}
			}
		}
	}
}
