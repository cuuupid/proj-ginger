using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Buildings
{
	public class GreenhouseBuilding : Building
	{
		protected Farm _farm;

		public GreenhouseBuilding(BluePrint b, Vector2 tileLocation)
			: base(b, tileLocation)
		{
		}

		protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
		{
			return null;
		}

		public GreenhouseBuilding()
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
		}

		public override void drawInMenu(SpriteBatch b, int x, int y)
		{
			Microsoft.Xna.Framework.Rectangle sourceRect = getSourceRect();
			y += 336;
			int num = 22;
			sourceRect.Height -= num;
			sourceRect.Y += num / 2;
			b.Draw(texture.Value, new Vector2(x, y), sourceRect, color, 0f, new Vector2(0f, sourceRect.Height / 2), 4f, SpriteEffects.None, 0.89f);
		}

		public override Microsoft.Xna.Framework.Rectangle getSourceRect()
		{
			return new Microsoft.Xna.Framework.Rectangle(0, 160, 112, 160);
		}

		public override void Update(GameTime time)
		{
			base.Update(time);
		}

		public override void drawInConstruction(SpriteBatch b)
		{
			float layerDepth = (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f;
			Microsoft.Xna.Framework.Rectangle sourceRect = getSourceRect();
			sourceRect.Y += sourceRect.Height;
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64)), sourceRect, color.Value * alpha, 0f, new Vector2(0f, sourceRect.Height), 4f, SpriteEffects.None, layerDepth);
		}

		public override void drawBackground(SpriteBatch b)
		{
			base.drawBackground(b);
			if (!base.isMoving)
			{
				DrawEntranceTiles(b);
				drawShadow(b);
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (base.isMoving)
			{
				return;
			}
			if ((int)daysOfConstructionLeft > 0 || (int)newConstructionTimer > 0)
			{
				drawInConstruction(b);
				return;
			}
			float layerDepth = (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f;
			Microsoft.Xna.Framework.Rectangle sourceRect = getSourceRect();
			if (!GetFarm().greenhouseUnlocked.Value)
			{
				sourceRect.Y -= sourceRect.Height;
			}
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64)), sourceRect, color.Value * alpha, 0f, new Vector2(0f, sourceRect.Height), 4f, SpriteEffects.None, layerDepth);
		}

		public Farm GetFarm()
		{
			if (_farm == null)
			{
				_farm = Game1.getFarm();
			}
			return _farm;
		}

		public override string isThereAnythingtoPreventConstruction(GameLocation location, Vector2 tile_position)
		{
			return null;
		}

		public override bool doesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
		{
			if (base.isMoving)
			{
				return false;
			}
			if (tile_x == (int)tileX + humanDoor.X && tile_y == (int)tileY + humanDoor.Y && layer_name == "Buildings" && property_name == "Action")
			{
				property_value = "WarpGreenhouse";
				return true;
			}
			if ((tile_x >= (int)tileX - 1 && tile_x <= (int)tileX + (int)tilesWide - 1 && tile_y <= (int)tileY + (int)tilesHigh && tile_y >= (int)tileY) || (CanDrawEntranceTiles() && tile_x >= (int)tileX + 1 && tile_x <= (int)tileX + (int)tilesWide - 2 && tile_y == (int)tileY + (int)tilesHigh + 1))
			{
				if (CanDrawEntranceTiles() && tile_x >= (int)tileX + humanDoor.X - 1 && tile_x <= (int)tileX + humanDoor.X + 1 && tile_y <= (int)tileY + (int)tilesHigh + 1 && tile_y >= (int)tileY + humanDoor.Y + 1)
				{
					if (property_name == "Type" && layer_name == "Back")
					{
						property_value = "Stone";
						return true;
					}
					if (property_name == "NoSpawn" && layer_name == "Back")
					{
						property_value = "All";
						return true;
					}
					if (property_name == "Buildable" && layer_name == "Back")
					{
						property_value = null;
						return true;
					}
				}
				if (property_name == "Buildable" && layer_name == "Back")
				{
					property_value = "T";
					return true;
				}
				if (property_name == "NoSpawn" && layer_name == "Back")
				{
					property_value = "Tree";
					return true;
				}
				if (property_name == "Diggable" && layer_name == "Back")
				{
					property_value = null;
					return true;
				}
			}
			return base.doesTileHaveProperty(tile_x, tile_y, property_name, layer_name, ref property_value);
		}

		public override int GetAdditionalTilePropertyRadius()
		{
			return 2;
		}

		public virtual bool CanDrawEntranceTiles()
		{
			return true;
		}

		public virtual void DrawEntranceTiles(SpriteBatch b)
		{
			Map map = GetFarm().Map;
			Layer layer = map.GetLayer("Back");
			TileSheet tileSheet = map.GetTileSheet("untitled tile sheet");
			if (tileSheet == null)
			{
				tileSheet = map.TileSheets[Math.Min(1, map.TileSheets.Count - 1)];
			}
			if (tileSheet != null)
			{
				Vector2 zero = Vector2.Zero;
				Location location = new Location(0, 0);
				StaticTile tile = new StaticTile(layer, tileSheet, BlendMode.Alpha, 812);
				if (CanDrawEntranceTiles())
				{
					float layerDepth = 0f;
					zero = Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX + humanDoor.Value.X - 1, (int)tileY + humanDoor.Value.Y + 1) * 64f);
					location.X = (int)zero.X;
					location.Y = (int)zero.Y;
					Game1.mapDisplayDevice.DrawTile(tile, location, layerDepth);
					location.X += 64;
					Game1.mapDisplayDevice.DrawTile(tile, location, layerDepth);
					location.X += 64;
					Game1.mapDisplayDevice.DrawTile(tile, location, layerDepth);
					tile = new StaticTile(layer, tileSheet, BlendMode.Alpha, 838);
					zero = Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX + humanDoor.Value.X - 1, (int)tileY + humanDoor.Value.Y + 2) * 64f);
					location.X = (int)zero.X;
					location.Y = (int)zero.Y;
					Game1.mapDisplayDevice.DrawTile(tile, location, layerDepth);
					location.X += 64;
					Game1.mapDisplayDevice.DrawTile(tile, location, layerDepth);
					location.X += 64;
					Game1.mapDisplayDevice.DrawTile(tile, location, layerDepth);
				}
			}
		}

		public override void drawShadow(SpriteBatch b, int localX = -1, int localY = -1)
		{
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(112, 0, 128, 144);
			if (CanDrawEntranceTiles())
			{
				value.Y = 144;
			}
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(((int)tileX - 1) * 64, (int)tileY * 64)), value, Color.White * ((localX == -1) ? ((float)alpha) : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
		}
	}
}
