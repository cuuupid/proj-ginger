using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley.BellsAndWhistles
{
	public class SandDuggy : INetObject<NetFields>
	{
		[XmlType("SandDuggy.State")]
		public enum State
		{
			DigUp,
			Idle,
			DigDown
		}

		[XmlIgnore]
		public NetList<Point, NetPoint> holeLocations = new NetList<Point, NetPoint>();

		[XmlIgnore]
		public int frame;

		[XmlIgnore]
		public NetInt currentHoleIndex = new NetInt(0);

		[XmlIgnore]
		public int _localIndex;

		[XmlIgnore]
		public NetLocationRef locationRef = new NetLocationRef();

		[XmlIgnore]
		public State currentState;

		[XmlIgnore]
		public Texture2D texture;

		[XmlIgnore]
		public float nextFrameUpdate;

		[XmlElement("whacked")]
		public NetBool whacked = new NetBool(value: false);

		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields();


		public SandDuggy()
		{
			InitNetFields();
		}

		public SandDuggy(GameLocation location, Point[] points)
			: this()
		{
			locationRef.Value = location;
			foreach (Point item in points)
			{
				holeLocations.Add(item);
			}
			currentHoleIndex.Value = FindRandomFreePoint();
		}

		public virtual int FindRandomFreePoint()
		{
			if (locationRef.Value == null)
			{
				return -1;
			}
			List<int> list = new List<int>();
			for (int i = 0; i < holeLocations.Count; i++)
			{
				Point p = holeLocations[i];
				if (!locationRef.Value.isObjectAtTile(p.X, p.Y) && !locationRef.Value.isTerrainFeatureAt(p.X, p.Y) && !locationRef.Value.terrainFeatures.ContainsKey(Utility.PointToVector2(p)))
				{
					list.Add(i);
				}
			}
			if (list.Count == 1)
			{
				return list[0];
			}
			for (int j = 0; j < list.Count; j++)
			{
				int index = list[j];
				Point location = holeLocations[index];
				bool flag = false;
				foreach (Farmer farmer in locationRef.Value.farmers)
				{
					if (NearFarmer(location, farmer))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					list.RemoveAt(j);
					j--;
				}
			}
			if (list.Count > 0)
			{
				return Utility.GetRandom(list);
			}
			return -1;
		}

		public virtual void InitNetFields()
		{
			NetFields.AddFields(holeLocations, currentHoleIndex, locationRef.NetFields, whacked);
			whacked.fieldChangeVisibleEvent += OnWhackedChanged;
		}

		public virtual void OnWhackedChanged(NetBool field, bool old_value, bool new_value)
		{
			if (Game1.gameMode == 6 || Utility.ShouldIgnoreValueChangeCallback() || !whacked.Value)
			{
				return;
			}
			if (Game1.IsMasterGame)
			{
				int num = currentHoleIndex.Value;
				if (num == -1)
				{
					num = 0;
				}
				Game1.player.team.MarkCollectedNut("SandDuggy");
				Game1.createItemDebris(new Object(73, 1), new Vector2(holeLocations[num].X, holeLocations[num].Y) * 64f, -1, locationRef.Value);
			}
			if (Game1.currentLocation == locationRef.Value)
			{
				AnimateWhacked();
			}
		}

		public virtual void AnimateWhacked()
		{
			if (Game1.currentLocation == locationRef.Value)
			{
				int num = currentHoleIndex.Value;
				if (num == -1)
				{
					num = 0;
				}
				Vector2 vector = new Vector2(holeLocations[num].X, holeLocations[num].Y);
				int ground_position = (int)(vector.Y * 64f - 32f);
				if (Utility.isOnScreen((vector + new Vector2(0.5f, 0.5f)) * 64f, 64))
				{
					Game1.playSound("axchop");
					Game1.playSound("rockGolemHit");
				}
				TemporaryAnimatedSprite duggy_sprite = new TemporaryAnimatedSprite("LooseSprites/SandDuggy", new Rectangle(0, 48, 16, 48), new Vector2(vector.X * 64f, vector.Y * 64f - 32f), flipped: false, 0f, Color.White)
				{
					motion = new Vector2(2f, -3f),
					acceleration = new Vector2(0f, 0.25f),
					interval = 1000f,
					animationLength = 1,
					alphaFade = 0.02f,
					layerDepth = 0.07682f,
					scale = 4f,
					yStopCoordinate = ground_position
				};
				duggy_sprite.reachedStopCoordinate = delegate
				{
					duggy_sprite.motion.Y = -3f;
					duggy_sprite.acceleration.Y = 0.25f;
					duggy_sprite.yStopCoordinate = ground_position;
					duggy_sprite.flipped = !duggy_sprite.flipped;
				};
				Game1.currentLocation.temporarySprites.Add(duggy_sprite);
			}
		}

		public virtual void ResetForPlayerEntry()
		{
			texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\SandDuggy");
		}

		public virtual void PerformToolAction(Tool tool, int tile_x, int tile_y)
		{
			if (currentState == State.Idle && _localIndex >= 0)
			{
				Point point = holeLocations[_localIndex];
				if (point.X == tile_x && point.Y == tile_y)
				{
					whacked.Value = true;
				}
			}
		}

		public virtual bool NearFarmer(Point location, Farmer farmer)
		{
			if (Math.Abs(location.X - farmer.getTileX()) <= 2 && Math.Abs(location.Y - farmer.getTileY()) <= 2)
			{
				return true;
			}
			return false;
		}

		public virtual void Update(GameTime time)
		{
			if (whacked.Value)
			{
				return;
			}
			if (currentHoleIndex.Value >= 0)
			{
				Point location = holeLocations[currentHoleIndex.Value];
				if (NearFarmer(location, Game1.player) && FindRandomFreePoint() != (int)currentHoleIndex)
				{
					currentHoleIndex.Value = -1;
					DelayedAction.playSoundAfterDelay((Game1.random.NextDouble() < 0.1) ? "cowboy_gopher" : "tinyWhip", 200);
				}
			}
			nextFrameUpdate -= (float)time.ElapsedGameTime.TotalSeconds;
			if (currentHoleIndex.Value < 0 && Game1.IsMasterGame)
			{
				currentHoleIndex.Value = FindRandomFreePoint();
			}
			if (currentState == State.DigDown && frame == 0)
			{
				if (currentHoleIndex.Value >= 0)
				{
					currentState = State.DigUp;
				}
				_localIndex = currentHoleIndex.Value;
			}
			if ((int)currentHoleIndex == -1 || currentHoleIndex.Value != _localIndex)
			{
				currentState = State.DigDown;
			}
			if (!(nextFrameUpdate <= 0f))
			{
				return;
			}
			if (_localIndex >= 0)
			{
				if (currentState == State.DigDown)
				{
					frame--;
					if (frame <= 0)
					{
						frame = 0;
					}
				}
				else if (currentState == State.DigUp)
				{
					if (_localIndex >= 0)
					{
						frame++;
						if (frame >= 4)
						{
							currentState = State.Idle;
						}
					}
				}
				else if (currentState == State.Idle)
				{
					frame++;
					if (frame > 7)
					{
						frame = 4;
					}
				}
			}
			nextFrameUpdate = 0.075f;
		}

		public virtual void Draw(SpriteBatch b)
		{
			if (!whacked.Value && _localIndex >= 0)
			{
				Point point = holeLocations[_localIndex];
				Vector2 globalPosition = (new Vector2(point.X, point.Y) + new Vector2(0.5f, 0.5f)) * 64f;
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, globalPosition), new Rectangle(frame % 4 * 16, frame / 4 * 24, 16, 24), Color.White, 0f, new Vector2(8f, 20f), 4f, SpriteEffects.None, globalPosition.Y / 10000f);
			}
		}
	}
}
