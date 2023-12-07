using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using StardewValley.Menus;
using xTile.Dimensions;

namespace StardewValley.Tools
{
	public class Hoe : Tool
	{
		public Hoe()
			: base("Hoe", 0, 21, 47, stackable: false)
		{
			base.UpgradeLevel = 0;
		}

		public override Item getOne()
		{
			Hoe hoe = new Hoe();
			hoe.UpgradeLevel = base.UpgradeLevel;
			CopyEnchantments(this, hoe);
			hoe._GetOneFrom(this);
			return hoe;
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Hoe.cs.14101");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Hoe.cs.14102");
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			if (location.Name.StartsWith("UndergroundMine"))
			{
				power = 1;
			}
			if (!isEfficient)
			{
				who.Stamina -= (float)(2 * power) - (float)who.FarmingLevel * 0.1f;
			}
			power = who.toolPower;
			who.stopJittering();
			location.playSound("woodyHit");
			Vector2 vector = new Vector2(x / 64, y / 64);
			List<Vector2> list = tilesAffected(vector, power, who);
			foreach (Vector2 item in list)
			{
				item.Equals(vector);
				if (location.terrainFeatures.ContainsKey(item))
				{
					if (location.terrainFeatures[item].performToolAction(this, 0, item, location))
					{
						location.terrainFeatures.Remove(item);
					}
					continue;
				}
				if (location.objects.ContainsKey(item) && location.Objects[item].performToolAction(this, location))
				{
					if (location.Objects[item].type.Equals("Crafting") && (int)location.Objects[item].fragility != 2)
					{
						location.debris.Add(new Debris(location.Objects[item].bigCraftable ? (-location.Objects[item].ParentSheetIndex) : location.Objects[item].ParentSheetIndex, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y)));
					}
					location.Objects[item].performRemoveAction(item, location);
					location.Objects.Remove(item);
				}
				if (location.doesTileHaveProperty((int)item.X, (int)item.Y, "Diggable", "Back") == null)
				{
					continue;
				}
				if (location.Name.StartsWith("UndergroundMine") && !location.isTileOccupied(item))
				{
					if ((location as MineShaft).getMineArea() != 77377)
					{
						location.makeHoeDirt(item);
						location.playSound("hoeHit");
						Game1.removeSquareDebrisFromTile((int)item.X, (int)item.Y);
						location.checkForBuriedItem((int)item.X, (int)item.Y, explosion: false, detectOnly: false, who);
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(vector.X * 64f, vector.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
						if (list.Count > 2)
						{
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(6, new Vector2(item.X * 64f, item.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, Vector2.Distance(vector, item) * 30f));
						}
					}
				}
				else if (!location.isTileOccupied(item) && location.isTilePassable(new Location((int)item.X, (int)item.Y), Game1.viewport))
				{
					location.makeHoeDirt(item);
					location.playSound("hoeHit");
					Game1.removeSquareDebrisFromTile((int)item.X, (int)item.Y);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(item.X * 64f, item.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
					if (list.Count > 2)
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(6, new Vector2(item.X * 64f, item.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, Vector2.Distance(vector, item) * 30f));
					}
					location.checkForBuriedItem((int)item.X, (int)item.Y, explosion: false, detectOnly: false, who);
				}
				Game1.stats.DirtHoed++;
				if (TutorialManager.Instance.numberOfTilesHoed < 6)
				{
					TutorialManager.Instance.numberOfTilesHoed++;
					if (TutorialManager.Instance.numberOfTilesHoed == 6)
					{
						TutorialManager.Instance.completeTutorial(tutorialType.USE_HOE);
					}
				}
			}
		}
	}
}
