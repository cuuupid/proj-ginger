using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Minigames
{
	public class OldMineCart : IMinigame
	{
		private class Spark
		{
			public float x;

			public float y;

			public Color c;

			public float dx;

			public float dy;

			public Spark(float x, float y, float dx, float dy)
			{
				this.x = x;
				this.y = y;
				this.dx = dx;
				this.dy = dy;
				c = Color.Yellow;
			}
		}

		public const int track1 = 1;

		public const int track2 = 2;

		public const int noTrack = 0;

		public const int trackSlopeLeft = 3;

		public const int trackSlopeRight = 4;

		public const int minecartObstacle = 1;

		public const int coinObstacle = 2;

		public int pixelScale = 4;

		public const int maxTrackDeviationFromZero = 6;

		public const int tilesBeyondViewportToSimulate = 4;

		public const int bgLoopWidth = 96;

		public const int tileOfMineCart = 6;

		public const int gapsBeforeForcedTrack = 4;

		public const int tracksBeforeConsideredObstacle = 2;

		public const float gravity = 0.21f;

		public const float snapMinecartToTrackThreshold = 6f;

		public const float maxDY = 4.5f;

		public float maxJumpHeight;

		public const float jumpStrengthPerTick = 0.6f;

		public const float dyThreshAtWhichJumpIsImpossible = 1f;

		public const int frostArea = 0;

		public const int lavaArea = 1;

		public const int waterArea = 2;

		public const int darkArea = 3;

		public const int heavenlyArea = 4;

		public const int brownArea = 5;

		public const int noSlope = 0;

		public const int slopingUp = 1;

		public const int slopingDown = 2;

		public const int mineLevelMode = 0;

		public const int arcadeTitleScreenMode = 1;

		public const int infiniteMode = 2;

		public const int progressMode = 3;

		public const int highScoreMode = 4;

		public const int respawnTime = 1400;

		public const int distanceToTravelInMineMode = 350;

		public const double ceilingHeightFluctuation = 0.15;

		public const double coinOccurance = 0.01;

		private float speed;

		private float speedAccumulator;

		private float lakeSpeedAccumulator;

		private float backBGPosition;

		private float midBGPosition;

		private float waterFallPosition;

		private int noiseSeed = Game1.random.Next(0, 2147483647);

		private int currentTrackY;

		private int screenWidth;

		private int screenHeight;

		private int tileSize;

		private int waterfallWidth = 1;

		private int ytileOffset;

		private int totalMotion;

		private int movingOnSlope;

		private int levelsBeat;

		private int gameMode;

		private int livesLeft;

		private int distanceToTravel = -1;

		private int respawnCounter;

		private int currentTheme;

		private float mineCartYPosition;

		private float mineCartXOffset;

		private float minecartDY;

		private float minecartPositionBeforeJump;

		private float minecartBumpOffset;

		private double lastNoiseValue;

		private double heightChangeThreshold;

		private double obstacleOccurance;

		private double heightFluctuationsThreshold;

		private bool isJumping;

		private bool reachedJumpApex;

		private bool reachedFinish;

		private float screenDarkness;

		private ICue minecartLoop;

		private Texture2D texture;

		private List<Point> track = new List<Point>();

		private List<Point> lakeDecor = new List<Point>();

		private List<Point> ceiling = new List<Point>();

		private List<Point> obstacles = new List<Point>();

		private List<Spark> sparkShower = new List<Spark>();

		private Color backBGTint;

		private Color midBGTint;

		private Color caveTint;

		private Color lakeTint;

		private Color waterfallTint;

		private Color trackShadowTint;

		private Color trackTint;

		private Matrix transformMatrix;

		public OldMineCart(int whichTheme, int mode)
		{
			changeScreenSize();
			maxJumpHeight = (float)(64 / pixelScale) * 5f;
			texture = Game1.content.Load<Texture2D>("Minigames\\MineCart");
			if (Game1.soundBank != null)
			{
				minecartLoop = Game1.soundBank.GetCue("minecartLoop");
				minecartLoop.Play();
			}
			ytileOffset = screenHeight / 2 / tileSize;
			gameMode = mode;
			setGameModeParameters();
			setUpTheme(whichTheme);
			createBeginningOfLevel();
			screenDarkness = 1f;
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public bool forceQuit()
		{
			unload();
			return true;
		}

		public bool tick(GameTime time)
		{
			if (!reachedFinish && (livesLeft > 0 || gameMode == 2) && screenDarkness > 0f)
			{
				screenDarkness -= (float)time.ElapsedGameTime.Milliseconds * 0.002f;
			}
			int num = ((track.ElementAt(6).X == 0) ? 9999 : (track.ElementAt(6).Y * tileSize + (int)((track.ElementAt(6).X == 3) ? (0f - speedAccumulator) : ((track.ElementAt(6).X == 4) ? (speedAccumulator - 16f) : 0f))));
			if (respawnCounter <= 0 || track[6].X == 0 || obstacles[6].X == 1 || obstacles[7].X == 1)
			{
				speedAccumulator += (float)time.ElapsedGameTime.Milliseconds * speed;
				num = ((track.ElementAt(6).X == 0) ? 9999 : (track.ElementAt(6).Y * tileSize + (int)((track.ElementAt(6).X == 3) ? (0f - speedAccumulator) : ((track.ElementAt(6).X == 4) ? (speedAccumulator - 16f) : 0f))));
				if (speedAccumulator >= (float)tileSize)
				{
					if (!isJumping && movingOnSlope == 0 && Game1.random.NextDouble() < 0.5)
					{
						minecartBumpOffset = Game1.random.Next(1, 3);
					}
					if ((totalMotion + 1) % 1000 == 0)
					{
						Game1.playSound("newArtifact");
					}
					else if ((totalMotion + 1) % 100 == 0)
					{
						Game1.playSound("Pickup_Coin15");
					}
					totalMotion++;
					if (totalMotion > Game1.minecartHighScore)
					{
						Game1.minecartHighScore = totalMotion;
					}
					if (distanceToTravel != -1 && totalMotion >= distanceToTravel + screenWidth / tileSize)
					{
						if (!reachedFinish)
						{
							Game1.playSound("reward");
						}
						reachedFinish = true;
					}
					track.RemoveAt(0);
					ceiling.RemoveAt(0);
					obstacles.RemoveAt(0);
					if (distanceToTravel == -1 || totalMotion < distanceToTravel)
					{
						double num2 = NoiseGenerator.Noise(totalMotion, noiseSeed);
						Point item = Point.Zero;
						if (num2 > heightChangeThreshold && lastNoiseValue <= heightChangeThreshold)
						{
							currentTrackY = Math.Max(currentTrackY - Game1.random.Next(1, 2), -6);
						}
						else if (num2 < heightChangeThreshold && lastNoiseValue >= heightChangeThreshold)
						{
							currentTrackY = Math.Min(currentTrackY + Game1.random.Next(1, (currentTrackY <= -3) ? 6 : 3), 4);
						}
						else if (Math.Abs(num2 - lastNoiseValue) > heightFluctuationsThreshold)
						{
							if (track[track.Count - 1].X == 0)
							{
								currentTrackY = Math.Max(-6, Math.Min(6, currentTrackY + Game1.random.Next(1, 1)));
							}
							else
							{
								currentTrackY = Math.Max(Math.Min(4, currentTrackY + Game1.random.Next(-3, 3)), -6);
							}
						}
						if (num2 < -0.5)
						{
							bool flag = false;
							for (int i = 0; i < 4 - ((Game1.random.NextDouble() < 0.1) ? 1 : 0); i++)
							{
								if (track[track.Count - 1 - i].X != 0)
								{
									flag = true;
									break;
								}
							}
							item = ((!flag) ? new Point(Game1.random.Next(1, 3), (Game1.random.NextDouble() < 0.5 && currentTrackY < 6) ? currentTrackY : (currentTrackY + 1)) : new Point(0, 999));
						}
						else
						{
							item = new Point(Game1.random.Next(1, 3), currentTrackY);
						}
						if (track[track.Count - 1].X == 0 && item.X != 0)
						{
							item.Y = Math.Min(6, item.Y + 1);
						}
						if (item.Y == track[track.Count - 1].Y - 1)
						{
							track.RemoveAt(track.Count - 1);
							track.Add(new Point(3, currentTrackY + 1));
						}
						else if (item.Y == track[track.Count - 1].Y + 1)
						{
							item.X = 4;
						}
						track.Add(item);
						ceiling.Add(new Point(Game1.random.Next(200), Math.Min(currentTrackY - 5 + ytileOffset, Math.Max(0, ceiling.Last().Y + ((Game1.random.NextDouble() < 0.15) ? Game1.random.Next(-1, 2) : 0)))));
						bool flag2 = false;
						for (int j = 0; j < 2; j++)
						{
							if (track[track.Count - 1 - j].X == 0 || track[track.Count - 1 - j - 1].Y != track[track.Count - 1 - j].Y)
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2 && Game1.random.NextDouble() < obstacleOccurance && currentTrackY > -2 && track.Last().X != 3 && track.Last().X != 4)
						{
							obstacles.Add(new Point(1, currentTrackY));
						}
						else
						{
							obstacles.Add(Point.Zero);
						}
						lastNoiseValue = num2;
					}
					else
					{
						track.Add(new Point(Game1.random.Next(1, 3), currentTrackY));
						ceiling.Add(new Point(Game1.random.Next(200), ceiling.Last().Y));
						obstacles.Add(Point.Zero);
						lakeDecor.Add(new Point(Game1.random.Next(2), Game1.random.Next(ytileOffset + 1, screenHeight / tileSize)));
					}
					speedAccumulator %= tileSize;
				}
				lakeSpeedAccumulator += (float)time.ElapsedGameTime.Milliseconds * (speed / 4f);
				if (lakeSpeedAccumulator >= (float)tileSize)
				{
					lakeSpeedAccumulator %= tileSize;
					lakeDecor.RemoveAt(0);
					lakeDecor.Add(new Point(Game1.random.Next(2), Game1.random.Next(ytileOffset + 3, screenHeight / tileSize)));
				}
				backBGPosition += (float)time.ElapsedGameTime.Milliseconds * (speed / 5f);
				backBGPosition %= 96f;
				midBGPosition += (float)time.ElapsedGameTime.Milliseconds * (speed / 4f);
				midBGPosition %= 96f;
				waterFallPosition += (float)time.ElapsedGameTime.Milliseconds * (speed * 6f / 5f);
				if (waterFallPosition > (float)(screenWidth * 3 / 2))
				{
					waterFallPosition %= screenWidth * 3 / 2;
					waterfallWidth = Game1.random.Next(6);
				}
			}
			else
			{
				respawnCounter -= time.ElapsedGameTime.Milliseconds;
				mineCartYPosition = num;
			}
			if (Math.Abs(mineCartYPosition - (float)num) <= 6f && minecartDY >= 0f && movingOnSlope == 0)
			{
				if (minecartDY > 0f)
				{
					mineCartYPosition = num;
					minecartDY = 0f;
					if (Game1.soundBank != null)
					{
						ICue cue = Game1.soundBank.GetCue("parry");
						cue.Play();
						minecartLoop = Game1.soundBank.GetCue("minecartLoop");
						minecartLoop.Play();
					}
					isJumping = false;
					reachedJumpApex = false;
					createSparkShower();
				}
				if (track[6].X == 3)
				{
					movingOnSlope = 1;
					createSparkShower();
				}
				else if (track[6].X == 4)
				{
					movingOnSlope = 2;
					createSparkShower();
				}
			}
			else if (!isJumping && Math.Abs(mineCartYPosition - (float)num) <= 6f && (track[6].X == 3 || track[6].X == 4))
			{
				mineCartYPosition = num;
				if (mineCartYPosition == (float)num && track[6].X == 3)
				{
					movingOnSlope = 1;
					if (respawnCounter <= 0)
					{
						createSparkShower(Game1.random.Next(2));
					}
				}
				else if (mineCartYPosition == (float)num && track[6].X == 4)
				{
					movingOnSlope = 2;
					if (respawnCounter <= 0)
					{
						createSparkShower(Game1.random.Next(2));
					}
				}
				minecartDY = 0f;
			}
			else
			{
				movingOnSlope = 0;
				minecartDY += (((reachedJumpApex || !isJumping) && mineCartYPosition != (float)num) ? 0.21f : 0f);
				if (minecartDY > 0f)
				{
					minecartDY = Math.Min(minecartDY, 9f);
				}
				if (minecartDY > 0f || minecartPositionBeforeJump - mineCartYPosition <= maxJumpHeight)
				{
					mineCartYPosition += minecartDY;
				}
			}
			if (minecartDY > 0f && minecartLoop != null && minecartLoop.IsPlaying)
			{
				minecartLoop.Stop(AudioStopOptions.Immediate);
			}
			if (reachedFinish)
			{
				mineCartXOffset += speed * (float)time.ElapsedGameTime.Milliseconds;
				if (Game1.random.NextDouble() < 0.25)
				{
					createSparkShower();
				}
			}
			if (mineCartXOffset > (float)(screenWidth - 6 * tileSize + tileSize))
			{
				switch (gameMode)
				{
				case 0:
					screenDarkness += (float)time.ElapsedGameTime.Milliseconds / 2000f;
					if (screenDarkness >= 1f)
					{
						if (Game1.mine != null)
						{
							Game1.enterMine(Game1.CurrentMineLevel + 3);
							Game1.fadeToBlackAlpha = 1f;
						}
						return true;
					}
					break;
				case 3:
					screenDarkness += (float)time.ElapsedGameTime.Milliseconds / 2000f;
					if (!(screenDarkness >= 1f))
					{
						break;
					}
					reachedFinish = false;
					currentTheme = (currentTheme + 1) % 6;
					levelsBeat++;
					if (levelsBeat == 6)
					{
						if (!Game1.player.hasOrWillReceiveMail("JunimoKart"))
						{
							Game1.addMailForTomorrow("JunimoKart");
						}
						Game1.multiplayer.globalChatInfoMessage("JunimoKart", Game1.player.Name);
						unload();
						Game1.currentMinigame = null;
						DelayedAction.playSoundAfterDelay("discoverMineral", 1000);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:OldMineCart.cs.12106"));
					}
					else
					{
						setUpTheme(currentTheme);
						restartLevel();
					}
					break;
				}
			}
			if (speedAccumulator >= (float)(tileSize / 2) && ((int)(mineCartYPosition / (float)tileSize) == obstacles[7].Y || (int)(mineCartYPosition / (float)tileSize - (float)(tileSize - 1)) == obstacles[7].Y))
			{
				switch (obstacles[7].X)
				{
				case 1:
					Game1.playSound("woodWhack");
					mineCartYPosition = screenHeight;
					break;
				case 2:
					Game1.playSound("money");
					obstacles.RemoveAt(6);
					obstacles.Insert(6, Point.Zero);
					break;
				}
			}
			if (mineCartYPosition > (float)screenHeight)
			{
				mineCartYPosition = -999999f;
				livesLeft--;
				Game1.playSound("fishEscape");
				if (gameMode == 0 && (float)livesLeft < 0f)
				{
					mineCartYPosition = 999999f;
					livesLeft++;
					screenDarkness += (float)time.ElapsedGameTime.Milliseconds * 0.001f;
					if (screenDarkness >= 1f)
					{
						if (Game1.player.health > 1)
						{
							Game1.player.health = 1;
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:OldMineCart.cs.12108"));
						}
						else
						{
							Game1.player.health = 0;
						}
						return true;
					}
				}
				else if (gameMode == 4 || (gameMode == 3 && livesLeft < 0))
				{
					if (gameMode == 3)
					{
						levelsBeat = 0;
						setUpTheme(5);
					}
					restartLevel();
				}
				else
				{
					respawnCounter = 1400;
					minecartDY = 0f;
					isJumping = false;
					reachedJumpApex = false;
					if (gameMode == 2)
					{
						totalMotion = 0;
					}
				}
			}
			minecartBumpOffset = Math.Max(0f, minecartBumpOffset - 0.5f);
			for (int num3 = sparkShower.Count - 1; num3 >= 0; num3--)
			{
				sparkShower[num3].dy += 0.105f;
				sparkShower[num3].x += sparkShower[num3].dx;
				sparkShower[num3].y += sparkShower[num3].dy;
				sparkShower[num3].c.B = (byte)(0.0 + Math.Max(0.0, Math.Sin((double)time.TotalGameTime.Milliseconds / (Math.PI * 20.0 / (double)sparkShower[num3].dx)) * 255.0));
				if (reachedFinish)
				{
					sparkShower[num3].c.R = (byte)(0.0 + Math.Max(0.0, Math.Sin((double)(time.TotalGameTime.Milliseconds + 50) / (Math.PI * 20.0 / (double)sparkShower[num3].dx)) * 255.0));
					sparkShower[num3].c.G = (byte)(0.0 + Math.Max(0.0, Math.Sin((double)(time.TotalGameTime.Milliseconds + 100) / (Math.PI * 20.0 / (double)sparkShower[num3].dx)) * 255.0));
					if (sparkShower[num3].c.R == 0)
					{
						sparkShower[num3].c.R = 255;
					}
					if (sparkShower[num3].c.G == 0)
					{
						sparkShower[num3].c.G = 255;
					}
				}
				if (sparkShower[num3].y > (float)screenHeight)
				{
					sparkShower.RemoveAt(num3);
				}
			}
			return false;
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
			jump();
		}

		public void releaseLeftClick(int x, int y)
		{
			releaseJump();
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void receiveKeyPress(Keys k)
		{
			if ((k.Equals(Keys.Escape) || Game1.options.doesInputListContain(Game1.options.menuButton, k)) && (!Game1.isAnyGamePadButtonBeingPressed() || Game1.input.GetGamePadState().IsButtonDown(Buttons.Start)))
			{
				unload();
				Game1.playSound("bigDeSelect");
				Game1.currentMinigame = null;
			}
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		private void restartLevel()
		{
			track.Clear();
			ceiling.Clear();
			lakeDecor.Clear();
			obstacles.Clear();
			totalMotion = 0;
			speedAccumulator = 0f;
			currentTrackY = 0;
			mineCartYPosition = 0f;
			minecartDY = 0f;
			isJumping = false;
			reachedJumpApex = false;
			reachedFinish = false;
			movingOnSlope = 0;
			mineCartXOffset = 0f;
			createBeginningOfLevel();
			setGameModeParameters();
		}

		public void createSparkShower()
		{
			int num = Game1.random.Next(3, 7);
			for (int i = 0; i < num; i++)
			{
				sparkShower.Add(new Spark((float)(6 * tileSize - 3) + mineCartXOffset, mineCartYPosition + (float)(ytileOffset * tileSize) + (float)tileSize - 4f, (float)Game1.random.Next(-200, 5) / 100f, (float)(-Game1.random.Next(5, 150)) / 100f));
			}
		}

		public void createSparkShower(int number)
		{
			for (int i = 0; i < number; i++)
			{
				sparkShower.Add(new Spark((float)(6 * tileSize - 3) + mineCartXOffset, mineCartYPosition + (float)(ytileOffset * tileSize) + (float)tileSize - 4f, (float)Game1.random.Next(-200, 5) / 100f, (float)(-Game1.random.Next(5, 150)) / 100f));
			}
		}

		public void createBeginningOfLevel()
		{
			for (int i = 0; i < screenWidth / tileSize + 4; i++)
			{
				track.Add(new Point(Game1.random.Next(1, 3), 0));
				ceiling.Add(new Point(Game1.random.Next(200), 0));
				obstacles.Add(Point.Zero);
				lakeDecor.Add(new Point(Game1.random.Next(2), Game1.random.Next(ytileOffset + 3, screenHeight / tileSize)));
			}
		}

		public void setGameModeParameters()
		{
			switch (gameMode)
			{
			case 0:
				distanceToTravel = 200;
				livesLeft = 3;
				break;
			case 3:
				distanceToTravel = 200;
				livesLeft = 3;
				break;
			}
		}

		public void setUpTheme(int whichTheme)
		{
			switch (whichTheme)
			{
			case 0:
				backBGTint = new Color(254, 254, 254);
				midBGTint = new Color(254, 254, 254);
				caveTint = new Color(230, 244, 254);
				lakeTint = new Color(150, 210, 255);
				waterfallTint = Color.LightCyan * 0.5f;
				trackTint = Color.LightCyan;
				speed = 0.085f;
				NoiseGenerator.Amplitude = 2.8;
				NoiseGenerator.Frequency = 0.18;
				heightChangeThreshold = 0.85;
				obstacleOccurance = 0.05;
				heightFluctuationsThreshold = 0.35;
				trackShadowTint = Color.DarkSlateBlue;
				break;
			case 2:
				backBGTint = new Color(50, 150, 225);
				midBGTint = new Color(120, 170, 225);
				caveTint = Color.SlateGray;
				lakeTint = new Color(30, 120, 215);
				waterfallTint = Color.White * 0.5f;
				trackTint = Color.Gray;
				speed = 0.085f;
				NoiseGenerator.Amplitude = 3.0;
				NoiseGenerator.Frequency = 0.15;
				heightChangeThreshold = 0.9;
				obstacleOccurance = 0.05;
				heightFluctuationsThreshold = 0.4;
				trackShadowTint = Color.DarkSlateBlue;
				break;
			case 1:
				backBGTint = Color.DarkRed;
				midBGTint = Color.DarkSalmon;
				caveTint = Color.DarkRed;
				lakeTint = Color.DarkRed;
				trackTint = Color.DarkGray;
				waterfallTint = Color.Red * 0.9f;
				trackShadowTint = Color.DarkOrange;
				speed = 0.12f;
				heightChangeThreshold = 0.8;
				NoiseGenerator.Amplitude = 3.0;
				NoiseGenerator.Frequency = 0.18;
				obstacleOccurance = 0.05;
				heightFluctuationsThreshold = 0.2;
				break;
			case 3:
				backBGTint = new Color(60, 60, 60);
				midBGTint = new Color(60, 60, 60);
				caveTint = new Color(70, 70, 70);
				lakeTint = new Color(60, 70, 80);
				trackTint = Color.DimGray;
				waterfallTint = Color.Black * 0f;
				trackShadowTint = Color.Black;
				speed = 0.1f;
				heightChangeThreshold = 0.7;
				NoiseGenerator.Amplitude = 3.0;
				NoiseGenerator.Frequency = 0.2;
				obstacleOccurance = 0.0;
				heightFluctuationsThreshold = 0.2;
				break;
			case 4:
				backBGTint = Color.SeaGreen;
				midBGTint = Color.Green;
				caveTint = new Color(255, 200, 60);
				lakeTint = Color.Lime;
				trackTint = Color.LightSlateGray;
				waterfallTint = Color.ForestGreen * 0.5f;
				trackShadowTint = new Color(0, 180, 50);
				speed = 0.08f;
				heightChangeThreshold = 0.6;
				NoiseGenerator.Amplitude = 3.1;
				NoiseGenerator.Frequency = 0.24;
				obstacleOccurance = 0.05;
				heightFluctuationsThreshold = 0.15;
				break;
			case 5:
				backBGTint = Color.DarkKhaki;
				midBGTint = Color.SandyBrown;
				caveTint = Color.SandyBrown;
				lakeTint = Color.MediumAquamarine;
				trackTint = Color.Beige;
				waterfallTint = Color.MediumAquamarine * 0.9f;
				trackShadowTint = new Color(60, 60, 60);
				speed = 0.085f;
				heightChangeThreshold = 0.8;
				NoiseGenerator.Amplitude = 2.0;
				NoiseGenerator.Frequency = 0.12;
				obstacleOccurance = 0.05;
				heightFluctuationsThreshold = 0.25;
				break;
			}
			currentTheme = whichTheme;
		}

		private void jump()
		{
			if (!(minecartDY < 1f) || respawnCounter > 0)
			{
				return;
			}
			if (!isJumping)
			{
				movingOnSlope = 0;
				minecartPositionBeforeJump = mineCartYPosition;
				isJumping = true;
				if (minecartLoop != null)
				{
					minecartLoop.Stop(AudioStopOptions.Immediate);
				}
				if (Game1.soundBank != null)
				{
					ICue cue = Game1.soundBank.GetCue("pickUpItem");
					cue.SetVariable("Pitch", 200);
					cue.Play();
				}
			}
			if (!reachedJumpApex)
			{
				minecartDY = Math.Max(-4.5f, minecartDY - 0.6f);
				if (minecartDY <= -4.5f)
				{
					reachedJumpApex = true;
				}
			}
		}

		private void releaseJump()
		{
			if (isJumping)
			{
				reachedJumpApex = true;
			}
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, transformMatrix);
			for (int i = 0; i <= screenWidth / tileSize + 1; i++)
			{
				b.Draw(texture, new Rectangle(i * tileSize - (int)lakeSpeedAccumulator, tileSize * 9, tileSize, screenHeight - 96), new Rectangle(0, 80, 16, 97), lakeTint);
			}
			for (int j = 0; j < lakeDecor.Count; j++)
			{
				b.Draw(texture, new Vector2((float)(j * tileSize) - lakeSpeedAccumulator, lakeDecor.ElementAt(j).Y * tileSize), new Rectangle(32 + lakeDecor.ElementAt(j).X * tileSize, 0, 16, 16), (lakeDecor.ElementAt(j).X == 0) ? midBGTint : lakeTint);
			}
			for (int k = 0; k <= screenWidth / 96 + 2; k++)
			{
				b.Draw(texture, new Vector2(0f - backBGPosition + (float)(k * 96), tileSize * 2), new Rectangle(64, 162, 96, 111), backBGTint);
			}
			for (int l = 0; l < screenWidth / 96 + 2; l++)
			{
				b.Draw(texture, new Vector2(0f - midBGPosition + (float)(l * 96), 0f), new Rectangle(64, 0, 96, 162), midBGTint);
			}
			for (int m = 0; m < track.Count; m++)
			{
				if (track.ElementAt(m).X != 0)
				{
					b.Draw(texture, new Vector2(0f - speedAccumulator + (float)(m * tileSize), (track.ElementAt(m).Y + ytileOffset) * tileSize - tileSize), new Rectangle(160 + (track.ElementAt(m).X - 1) * 16, 144, 16, 32), trackTint);
					float num = 0f;
					for (int n = track.ElementAt(m).Y + 1; n < screenHeight / tileSize; n++)
					{
						b.Draw(texture, new Vector2(0f - speedAccumulator + (float)(m * tileSize), (n + ytileOffset) * tileSize), new Rectangle(16 + ((n % 2 == 0) ? ((track.ElementAt(m).X + 1) % 2) : (track.ElementAt(m).X % 2)) * 16, 32, 16, 16), trackTint);
						b.Draw(texture, new Vector2(0f - speedAccumulator + (float)(m * tileSize), (n + ytileOffset) * tileSize), new Rectangle(16 + ((n % 2 == 0) ? ((track.ElementAt(m).X + 1) % 2) : (track.ElementAt(m).X % 2)) * 16, 32, 16, 16), trackShadowTint * num);
						num += 0.1f;
					}
				}
			}
			for (int num2 = 0; num2 < obstacles.Count; num2++)
			{
				switch (obstacles[num2].X)
				{
				case 1:
					b.Draw(texture, new Vector2(0f - speedAccumulator + (float)(num2 * tileSize), (obstacles[num2].Y + ytileOffset) * tileSize), new Rectangle(16, 0, 16, 16), Color.White);
					break;
				case 2:
					b.Draw(Game1.debrisSpriteSheet, new Vector2(0f - speedAccumulator + (float)(num2 * tileSize), (obstacles[num2].Y + ytileOffset) * tileSize), Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8), Color.White, 0f, new Vector2(32f, 0f), 0.25f, SpriteEffects.None, 0f);
					break;
				}
			}
			if (respawnCounter / 200 % 2 == 0)
			{
				b.Draw(texture, new Vector2((float)(6 * tileSize + tileSize / 2) + mineCartXOffset, (float)(ytileOffset * tileSize) + mineCartYPosition + (float)tileSize - minecartBumpOffset - 4f), new Rectangle(0, 0, 16, 16), Color.White, (minecartDY < 0f) ? (minecartDY / (float)Math.PI / 2f) : 0f, new Vector2(16f, 16f), 1f, SpriteEffects.None, 0f);
				Game1.player.faceDirection(1);
				b.Draw(Game1.mouseCursors, new Vector2((float)(6 * tileSize - 2) + mineCartXOffset, (float)(ytileOffset * tileSize - 22) + mineCartYPosition + (float)tileSize + (float)tileSize - minecartBumpOffset - 8f), new Rectangle(294 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0) / 100 * 16, 1432, 16, 16), Color.Lime, (minecartDY < 0f) ? (minecartDY / (float)Math.PI / 4f) : 0f, new Vector2(8f, 8f), 2f / 3f, SpriteEffects.None, 0.1f);
				b.Draw(texture, new Vector2((float)(6 * tileSize + tileSize / 2) + mineCartXOffset, (float)(ytileOffset * tileSize) + mineCartYPosition + (float)tileSize - minecartBumpOffset - 4f + 8f), new Rectangle(0, 8, 16, 8), Color.White, (minecartDY < 0f) ? (minecartDY / (float)Math.PI / 4f) : 0f, new Vector2(16f, 16f), 1f, SpriteEffects.None, 0.1f);
			}
			foreach (Spark item in sparkShower)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)item.x, (int)item.y, pixelScale / 4, pixelScale / 4), item.c);
			}
			for (int num3 = 0; num3 < waterfallWidth; num3 += 2)
			{
				for (int num4 = -2; num4 <= screenHeight / tileSize + 1; num4++)
				{
					b.Draw(texture, new Vector2((float)(screenWidth + tileSize * num3) - waterFallPosition, (float)(num4 * tileSize) + lakeSpeedAccumulator * 2f), new Rectangle(48, 32, 16, 16), waterfallTint);
				}
			}
			if (gameMode != 2 && totalMotion < distanceToTravel + screenWidth / tileSize)
			{
				if (gameMode != 4)
				{
					for (int num5 = 0; num5 < livesLeft; num5++)
					{
						b.Draw(texture, new Vector2(screenWidth - num5 * (tileSize + 2) - tileSize, 0f), new Rectangle(0, 0, 16, 16), Color.White);
					}
				}
				b.Draw(Game1.staminaRect, new Rectangle(pixelScale, pixelScale, tileSize * 8, pixelScale), Color.LightGray);
				b.Draw(Game1.staminaRect, new Rectangle(pixelScale + (int)((float)totalMotion / (float)(distanceToTravel + screenWidth / tileSize) * (float)(tileSize * 8 - pixelScale)), pixelScale, pixelScale, pixelScale), Color.Aquamarine);
				for (int num6 = 0; num6 < 4; num6++)
				{
					b.Draw(Game1.staminaRect, new Rectangle(pixelScale + tileSize * 8, pixelScale + num6 * (pixelScale / 4), pixelScale / 4, pixelScale / 4), (num6 % 2 == 0) ? Color.White : Color.Black);
					b.Draw(Game1.staminaRect, new Rectangle(pixelScale + tileSize * 8 + pixelScale / 4, pixelScale + num6 * (pixelScale / 4), pixelScale / 4, pixelScale / 4), (num6 % 2 == 0) ? Color.Black : Color.White);
				}
				b.DrawString(Game1.dialogueFont, (levelsBeat + 1).ToString() ?? "", new Vector2(pixelScale * 2 + tileSize * 8, (float)pixelScale / 2f), Color.Orange, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
			}
			else if (gameMode == 2)
			{
				string text = Game1.content.LoadString("Strings\\StringsFromCSFiles:OldMineCart.cs.12115");
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.10444", totalMotion), new Vector2(1f, 1f), Color.White, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
				b.DrawString(Game1.dialogueFont, text + Game1.minecartHighScore, new Vector2(128f, 1f), Color.White, 0f, Vector2.Zero, 0.25f, SpriteEffects.None, 0f);
			}
			if (screenDarkness > 0f)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, screenWidth, screenHeight + tileSize), Color.Black * screenDarkness);
			}
			b.End();
		}

		public void changeScreenSize()
		{
			pixelScale = 4;
			int num = ((Game1.viewport.Height < 1000) ? 3 : 4);
			screenWidth = Game1.viewport.Width / num;
			screenHeight = Game1.viewport.Height / num;
			tileSize = 64 / pixelScale;
			ytileOffset = screenHeight / 2 / tileSize;
			maxJumpHeight = (float)(64 / pixelScale) * 5f;
			transformMatrix = Matrix.CreateScale(num);
		}

		public void unload()
		{
			Game1.player.faceDirection(0);
			if (minecartLoop != null && minecartLoop.IsPlaying)
			{
				minecartLoop.Stop(AudioStopOptions.Immediate);
			}
		}

		public void leftClickHeld(int x, int y)
		{
			if (isJumping && !reachedJumpApex)
			{
				minecartDY = Math.Max(-4.5f, minecartDY - 0.6f);
				if (minecartDY == -4.5f)
				{
					reachedJumpApex = true;
				}
			}
		}

		public void receiveEventPoke(int data)
		{
			throw new NotImplementedException();
		}

		public string minigameId()
		{
			return "OldMineCart";
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public float GetForcedScaleFactor()
		{
			return Game1.NativeZoomLevel;
		}
	}
}
