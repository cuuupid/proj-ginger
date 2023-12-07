using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus
{
	public class MobileScrollbox
	{
		public Rectangle Bounds;

		public Rectangle scissorRectangle;

		public MobileScrollbar scrollBar;

		public bool panelScrolling;

		public bool havePanelScrolled;

		public bool scrollingWithMomentum;

		public int yOffsetForScroll;

		public int panelScrollStartY;

		public int yOffSetAtStartOfPanelScroll;

		public int lastYValue;

		private float[] speedMeasure;

		private int currentSpeedMeasure;

		private float speed;

		private const float minSpeed = 1f;

		private const float dampingFactor = 1.05f;

		public int maxYOffset;

		private RasterizerState _rasterizerState;

		private Rectangle _scissorRectangleBackup;

		private const int yChangeToRegisterScroll = 12;

		private static int oldYDiff;

		public MobileScrollbox(int boxX, int boxY, int boxWidth, int boxHeight, int boxContentHeight, Rectangle clipRect, MobileScrollbar scrollBar = null)
		{
			Bounds.X = boxX;
			Bounds.Y = boxY;
			Bounds.Width = boxWidth;
			Bounds.Height = boxHeight;
			scissorRectangle = clipRect;
			this.scrollBar = scrollBar;
			speedMeasure = new float[8];
			maxYOffset = ((boxContentHeight == 0) ? 1 : boxContentHeight);
			scrollingWithMomentum = false;
			speed = 0f;
			_rasterizerState = new RasterizerState
			{
				ScissorTestEnable = true
			};
		}

		public void setUpForScrollBoxDrawing(SpriteBatch b, float scale = 1f)
		{
			b.End();
			_scissorRectangleBackup = b.GraphicsDevice.ScissorRectangle;
			try
			{
				b.GraphicsDevice.ScissorRectangle = scissorRectangle;
			}
			catch (Exception)
			{
			}
			if (scale == 1f)
			{
				b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, _rasterizerState);
			}
			else
			{
				b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, _rasterizerState, null, Matrix.CreateScale(scale));
			}
		}

		public void finishScrollBoxDrawing(SpriteBatch b, float scale = 1f)
		{
			b.End();
			b.GraphicsDevice.ScissorRectangle = _scissorRectangleBackup;
			if (scale == 1f)
			{
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			}
			else
			{
				b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(scale));
			}
		}

		public void setMaxYOffset(int offset)
		{
			maxYOffset = ((offset == 0) ? 1 : offset);
			havePanelScrolled = false;
		}

		public int getMaxYOffset()
		{
			return maxYOffset;
		}

		public void setYOffsetForScroll(int offset)
		{
			yOffsetForScroll = offset;
			if (scrollBar != null)
			{
				scrollBar.setPercentage(yOffsetForScroll * 100 / -maxYOffset);
			}
		}

		public int getYOffsetForScroll()
		{
			return Utility.To4(yOffsetForScroll);
		}

		public void update(GameTime time)
		{
			if (!scrollingWithMomentum)
			{
				return;
			}
			if (Math.Abs(speed) > 1f)
			{
				yOffsetForScroll += (int)speed;
				if (scrollBar != null)
				{
					scrollBar.setPercentage(yOffsetForScroll * 100 / -maxYOffset);
				}
				if (yOffsetForScroll > 0)
				{
					speed = 0f;
					yOffsetForScroll = 0;
					scrollingWithMomentum = false;
					return;
				}
				if (yOffsetForScroll < -maxYOffset)
				{
					yOffsetForScroll = -maxYOffset;
					speed = 0f;
					scrollingWithMomentum = false;
					return;
				}
				float num = 1f;
				if (speed < 0f)
				{
					float num2 = (float)yOffsetForScroll / (0f - (float)maxYOffset);
					if ((double)num2 > 0.9)
					{
						num = Math.Max(1f, (num2 - 0.9f) * 20f);
					}
				}
				else if (speed > 0f)
				{
					float num3 = ((float)maxYOffset + (float)yOffsetForScroll) / (float)maxYOffset;
					if ((double)num3 > 0.9)
					{
						num = Math.Max(1f, (num3 - 0.9f) * 20f);
					}
				}
				speed /= 1.05f * num;
			}
			else
			{
				scrollingWithMomentum = false;
			}
		}

		public void leftClickHeld(int x, int y)
		{
			if ((y > panelScrollStartY && yOffSetAtStartOfPanelScroll >= 0) || (y < panelScrollStartY && yOffSetAtStartOfPanelScroll <= -maxYOffset))
			{
				panelScrollStartY = y;
				return;
			}
			if (panelScrolling && !havePanelScrolled)
			{
				if ((y >= panelScrollStartY && yOffSetAtStartOfPanelScroll >= 0) || (y <= panelScrollStartY && yOffSetAtStartOfPanelScroll <= -maxYOffset))
				{
					panelScrollStartY = y;
					return;
				}
				if (y > panelScrollStartY + 12 || y < panelScrollStartY - 12)
				{
					havePanelScrolled = true;
					lastYValue = y;
				}
			}
			if (!havePanelScrolled)
			{
				return;
			}
			if ((y >= panelScrollStartY && yOffSetAtStartOfPanelScroll >= 0) || (y <= panelScrollStartY && yOffSetAtStartOfPanelScroll <= -maxYOffset))
			{
				panelScrollStartY = y;
				return;
			}
			int num = y - lastYValue;
			if (num > 0)
			{
				if (oldYDiff <= 0)
				{
					panelScrollStartY = y;
					yOffSetAtStartOfPanelScroll = yOffsetForScroll;
					oldYDiff = num;
					return;
				}
				yOffsetForScroll = Math.Min(0, yOffSetAtStartOfPanelScroll + y - panelScrollStartY);
			}
			else if (num < 0)
			{
				if (oldYDiff >= 0)
				{
					panelScrollStartY = y;
					yOffSetAtStartOfPanelScroll = yOffsetForScroll;
					oldYDiff = num;
					return;
				}
				yOffsetForScroll = Math.Max(-maxYOffset, yOffSetAtStartOfPanelScroll + y - panelScrollStartY);
			}
			oldYDiff = num;
			speedMeasure[currentSpeedMeasure] = num;
			lastYValue = y;
			if (scrollBar != null)
			{
				scrollBar.setPercentage(yOffsetForScroll * 100 / -maxYOffset);
			}
			currentSpeedMeasure++;
			if (currentSpeedMeasure >= speedMeasure.Length)
			{
				currentSpeedMeasure = 0;
			}
		}

		public void releaseLeftClick(int x, int y)
		{
			if (havePanelScrolled)
			{
				speed = 0f;
				for (int i = 0; i < speedMeasure.Length; i++)
				{
					speed += speedMeasure[i];
				}
				speed /= speedMeasure.Length;
				scrollingWithMomentum = true;
			}
			panelScrolling = false;
			havePanelScrolled = false;
		}

		public void receiveLeftClick(int x, int y)
		{
			speed = 0f;
			if (Bounds.Contains(x, y))
			{
				panelScrolling = true;
				havePanelScrolled = false;
				panelScrollStartY = y;
				yOffSetAtStartOfPanelScroll = yOffsetForScroll;
				for (int i = 0; i < speedMeasure.Length; i++)
				{
					speedMeasure[i] = 0f;
				}
				currentSpeedMeasure = 0;
			}
		}

		public void receiveScrollWheelAction(int direction)
		{
			yOffsetForScroll = Math.Min(0, Math.Max(yOffsetForScroll + direction, -maxYOffset));
			if (scrollBar != null)
			{
				scrollBar.setPercentage(yOffsetForScroll * 100 / -maxYOffset);
			}
		}
	}
}
