using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Objects;

namespace StardewValley.Menus
{
	public class CharacterCustomization : GameMenu
	{
		[XmlType(TypeName = "MobileSource")]
		public enum Source
		{
			NewGame,
			NewFarmhand,
			Wizard,
			HostNewFarm,
			Dresser,
			ClothesDye,
			DyePots
		}

		public static bool clickedOnMenu;

		public bool showingCoopHelp;

		public Source source;

		public CharacterCustomization(Clothing item)
			: this(Source.ClothesDye, tutorialsWanted: false, item)
		{
		}

		public CharacterCustomization(Source source = Source.NewGame, bool tutorialsWanted = false, Clothing item = null)
			: base(standardTabs: false)
		{
			width = Game1.uiViewport.Width;
			height = Game1.uiViewport.Height;
			if (height > 1080)
			{
				height = (height = 1080);
				yPositionOnScreen = (yPositionOnScreen = (Game1.uiViewport.Height - height) / 2);
			}
			widthMod = (float)width / 1280f;
			heightMod = (float)height / 720f;
			this.source = source;
			TutorialManager.Instance.showTheTutorials = tutorialsWanted;
			tabWidth = 0;
			GameMenu.tabHeight = 0;
			tabCollisionHeight = 90;
			edgeX = Game1.xEdge;
			edgeY = 0;
			tabY = (int)(5f * heightMod);
			tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + tabY, tabWidth, tabCollisionHeight), "customize", ""));
			pages.Add(new MobileCustomizer(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + GameMenu.tabHeight, width - edgeX * 2, height, source, item));
			if (source != Source.Wizard && source != Source.ClothesDye && source != Source.DyePots)
			{
				tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + edgeX + tabWidth, yPositionOnScreen + tabY + edgeY, tabWidth, tabCollisionHeight), "farmtype", ""));
				pages.Add(new MobileFarmChooser(xPositionOnScreen + edgeX, yPositionOnScreen + edgeY + GameMenu.tabHeight, width - edgeX * 2, height, source));
			}
			if (Game1.activeClickableMenu == null)
			{
				Game1.playSound("bigSelect");
			}
			if (source == Source.Wizard)
			{
				upperRightCloseButton = null;
			}
			if (Game1.options.SnappyMenus)
			{
				changeTab(0, playSound: false);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			pages[currentTab].update(time);
		}

		public override void releaseLeftClick(int x, int y)
		{
			if (clickedOnMenu)
			{
				clickedOnMenu = false;
				pages[currentTab].releaseLeftClick(x, y);
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			if (clickedOnMenu)
			{
				pages[currentTab].leftClickHeld(x, y);
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			clickedOnMenu = true;
			base.receiveLeftClick(x, y, playSound);
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.B)
			{
				Game1.playSound("bigDeSelect");
				exitThisMenu();
			}
			else
			{
				pages[currentTab].receiveGamePadButton(b);
			}
		}

		public override void draw(SpriteBatch b)
		{
			pages[currentTab].draw(b);
			if (upperRightCloseButton != null)
			{
				upperRightCloseButton.draw(b);
			}
		}
	}
}
