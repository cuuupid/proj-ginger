using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Menus;
using xTile.ObjectModel;

namespace StardewValley.Tools
{
	public class Axe : Tool
	{
		public const int StumpStrength = 4;

		private int stumpTileX;

		private int stumpTileY;

		private int hitsToStump;

		public NetInt additionalPower = new NetInt(0);

		public Axe()
			: base("Axe", 0, 189, 215, stackable: false)
		{
			base.UpgradeLevel = 0;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(additionalPower);
		}

		public override Item getOne()
		{
			Axe axe = new Axe();
			axe.UpgradeLevel = base.UpgradeLevel;
			CopyEnchantments(this, axe);
			axe._GetOneFrom(this);
			return axe;
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Axe.cs.1");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Axe.cs.14019");
		}

		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			Update(who.FacingDirection, 0, who);
			who.EndUsingTool();
			return true;
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			if (!isEfficient)
			{
				who.Stamina -= (float)(2 * power) - (float)who.ForagingLevel * 0.1f;
			}
			int num = x / 64;
			int num2 = y / 64;
			Rectangle value = new Rectangle(num * 64, num2 * 64, 64, 64);
			Vector2 vector = new Vector2(num, num2);
			if (location.Map.GetLayer("Buildings").Tiles[num, num2] != null)
			{
				PropertyValue value2 = null;
				location.Map.GetLayer("Buildings").Tiles[num, num2].TileIndexProperties.TryGetValue("TreeStump", out value2);
				if (value2 != null)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Axe.cs.14023"));
					return;
				}
			}
			upgradeLevel.Value += additionalPower.Value;
			location.performToolAction(this, num, num2);
			if (location.terrainFeatures.ContainsKey(vector) && location.terrainFeatures[vector].performToolAction(this, 0, vector, location))
			{
				location.terrainFeatures.Remove(vector);
				if (TutorialManager.Instance.numberOfThingsCleared < 8)
				{
					if (TutorialManager.Instance.numberOfThingsCleared < 1)
					{
						TutorialManager.Instance.completeTutorial(tutorialType.TAP_FARM);
					}
					TutorialManager.Instance.numberOfThingsCleared++;
					if (TutorialManager.Instance.numberOfThingsCleared >= 8)
					{
						TutorialManager.Instance.completeTutorial(tutorialType.TAP_FARM2);
					}
				}
			}
			if (location.largeTerrainFeatures != null)
			{
				for (int num3 = location.largeTerrainFeatures.Count - 1; num3 >= 0; num3--)
				{
					if (location.largeTerrainFeatures[num3].getBoundingBox().Intersects(value) && location.largeTerrainFeatures[num3].performToolAction(this, 0, vector, location))
					{
						location.largeTerrainFeatures.RemoveAt(num3);
					}
					if (TutorialManager.Instance.numberOfThingsCleared < 8)
					{
						if (TutorialManager.Instance.numberOfThingsCleared < 1)
						{
							TutorialManager.Instance.completeTutorial(tutorialType.TAP_FARM);
						}
						TutorialManager.Instance.numberOfThingsCleared++;
						if (TutorialManager.Instance.numberOfThingsCleared >= 8)
						{
							TutorialManager.Instance.completeTutorial(tutorialType.TAP_FARM2);
						}
					}
				}
			}
			Vector2 vector2 = new Vector2(num, num2);
			if (location.Objects.ContainsKey(vector2) && location.Objects[vector2].Type != null && location.Objects[vector2].performToolAction(this, location))
			{
				if (location.Objects[vector2].type.Equals("Crafting") && (int)location.Objects[vector2].fragility != 2)
				{
					location.debris.Add(new Debris(location.Objects[vector2].bigCraftable ? (-location.Objects[vector2].ParentSheetIndex) : location.Objects[vector2].ParentSheetIndex, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y)));
				}
				location.Objects[vector2].performRemoveAction(vector2, location);
				location.Objects.Remove(vector2);
				if (TutorialManager.Instance.numberOfThingsCleared < 8)
				{
					if (TutorialManager.Instance.numberOfThingsCleared < 1)
					{
						TutorialManager.Instance.completeTutorial(tutorialType.TAP_FARM);
					}
					TutorialManager.Instance.numberOfThingsCleared++;
					if (TutorialManager.Instance.numberOfThingsCleared >= 8)
					{
						TutorialManager.Instance.completeTutorial(tutorialType.TAP_FARM2);
					}
				}
			}
			upgradeLevel.Value -= additionalPower.Value;
		}
	}
}
