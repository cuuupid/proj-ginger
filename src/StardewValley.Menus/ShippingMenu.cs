using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	public class ShippingMenu : IClickableMenu
	{
		public const int region_okbutton = 101;

		public const int region_forwardButton = 102;

		public const int region_backButton = 103;

		public const int farming_category = 0;

		public const int foraging_category = 1;

		public const int fishing_category = 2;

		public const int mining_category = 3;

		public const int other_category = 4;

		public const int total_category = 5;

		public const int timePerIntroCategory = 500;

		public const int outroFadeTime = 800;

		public const int smokeRate = 100;

		public int categorylabelHeight = 25;

		public int itemsPerCategoryPage;

		public int currentPage = -1;

		public int currentTab;

		public List<ClickableTextureComponent> categories = new List<ClickableTextureComponent>();

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent forwardButton;

		public ClickableTextureComponent backButton;

		private List<int> categoryTotals = new List<int>();

		private List<MoneyDial> categoryDials = new List<MoneyDial>();

		private List<List<Item>> categoryItems = new List<List<Item>>();

		private int categoryLabelsWidth;

		private int plusButtonWidth;

		private int itemSlotWidth;

		private int itemAndPlusButtonWidth;

		private int totalWidth;

		private int centerX;

		private int centerY;

		private int introTimer = 3500;

		private int outroFadeTimer;

		private int outroPauseBeforeDateChange;

		private int finalOutroTimer;

		private int smokeTimer;

		private int dayPlaqueY;

		private float weatherX;

		private bool outro;

		private bool newDayPlaque;

		private bool savedYet;

		public List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();

		private float heightMod;

		private SaveGameMenu saveGameMenu;

		public int viewportWidth => Game1.uiViewport.Width;

		public int viewportHeight => Game1.uiViewport.Height;

		public ShippingMenu(IList<Item> items)
			: base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height)
		{
			initialize(0, 0, viewportWidth, viewportHeight);
			parseItems(items);
			if (!Game1.wasRainingYesterday)
			{
				Game1.changeMusicTrack(Game1.currentSeason.Equals("summer") ? "nightTime" : "none");
			}
			categoryLabelsWidth = 512;
			categoryLabelsWidth = 640;
			heightMod = (float)viewportHeight / 720f;
			if (heightMod < 1f)
			{
				categorylabelHeight = Math.Max((int)((float)categorylabelHeight * heightMod), 20);
			}
			plusButtonWidth = 80;
			itemSlotWidth = 96;
			itemsPerCategoryPage = (int)(8f * heightMod);
			itemAndPlusButtonWidth = plusButtonWidth + itemSlotWidth + 8;
			totalWidth = categoryLabelsWidth + itemAndPlusButtonWidth;
			centerX = viewportWidth / 2;
			centerY = viewportHeight / 2;
			int num = -1;
			for (int i = 0; i < 6; i++)
			{
				categories.Add(new ClickableTextureComponent("", new Rectangle(centerX + totalWidth / 2 - plusButtonWidth, centerY - categorylabelHeight * 4 * 3 + i * (categorylabelHeight + 2) * 4 - 20, plusButtonWidth, 88), "", getCategoryName(i), Game1.mobileSpriteSheet, new Rectangle(0, 22, 20, 22), 4f)
				{
					visible = (i < 5 && categoryItems[i].Count > 0),
					myID = i,
					downNeighborID = ((i < 4) ? (i + 1) : 101),
					upNeighborID = ((i > 0) ? num : (-1)),
					upNeighborImmutable = true
				});
				num = ((i < 5 && categoryItems[i].Count > 0) ? i : num);
			}
			dayPlaqueY = categories[0].bounds.Y - 128;
			dayPlaqueY -= 64;
			Rectangle bounds = new Rectangle(centerX + categoryLabelsWidth / 2 + 12, centerY + categorylabelHeight * 4 * 3 - 64 - 12, 80, 80);
			if (heightMod < 1f)
			{
				bounds.Y = categories[5].bounds.Y;
			}
			okButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), bounds, null, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), Game1.mobileSpriteSheet, new Rectangle(0, 0, 20, 20), 4f)
			{
				myID = 101,
				upNeighborID = num
			};
			backButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + Game1.xEdge + 28, yPositionOnScreen + height - 112, 80, 76), null, "", Game1.mobileSpriteSheet, new Rectangle(80, 0, 20, 19), 4f)
			{
				myID = 103,
				rightNeighborID = -7777
			};
			forwardButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width - Game1.xEdge - 28 - 80, yPositionOnScreen + height - 112, 80, 76), null, "", Game1.mobileSpriteSheet, new Rectangle(100, 0, 20, 19), 4f)
			{
				myID = 102,
				leftNeighborID = 103
			};
			if (Game1.dayOfMonth == 25 && Game1.currentSeason.Equals("winter"))
			{
				Vector2 position = new Vector2(viewportWidth, Game1.random.Next(0, 200));
				Rectangle sourceRect = new Rectangle(640, 800, 32, 16);
				int numberOfLoops = 1000;
				TemporaryAnimatedSprite item = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 80f, 2, numberOfLoops, position, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-4f, 0f),
					delayBeforeAnimationStart = 3000
				};
				animations.Add(item);
			}
			Game1.stats.checkForShippingAchievements();
			if (!Game1.player.achievements.Contains(34) && Utility.hasFarmerShippedAllItems())
			{
				Game1.getAchievement(34);
			}
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (oldID == 103 && direction == 1 && showForwardButton())
			{
				currentlySnappedComponent = getComponentWithID(102);
				snapCursorToCurrentSnappedComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(101);
			snapCursorToCurrentSnappedComponent();
		}

		public void parseItems(IList<Item> items)
		{
			Utility.consolidateStacks(items);
			for (int i = 0; i < 6; i++)
			{
				categoryItems.Add(new List<Item>());
				categoryTotals.Add(0);
				categoryDials.Add(new MoneyDial(7, i == 5));
			}
			foreach (Item item in items)
			{
				if (item is Object)
				{
					Object @object = item as Object;
					int categoryIndexForObject = getCategoryIndexForObject(@object);
					categoryItems[categoryIndexForObject].Add(@object);
					categoryTotals[categoryIndexForObject] += @object.sellToStorePrice(-1L) * @object.Stack;
					Game1.stats.itemsShipped += (uint)@object.Stack;
					if (@object.Category == -75 || @object.Category == -79)
					{
						Game1.stats.CropsShipped += (uint)@object.Stack;
					}
					if (@object.countsForShippedCollection())
					{
						Game1.player.shippedBasic(@object.parentSheetIndex, @object.stack);
					}
				}
			}
			for (int j = 0; j < 5; j++)
			{
				categoryTotals[5] += categoryTotals[j];
				categoryItems[5].AddRange(categoryItems[j]);
				categoryDials[j].currentValue = categoryTotals[j];
				categoryDials[j].previousTargetValue = categoryDials[j].currentValue;
			}
			categoryDials[5].currentValue = categoryTotals[5];
			Game1.setRichPresence("earnings", categoryTotals[5]);
		}

		public int getCategoryIndexForObject(Object o)
		{
			switch ((int)o.parentSheetIndex)
			{
			case 296:
			case 396:
			case 402:
			case 406:
			case 410:
			case 414:
			case 418:
				return 1;
			default:
				switch (o.Category)
				{
				case -80:
				case -79:
				case -75:
				case -26:
				case -14:
				case -6:
				case -5:
					return 0;
				case -20:
				case -4:
					return 2;
				case -81:
				case -27:
				case -23:
					return 1;
				case -15:
				case -12:
				case -2:
					return 3;
				default:
					return 4;
				}
			}
		}

		public string getCategoryName(int index)
		{
			return index switch
			{
				0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11389"), 
				1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11390"), 
				2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11391"), 
				3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11392"), 
				4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11393"), 
				5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11394"), 
				_ => "", 
			};
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (saveGameMenu != null)
			{
				saveGameMenu.update(time);
				if (saveGameMenu.quit)
				{
					saveGameMenu = null;
					savedYet = true;
				}
			}
			weatherX += (float)time.ElapsedGameTime.Milliseconds * 0.03f;
			for (int num = animations.Count - 1; num >= 0; num--)
			{
				if (animations[num].update(time))
				{
					animations.RemoveAt(num);
				}
			}
			if (outro)
			{
				if (outroFadeTimer > 0)
				{
					outroFadeTimer -= time.ElapsedGameTime.Milliseconds;
				}
				else if (outroFadeTimer <= 0 && dayPlaqueY < centerY - 64)
				{
					if (animations.Count > 0)
					{
						animations.Clear();
					}
					dayPlaqueY += (int)Math.Ceiling((float)time.ElapsedGameTime.Milliseconds * 0.35f);
					if (dayPlaqueY >= centerY - 64)
					{
						outroPauseBeforeDateChange = 700;
					}
				}
				else if (outroPauseBeforeDateChange > 0)
				{
					outroPauseBeforeDateChange -= time.ElapsedGameTime.Milliseconds;
					if (outroPauseBeforeDateChange <= 0)
					{
						newDayPlaque = true;
						Game1.playSound("newRecipe");
						if (!Game1.currentSeason.Equals("winter"))
						{
							DelayedAction.playSoundAfterDelay(Game1.isRaining ? "rainsound" : "rooster", 1500);
						}
						finalOutroTimer = 2000;
						animations.Clear();
						if (!savedYet)
						{
							if (saveGameMenu == null)
							{
								saveGameMenu = new SaveGameMenu();
							}
							return;
						}
					}
				}
				else if (finalOutroTimer > 0 && savedYet)
				{
					finalOutroTimer -= time.ElapsedGameTime.Milliseconds;
					if (finalOutroTimer <= 0)
					{
						exitThisMenu(playSound: false);
					}
				}
			}
			if (introTimer >= 0)
			{
				int num2 = introTimer;
				introTimer -= time.ElapsedGameTime.Milliseconds * ((Game1.oldMouseState.LeftButton != ButtonState.Pressed) ? 1 : 3);
				if (num2 % 500 < introTimer % 500 && introTimer <= 3000)
				{
					int num3 = 4 - introTimer / 500;
					if (num3 < 6 && num3 > -1)
					{
						if (categoryItems[num3].Count > 0)
						{
							Game1.playSound(getCategorySound(num3));
							categoryDials[num3].currentValue = 0;
							categoryDials[num3].previousTargetValue = 0;
						}
						else
						{
							Game1.playSound("stoneStep");
						}
					}
				}
				if (introTimer < 0)
				{
					Game1.playSound("money");
					categoryDials[5].currentValue = 0;
					categoryDials[5].previousTargetValue = 0;
				}
			}
			else
			{
				if (Game1.dayOfMonth == 28 || outro)
				{
					return;
				}
				if (!Game1.wasRainingYesterday)
				{
					Vector2 position = new Vector2(viewportWidth, Game1.random.Next(200));
					Rectangle sourceRect = new Rectangle(640, 752, 16, 16);
					int num4 = Game1.random.Next(1, 4);
					int num5 = -1;
					if (Game1.random.NextDouble() < 0.001)
					{
						bool flag = Game1.random.NextDouble() < 0.5;
						if (Game1.random.NextDouble() < 0.5)
						{
							animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(640, 826, 16, 8), 40f, 4, 0, new Vector2(Game1.random.Next(centerX * 2), Game1.random.Next(centerY)), flicker: false, flag)
							{
								rotation = (float)Math.PI,
								scale = 4f,
								motion = new Vector2(flag ? (-8) : 8, 8f),
								local = true
							});
						}
						else
						{
							animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(258, 1680, 16, 16), 40f, 4, 0, new Vector2(Game1.random.Next(centerX * 2), Game1.random.Next(centerY)), flicker: false, flag)
							{
								scale = 4f,
								motion = new Vector2(flag ? (-8) : 8, 8f),
								local = true
							});
						}
					}
					else if (Game1.random.NextDouble() < 0.0002)
					{
						TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite(position: new Vector2(viewportWidth, Game1.random.Next(4, 256)), textureName: "", sourceRect: new Rectangle(0, 0, 1, 1), animationInterval: 9999f, animationLength: 1, numberOfLoops: 10000, flicker: false, flipped: false, layerDepth: 0.01f, alphaFade: 0f, color: Color.White * (0.25f + (float)Game1.random.NextDouble()), scale: 4f, scaleChange: 0f, rotation: 0f, rotationChange: 0f, local: true);
						temporaryAnimatedSprite.motion = new Vector2(-0.25f, 0f);
						animations.Add(temporaryAnimatedSprite);
					}
					else if (Game1.random.NextDouble() < 5E-05)
					{
						position = new Vector2(viewportWidth, viewportHeight - 192);
						for (int i = 0; i < num4; i++)
						{
							TemporaryAnimatedSprite temporaryAnimatedSprite2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(60, 101), 4, 100, position + new Vector2((i + 1) * Game1.random.Next(15, 18), (i + 1) * -20), flicker: false, flipped: false, 0.01f, 0f, Color.Black, 4f, 0f, 0f, 0f, local: true);
							temporaryAnimatedSprite2.motion = new Vector2(-1f, 0f);
							animations.Add(temporaryAnimatedSprite2);
							temporaryAnimatedSprite2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(60, 101), 4, 100, position + new Vector2((i + 1) * Game1.random.Next(15, 18), (i + 1) * 20), flicker: false, flipped: false, 0.01f, 0f, Color.Black, 4f, 0f, 0f, 0f, local: true);
							temporaryAnimatedSprite2.motion = new Vector2(-1f, 0f);
							animations.Add(temporaryAnimatedSprite2);
						}
					}
					else if (Game1.random.NextDouble() < 1E-05)
					{
						sourceRect = new Rectangle(640, 784, 16, 16);
						TemporaryAnimatedSprite temporaryAnimatedSprite3 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 75f, 4, 1000, position, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true);
						temporaryAnimatedSprite3.motion = new Vector2(-3f, 0f);
						temporaryAnimatedSprite3.yPeriodic = true;
						temporaryAnimatedSprite3.yPeriodicLoopTime = 1000f;
						temporaryAnimatedSprite3.yPeriodicRange = 8f;
						temporaryAnimatedSprite3.shakeIntensity = 0.5f;
						animations.Add(temporaryAnimatedSprite3);
					}
				}
				smokeTimer -= time.ElapsedGameTime.Milliseconds;
				if (smokeTimer <= 0)
				{
					smokeTimer = 50;
					animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(684, 1075, 1, 1), 1000f, 1, 1000, new Vector2(188f, viewportHeight - 128 + 20), flicker: false, flipped: false)
					{
						color = (Game1.wasRainingYesterday ? Color.SlateGray : Color.White),
						scale = 4f,
						scaleChange = 0f,
						alphaFade = 0.0025f,
						motion = new Vector2(0f, (float)(-Game1.random.Next(25, 75)) / 100f / 4f),
						acceleration = new Vector2(-0.001f, 0f)
					});
				}
			}
		}

		public string getCategorySound(int which)
		{
			switch (which)
			{
			case 0:
				if (!(categoryItems[0][0] as Object).isAnimalProduct())
				{
					return "harvest";
				}
				return "cluck";
			case 2:
				return "button1";
			case 3:
				return "hammer";
			case 1:
				return "leafrustle";
			case 4:
				return "coin";
			case 5:
				return "money";
			default:
				return "stoneStep";
			}
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void receiveKeyPress(Keys key)
		{
			if (introTimer <= 0 && !Game1.options.gamepadControls && (key.Equals(Keys.Escape) || Game1.options.doesInputListContain(Game1.options.menuButton, key)))
			{
				receiveLeftClick(okButton.bounds.Center.X, okButton.bounds.Center.Y);
			}
			else if (introTimer <= 0 && (!Game1.options.gamepadControls || !Game1.options.doesInputListContain(Game1.options.menuButton, key)))
			{
				base.receiveKeyPress(key);
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (!outro)
			{
				if (introTimer <= 0)
				{
					okClicked();
					return;
				}
				introTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds * 2;
				Game1.playSound("smallSelect");
			}
		}

		private void okClicked()
		{
			outro = true;
			outroFadeTimer = 800;
			Game1.playSound("bigDeSelect");
			Game1.changeMusicTrack("none");
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (outro && !savedYet)
			{
				_ = saveGameMenu;
			}
			else
			{
				if (savedYet)
				{
					return;
				}
				base.receiveLeftClick(x, y, playSound);
				if (currentPage == -1 && introTimer <= 0 && okButton.containsPoint(x, y))
				{
					okClicked();
				}
				if (currentPage == -1)
				{
					for (int i = 0; i < categories.Count; i++)
					{
						if (categories[i].visible && categories[i].containsPoint(x, y))
						{
							currentPage = i;
							Game1.playSound("shwip");
							if (Game1.options.SnappyMenus)
							{
								currentlySnappedComponent = getComponentWithID(103);
								snapCursorToCurrentSnappedComponent();
							}
							break;
						}
					}
				}
				else if (backButton.containsPoint(x, y))
				{
					if (currentTab == 0)
					{
						if (Game1.options.SnappyMenus)
						{
							currentlySnappedComponent = getComponentWithID(currentPage);
							snapCursorToCurrentSnappedComponent();
						}
						currentPage = -1;
					}
					else
					{
						currentTab--;
					}
					Game1.playSound("shwip");
				}
				else if (showForwardButton() && forwardButton.containsPoint(x, y))
				{
					currentTab++;
					Game1.playSound("shwip");
				}
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public bool showForwardButton()
		{
			return categoryItems[currentPage].Count > itemsPerCategoryPage * (currentTab + 1);
		}

		public override void draw(SpriteBatch b)
		{
			if (Game1.wasRainingYesterday)
			{
				b.Draw(Game1.mobileSpriteSheet, new Rectangle(0, 0, viewportWidth, viewportHeight), new Rectangle(228, 0, 4, 144), Game1.currentSeason.Equals("winter") ? Color.LightSlateGray : (Color.SlateGray * (1f - (float)introTimer / 3500f)));
				b.Draw(Game1.mobileSpriteSheet, new Rectangle(2556, 0, viewportWidth, viewportHeight), new Rectangle(228, 0, 4, 144), Game1.currentSeason.Equals("winter") ? Color.LightSlateGray : (Color.SlateGray * (1f - (float)introTimer / 3500f)));
				for (int i = -244; i < viewportWidth + 244; i += 244)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)i + weatherX / 2f % 244f, 32f), new Rectangle(643, 1142, 61, 53), Color.DarkSlateGray * 1f * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
				}
				b.Draw(Game1.mouseCursors, new Vector2(0f, viewportHeight - 192), new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 48), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.25f) : new Color(30, 62, 50)) * (0.5f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.08f);
				b.Draw(Game1.mouseCursors, new Vector2(2556f, viewportHeight - 192), new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 48), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.25f) : new Color(30, 62, 50)) * (0.5f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.08f);
				b.Draw(Game1.mouseCursors, new Vector2(0f, viewportHeight - 128), new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 32), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.5f) : new Color(30, 62, 50)) * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
				b.Draw(Game1.mouseCursors, new Vector2(2556f, viewportHeight - 128), new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 32), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.5f) : new Color(30, 62, 50)) * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
				b.Draw(Game1.mouseCursors, new Vector2(160f, viewportHeight - 128 + 16 + 8), new Rectangle(653, 880, 10, 10), Color.White * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
				for (int j = -244; j < viewportWidth + 244; j += 244)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)j + weatherX % 244f, -32f), new Rectangle(643, 1142, 61, 53), Color.SlateGray * 0.85f * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.081f);
				}
				foreach (TemporaryAnimatedSprite animation in animations)
				{
					animation.draw(b, localPosition: true);
				}
				for (int k = -244; k < viewportWidth + 244; k += 244)
				{
					b.Draw(Game1.mouseCursors, new Vector2((float)k + weatherX * 1.5f % 244f, -128f), new Rectangle(643, 1142, 61, 53), Color.LightSlateGray * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.081f);
				}
			}
			else
			{
				b.Draw(Game1.mobileSpriteSheet, new Rectangle(0, 0, viewportWidth, viewportHeight), new Rectangle(228, 0, 4, 144), Color.White * (1f - (float)introTimer / 3500f));
				b.Draw(Game1.mobileSpriteSheet, new Rectangle(2556, 0, viewportWidth, viewportHeight), new Rectangle(228, 0, 4, 144), Color.White * (1f - (float)introTimer / 3500f));
				b.Draw(Game1.mouseCursors, new Vector2(0f, 0f), new Rectangle(0, 1453, 639, 195), Color.White * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
				b.Draw(Game1.mouseCursors, new Vector2(2556f, 0f), new Rectangle(0, 1453, 639, 195), Color.White * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
				if (Game1.dayOfMonth == 28)
				{
					b.Draw(Game1.mouseCursors, new Vector2(viewportWidth - 176, 4f), new Rectangle(642, 835, 43, 43), Color.White * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
				}
				b.Draw(Game1.mouseCursors, new Vector2(0f, viewportHeight - 192), new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 48), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.25f) : new Color(0, 20, 40)) * (0.65f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.08f);
				b.Draw(Game1.mouseCursors, new Vector2(2556f, viewportHeight - 192), new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 48), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.25f) : new Color(0, 20, 40)) * (0.65f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 0.08f);
				b.Draw(Game1.mouseCursors, new Vector2(0f, viewportHeight - 128), new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 32), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.5f) : new Color(0, 32, 20)) * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
				b.Draw(Game1.mouseCursors, new Vector2(2556f, viewportHeight - 128), new Rectangle(0, Game1.currentSeason.Equals("winter") ? 1034 : 737, 639, 32), (Game1.currentSeason.Equals("winter") ? (Color.White * 0.5f) : new Color(0, 32, 20)) * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
				b.Draw(Game1.mouseCursors, new Vector2(160f, viewportHeight - 128 + 16 + 8), new Rectangle(653, 880, 10, 10), Color.White * (1f - (float)introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.08f);
			}
			if (!outro && !Game1.wasRainingYesterday)
			{
				foreach (TemporaryAnimatedSprite animation2 in animations)
				{
					animation2.draw(b, localPosition: true);
				}
			}
			if (currentPage == -1)
			{
				int num = 0;
				int num2 = (categorylabelHeight * 4 - SpriteText.getHeightOfString("A")) / 2;
				int num3 = 0;
				foreach (ClickableTextureComponent category in categories)
				{
					if (introTimer < 2500 - num3 * 500)
					{
						Vector2 vector = category.getVector2() + new Vector2(12f, 0f);
						if (category.visible)
						{
							category.draw(b);
							b.Draw(Game1.mouseCursors, vector + new Vector2(-104f, num), new Rectangle(293, 360, 24, 24), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.088f);
							categoryItems[num3][0].drawInMenu(b, vector + new Vector2(-88f, num + 16), 1f, 1f, 0.09f, StackDrawType.Hide);
						}
						IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), (int)(vector.X + (float)(-itemSlotWidth) - (float)categoryLabelsWidth - 12f), (int)(vector.Y + (float)num), categoryLabelsWidth, categorylabelHeight * 4, Color.White, 4f, drawShadow: false);
						SpriteText.drawString(b, category.hoverText, (int)vector.X - itemSlotWidth - categoryLabelsWidth + 24, (int)vector.Y + 4 + num2);
						for (int l = 0; l < 6; l++)
						{
							b.Draw(Game1.mouseCursors, vector + new Vector2(-itemSlotWidth - 192 - 24 + l * 6 * 4, num2 + 4), new Rectangle(355, 476, 7, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.088f);
						}
						categoryDials[num3].draw(b, vector + new Vector2(-itemSlotWidth - 192 - 48 + 4, num2 + 12), categoryTotals[num3]);
						b.Draw(Game1.mouseCursors, vector + new Vector2(-itemSlotWidth - 64 - 4, num2 + 4), new Rectangle(408, 476, 9, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.088f);
					}
					num3++;
				}
				if (introTimer <= 0)
				{
					okButton.draw(b);
				}
			}
			else
			{
				IClickableMenu.drawTextureBox(b, Game1.xEdge, 0, viewportWidth - Game1.xEdge * 2, viewportHeight, Color.White);
				Vector2 location = new Vector2(xPositionOnScreen + Game1.xEdge + 32, yPositionOnScreen + 32);
				for (int m = currentTab * itemsPerCategoryPage; m < currentTab * itemsPerCategoryPage + itemsPerCategoryPage; m++)
				{
					if (categoryItems[currentPage].Count <= m)
					{
						continue;
					}
					categoryItems[currentPage][m].drawInMenu(b, location, 1f, 1f, 0.085f, StackDrawType.Draw);
					if (LocalizedContentManager.CurrentLanguageLatin)
					{
						string text = categoryItems[currentPage][m].DisplayName + ((categoryItems[currentPage][m].Stack > 1) ? (" x" + categoryItems[currentPage][m].Stack) : "");
						SpriteText.drawString(b, text, (int)location.X + 64 + 12, (int)location.Y + 12);
						string text2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", (categoryItems[currentPage][m] as Object).sellToStorePrice(-1L) * (categoryItems[currentPage][m] as Object).Stack);
						string text3 = ".";
						int num4 = width - Game1.xEdge * 2 - 96 - SpriteText.getWidthOfString(text + text2);
						int widthOfString = SpriteText.getWidthOfString(" .");
						for (int n = 0; n < num4; n += widthOfString)
						{
							text3 += " .";
						}
						int x = (int)location.X + 80 + SpriteText.getWidthOfString(text);
						SpriteText.drawString(b, text3, x, (int)location.Y + 8);
						x = (int)location.X + width - Game1.xEdge * 2 - 64 - SpriteText.getWidthOfString(text2);
						SpriteText.drawString(b, text2, x, (int)location.Y + 12);
					}
					else
					{
						string text4 = categoryItems[currentPage][m].DisplayName + ((categoryItems[currentPage][m].Stack > 1) ? (" x" + categoryItems[currentPage][m].Stack) : ".");
						string text5 = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", (categoryItems[currentPage][m] as Object).sellToStorePrice(-1L) * (categoryItems[currentPage][m] as Object).Stack);
						int x2 = (int)location.X + width - Game1.xEdge * 2 - 64 - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", (categoryItems[currentPage][m] as Object).sellToStorePrice(-1L) * (categoryItems[currentPage][m] as Object).Stack));
						int widthOfString2 = SpriteText.getWidthOfString(text4 + text5);
						while (SpriteText.getWidthOfString(text4 + text5) < width - Game1.xEdge * 2 - 128)
						{
							text4 += " .";
						}
						if (SpriteText.getWidthOfString(text4 + text5) >= width - Game1.xEdge * 2 - 128)
						{
							text4 = text4.Remove(text4.Length - 1);
						}
						SpriteText.drawString(b, text4, (int)location.X + 64 + 12, (int)location.Y + 12);
						SpriteText.drawString(b, text5, x2, (int)location.Y + 12);
					}
					location.Y += 68f;
				}
				backButton.draw(b);
				if (showForwardButton())
				{
					forwardButton.draw(b);
				}
			}
			if (outro)
			{
				b.Draw(Game1.mobileSpriteSheet, new Rectangle(0, 0, viewportWidth, viewportHeight), new Rectangle(228, 0, 4, 144), Color.Black * (1f - (float)outroFadeTimer / 800f));
				SpriteText.drawStringWithScrollCenteredAt(b, newDayPlaque ? Utility.getDateString() : Utility.getYesterdaysDate(), viewportWidth / 2, dayPlaqueY);
				foreach (TemporaryAnimatedSprite animation3 in animations)
				{
					animation3.draw(b, localPosition: true);
				}
				if (finalOutroTimer > 0)
				{
					b.Draw(Game1.staminaRect, new Rectangle(0, 0, viewportWidth, viewportHeight), new Rectangle(0, 0, 1, 1), Color.Black * (1f - (float)finalOutroTimer / 2000f));
				}
			}
			if (saveGameMenu != null)
			{
				saveGameMenu.draw(b);
			}
			if (!Game1.options.SnappyMenus || (introTimer <= 0 && !outro))
			{
				drawMouse(b);
			}
		}
	}
}
