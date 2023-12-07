using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus
{
	public class LanguageSelectionMenu : IClickableMenu
	{
		public int buttonWidth = 500;

		public new int height;

		public const int numLanguages = 12;

		private MobileScrollbar newScrollbar;

		private MobileScrollbox scrollArea;

		private Rectangle mainBox;

		private int menuWidth = 24;

		private int scrollBarWidth = 24;

		private int buttonHeight;

		private string languageCodeName;

		private Texture2D texture;

		public List<ClickableComponent> languages = new List<ClickableComponent>();

		private int currentItemIndex;

		public LanguageSelectionMenu()
			: base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height, showUpperRightCloseButton: true)
		{
			texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\LanguageButtons");
			SetupButtons();
		}

		private void SetupButtons()
		{
			languages.Clear();
			buttonHeight = 83;
			height = Math.Min(Game1.uiViewport.Height, 12 * buttonHeight + 32);
			int num = buttonWidth + 32;
			initializeUpperRightCloseButton();
			if (12 * buttonHeight >= Game1.uiViewport.Height - 32)
			{
				num += scrollBarWidth;
				mainBox = new Rectangle((Game1.uiViewport.Width - num) / 2, (Game1.uiViewport.Height - height) / 2, num, height);
				newScrollbar = new MobileScrollbar(mainBox.X + buttonWidth, mainBox.Y + 16, 24, height - 36, 0, 32);
				scrollArea = new MobileScrollbox(mainBox.X, mainBox.Y, buttonWidth, height, (int)((float)buttonHeight * (12f - (float)(height - 32) / (float)buttonHeight)), new Rectangle(mainBox.X + 16, mainBox.Y + 16, buttonWidth, height - 32), newScrollbar);
			}
			else
			{
				mainBox = new Rectangle((Game1.uiViewport.Width - num) / 2, (Game1.uiViewport.Height - height) / 2, num, height);
			}
			languages.Add(new ClickableComponent(new Rectangle(mainBox.X + 16, mainBox.Y + 16, buttonWidth, buttonHeight), "English", null));
			languages.Add(new ClickableComponent(new Rectangle(mainBox.X + 16, mainBox.Y + 16 + buttonHeight, buttonWidth, buttonHeight), "German", null));
			languages.Add(new ClickableComponent(new Rectangle(mainBox.X + 16, mainBox.Y + 16 + buttonHeight * 2, buttonWidth, buttonHeight), "Russian", null));
			languages.Add(new ClickableComponent(new Rectangle(mainBox.X + 16, mainBox.Y + 16 + buttonHeight * 3, buttonWidth, buttonHeight), "Spanish", null));
			languages.Add(new ClickableComponent(new Rectangle(mainBox.X + 16, mainBox.Y + 16 + buttonHeight * 5, buttonWidth, buttonHeight), "Chinese", null));
			languages.Add(new ClickableComponent(new Rectangle(mainBox.X + 16, mainBox.Y + 16 + buttonHeight * 4, buttonWidth, buttonHeight), "Portuguese", null));
			languages.Add(new ClickableComponent(new Rectangle(mainBox.X + 16, mainBox.Y + 16 + buttonHeight * 6, buttonWidth, buttonHeight), "Japanese", null));
			languages.Add(new ClickableComponent(new Rectangle(mainBox.X + 16, mainBox.Y + 16 + buttonHeight * 7, buttonWidth, buttonHeight), "Korean", null));
			languages.Add(new ClickableComponent(new Rectangle(mainBox.X + 16, mainBox.Y + 16 + buttonHeight * 8, buttonWidth, buttonHeight), "French", null));
			languages.Add(new ClickableComponent(new Rectangle(mainBox.X + 16, mainBox.Y + 16 + buttonHeight * 9, buttonWidth, buttonHeight), "Italian", null));
			languages.Add(new ClickableComponent(new Rectangle(mainBox.X + 16, mainBox.Y + 16 + buttonHeight * 10, buttonWidth, buttonHeight), "Turkish", null));
			languages.Add(new ClickableComponent(new Rectangle(mainBox.X + 16, mainBox.Y + 16 + buttonHeight * 11, buttonWidth, buttonHeight), "Hungarian", null));
			if (Game1.options.SnappyMenus)
			{
				int id = ((currentlySnappedComponent != null) ? currentlySnappedComponent.myID : 0);
				populateClickableComponentList();
				currentlySnappedComponent = getComponentWithID(id);
				snapCursorToCurrentSnappedComponent();
			}
			setCurrentItemIndex();
		}

		private void setCurrentItemIndex()
		{
			currentItemIndex = 0;
			switch (LocalizedContentManager.CurrentLanguageCode)
			{
			case LocalizedContentManager.LanguageCode.en:
				languageCodeName = "English";
				break;
			case LocalizedContentManager.LanguageCode.de:
				languageCodeName = "German";
				break;
			case LocalizedContentManager.LanguageCode.ru:
				languageCodeName = "Russian";
				break;
			case LocalizedContentManager.LanguageCode.zh:
				languageCodeName = "Chinese";
				break;
			case LocalizedContentManager.LanguageCode.ja:
				languageCodeName = "Japanese";
				break;
			case LocalizedContentManager.LanguageCode.es:
				languageCodeName = "Spanish";
				break;
			case LocalizedContentManager.LanguageCode.pt:
				languageCodeName = "Portuguese";
				break;
			case LocalizedContentManager.LanguageCode.fr:
				languageCodeName = "French";
				break;
			case LocalizedContentManager.LanguageCode.ko:
				languageCodeName = "Korean";
				break;
			case LocalizedContentManager.LanguageCode.tr:
				languageCodeName = "Turkish";
				break;
			case LocalizedContentManager.LanguageCode.hu:
				languageCodeName = "Hungarian";
				break;
			}
			for (int i = 0; i < languages.Count; i++)
			{
				if (languages[i].name == languageCodeName)
				{
					currentItemIndex = i;
					highlightLanguage(currentItemIndex);
					break;
				}
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.B && upperRightCloseButton != null)
			{
				Game1.playSound("bigDeSelect");
				if (TitleMenu.subMenu != null && Game1.activeClickableMenu != null)
				{
					TitleMenu.subMenu = null;
				}
			}
			else
			{
				string name;
				char c;
				switch (b)
				{
				case Buttons.DPadUp:
				case Buttons.LeftThumbstickUp:
					currentItemIndex--;
					if (currentItemIndex < 0)
					{
						currentItemIndex = 0;
						break;
					}
					if (scrollArea != null)
					{
						int yOffsetForScroll2 = scrollArea.getYOffsetForScroll();
						if (yOffsetForScroll2 < -buttonHeight * currentItemIndex)
						{
							scrollArea.setYOffsetForScroll(yOffsetForScroll2 + buttonHeight);
						}
					}
					Game1.playSound("shwip");
					break;
				case Buttons.DPadDown:
				case Buttons.LeftThumbstickDown:
					currentItemIndex++;
					if (currentItemIndex >= languages.Count)
					{
						currentItemIndex = languages.Count - 1;
						break;
					}
					if (scrollArea != null)
					{
						int yOffsetForScroll = scrollArea.getYOffsetForScroll();
						if (height - yOffsetForScroll < buttonHeight * (currentItemIndex + 1))
						{
							scrollArea.setYOffsetForScroll(yOffsetForScroll - buttonHeight);
						}
					}
					Game1.playSound("shwip");
					break;
				case Buttons.A:
					{
						Game1.playSound("select");
						name = languages[currentItemIndex].name;
						if (name != null)
						{
							switch (name.Length)
							{
							case 7:
								break;
							case 6:
								goto IL_01e7;
							case 8:
								goto IL_02c4;
							case 10:
								goto IL_02d6;
							case 9:
								goto IL_02e5;
							default:
								goto IL_0358;
							}
							switch (name[0])
							{
							case 'E':
								break;
							case 'R':
								goto IL_0225;
							case 'C':
								goto IL_023a;
							case 'S':
								goto IL_024f;
							case 'T':
								goto IL_0264;
							case 'I':
								goto IL_0279;
							default:
								goto IL_0358;
							}
							if (name == "English")
							{
								LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.en;
								goto IL_035e;
							}
						}
						goto IL_0358;
					}
					IL_02e5:
					if (!(name == "Hungarian"))
					{
						goto IL_0358;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.hu;
					goto IL_035e;
					IL_0264:
					if (!(name == "Turkish"))
					{
						goto IL_0358;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.tr;
					goto IL_035e;
					IL_02d6:
					if (!(name == "Portuguese"))
					{
						goto IL_0358;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.pt;
					goto IL_035e;
					IL_023a:
					if (!(name == "Chinese"))
					{
						goto IL_0358;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.zh;
					goto IL_035e;
					IL_02c4:
					if (!(name == "Japanese"))
					{
						goto IL_0358;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.ja;
					goto IL_035e;
					IL_0279:
					if (!(name == "Italian"))
					{
						goto IL_0358;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.it;
					goto IL_035e;
					IL_01e7:
					c = name[0];
					if (c != 'F')
					{
						if (c != 'G')
						{
							if (c != 'K' || !(name == "Korean"))
							{
								goto IL_0358;
							}
							LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.ko;
						}
						else
						{
							if (!(name == "German"))
							{
								goto IL_0358;
							}
							LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.de;
						}
					}
					else
					{
						if (!(name == "French"))
						{
							goto IL_0358;
						}
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.fr;
					}
					goto IL_035e;
					IL_0225:
					if (!(name == "Russian"))
					{
						goto IL_0358;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.ru;
					goto IL_035e;
					IL_0358:
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.en;
					goto IL_035e;
					IL_035e:
					exitThisMenu();
					break;
					IL_024f:
					if (!(name == "Spanish"))
					{
						goto IL_0358;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.es;
					goto IL_035e;
				}
			}
			highlightLanguage(currentItemIndex);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (upperRightCloseButton != null && upperRightCloseButton.containsPoint(x, y))
			{
				exitThisMenu();
			}
			if (scrollArea != null)
			{
				if (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y))
				{
					float num = newScrollbar.setY(y);
					scrollArea.setYOffsetForScroll(-(int)(num * (float)scrollArea.getMaxYOffset() / 100f));
					updateSlots();
					Game1.playSound("shwip");
				}
				else
				{
					scrollArea.receiveLeftClick(x, y);
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			if (scrollArea == null || !scrollArea.havePanelScrolled)
			{
				foreach (ClickableComponent language in languages)
				{
					if (!language.containsPoint(x, y))
					{
						continue;
					}
					Game1.playSound("select");
					string name = language.name;
					if (name != null)
					{
						switch (name.Length)
						{
						case 7:
							break;
						case 6:
							goto IL_00cc;
						case 8:
							goto IL_01a9;
						case 10:
							goto IL_01bb;
						case 9:
							goto IL_01ca;
						default:
							goto IL_023d;
						}
						switch (name[0])
						{
						case 'E':
							break;
						case 'R':
							goto IL_010a;
						case 'C':
							goto IL_011f;
						case 'S':
							goto IL_0134;
						case 'T':
							goto IL_0149;
						case 'I':
							goto IL_015e;
						default:
							goto IL_023d;
						}
						if (name == "English")
						{
							LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.en;
							goto IL_0243;
						}
					}
					goto IL_023d;
					IL_010a:
					if (!(name == "Russian"))
					{
						goto IL_023d;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.ru;
					goto IL_0243;
					IL_023d:
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.en;
					goto IL_0243;
					IL_0243:
					exitThisMenu();
					continue;
					IL_01ca:
					if (!(name == "Hungarian"))
					{
						goto IL_023d;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.hu;
					goto IL_0243;
					IL_0149:
					if (!(name == "Turkish"))
					{
						goto IL_023d;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.tr;
					goto IL_0243;
					IL_01bb:
					if (!(name == "Portuguese"))
					{
						goto IL_023d;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.pt;
					goto IL_0243;
					IL_0134:
					if (!(name == "Spanish"))
					{
						goto IL_023d;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.es;
					goto IL_0243;
					IL_01a9:
					if (!(name == "Japanese"))
					{
						goto IL_023d;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.ja;
					goto IL_0243;
					IL_015e:
					if (!(name == "Italian"))
					{
						goto IL_023d;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.it;
					goto IL_0243;
					IL_00cc:
					char c = name[0];
					if (c != 'F')
					{
						if (c != 'G')
						{
							if (c != 'K' || !(name == "Korean"))
							{
								goto IL_023d;
							}
							LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.ko;
						}
						else
						{
							if (!(name == "German"))
							{
								goto IL_023d;
							}
							LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.de;
						}
					}
					else
					{
						if (!(name == "French"))
						{
							goto IL_023d;
						}
						LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.fr;
					}
					goto IL_0243;
					IL_011f:
					if (!(name == "Chinese"))
					{
						goto IL_023d;
					}
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.zh;
					goto IL_0243;
				}
			}
			if (scrollArea != null)
			{
				scrollArea.releaseLeftClick(x, y);
			}
		}

		public void highlightLanguage(int index)
		{
			if (index >= languages.Count)
			{
				return;
			}
			for (int i = 0; i < languages.Count; i++)
			{
				if (i != index)
				{
					languages[i].label = null;
				}
			}
			languages[index].label = "hovered";
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (scrollArea != null)
			{
				if (newScrollbar.sliderContains(x, y) || newScrollbar.sliderRunnerContains(x, y))
				{
					float num = newScrollbar.setY(y);
					scrollArea.setYOffsetForScroll(-(int)(num * (float)scrollArea.getMaxYOffset() / 100f));
					updateSlots();
				}
				else
				{
					scrollArea.leftClickHeld(x, y);
				}
			}
			foreach (ClickableComponent language in languages)
			{
				if (language.containsPoint(x, y))
				{
					if (language.label == null)
					{
						Game1.playSound("Cowboy_Footstep");
						language.label = "hovered";
					}
				}
				else
				{
					language.label = null;
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if (scrollArea != null)
			{
				scrollArea.receiveScrollWheelAction(direction);
			}
		}

		public void updateSlots()
		{
			if (scrollArea != null)
			{
				int num = scrollArea.getYOffsetForScroll() + mainBox.Y + 16;
				for (int i = 0; i < languages.Count; i++)
				{
					languages[i].bounds.Y = num + buttonHeight * i;
				}
			}
		}

		public override void update(GameTime time)
		{
			if (scrollArea != null)
			{
				scrollArea.update(time);
				updateSlots();
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void draw(SpriteBatch b)
		{
			Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(width, height - 100);
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
			IClickableMenu.drawTextureBox(b, mainBox.X, mainBox.Y, mainBox.Width, mainBox.Height, Color.White);
			if (scrollArea != null)
			{
				newScrollbar.draw(b);
				scrollArea.setUpForScrollBoxDrawing(b);
			}
			foreach (ClickableComponent language in languages)
			{
				int num = 0;
				string name = language.name;
				if (name != null)
				{
					switch (name.Length)
					{
					case 7:
						switch (name[0])
						{
						case 'E':
							if (name == "English")
							{
								num = 0;
							}
							break;
						case 'S':
							if (name == "Spanish")
							{
								num = 1;
							}
							break;
						case 'R':
							if (name == "Russian")
							{
								num = 3;
							}
							break;
						case 'C':
							if (name == "Chinese")
							{
								num = 4;
							}
							break;
						case 'I':
							if (name == "Italian")
							{
								num = 10;
							}
							break;
						case 'T':
							if (name == "Turkish")
							{
								num = 9;
							}
							break;
						}
						break;
					case 6:
						switch (name[0])
						{
						case 'G':
							if (name == "German")
							{
								num = 6;
							}
							break;
						case 'F':
							if (name == "French")
							{
								num = 7;
							}
							break;
						case 'K':
							if (name == "Korean")
							{
								num = 8;
							}
							break;
						}
						break;
					case 10:
						if (name == "Portuguese")
						{
							num = 2;
						}
						break;
					case 8:
						if (name == "Japanese")
						{
							num = 5;
						}
						break;
					case 9:
						if (name == "Hungarian")
						{
							num = 11;
						}
						break;
					}
				}
				int num2 = ((num <= 6) ? (num * 78) : ((num - 7) * 78));
				num2 += ((language.label != null) ? 39 : 0);
				int x = ((num > 6) ? 174 : 0);
				b.Draw(texture, language.bounds, new Rectangle(x, num2, 174, 40), Color.White, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0f);
			}
			if (scrollArea != null)
			{
				scrollArea.finishScrollBoxDrawing(b);
			}
			base.draw(b);
		}

		public override bool readyToClose()
		{
			return true;
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			SetupButtons();
		}
	}
}
