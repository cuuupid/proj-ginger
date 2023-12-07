using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace StardewValley
{
	public class SlimeHutch : GameLocation
	{
		[XmlElement("slimeMatingsLeft")]
		public readonly NetInt slimeMatingsLeft = new NetInt();

		public readonly NetArray<bool, NetBool> waterSpots = new NetArray<bool, NetBool>(4);

		public SlimeHutch()
		{
		}

		public SlimeHutch(string m, string name)
			: base(m, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(slimeMatingsLeft, waterSpots);
		}

		public void updateWhenNotCurrentLocation(Building parentBuilding, GameTime time)
		{
		}

		public bool isFull()
		{
			return characters.Count >= 20;
		}

		public Building getBuilding()
		{
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building.indoors.Value != null && building.indoors.Value.Equals(this))
				{
					return building;
				}
			}
			return null;
		}

		public override bool canSlimeMateHere()
		{
			int num = slimeMatingsLeft;
			slimeMatingsLeft.Value--;
			if (!isFull())
			{
				return num > 0;
			}
			return false;
		}

		public override bool canSlimeHatchHere()
		{
			return !isFull();
		}

		public override void DayUpdate(int dayOfMonth)
		{
			int num = 0;
			int num2 = Game1.random.Next(waterSpots.Length);
			for (int i = 0; i < waterSpots.Length; i++)
			{
				if (waterSpots[(i + num2) % waterSpots.Length] && num * 5 < characters.Count)
				{
					num++;
					waterSpots[(i + num2) % waterSpots.Length] = false;
				}
			}
			for (int num3 = objects.Count() - 1; num3 >= 0; num3--)
			{
				if (objects.Pairs.ElementAt(num3).Value.IsSprinkler())
				{
					List<Vector2> sprinklerTiles = objects.Pairs.ElementAt(num3).Value.GetSprinklerTiles();
					foreach (Vector2 item in sprinklerTiles)
					{
						if (item.X == 16f && item.Y >= 6f && item.Y <= 9f)
						{
							waterSpots[(int)item.Y - 6] = true;
						}
					}
				}
			}
			for (int num4 = Math.Min(characters.Count / 5, num); num4 > 0; num4--)
			{
				int num5 = 50;
				Vector2 randomTile = getRandomTile();
				while ((!isTileLocationTotallyClearAndPlaceable(randomTile) || doesTileHaveProperty((int)randomTile.X, (int)randomTile.Y, "NPCBarrier", "Back") != null || randomTile.Y >= 12f) && num5 > 0)
				{
					randomTile = getRandomTile();
					num5--;
				}
				if (num5 > 0)
				{
					objects.Add(randomTile, new Object(randomTile, 56));
				}
			}
			while ((int)slimeMatingsLeft > 0)
			{
				if (characters.Count > 1 && !isFull())
				{
					NPC nPC = characters[Game1.random.Next(characters.Count)];
					if (nPC is GreenSlime)
					{
						GreenSlime greenSlime = nPC as GreenSlime;
						if ((int)greenSlime.ageUntilFullGrown <= 0)
						{
							for (int j = 1; j < 10; j++)
							{
								GreenSlime greenSlime2 = (GreenSlime)Utility.checkForCharacterWithinArea(greenSlime.GetType(), nPC.Position, this, new Rectangle((int)greenSlime.Position.X - 64 * j, (int)greenSlime.Position.Y - 64 * j, 64 * (j * 2 + 1), 64 * (j * 2 + 1)));
								if (greenSlime2 != null && greenSlime2.cute != greenSlime.cute && (int)greenSlime2.ageUntilFullGrown <= 0)
								{
									greenSlime.mateWith(greenSlime2, this);
									break;
								}
							}
						}
					}
				}
				slimeMatingsLeft.Value--;
			}
			slimeMatingsLeft.Value = characters.Count / 5 + 1;
			base.DayUpdate(dayOfMonth);
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			if (l is SlimeHutch)
			{
				for (int i = 0; i < waterSpots.Length; i++)
				{
					if (i < (l as SlimeHutch).waterSpots.Count)
					{
						waterSpots[i] = (l as SlimeHutch).waterSpots[i];
					}
				}
			}
			base.TransferDataFromSavedLocation(l);
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if (t is WateringCan && tileX == 16 && tileY >= 6 && tileY <= 9)
			{
				waterSpots[tileY - 6] = true;
			}
			return false;
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			for (int i = 0; i < waterSpots.Length; i++)
			{
				if (waterSpots[i])
				{
					setMapTileIndex(16, 6 + i, 2135, "Buildings");
				}
				else
				{
					setMapTileIndex(16, 6 + i, 2134, "Buildings");
				}
			}
		}
	}
}
