using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData;
using StardewValley.Locations;

namespace StardewValley.Objects
{
	public class Wallpaper : Object
	{
		[XmlElement("sourceRect")]
		public readonly NetRectangle sourceRect = new NetRectangle();

		[XmlElement("isFloor")]
		public readonly NetBool isFloor = new NetBool(value: false);

		[XmlElement("sourceTexture")]
		public readonly NetString modDataID = new NetString(null);

		protected ModWallpaperOrFlooring _modData;

		private static readonly Rectangle wallpaperContainerRect = new Rectangle(39, 31, 16, 16);

		private static readonly Rectangle floorContainerRect = new Rectangle(55, 31, 16, 16);

		public static Texture2D wallpaperTexture;

		public override string Name => base.name;

		public Wallpaper()
		{
			base.NetFields.AddFields(sourceRect, isFloor, modDataID);
		}

		public Wallpaper(int which, bool isFloor = false)
			: this()
		{
			this.isFloor.Value = isFloor;
			base.ParentSheetIndex = which;
			base.name = (isFloor ? "Flooring" : "Wallpaper");
			sourceRect.Value = (isFloor ? new Rectangle(which % 8 * 32, 336 + which / 8 * 32, 28, 26) : new Rectangle(which % 16 * 16, which / 16 * 48 + 8, 16, 28));
			price.Value = 100;
		}

		public Wallpaper(string mod_id, int which)
			: this()
		{
			modDataID.Value = mod_id;
			base.ParentSheetIndex = which;
			if (GetModData() != null)
			{
				isFloor.Value = GetModData().IsFlooring;
			}
			else
			{
				modDataID.Value = null;
			}
			sourceRect.Value = (isFloor ? new Rectangle(which % 8 * 32, 336 + which / 8 * 32, 28, 26) : new Rectangle(which % 16 * 16, which / 16 * 48 + 8, 16, 28));
			if (GetModData() != null && isFloor.Value)
			{
				sourceRect.Y = which / 8 * 32;
			}
			base.name = (isFloor ? "Flooring" : "Wallpaper");
			price.Value = 100;
		}

		public virtual ModWallpaperOrFlooring GetModData()
		{
			if (modDataID.Value == null)
			{
				return null;
			}
			if (_modData != null)
			{
				return _modData;
			}
			List<ModWallpaperOrFlooring> list = Game1.content.Load<List<ModWallpaperOrFlooring>>("Data\\AdditionalWallpaperFlooring");
			foreach (ModWallpaperOrFlooring item in list)
			{
				if (item.ID == modDataID.Value)
				{
					_modData = item;
					return item;
				}
			}
			return null;
		}

		protected override string loadDisplayName()
		{
			if (!isFloor)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13204");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13203");
		}

		public override string getDescription()
		{
			if (!isFloor)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13206");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13205");
		}

		public override bool performDropDownAction(Farmer who)
		{
			return true;
		}

		public override bool performObjectDropInAction(Item dropIn, bool probe, Farmer who)
		{
			return false;
		}

		public override bool canBePlacedHere(GameLocation l, Vector2 tile)
		{
			Vector2 vector = tile * 64f;
			vector.X += 32f;
			vector.Y += 32f;
			foreach (Furniture item in l.furniture)
			{
				if ((int)item.furniture_type != 12 && item.getBoundingBox(item.tileLocation).Contains((int)vector.X, (int)vector.Y))
				{
					return false;
				}
			}
			return true;
		}

		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			location.tapToMove.Reset();
			if (who == null)
			{
				who = Game1.player;
			}
			if (who.currentLocation is DecoratableLocation)
			{
				Point point = new Point(x / 64, y / 64);
				DecoratableLocation decoratableLocation = who.currentLocation as DecoratableLocation;
				if ((bool)isFloor)
				{
					string floorID = decoratableLocation.GetFloorID(point.X, point.Y);
					if (floorID != null)
					{
						if (GetModData() != null)
						{
							decoratableLocation.SetFloor(GetModData().ID + ":" + parentSheetIndex.ToString(), floorID);
						}
						else
						{
							decoratableLocation.SetFloor(parentSheetIndex.ToString(), floorID);
						}
						location.playSound("coin");
						return true;
					}
				}
				else
				{
					string wallpaperID = decoratableLocation.GetWallpaperID(point.X, point.Y);
					if (wallpaperID != null)
					{
						if (GetModData() != null)
						{
							decoratableLocation.SetWallpaper(GetModData().ID + ":" + parentSheetIndex.ToString(), wallpaperID);
						}
						else
						{
							decoratableLocation.SetWallpaper(parentSheetIndex.ToString(), wallpaperID);
						}
						location.playSound("coin");
						return true;
					}
				}
			}
			return false;
		}

		public bool canReallyBePlacedHere(DecoratableLocation location, Vector2 tileLocation)
		{
			int x = (int)tileLocation.X;
			int y = (int)tileLocation.Y;
			DecoratableLocation decoratableLocation = Game1.player.currentLocation as DecoratableLocation;
			if ((bool)isFloor)
			{
				List<Rectangle> floors = decoratableLocation.getFloors();
				for (int i = 0; i < floors.Count; i++)
				{
					if (floors[i].Contains(x, y))
					{
						return true;
					}
				}
			}
			else
			{
				List<Rectangle> walls = decoratableLocation.getWalls();
				for (int j = 0; j < walls.Count; j++)
				{
					if (walls[j].Contains(x, y))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override bool isPlaceable()
		{
			return true;
		}

		public override Rectangle getBoundingBox(Vector2 tileLocation)
		{
			return boundingBox;
		}

		public override int salePrice()
		{
			return price;
		}

		public override int maximumStackSize()
		{
			return 1;
		}

		public override int addToStack(Item stack)
		{
			return 1;
		}

		public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
			drawInMenu(spriteBatch, objectPosition, 1f);
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			if (wallpaperTexture == null)
			{
				wallpaperTexture = Game1.content.Load<Texture2D>("Maps\\walls_and_floors");
			}
			if ((bool)isFloor)
			{
				spriteBatch.Draw(Game1.mouseCursors2, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2), floorContainerRect, Color.White * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(wallpaperTexture, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2 - 2), sourceRect, Color.White * transparency, 0f, new Vector2(14f, 13f), 2f * scaleSize, SpriteEffects.None, layerDepth + 0.001f);
			}
			else
			{
				spriteBatch.Draw(Game1.mouseCursors2, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2), wallpaperContainerRect, Color.White * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(wallpaperTexture, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2), sourceRect, Color.White * transparency, 0f, new Vector2(8f, 14f), 2f * scaleSize, SpriteEffects.None, layerDepth + 0.001f);
			}
		}

		public override Item getOne()
		{
			Wallpaper wallpaper = ((GetModData() == null) ? new Wallpaper(parentSheetIndex, isFloor) : new Wallpaper(GetModData().ID, parentSheetIndex));
			wallpaper._GetOneFrom(this);
			return wallpaper;
		}
	}
}
