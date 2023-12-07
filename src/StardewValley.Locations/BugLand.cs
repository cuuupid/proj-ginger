using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;
using xTile.Tiles;

namespace StardewValley.Locations
{
	public class BugLand : GameLocation
	{
		[XmlElement("hasSpawnedBugsToday")]
		public bool hasSpawnedBugsToday;

		public BugLand()
		{
		}

		public BugLand(string map, string name)
			: base(map, name)
		{
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			if (l is BugLand)
			{
				BugLand bugLand = l as BugLand;
				hasSpawnedBugsToday = bugLand.hasSpawnedBugsToday;
			}
			base.TransferDataFromSavedLocation(l);
		}

		public override void hostSetup()
		{
			base.hostSetup();
			if (Game1.IsMasterGame && !hasSpawnedBugsToday)
			{
				InitializeBugLand();
			}
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			for (int i = 0; i < characters.Count; i++)
			{
				if (characters[i] is Grub || characters[i] is Fly)
				{
					characters.RemoveAt(i);
					i--;
				}
			}
			hasSpawnedBugsToday = false;
		}

		public virtual void InitializeBugLand()
		{
			if (hasSpawnedBugsToday)
			{
				return;
			}
			hasSpawnedBugsToday = true;
			for (int i = 0; i < map.Layers[0].LayerWidth; i++)
			{
				for (int j = 0; j < map.Layers[0].LayerHeight; j++)
				{
					if (!(Game1.random.NextDouble() < 0.33))
					{
						continue;
					}
					Tile tile = map.GetLayer("Paths").Tiles[i, j];
					if (tile == null)
					{
						continue;
					}
					Vector2 vector = new Vector2(i, j);
					switch (tile.TileIndex)
					{
					case 13:
					case 14:
					case 15:
						if (!objects.ContainsKey(vector))
						{
							objects.Add(vector, new Object(vector, GameLocation.getWeedForSeason(Game1.random, "spring"), 1));
						}
						break;
					case 16:
						if (!objects.ContainsKey(vector))
						{
							objects.Add(vector, new Object(vector, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
						}
						break;
					case 17:
						if (!objects.ContainsKey(vector))
						{
							objects.Add(vector, new Object(vector, (Game1.random.NextDouble() < 0.5) ? 343 : 450, 1));
						}
						break;
					case 18:
						if (!objects.ContainsKey(vector))
						{
							objects.Add(vector, new Object(vector, (Game1.random.NextDouble() < 0.5) ? 294 : 295, 1));
						}
						break;
					case 28:
						if (isTileLocationTotallyClearAndPlaceable(vector) && characters.Count < 50)
						{
							characters.Add(new Grub(new Vector2(vector.X * 64f, vector.Y * 64f), hard: true));
						}
						break;
					}
				}
			}
		}
	}
}
