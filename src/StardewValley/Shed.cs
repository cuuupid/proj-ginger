using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace StardewValley
{
	public class Shed : DecoratableLocation
	{
		public readonly NetInt upgradeLevel = new NetInt(0);

		private bool isRobinUpgrading;

		public Shed()
		{
		}

		public Shed(string m, string name)
			: base(m, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(upgradeLevel);
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.isDarkOut())
			{
				Game1.ambientLight = new Color(180, 180, 0);
			}
			if (Game1.getFarm().isThereABuildingUnderConstruction() && Game1.getFarm().getBuildingUnderConstruction().indoors.Value != null && Game1.getFarm().getBuildingUnderConstruction().indoors.Value.Equals(this))
			{
				isRobinUpgrading = true;
			}
			else
			{
				isRobinUpgrading = false;
			}
		}

		public Building getBuilding()
		{
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building.indoors.Value != null && building.indoors.Value.Equals(this))
				{
					return building;
				}
			}
			return null;
		}

		public virtual void setUpgradeLevel(int upgrade_level)
		{
			upgradeLevel.Set(upgrade_level);
			updateMap();
			updateLayout();
			base.tapToMove.Init(this);
		}

		public void updateLayout()
		{
			updateDoors();
			updateWarps();
			setWallpapers();
			setFloors();
		}

		public override List<Rectangle> getWalls()
		{
			List<Rectangle> list = new List<Rectangle>();
			if ((int)upgradeLevel == 0)
			{
				list.Add(new Rectangle(1, 1, 11, 3));
			}
			else if ((int)upgradeLevel == 1)
			{
				list.Add(new Rectangle(1, 1, 17, 3));
			}
			return list;
		}

		public override List<Rectangle> getFloors()
		{
			List<Rectangle> list = new List<Rectangle>();
			if ((int)upgradeLevel == 0)
			{
				list.Add(new Rectangle(1, 3, 11, 11));
			}
			else if ((int)upgradeLevel == 1)
			{
				list.Add(new Rectangle(1, 3, 17, 14));
			}
			return list;
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (isRobinUpgrading)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(64f, 64f)), new Rectangle(90, 0, 33, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01546f);
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(64f, 84f)), new Rectangle(90, 0, 33, 31), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.015360001f);
			}
		}
	}
}
