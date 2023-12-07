using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewValley.Minigames
{
	public class CraneGame : IMinigame
	{
		public enum GameButtons
		{
			Action,
			Tool,
			Confirm,
			Cancel,
			Run,
			Up,
			Left,
			Down,
			Right,
			MAX
		}

		public class GameLogic : CraneGameObject
		{
			[XmlType("CraneGame.GameStates")]
			public enum GameStates
			{
				Setup,
				Idle,
				MoveClawRight,
				WaitForMoveDown,
				MoveClawDown,
				ClawDescend,
				ClawAscend,
				ClawReturn,
				ClawRelease,
				ClawReset,
				EndGame
			}

			public List<Item> collectedItems;

			public const int CLAW_HEIGHT = 50;

			protected Claw _claw;

			public int maxLives = 3;

			public int lives = 3;

			public Vector2 _startPosition = new Vector2(24f, 56f);

			public Vector2 _dropPosition = new Vector2(32f, 56f);

			public Rectangle playArea = new Rectangle(16, 48, 272, 64);

			public Rectangle prizeChute = new Rectangle(16, 48, 32, 32);

			protected GameStates _currentState;

			protected int _stateTimer;

			public CraneGameObject moveRightIndicator;

			public CraneGameObject moveDownIndicator;

			public CraneGameObject creditsDisplay;

			public CraneGameObject timeDisplay1;

			public CraneGameObject timeDisplay2;

			public CraneGameObject sunShockedFace;

			public int currentTimer;

			public CraneGameObject joystick;

			public int[] conveyerBeltTiles = new int[68]
			{
				0, 0, 0, 0, 7, 6, 6, 9, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 8, 0, 0, 2, 0, 0, 0, 7, 6,
				6, 6, 6, 9, 0, 0, 0, 0, 8, 0,
				0, 2, 0, 0, 0, 8, 0, 0, 0, 0,
				2, 0, 0, 0, 0, 1, 4, 4, 3, 0,
				0, 0, 1, 4, 4, 4, 4, 3
			};

			public int[] prizeMap = new int[68]
			{
				0, 0, 0, 0, 1, 0, 0, 1, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
				0, 1, 0, 2, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 1, 0, 0, 1, 0,
				0, 0, 0, 1, 0, 2, 0, 3
			};

			public GameLogic(CraneGame game)
				: base(game)
			{
				if (Game1.musicWaveBankState == Game1.MusicWaveBankState.Created)
				{
					_game.music = Game1.soundBank.GetCue("crane_game");
					_game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
					_game.music.Play();
				}
				_claw = new Claw(_game);
				_claw.position = _startPosition;
				_claw.zPosition = 50f;
				collectedItems = new List<Item>();
				SetState(GameStates.Setup);
				new Bush(_game, 55, 2, 3, 31, 111);
				new Bush(_game, 45, 2, 2, 112, 84);
				new Bush(_game, 45, 2, 2, 63, 63);
				new Bush(_game, 48, 1, 2, 56, 80);
				new Bush(_game, 48, 1, 2, 72, 80);
				new Bush(_game, 48, 1, 2, 56, 96);
				new Bush(_game, 48, 1, 2, 72, 96);
				new Bush(_game, 48, 1, 2, 56, 112);
				new Bush(_game, 48, 1, 2, 72, 112);
				new Bush(_game, 45, 2, 2, 159, 63);
				new Bush(_game, 48, 1, 2, 152, 80);
				new Bush(_game, 48, 1, 2, 168, 80);
				new Bush(_game, 48, 1, 2, 152, 96);
				new Bush(_game, 48, 1, 2, 168, 96);
				new Bush(_game, 48, 1, 2, 152, 112);
				new Bush(_game, 48, 1, 2, 168, 112);
				sunShockedFace = new CraneGameObject(_game);
				sunShockedFace.SetSpriteFromIndex(9);
				sunShockedFace.position = new Vector2(96f, 0f);
				sunShockedFace.spriteAnchor = Vector2.Zero;
				CraneGameObject craneGameObject = new CraneGameObject(_game);
				craneGameObject.position.X = 16f;
				craneGameObject.position.Y = 87f;
				craneGameObject.SetSpriteFromIndex(3);
				craneGameObject.spriteRect.Width = 32;
				craneGameObject.spriteAnchor = new Vector2(0f, 15f);
				joystick = new CraneGameObject(_game);
				joystick.position.X = 151f;
				joystick.position.Y = 134f;
				joystick.SetSpriteFromIndex(28);
				joystick.spriteRect.Width = 32;
				joystick.spriteRect.Height = 48;
				joystick.spriteAnchor = new Vector2(15f, 47f);
				lives = maxLives;
				moveRightIndicator = new CraneGameObject(_game);
				moveRightIndicator.position.X = 21f;
				moveRightIndicator.position.Y = 126f;
				moveRightIndicator.SetSpriteFromIndex(26);
				moveRightIndicator.spriteAnchor = Vector2.Zero;
				moveRightIndicator.visible = false;
				moveDownIndicator = new CraneGameObject(_game);
				moveDownIndicator.position.X = 49f;
				moveDownIndicator.position.Y = 126f;
				moveDownIndicator.SetSpriteFromIndex(27);
				moveDownIndicator.spriteAnchor = Vector2.Zero;
				moveDownIndicator.visible = false;
				creditsDisplay = new CraneGameObject(_game);
				creditsDisplay.SetSpriteFromIndex(70);
				creditsDisplay.position = new Vector2(234f, 125f);
				creditsDisplay.spriteAnchor = Vector2.Zero;
				timeDisplay1 = new CraneGameObject(_game);
				timeDisplay1.SetSpriteFromIndex(70);
				timeDisplay1.position = new Vector2(274f, 125f);
				timeDisplay1.spriteAnchor = Vector2.Zero;
				timeDisplay2 = new CraneGameObject(_game);
				timeDisplay2.SetSpriteFromIndex(70);
				timeDisplay2.position = new Vector2(285f, 125f);
				timeDisplay2.spriteAnchor = Vector2.Zero;
				int num = 17;
				for (int i = 0; i < conveyerBeltTiles.Length; i++)
				{
					if (conveyerBeltTiles[i] != 0)
					{
						int x = i % num + 1;
						int y = i / num + 3;
						if (conveyerBeltTiles[i] == 8)
						{
							new ConveyerBelt(_game, x, y, 0);
						}
						else if (conveyerBeltTiles[i] == 4)
						{
							new ConveyerBelt(_game, x, y, 3);
						}
						else if (conveyerBeltTiles[i] == 6)
						{
							new ConveyerBelt(_game, x, y, 1);
						}
						else if (conveyerBeltTiles[i] == 2)
						{
							new ConveyerBelt(_game, x, y, 2);
						}
						else if (conveyerBeltTiles[i] == 7)
						{
							new ConveyerBelt(_game, x, y, 1).SetSpriteFromCorner(240, 272);
						}
						else if (conveyerBeltTiles[i] == 9)
						{
							new ConveyerBelt(_game, x, y, 2).SetSpriteFromCorner(240, 240);
						}
						else if (conveyerBeltTiles[i] == 1)
						{
							new ConveyerBelt(_game, x, y, 0).SetSpriteFromCorner(240, 224);
						}
						else if (conveyerBeltTiles[i] == 3)
						{
							new ConveyerBelt(_game, x, y, 3).SetSpriteFromCorner(240, 256);
						}
					}
				}
				Dictionary<int, List<Item>> dictionary = new Dictionary<int, List<Item>> { [1] = new List<Item>
				{
					new Furniture(1760, Vector2.Zero),
					new Furniture(1761, Vector2.Zero),
					new Furniture(1762, Vector2.Zero),
					new Furniture(1763, Vector2.Zero),
					new Furniture(1764, Vector2.Zero),
					new Furniture(1365, Vector2.Zero)
				} };
				List<Item> list = new List<Item>();
				MovieData movieForDate = MovieTheater.GetMovieForDate(Game1.Date);
				if (movieForDate != null)
				{
					string iD = movieForDate.ID;
					if (iD != null)
					{
						switch (iD.Length)
						{
						case 14:
							switch (iD[1])
							{
							case 'p':
								if (!(iD == "spring_movie_0"))
								{
									if (iD == "spring_movie_1")
									{
										list.Add(new Furniture(1958, Vector2.Zero));
									}
								}
								else
								{
									list.Add(new Furniture(1952, Vector2.Zero));
								}
								break;
							case 'u':
								if (!(iD == "summer_movie_0"))
								{
									if (iD == "summer_movie_1")
									{
										list.Add(new Furniture(1955, Vector2.Zero));
									}
								}
								else
								{
									list.Add(new Furniture(1954, Vector2.Zero));
								}
								break;
							case 'i':
								if (!(iD == "winter_movie_1"))
								{
									if (iD == "winter_movie_0")
									{
										list.Add(new Furniture(1957, Vector2.Zero));
									}
								}
								else
								{
									list.Add(new Furniture(1956, Vector2.Zero));
								}
								break;
							}
							break;
						case 12:
							switch (iD[11])
							{
							case '0':
								if (iD == "fall_movie_0")
								{
									list.Add(new Furniture(1953, Vector2.Zero));
								}
								break;
							case '1':
								if (iD == "fall_movie_1")
								{
									list.Add(new Furniture(1959, Vector2.Zero));
								}
								break;
							}
							break;
						}
					}
				}
				list.Add(new Furniture(1669, Vector2.Zero));
				if (Game1.currentSeason == "spring")
				{
					list.Add(new Furniture(1960, Vector2.Zero));
				}
				else if (Game1.currentSeason == "winter")
				{
					list.Add(new Furniture(1961, Vector2.Zero));
				}
				else if (Game1.currentSeason == "summer")
				{
					list.Add(new Furniture(1294, Vector2.Zero));
				}
				else if (Game1.currentSeason == "fall")
				{
					list.Add(new Furniture(1918, Vector2.Zero));
				}
				list.Add(new Object(Vector2.Zero, 2));
				dictionary[2] = list;
				list = new List<Item>();
				if (Game1.currentSeason == "spring")
				{
					list.Add(new Object(Vector2.Zero, 107));
					list.Add(new Object(Vector2.Zero, 36));
					list.Add(new Object(Vector2.Zero, 48));
					list.Add(new Object(Vector2.Zero, 184));
					list.Add(new Object(Vector2.Zero, 188));
					list.Add(new Object(Vector2.Zero, 192));
					list.Add(new Object(Vector2.Zero, 204));
				}
				else if (Game1.currentSeason == "winter")
				{
					list.Add(new Furniture(1440, Vector2.Zero));
					list.Add(new Object(Vector2.Zero, 44));
					list.Add(new Object(Vector2.Zero, 40));
					list.Add(new Object(Vector2.Zero, 41));
					list.Add(new Object(Vector2.Zero, 43));
					list.Add(new Object(Vector2.Zero, 42));
				}
				else if (Game1.currentSeason == "summer")
				{
					if (movieForDate != null && movieForDate.ID == "summer_movie_1")
					{
						list.Add(new Furniture(1371, Vector2.Zero));
						list.Add(new Furniture(1373, Vector2.Zero));
					}
					else
					{
						list.Add(new Furniture(985, Vector2.Zero));
						list.Add(new Furniture(984, Vector2.Zero));
					}
				}
				else if (Game1.currentSeason == "fall")
				{
					list.Add(new Furniture(1917, Vector2.Zero));
					list.Add(new Furniture(1307, Vector2.Zero));
					list.Add(new Object(Vector2.Zero, 47));
					list.Add(new Furniture(1471, Vector2.Zero));
					list.Add(new Furniture(1375, Vector2.Zero));
				}
				dictionary[3] = list;
				for (int j = 0; j < prizeMap.Length; j++)
				{
					if (prizeMap[j] == 0)
					{
						continue;
					}
					int num2 = j % num + 1;
					int num3 = j / num + 3;
					Item item = null;
					int num4 = j;
					while (num4 > 0 && item == null)
					{
						if (prizeMap[j] == 1)
						{
							item = Utility.GetRandom(dictionary[1]);
						}
						else if (prizeMap[j] == 2)
						{
							item = Utility.GetRandom(dictionary[2]);
						}
						else if (prizeMap[j] == 3)
						{
							item = Utility.GetRandom(dictionary[3]);
						}
						num4--;
					}
					Prize prize = new Prize(_game, item)
					{
						position = 
						{
							X = num2 * 16 + 8,
							Y = num3 * 16 + 8
						}
					};
				}
				if (Game1.random.NextDouble() < 0.1)
				{
					Item item2 = null;
					Vector2 vector = new Vector2(0f, 4f);
					switch (Game1.random.Next(4))
					{
					case 0:
						item2 = new Object(107, 1);
						break;
					case 1:
						item2 = new Object(749, 5);
						break;
					case 2:
						item2 = new Object(688, 5);
						break;
					case 3:
						item2 = new Object(288, 5);
						break;
					}
					Prize prize2 = new Prize(_game, item2)
					{
						position = 
						{
							X = vector.X * 16f + 30f,
							Y = vector.Y * 16f + 32f
						}
					};
				}
				else if (Game1.random.NextDouble() < 0.2)
				{
					Prize prize3 = new Prize(_game, new Object(809, 1))
					{
						position = 
						{
							X = 160f,
							Y = 58f
						}
					};
				}
				if (Game1.random.NextDouble() < 0.25)
				{
					Prize prize4 = new Prize(_game, new Furniture(986, Vector2.Zero))
					{
						position = new Vector2(263f, 56f),
						zPosition = 0f
					};
					prize4 = new Prize(_game, new Furniture(986, Vector2.Zero))
					{
						position = new Vector2(215f, 56f),
						zPosition = 0f
					};
				}
				else
				{
					Prize prize5 = new Prize(_game, new Furniture(989, Vector2.Zero))
					{
						position = new Vector2(263f, 56f),
						zPosition = 0f
					};
					prize5 = new Prize(_game, new Furniture(989, Vector2.Zero))
					{
						position = new Vector2(215f, 56f),
						zPosition = 0f
					};
				}
			}

			public GameStates GetCurrentState()
			{
				return _currentState;
			}

			public override void Update(GameTime time)
			{
				float to = 0f;
				foreach (Shadow item in _game.GetObjectsOfType<Shadow>())
				{
					if (prizeChute.Contains(new Point((int)item.position.X, (int)item.position.Y)))
					{
						item.visible = false;
					}
					else
					{
						item.visible = true;
					}
				}
				int num = currentTimer / 60;
				if (_currentState == GameStates.Setup)
				{
					creditsDisplay.SetSpriteFromIndex(70);
				}
				else
				{
					creditsDisplay.SetSpriteFromIndex(70 + lives);
				}
				timeDisplay1.SetSpriteFromIndex(70 + num / 10);
				timeDisplay2.SetSpriteFromIndex(70 + num % 10);
				if (currentTimer < 0)
				{
					timeDisplay1.SetSpriteFromIndex(80);
					timeDisplay2.SetSpriteFromIndex(81);
				}
				if (_currentState == GameStates.Setup)
				{
					if (_game.music != null && !_game.music.IsPlaying)
					{
						_game.music.Play();
					}
					_claw.openAngle = 40f;
					bool flag = false;
					foreach (Prize item2 in _game.GetObjectsOfType<Prize>())
					{
						if (!item2.CanBeGrabbed())
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						if (_stateTimer >= 10)
						{
							SetState(GameStates.Idle);
						}
					}
					else
					{
						_stateTimer = 0;
					}
				}
				else if (_currentState == GameStates.Idle)
				{
					if (_game.music != null && !_game.music.IsPlaying)
					{
						_game.music.Play();
					}
					if (_game.fastMusic != null && _game.fastMusic.IsPlaying)
					{
						_game.fastMusic.Stop(AudioStopOptions.Immediate);
						_game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
					}
					currentTimer = 900;
					moveRightIndicator.visible = Game1.ticks / 20 % 2 == 0;
					if (_game.IsButtonPressed(GameButtons.Tool) || _game.IsButtonPressed(GameButtons.Action) || _game.IsButtonPressed(GameButtons.Right))
					{
						Game1.playSound("bigSelect");
						SetState(GameStates.MoveClawRight);
					}
				}
				else if (_currentState == GameStates.MoveClawRight)
				{
					to = 15f;
					if (_stateTimer < 15)
					{
						if (!_game.IsButtonDown(GameButtons.Tool) && !_game.IsButtonDown(GameButtons.Action) && !_game.IsButtonDown(GameButtons.Right))
						{
							Game1.playSound("bigDeSelect");
							SetState(GameStates.Idle);
							return;
						}
					}
					else
					{
						if (_game.craneSound == null || !_game.craneSound.IsPlaying)
						{
							_game.craneSound = Game1.soundBank.GetCue("crane");
							_game.craneSound.Play();
							_game.craneSound.SetVariable("Pitch", 1200);
						}
						currentTimer--;
						if (currentTimer <= 0)
						{
							SetState(GameStates.ClawDescend);
							currentTimer = -1;
							if (_game.craneSound != null && !_game.craneSound.IsStopped)
							{
								_game.craneSound.Stop(AudioStopOptions.Immediate);
							}
						}
						moveRightIndicator.visible = true;
						if (_stateTimer > 10)
						{
							if (_stateTimer == 11)
							{
								_claw.ApplyDrawEffect(new ShakeEffect(1f, 1f));
								_claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
								_claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 50));
							}
							if (!_game.IsButtonDown(GameButtons.Tool) && !_game.IsButtonDown(GameButtons.Right) && !_game.IsButtonDown(GameButtons.Action))
							{
								Game1.playSound("bigDeSelect");
								_claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
								_claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 100));
								SetState(GameStates.WaitForMoveDown);
								moveRightIndicator.visible = false;
								if (_game.craneSound != null && !_game.craneSound.IsStopped)
								{
									_game.craneSound.Stop(AudioStopOptions.Immediate);
								}
							}
							else
							{
								_claw.Move(0.5f, 0f);
								if (_claw.GetBounds().Right >= playArea.Right)
								{
									_claw.Move(-0.5f, 0f);
								}
							}
						}
					}
				}
				else if (_currentState == GameStates.WaitForMoveDown)
				{
					currentTimer--;
					if (currentTimer <= 0)
					{
						SetState(GameStates.ClawDescend);
						currentTimer = -1;
					}
					moveDownIndicator.visible = Game1.ticks / 20 % 2 == 0;
					if (_game.IsButtonPressed(GameButtons.Tool) || _game.IsButtonPressed(GameButtons.Down) || _game.IsButtonPressed(GameButtons.Action))
					{
						Game1.playSound("bigSelect");
						SetState(GameStates.MoveClawDown);
					}
				}
				else if (_currentState == GameStates.MoveClawDown)
				{
					if (_game.craneSound == null || !_game.craneSound.IsPlaying)
					{
						_game.craneSound = Game1.soundBank.GetCue("crane");
						_game.craneSound.SetVariable("Pitch", 1200);
						_game.craneSound.Play();
					}
					currentTimer--;
					if (currentTimer <= 0)
					{
						SetState(GameStates.ClawDescend);
						currentTimer = -1;
						if (_game.craneSound != null && !_game.craneSound.IsStopped)
						{
							_game.craneSound.Stop(AudioStopOptions.Immediate);
						}
					}
					to = -5f;
					moveDownIndicator.visible = true;
					if (_stateTimer > 10)
					{
						if (_stateTimer == 11)
						{
							_claw.ApplyDrawEffect(new ShakeEffect(1f, 1f));
							_claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
							_claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 50));
						}
						if (!_game.IsButtonDown(GameButtons.Tool) && !_game.IsButtonDown(GameButtons.Down) && !_game.IsButtonDown(GameButtons.Action))
						{
							Game1.playSound("bigDeSelect");
							_claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
							_claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 100));
							moveDownIndicator.visible = false;
							SetState(GameStates.ClawDescend);
							if (_game.craneSound != null && !_game.craneSound.IsStopped)
							{
								_game.craneSound.Stop(AudioStopOptions.Immediate);
							}
						}
						else
						{
							_claw.Move(0f, 0.5f);
							if (_claw.GetBounds().Bottom >= playArea.Bottom)
							{
								_claw.Move(0f, -0.5f);
							}
						}
					}
				}
				else if (_currentState == GameStates.ClawDescend)
				{
					if (_claw.openAngle < 40f)
					{
						_claw.openAngle += 1.5f;
						_stateTimer = 0;
					}
					else if (_stateTimer > 30)
					{
						if (_game.craneSound != null && _game.craneSound.IsPlaying)
						{
							_game.craneSound.SetVariable("Pitch", 2000);
						}
						else
						{
							_game.craneSound = Game1.soundBank.GetCue("crane");
							_game.craneSound.SetVariable("Pitch", 2000);
							_game.craneSound.Play();
						}
						if (_claw.zPosition > 0f)
						{
							_claw.zPosition -= 0.5f;
							if (_claw.zPosition <= 0f)
							{
								_claw.zPosition = 0f;
								SetState(GameStates.ClawAscend);
								if (_game.craneSound != null && !_game.craneSound.IsStopped)
								{
									_game.craneSound.Stop(AudioStopOptions.Immediate);
								}
							}
						}
					}
				}
				else if (_currentState == GameStates.ClawAscend)
				{
					if (_claw.openAngle > 0f && _claw.GetGrabbedPrize() == null)
					{
						_claw.openAngle -= 1f;
						if (_claw.openAngle == 15f)
						{
							_claw.GrabObject();
							if (_claw.GetGrabbedPrize() != null)
							{
								Game1.playSound("FishHit");
								sunShockedFace.ApplyDrawEffect(new ShakeEffect(1f, 1f, 5));
								_game.freezeFrames = 60;
								if (_game.music != null && _game.music.IsPlaying)
								{
									_game.music.Stop(AudioStopOptions.Immediate);
									_game.music = Game1.soundBank.GetCue("crane_game");
								}
							}
						}
						else if (_claw.openAngle == 0f && _claw.GetGrabbedPrize() == null)
						{
							if (lives == 1)
							{
								if (_game.music != null)
								{
									_game.music.Stop(AudioStopOptions.Immediate);
								}
								Game1.playSound("fishEscape");
							}
							else
							{
								Game1.playSound("stoneStep");
							}
						}
						_stateTimer = 0;
					}
					else
					{
						if (_claw.GetGrabbedPrize() != null)
						{
							if (_game.fastMusic != null && !_game.fastMusic.IsPlaying)
							{
								_game.fastMusic.Play();
							}
						}
						else if (_game.fastMusic != null && _game.fastMusic.IsPlaying)
						{
							_game.fastMusic.Stop(AudioStopOptions.AsAuthored);
							_game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
						}
						if (_claw.zPosition < 50f)
						{
							_claw.zPosition += 0.5f;
							if (_claw.zPosition >= 50f)
							{
								_claw.zPosition = 50f;
								SetState(GameStates.ClawReturn);
								if (_claw.GetGrabbedPrize() == null && lives == 1)
								{
									SetState(GameStates.EndGame);
								}
							}
						}
						_claw.CheckDropPrize();
					}
				}
				else if (_currentState == GameStates.ClawReturn)
				{
					if (_claw.GetGrabbedPrize() != null)
					{
						if (_game.fastMusic != null && !_game.fastMusic.IsPlaying)
						{
							_game.fastMusic.Play();
						}
					}
					else if (_game.fastMusic != null && _game.fastMusic.IsPlaying)
					{
						_game.fastMusic.Stop(AudioStopOptions.AsAuthored);
						_game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
					}
					if (_stateTimer > 10)
					{
						if (_claw.position.Equals(_dropPosition))
						{
							SetState(GameStates.ClawRelease);
						}
						else
						{
							float delta = 0.5f;
							if (_claw.GetGrabbedPrize() == null)
							{
								delta = 0.75f;
							}
							if (_claw.position.X != _dropPosition.X)
							{
								_claw.position.X = Utility.MoveTowards(_claw.position.X, _dropPosition.X, delta);
							}
							if (_claw.position.X != _dropPosition.Y)
							{
								_claw.position.Y = Utility.MoveTowards(_claw.position.Y, _dropPosition.Y, delta);
							}
						}
					}
					_claw.CheckDropPrize();
				}
				else if (_currentState == GameStates.ClawRelease)
				{
					bool flag2 = _claw.GetGrabbedPrize() != null;
					if (_stateTimer > 10)
					{
						_claw.ReleaseGrabbedObject();
						if (_claw.openAngle < 40f)
						{
							_claw.openAngle++;
						}
						else
						{
							SetState(GameStates.ClawReset);
							if (!flag2)
							{
								Game1.playSound("button1");
								_claw.ApplyDrawEffect(new ShakeEffect(1f, 1f));
							}
						}
					}
				}
				else if (_currentState == GameStates.ClawReset)
				{
					if (_stateTimer > 50)
					{
						if (_claw.position.Equals(_startPosition))
						{
							lives--;
							if (lives <= 0)
							{
								SetState(GameStates.EndGame);
							}
							else
							{
								SetState(GameStates.Idle);
							}
						}
						else
						{
							float delta2 = 0.5f;
							if (_claw.position.X != _startPosition.X)
							{
								_claw.position.X = Utility.MoveTowards(_claw.position.X, _startPosition.X, delta2);
							}
							if (_claw.position.X != _startPosition.Y)
							{
								_claw.position.Y = Utility.MoveTowards(_claw.position.Y, _startPosition.Y, delta2);
							}
						}
					}
				}
				else if (_currentState == GameStates.EndGame)
				{
					if (_game.music != null && _game.music.IsPlaying)
					{
						_game.music.Stop(AudioStopOptions.Immediate);
					}
					if (_game.fastMusic != null && _game.fastMusic.IsPlaying)
					{
						_game.fastMusic.Stop(AudioStopOptions.Immediate);
					}
					bool flag3 = false;
					foreach (Prize item3 in _game.GetObjectsOfType<Prize>())
					{
						if (!item3.CanBeGrabbed())
						{
							flag3 = true;
							break;
						}
					}
					if (!flag3 && _stateTimer >= 20)
					{
						if (collectedItems.Count > 0)
						{
							List<Item> list = new List<Item>();
							foreach (Item collectedItem in collectedItems)
							{
								list.Add(collectedItem.getOne());
							}
							Game1.activeClickableMenu = new ItemGrabMenu(list, reverseGrab: false, showReceivingMenu: true, null, null, "Rewards", null, snapToBottom: false, canBeExitedWithKey: false, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, _game);
						}
						_game.Quit();
					}
				}
				sunShockedFace.visible = _claw.GetGrabbedPrize() != null;
				joystick.rotation = Utility.MoveTowards(joystick.rotation, to, 2f);
				_stateTimer++;
			}

			public override void Draw(SpriteBatch b, float layer_depth)
			{
			}

			public void SetState(GameStates new_state)
			{
				_currentState = new_state;
				_stateTimer = 0;
			}
		}

		public class Trampoline : CraneGameObject
		{
			public Trampoline(CraneGame game, int x, int y)
				: base(game)
			{
				SetSpriteFromIndex(30);
				spriteRect.Width = 32;
				spriteRect.Height = 32;
				spriteAnchor.X = 15f;
				spriteAnchor.Y = 15f;
				position.X = x;
				position.Y = y;
			}
		}

		public class Shadow : CraneGameObject
		{
			public CraneGameObject _target;

			public Shadow(CraneGame game, CraneGameObject target)
				: base(game)
			{
				SetSpriteFromIndex(2);
				layerDepth = 900f;
				_target = target;
			}

			public override void Update(GameTime time)
			{
				if (_target != null)
				{
					position = _target.position;
				}
				if (_target is Prize && (_target as Prize).grabbed)
				{
					visible = false;
				}
				if (_target.IsDestroyed())
				{
					Destroy();
					return;
				}
				color.A = (byte)(Math.Min(1f, _target.zPosition / 50f) * 255f);
				scale = Utility.Lerp(1f, 0.5f, Math.Min(_target.zPosition / 100f, 1f)) * new Vector2(1f, 1f);
			}
		}

		public class Claw : CraneGameObject
		{
			protected CraneGameObject _leftArm;

			protected CraneGameObject _rightArm;

			protected Prize _grabbedPrize;

			protected Vector2 _prizePositionOffset;

			protected int _nextDropCheckTimer;

			protected int _dropChances;

			protected int _grabTime;

			public float openAngle
			{
				get
				{
					return _leftArm.rotation;
				}
				set
				{
					_leftArm.rotation = value;
				}
			}

			public Claw(CraneGame game)
				: base(game)
			{
				SetSpriteFromIndex();
				spriteAnchor = new Vector2(8f, 24f);
				_leftArm = new CraneGameObject(game);
				_leftArm.SetSpriteFromIndex(1);
				_leftArm.spriteAnchor = new Vector2(16f, 0f);
				_rightArm = new CraneGameObject(game);
				_rightArm.SetSpriteFromIndex(1);
				_rightArm.flipX = true;
				_rightArm.spriteAnchor = new Vector2(0f, 0f);
				new Shadow(_game, this);
			}

			public void CheckDropPrize()
			{
				if (_grabbedPrize == null)
				{
					return;
				}
				_nextDropCheckTimer--;
				if (_nextDropCheckTimer > 0)
				{
					return;
				}
				float num = _prizePositionOffset.Length() * 0.1f;
				num += zPosition * 0.001f;
				if (_grabbedPrize.isLargeItem)
				{
					num += 0.1f;
				}
				double num2 = Game1.random.NextDouble();
				if (num2 < (double)num)
				{
					_dropChances--;
					if (_dropChances <= 0)
					{
						Game1.playSound("fishEscape");
						ReleaseGrabbedObject();
					}
					else
					{
						Game1.playSound("bob");
						_grabbedPrize.ApplyDrawEffect(new ShakeEffect(2f, 2f, 50));
						_grabbedPrize.rotation += (float)Game1.random.NextDouble() * 10f;
					}
				}
				else if (num2 < (double)num)
				{
					Game1.playSound("dwop");
					_grabbedPrize.ApplyDrawEffect(new ShakeEffect(1f, 1f, 50));
				}
				_nextDropCheckTimer = Game1.random.Next(50, 100);
			}

			public void ApplyDrawEffectToArms(DrawEffect new_effect)
			{
				_leftArm.ApplyDrawEffect(new_effect);
				_rightArm.ApplyDrawEffect(new_effect);
			}

			public void ReleaseGrabbedObject()
			{
				if (_grabbedPrize != null)
				{
					_grabbedPrize.grabbed = false;
					_grabbedPrize.OnDrop();
					_grabbedPrize = null;
				}
			}

			public void GrabObject()
			{
				Prize prize = null;
				float num = 0f;
				List<Prize> objectsAtPoint = _game.GetObjectsAtPoint<Prize>(position);
				foreach (Prize item in objectsAtPoint)
				{
					if (!item.IsDestroyed() && item.CanBeGrabbed())
					{
						float num2 = (position - item.position).LengthSquared();
						if (prize == null || num2 < num)
						{
							num = num2;
							prize = item;
						}
					}
				}
				if (prize != null)
				{
					_grabbedPrize = prize;
					_grabbedPrize.grabbed = true;
					_prizePositionOffset = _grabbedPrize.position - position;
					_nextDropCheckTimer = Game1.random.Next(50, 100);
					_dropChances = 3;
					Game1.playSound("pickUpItem");
					_grabTime = 0;
					_grabbedPrize.ApplyDrawEffect(new StretchEffect(0.95f, 1.1f));
					_grabbedPrize.ApplyDrawEffect(new ShakeEffect(1f, 1f, 20));
				}
			}

			public override void Move(float x, float y)
			{
				base.Move(x, y);
			}

			public Prize GetGrabbedPrize()
			{
				return _grabbedPrize;
			}

			public override void Update(GameTime time)
			{
				_leftArm.position = position + new Vector2(0f, -16f);
				_rightArm.position = position + new Vector2(0f, -16f);
				_rightArm.rotation = 0f - _leftArm.rotation;
				_leftArm.layerDepth = (_rightArm.layerDepth = GetRendererLayerDepth() + 0.01f);
				_leftArm.zPosition = (_rightArm.zPosition = zPosition);
				if (_grabbedPrize != null)
				{
					_grabbedPrize.position = position + _prizePositionOffset * Utility.Lerp(1f, 0.25f, Math.Min(1f, (float)_grabTime / 200f));
					_grabbedPrize.zPosition = zPosition + _grabbedPrize.GetRestingZPosition();
				}
				_grabTime++;
			}

			public override void Destroy()
			{
				_leftArm.Destroy();
				_rightArm.Destroy();
				base.Destroy();
			}
		}

		public class ConveyerBelt : CraneGameObject
		{
			protected int _direction;

			protected Vector2 _spriteStartPosition;

			protected int _spriteOffset;

			public int GetDirection()
			{
				return _direction;
			}

			public ConveyerBelt(CraneGame game, int x, int y, int direction)
				: base(game)
			{
				position.X = x * 16;
				position.Y = y * 16;
				_direction = direction;
				spriteAnchor = Vector2.Zero;
				layerDepth = 1000f;
				if (_direction == 0)
				{
					SetSpriteFromIndex(5);
				}
				else if (_direction == 2)
				{
					SetSpriteFromIndex(10);
				}
				else if (_direction == 3)
				{
					SetSpriteFromIndex(15);
				}
				else if (_direction == 1)
				{
					SetSpriteFromIndex(20);
				}
				_spriteStartPosition = new Vector2(spriteRect.X, spriteRect.Y);
			}

			public void SetSpriteFromCorner(int x, int y)
			{
				spriteRect.X = x;
				spriteRect.Y = y;
				_spriteStartPosition = new Vector2(spriteRect.X, spriteRect.Y);
			}

			public override void Update(GameTime time)
			{
				int num = 4;
				int num2 = 4;
				spriteRect.X = (int)_spriteStartPosition.X + _spriteOffset / num * 16;
				_spriteOffset++;
				if (_spriteOffset >= (num2 - 1) * num)
				{
					_spriteOffset = 0;
				}
			}
		}

		public class Bush : CraneGameObject
		{
			public Bush(CraneGame game, int tile_index, int tile_width, int tile_height, int x, int y)
				: base(game)
			{
				SetSpriteFromIndex(tile_index);
				spriteRect.Width = tile_width * 16;
				spriteRect.Height = tile_height * 16;
				spriteAnchor.X = (float)spriteRect.Width / 2f;
				spriteAnchor.Y = spriteRect.Height;
				if (tile_height > 16)
				{
					spriteAnchor.Y -= 8f;
				}
				else
				{
					spriteAnchor.Y -= 4f;
				}
				position.X = x;
				position.Y = y;
			}

			public override void Update(GameTime time)
			{
				rotation = (float)Math.Sin(time.TotalGameTime.TotalMilliseconds * 0.0024999999441206455 + (double)position.Y + (double)(position.X * 2f)) * 2f;
			}
		}

		public class Prize : CraneGameObject
		{
			protected Vector2 _conveyerBeltMove;

			public bool grabbed;

			public float gravity;

			protected Vector2 _velocity = Vector2.Zero;

			protected Item _item;

			protected float _restingZPosition;

			protected float _angularSpeed;

			protected bool _isBeingCollected;

			public bool isLargeItem;

			public float GetRestingZPosition()
			{
				return _restingZPosition;
			}

			public Prize(CraneGame game, Item item)
				: base(game)
			{
				SetSpriteFromIndex(3);
				spriteAnchor = new Vector2(8f, 12f);
				_item = item;
				_UpdateItemSprite();
				new Shadow(_game, this);
			}

			public void OnDrop()
			{
				if (!isLargeItem)
				{
					_angularSpeed = Utility.Lerp(-5f, 5f, (float)Game1.random.NextDouble());
				}
				else
				{
					rotation = 0f;
				}
			}

			public void _UpdateItemSprite()
			{
				if (_item is Furniture)
				{
					texture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\furniture");
					spriteRect = (_item as Furniture).defaultSourceRect.Value;
				}
				else if (_item is Object)
				{
					Object @object = _item as Object;
					if ((bool)@object.bigCraftable)
					{
						texture = Game1.bigCraftableSpriteSheet;
						spriteRect = Object.getSourceRectForBigCraftable(@object.ParentSheetIndex);
					}
					else
					{
						texture = Game1.objectSpriteSheet;
						spriteRect = GameLocation.getSourceRectForObject(@object.ParentSheetIndex);
					}
				}
				width = spriteRect.Width;
				height = spriteRect.Height;
				if (width > 16 || height > 16)
				{
					isLargeItem = true;
				}
				else
				{
					isLargeItem = false;
				}
				if (height <= 16)
				{
					spriteAnchor = new Vector2(width / 2, (float)height - 4f);
				}
				else
				{
					spriteAnchor = new Vector2(width / 2, (float)height - 8f);
				}
				_restingZPosition = 0f;
			}

			public bool CanBeGrabbed()
			{
				if (IsDestroyed())
				{
					return false;
				}
				if (_isBeingCollected)
				{
					return false;
				}
				if (zPosition != _restingZPosition)
				{
					return false;
				}
				return true;
			}

			public override void Update(GameTime time)
			{
				if (_isBeingCollected)
				{
					Vector4 vector = color.ToVector4();
					vector.X = Utility.MoveTowards(vector.X, 0f, 0.05f);
					vector.Y = Utility.MoveTowards(vector.Y, 0f, 0.05f);
					vector.Z = Utility.MoveTowards(vector.Z, 0f, 0.05f);
					vector.W = Utility.MoveTowards(vector.W, 0f, 0.05f);
					color = new Color(vector);
					scale.X = Utility.MoveTowards(scale.X, 0.5f, 0.05f);
					scale.Y = Utility.MoveTowards(scale.Y, 0.5f, 0.05f);
					if (vector.W == 0f)
					{
						Game1.playSound("Ship");
						Destroy();
					}
					position.Y += 0.5f;
				}
				else
				{
					if (grabbed)
					{
						return;
					}
					if (_velocity.X != 0f || _velocity.Y != 0f)
					{
						position.X += _velocity.X;
						if (!_game.GetObjectsOfType<GameLogic>()[0].playArea.Contains(new Point((int)position.X, (int)position.Y)))
						{
							position.X -= _velocity.X;
							_velocity.X *= -1f;
						}
						position.Y += _velocity.Y;
						if (!_game.GetObjectsOfType<GameLogic>()[0].playArea.Contains(new Point((int)position.X, (int)position.Y)))
						{
							position.Y -= _velocity.Y;
							_velocity.Y *= -1f;
						}
					}
					if (zPosition < _restingZPosition)
					{
						zPosition = _restingZPosition;
					}
					if (zPosition > _restingZPosition || _velocity != Vector2.Zero || gravity != 0f)
					{
						if (!isLargeItem)
						{
							rotation += _angularSpeed;
						}
						_conveyerBeltMove = Vector2.Zero;
						if (zPosition > _restingZPosition)
						{
							gravity += 0.1f;
						}
						zPosition -= gravity;
						if (!(zPosition < _restingZPosition))
						{
							return;
						}
						zPosition = _restingZPosition;
						if (!(gravity >= 0f))
						{
							return;
						}
						if (!isLargeItem)
						{
							_angularSpeed = Utility.Lerp(-10f, 10f, (float)Game1.random.NextDouble());
						}
						gravity = (0f - gravity) * 0.6f;
						if (_game.GetObjectsOfType<GameLogic>()[0].prizeChute.Contains(new Point((int)position.X, (int)position.Y)))
						{
							if (_game.GetObjectsOfType<GameLogic>()[0].GetCurrentState() != 0)
							{
								Game1.playSound("reward");
								_isBeingCollected = true;
								_game.GetObjectsOfType<GameLogic>()[0].collectedItems.Add(_item);
							}
							else
							{
								gravity = -2.5f;
								Vector2 vector2 = new Vector2(_game.GetObjectsOfType<GameLogic>()[0].playArea.Center.X, _game.GetObjectsOfType<GameLogic>()[0].playArea.Center.Y);
								Vector2 vector3 = vector2 - new Vector2(position.X, position.Y);
								vector3.Normalize();
								_velocity = vector3 * Utility.Lerp(1f, 2f, (float)Game1.random.NextDouble());
							}
							return;
						}
						if (_game.GetOverlaps<Trampoline>(this, 1).Count > 0)
						{
							Trampoline trampoline = _game.GetOverlaps<Trampoline>(this, 1)[0];
							Game1.playSound("axchop");
							trampoline.ApplyDrawEffect(new StretchEffect(0.75f, 0.75f, 5));
							trampoline.ApplyDrawEffect(new ShakeEffect(2f, 2f));
							ApplyDrawEffect(new ShakeEffect(2f, 2f));
							gravity = -2.5f;
							Vector2 vector4 = new Vector2(_game.GetObjectsOfType<GameLogic>()[0].playArea.Center.X, _game.GetObjectsOfType<GameLogic>()[0].playArea.Center.Y);
							Vector2 vector5 = vector4 - new Vector2(position.X, position.Y);
							vector5.Normalize();
							_velocity = vector5 * Utility.Lerp(0.5f, 1f, (float)Game1.random.NextDouble());
							return;
						}
						if (Math.Abs(gravity) < 1.5f)
						{
							rotation = 0f;
							_velocity = Vector2.Zero;
							gravity = 0f;
							return;
						}
						bool flag = false;
						foreach (Prize overlap in _game.GetOverlaps<Prize>(this))
						{
							if (overlap.gravity == 0f && overlap.CanBeGrabbed())
							{
								Vector2 vector6 = position - overlap.position;
								vector6.Normalize();
								_velocity = vector6 * Utility.Lerp(0.25f, 1f, (float)Game1.random.NextDouble());
								if (!overlap.isLargeItem || isLargeItem)
								{
									overlap._velocity = -vector6 * Utility.Lerp(0.75f, 1.5f, (float)Game1.random.NextDouble());
									overlap.gravity = gravity * 0.75f;
									overlap.ApplyDrawEffect(new ShakeEffect(2f, 2f, 20));
								}
								flag = true;
							}
						}
						ApplyDrawEffect(new ShakeEffect(2f, 2f, 20));
						if (!flag)
						{
							float num = Utility.Lerp(0f, (float)Math.PI * 2f, (float)Game1.random.NextDouble());
							_velocity = new Vector2((float)Math.Sin(num), (float)Math.Cos(num)) * Utility.Lerp(0.5f, 1f, (float)Game1.random.NextDouble());
						}
					}
					else if (_conveyerBeltMove.X == 0f && _conveyerBeltMove.Y == 0f)
					{
						List<ConveyerBelt> objectsAtPoint = _game.GetObjectsAtPoint<ConveyerBelt>(position, 1);
						if (objectsAtPoint.Count > 0)
						{
							switch (objectsAtPoint[0].GetDirection())
							{
							case 0:
								_conveyerBeltMove = new Vector2(0f, -16f);
								break;
							case 2:
								_conveyerBeltMove = new Vector2(0f, 16f);
								break;
							case 3:
								_conveyerBeltMove = new Vector2(-16f, 0f);
								break;
							case 1:
								_conveyerBeltMove = new Vector2(16f, 0f);
								break;
							}
						}
					}
					else
					{
						float num2 = 0.3f;
						if (_conveyerBeltMove.X != 0f)
						{
							Move(num2 * (float)Math.Sign(_conveyerBeltMove.X), 0f);
							_conveyerBeltMove.X = Utility.MoveTowards(_conveyerBeltMove.X, 0f, num2);
						}
						if (_conveyerBeltMove.Y != 0f)
						{
							Move(0f, num2 * (float)Math.Sign(_conveyerBeltMove.Y));
							_conveyerBeltMove.Y = Utility.MoveTowards(_conveyerBeltMove.Y, 0f, num2);
						}
					}
				}
			}
		}

		public class CraneGameObject
		{
			protected CraneGame _game;

			public Vector2 position = Vector2.Zero;

			public float rotation;

			public Vector2 scale = new Vector2(1f, 1f);

			public bool flipX;

			public bool flipY;

			public Rectangle spriteRect;

			public Texture2D texture;

			public Vector2 spriteAnchor;

			public Color color = Color.White;

			public float layerDepth = -1f;

			public int width = 16;

			public int height = 16;

			public float zPosition;

			public bool visible = true;

			public List<DrawEffect> drawEffects;

			protected bool _destroyed;

			public CraneGameObject(CraneGame game)
			{
				_game = game;
				texture = _game.spriteSheet;
				spriteRect = new Rectangle(0, 0, 16, 16);
				spriteAnchor = new Vector2(8f, 8f);
				drawEffects = new List<DrawEffect>();
				_game.RegisterGameObject(this);
			}

			public void SetSpriteFromIndex(int index = 0)
			{
				spriteRect.X = 304 + index % 5 * 16;
				spriteRect.Y = index / 5 * 16;
			}

			public bool IsDestroyed()
			{
				return _destroyed;
			}

			public virtual void Destroy()
			{
				_destroyed = true;
				_game.UnregisterGameObject(this);
			}

			public virtual void Move(float x, float y)
			{
				position.X += x;
				position.Y += y;
			}

			public Rectangle GetBounds()
			{
				return new Rectangle((int)(position.X - spriteAnchor.X), (int)(position.Y - spriteAnchor.Y), width, height);
			}

			public virtual void Update(GameTime time)
			{
			}

			public float GetRendererLayerDepth()
			{
				float num = layerDepth;
				if (num < 0f)
				{
					num = (float)_game.gameHeight - position.Y;
				}
				return num;
			}

			public void ApplyDrawEffect(DrawEffect new_effect)
			{
				drawEffects.Add(new_effect);
			}

			public virtual void Draw(SpriteBatch b, float layer_depth)
			{
				if (!visible)
				{
					return;
				}
				SpriteEffects spriteEffects = SpriteEffects.None;
				if (flipX)
				{
					spriteEffects |= SpriteEffects.FlipHorizontally;
				}
				if (flipY)
				{
					spriteEffects |= SpriteEffects.FlipVertically;
				}
				float num = rotation;
				Vector2 vector = scale;
				Vector2 vector2 = position - new Vector2(0f, zPosition);
				for (int i = 0; i < drawEffects.Count; i++)
				{
					if (drawEffects[i].Apply(ref vector2, ref num, ref vector))
					{
						drawEffects.RemoveAt(i);
						i--;
					}
				}
				b.Draw(texture, _game.upperLeft + vector2 * 4f, spriteRect, color, num * ((float)Math.PI / 180f), spriteAnchor, 4f * vector, spriteEffects, layer_depth);
			}
		}

		public class SwayEffect : DrawEffect
		{
			public float swayMagnitude;

			public float swaySpeed;

			public int swayDuration = 1;

			public int age;

			public SwayEffect(float magnitude, float speed = 1f, int sway_duration = 10)
			{
				swayMagnitude = magnitude;
				swaySpeed = speed;
				swayDuration = sway_duration;
				age = 0;
			}

			public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
			{
				if (age > swayDuration)
				{
					return true;
				}
				float num = (float)age / (float)swayDuration;
				rotation += (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1000.0 * 360.0 * (double)swaySpeed * 0.01745329238474369) * (1f - num) * swayMagnitude;
				age++;
				return false;
			}
		}

		public class ShakeEffect : DrawEffect
		{
			public Vector2 shakeAmount;

			public int shakeDuration = 1;

			public int age;

			public ShakeEffect(float shake_x, float shake_y, int shake_duration = 10)
			{
				shakeAmount = new Vector2(shake_x, shake_y);
				shakeDuration = shake_duration;
				age = 0;
			}

			public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
			{
				if (age > shakeDuration)
				{
					return true;
				}
				float t = (float)age / (float)shakeDuration;
				Vector2 vector = new Vector2(Utility.Lerp(shakeAmount.X, 1f, t), Utility.Lerp(shakeAmount.Y, 1f, t));
				position += new Vector2((float)(Game1.random.NextDouble() - 0.5) * 2f * vector.X, (float)(Game1.random.NextDouble() - 0.5) * 2f * vector.Y);
				age++;
				return false;
			}
		}

		public class StretchEffect : DrawEffect
		{
			public Vector2 stretchScale;

			public int stretchDuration = 1;

			public int age;

			public StretchEffect(float x_scale, float y_scale, int stretch_duration = 10)
			{
				stretchScale = new Vector2(x_scale, y_scale);
				stretchDuration = stretch_duration;
				age = 0;
			}

			public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
			{
				if (age > stretchDuration)
				{
					return true;
				}
				float t = (float)age / (float)stretchDuration;
				Vector2 vector = new Vector2(Utility.Lerp(stretchScale.X, 1f, t), Utility.Lerp(stretchScale.Y, 1f, t));
				scale *= vector;
				age++;
				return false;
			}
		}

		public class DrawEffect
		{
			public virtual bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
			{
				return true;
			}
		}

		public ClickableTextureComponent upperRightCloseButton;

		public int gameWidth = 304;

		public int gameHeight = 150;

		protected LocalizedContentManager _content;

		public Texture2D spriteSheet;

		public Vector2 upperLeft;

		protected List<CraneGameObject> _gameObjects;

		protected Dictionary<GameButtons, int> _buttonStates;

		protected bool _shouldQuit;

		public Action onQuit;

		public ICue music;

		public ICue fastMusic;

		public Effect _effect;

		public int freezeFrames;

		public ICue craneSound;

		public List<Type> _gameObjectTypes;

		public Dictionary<Type, List<CraneGameObject>> _gameObjectsByType;

		public CraneGame()
		{
			Utility.farmerHeardSong("crane_game");
			Utility.farmerHeardSong("crane_game_fast");
			_effect = Game1.content.Load<Effect>("Effects\\ShadowRemove");
			_content = Game1.content.CreateTemporary();
			spriteSheet = _content.Load<Texture2D>("LooseSprites\\CraneGame");
			_buttonStates = new Dictionary<GameButtons, int>();
			_gameObjects = new List<CraneGameObject>();
			_gameObjectTypes = new List<Type>();
			_gameObjectsByType = new Dictionary<Type, List<CraneGameObject>>();
			changeScreenSize();
			new GameLogic(this);
			for (int i = 0; i < 9; i++)
			{
				_buttonStates[(GameButtons)i] = 0;
			}
			upperRightCloseButton = new ClickableTextureComponent(new Rectangle(Game1.viewport.Width - 68 - Game1.xEdge, 0, 68 + Game1.xEdge, 80), Game1.mobileSpriteSheet, new Rectangle(62, 0, 17, 17), 4f, drawShadow: true);
		}

		public void Quit()
		{
			if (!_shouldQuit)
			{
				if (onQuit != null)
				{
					onQuit();
				}
				_shouldQuit = true;
			}
		}

		protected void _UpdateInput()
		{
			HashSet<InputButton> hashSet = new HashSet<InputButton>();
			if (Game1.options.gamepadControls)
			{
				GamePadState padState = Game1.input.GetGamePadState();
				ButtonCollection.ButtonEnumerator enumerator = new ButtonCollection(ref padState).GetEnumerator();
				while (enumerator.MoveNext())
				{
					Buttons current = enumerator.Current;
					Keys key = Utility.mapGamePadButtonToKey(current);
					hashSet.Add(new InputButton(key));
				}
			}
			if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed)
			{
				hashSet.Add(new InputButton(mouseLeft: true));
			}
			else if (Game1.input.GetMouseState().RightButton == ButtonState.Pressed)
			{
				hashSet.Add(new InputButton(mouseLeft: false));
			}
			_UpdateButtonState(GameButtons.Action, Game1.options.actionButton, hashSet);
			_UpdateButtonState(GameButtons.Tool, Game1.options.useToolButton, hashSet);
			_UpdateButtonState(GameButtons.Confirm, Game1.options.menuButton, hashSet);
			_UpdateButtonState(GameButtons.Cancel, Game1.options.cancelButton, hashSet);
			_UpdateButtonState(GameButtons.Run, Game1.options.runButton, hashSet);
			_UpdateButtonState(GameButtons.Up, Game1.options.moveUpButton, hashSet);
			_UpdateButtonState(GameButtons.Down, Game1.options.moveDownButton, hashSet);
			_UpdateButtonState(GameButtons.Left, Game1.options.moveLeftButton, hashSet);
			_UpdateButtonState(GameButtons.Right, Game1.options.moveRightButton, hashSet);
		}

		public bool IsButtonPressed(GameButtons button)
		{
			return _buttonStates[button] == 1;
		}

		public bool IsButtonDown(GameButtons button)
		{
			return _buttonStates[button] > 0;
		}

		public bool IsButtonReleased(GameButtons button)
		{
			return _buttonStates[button] == -1;
		}

		public bool IsButtonDownRepeating(GameButtons button, int repeat_rate, int repeat_delay = 0)
		{
			int num = _buttonStates[button];
			if (num < 0)
			{
				num = -1;
			}
			if (num >= repeat_delay)
			{
				return (num - repeat_delay) % repeat_rate == 0;
			}
			return false;
		}

		public int GetButtonHoldTime(GameButtons button)
		{
			return Math.Max(_buttonStates[button], 0);
		}

		protected void _UpdateButtonState(GameButtons button, InputButton[] keys, HashSet<InputButton> emulated_keys)
		{
			bool flag = Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), keys);
			for (int i = 0; i < keys.Length; i++)
			{
				if (emulated_keys.Contains(keys[i]))
				{
					flag = true;
					break;
				}
			}
			if (_buttonStates[button] == -1)
			{
				_buttonStates[button] = 0;
			}
			if (flag)
			{
				_buttonStates[button]++;
			}
			else if (_buttonStates[button] > 0)
			{
				_buttonStates[button] = -1;
			}
		}

		public T GetObjectAtPoint<T>(Vector2 point, int max_count = -1) where T : CraneGameObject
		{
			foreach (CraneGameObject gameObject in _gameObjects)
			{
				if (gameObject is T && gameObject.GetBounds().Contains((int)point.X, (int)point.Y))
				{
					return gameObject as T;
				}
			}
			return null;
		}

		public List<T> GetObjectsAtPoint<T>(Vector2 point, int max_count = -1) where T : CraneGameObject
		{
			List<T> list = new List<T>();
			foreach (CraneGameObject gameObject in _gameObjects)
			{
				if (gameObject is T && gameObject.GetBounds().Contains((int)point.X, (int)point.Y))
				{
					list.Add(gameObject as T);
					if (max_count >= 0 && list.Count >= max_count)
					{
						return list;
					}
				}
			}
			return list;
		}

		public T GetObjectOfType<T>() where T : CraneGameObject
		{
			if (_gameObjectsByType.ContainsKey(typeof(T)))
			{
				List<CraneGameObject> list = _gameObjectsByType[typeof(T)];
				if (list.Count > 0)
				{
					return list[0] as T;
				}
			}
			return null;
		}

		public List<T> GetObjectsOfType<T>() where T : CraneGameObject
		{
			List<T> list = new List<T>();
			foreach (CraneGameObject gameObject in _gameObjects)
			{
				if (gameObject is T)
				{
					list.Add(gameObject as T);
				}
			}
			return list;
		}

		public T GetOverlap<T>(CraneGameObject target, int max_count = -1) where T : CraneGameObject
		{
			foreach (CraneGameObject gameObject in _gameObjects)
			{
				if (gameObject is T && target.GetBounds().Intersects(gameObject.GetBounds()) && target != gameObject)
				{
					return gameObject as T;
				}
			}
			return null;
		}

		public List<T> GetOverlaps<T>(CraneGameObject target, int max_count = -1) where T : CraneGameObject
		{
			List<T> list = new List<T>();
			foreach (CraneGameObject gameObject in _gameObjects)
			{
				if (gameObject is T && target.GetBounds().Intersects(gameObject.GetBounds()) && target != gameObject)
				{
					list.Add(gameObject as T);
					if (max_count >= 0 && list.Count >= max_count)
					{
						return list;
					}
				}
			}
			return list;
		}

		public bool tick(GameTime time)
		{
			if (_shouldQuit)
			{
				if (music != null && music.IsPlaying)
				{
					music.Stop(AudioStopOptions.Immediate);
				}
				if (fastMusic != null && fastMusic.IsPlaying)
				{
					fastMusic.Stop(AudioStopOptions.Immediate);
				}
				if (craneSound != null && !craneSound.IsStopped)
				{
					craneSound.Stop(AudioStopOptions.Immediate);
				}
				return true;
			}
			if (freezeFrames > 0)
			{
				freezeFrames--;
			}
			else
			{
				_UpdateInput();
				for (int i = 0; i < _gameObjects.Count; i++)
				{
					if (_gameObjects[i] != null)
					{
						_gameObjects[i].Update(time);
					}
				}
			}
			if (IsButtonPressed(GameButtons.Confirm))
			{
				Quit();
				Game1.playSound("bigDeSelect");
				GameLogic objectOfType = GetObjectOfType<GameLogic>();
				if (objectOfType != null && objectOfType.collectedItems.Count > 0)
				{
					List<Item> list = new List<Item>();
					foreach (Item collectedItem in objectOfType.collectedItems)
					{
						list.Add(collectedItem.getOne());
					}
					Game1.activeClickableMenu = new ItemGrabMenu(list, reverseGrab: false, showReceivingMenu: true, null, null, "Rewards", null, snapToBottom: false, canBeExitedWithKey: false, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, this);
				}
			}
			return false;
		}

		public bool forceQuit()
		{
			Quit();
			GameLogic objectOfType = GetObjectOfType<GameLogic>();
			if (objectOfType != null)
			{
				List<Item> list = new List<Item>();
				foreach (Item collectedItem in objectOfType.collectedItems)
				{
					Utility.CollectOrDrop(collectedItem.getOne());
				}
			}
			return true;
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (upperRightCloseButton.containsPoint(x, y))
			{
				Quit();
			}
		}

		public void leftClickHeld(int x, int y)
		{
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void releaseLeftClick(int x, int y)
		{
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public void receiveKeyPress(Keys k)
		{
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public void RegisterGameObject(CraneGameObject game_object)
		{
			if (!_gameObjectTypes.Contains(game_object.GetType()))
			{
				_gameObjectTypes.Add(game_object.GetType());
				_gameObjectsByType[game_object.GetType()] = new List<CraneGameObject>();
			}
			_gameObjectsByType[game_object.GetType()].Add(game_object);
			_gameObjects.Add(game_object);
		}

		public void UnregisterGameObject(CraneGameObject game_object)
		{
			_gameObjectsByType[game_object.GetType()].Remove(game_object);
			_gameObjects.Remove(game_object);
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _effect);
			b.Draw(spriteSheet, upperLeft, new Rectangle(0, 0, gameWidth, gameHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			Dictionary<CraneGameObject, float> dictionary = new Dictionary<CraneGameObject, float>();
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < _gameObjects.Count; i++)
			{
				if (_gameObjects[i] != null)
				{
					float rendererLayerDepth = _gameObjects[i].GetRendererLayerDepth();
					dictionary[_gameObjects[i]] = rendererLayerDepth;
					if (rendererLayerDepth < num)
					{
						num = rendererLayerDepth;
					}
					if (rendererLayerDepth > num2)
					{
						num2 = rendererLayerDepth;
					}
				}
			}
			for (int j = 0; j < _gameObjectTypes.Count; j++)
			{
				Type key = _gameObjectTypes[j];
				for (int k = 0; k < _gameObjectsByType[key].Count; k++)
				{
					float layer_depth = Utility.Lerp(0.1f, 0.9f, (dictionary[_gameObjectsByType[key][k]] - num) / (num2 - num));
					_gameObjectsByType[key][k].Draw(b, layer_depth);
				}
			}
			upperRightCloseButton.bounds.X = Game1.viewport.Width - 68 - Game1.xEdge;
			upperRightCloseButton.draw(b);
			b.End();
		}

		public void changeScreenSize()
		{
			float num = 1f / Game1.options.zoomLevel;
			Rectangle localMultiplayerWindow = Game1.game1.localMultiplayerWindow;
			float num2 = localMultiplayerWindow.Width;
			float num3 = localMultiplayerWindow.Height;
			Vector2 vector = new Vector2(num2 / 2f, num3 / 2f) * num;
			vector.X -= gameWidth / 2 * 4;
			vector.Y -= gameHeight / 2 * 4;
			if (upperLeft != vector)
			{
				upperLeft = vector;
				Console.WriteLine("CraneGame.changeScreenSize(); vp={0}, zl={1}, upperLeft={2}", localMultiplayerWindow, Game1.options.zoomLevel, upperLeft);
			}
		}

		public void unload()
		{
			Game1.stopMusicTrack(Game1.MusicContext.MiniGame);
			_content.Unload();
		}

		public void receiveEventPoke(int data)
		{
		}

		public string minigameId()
		{
			return "CraneGame";
		}

		public float GetForcedScaleFactor()
		{
			return Game1.NativeZoomLevel;
		}
	}
}
