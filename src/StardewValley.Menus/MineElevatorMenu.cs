using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;

namespace StardewValley.Menus
{
	public class MineElevatorMenu : IClickableMenu
	{
		public List<ClickableComponent> elevators = new List<ClickableComponent>();

		public MineElevatorMenu()
			: base(0, 0, 0, 0, showUpperRightCloseButton: true)
		{
			int num = Math.Min(MineShaft.lowestLevelReached, 120) / 5;
			float num2 = (float)Game1.uiViewport.Width / 1280f;
			float num3 = (float)Game1.uiViewport.Height / 720f;
			int num4 = (int)(num2 * 64f * 3f / 2f);
			int num5 = (int)(num2 * 64f / 4f);
			int num6 = (int)(num3 * 64f * 3f / 2f);
			width = ((num > 50) ? ((num4 + num5) * 11 + IClickableMenu.borderWidth * 2) : Math.Min((num4 + num5) * 5 + IClickableMenu.borderWidth * 2, num * (num4 + num5) + IClickableMenu.borderWidth * 2));
			int num7 = Math.Max(1, width / (num4 + num5));
			height = Game1.uiViewport.Height - 50;
			xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2;
			Game1.playSound("crystal");
			int num8 = xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
			int num9 = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.borderWidth / 3;
			elevators.Add(new ClickableComponent(new Rectangle(num8, num9, num4, num6), 0.ToString() ?? "")
			{
				myID = 0,
				rightNeighborID = 1,
				downNeighborID = num7
			});
			num8 = num8 + num4 + num5;
			if (num8 > xPositionOnScreen + width - IClickableMenu.borderWidth)
			{
				num8 = xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
				num9 += num6 + num5;
			}
			for (int i = 1; i <= num; i++)
			{
				elevators.Add(new ClickableComponent(new Rectangle(num8, num9, num4, num6), (i * 5).ToString() ?? "")
				{
					myID = i,
					rightNeighborID = ((i % num7 == num7 - 1) ? (-1) : (i + 1)),
					leftNeighborID = ((i % num7 == 0) ? (-1) : (i - 1)),
					downNeighborID = i + num7,
					upNeighborID = i - num7
				});
				num8 = num8 + num4 + num5;
				if (num8 > xPositionOnScreen + width - IClickableMenu.borderWidth)
				{
					num8 = xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder * 3 / 4;
					num9 += num6 + num5;
				}
			}
			initializeUpperRightCloseButton();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (isWithinBounds(x, y))
			{
				foreach (ClickableComponent elevator in elevators)
				{
					if (!elevator.containsPoint(x, y))
					{
						continue;
					}
					Game1.playSound("smallSelect");
					if (Convert.ToInt32(elevator.name) == 0)
					{
						if (!(Game1.currentLocation is MineShaft))
						{
							return;
						}
						Game1.warpFarmer("Mine", 17, 4, flip: true);
						Game1.exitActiveMenu();
						Game1.changeMusicTrack("none");
					}
					else
					{
						if (Convert.ToInt32(elevator.name) == Game1.CurrentMineLevel)
						{
							return;
						}
						Game1.player.ridingMineElevator = true;
						Game1.enterMine(Convert.ToInt32(elevator.name));
						Game1.exitActiveMenu();
					}
				}
				base.receiveLeftClick(x, y);
			}
			else
			{
				Game1.exitActiveMenu();
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			foreach (ClickableComponent elevator in elevators)
			{
				if (elevator.containsPoint(x, y))
				{
					elevator.scale = 2f;
				}
				else
				{
					elevator.scale = 1f;
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);
			foreach (ClickableComponent elevator in elevators)
			{
				bool flag = false;
				if (elevator.myID == Game1.CurrentMineLevel / 5)
				{
					flag = true;
				}
				IClickableMenu.drawTextureBoxWithIconAndText(b, Game1.dialogueFont, Game1.mouseCursors, new Rectangle(256, 256, 10, 10), null, new Rectangle(0, 0, 1, 1), elevator.name, elevator.bounds.X, elevator.bounds.Y, elevator.bounds.Width, elevator.bounds.Height, Color.White, flag ? 5 : 4, drawShadow: true, iconLeft: false, isClickable: true, flag, drawIcon: false);
			}
			base.draw(b);
			drawMouse(b);
		}
	}
}
