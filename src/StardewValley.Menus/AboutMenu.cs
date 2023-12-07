using System;
using Android.App;
using Android.Content;
using Android.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus
{
	public class AboutMenu : IClickableMenu
	{
		public const int region_linkToTwitter = 91111;

		public const int region_linkToSVSite = 92222;

		public const int minWidth = 950;

		public const int maxWidth = 1200;

		public int screenWidth;

		public int screenHeight;

		public ClickableComponent linkToTwitter;

		public ClickableComponent linkToSVSite;

		public ClickableComponent backButton;

		public AboutMenu()
		{
			Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
			width = Game1.uiViewport.Width - Game1.xEdge * 2;
			screenWidth = Game1.uiViewport.Width;
			screenHeight = Game1.uiViewport.Height;
			height = Math.Min(800, screenHeight);
			topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
			initializeUpperRightCloseButton();
			string s = Game1.content.LoadString("Strings\\UI:About_Credit_Mobile").Replace('\n', '^');
			int heightOfString = SpriteText.getHeightOfString(s);
			int x = (int)topLeftPositionForCenteringOnScreen.X + 32;
			int num = (int)topLeftPositionForCenteringOnScreen.Y + 64 + heightOfString;
			linkToSVSite = new ClickableComponent(new Rectangle(x, num, width - 64, 64), "", Game1.content.LoadString("Strings\\UI:About_Website"))
			{
				myID = 92222,
				downNeighborID = 91111
			};
			linkToTwitter = new ClickableComponent(new Rectangle(x, num + 64, width - 64, 64), "", Game1.content.LoadString("Strings\\UI:About_ConcernedApe"))
			{
				myID = 91111,
				upNeighborID = 92222
			};
			if (height < 700)
			{
				linkToSVSite.visible = true;
				linkToTwitter.visible = false;
			}
			else
			{
				linkToSVSite.visible = true;
				linkToTwitter.visible = true;
			}
			linkToTwitter.downNeighborID = 81114;
			backButton = new ClickableComponent(new Rectangle(Game1.uiViewport.Width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, Game1.uiViewport.Height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), "")
			{
				myID = 81114,
				upNeighborID = 91111
			};
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(81114);
			snapCursorToCurrentSnappedComponent();
		}

		private static void LaunchBrowser(string url)
		{
			Game1.playSound("bigSelect");
			Intent intent = new Intent("android.intent.action.VIEW", Android.Net.Uri.Parse(url));
			intent.AddFlags(ActivityFlags.NewTask);
			try
			{
				Application.Context.StartActivity(intent);
			}
			catch (Exception)
			{
				try
				{
					MainActivity.instance.StartActivity(intent);
				}
				catch (Exception)
				{
				}
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
			{
				if (playSound)
				{
					Game1.playSound("bigDeSelect");
				}
				if (TitleMenu.subMenu != null && Game1.activeClickableMenu != null)
				{
					TitleMenu.subMenu = null;
				}
			}
			if (linkToSVSite.containsPoint(x, y) && linkToSVSite.visible)
			{
				LaunchBrowser("http://www.stardewvalley.net");
			}
			else if (linkToTwitter.containsPoint(x, y) && linkToTwitter.visible)
			{
				LaunchBrowser("http://www.twitter.com/ConcernedApe");
			}
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void draw(SpriteBatch b)
		{
			Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(width, height - 100);
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), (int)topLeftPositionForCenteringOnScreen.X, (int)topLeftPositionForCenteringOnScreen.Y, width, height - 150, Color.White, 4f);
			SpriteText.drawString(b, Game1.content.LoadString("Strings\\UI:About_Title"), (int)topLeftPositionForCenteringOnScreen.X + 32, (int)topLeftPositionForCenteringOnScreen.Y + 32, 9999, -1, 9999, 1f, 0.88f, junimoText: false, -1, "", 6);
			string text = Game1.content.LoadString("Strings\\UI:About_Credit_Mobile");
			SpriteText.drawString(b, text.Replace('\n', '^'), (int)topLeftPositionForCenteringOnScreen.X + 32, (int)topLeftPositionForCenteringOnScreen.Y + 32, 9999, -1, 9999, 1f, 0.88f, junimoText: false, -1, "", 4);
			int heightOfString = SpriteText.getHeightOfString(text.Replace('\n', '^'));
			linkToSVSite.bounds.Y = (int)(topLeftPositionForCenteringOnScreen.Y + 32f) + heightOfString - 28;
			linkToTwitter.bounds.Y = linkToSVSite.bounds.Y + 64;
			upperRightCloseButton.draw(b);
			if (linkToSVSite.visible)
			{
				SpriteText.drawString(b, "< " + linkToSVSite.label, (int)topLeftPositionForCenteringOnScreen.X + 4 + 32, linkToSVSite.bounds.Y, 999, -1, 999, 1f, 1f, junimoText: false, -1, "", (linkToSVSite.scale == 1f) ? 3 : 7);
			}
			if (linkToTwitter.visible)
			{
				SpriteText.drawString(b, "= " + linkToTwitter.label, (int)topLeftPositionForCenteringOnScreen.X + 4 + 32, linkToTwitter.bounds.Y, 999, -1, 999, 1f, 1f, junimoText: false, -1, "", (linkToTwitter.scale == 1f) ? 3 : 7);
			}
			b.Draw(Game1.mouseCursors, new Vector2(topLeftPositionForCenteringOnScreen.X + (float)width - 96f, topLeftPositionForCenteringOnScreen.Y + 128f), new Rectangle(540 + 13 * (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / 150.0), 333, 13, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
			if (linkToSVSite.visible || linkToTwitter.visible)
			{
				b.Draw(Game1.mouseCursors, new Vector2(topLeftPositionForCenteringOnScreen.X + (float)width - 96f, topLeftPositionForCenteringOnScreen.Y + (float)height - 256f), new Rectangle(592 + 13 * (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / 150.0), 333, 13, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is TitleMenu && (Game1.activeClickableMenu as TitleMenu).startupMessage.Length > 0)
			{
				b.DrawString(Game1.smallFont, Game1.parseText((Game1.activeClickableMenu as TitleMenu).startupMessage, Game1.smallFont, 640), new Vector2(8f, (float)Game1.uiViewport.Height - Game1.smallFont.MeasureString(Game1.parseText((Game1.activeClickableMenu as TitleMenu).startupMessage, Game1.smallFont, 640)).Y - 4f), Color.White);
			}
			else
			{
				b.DrawString(Game1.smallFont, "v" + Game1.GetVersionString(), new Vector2(16 + Game1.xEdge, (float)Game1.uiViewport.Height - Game1.smallFont.MeasureString("v" + Game1.version).Y - 8f), Color.White);
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			backButton = new ClickableComponent(new Rectangle(Game1.uiViewport.Width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, Game1.uiViewport.Height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), "")
			{
				myID = 81114,
				upNeighborID = 91111
			};
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				int id = ((currentlySnappedComponent != null) ? currentlySnappedComponent.myID : 81114);
				populateClickableComponentList();
				currentlySnappedComponent = getComponentWithID(id);
				snapCursorToCurrentSnappedComponent();
			}
		}
	}
}
