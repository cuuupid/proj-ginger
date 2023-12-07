using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley
{
	public class MapSeat : INetObject<NetFields>, ISittable
	{
		[XmlIgnore]
		public static Texture2D mapChairTexture;

		[XmlIgnore]
		public NetLongDictionary<int, NetInt> sittingFarmers = new NetLongDictionary<int, NetInt>();

		[XmlIgnore]
		public NetVector2 tilePosition = new NetVector2();

		[XmlIgnore]
		public NetVector2 size = new NetVector2();

		[XmlIgnore]
		public NetInt direction = new NetInt();

		[XmlIgnore]
		public NetVector2 drawTilePosition = new NetVector2(new Vector2(-1f, -1f));

		[XmlIgnore]
		public NetBool seasonal = new NetBool();

		[XmlIgnore]
		public NetString seatType = new NetString();

		[XmlIgnore]
		public NetString textureFile = new NetString(null);

		[XmlIgnore]
		public string _loadedTextureFile;

		[XmlIgnore]
		public Texture2D overlayTexture;

		[XmlIgnore]
		public int localSittingDirection = 2;

		[XmlIgnore]
		public Vector3? customDrawValues;

		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields();


		public MapSeat()
		{
			NetFields.AddFields(sittingFarmers, tilePosition, size, direction, drawTilePosition, seasonal, seatType, textureFile);
		}

		public static MapSeat FromData(string data, int x, int y)
		{
			MapSeat mapSeat = new MapSeat();
			try
			{
				string[] array = data.Split('/');
				mapSeat.tilePosition.Set(new Vector2(x, y));
				mapSeat.size.Set(new Vector2(int.Parse(array[0]), int.Parse(array[1])));
				mapSeat.seatType.Value = array[3];
				if (array[2] == "right")
				{
					mapSeat.direction.Value = 1;
				}
				else if (array[2] == "left")
				{
					mapSeat.direction.Value = 3;
				}
				else if (array[2] == "down")
				{
					mapSeat.direction.Value = 2;
				}
				else if (array[2] == "up")
				{
					mapSeat.direction.Value = 0;
				}
				else if (array[2] == "opposite")
				{
					mapSeat.direction.Value = -2;
				}
				mapSeat.drawTilePosition.Set(new Vector2(int.Parse(array[4]), int.Parse(array[5])));
				mapSeat.seasonal.Value = array[6] == "true";
				if (array.Length > 7)
				{
					mapSeat.textureFile.Value = array[7];
					return mapSeat;
				}
				mapSeat.textureFile.Value = null;
				return mapSeat;
			}
			catch (Exception)
			{
				return mapSeat;
			}
		}

		public bool IsBlocked(GameLocation location)
		{
			Rectangle seatBounds = GetSeatBounds();
			seatBounds.X *= 64;
			seatBounds.Y *= 64;
			seatBounds.Width *= 64;
			seatBounds.Height *= 64;
			Rectangle value = seatBounds;
			if ((int)direction == 0)
			{
				value.Y -= 32;
				value.Height += 32;
			}
			else if ((int)direction == 2)
			{
				value.Height += 32;
			}
			if ((int)direction == 3)
			{
				value.X -= 32;
				value.Width += 32;
			}
			else if ((int)direction == 1)
			{
				value.Width += 32;
			}
			foreach (NPC character in location.characters)
			{
				Rectangle boundingBox = character.GetBoundingBox();
				if (boundingBox.Intersects(seatBounds))
				{
					return true;
				}
				if (!character.isMovingOnPathFindPath.Value && boundingBox.Intersects(value))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsSittingHere(Farmer who)
		{
			if (sittingFarmers.ContainsKey(who.UniqueMultiplayerID))
			{
				return true;
			}
			return false;
		}

		public bool HasSittingFarmers()
		{
			return sittingFarmers.Count() > 0;
		}

		public List<Vector2> GetSeatPositions(bool ignore_offsets = false)
		{
			customDrawValues = null;
			List<Vector2> list = new List<Vector2>();
			if (seatType.Value.StartsWith("custom "))
			{
				float x = 0f;
				float y = 0f;
				float z = 0f;
				string[] array = seatType.Value.Split(' ');
				try
				{
					if (array.Length > 1)
					{
						x = float.Parse(array[1]);
					}
					if (array.Length > 2)
					{
						y = float.Parse(array[2]);
					}
					if (array.Length > 3)
					{
						z = float.Parse(array[3]);
					}
				}
				catch (Exception)
				{
				}
				customDrawValues = new Vector3(x, y, z);
				Vector2 item = new Vector2(tilePosition.X + customDrawValues.Value.X, tilePosition.Y);
				if (!ignore_offsets)
				{
					item.Y += customDrawValues.Value.Y;
				}
				list.Add(item);
			}
			else if (seatType.Value == "playground")
			{
				Vector2 item2 = new Vector2(tilePosition.X + 0.75f, tilePosition.Y);
				if (!ignore_offsets)
				{
					item2.Y -= 0.1f;
				}
				list.Add(item2);
			}
			else if (seatType.Value == "ccdesk")
			{
				Vector2 item3 = new Vector2(tilePosition.X + 0.5f, tilePosition.Y);
				if (!ignore_offsets)
				{
					item3.Y -= 0.4f;
				}
				list.Add(item3);
			}
			else
			{
				for (int i = 0; (float)i < size.X; i++)
				{
					for (int j = 0; (float)j < size.Y; j++)
					{
						Vector2 vector = new Vector2(0f, 0f);
						if (seatType.Value.StartsWith("bench"))
						{
							if (direction.Value == 2)
							{
								vector.Y += 0.25f;
							}
							else if ((direction.Value == 3 || direction.Value == 1) && j == 0)
							{
								vector.Y += 0.5f;
							}
						}
						if (seatType.Value.StartsWith("picnic"))
						{
							if (direction.Value == 2)
							{
								vector.Y -= 0.25f;
							}
							else if (direction.Value == 0)
							{
								vector.Y += 0.25f;
							}
						}
						if (seatType.Value.EndsWith("swings"))
						{
							vector.Y -= 0.5f;
						}
						if (seatType.Value.EndsWith("summitbench"))
						{
							vector.Y -= 0.2f;
						}
						if (seatType.Value.EndsWith("tall"))
						{
							vector.Y -= 0.3f;
						}
						if (seatType.Value.EndsWith("short"))
						{
							vector.Y += 0.3f;
						}
						if (ignore_offsets)
						{
							vector = Vector2.Zero;
						}
						list.Add(tilePosition.Value + new Vector2((float)i + vector.X, (float)j + vector.Y));
					}
				}
			}
			return list;
		}

		public virtual void Draw(SpriteBatch b)
		{
			if (_loadedTextureFile != textureFile.Value)
			{
				_loadedTextureFile = textureFile.Value;
				try
				{
					overlayTexture = Game1.content.Load<Texture2D>(_loadedTextureFile);
				}
				catch (Exception)
				{
					overlayTexture = null;
				}
			}
			if (overlayTexture == null)
			{
				overlayTexture = mapChairTexture;
			}
			if (!(drawTilePosition.Value.X >= 0f) || !HasSittingFarmers())
			{
				return;
			}
			float num = 0f;
			if (customDrawValues.HasValue)
			{
				num = customDrawValues.Value.Z;
			}
			else if (seatType.Value.StartsWith("highback_chair") || seatType.Value.StartsWith("ccdesk"))
			{
				num = 1f;
			}
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(tilePosition.X * 64f, (tilePosition.Y - num) * 64f));
			float layerDepth = (float)(((double)((float)(int)tilePosition.Y + size.Y) + 0.1) * 64.0) / 10000f;
			Rectangle value = new Rectangle((int)drawTilePosition.Value.X * 16, (int)(drawTilePosition.Value.Y - num) * 16, (int)size.Value.X * 16, (int)(size.Value.Y + num) * 16);
			if (seasonal.Value)
			{
				if (Game1.currentLocation.GetSeasonForLocation() == "summer")
				{
					value.X += value.Width;
				}
				else if (Game1.currentLocation.GetSeasonForLocation() == "fall")
				{
					value.X += value.Width * 2;
				}
				else if (Game1.currentLocation.GetSeasonForLocation() == "winter")
				{
					value.X += value.Width * 3;
				}
			}
			b.Draw(overlayTexture, position, value, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
		}

		public bool OccupiesTile(int x, int y)
		{
			return GetSeatBounds().Contains(x, y);
		}

		public virtual Vector2? AddSittingFarmer(Farmer who)
		{
			if (who == Game1.player)
			{
				localSittingDirection = direction.Value;
				if (seatType.Value.StartsWith("stool"))
				{
					localSittingDirection = Game1.player.FacingDirection;
				}
				if (direction.Value == -2)
				{
					localSittingDirection = Utility.GetOppositeFacingDirection(Game1.player.FacingDirection);
				}
				if (seatType.Value.StartsWith("bathchair") && localSittingDirection == 0)
				{
					localSittingDirection = 2;
				}
			}
			List<Vector2> seatPositions = GetSeatPositions();
			int value = -1;
			Vector2? result = null;
			float num = 96f;
			for (int i = 0; i < seatPositions.Count; i++)
			{
				if (!sittingFarmers.Values.Contains(i))
				{
					float num2 = ((seatPositions[i] + new Vector2(0.5f, 0.5f)) * 64f - who.getStandingPosition()).Length();
					if (num2 < num)
					{
						num = num2;
						result = seatPositions[i];
						value = i;
					}
				}
			}
			if (result.HasValue)
			{
				sittingFarmers[who.UniqueMultiplayerID] = value;
			}
			return result;
		}

		public bool IsSeatHere(GameLocation location)
		{
			return location.mapSeats.Contains(this);
		}

		public int GetSittingDirection()
		{
			return localSittingDirection;
		}

		public Vector2? GetSittingPosition(Farmer who, bool ignore_offsets = false)
		{
			if (sittingFarmers.ContainsKey(who.UniqueMultiplayerID))
			{
				return GetSeatPositions(ignore_offsets)[sittingFarmers[who.UniqueMultiplayerID]];
			}
			return null;
		}

		public virtual Rectangle GetSeatBounds()
		{
			if (seatType.Value == "chair" && (int)direction == 0)
			{
				new Rectangle((int)tilePosition.X, (int)tilePosition.Y + 1, (int)size.X, (int)size.Y - 1);
			}
			return new Rectangle((int)tilePosition.X, (int)tilePosition.Y, (int)size.X, (int)size.Y);
		}

		public virtual void RemoveSittingFarmer(Farmer farmer)
		{
			sittingFarmers.Remove(farmer.UniqueMultiplayerID);
		}

		public virtual int GetSittingFarmerCount()
		{
			return sittingFarmers.Count();
		}
	}
}
