using System.Collections.Generic;
using Microsoft.Xna.Framework;

public sealed class TilePositionComparer : IEqualityComparer<Vector2>
{
	public bool Equals(Vector2 a, Vector2 b)
	{
		if (a.X == b.X)
		{
			return a.Y == b.Y;
		}
		return false;
	}

	public int GetHashCode(Vector2 a)
	{
		return (ushort)a.X | ((ushort)a.Y << 16);
	}
}
