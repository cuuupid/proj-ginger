using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Content;
using StardewValley.GameData;

namespace StardewValley
{
	public class LocalizedContentManager : ContentManager
	{
		public delegate void LanguageChangedHandler(LanguageCode code);

		public enum LanguageCode
		{
			en,
			ja,
			ru,
			zh,
			pt,
			es,
			de,
			th,
			fr,
			ko,
			it,
			tr,
			hu,
			mod
		}

		public const string genderDialogueSplitCharacter = "¦";

		private static LanguageCode _currentLangCode = GetDefaultLanguageCode();

		private static ModLanguage _currentModLanguage = null;

		public CultureInfo CurrentCulture;

		protected static StringBuilder _timeFormatStringBuilder = new StringBuilder();

		public static readonly Dictionary<string, string> localizedAssetNames = new Dictionary<string, string>();

		public static LanguageCode CurrentLanguageCode
		{
			get
			{
				return _currentLangCode;
			}
			set
			{
				if (_currentLangCode != value)
				{
					LanguageCode currentLangCode = _currentLangCode;
					_currentLangCode = value;
					if (_currentLangCode != LanguageCode.mod)
					{
						_currentModLanguage = null;
					}
					Console.WriteLine("LocalizedContentManager.CurrentLanguageCode CHANGING from '{0}' to '{1}'", currentLangCode, _currentLangCode);
					LocalizedContentManager.OnLanguageChange?.Invoke(_currentLangCode);
					Console.WriteLine("LocalizedContentManager.CurrentLanguageCode CHANGED from '{0}' to '{1}'", currentLangCode, _currentLangCode);
				}
			}
		}

		public static bool CurrentLanguageLatin
		{
			get
			{
				if (CurrentLanguageCode != 0 && CurrentLanguageCode != LanguageCode.es && CurrentLanguageCode != LanguageCode.de && CurrentLanguageCode != LanguageCode.pt && CurrentLanguageCode != LanguageCode.fr && CurrentLanguageCode != LanguageCode.it && CurrentLanguageCode != LanguageCode.tr && CurrentLanguageCode != LanguageCode.hu)
				{
					if (CurrentLanguageCode == LanguageCode.mod)
					{
						return _currentModLanguage.UseLatinFont;
					}
					return false;
				}
				return true;
			}
		}

		public static ModLanguage CurrentModLanguage => _currentModLanguage;

		public static event LanguageChangedHandler OnLanguageChange;

		public static LanguageCode GetDefaultLanguageCode()
		{
			return LanguageCode.en;
		}

		public LocalizedContentManager(IServiceProvider serviceProvider, string rootDirectory, CultureInfo currentCulture)
			: base(serviceProvider, rootDirectory)
		{
			CurrentCulture = currentCulture;
		}

		public LocalizedContentManager(IServiceProvider serviceProvider, string rootDirectory)
			: this(serviceProvider, rootDirectory, Thread.CurrentThread.CurrentUICulture)
		{
		}

		protected static bool _IsStringAt(string source, string string_to_find, int index)
		{
			for (int i = 0; i < string_to_find.Length; i++)
			{
				int num = index + i;
				if (num >= source.Length)
				{
					return false;
				}
				if (source[num] != string_to_find[i])
				{
					return false;
				}
			}
			return true;
		}

		public static StringBuilder FormatTimeString(int time, string format)
		{
			_timeFormatStringBuilder.Clear();
			int num = -1;
			for (int i = 0; i < format.Length; i++)
			{
				char c = format[i];
				switch (c)
				{
				case '[':
				{
					if (num < 0)
					{
						num = i;
						continue;
					}
					for (int k = num; k <= i; k++)
					{
						_timeFormatStringBuilder.Append(format[k]);
					}
					num = i;
					continue;
				}
				case ']':
					if (num < 0)
					{
						break;
					}
					if (_IsStringAt(format, "[HOURS_12]", num))
					{
						_timeFormatStringBuilder.Append((time / 100 % 12 == 0) ? "12" : (time / 100 % 12).ToString());
					}
					else if (_IsStringAt(format, "[HOURS_12_0]", num))
					{
						_timeFormatStringBuilder.Append((time / 100 % 12 == 0) ? "0" : (time / 100 % 12).ToString());
					}
					else if (_IsStringAt(format, "[HOURS_24]", num))
					{
						_timeFormatStringBuilder.Append(time / 100 % 24);
					}
					else if (_IsStringAt(format, "[HOURS_24_00]", num))
					{
						_timeFormatStringBuilder.Append((time / 100 % 24).ToString("00"));
					}
					else if (_IsStringAt(format, "[MINUTES]", num))
					{
						_timeFormatStringBuilder.Append((time % 100).ToString("00"));
					}
					else if (_IsStringAt(format, "[AM_PM]", num))
					{
						if (time < 1200 || time >= 2400)
						{
							_timeFormatStringBuilder.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370"));
						}
						else
						{
							_timeFormatStringBuilder.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371"));
						}
					}
					else
					{
						for (int j = num; j <= i; j++)
						{
							_timeFormatStringBuilder.Append(format[j]);
						}
					}
					num = -1;
					continue;
				}
				if (num < 0)
				{
					_timeFormatStringBuilder.Append(c);
				}
			}
			return _timeFormatStringBuilder;
		}

