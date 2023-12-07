using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Menus;

namespace StardewValley.Tools
{
	public class Pickaxe : Tool
	{
		public const int hitMargin = 8;

		public const int BoulderStrength = 4;

		private int boulderTileX;

		private int boulderTileY;

		private int hitsToBoulder;

		public NetInt additionalPower = new NetInt(0);

		public Pickaxe()
			: base("Pickaxe", 0, 105, 131, stackable: false)
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
			Pickaxe pickaxe = new Pickaxe();
			pickaxe.UpgradeLevel = base.UpgradeLevel;
			pickaxe.additionalPower.Value = additionalPower.Value;
			CopyEnchantments(this, pickaxe);
			pickaxe._GetOneFrom(this);
			return pickaxe;
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Pickaxe.cs.14184");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Pickaxe.cs.14185");
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
			power = who.toolPower;
			if (!isEfficient)
			{
				who.Stamina -= (float)(2 * (power + 1)) - (float)who.MiningLevel * 0.1f;
			}
			Vector2 nonTileLocation = new Vector2(x, y);
			Utility.clampToTile(nonTileLocation);
			int num = x / 64;
			int num2 = y / 64;
			Vector2 vector = new Vector2(num, num2);
			if (location.performToolAction(this, num, num2))
			{
				return;
			}
			Object value = null;
			location.Objects.TryGetValue(vector, out value);
			if (value == null)
			{
				if (who.FacingDirection == 0 || who.FacingDirection == 2)
				{
					num = (x - 8) / 64;
					location.Objects.TryGetValue(new Vector2(num, num2), out value);
					if (value == null)
					{
						num = (x + 8) / 64;
						location.Objects.TryGetValue(new Vector2(num, num2), out value);
					}
				}
				else
				{
					num2 = (y + 8) / 64;
					location.Objects.TryGetValue(new Vector2(num, num2), out value);
					if (value == null)
					{
						num2 = (y - 8) / 64;
						location.Objects.TryGetValue(new Vector2(num, num2), out value);
					}
				}
				x = num * 64;
				y = num2 * 64;
				if (location.terrainFeatures.ContainsKey(vector) && location.terrainFeatures[vector].performToolAction(this, 0, vector, location))
				{
					location.terrainFeatures.Remove(vector);
				}
			}
			vector = new Vector2(num, num2);
			if (value != null)
			{
				if (value.Name.Equals("Stone"))
				{
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
					location.playSound("hammer");
					if ((int)value.minutesUntilReady > 0)
					{
						int num3 = Math.Max(1, (int)upgradeLevel + 1) + additionalPower.Value;
						value.minutesUntilReady.Value -= num3;
						value.shakeTimer = 200;
						if ((int)value.minutesUntilReady > 0)
						{
							Game1.createRadialDebris(Game1.currentLocation, 14, num, num2, Game1.random.Next(2, 5), resource: false);
							return;
						}
					}
					if (value.ParentSheetIndex < 200 && !Game1.objectInformation.ContainsKey(value.ParentSheetIndex + 1) && (int)value.parentSheetIndex != 25)
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(value.ParentSheetIndex + 1, 300f, 1, 2, new Vector2(x - x % 64, y - y % 64), flicker: true, value.flipped)
						{
							alphaFade = 0.01f
						});
					}
					else
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(47, new Vector2(num * 64, num2 * 64), Color.Gray, 10, flipped: false, 80f));
					}
					Game1.createRadialDebris(location, 14, num, num2, Game1.random.Next(2, 5), resource: false);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(46, new Vector2(num * 64, num2 * 64), Color.White, 10, flipped: false, 80f)
					{
						motion = new Vector2(0f, -0.6f),
						acceleration = new Vector2(0f, 0.002f),
						alphaFade = 0.015f
					});
					location.OnStoneDestroyed(value.parentSheetIndex, num, num2, getLastFarmerToUse());
					if ((int)value.minutesUntilReady <= 0)
					{
						value.performRemoveAction(new Vector2(num, num2), location);
						location.Objects.Remove(new Vector2(num, num2));
						location.playSound("stoneCrack");
						Game1.stats.RocksCrushed++;
					}
				}
				else if (value.Name.Contains("Boulder"))
				{
					location.playSound("hammer");
					if (base.UpgradeLevel < 2)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Pickaxe.cs.14194")));
						return;
					}
					if (num == boulderTileX && num2 == boulderTileY)
					{
						hitsToBoulder += power + 1;
						value.shakeTimer = 190;
					}
					else
					{
						hitsToBoulder = 0;
						boulderTileX = num;
						boulderTileY = num2;
					}
					if (hitsToBoulder >= 4)
					{
						location.removeObject(vector, showDestroyedObject: false);
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * vector.X - 32f, 64f * (vector.Y - 1f)), Color.Gray, 8, Game1.random.NextDouble() < 0.5, 50f)
						{
							delayBeforeAnimationStart = 0
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * vector.X + 32f, 64f * (vector.Y - 1f)), Color.Gray, 8, Game1.random.NextDouble() < 0.5, 50f)
						{
							delayBeforeAnimationStart = 200
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * vector.X, 64f * (vector.Y - 1f) - 32f), Color.Gray, 8, Game1.random.NextDouble() < 0.5, 50f)
						{
							delayBeforeAnimationStart = 400
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * vector.X, 64f * vector.Y - 32f), Color.Gray, 8, Game1.random.NextDouble() < 0.5, 50f)
						{
							delayBeforeAnimationStart = 600
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64f * vector.X, 64f * vector.Y), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, 128));
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64f * vector.X + 32f, 64f * vector.Y), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, 128)
						{
							delayBeforeAnimationStart = 250
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64f * vector.X - 32f, 64f * vector.Y), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, 128)
						{
							delayBeforeAnimationStart = 500
						});
						location.playSound("boulderBreak");
						Game1.stats.BouldersCracked++;
					}
				}
				else if (value.performToolAction(this, location))
				{
					value.performRemoveAction(vector, location);
					if (value.type.Equals("Crafting") && (int)value.fragility != 2)
					{
						Game1.currentLocation.debris.Add(new Debris(value.bigCraftable ? (-value.ParentSheetIndex) : value.ParentSheetIndex, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y)));
					}
					Game1.currentLocation.Objects.Remove(vector);
				}
			}
			else
			{
				location.playSound("woodyHit");
				if (location.doesTileHaveProperty(num, num2, "Diggable", "Back") != null)
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(num * 64, num2 * 64), Color.White, 8, flipped: false, 80f)
					{
						alphaFade = 0.015f
					});
				}
			}
		}
	}
}
