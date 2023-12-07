using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace StardewValley
{
	public class Polygon
	{
		public class Line
		{
			public Vector2 Start;

			public Vector2 End;

			public Line(Vector2 Start, Vector2 End)
			{
				this.Start = Start;
				this.End = End;
			}
		}

		public List<Line> lines = new List<Line>();

		public List<Line> Lines
		{
			get
			{
				return lines;
			}
			set
			{
				lines = value;
			}
		}

		public void addPoint(Vector2 point)
		{
			if (lines.Count > 0)
			{
				lines.Add(new Line(Lines[Lines.Count - 1].End, point));
			}
		}

		public bool containsPoint(Vector2 point)
		{
			foreach (Line line in Lines)
			{
				if (line.End.Equals(point))
				{
					return true;
				}
			}
			return false;
		}

		public static Polygon getGentlerBorderForLakes(Rectangle room, Random mineRandom)
		{
			return getGentlerBorderForLakes(room, mineRandom, Rectangle.Empty);
		}

		public static Polygon getEdgeBorder(Rectangle room, Random mineRandom)
		{
			return getEdgeBorder(room, mineRandom, new List<Rectangle>(), (room.Width - 2) / 2, (room.Height - 2) / 2);
		}

		public static Polygon getEdgeBorder(Rectangle room, Random mineRandom, List<Rectangle> smoothZone)
		{
			return getEdgeBorder(room, mineRandom, smoothZone, (room.Width - 2) / 2, (room.Height - 2) / 2);
		}

		public static Polygon getEdgeBorder(Rectangle room, Random mineRandom, List<Rectangle> smoothZone, int horizontalInwardLimit, int verticalInwardLimit)
		{
			if (smoothZone == null)
			{
				smoothZone = new List<Rectangle>();
			}
			int width = room.Width;
			int height = room.Height;
			int num = room.Width - 2;
			int num2 = room.Height - 2;
			int num3 = room.X + 1;
			int num4 = room.Y + 1;
			Rectangle rectangle = new Rectangle(num3, num4, num, num2);
			Polygon polygon = new Polygon();
			Vector2 vector = new Vector2(mineRandom.Next(num3 + 5, num3 + 8), mineRandom.Next(num4 + 5, num4 + 8));
			polygon.Lines.Add(new Line(vector, new Vector2(vector.X + 1f, vector.Y)));
			vector.X += 1f;
			int num5 = num - 12;
			List<int> list = new List<int> { 2, 2, 2 };
			int num6 = 0;
			while (num6 < num5)
			{
				int num7 = mineRandom.Next(3);
				if (list.Last() != num7 && list[list.Count - 2] != list.Last())
				{
					num7 = list.Last();
				}
				if (num7 == 0 && vector.Y > (float)num4 && !list.Contains(1))
				{
					vector.Y -= 1f;
					list.Add(0);
				}
				else if (num7 == 1 && vector.Y < (float)(num4 + verticalInwardLimit) && !list.Contains(0))
				{
					vector.Y += 1f;
					list.Add(1);
				}
				else
				{
					vector.X += 1f;
					num6++;
					list.Add(2);
				}
				list.RemoveAt(0);
				polygon.addPoint(vector);
			}
			int num8 = num2 - 4 - (int)(vector.Y - (float)room.Y);
			vector.Y += 1f;
			list = new List<int> { 2, 2, 2 };
			polygon.addPoint(vector);
			int num9 = 0;
			while (num9 < num8)
			{
				int num10 = mineRandom.Next(3);
				if (list.Last() != num10 && list[list.Count - 2] != list.Last())
				{
					num10 = list.Last();
				}
				if (num9 > 4 && num10 == 0 && vector.X < (float)(num3 + num - 1) && !list.Contains(1) && !Utility.pointInRectangles(smoothZone, (int)vector.X, (int)vector.Y))
				{
					vector.X += 1f;
					list.Add(0);
				}
				else if (num9 > 4 && num10 == 1 && vector.X > (float)(num3 + num - horizontalInwardLimit + 1) && !list.Contains(0) && !Utility.pointInRectangles(smoothZone, (int)vector.X, (int)vector.Y))
				{
					vector.X -= 1f;
					list.Add(1);
				}
				else
				{
					vector.Y += 1f;
					num9++;
					list.Add(2);
				}
				list.RemoveAt(0);
				polygon.addPoint(vector);
			}
			int num11 = (int)vector.X - (int)polygon.Lines[0].Start.X + 1;
			vector.X -= 1f;
			list = new List<int> { 2, 2, 2 };
			polygon.addPoint(vector);
			int num12 = 0;
			while (num12 < num11)
			{
				int num13 = mineRandom.Next(3);
				if (list.Last() != num13 && list[list.Count - 2] != list.Last())
				{
					num13 = list.Last();
				}
				if (num12 > 4 && num13 == 0 && vector.Y > (float)(num4 + num2 - verticalInwardLimit) && !list.Contains(1) && !polygon.containsPoint(new Vector2(vector.X, vector.Y - 1f)))
				{
					vector.Y -= 1f;
					list.Add(0);
				}
				else if (num12 > 4 && num13 == 1 && vector.Y < (float)(num4 + num2) && !list.Contains(0))
				{
					vector.Y += 1f;
					list.Add(1);
				}
				else
				{
					vector.X -= 1f;
					num12++;
					list.Add(2);
				}
				list.RemoveAt(0);
				polygon.addPoint(vector);
			}
			int num14 = (int)vector.Y - (int)polygon.Lines[0].Start.Y - 1;
			vector.Y -= 1f;
			list = new List<int> { 2, 2, 2 };
			polygon.addPoint(vector);
			int num15 = 0;
			while (num15 < num14)
			{
				int num16 = mineRandom.Next(3);
				if (list.Last() != num16 && list[list.Count - 2] != list.Last())
				{
					num16 = list.Last();
				}
				if (num15 > 4 && num16 == 0 && vector.X < (float)(int)polygon.Lines[0].Start.X && !list.Contains(1) && !polygon.containsPoint(new Vector2(vector.X + 1f, vector.Y)) && !polygon.containsPoint(new Vector2(vector.X, vector.Y - 1f)) && !Utility.pointInRectangles(smoothZone, (int)vector.X, (int)vector.Y))
				{
					vector.X += 1f;
					list.Add(0);
				}
				else if (num15 > 4 && num16 == 1 && vector.X > (float)(num3 + 1) && !list.Contains(0) && !Utility.pointInRectangles(smoothZone, (int)vector.X, (int)vector.Y))
				{
					vector.X -= 1f;
					list.Add(1);
				}
				else
				{
					vector.Y -= 1f;
					num15++;
					list.Add(2);
				}
				list.RemoveAt(0);
				polygon.addPoint(vector);
			}
			if (vector.X < (float)(int)polygon.Lines[0].Start.X)
			{
				int num17 = (int)polygon.Lines[0].Start.X + 1 - (int)vector.X - 1;
				for (int i = 0; i < num17; i++)
				{
					vector.X += 1f;
					polygon.addPoint(vector);
				}
			}
			return polygon;
		}

		public static Polygon getGentlerBorderForLakes(Rectangle room, Random mineRandom, Rectangle smoothZone)
		{
			int width = room.Width;
			int height = room.Height;
			int num = room.Width - 2;
			int num2 = room.Height - 2;
			int num3 = room.X + 1;
			int num4 = room.Y + 1;
			Rectangle rectangle = new Rectangle(num3, num4, num, num2);
			Polygon polygon = new Polygon();
			Vector2 vector = new Vector2(mineRandom.Next(num3 + 5, num3 + 8), mineRandom.Next(num4 + 5, num4 + 8));
			polygon.Lines.Add(new Line(vector, new Vector2(vector.X + 1f, vector.Y)));
			vector.X += 1f;
			int num5 = num - 12;
			List<int> list = new List<int> { 2, 2, 2 };
			int num6 = 0;
			while (num6 < num5)
			{
				int num7 = mineRandom.Next(3);
				if (num7 == 0 && vector.Y > (float)num4 && !list.Contains(1) && !smoothZone.Contains((int)vector.X, (int)vector.Y))
				{
					vector.Y -= 1f;
					list.Add(0);
				}
				else if (num7 == 1 && vector.Y < (float)(num4 + num2 / 2) && !list.Contains(0) && !smoothZone.Contains((int)vector.X, (int)vector.Y))
				{
					vector.Y += 1f;
					list.Add(1);
				}
				else
				{
					vector.X += 1f;
					num6++;
					list.Add(2);
				}
				list.RemoveAt(0);
				polygon.addPoint(vector);
			}
			int num8 = num2 - 4 - (int)(vector.Y - (float)room.Y);
			vector.Y += 1f;
			list = new List<int> { 2, 2, 2 };
			polygon.addPoint(vector);
			int num9 = 0;
			while (num9 < num8)
			{
				int num10 = mineRandom.Next(3);
				if (num10 == 0 && vector.X < (float)(num3 + num) && !list.Contains(1) && !smoothZone.Contains((int)vector.X, (int)vector.Y))
				{
					vector.X += 1f;
					list.Add(0);
				}
				else if (num10 == 1 && vector.X > (float)(num3 + num / 2 + 1) && !list.Contains(0) && !smoothZone.Contains((int)vector.X, (int)vector.Y))
				{
					vector.X -= 1f;
					list.Add(1);
				}
				else
				{
					vector.Y += 1f;
					num9++;
					list.Add(2);
				}
				list.RemoveAt(0);
				polygon.addPoint(vector);
			}
			int num11 = (int)vector.X - (int)polygon.Lines[0].Start.X + 1;
			vector.X -= 1f;
			list = new List<int> { 2, 2, 2 };
			polygon.addPoint(vector);
			int num12 = 0;
			while (num12 < num11)
			{
				int num13 = mineRandom.Next(3);
				if (num13 == 0 && vector.Y > (float)(num4 + num2 / 2) && !list.Contains(1) && !polygon.containsPoint(new Vector2(vector.X, vector.Y - 1f)) && !smoothZone.Contains((int)vector.X, (int)vector.Y))
				{
					vector.Y -= 1f;
					list.Add(0);
				}
				else if (num13 == 1 && vector.Y < (float)(num4 + num2) && !list.Contains(0) && !smoothZone.Contains((int)vector.X, (int)vector.Y))
				{
					vector.Y += 1f;
					list.Add(1);
				}
				else
				{
					vector.X -= 1f;
					num12++;
					list.Add(2);
				}
				list.RemoveAt(0);
				polygon.addPoint(vector);
			}
			int num14 = (int)vector.Y - (int)polygon.Lines[0].Start.Y - 1;
			vector.Y -= 1f;
			list = new List<int> { 2, 2, 2 };
			polygon.addPoint(vector);
			int num15 = 0;
			while (num15 < num14)
			{
				int num16 = mineRandom.Next(3);
				if (num16 == 0 && vector.X < (float)(int)polygon.Lines[0].Start.X && !list.Contains(1) && !polygon.containsPoint(new Vector2(vector.X + 1f, vector.Y)) && !polygon.containsPoint(new Vector2(vector.X, vector.Y - 1f)) && !smoothZone.Contains((int)vector.X, (int)vector.Y))
				{
					vector.X += 1f;
					list.Add(0);
				}
				else if (num16 == 1 && vector.X > (float)(num3 + 1) && !list.Contains(0) && !smoothZone.Contains((int)vector.X, (int)vector.Y))
				{
					vector.X -= 1f;
					list.Add(1);
				}
				else
				{
					vector.Y -= 1f;
					num15++;
					list.Add(2);
				}
				list.RemoveAt(0);
				polygon.addPoint(vector);
			}
			if (vector.X < (float)(int)polygon.Lines[0].Start.X)
			{
				int num17 = (int)polygon.Lines[0].Start.X + 1 - (int)vector.X - 1;
				for (int i = 0; i < num17; i++)
				{
					vector.X += 1f;
					polygon.addPoint(vector);
				}
			}
			return polygon;
		}

		public static Polygon getRandomBorderRoom(Rectangle room, Random mineRandom)
		{
			int width = room.Width;
			int height = room.Height;
			int num = room.Width - 2;
			int num2 = room.Height - 2;
			int num3 = room.X + 1;
			int num4 = room.Y + 1;
			Rectangle rectangle = new Rectangle(num3, num4, num, num2);
			Polygon polygon = new Polygon();
			Vector2 vector = new Vector2(mineRandom.Next(num3 + 5, num3 + 8), mineRandom.Next(num4 + 5, num4 + 8));
			polygon.Lines.Add(new Line(vector, new Vector2(vector.X + 1f, vector.Y)));
			vector.X += 1f;
			int num5 = room.Right - (int)vector.X - num / 8;
			int num6 = 2;
			int num7 = 0;
			while (num7 < num5)
			{
				int num8 = mineRandom.Next(3);
				if ((num8 == 0 && vector.Y > (float)room.Y && num6 != 1) || (num6 == 2 && vector.Y >= (float)(num4 + num2 / 2)))
				{
					vector.Y -= 1f;
					num6 = 0;
				}
				else if ((num8 == 1 && vector.Y < (float)(num4 + num2 / 2) && num6 != 0) || (num6 == 2 && vector.Y <= (float)room.Y))
				{
					vector.Y += 1f;
					num6 = 1;
				}
				else
				{
					vector.X += 1f;
					num7++;
					num6 = 2;
				}
				polygon.addPoint(vector);
			}
			int num9 = num2 - 4 - (int)(vector.Y - (float)room.Y);
			vector.Y += 1f;
			num6 = 2;
			polygon.addPoint(vector);
			int num10 = 0;
			while (num10 < num9)
			{
				int num11 = mineRandom.Next(3);
				if ((num11 == 0 && vector.X < (float)room.Right && num6 != 1) || (num6 == 2 && vector.X <= (float)(num3 + num / 2 + 1)))
				{
					vector.X += 1f;
					num6 = 0;
				}
				else if ((num11 == 1 && vector.X > (float)(num3 + num / 2 + 1) && num6 != 0) || (num6 == 2 && vector.X >= (float)room.Right))
				{
					vector.X -= 1f;
					num6 = 1;
				}
				else
				{
					vector.Y += 1f;
					num10++;
					num6 = 2;
				}
				polygon.addPoint(vector);
			}
			int num12 = (int)vector.X - (int)polygon.Lines[0].Start.X + num / 4;
			vector.X -= 1f;
			num6 = 2;
			polygon.addPoint(vector);
			int num13 = 0;
			while (num13 < num12)
			{
				int num14 = mineRandom.Next(3);
				if ((num14 == 0 && vector.Y > (float)(num4 + num2 / 2) && num6 != 1 && !polygon.containsPoint(new Vector2(vector.X, vector.Y - 1f))) || (num6 == 2 && vector.Y >= (float)room.Bottom))
				{
					vector.Y -= 1f;
					num6 = 0;
				}
				else if ((num14 == 1 && vector.Y < (float)room.Bottom && num6 != 0) || (num6 == 2 && vector.Y <= (float)(num4 + num2 / 2)))
				{
					vector.Y += 1f;
					num6 = 1;
				}
				else
				{
					vector.X -= 1f;
					num13++;
					num6 = 2;
				}
				polygon.addPoint(vector);
			}
			int num15 = (int)vector.Y - (int)polygon.Lines[0].Start.Y - 1;
			vector.Y -= 1f;
			num6 = 2;
			polygon.addPoint(vector);
			int num16 = 0;
			while (num16 < num15)
			{
				int num17 = mineRandom.Next(3);
				if ((num17 == 0 && vector.X < (float)room.Center.X && !polygon.containsPoint(new Vector2(vector.X + 1f, vector.Y)) && !polygon.containsPoint(new Vector2(vector.X, vector.Y - 1f))) || (num6 == 2 && vector.X <= (float)room.X))
				{
					vector.X += 1f;
					num6 = 0;
				}
				else if ((num17 == 1 && vector.X > (float)room.X && num6 != 0) || (num6 == 2 && vector.X >= (float)room.Center.X))
				{
					vector.X -= 1f;
					num6 = 1;
				}
				else
				{
					vector.Y -= 1f;
					num16++;
					num6 = 2;
				}
				polygon.addPoint(vector);
			}
			if (vector.X < (float)(int)polygon.Lines[0].Start.X)
			{
				int num18 = (int)polygon.Lines[0].Start.X + 1 - (int)vector.X - 1;
				for (int i = 0; i < num18; i++)
				{
					vector.X += 1f;
					polygon.addPoint(vector);
				}
			}
			return polygon;
		}
	}
}