		public static void SetModLanguage(ModLanguage new_mod_language)
		{
			if (new_mod_language != _currentModLanguage)
			{
				_currentModLanguage = new_mod_language;
				CurrentLanguageCode = LanguageCode.mod;
			}
		}

		public virtual T LoadBase<T>(string assetName)
		{
			return base.Load<T>(assetName);
		}

		public override T Load<T>(string assetName)
		{
			return Load<T>(assetName, CurrentLanguageCode);
		}

		public virtual T Load<T>(string assetName, LanguageCode language)
		{
			if (language != 0)
			{
				if (!localizedAssetNames.TryGetValue(assetName, out var _))
				{
					string text = assetName + "." + LanguageCodeString(language);
					try
					{
						T val = base.Load<T>(text);
						localizedAssetNames[assetName] = text;
					}
					catch (ContentLoadException)
					{
						text = assetName + "_international";
						try
						{
							T val2 = base.Load<T>(text);
							localizedAssetNames[assetName] = text;
						}
						catch (ContentLoadException)
						{
							localizedAssetNames[assetName] = assetName;
						}
					}
				}
				return base.Load<T>(localizedAssetNames[assetName]);
			}
			bool flag = false;
			return base.Load<T>(assetName);
		}

		public string LanguageCodeString(LanguageCode code)
		{
			string result = "";
			switch (code)
			{
			case LanguageCode.ja:
				result = "ja-JP";
				break;
			case LanguageCode.ru:
				result = "ru-RU";
				break;
			case LanguageCode.zh:
				result = "zh-CN";
				break;
			case LanguageCode.pt:
				result = "pt-BR";
				break;
			case LanguageCode.es:
				result = "es-ES";
				break;
			case LanguageCode.de:
				result = "de-DE";
				break;
			case LanguageCode.th:
				result = "th-TH";
				break;
			case LanguageCode.fr:
				result = "fr-FR";
				break;
			case LanguageCode.ko:
				result = "ko-KR";
				break;
			case LanguageCode.it:
				result = "it-IT";
				break;
			case LanguageCode.tr:
				result = "tr-TR";
				break;
			case LanguageCode.hu:
				result = "hu-HU";
				break;
			case LanguageCode.mod:
				result = _currentModLanguage.LanguageCode;
				break;
			}
			return result;
		}

		public LanguageCode GetCurrentLanguage()
		{
			return CurrentLanguageCode;
		}

		private string GetString(Dictionary<string, string> strings, string key)
		{
			if (strings.TryGetValue(key + ".mobile", out var value))
			{
				return value;
			}
			return strings[key];
		}

		public virtual string LoadStringReturnNullIfNotFound(string path)
		{
			string text = LoadString(path);
			if (!text.Equals(path))
			{
				return text;
			}
			return null;
		}

		public virtual string LoadString(string path)
		{
			parseStringPath(path, out var assetName, out var key);
			Dictionary<string, string> dictionary = Load<Dictionary<string, string>>(assetName);
			if (dictionary != null && dictionary.ContainsKey(key))
			{
				string text = GetString(dictionary, key);
				if (text.Contains("¦"))
				{
					text = ((!Game1.player.IsMale) ? text.Substring(text.IndexOf("¦") + 1) : text.Substring(0, text.IndexOf("¦")));
				}
				return text;
			}
			return LoadBaseString(path);
		}

		public virtual bool ShouldUseGenderedCharacterTranslations()
		{
			if (CurrentLanguageCode == LanguageCode.pt)
			{
				return true;
			}
			if (CurrentLanguageCode == LanguageCode.mod && CurrentModLanguage != null)
			{
				return CurrentModLanguage.UseGenderedCharacterTranslations;
			}
			return false;
		}

		public virtual string LoadString(string path, object sub1)
		{
			string text = LoadString(path);
			try
			{
				return string.Format(text, sub1);
			}
			catch (Exception)
			{
				return text;
			}
		}

		public virtual string LoadString(string path, object sub1, object sub2)
		{
			string text = LoadString(path);
			try
			{
				return string.Format(text, sub1, sub2);
			}
			catch (Exception)
			{
				return text;
			}
		}

		public virtual string LoadString(string path, object sub1, object sub2, object sub3)
		{
			string text = LoadString(path);
			try
			{
				return string.Format(text, sub1, sub2, sub3);
			}
			catch (Exception)
			{
				return text;
			}
		}

		public virtual string LoadString(string path, params object[] substitutions)
		{
			string text = LoadString(path);
			if (substitutions.Length != 0)
			{
				try
				{
					return string.Format(text, substitutions);
				}
				catch (Exception)
				{
					return text;
				}
			}
			return text;
		}

		public virtual string LoadBaseString(string path)
		{
			parseStringPath(path, out var assetName, out var key);
			Dictionary<string, string> dictionary = base.Load<Dictionary<string, string>>(assetName);
			if (dictionary != null && dictionary.ContainsKey(key))
			{
				return GetString(dictionary, key);
			}
			return path;
		}

		private void parseStringPath(string path, out string assetName, out string key)
		{
			int num = path.IndexOf(':');
			if (num == -1)
			{
				throw new ContentLoadException("Unable to parse string path: " + path);
			}
			assetName = path.Substring(0, num);
			key = path.Substring(num + 1, path.Length - num - 1);
		}

		public virtual LocalizedContentManager CreateTemporary()
		{
			return new LocalizedContentManager(base.ServiceProvider, base.RootDirectory, CurrentCulture);
		}
	}
}
