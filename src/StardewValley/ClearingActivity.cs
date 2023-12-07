using Microsoft.Xna.Framework;
using xTile.Dimensions;

namespace StardewValley
{
	public class ClearingActivity : FarmActivity
	{
		protected override bool _AttemptActivity(Farm farm)
		{
			Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(0, 0, farm.map.Layers[0].LayerWidth, farm.map.Layers[0].LayerHeight);
			for (int i = 0; i < 5; i++)
			{
				Vector2 randomPositionInThisRectangle = Utility.getRandomPositionInThisRectangle(r, Game1.random);
				randomPositionInThisRectangle.X = (int)randomPositionInThisRectangle.X;
				randomPositionInThisRectangle.Y = (int)randomPositionInThisRectangle.Y;
				Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int)randomPositionInThisRectangle.X, (int)randomPositionInThisRectangle.Y, 1, 1);
				if (!farm.isTileLocationTotallyClearAndPlaceableIgnoreFloors(randomPositionInThisRectangle))
				{
					continue;
				}
				rectangle.Inflate(1, 1);
				bool flag = false;
				for (int j = rectangle.Left; j < rectangle.Right; j++)
				{
					for (int k = rectangle.Top; k < rectangle.Bottom; k++)
					{
						if ((float)j != randomPositionInThisRectangle.X || (float)k != randomPositionInThisRectangle.Y)
						{
							if (!farm.isTileOnMap(new Vector2(j, k)))
							{
								flag = true;
								break;
							}
							if (farm.isTileOccupiedIgnoreFloors(new Vector2(j, k)))
							{
								flag = true;
								break;
							}
							if (!farm.isTilePassable(new Location(j, k), Game1.viewport))
							{
								flag = true;
								break;
							}
							if (farm.getBuildingAt(new Vector2(j, k)) != null)
							{
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
				if (!flag)
				{
					activityPosition = randomPositionInThisRectangle;
					activityDirection = 2;
					return true;
				}
			}
			return false;
		}

		protected override void _BeginActivity()
		{
			if (_character.Name == "Haley" && Game1.random.NextDouble() <= 0.5)
			{
				_character.StartActivityRouteEndBehavior("haley_photo", "");
			}
			else
			{
				_character.StartActivityWalkInSquare(2, 2, 0);
			}
		}

		protected override bool _Update(GameTime time)
		{
			if ((double)_age > 5.0)
			{
				if (!_character.IsReturningToEndPoint())
				{
					_character.EndActivityRouteEndBehavior();
				}
				if (!_character.IsWalkingInSquare)
				{
					return true;
				}
			}
			return false;
		}

		protected override void _EndActivity()
		{
		}
	}
}
