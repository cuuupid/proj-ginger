using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Objects;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class IslandFarmHouse : DecoratableLocation
	{
		[XmlElement("fridge")]
		public readonly NetRef<Chest> fridge = new NetRef<Chest>(new Chest(playerChest: true));

		public Point fridgePosition;

		public NetBool visited = new NetBool(value: false);

		public IslandFarmHouse()
		{
		}

		public IslandFarmHouse(string map, string name)
			: base(map, name)
		{
			furniture.Add(new Furniture(1798, new Vector2(12f, 8f)));
			furniture.Add(new Furniture(1614, new Vector2(3f, 1f)));
			furniture.Add(new Furniture(1614, new Vector2(8f, 1f)));
			furniture.Add(new Furniture(1614, new Vector2(20f, 1f)));
			furniture.Add(new Furniture(1614, new Vector2(25f, 1f)));
			furniture.Add(new Furniture(1294, new Vector2(1f, 4f)));
			furniture.Add(new Furniture(1294, new Vector2(10f, 4f)));
			furniture.Add(new Furniture(1294, new Vector2(18f, 4f)));
			furniture.Add(new Furniture(1294, new Vector2(28f, 4f)));
			furniture.Add(new Furniture(1742, new Vector2(20f, 4f)));
			Furniture item = new Furniture(1755, new Vector2(14f, 9f));
			furniture.Add(item);
			ReadWallpaperAndFloorTileData();
			setWallpaper(88, 0, persist: true);
			setFloor(23, 0, persist: true);
			setWallpaper(88, 1, persist: true);
			setFloor(48, 1, persist: true);
			setWallpaper(87, 2, persist: true);
			setFloor(52, 2, persist: true);
			setWallpaper(87, 3, persist: true);
			setFloor(23, 3, persist: true);
			setWallpaper(87, 4, persist: true);
			fridgePosition = default(Point);
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			fridge.Value = (l as IslandFarmHouse).fridge.Value;
			visited.Value = (l as IslandFarmHouse).visited.Value;
			base.TransferDataFromSavedLocation(l);
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action != null && who.IsLocalPlayer)
			{
				string[] array = action.Split(' ');
				string text = array[0];
				if (text == "kitchen")
				{
					ActivateKitchen(fridge);
					return true;
				}
				if (text == "drawer")
				{
					return performAction("kitchen", who, tileLocation);
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			fridge.Value.updateWhenCurrentLocation(time, this);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
		}

		public override List<Microsoft.Xna.Framework.Rectangle> getWalls()
		{
			List<Microsoft.Xna.Framework.Rectangle> list = new List<Microsoft.Xna.Framework.Rectangle>();
			list.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 10, 3));
			list.Add(new Microsoft.Xna.Framework.Rectangle(18, 1, 11, 3));
			list.Add(new Microsoft.Xna.Framework.Rectangle(12, 5, 5, 2));
			list.Add(new Microsoft.Xna.Framework.Rectangle(17, 9, 2, 2));
			list.Add(new Microsoft.Xna.Framework.Rectangle(21, 9, 8, 2));
			return list;
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (!visited.Value)
			{
				visited.Value = true;
			}
			bool flag = false;
			for (int i = 0; i < map.GetLayer("Buildings").LayerWidth; i++)
			{
				for (int j = 0; j < map.GetLayer("Buildings").LayerHeight; j++)
				{
					if (map.GetLayer("Buildings").Tiles[i, j] != null)
					{
						int tileIndex = map.GetLayer("Buildings").Tiles[i, j].TileIndex;
						if (tileIndex == 258)
						{
							fridgePosition = new Point(i, j);
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					break;
				}
			}
		}

		public override List<Microsoft.Xna.Framework.Rectangle> getFloors()
		{
			List<Microsoft.Xna.Framework.Rectangle> list = new List<Microsoft.Xna.Framework.Rectangle>();
			list.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 11, 12));
			list.Add(new Microsoft.Xna.Framework.Rectangle(11, 7, 6, 9));
			list.Add(new Microsoft.Xna.Framework.Rectangle(18, 3, 11, 6));
			list.Add(new Microsoft.Xna.Framework.Rectangle(17, 11, 12, 6));
			return list;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(fridge, visited);
			visited.InterpolationEnabled = false;
			visited.fieldChangeVisibleEvent += delegate
			{
				InitializeBeds();
			};
		}

		public virtual void InitializeBeds()
		{
			if (!Game1.IsMasterGame || Game1.gameMode == 6 || !visited.Value)
			{
				return;
			}
			int num = 0;
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				num++;
			}
			int which = 2176;
			furniture.Add(new BedFurniture(which, new Vector2(22f, 3f)));
			num--;
			if (num > 0)
			{
				furniture.Add(new BedFurniture(which, new Vector2(26f, 3f)));
				num--;
			}
			for (int i = 0; i < Math.Min(6, num); i++)
			{
				int num2 = 3;
				int num3 = 3;
				if (i % 2 == 0)
				{
					num2 += 4;
				}
				num3 += i / 2 * 4;
				furniture.Add(new BedFurniture(which, new Vector2(num2, num3)));
			}
		}

		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			base.drawAboveFrontLayer(b);
			if (fridge.Value.mutex.IsLocked())
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(fridgePosition.X, fridgePosition.Y - 1) * 64f), new Microsoft.Xna.Framework.Rectangle(0, 192, 16, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((fridgePosition.Y + 1) * 64 + 1) / 10000f);
			}
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null)
			{
				int tileIndex = map.GetLayer("Buildings").Tiles[tileLocation].TileIndex;
				if (tileIndex == 258)
				{
					fridge.Value.fridge.Value = true;
					fridge.Value.checkForAction(who);
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}
	}
}
