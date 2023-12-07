using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	public class ChooseFromListMenu : IClickableMenu
	{
		public delegate void actionOnChoosingListOption(string s);

		public const int region_backButton = 101;

		public const int region_forwardButton = 102;

		public const int region_okButton = 103;

		public const int region_cancelButton = 104;

		public const int w = 640;

		public const int h = 192;

		public ClickableTextureComponent backButton;

		public ClickableTextureComponent forwardButton;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent cancelButton;

		private List<string> options = new List<string>();

		private int index;

		private actionOnChoosingListOption chooseAction;

		private bool isJukebox;

		private Rectangle nameBox;

		public ChooseFromListMenu(List<string> options, actionOnChoosingListOption chooseAction, bool isJukebox = false, string default_selection = null)
			: base(Game1.xEdge, 0, Game1.uiViewport.Width - Game1.xEdge / 2, Game1.uiViewport.Height, showUpperRightCloseButton: true)
		{
			this.chooseAction = chooseAction;
			float num = (float)width / 1280f;
			float num2 = (float)Game1.uiViewport.Height / 720f;
			backButton = new ClickableTextureComponent(new Rectangle(0, 0, 80, 76), Game1.mobileSpriteSheet, new Rectangle(80, 0, 20, 19), 4f, drawShadow: true);
			forwardButton = new ClickableTextureComponent(new Rectangle(0, 0, 80, 76), Game1.mobileSpriteSheet, new Rectangle(100, 0, 20, 19), 4f, drawShadow: true);
			nameBox = default(Rectangle);
			initializeUpperRightCloseButton();
			Game1.playSound("bigSelect");
			this.isJukebox = isJukebox;
			if (isJukebox)
			{
				FilterJukeboxTracks(options);
			}
			this.options = options;
			if (default_selection != null)
			{
				int num3 = options.IndexOf(default_selection);
				if (num3 >= 0)
				{
					index = num3;
				}
			}
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public static void FilterJukeboxTracks(List<string> options)
		{
			for (int num = options.Count - 1; num >= 0; num--)
			{
				if (!IsValidJukeboxSong(options[num]))
				{
					options.RemoveAt(num);
				}
				else
				{
					string text = options[num];
					if (text != null)
					{
						switch (text.Length)
						{
						case 9:
							switch (text[0])
							{
							case 'n':
								if (text == "nightTime")
								{
									options.RemoveAt(num);
								}
								break;
							case 't':
								if (text == "title_day")
								{
									options.RemoveAt(num);
									options.Add("MainTheme");
								}
								break;
							}
							break;
						case 5:
							if (text == "ocean")
							{
								options.RemoveAt(num);
							}
							break;
						case 15:
							if (text == "communityCenter")
							{
								options.RemoveAt(num);
							}
							break;
						case 4:
							if (text == "coin")
							{
								options.RemoveAt(num);
							}
							break;
						case 12:
							if (text == "buglevelloop")
							{
								options.RemoveAt(num);
							}
							break;
						case 20:
							if (text == "jojaOfficeSoundscape")
							{
								options.RemoveAt(num);
							}
							break;
						}
					}
				}
			}
		}

		public static bool IsValidJukeboxSong(string name)
		{
			name = name.ToLower();
			if (name.Trim() == "")
			{
				return false;
			}
			if (name.Contains("ambient") || name.Contains("bigdrums") || name.Contains("clubloop") || name.Contains("ambience"))
			{
				return false;
			}
			return true;
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(103);
			snapCursorToCurrentSnappedComponent();
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			UpdatePositions();
		}

		private void UpdatePositions()
		{
			width = Game1.uiViewport.Width;
			height = Game1.uiViewport.Height;
			xPositionOnScreen = (width - 640) / 2;
			yPositionOnScreen = (height - 192) / 2;
			backButton.bounds.Y = yPositionOnScreen + 85;
			forwardButton.bounds.Y = yPositionOnScreen + 85;
			upperRightCloseButton.bounds = new Rectangle(width - 68 - Game1.xEdge, 0, 68 + Game1.xEdge, 80);
		}

		public static void playSongAction(string s)
		{
			Game1.startedJukeboxMusic = true;
			Game1.changeMusicTrack(s);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			backButton.tryHover(x, y);
			forwardButton.tryHover(x, y);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			base.receiveLeftClick(x, y, playSound);
			if (backButton.containsPoint(x, y))
			{
				index--;
				if (index < 0)
				{
					index = options.Count - 1;
				}
				backButton.scale = backButton.baseScale + 0.2f;
				Game1.playSound("shwip");
				if (chooseAction != null)
				{
					chooseAction(options[index]);
				}
			}
			if (forwardButton.containsPoint(x, y))
			{
				index++;
				index %= options.Count;
				Game1.playSound("shwip");
				if (chooseAction != null)
				{
					chooseAction(options[index]);
				}
				forwardButton.scale = forwardButton.baseScale + 0.2f;
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			UpdatePositions();
			string text = "Summer (The Sun Can Bend An Orange Sky)";
			int num = (int)Game1.dialogueFont.MeasureString(isJukebox ? text : options[index]).X + 64;
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.8f);
			upperRightCloseButton.draw(b);
			IClickableMenu.drawTextureBox(b, width / 2 - num / 2, backButton.bounds.Y - 4, num, 80, Color.White);
			Vector2 vector = Game1.dialogueFont.MeasureString(isJukebox ? Utility.getSongTitleFromCueName(options[index]) : options[index]);
			if (index < options.Count)
			{
				Utility.drawTextWithShadow(b, isJukebox ? Utility.getSongTitleFromCueName(options[index]) : options[index], Game1.dialogueFont, new Vector2((float)(width / 2) - vector.X / 2f, (float)backButton.bounds.Y + (80f - vector.Y) / 2f), Game1.textColor);
			}
			backButton.bounds.X = width / 2 - num / 2 - backButton.bounds.Width - 12;
			forwardButton.bounds.X = width / 2 + num / 2 + 12;
			forwardButton.draw(b);
			backButton.draw(b);
			if (isJukebox)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:JukeboxMenu_Title"), width / 2, backButton.bounds.Y - 128);
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			switch (b)
			{
			case Buttons.DPadLeft:
			case Buttons.LeftThumbstickLeft:
				index--;
				if (index < 0)
				{
					index = options.Count - 1;
				}
				backButton.scale = backButton.baseScale + 0.2f;
				Game1.playSound("shwip");
				if (chooseAction != null)
				{
					chooseAction(options[index]);
				}
				break;
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickRight:
				index++;
				index %= options.Count;
				Game1.playSound("shwip");
				if (chooseAction != null)
				{
					chooseAction(options[index]);
				}
				forwardButton.scale = forwardButton.baseScale + 0.2f;
				break;
			case Buttons.A:
				Game1.playSound("smallSelect");
				exitThisMenu();
				break;
			}
		}
	}
}
