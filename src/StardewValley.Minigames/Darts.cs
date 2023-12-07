using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Minigames
{
	public class Darts : IMinigame
	{
		public enum GameState
		{
			Aiming,
			Charging,
			Firing,
			ShowScore,
			Scoring,
			GameOver
		}

		public GameState currentGameState;

		public float stateTimer;

		public float pixelScale = 4f;

		public bool gamePaused;

		public Vector2 upperLeft;

		private int screenWidth;

		private int screenHeight;

		private Texture2D texture;

		public Vector2 cursorPosition = new Vector2(0f, 0f);

		public Vector2 aimPosition = new Vector2(0f, 0f);

		public Vector2 dartBoardCenter = Vector2.Zero;

		protected bool canCancelShot = true;

		public float chargeTime;

		public float chargeDirection = 1f;

		public float hangTime;

		public int previousPoints;

		public int points;

		public float nextPointTransferTime;

		public static ICue chargeSound;

		public Vector2 throwStartPosition;

		public Vector2 dartPosition;

		public float dartTime = -1f;

		public string lastHitString = "";

		public int lastHitAmount;

		public bool shakeScore;

		public int startingDartCount = 20;

		public int dartCount = 20;

		public int throwsCount;

		public string alternateTextString = "";

		public string gameOverString = "";

		public bool lastHitWasDouble;

		public Vector2 aimOffset = Vector2.Zero;

		public ClickableTextureComponent upperRightCloseButton;

		public ClickableTextureComponent startDartThrowButton;

		protected float _dartThrowCancelTimer;

		protected bool _isTouchAiming;

		protected Vector2 _aimStartPosition;

		public bool overrideFreeMouseMovement()
		{
			return false;
		}

		public Darts(int dart_count = 20)
		{
			startingDartCount = (dartCount = dart_count);
			changeScreenSize();
			texture = Game1.content.Load<Texture2D>("Minigames\\Darts");
			points = 301;
			RepositionButtons();
			SetGameState(GameState.Aiming);
		}

		public virtual void SetGameState(GameState new_state)
		{
			if (currentGameState == GameState.Scoring)
			{
				previousPoints = points;
				shakeScore = false;
				alternateTextString = "";
			}
			if (currentGameState == GameState.Charging && chargeSound != null)
			{
				chargeSound.Stop(AudioStopOptions.Immediate);
				chargeSound = null;
			}
			currentGameState = new_state;
			if (currentGameState == GameState.Aiming)
			{
				startDartThrowButton.visible = true;
				dartTime = -1f;
				if (Game1.options.gamepadControls)
				{
					Game1.setMousePosition(Utility.Vector2ToPoint(TransformDraw(new Vector2(screenWidth / 2, screenHeight / 2))));
				}
			}
			else if (currentGameState == GameState.Charging)
			{
				if (chargeSound == null && Game1.soundBank != null)
				{
					chargeSound = Game1.soundBank.GetCue("SinWave");
					chargeSound.Play();
				}
				chargeTime = 1f;
				chargeDirection = -1f;
				canCancelShot = true;
			}
			else if (currentGameState == GameState.Firing)
			{
				throwStartPosition = dartBoardCenter + new Vector2(Utility.RandomFloat(-64f, 64f), 200f);
				Game1.playSound("FishHit");
				hangTime = 0.25f;
			}
			else if (currentGameState == GameState.ShowScore)
			{
				stateTimer = 1f;
			}
			else if (currentGameState == GameState.GameOver)
			{
				if (points == 0)
				{
					gameOverString = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11943");
					Game1.playSound("yoba");
				}
				else
				{
					gameOverString = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11946");
					Game1.playSound("slimedead");
				}
				stateTimer = 3f;
			}
		}

		public bool WasButtonHeld()
		{
			if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed)
			{
				return true;
			}
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.A))
			{
				return true;
			}
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.X))
			{
				return true;
			}
			if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton))
			{
				return true;
			}
			if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton))
			{
				return true;
			}
			return false;
		}

		public bool WasButtonPressed()
		{
			if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released && currentGameState != 0)
			{
				return true;
			}
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonUp(Buttons.A))
			{
				return true;
			}
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.X) && Game1.oldPadState.IsButtonUp(Buttons.X))
			{
				return true;
			}
			if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.actionButton))
			{
				return true;
			}
			if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.actionButton))
			{
				return true;
			}
			return false;
		}

		public bool tick(GameTime time)
		{
			if (_dartThrowCancelTimer > 0f)
			{
				_dartThrowCancelTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (stateTimer > 0f)
			{
				stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
				if (stateTimer <= 0f)
				{
					stateTimer = 0f;
					if (currentGameState == GameState.ShowScore)
					{
						if (lastHitAmount == 0)
						{
							if (dartCount <= 0)
							{
								SetGameState(GameState.Scoring);
							}
							else
							{
								SetGameState(GameState.Aiming);
							}
						}
						else
						{
							nextPointTransferTime = 0.5f;
							SetGameState(GameState.Scoring);
						}
					}
					else if (currentGameState == GameState.GameOver)
					{
						QuitGame();
						return true;
					}
				}
			}
			if (currentGameState == GameState.GameOver && WasButtonPressed())
			{
				QuitGame();
				return true;
			}
			if (_isTouchAiming)
			{
				aimOffset = new Vector2(-screenWidth / 2, -screenHeight / 2);
			}
			else
			{
				aimOffset = Vector2.Zero;
			}
			cursorPosition = (Utility.PointToVector2(Game1.getMousePosition()) + aimOffset - upperLeft) / GetPixelScale();
			if (currentGameState == GameState.Aiming)
			{
				chargeTime = 1f;
				aimPosition = cursorPosition;
				aimPosition.X += (float)Math.Sin(time.TotalGameTime.TotalSeconds * 0.75) * 32f;
				aimPosition.Y += (float)Math.Sin(time.TotalGameTime.TotalSeconds * 1.5) * 32f;
				if (WasButtonPressed() && IsAiming())
				{
					SetGameState(GameState.Charging);
				}
			}
			else if (currentGameState == GameState.Charging)
			{
				if (chargeSound != null)
				{
					chargeSound.SetVariable("Pitch", 2400f * (1f - chargeTime));
				}
				chargeTime += (float)time.ElapsedGameTime.TotalSeconds * chargeDirection;
				if (chargeDirection < 0f && chargeTime < 0f)
				{
					canCancelShot = false;
					chargeTime = 0f;
					chargeDirection = 1f;
				}
				else if (chargeDirection > 0f && chargeTime >= 1f)
				{
					chargeTime = 1f;
					chargeDirection = -1f;
				}
			}
			else if (currentGameState == GameState.Firing)
			{
				if (hangTime > 0f)
				{
					hangTime -= (float)time.ElapsedGameTime.TotalSeconds;
					if (hangTime <= 0f)
					{
						float num = Utility.RandomFloat(0f, (float)Math.PI * 2f);
						aimPosition += new Vector2((float)Math.Sin(num), (float)Math.Cos(num)) * Utility.RandomFloat(0f, GetRadiusFromCharge() * 32f);
						Game1.playSound("cast");
						dartTime = 0f;
						dartPosition = throwStartPosition;
					}
				}
				else if (dartTime >= 0f)
				{
					dartTime += (float)time.ElapsedGameTime.TotalSeconds / 0.75f;
					dartPosition.X = Utility.Lerp(throwStartPosition.X, aimPosition.X, dartTime);
					dartPosition.Y = Utility.Lerp(throwStartPosition.Y, aimPosition.Y, dartTime);
					if (dartTime >= 1f)
					{
						Game1.playSound("Cowboy_gunshot");
						lastHitAmount = GetPointsForAim();
						SetGameState(GameState.ShowScore);
					}
				}
			}
			else if (currentGameState == GameState.Scoring)
			{
				if (lastHitAmount > 0)
				{
					if (nextPointTransferTime > 0f)
					{
						nextPointTransferTime -= (float)time.ElapsedGameTime.TotalSeconds;
						if (nextPointTransferTime < 0f)
						{
							int num2 = points;
							shakeScore = true;
							int num3 = 1;
							if (lastHitAmount > 10 && points > 10)
							{
								num3 = 10;
							}
							points -= num3;
							lastHitAmount -= num3;
							Game1.playSound("moneyDial");
							nextPointTransferTime = 0.05f;
							if (points < 0)
							{
								alternateTextString = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11947");
								Game1.playSound("fishEscape");
								nextPointTransferTime = 1f;
								lastHitAmount = 0;
							}
						}
					}
				}
				else
				{
					if (nextPointTransferTime > 0f)
					{
						nextPointTransferTime -= (float)time.ElapsedGameTime.TotalSeconds;
					}
					if (nextPointTransferTime <= 0f)
					{
						nextPointTransferTime = 0f;
						if (points == 0)
						{
							SetGameState(GameState.GameOver);
						}
						else
						{
							if (points < 0)
							{
								points = previousPoints;
							}
							if (dartCount <= 0)
							{
								SetGameState(GameState.GameOver);
							}
							else
							{
								SetGameState(GameState.Aiming);
							}
						}
					}
				}
			}
			if (IsAiming() || currentGameState == GameState.Charging)
			{
				Game1.mouseCursorTransparency = 0f;
			}
			else
			{
				Game1.mouseCursorTransparency = 1f;
			}
			return false;
		}

		public virtual bool IsAiming()
		{
			if (currentGameState == GameState.Aiming && cursorPosition.X > 0f && cursorPosition.X < 320f && cursorPosition.Y > 0f && cursorPosition.Y < 320f)
			{
				return true;
			}
			return false;
		}

		public float GetRadiusFromCharge()
		{
			return (float)Math.Pow(chargeTime, 0.5);
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (upperRightCloseButton.containsPoint(x, y))
			{
				QuitGame();
			}
			if (startDartThrowButton.containsPoint(x, y))
			{
				_aimStartPosition = new Vector2(x, y);
				startDartThrowButton.visible = false;
				_isTouchAiming = true;
				_dartThrowCancelTimer = 0.5f;
				Game1.playSound("coin");
			}
			if (!Game1.options.gamepadControls && currentGameState == GameState.Charging)
			{
				dartCount--;
				throwsCount++;
				FireDart(chargeTime);
			}
		}

		public void releaseLeftClick(int x, int y)
		{
			if (_isTouchAiming)
			{
				_isTouchAiming = false;
				if (IsAiming() && _dartThrowCancelTimer <= 0f)
				{
					SetGameState(GameState.Charging);
					return;
				}
				Game1.playSound("dwop");
				startDartThrowButton.visible = true;
			}
		}

		public virtual int GetPointsForAim()
		{
			Vector2 vector = aimPosition;
			Vector2 vector2 = dartBoardCenter - vector;
			float num = vector2.Length();
			if (num < 5f)
			{
				Game1.playSound("parrot");
				lastHitWasDouble = true;
				lastHitString = Game1.content.LoadString("Strings\\UI:Darts_Bullseye");
				return 50;
			}
			if (num < 12f)
			{
				Game1.playSound("parrot");
				lastHitString = Game1.content.LoadString("Strings\\UI:Darts_Bull");
				return 25;
			}
			if (num > 88f)
			{
				Game1.playSound("fishEscape");
				lastHitString = Game1.content.LoadString("Strings\\UI:Darts_OffTheIsland");
				return 0;
			}
			float num2 = (float)(Math.Atan2(vector2.Y, vector2.X) * (180.0 / Math.PI));
			num2 -= 81f;
			if (num2 < 0f)
			{
				num2 += 360f;
			}
			int num3 = (int)(num2 / 18f);
			int[] array = new int[20]
			{
				20, 1, 18, 4, 13, 6, 10, 15, 2, 17,
				3, 19, 7, 16, 8, 11, 14, 9, 12, 5
			};
			int num4 = 0;
			if (num3 < array.Length)
			{
				num4 = array[num3];
			}
			if (num >= 46f && num < 55f)
			{
				Game1.playSound("parrot");
				lastHitString = num4 + "x3";
				return num4 * 3;
			}
			if (num >= 79f)
			{
				lastHitWasDouble = true;
				Game1.playSound("parrot");
				lastHitString = num4 + "x2";
				return num4 * 2;
			}
			lastHitString = num4.ToString() ?? "";
			return num4;
		}

		public virtual void FireDart(float radius)
		{
			SetGameState(GameState.Firing);
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void receiveKeyPress(Keys k)
		{
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.Back) || k.Equals(Keys.Escape))
			{
				QuitGame();
			}
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public void QuitGame()
		{
			unload();
			Game1.playSound("bigDeSelect");
			Game1.currentMinigame = null;
			if (currentGameState != GameState.GameOver)
			{
				return;
			}
			if (points == 0)
			{
				bool flag = IsPerfectVictory();
				if (flag)
				{
					Game1.multiplayer.globalChatInfoMessage("DartsWinPerfect", Game1.player.Name);
				}
				else
				{
					Game1.multiplayer.globalChatInfoMessage("DartsWin", Game1.player.Name, throwsCount.ToString() ?? "");
				}
				if (!(Game1.currentLocation is IslandSouthEastCave))
				{
					return;
				}
				string text = Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_Win");
				if (flag)
				{
					text = Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_Win_Perfect");
				}
				text += "#";
				int droppedLimitedNutCount = Game1.player.team.GetDroppedLimitedNutCount("Darts");
				if ((startingDartCount == 20 && droppedLimitedNutCount == 0) || (startingDartCount == 15 && droppedLimitedNutCount == 1) || (startingDartCount == 10 && droppedLimitedNutCount == 2))
				{
					text += Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_WinPrize");
					Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
					{
						Game1.player.team.RequestLimitedNutDrops("Darts", Game1.currentLocation, 1984, 512, 3);
					});
				}
				else
				{
					text += Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_WinNoPrize");
				}
				Game1.drawDialogueNoTyping(text);
			}
			else if (Game1.currentLocation is IslandSouthEastCave)
			{
				Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_Lose"));
			}
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, new RasterizerState());
			b.Draw(texture, TransformDraw(new Rectangle(0, 0, 320, 320)), new Rectangle(0, 0, 320, 320), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			if ((IsAiming() || currentGameState == GameState.Charging) && (_isTouchAiming || currentGameState != 0 || Game1.options.gamepadControls))
			{
				b.Draw(texture, TransformDraw(aimPosition), new Rectangle(0, 320, 64, 64), Color.White * 0.5f, 0f, new Vector2(32f, 32f), GetPixelScale() * GetRadiusFromCharge(), SpriteEffects.None, 0f);
			}
			if (dartTime >= 0f)
			{
				Rectangle value = new Rectangle(0, 384, 16, 32);
				if (dartTime > 0.65f)
				{
					value.X = 16;
				}
				if (dartTime > 0.9f)
				{
					value.X = 32;
				}
				float y = (float)Math.Sin((double)dartTime * Math.PI) * 200f;
				float rotation = (float)Math.Atan2(aimPosition.X - throwStartPosition.X, throwStartPosition.Y - aimPosition.Y);
				b.Draw(texture, TransformDraw(dartPosition - new Vector2(0f, y)), value, Color.White, rotation, new Vector2(8f, 16f), GetPixelScale(), SpriteEffects.None, 0.02f);
			}
			Vector2 vector = TransformDraw(new Vector2(160f, 16f));
			Vector2 vector2 = Vector2.Zero;
			if (shakeScore)
			{
				vector2 = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			if (alternateTextString != "")
			{
				SpriteText.drawStringWithScrollCenteredAt(b, alternateTextString, (int)(vector.X + vector2.X), (int)(vector.Y + vector2.Y), "", 1f, 2);
			}
			else if (points >= 0)
			{
				string s = Game1.content.LoadString("Strings\\UI:Darts_PointsToGo", points);
				if (points == 1)
				{
					s = Game1.content.LoadString("Strings\\UI:Darts_PointToGo", points);
				}
				SpriteText.drawStringWithScrollCenteredAt(b, s, (int)(vector.X + vector2.X), (int)(vector.Y + vector2.Y));
				if (currentGameState == GameState.ShowScore || currentGameState == GameState.Scoring)
				{
					if (shakeScore)
					{
						vector2 = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
					}
					vector.Y += 64f;
					string text = "";
					text = ((currentGameState != GameState.ShowScore) ? (" " + lastHitAmount + " ") : (" " + lastHitString + " "));
					SpriteText.drawStringWithScrollCenteredAt(b, text, (int)(vector.X + vector2.X), (int)(vector.Y + vector2.Y), "", 1f, 1, 2);
				}
			}
			for (int i = 0; i < dartCount; i++)
			{
				b.Draw(position: TransformDraw(new Vector2(7 + i * 10, 317f)), texture: texture, sourceRectangle: new Rectangle(64, 384, 16, 32), color: Color.White, rotation: 0f, origin: new Vector2(0f, 32f), scale: GetPixelScale(), effects: SpriteEffects.None, layerDepth: 0.02f);
			}
			if (gameOverString != "")
			{
				b.Draw(Game1.staminaRect, TransformDraw(new Rectangle(0, 0, screenWidth, screenHeight)), null, Color.Black * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, 0f);
				if (points == 0)
				{
					vector = TransformDraw(new Vector2(160f, 144f));
					SpriteText.drawStringWithScrollCenteredAt(b, gameOverString, (int)vector.X, (int)vector.Y);
					vector = TransformDraw(new Vector2(160f, 176f));
					if (IsPerfectVictory())
					{
						SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:Darts_WinTextPerfect", throwsCount), (int)(vector.X + vector2.X), (int)(vector.Y + vector2.Y), "", 1f, 1, 2);
					}
					else
					{
						SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:Darts_WinText", throwsCount), (int)(vector.X + vector2.X), (int)(vector.Y + vector2.Y), "", 1f, 1, 2);
					}
				}
				else
				{
					vector = TransformDraw(new Vector2(160f, 160f));
					SpriteText.drawStringWithScrollCenteredAt(b, gameOverString, (int)vector.X, (int)vector.Y);
				}
			}
			if (Game1.options.gamepadControls && !Game1.options.hardwareCursor)
			{
				b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, (Game1.options.snappyMenus && Game1.options.gamepadControls) ? 44 : 0, 16, 16), Color.White * Game1.mouseCursorTransparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
			if (startDartThrowButton.visible)
			{
				startDartThrowButton.draw(b);
				b.Draw(texture, Utility.PointToVector2(startDartThrowButton.bounds.Center), new Rectangle(0, 384, 16, 32), Color.White, 0f, new Vector2(8f, 16f), GetPixelScale(), SpriteEffects.None, 1f);
			}
			upperRightCloseButton.bounds.X = Game1.viewport.Width - 68 - Game1.xEdge;
			upperRightCloseButton.draw(b);
			b.End();
		}

		public void DrawNumberString(SpriteBatch b, string text, int x, int y)
		{
			int num = 14;
			for (int i = 0; i < text.Length; i++)
			{
				Rectangle value = new Rectangle(96, 320, 16, 32);
				if (text[i] >= '0' && text[i] <= '9')
				{
					int num2 = text[i] - 48;
					value.X += num2 * 16;
				}
				else
				{
					if (text[i] != 'x')
					{
						x += num;
						continue;
					}
					value.X = 256;
				}
				b.Draw(texture, TransformDraw(new Vector2(x, y)), value, Color.White, 0f, Vector2.Zero, GetPixelScale(), SpriteEffects.None, 0f);
				x += num;
			}
		}

		public float GetPixelScale()
		{
			return pixelScale;
		}

		public Rectangle TransformDraw(Rectangle dest)
		{
			dest.X = (int)Math.Round((float)dest.X * pixelScale) + (int)upperLeft.X;
			dest.Y = (int)Math.Round((float)dest.Y * pixelScale) + (int)upperLeft.Y;
			dest.Width = (int)((float)dest.Width * pixelScale);
			dest.Height = (int)((float)dest.Height * pixelScale);
			return dest;
		}

		public Vector2 TransformDraw(Vector2 dest)
		{
			dest.X = (int)Math.Round(dest.X * pixelScale) + (int)upperLeft.X;
			dest.Y = (int)Math.Round(dest.Y * pixelScale) + (int)upperLeft.Y;
			return dest;
		}

		public bool IsPerfectVictory()
		{
			if (points == 0)
			{
				return throwsCount <= 6;
			}
			return false;
		}

		public void changeScreenSize()
		{
			screenWidth = 320;
			screenHeight = 320;
			float num = 1f / Game1.options.zoomLevel;
			int width = Game1.game1.localMultiplayerWindow.Width;
			int height = Game1.game1.localMultiplayerWindow.Height;
			pixelScale = Math.Min(5f, Math.Min((float)width * num / (float)screenWidth, (float)height * num / (float)screenHeight));
			float num2 = 0.1f;
			pixelScale = (float)(int)(pixelScale / num2) * num2;
			upperLeft = new Vector2((float)(width / 2) * num, (float)(height / 2) * num);
			upperLeft.X -= (float)(screenWidth / 2) * pixelScale;
			upperLeft.Y -= (float)(screenHeight / 2) * pixelScale;
			dartBoardCenter = new Vector2(160f, 160f);
			RepositionButtons();
		}

		public void unload()
		{
			if (chargeSound != null)
			{
				chargeSound.Stop(AudioStopOptions.Immediate);
				chargeSound = null;
			}
			Game1.stopMusicTrack(Game1.MusicContext.MiniGame);
			Game1.player.faceDirection(0);
		}

		public bool forceQuit()
		{
			unload();
			return true;
		}

		public void leftClickHeld(int x, int y)
		{
			if ((new Vector2(x, y) - _aimStartPosition).Length() > 16f)
			{
				_dartThrowCancelTimer = 0f;
			}
		}

		public void receiveEventPoke(int data)
		{
			throw new NotImplementedException();
		}

		public string minigameId()
		{
			return "Darts";
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public void RepositionButtons()
		{
			upperRightCloseButton = new ClickableTextureComponent(new Rectangle(Game1.viewport.Width - 68 - Game1.xEdge, 0, 68 + Game1.xEdge, 80), Game1.mobileSpriteSheet, new Rectangle(62, 0, 17, 17), 4f, drawShadow: true);
			int num = 37;
			float num2 = 2f;
			Vector2 dest = new Vector2((float)screenWidth - (float)num * num2 - 8f, (float)screenHeight - (float)num * num2 - 8f);
			dest = TransformDraw(dest);
			num2 = (int)(num2 * pixelScale);
			startDartThrowButton = new ClickableTextureComponent(new Rectangle((int)dest.X, (int)dest.Y, (int)((float)num * num2), (int)((float)num * num2)), Game1.mobileSpriteSheet, new Rectangle(0, 116, num, num), num2, drawShadow: true);
		}

		public float GetForcedScaleFactor()
		{
			return Game1.NativeZoomLevel;
		}
	}
}
