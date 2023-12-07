using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace StardewValley.Tools
{
	public class MagnifyingGlass : Tool
	{
		public MagnifyingGlass()
			: base("Magnifying Glass", -1, 5, 5, stackable: false)
		{
			base.InstantUse = true;
		}

		public override Item getOne()
		{
			MagnifyingGlass magnifyingGlass = new MagnifyingGlass();
			magnifyingGlass._GetOneFrom(this);
			return magnifyingGlass;
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:MagnifyingGlass.cs.14119");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:MagnifyingGlass.cs.14120");
		}

		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			who.Halt();
			who.canMove = true;
			who.UsingTool = false;
			DoFunction(location, Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y, 0, who);
			return true;
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			base.CurrentParentTileIndex = 5;
			base.IndexOfMenuItemView = 5;
			Rectangle value = new Rectangle(x / 64 * 64, y / 64 * 64, 64, 64);
			if (location is Farm)
			{
				foreach (KeyValuePair<long, FarmAnimal> pair in (location as Farm).animals.Pairs)
				{
					if (pair.Value.GetBoundingBox().Intersects(value))
					{
						Game1.activeClickableMenu = new AnimalQueryMenu(pair.Value);
						break;
					}
				}
				return;
			}
			if (!(location is AnimalHouse))
			{
				return;
			}
			foreach (KeyValuePair<long, FarmAnimal> pair2 in (location as AnimalHouse).animals.Pairs)
			{
				if (pair2.Value.GetBoundingBox().Intersects(value))
				{
					Game1.activeClickableMenu = new AnimalQueryMenu(pair2.Value);
					break;
				}
			}
		}
	}
}
