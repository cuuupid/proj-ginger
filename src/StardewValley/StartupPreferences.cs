using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using StardewValley.GameData;

namespace StardewValley
{
	public class StartupPreferences
	{
		public const int windowed_borderless = 0;

		public const int windowed = 1;

		public const int fullscreen = 2;

		private static readonly string _filename = "startup_preferences";

		public static XmlSerializer serializer = new XmlSerializer(typeof(StartupPreferences));

		public bool startMuted;

		public bool levelTenFishing;

		public bool levelTenMining;

		public bool levelTenForaging;

		public bool levelTenCombat;

		public bool skipWindowPreparation;

		public bool sawAdvancedCharacterCreationIndicator;

		public int timesPlayed;

		public int windowMode;

		public int displayIndex = -1;

		public Options.GamepadModes gamepadMode;

		public int playerLimit = -1;

		public int fullscreenResolutionX;

		public int fullscreenResolutionY;

		public string lastEnteredIP = "";

		public float safeRegionSize = -1f;

		public string languageCode;

		public Options clientOptions = new Options();

		public bool androidDoneStrorageMigration;

		[XmlIgnore]
		public bool isLoaded;

		private bool _isBusy;

		private bool _pendingApplyLanguage;

		private Task _task;

		private bool _resetViewportAfterLoadSettings;

		[XmlIgnore]
		public bool IsBusy
		{
			get
			{
				lock (this)
				{
					if (!_isBusy)
					{
						return false;
					}
					if (_task == null)
					{
						throw new Exception("StartupPreferences.IsBusy; was busy but task is null?");
					}
					if (_task.IsFaulted)
					{
						Exception baseException = _task.Exception.GetBaseException();
						Console.WriteLine("StartupPreferences._task failed with an exception");
						Console.WriteLine(baseException);
						throw baseException;
					}
					if (_task.IsCompleted)
					{
						_task = null;
						_isBusy = false;
						if (_pendingApplyLanguage)
						{
							_SetLanguageFromCode(languageCode);
						}
					}
					return _isBusy;
				}
			}
		}

		private void Init()
		{
			isLoaded = false;
			ensureFolderStructureExists();
		}

		public void OnLanguageChange(LocalizedContentManager.LanguageCode code)
		{
			string text = code.ToString();
			if (code == LocalizedContentManager.LanguageCode.mod && LocalizedContentManager.CurrentModLanguage != null)
			{
				text = LocalizedContentManager.CurrentModLanguage.ID;
			}
			if (isLoaded && languageCode != text)
			{
				savePreferences(async: false, update_language_from_ingame_language: true);
			}
		}

		private void ensureFolderStructureExists()
		{
		}

