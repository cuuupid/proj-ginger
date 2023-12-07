using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	public class GameRunner : Game
	{
		public static GameRunner instance;

		public List<Game1> gameInstances = new List<Game1>();

		public List<Game1> gameInstancesToRemove = new List<Game1>();

		public Game1 gamePtr;

		public bool shouldLoadContent;

		protected bool _initialized;

		protected bool _windowSizeChanged;

		public List<int> startButtonState = new List<int>();

		public List<KeyValuePair<Game1, IEnumerator<int>>> activeNewDayProcesses = new List<KeyValuePair<Game1, IEnumerator<int>>>();

		public int nextInstanceId;

		public static int MaxTextureSize = 4096;

		public GameRunner()
		{
			Program.sdk.EarlyInitialize();
			if (!Program.releaseBuild)
			{
				base.InactiveSleepTime = new TimeSpan(0L);
			}
			Game1.graphics = new GraphicsDeviceManager(this);
			Game1.graphics.PreparingDeviceSettings += delegate(object sender, PreparingDeviceSettingsEventArgs args)
			{
				args.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
			};
			Game1.graphics.IsFullScreen = true;
			base.Content.RootDirectory = "Content";
			SpriteBatch.TextureTuckAmount = 0.001f;
			LocalMultiplayer.Initialize();
			MaxTextureSize = 2147483647;
			Game1.InitializeRunner();
			SubscribeClientSizeChange();
			base.Exiting += delegate(object sender, EventArgs args)
			{
				ExecuteForInstances(delegate(Game1 instance)
				{
					instance.exitEvent(sender, args);
				});
				Process.GetCurrentProcess().Kill();
			};
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			LocalizedContentManager.OnLanguageChange += delegate
			{
				ExecuteForInstances(delegate(Game1 instance)
				{
					instance.TranslateFields();
				});
			};
			DebugTools.GameConstructed(this);
		}

		protected override void OnActivated(object sender, EventArgs args)
		{
			ExecuteForInstances(delegate(Game1 instance)
			{
				instance.Instance_OnActivated(sender, args);
			});
		}

		public void SubscribeClientSizeChange()
		{
			base.Window.ClientSizeChanged += OnWindowSizeChange;
		}

		public void OnWindowSizeChange(object sender, EventArgs args)
		{
			base.Window.ClientSizeChanged -= OnWindowSizeChange;
			_windowSizeChanged = true;
		}

		protected override bool BeginDraw()
		{
			return base.BeginDraw();
		}

		protected override void BeginRun()
		{
			base.BeginRun();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		protected override void Draw(GameTime gameTime_)
		{
			lock (Game1.RenderLock)
			{
				InnerDraw(gameTime_);
			}
		}

		private void InnerDraw(GameTime gameTime_)
		{
			if (_windowSizeChanged)
			{
				ExecuteForInstances(delegate(Game1 instance)
				{
					instance.Window_ClientSizeChanged(null, null);
				});
				_windowSizeChanged = false;
				SubscribeClientSizeChange();
			}
			GameTime time = gameTime_;
			DebugTools.BeforeGameDraw(this, ref time);
			foreach (Game1 gameInstance in gameInstances)
			{
				LoadInstance(gameInstance);
				Viewport viewport = base.GraphicsDevice.Viewport;
				Game1.graphics.GraphicsDevice.Viewport = new Viewport(0, 0, Math.Min(gameInstance.localMultiplayerWindow.Width, Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth), Math.Min(gameInstance.localMultiplayerWindow.Height, Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight));
				gameInstance.Instance_Draw(time);
				base.GraphicsDevice.Viewport = viewport;
				SaveInstance(gameInstance);
			}
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				base.GraphicsDevice.Clear(Game1.bgColor);
				foreach (Game1 gameInstance2 in gameInstances)
				{
					Game1.isRenderingScreenBuffer = true;
					gameInstance2.DrawSplitScreenWindow();
					Game1.isRenderingScreenBuffer = false;
				}
			}
			if (Game1.shouldDrawSafeAreaBounds)
			{
				SpriteBatch spriteBatch = Game1.spriteBatch;
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
				Rectangle safeAreaBounds = Game1.safeAreaBounds;
				spriteBatch.Draw(Game1.staminaRect, new Rectangle(safeAreaBounds.X, safeAreaBounds.Y, safeAreaBounds.Width, 2), Color.White);
				spriteBatch.Draw(Game1.staminaRect, new Rectangle(safeAreaBounds.X, safeAreaBounds.Y + safeAreaBounds.Height - 2, safeAreaBounds.Width, 2), Color.White);
				spriteBatch.Draw(Game1.staminaRect, new Rectangle(safeAreaBounds.X, safeAreaBounds.Y, 2, safeAreaBounds.Height), Color.White);
				spriteBatch.Draw(Game1.staminaRect, new Rectangle(safeAreaBounds.X + safeAreaBounds.Width - 2, safeAreaBounds.Y, 2, safeAreaBounds.Height), Color.White);
				spriteBatch.End();
			}
			base.Draw(time);
		}

		public int GetNewInstanceID()
		{
			return nextInstanceId++;
		}

		public virtual Game1 GetFirstInstanceAtThisLocation(GameLocation location, Func<Game1, bool> additional_check = null)
		{
			if (location == null)
			{
				return null;
			}
			Game1 game = Game1.game1;
			if (game != null)
			{
				SaveInstance(game);
			}
			foreach (Game1 gameInstance in gameInstances)
			{
				if (gameInstance.instanceGameLocation == null || !gameInstance.instanceGameLocation.Equals(location))
				{
					continue;
				}
				if (additional_check != null)
				{
					LoadInstance(gameInstance);
					bool flag = additional_check(gameInstance);
					SaveInstance(gameInstance);
					if (!flag)
					{
						continue;
					}
				}
				if (game != null)
				{
					LoadInstance(game);
				}
				else
				{
					Game1.game1 = null;
				}
				return gameInstance;
			}
			if (game != null)
			{
				LoadInstance(game);
			}
			else
			{
				Game1.game1 = null;
			}
			return null;
		}

		protected override void EndDraw()
		{
			base.EndDraw();
		}

		protected override void EndRun()
		{
			base.EndRun();
		}

		protected override void Initialize()
		{
			DebugTools.BeforeGameInitialize(this);
			InitializeMainInstance();
			base.IsFixedTimeStep = true;
			base.Initialize();
			Game1.graphics.SynchronizeWithVerticalRetrace = true;
			Program.sdk.Initialize();
		}

		public bool WasWindowSizeChanged()
		{
			return _windowSizeChanged;
		}

		public int GetMaxSimultaneousPlayers()
		{
			return 4;
		}

		public void InitializeMainInstance()
		{
			gameInstances = new List<Game1>();
			AddGameInstance(PlayerIndex.One);
		}

		public virtual void ExecuteForInstances(Action<Game1> action)
		{
			Game1 game = Game1.game1;
			if (game != null)
			{
				SaveInstance(game);
			}
			foreach (Game1 gameInstance in gameInstances)
			{
				LoadInstance(gameInstance);
				action(gameInstance);
				SaveInstance(gameInstance);
			}
			if (game != null)
			{
				LoadInstance(game);
			}
			else
			{
				Game1.game1 = null;
			}
		}

		public virtual void RemoveGameInstance(Game1 instance)
		{
			if (gameInstances.Contains(instance) && !gameInstancesToRemove.Contains(instance))
			{
				gameInstancesToRemove.Add(instance);
			}
		}

		public virtual void AddGameInstance(PlayerIndex player_index)
		{
			Game1 game = Game1.game1;
			if (game != null)
			{
				SaveInstance(game, force: true);
			}
			if (gameInstances.Count > 0)
			{
				Game1 game2 = gameInstances[0];
				LoadInstance(game2);
				Game1.StartLocalMultiplayerIfNecessary();
				SaveInstance(game2, force: true);
			}
			Game1 game3 = null;
			game3 = ((gameInstances.Count != 0) ? CreateGameInstance(player_index, gameInstances.Count) : CreateGameInstance());
			gameInstances.Add(game3);
			if (gamePtr == null)
			{
				gamePtr = game3;
			}
			_ = gameInstances.Count;
			_ = 0;
			Game1.game1 = game3;
			game3.Instance_Initialize();
			if (shouldLoadContent)
			{
				game3.Instance_LoadContent();
			}
			SaveInstance(game3);
			if (game != null)
			{
				LoadInstance(game);
			}
			else
			{
				Game1.game1 = null;
			}
			_windowSizeChanged = true;
		}

		public virtual Game1 CreateGameInstance(PlayerIndex player_index = PlayerIndex.One, int index = 0)
		{
			return new Game1(player_index, index);
		}

		public Game1 GetGamePtr()
		{
			return gamePtr;
		}

		protected override void LoadContent()
		{
			LoadInstance(gamePtr);
			gamePtr.Instance_LoadContent();
			SaveInstance(gamePtr);
			DebugTools.GameLoadContent(this);
			foreach (Game1 gameInstance in gameInstances)
			{
				if (gameInstance != gamePtr)
				{
					LoadInstance(gameInstance);
					gameInstance.Instance_LoadContent();
					SaveInstance(gameInstance);
				}
			}
			shouldLoadContent = true;
			base.LoadContent();
		}

		protected override void UnloadContent()
		{
			gamePtr.Instance_UnloadContent();
			base.UnloadContent();
		}

		protected override void Update(GameTime gameTime_)
		{
			GameTime gameTime = gameTime_;
			for (int i = 0; i < activeNewDayProcesses.Count; i++)
			{
				KeyValuePair<Game1, IEnumerator<int>> keyValuePair = activeNewDayProcesses[i];
				Game1 key = activeNewDayProcesses[i].Key;
				LoadInstance(key);
				if (!keyValuePair.Value.MoveNext())
				{
					key.isLocalMultiplayerNewDayActive = false;
					activeNewDayProcesses.RemoveAt(i);
					i--;
					Utility.CollectGarbage();
				}
				SaveInstance(key);
			}
			while (startButtonState.Count < 4)
			{
				startButtonState.Add(-1);
			}
			for (PlayerIndex playerIndex = PlayerIndex.One; playerIndex <= PlayerIndex.Four; playerIndex++)
			{
				if (GamePad.GetState(playerIndex).IsButtonDown(Buttons.Start))
				{
					if (startButtonState[(int)playerIndex] >= 0)
					{
						startButtonState[(int)playerIndex]++;
					}
				}
				else if (startButtonState[(int)playerIndex] != 0)
				{
					startButtonState[(int)playerIndex] = 0;
				}
			}
			for (int j = 0; j < gameInstances.Count; j++)
			{
				Game1 game = gameInstances[j];
				LoadInstance(game);
				if (j == 0)
				{
					DebugTools.BeforeGameUpdate(this, ref gameTime);
					PlayerIndex playerIndex2 = PlayerIndex.Two;
					if (game.instanceOptions.gamepadMode == Options.GamepadModes.ForceOff)
					{
						playerIndex2 = PlayerIndex.One;
					}
					for (PlayerIndex playerIndex3 = playerIndex2; playerIndex3 <= PlayerIndex.Four; playerIndex3++)
					{
						bool flag = false;
						foreach (Game1 gameInstance in gameInstances)
						{
							if (gameInstance.instancePlayerOneIndex == playerIndex3)
							{
								flag = true;
								break;
							}
						}
						if (!flag && game.IsLocalCoopJoinable() && IsStartDown(playerIndex3) && game.ShowLocalCoopJoinMenu())
						{
							InvalidateStartPress(playerIndex3);
						}
					}
				}
				else
				{
					Game1.options.gamepadMode = Options.GamepadModes.ForceOn;
				}
				game.Instance_Update(gameTime);
				SaveInstance(game);
			}
			if (gameInstancesToRemove.Count > 0)
			{
				foreach (Game1 item in gameInstancesToRemove)
				{
					LoadInstance(item);
					item.exitEvent(null, null);
					gameInstances.Remove(item);
					Game1.game1 = null;
				}
				for (int k = 0; k < gameInstances.Count; k++)
				{
					Game1 game2 = gameInstances[k];
					game2.instanceIndex = k;
				}
				if (gameInstances.Count == 1)
				{
					Game1 game3 = gameInstances[0];
					LoadInstance(game3, force: true);
					game3.staticVarHolder = null;
					Game1.EndLocalMultiplayer();
				}
				bool flag2 = false;
				if (gameInstances.Count > 0)
				{
					foreach (Game1 gameInstance2 in gameInstances)
					{
						if (gameInstance2.instancePlayerOneIndex == PlayerIndex.One)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						gameInstances[0].instancePlayerOneIndex = PlayerIndex.One;
					}
				}
				gameInstancesToRemove.Clear();
				_windowSizeChanged = true;
			}
			base.Update(gameTime);
		}

		public virtual void InvalidateStartPress(PlayerIndex index)
		{
			if (index >= PlayerIndex.One && (int)index < startButtonState.Count)
			{
				startButtonState[(int)index] = -1;
			}
		}

		public virtual bool IsStartDown(PlayerIndex index)
		{
			if (index >= PlayerIndex.One && (int)index < startButtonState.Count)
			{
				return startButtonState[(int)index] == 1;
			}
			return false;
		}

		private static void SetInstanceDefaults(InstanceGame instance)
		{
		}

		public static void SaveInstance(InstanceGame instance, bool force = false)
		{
		}

		public static void LoadInstance(InstanceGame instance, bool force = false)
		{
			Game1.game1 = instance as Game1;
			if (force || LocalMultiplayer.IsLocalMultiplayer())
			{
				_ = instance.staticVarHolder;
			}
		}
	}
}
