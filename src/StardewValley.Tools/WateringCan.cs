using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Menus;

namespace StardewValley.Tools
{
	public class WateringCan : Tool
	{
		[XmlElement("isBottomless")]
		public readonly NetBool isBottomless = new NetBool();

		[XmlIgnore]
		protected bool _emptyCanPlayed;

		public int waterCanMax = 40;

		private int waterLeft = 40;

		public int WaterLeft
		{
			get
			{
				return waterLeft;
			}
			set
			{
				waterLeft = value;
			}
		}

		public bool IsBottomless
		{
			get
			{
				return isBottomless;
			}
			set
			{
				isBottomless.Value = value;
			}
		}

		public WateringCan()
			: base("Watering Can", 0, 273, 296, stackable: false)
		{
			base.UpgradeLevel = 0;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(isBottomless);
		}

		public override Item getOne()
		{
			WateringCan wateringCan = new WateringCan();
			wateringCan.UpgradeLevel = base.UpgradeLevel;
			CopyEnchantments(this, wateringCan);
			wateringCan._GetOneFrom(this);
			return wateringCan;
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan.cs.14324");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan.cs.14325");
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			Vector2 vector = (Game1.player.hasWateringCanEnchantment ? new Vector2(0f, -4f) : new Vector2(0f, -16f)) * scaleSize;
			Vector2 position = location + new Vector2(((float)base.itemSlotSize - 56f * scaleSize) / 2f, (float)base.itemSlotSize - 24f * scaleSize);
			float scale = 4f * scaleSize;
			Vector2 vector2 = new Vector2(position.X + (float)(int)Math.Round(4f * scaleSize), position.Y + (float)(int)Math.Round(4f * scaleSize));
			base.drawInMenu(spriteBatch, location + vector, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
			if (drawStackNumber != 0 && !Game1.player.hasWateringCanEnchantment)
			{
				spriteBatch.Draw(Game1.mouseCursors, position, new Rectangle(297, 420, 14, 5), Color.White * transparency, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth + 0.0001f);
				spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)vector2.X, (int)vector2.Y, (int)((float)waterLeft / (float)waterCanMax * (float)(int)Math.Ceiling(48f * scaleSize)), (int)Math.Ceiling(8f * scaleSize)), IsBottomless ? (Color.BlueViolet * 1f * transparency) : (Color.DodgerBlue * 0.7f * transparency));
			}
		}

		public override string getDescription()
		{
			return Game1.parseText(base.description + (Game1.player.hasWateringCanEnchantment ? (Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan_enchant")) : ""), Game1.smallFont, getDescriptionWidth());
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			power = who.toolPower;
			who.stopJittering();
			List<Vector2> list = tilesAffected(new Vector2(x / 64, y / 64), power, who);
			if (Game1.currentLocation.CanRefillWateringCanOnTile(x / 64, y / 64))
			{
				who.jitterStrength = 0.5f;
				switch ((int)upgradeLevel)
				{
				case 0:
					waterCanMax = 40;
					break;
				case 1:
					waterCanMax = 55;
					break;
				case 2:
					waterCanMax = 70;
					break;
				case 3:
					waterCanMax = 85;
					break;
				case 4:
					waterCanMax = 100;
					break;
				}
				waterLeft = waterCanMax;
				location.playSound("slosh");
				DelayedAction.playSoundAfterDelay("glug", 250, location);
			}
			else if (waterLeft > 0 || who.hasWateringCanEnchantment)
			{
				if (!isEfficient)
				{
					who.Stamina -= (float)(2 * (power + 1)) - (float)who.FarmingLevel * 0.1f;
				}
				int num = 0;
				foreach (Vector2 item in list)
				{
					if (location.terrainFeatures.ContainsKey(item))
					{
						location.terrainFeatures[item].performToolAction(this, 0, item, location);
						if (!TutorialManager.Instance.hasUsedWateringCan && TutorialManager.Instance.completeTutorial(tutorialType.WATER_GROUND))
						{
							TutorialManager.Instance.hasUsedWateringCan = true;
						}
					}
					if (location.objects.ContainsKey(item))
					{
						location.Objects[item].performToolAction(this, location);
					}
					location.performToolAction(this, (int)item.X, (int)item.Y);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(13, new Vector2(item.X * 64f, item.Y * 64f), Color.White, 10, Game1.random.NextDouble() < 0.5, 70f, 0, 64, (item.Y * 64f + 32f) / 10000f - 0.01f)
					{
						delayBeforeAnimationStart = 200 + num * 10
					});
					num++;
				}
				if (!isBottomless)
				{
					waterLeft -= power + 1;
				}
				Vector2 vector = new Vector2(who.Position.X - 32f - 4f, who.Position.Y - 16f - 4f);
				switch (who.FacingDirection)
				{
				case 1:
					vector.X += 136f;
					break;
				case 2:
					vector.X += 72f;
					vector.Y += 44f;
					break;
				case 0:
					vector = Vector2.Zero;
					break;
				}
				if (!vector.Equals(Vector2.Zero))
				{
					for (int i = 0; i < 30; i++)
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("", new Rectangle(0, 0, 1, 1), 999f, 1, 999, vector + new Vector2(Game1.random.Next(-3, 0) * 4, Game1.random.Next(2) * 4), flicker: false, flipped: false, (float)(who.GetBoundingBox().Bottom + 32) / 10000f, 0.04f, (Game1.random.NextDouble() < 0.5) ? Color.DeepSkyBlue : Color.LightBlue, 4f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = i * 15,
							motion = new Vector2((float)Game1.random.Next(-10, 11) / 100f, 0.5f),
							acceleration = new Vector2(0f, 0.1f)
						});
					}
				}
			}
			else if (!_emptyCanPlayed)
			{
				_emptyCanPlayed = true;
				who.doEmote(4);
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan.cs.14335"));
			}
		}

		public override bool CanUseOnStandingTile()
		{
			return true;
		}

		public override void tickUpdate(GameTime time, Farmer who)
		{
			base.tickUpdate(time, who);
			if (who.IsLocalPlayer)
			{
				if (Game1.areAllOfTheseKeysUp(Game1.input.GetKeyboardState(), Game1.options.useToolButton) && Game1.input.GetMouseState().LeftButton == ButtonState.Released && Game1.input.GetGamePadState().IsButtonUp(Buttons.X))
				{
					_emptyCanPlayed = false;
				}
			}
			else
			{
				_emptyCanPlayed = false;
			}
		}
	}
}