		public void savePreferences(bool async, bool update_language_from_ingame_language = false)
		{
			lock (this)
			{
				if (_isBusy)
				{
					Console.WriteLine("savePreferences(); ignoring because already busy");
					return;
				}
				_isBusy = true;
				if (update_language_from_ingame_language)
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.mod)
					{
						languageCode = LocalizedContentManager.CurrentModLanguage.ID;
					}
					else
					{
						languageCode = LocalizedContentManager.CurrentLanguageCode.ToString();
					}
				}
				Console.WriteLine("savePreferences(); async={0}, languageCode={1}", async, languageCode);
				Task task = new Task(delegate
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					_savePreferences();
				});
				task.Start();
				_task = task;
				if (!async)
				{
					try
					{
						task.Wait();
					}
					catch (Exception ex)
					{
						Exception baseException = ex.GetBaseException();
						Console.WriteLine("StartupPreferences._task failed with an exception");
						Console.WriteLine(baseException.GetType());
						Console.WriteLine(baseException.Message);
						Console.WriteLine(baseException.StackTrace);
						throw ex;
					}
					_task = null;
					_isBusy = false;
				}
			}
		}

		private void _savePreferences()
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _filename);
			MemoryStream memoryStream = new MemoryStream();
			writeSettings(memoryStream);
			byte[] array = memoryStream.ToArray();
			using FileStream fileStream = File.Open(path, FileMode.Create);
			fileStream.Write(array, 0, array.Length);
		}

		private long writeSettings(Stream stream)
		{
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			xmlWriterSettings.CloseOutput = true;
			using XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings);
			xmlWriter.WriteStartDocument();
			serializer.Serialize(xmlWriter, this);
			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();
			return stream.Length;
		}

		public void loadPreferences(bool async, bool applyLanguage = true, bool resetViewportAfterLoad = false)
		{
			_resetViewportAfterLoadSettings = resetViewportAfterLoad;
			lock (this)
			{
				if (_isBusy)
				{
					Console.WriteLine("loadPreferences(); ignoring because already busy");
					return;
				}
				_isBusy = true;
				_pendingApplyLanguage = applyLanguage;
				Console.WriteLine("loadPreferences(); begin - languageCode={0}", languageCode);
				Init();
				Task task = new Task(delegate
				{
					Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
					_loadPreferences();
				});
				task.Start();
				_task = task;
				if (!async)
				{
					try
					{
						_task.Wait();
					}
					catch (Exception)
					{
						Exception baseException = _task.Exception.GetBaseException();
						Console.WriteLine("StartupPreferences._task failed with an exception");
						Console.WriteLine(baseException.GetType());
						Console.WriteLine(baseException.Message);
						Console.WriteLine(baseException.StackTrace);
						throw baseException;
					}
					_task = null;
					_isBusy = false;
					if (applyLanguage)
					{
						_SetLanguageFromCode(languageCode);
					}
				}
			}
		}

		protected virtual void _SetLanguageFromCode(string language_code_string)
		{
			List<ModLanguage> list = Game1.content.Load<List<ModLanguage>>("Data\\AdditionalLanguages");
			bool flag = false;
			if (list != null)
			{
				foreach (ModLanguage item in list)
				{
					if (item.ID == language_code_string)
					{
						LocalizedContentManager.SetModLanguage(item);
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				LocalizedContentManager.LanguageCode result = LocalizedContentManager.LanguageCode.en;
				if (Enum.TryParse<LocalizedContentManager.LanguageCode>(language_code_string, out result) && result != LocalizedContentManager.LanguageCode.mod)
				{
					LocalizedContentManager.CurrentLanguageCode = result;
				}
				else
				{
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.GetDefaultLanguageCode();
				}
			}
		}

		private void _loadPreferences()
		{
			clientOptions = Game1.options;
			isLoaded = true;
			languageCode = LocalizedContentManager.CurrentLanguageCode.ToString();
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _filename);
			if (!File.Exists(path))
			{
				return;
			}
			using FileStream stream = File.OpenRead(path);
			readSettings(stream);
		}

		private void readSettings(Stream stream)
		{
			StartupPreferences p = (StartupPreferences)serializer.Deserialize(stream);
			readSettings(p);
		}

		private void readSettings(StartupPreferences p)
		{
			startMuted = p.startMuted;
			timesPlayed = p.timesPlayed + 1;
			levelTenCombat = p.levelTenCombat;
			levelTenFishing = p.levelTenFishing;
			levelTenForaging = p.levelTenForaging;
			levelTenMining = p.levelTenMining;
			skipWindowPreparation = p.skipWindowPreparation;
			windowMode = p.windowMode;
			displayIndex = p.displayIndex;
			playerLimit = p.playerLimit;
			gamepadMode = p.gamepadMode;
			fullscreenResolutionX = p.fullscreenResolutionX;
			fullscreenResolutionY = p.fullscreenResolutionY;
			lastEnteredIP = p.lastEnteredIP;
			languageCode = p.languageCode;
			safeRegionSize = p.safeRegionSize;
			clientOptions = p.clientOptions;
			androidDoneStrorageMigration = p.androidDoneStrorageMigration;
			clientOptions.autoSave = true;
			if (Game1.graphics.PreferredBackBufferWidth <= 1200)
			{
				clientOptions.xEdge = (Game1.options.xEdge = (Game1.xEdge = 0));
			}
			clientOptions.gamepadMode = (gamepadMode = Options.GamepadModes.Auto);
			if (clientOptions.toolbarSlotSize > 0)
			{
				Game1.maxItemSlotSize = clientOptions.toolbarSlotSize;
				if (Game1.toolbar != null)
				{
					Game1.toolbar.itemSlotSize = clientOptions.toolbarSlotSize;
				}
			}
			if (_resetViewportAfterLoadSettings)
			{
				ResetViewport();
			}
		}

		public void ResetViewport()
		{
			Game1.viewport.X = 0;
			Game1.viewport.Y = 0;
			Game1.viewport.Width = (int)((float)Game1.graphics.PreferredBackBufferWidth / Game1.options.zoomLevel);
			Game1.viewport.Height = (int)((float)Game1.graphics.PreferredBackBufferHeight / Game1.options.zoomLevel);
		}
	}
}
