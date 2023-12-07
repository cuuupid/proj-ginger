using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Util
{
	public class BoundingBoxGroup
	{
		private List<Rectangle> rectangles = new List<Rectangle>();

		public bool Intersects(Rectangle rect)
		{
			foreach (Rectangle rectangle in rectangles)
			{
				if (rectangle.Intersects(rect))
				{
					return true;
				}
			}
			return false;
		}

		public bool Contains(int x, int y)
		{
			foreach (Rectangle rectangle in rectangles)
			{
				if (rectangle.Contains(x, y))
				{
					return true;
				}
			}
			return false;
		}

		public void Add(Rectangle rect)
		{
			if (!rectangles.Contains(rect))
			{
				rectangles.Add(rect);
			}
		}

		public void ClearNonIntersecting(Rectangle rect)
		{
			for (int num = rectangles.Count - 1; num >= 0; num--)
			{
				if (!rectangles[num].Intersects(rect))
				{
					rectangles.RemoveAt(num);
				}
			}
		}

		public void Clear()
		{
			rectangles.Clear();
		}

		public void Draw(SpriteBatch b)
		{
			foreach (Rectangle rectangle in rectangles)
			{
				Rectangle destinationRectangle = rectangle;
				destinationRectangle.Offset(-Game1.viewport.X, -Game1.viewport.Y);
				b.Draw(Game1.fadeToBlackRect, destinationRectangle, Color.Green * 0.5f);
			}
		}

		public bool IsEmpty()
		{
			return rectangles.Count == 0;
		}
	}
}
