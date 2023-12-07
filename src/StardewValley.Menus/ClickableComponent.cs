using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley.Menus
{
	public class ClickableComponent
	{
		public const int ID_ignore = -500;

		public const int CUSTOM_SNAP_BEHAVIOR = -7777;

		public const int SNAP_AUTOMATIC = -99998;

		public const int SNAP_TO_DEFAULT = -99999;

		public Rectangle bounds;

		public string name;

		public string label;

		public float scale = 1f;

		public Item item;

		public bool visible = true;

		public bool leftNeighborImmutable;

		public bool rightNeighborImmutable;

		public bool upNeighborImmutable;

		public bool downNeighborImmutable;

		public bool tryDefaultIfNoDownNeighborExists;

		public bool tryDefaultIfNoRightNeighborExists;

		public bool fullyImmutable;

		public int myID = -500;

		public int leftNeighborID = -1;

		public int rightNeighborID = -1;

		public int upNeighborID = -1;

		public int downNeighborID = -1;

		public int myAlternateID = -500;

		public int region;

		public ClickableComponent(Rectangle bounds, string name)
		{
			this.bounds = bounds;
			this.name = name;
		}

		public ClickableComponent(Rectangle bounds, string name, string label)
		{
			this.bounds = bounds;
			this.name = name;
			this.label = label;
		}

		public ClickableComponent(Rectangle bounds, Item item)
		{
			this.bounds = bounds;
			this.item = item;
		}

		public virtual bool containsPoint(int x, int y)
		{
			if (!visible)
			{
				return false;
			}
			if (bounds.Contains(x, y))
			{
				Game1.SetFreeCursorDrag();
				return true;
			}
			return false;
		}

		public virtual void snapMouseCursor()
		{
			Game1.setMousePosition(bounds.Right - bounds.Width / 8, bounds.Bottom - bounds.Height / 8);
		}

		public void snapMouseCursorToCenter()
		{
			Game1.setMousePosition(bounds.Center.X, bounds.Center.Y);
		}

		public static void SetUpNeighbors<T>(List<T> components, int id) where T : ClickableComponent
		{
			for (int i = 0; i < components.Count; i++)
			{
				T val = components[i];
				if (val != null)
				{
					val.upNeighborID = id;
				}
			}
		}

		public static void ChainNeighborsLeftRight<T>(List<T> components) where T : ClickableComponent
		{
			ClickableComponent clickableComponent = null;
			for (int i = 0; i < components.Count; i++)
			{
				T val = components[i];
				if (val != null)
				{
					val.rightNeighborID = -1;
					val.leftNeighborID = -1;
					if (clickableComponent != null)
					{
						val.leftNeighborID = clickableComponent.myID;
						clickableComponent.rightNeighborID = val.myID;
					}
					clickableComponent = val;
				}
			}
		}

		public static void ChainNeighborsUpDown<T>(List<T> components) where T : ClickableComponent
		{
			ClickableComponent clickableComponent = null;
			for (int i = 0; i < components.Count; i++)
			{
				T val = components[i];
				if (val != null)
				{
					val.downNeighborID = -1;
					val.upNeighborID = -1;
					if (clickableComponent != null)
					{
						val.upNeighborID = clickableComponent.myID;
						clickableComponent.downNeighborID = val.myID;
					}
					clickableComponent = val;
				}
			}
		}

		public void SetSnapAutomatic()
		{
			if (myID == -500)
			{
				myID = -1;
			}
			upNeighborID = -99998;
			leftNeighborID = -99998;
			rightNeighborID = -99998;
			downNeighborID = -99998;
		}
	}
}
