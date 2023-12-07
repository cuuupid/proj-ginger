using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Monsters
{
	public class Spiker : Monster
	{
		[XmlIgnore]
		public int targetDirection;

		[XmlIgnore]
		public NetBool moving = new NetBool(value: false);

		protected bool _localMoving;

		[XmlIgnore]
		public float nextMoveCheck;

		public Spiker()
		{
		}

		public Spiker(Vector2 position, int direction)
			: base("Spiker", position)
		{
			Sprite.SpriteWidth = 16;
			Sprite.SpriteHeight = 16;
			Sprite.UpdateSourceRect();
			targetDirection = direction;
			base.speed = 14;
			ignoreMovementAnimations = true;
			onCollision = collide;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(moving);
		}

		public static Vector3? GetSpawnPosition(GameLocation location, Point start_point)
		{
			Vector2 zero = Vector2.Zero;
			int num = ((Game1.random.Next(0, 2) == 0) ? 1 : (-1));
			int num2 = 0;
			Vector2 zero2 = Vector2.Zero;
			int num3 = 0;
			Vector2 zero3 = Vector2.Zero;
			Point p = start_point;
			p.Y += num;
			while (location.isTileOnMap(p.X, p.Y) && location.getTileIndexAt(p, "Buildings") < 0)
			{
				p.Y += num;
				num2++;
			}
			p = start_point;
			p.Y -= num;
			while (location.isTileOnMap(p.X, p.Y) && location.getTileIndexAt(p, "Buildings") < 0)
			{
				zero2.X = p.X;
				zero2.Y = p.Y;
				p.Y -= num;
				num2++;
			}
			p = start_point;
			p.X += num;
			while (location.isTileOnMap(p.X, p.Y) && location.getTileIndexAt(p, "Buildings") < 0)
			{
				p.X += num;
				num3++;
			}
			p = start_point;
			p.X -= num;
			while (location.isTileOnMap(p.X, p.Y) && location.getTileIndexAt(p, "Buildings") < 0)
			{
				zero3.X = p.X;
				zero3.Y = p.Y;
				p.X -= num;
				num3++;
			}
			if (num3 < num2)
			{
				if (num3 <= 4)
				{
					return null;
				}
				return new Vector3(zero3.X, zero3.Y, (num == 1) ? 1 : 3);
			}
			if (num2 <= 4)
			{
				return null;
			}
			return new Vector3(zero2.X, zero2.Y, (num == 1) ? 2 : 0);
		}

		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
			if (moving.Value == _localMoving)
			{
				return;
			}
			_localMoving = moving.Value;
			if (_localMoving)
			{
				if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
				{
					Game1.playSound("parry");
				}
			}
			else if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
			{
				Game1.playSound("hammer");
			}
		}

		public override bool passThroughCharacters()
		{
			return true;
		}

		public override void draw(SpriteBatch b)
		{
			Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, base.Position), (float)GetBoundingBox().Center.Y / 10000f);
		}

		private void collide(GameLocation location)
		{
			Rectangle value = nextPosition(FacingDirection);
			foreach (Farmer farmer in location.farmers)
			{
				if (farmer.GetBoundingBox().Intersects(value))
				{
					return;
				}
			}
			if ((bool)moving)
			{
				moving.Value = false;
				targetDirection = (targetDirection + 2) % 4;
				nextMoveCheck = 0.75f;
			}
		}

		public override void updateMovement(GameLocation location, GameTime time)
		{
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			return -1;
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			if (nextMoveCheck > 0f)
			{
				nextMoveCheck -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (nextMoveCheck <= 0f)
			{
				nextMoveCheck = 0.25f;
				foreach (Farmer farmer in base.currentLocation.farmers)
				{
					if ((targetDirection == 0 || targetDirection == 2) && Math.Abs(farmer.getTileX() - getTileX()) <= 1)
					{
						if (targetDirection == 0 && farmer.Position.Y < base.Position.Y)
						{
							moving.Value = true;
							break;
						}
						if (targetDirection == 2 && farmer.Position.Y > base.Position.Y)
						{
							moving.Value = true;
							break;
						}
					}
					if ((targetDirection == 3 || targetDirection == 1) && Math.Abs(farmer.getTileY() - getTileY()) <= 1)
					{
						if (targetDirection == 3 && farmer.Position.X < base.Position.X)
						{
							moving.Value = true;
							break;
						}
						if (targetDirection == 1 && farmer.Position.X > base.Position.X)
						{
							moving.Value = true;
							break;
						}
					}
				}
			}
			moveUp = false;
			moveDown = false;
			moveLeft = false;
			moveRight = false;
			if (moving.Value)
			{
				if (targetDirection == 0)
				{
					moveUp = true;
				}
				if (targetDirection == 2)
				{
					moveDown = true;
				}
				else if (targetDirection == 3)
				{
					moveLeft = true;
				}
				else if (targetDirection == 1)
				{
					moveRight = true;
				}
				MovePosition(time, Game1.viewport, base.currentLocation);
			}
			faceDirection(2);
		}
	}
}
