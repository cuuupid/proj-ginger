using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace StardewValley
{
	public class TreeActivity : FarmActivity
	{
		protected override bool _AttemptActivity(Farm farm)
		{
			TerrainFeature randomTerrainFeature = GetRandomTerrainFeature(farm, (TerrainFeature feature) => (feature is Tree && (feature as Tree).growthStage.Value >= 5) || (feature is FruitTree && (feature as FruitTree).growthStage.Value >= 4));
			if (randomTerrainFeature != null)
			{
				Vector2 v = randomTerrainFeature.currentTileLocation + new Vector2(0f, 1f);
				Rectangle rectangle = new Rectangle((int)randomTerrainFeature.currentTileLocation.X, (int)randomTerrainFeature.currentTileLocation.Y, 1, 1);
				if (!farm.isTileLocationTotallyClearAndPlaceableIgnoreFloors(v))
				{
					return false;
				}
				rectangle.Inflate(2, 2);
				for (int i = rectangle.Left; i < rectangle.Right; i++)
				{
					for (int j = rectangle.Top; j < rectangle.Bottom; j++)
					{
						if ((float)i == randomTerrainFeature.currentTileLocation.X && (float)j == randomTerrainFeature.currentTileLocation.Y)
						{
							continue;
						}
						Object objectAtTile = farm.getObjectAtTile(i, j);
						if (objectAtTile != null)
						{
							if (objectAtTile.Name.Equals("Weeds"))
							{
								return false;
							}
							if (objectAtTile.Name.Equals("Stone"))
							{
								return false;
							}
						}
						if (farm.terrainFeatures.ContainsKey(new Vector2(i, j)))
						{
							TerrainFeature terrainFeature = farm.terrainFeatures[new Vector2(i, j)];
							if (terrainFeature is Tree || terrainFeature is FruitTree)
							{
								return false;
							}
						}
					}
				}
				activityPosition = v;
				activityDirection = 2;
				return true;
			}
			return false;
		}

		protected override void _BeginActivity()
		{
			if (_character.Name == "Haley")
			{
				_character.StartActivityRouteEndBehavior("haley_photo", "");
			}
			else if (_character.Name == "Penny")
			{
				_character.StartActivityRouteEndBehavior("penny_read", "");
			}
			else if (_character.Name == "Leah")
			{
				_character.StartActivityRouteEndBehavior("leah_draw", "");
			}
		}

		protected override void _EndActivity()
		{
			_character.EndActivityRouteEndBehavior();
		}
	}
}
