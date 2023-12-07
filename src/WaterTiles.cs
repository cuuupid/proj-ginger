public class WaterTiles
{
	public struct WaterTileData
	{
		public bool isWater;

		public bool isVisible;

		public WaterTileData(bool is_water, bool is_visible)
		{
			isWater = is_water;
			isVisible = is_visible;
		}
	}

	public WaterTileData[,] waterTiles;

	public bool this[int x, int y]
	{
		get
		{
			return waterTiles[x, y].isWater;
		}
		set
		{
			waterTiles[x, y] = new WaterTileData(value, is_visible: true);
		}
	}

	public static implicit operator WaterTiles(bool[,] source)
	{
		WaterTiles waterTiles = new WaterTiles();
		waterTiles.waterTiles = new WaterTileData[source.GetLength(0), source.GetLength(1)];
		for (int i = 0; i < source.GetLength(0); i++)
		{
			for (int j = 0; j < source.GetLength(1); j++)
			{
				waterTiles.waterTiles[i, j] = new WaterTileData(source[i, j], is_visible: true);
			}
		}
		return waterTiles;
	}
}
