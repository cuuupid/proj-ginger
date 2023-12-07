using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley.Locations
{
	public class DecoratableLocation : GameLocation
	{
		public readonly DecorationFacade wallPaper = new DecorationFacade();

		[XmlIgnore]
		public readonly NetStringList wallpaperIDs = new NetStringList();

		public readonly NetStringDictionary<string, NetString> appliedWallpaper = new NetStringDictionary<string, NetString>();

		[XmlIgnore]
		public readonly Dictionary<string, List<Vector3>> wallpaperTiles = new Dictionary<string, List<Vector3>>();

		public readonly DecorationFacade floor = new DecorationFacade();

		[XmlIgnore]
		public readonly NetStringList floorIDs = new NetStringList();

		public readonly NetStringDictionary<string, NetString> appliedFloor = new NetStringDictionary<string, NetString>();

		[XmlIgnore]
		public readonly Dictionary<string, List<Vector3>> floorTiles = new Dictionary<string, List<Vector3>>();

		protected Dictionary<string, TileSheet> _wallAndFloorTileSheets = new Dictionary<string, TileSheet>();

		protected Map _wallAndFloorTileSheetMap;

		protected override void initNetFields()
		{
			base.initNetFields();
			appliedWallpaper.InterpolationWait = false;
			appliedFloor.InterpolationWait = false;
			base.NetFields.AddFields(appliedWallpaper, appliedFloor, floorIDs, wallpaperIDs);
			appliedWallpaper.OnValueAdded += delegate(string key, string value)
			{
				UpdateWallpaper(key);
			};
			appliedWallpaper.OnConflictResolve += delegate(string key, NetString rejected, NetString accepted)
			{
				UpdateWallpaper(key);
			};
			appliedWallpaper.OnValueTargetUpdated += delegate(string key, string old_value, string new_value)
			{
				if (appliedWallpaper.FieldDict.ContainsKey(key))
				{
					appliedWallpaper.FieldDict[key].CancelInterpolation();
				}
				UpdateWallpaper(key);
			};
			appliedFloor.OnValueAdded += delegate(string key, string value)
			{
				UpdateFloor(key);
			};
			appliedFloor.OnConflictResolve += delegate(string key, NetString rejected, NetString accepted)
			{
				UpdateFloor(key);
			};
			appliedFloor.OnValueTargetUpdated += delegate(string key, string old_value, string new_value)
			{
				if (appliedFloor.FieldDict.ContainsKey(key))
				{
					appliedFloor.FieldDict[key].CancelInterpolation();
				}
				UpdateFloor(key);
			};
		}

		public DecoratableLocation()
		{
		}

		public DecoratableLocation(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		public virtual void ReadWallpaperAndFloorTileData()
		{
			updateMap();
			wallpaperTiles.Clear();
			floorTiles.Clear();
			wallpaperIDs.Clear();
			floorIDs.Clear();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			if (map.Properties.ContainsKey("WallIDs"))
			{
				string[] array = map.Properties["WallIDs"].ToString().Split(',');
				string[] array2 = array;
				foreach (string text in array2)
				{
					string[] array3 = text.Trim().Split(' ');
					if (array3.Length >= 1)
					{
						wallpaperIDs.Add(array3[0]);
					}
					if (array3.Length >= 2)
					{
						dictionary[array3[0]] = array3[1];
					}
				}
			}
			if (wallpaperIDs.Count == 0)
			{
				List<Microsoft.Xna.Framework.Rectangle> walls = getWalls();
				for (int j = 0; j < walls.Count; j++)
				{
					string text2 = "Wall_" + j;
					wallpaperIDs.Add(text2);
					Microsoft.Xna.Framework.Rectangle rectangle = walls[j];
					if (!wallpaperTiles.ContainsKey(j.ToString()))
					{
						wallpaperTiles[text2] = new List<Vector3>();
					}
					for (int k = rectangle.Left; k < rectangle.Right; k++)
					{
						for (int l = rectangle.Top; l < rectangle.Bottom; l++)
						{
							wallpaperTiles[text2].Add(new Vector3(k, l, l - rectangle.Top));
						}
					}
				}
			}
			else
			{
				for (int m = 0; m < map.Layers[0].LayerWidth; m++)
				{
					for (int n = 0; n < map.Layers[0].LayerHeight; n++)
					{
						string text3 = doesTileHaveProperty(m, n, "WallID", "Back");
						int tileIndexAt = getTileIndexAt(new Point(m, n), "Back");
						if (text3 == null)
						{
							continue;
						}
						if (!wallpaperIDs.Contains(text3))
						{
							wallpaperIDs.Add(text3);
						}
						if (!appliedWallpaper.ContainsKey(text3))
						{
							appliedWallpaper[text3] = "0";
							if (dictionary.ContainsKey(text3))
							{
								string text4 = dictionary[text3];
								if (appliedWallpaper.ContainsKey(text4))
								{
									appliedWallpaper[text3] = appliedWallpaper[text4];
								}
								else if (GetWallpaperSource(text4).Value >= 0)
								{
									appliedWallpaper[text3] = text4;
								}
							}
						}
						if (!wallpaperTiles.ContainsKey(text3))
						{
							wallpaperTiles[text3] = new List<Vector3>();
						}
						wallpaperTiles[text3].Add(new Vector3(m, n, 0f));
						string tileSheetIDAt = getTileSheetIDAt(m, n, "Back");
						TileSheet tileSheet = map.GetTileSheet(tileSheetIDAt);
						int sheetWidth = tileSheet.SheetWidth;
						if (IsFloorableOrWallpaperableTile(m, n + 1, "Back"))
						{
							wallpaperTiles[text3].Add(new Vector3(m, n + 1, 1f));
						}
						if (IsFloorableOrWallpaperableTile(m, n + 2, "Buildings"))
						{
							wallpaperTiles[text3].Add(new Vector3(m, n + 2, 2f));
						}
						else if (IsFloorableOrWallpaperableTile(m, n + 2, "Back") && !IsFloorableTile(m, n + 2, "Back"))
						{
							wallpaperTiles[text3].Add(new Vector3(m, n + 2, 2f));
						}
					}
				}
			}
			dictionary.Clear();
			if (map.Properties.ContainsKey("FloorIDs"))
			{
				string[] array4 = map.Properties["FloorIDs"].ToString().Split(',');
				string[] array5 = array4;
				foreach (string text5 in array5)
				{
					string[] array6 = text5.Trim().Split(' ');
					if (array6.Length >= 1)
					{
						floorIDs.Add(array6[0]);
					}
					if (array6.Length >= 2)
					{
						dictionary[array6[0]] = array6[1];
					}
				}
			}
			if (floorIDs.Count == 0)
			{
				List<Microsoft.Xna.Framework.Rectangle> floors = getFloors();
				for (int num2 = 0; num2 < floors.Count; num2++)
				{
					string text6 = "Floor_" + num2;
					floorIDs.Add(text6);
					Microsoft.Xna.Framework.Rectangle rectangle2 = floors[num2];
					if (!floorTiles.ContainsKey(num2.ToString()))
					{
						floorTiles[text6] = new List<Vector3>();
					}
					for (int num3 = rectangle2.Left; num3 < rectangle2.Right; num3++)
					{
						for (int num4 = rectangle2.Top; num4 < rectangle2.Bottom; num4++)
						{
							floorTiles[text6].Add(new Vector3(num3, num4, 0f));
						}
					}
				}
			}
			else
			{
				for (int num5 = 0; num5 < map.Layers[0].LayerWidth; num5++)
				{
					for (int num6 = 0; num6 < map.Layers[0].LayerHeight; num6++)
					{
						string text7 = doesTileHaveProperty(num5, num6, "FloorID", "Back");
						int tileIndexAt2 = getTileIndexAt(new Point(num5, num6), "Back");
						if (text7 == null)
						{
							continue;
						}
						if (!floorIDs.Contains(text7))
						{
							floorIDs.Add(text7);
						}
						if (!appliedFloor.ContainsKey(text7))
						{
							appliedFloor[text7] = "0";
							if (dictionary.ContainsKey(text7))
							{
								string text8 = dictionary[text7];
								if (appliedFloor.ContainsKey(text8))
								{
									appliedFloor[text7] = appliedFloor[text8];
								}
								else if (GetFloorSource(text8).Value >= 0)
								{
									appliedFloor[text7] = text8;
								}
							}
						}
						if (!floorTiles.ContainsKey(text7))
						{
							floorTiles[text7] = new List<Vector3>();
						}
						floorTiles[text7].Add(new Vector3(num5, num6, 0f));
					}
				}
			}
			setFloors();
			setWallpapers();
		}

		public virtual TileSheet GetWallAndFloorTilesheet(string id)
		{
			if (map != _wallAndFloorTileSheetMap)
			{
				_wallAndFloorTileSheets.Clear();
				_wallAndFloorTileSheetMap = map;
			}
			if (_wallAndFloorTileSheets.ContainsKey(id))
			{
				return _wallAndFloorTileSheets[id];
			}
			try
			{
				List<ModWallpaperOrFlooring> list = Game1.content.Load<List<ModWallpaperOrFlooring>>("Data\\AdditionalWallpaperFlooring");
				ModWallpaperOrFlooring modWallpaperOrFlooring = null;
				foreach (ModWallpaperOrFlooring item in list)
				{
					if (item.ID == id)
					{
						modWallpaperOrFlooring = item;
						break;
					}
				}
				if (modWallpaperOrFlooring != null)
				{
					Texture2D texture2D = Game1.content.Load<Texture2D>(modWallpaperOrFlooring.Texture);
					int num = texture2D.Width / 16;
					if (num != 16)
					{
						Console.WriteLine("WARNING: Wallpaper/floor tilesheets must be 16 tiles wide.");
					}
					TileSheet tileSheet = new TileSheet("x_WallsAndFloors_" + id, map, modWallpaperOrFlooring.Texture, new Size(texture2D.Width / 16, texture2D.Height / 16), new Size(16, 16));
					map.AddTileSheet(tileSheet);
					map.LoadTileSheets(Game1.mapDisplayDevice);
					_wallAndFloorTileSheets[id] = tileSheet;
					return tileSheet;
				}
				Console.WriteLine("Error trying to load wallpaper/floor tilesheet: " + id);
				_wallAndFloorTileSheets[id] = null;
				return null;
			}
			catch (Exception)
			{
				Console.WriteLine("Error trying to load wallpaper/floor tilesheet: " + id);
				_wallAndFloorTileSheets[id] = null;
				return null;
			}
		}

		public virtual KeyValuePair<int, int> GetFloorSource(string pattern_id)
		{
			int result = -1;
			if (pattern_id.Contains(":"))
			{
				string[] array = pattern_id.Split(':');
				TileSheet wallAndFloorTilesheet = GetWallAndFloorTilesheet(array[0]);
				if (int.TryParse(array[1], out result) && wallAndFloorTilesheet != null)
				{
					return new KeyValuePair<int, int>(map.TileSheets.IndexOf(wallAndFloorTilesheet), result);
				}
			}
			if (int.TryParse(pattern_id, out result))
			{
				TileSheet tileSheet = map.GetTileSheet("walls_and_floors");
				int key = map.TileSheets.IndexOf(tileSheet);
				return new KeyValuePair<int, int>(key, result);
			}
			return new KeyValuePair<int, int>(-1, -1);
		}

		public virtual KeyValuePair<int, int> GetWallpaperSource(string pattern_id)
		{
			int result = -1;
			if (pattern_id.Contains(":"))
			{
				string[] array = pattern_id.Split(':');
				TileSheet wallAndFloorTilesheet = GetWallAndFloorTilesheet(array[0]);
				if (int.TryParse(array[1], out result) && wallAndFloorTilesheet != null)
				{
					return new KeyValuePair<int, int>(map.TileSheets.IndexOf(wallAndFloorTilesheet), result);
				}
			}
			if (int.TryParse(pattern_id, out result))
			{
				TileSheet tileSheet = map.GetTileSheet("walls_and_floors");
				int key = map.TileSheets.IndexOf(tileSheet);
				return new KeyValuePair<int, int>(key, result);
			}
			return new KeyValuePair<int, int>(-1, -1);
		}

		public virtual void UpdateFloor(string floor_id)
		{
			updateMap();
			if (!appliedFloor.ContainsKey(floor_id) || !floorTiles.ContainsKey(floor_id))
			{
				return;
			}
			string pattern_id = appliedFloor[floor_id];
			List<Vector3> list = floorTiles[floor_id];
			foreach (Vector3 item in list)
			{
				int num = (int)item.X;
				int num2 = (int)item.Y;
				KeyValuePair<int, int> floorSource = GetFloorSource(pattern_id);
				if (floorSource.Value < 0)
				{
					continue;
				}
				int key = floorSource.Key;
				int value = floorSource.Value;
				int sheetWidth = map.TileSheets[key].SheetWidth;
				string id = map.TileSheets[key].Id;
				string text = "Back";
				value = value * 2 + value / (sheetWidth / 2) * sheetWidth;
				if (id == "walls_and_floors")
				{
					value += GetFirstFlooringTile();
				}
				if (!IsFloorableOrWallpaperableTile(num, num2, text))
				{
					continue;
				}
				Tile tile = map.GetLayer(text).Tiles[num, num2];
				setMapTile(num, num2, GetFlooringIndex(value, num, num2), text, null, key);
				Tile tile2 = map.GetLayer(text).Tiles[num, num2];
				if (tile == null)
				{
					continue;
				}
				foreach (KeyValuePair<string, PropertyValue> property in tile.Properties)
				{
					tile2.Properties[property.Key] = property.Value;
				}
			}
		}

		public virtual void UpdateWallpaper(string wallpaper_id)
		{
			updateMap();
			if (!appliedWallpaper.ContainsKey(wallpaper_id) || !wallpaperTiles.ContainsKey(wallpaper_id))
			{
				return;
			}
			string pattern_id = appliedWallpaper[wallpaper_id];
			List<Vector3> list = wallpaperTiles[wallpaper_id];
			foreach (Vector3 item in list)
			{
				int num = (int)item.X;
				int num2 = (int)item.Y;
				int num3 = (int)item.Z;
				KeyValuePair<int, int> wallpaperSource = GetWallpaperSource(pattern_id);
				if (wallpaperSource.Value < 0)
				{
					continue;
				}
				int key = wallpaperSource.Key;
				int value = wallpaperSource.Value;
				int sheetWidth = map.TileSheets[key].SheetWidth;
				string text = "Back";
				if (num3 == 2)
				{
					text = "Buildings";
					if (!IsFloorableOrWallpaperableTile(num, num2, "Buildings"))
					{
						text = "Back";
					}
				}
				if (!IsFloorableOrWallpaperableTile(num, num2, text))
				{
					continue;
				}
				Tile tile = map.GetLayer(text).Tiles[num, num2];
				setMapTile(num, num2, value / sheetWidth * sheetWidth * 3 + value % sheetWidth + num3 * sheetWidth, text, null, key);
				Tile tile2 = map.GetLayer(text).Tiles[num, num2];
				if (tile == null)
				{
					continue;
				}
				foreach (KeyValuePair<string, PropertyValue> property in tile.Properties)
				{
					tile2.Properties[property.Key] = property.Value;
				}
			}
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (!wasUpdated)
			{
				base.UpdateWhenCurrentLocation(time);
			}
		}

		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (!(this is FarmHouse))
			{
				ReadWallpaperAndFloorTileData();
				setWallpapers();
				setFloors();
			}
			if (getTileIndexAt(Game1.player.getTileX(), Game1.player.getTileY(), "Buildings") != -1)
			{
				Game1.player.position.Y += 64f;
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (!Game1.player.mailReceived.Contains("button_tut_1"))
			{
				Game1.player.mailReceived.Add("button_tut_1");
				Game1.onScreenMenus.Add(new ButtonTutorialMenu(0));
			}
		}

		public override void shiftObjects(int dx, int dy)
		{
			base.shiftObjects(dx, dy);
			foreach (Furniture item in furniture)
			{
				item.removeLights(this);
				item.tileLocation.X += dx;
				item.tileLocation.Y += dy;
				item.boundingBox.X += dx * 64;
				item.boundingBox.Y += dy * 64;
				item.updateDrawPosition();
				if (Game1.isDarkOut())
				{
					item.addLights(this);
				}
			}
			List<KeyValuePair<Vector2, TerrainFeature>> list = new List<KeyValuePair<Vector2, TerrainFeature>>(terrainFeatures.Pairs);
			terrainFeatures.Clear();
			foreach (KeyValuePair<Vector2, TerrainFeature> item2 in list)
			{
				terrainFeatures.Add(new Vector2(item2.Key.X + (float)dx, item2.Key.Y + (float)dy), item2.Value);
			}
		}

		public void moveFurniture(int oldX, int oldY, int newX, int newY)
		{
			Vector2 vector = new Vector2(oldX, oldY);
			foreach (Furniture item in furniture)
			{
				if (item.tileLocation.Equals(vector))
				{
					item.removeLights(this);
					item.tileLocation.Value = new Vector2(newX, newY);
					item.boundingBox.X = newX * 64;
					item.boundingBox.Y = newY * 64;
					item.updateDrawPosition();
					if (Game1.isDarkOut())
					{
						item.addLights(this);
					}
					return;
				}
			}
			if (objects.ContainsKey(vector))
			{
				Object @object = objects[vector];
				objects.Remove(vector);
				@object.tileLocation.Value = new Vector2(newX, newY);
				objects.Add(new Vector2(newX, newY), @object);
			}
		}

		public override bool CanFreePlaceFurniture()
		{
			return true;
		}

		public bool isTileOnWall(int x, int y)
		{
			foreach (string key in wallpaperTiles.Keys)
			{
				foreach (Vector3 item in wallpaperTiles[key])
				{
					if ((int)item.X == x && (int)item.Y == y)
					{
						return true;
					}
				}
			}
			return false;
		}

		public int GetWallTopY(int x, int y)
		{
			foreach (string key in wallpaperTiles.Keys)
			{
				foreach (Vector3 item in wallpaperTiles[key])
				{
					if ((int)item.X == x && (int)item.Y == y)
					{
						return y - (int)item.Z;
					}
				}
			}
			return -1;
		}

		public void setFloors()
		{
			foreach (KeyValuePair<string, string> pair in appliedFloor.Pairs)
			{
				UpdateFloor(pair.Key);
			}
		}

		public void setWallpapers()
		{
			foreach (KeyValuePair<string, string> pair in appliedWallpaper.Pairs)
			{
				UpdateWallpaper(pair.Key);
			}
		}

		public void SetFloor(string which, string which_room)
		{
			if (which_room == null)
			{
				foreach (string floorID in floorIDs)
				{
					appliedFloor[floorID] = which;
				}
				return;
			}
			appliedFloor[which_room] = which;
		}

		public void SetWallpaper(string which, string which_room)
		{
			if (which_room == null)
			{
				foreach (string wallpaperID in wallpaperIDs)
				{
					appliedWallpaper[wallpaperID] = which;
				}
				return;
			}
			appliedWallpaper[which_room] = which;
		}

		public string GetFloorID(int x, int y)
		{
			foreach (string key in floorTiles.Keys)
			{
				foreach (Vector3 item in floorTiles[key])
				{
					if ((int)item.X == x && (int)item.Y == y)
					{
						return key;
					}
				}
			}
			return null;
		}

		public string GetWallpaperID(int x, int y)
		{
			foreach (string key in wallpaperTiles.Keys)
			{
				foreach (Vector3 item in wallpaperTiles[key])
				{
					if ((int)item.X == x && (int)item.Y == y)
					{
						return key;
					}
				}
			}
			return null;
		}

		[Obsolete("Use string based SetFloor.")]
		public void setFloor(int which, int whichRoom = -1, bool persist = false)
		{
			string which_room = null;
			if (whichRoom >= 0 && whichRoom < floorIDs.Count)
			{
				which_room = floorIDs[whichRoom];
			}
			SetFloor(which.ToString(), which_room);
		}

		[Obsolete("Use string based SetWallpaper.")]
		public void setWallpaper(int which, int whichRoom = -1, bool persist = false)
		{
			string which_room = null;
			if (whichRoom >= 0 && whichRoom < wallpaperIDs.Count)
			{
				which_room = wallpaperIDs[whichRoom];
			}
			SetWallpaper(which.ToString(), which_room);
		}

		protected bool IsFloorableTile(int x, int y, string layer_name)
		{
			int tileIndexAt = getTileIndexAt(x, y, "Buildings");
			if (tileIndexAt >= 197 && tileIndexAt <= 199 && getTileSheetIDAt(x, y, "Buildings") == "untitled tile sheet")
			{
				return false;
			}
			return IsFloorableOrWallpaperableTile(x, y, layer_name);
		}

		public bool IsWallAndFloorTilesheet(string tilesheet_id)
		{
			if (tilesheet_id.StartsWith("x_WallsAndFloors_"))
			{
				return true;
			}
			return tilesheet_id == "walls_and_floors";
		}

		protected bool IsFloorableOrWallpaperableTile(int x, int y, string layer_name)
		{
			Layer layer = map.GetLayer(layer_name);
			if (layer != null && x < layer.LayerWidth && y < layer.LayerHeight && layer.Tiles[x, y] != null && layer.Tiles[x, y].TileSheet != null && IsWallAndFloorTilesheet(layer.Tiles[x, y].TileSheet.Id))
			{
				return true;
			}
			return false;
		}

		public override void drawFloorDecorations(SpriteBatch b)
		{
			base.drawFloorDecorations(b);
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			if (l is DecoratableLocation decoratableLocation)
			{
				if (!decoratableLocation.appliedWallpaper.Keys.Any())
				{
					ReadWallpaperAndFloorTileData();
					for (int i = 0; i < decoratableLocation.wallPaper.Count; i++)
					{
						try
						{
							string key = wallpaperIDs[i];
							string value = decoratableLocation.wallPaper[i].ToString();
							appliedWallpaper[key] = value;
						}
						catch (Exception)
						{
						}
					}
					for (int j = 0; j < decoratableLocation.floor.Count; j++)
					{
						try
						{
							string key2 = floorIDs[j];
							string value2 = decoratableLocation.floor[j].ToString();
							appliedFloor[key2] = value2;
						}
						catch (Exception)
						{
						}
					}
				}
				else
				{
					foreach (string key3 in decoratableLocation.appliedWallpaper.Keys)
					{
						appliedWallpaper[key3] = decoratableLocation.appliedWallpaper[key3];
					}
					foreach (string key4 in decoratableLocation.appliedFloor.Keys)
					{
						appliedFloor[key4] = decoratableLocation.appliedFloor[key4];
					}
				}
			}
			setWallpapers();
			setFloors();
			base.TransferDataFromSavedLocation(l);
		}

		public Furniture getRandomFurniture(Random r)
		{
			if (furniture.Count > 0)
			{
				return furniture.ElementAt(r.Next(furniture.Count));
			}
			return null;
		}

		public int getFloorAt(Point p)
		{
			foreach (string key in floorTiles.Keys)
			{
				foreach (Vector3 item in floorTiles[key])
				{
					if ((int)item.X == p.X && (int)item.Y == p.Y)
					{
						return floorIDs.IndexOf(key);
					}
				}
			}
			return -1;
		}

		public int getWallForRoomAt(Point p)
		{
			foreach (string key in wallpaperTiles.Keys)
			{
				foreach (Vector3 item in wallpaperTiles[key])
				{
					if ((int)item.X == p.X && (int)item.Y == p.Y)
					{
						return wallpaperIDs.IndexOf(key);
					}
				}
			}
			return -1;
		}

		public virtual int GetFirstFlooringTile()
		{
			return 336;
		}

		public virtual int GetFlooringIndex(int base_tile_sheet, int tile_x, int tile_y)
		{
			int num = getTileIndexAt(tile_x, tile_y, "Back");
			if (num < 0)
			{
				return 0;
			}
			string tileSheetIDAt = getTileSheetIDAt(tile_x, tile_y, "Back");
			TileSheet tileSheet = map.GetTileSheet(tileSheetIDAt);
			int num2 = 16;
			if (tileSheet != null)
			{
				num2 = tileSheet.SheetWidth;
			}
			if (tileSheetIDAt == "walls_and_floors")
			{
				num -= GetFirstFlooringTile();
			}
			int num3 = num % 2;
			int num4 = num % (num2 * 2) / num2;
			return base_tile_sheet + num3 + num2 * num4;
		}

		[Obsolete("Replaced by SetFloor.")]
		protected virtual void doSetVisibleFloor(int whichRoom, int which)
		{
			SetFloor(which.ToString(), whichRoom.ToString());
		}

		[Obsolete("Replaced by SetWallpaper.")]
		protected virtual void doSetVisibleWallpaper(int whichRoom, int which)
		{
			SetWallpaper(which.ToString(), whichRoom.ToString());
		}

		public virtual List<Microsoft.Xna.Framework.Rectangle> getFloors()
		{
			return new List<Microsoft.Xna.Framework.Rectangle>();
		}
	}
}
