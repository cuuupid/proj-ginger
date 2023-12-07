using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace StardewValley.Objects
{
	public class BreakableContainer : Object
	{
		public const int barrel = 118;

		public const int frostBarrel = 120;

		public const int darkBarrel = 122;

		public const int desertBarrel = 124;

		public const int volcanoBarrel = 174;

		public const int waterBarrel = 262;

		[XmlElement("debris")]
		private readonly NetInt debris = new NetInt();

		private new int shakeTimer;

		[XmlElement("health")]
		private new readonly NetInt health = new NetInt();

		[XmlElement("containerType")]
		private readonly NetInt containerType = new NetInt();

		[XmlElement("hitSound")]
		private readonly NetString hitSound = new NetString();

		[XmlElement("breakSound")]
		private readonly NetString breakSound = new NetString();

		[XmlElement("breakDebrisSource")]
		private readonly NetRectangle breakDebrisSource = new NetRectangle();

		[XmlElement("breakDebrisSource2")]
		private readonly NetRectangle breakDebrisSource2 = new NetRectangle();

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(debris, health, containerType, hitSound, breakSound, breakDebrisSource, breakDebrisSource2);
		}

		public BreakableContainer()
		{
		}

		public BreakableContainer(Vector2 tile, int type, MineShaft mine)
			: base(tile, typeToIndex(type))
		{
			containerType.Value = type;
			if (type != 118)
			{
				return;
			}
			if (mine.GetAdditionalDifficulty() > 0)
			{
				if (mine.getMineArea() == 0 || mine.getMineArea() == 10)
				{
					base.ParentSheetIndex = (mine.isDarkArea() ? 118 : 262);
				}
				else if (mine.getMineArea() == 40)
				{
					base.ParentSheetIndex = 118;
				}
			}
			else
			{
				if (mine.getMineArea() == 40)
				{
					base.ParentSheetIndex = 120;
					containerType.Value = 120;
				}
				if (mine.getMineArea() == 80)
				{
					base.ParentSheetIndex = 122;
					containerType.Value = 122;
				}
				if (mine.getMineArea() == 121)
				{
					base.ParentSheetIndex = 124;
					containerType.Value = 124;
				}
			}
			if (Game1.random.NextDouble() < 0.5)
			{
				base.ParentSheetIndex++;
			}
			health.Value = 3;
			debris.Value = 12;
			hitSound.Value = "woodWhack";
			breakSound.Value = "barrelBreak";
			breakDebrisSource.Value = new Rectangle(598, 1275, 13, 4);
			breakDebrisSource2.Value = new Rectangle(611, 1275, 10, 4);
		}

		public BreakableContainer(Vector2 tile, bool isVolcano)
			: base(tile, 174)
		{
			containerType.Value = 174;
			base.ParentSheetIndex = 174;
			if (Game1.random.NextDouble() < 0.5)
			{
				base.ParentSheetIndex++;
			}
			health.Value = 4;
			debris.Value = 14;
			hitSound.Value = "clank";
			breakSound.Value = "boulderBreak";
			breakDebrisSource.Value = new Rectangle(598, 1275, 13, 4);
			breakDebrisSource2.Value = new Rectangle(611, 1275, 10, 4);
		}

		public static int typeToIndex(int type)
		{
			return type switch
			{
				118 => type, 
				120 => type, 
				_ => 0, 
			};
		}

		public override bool performToolAction(Tool t, GameLocation location)
		{
			if (t != null && t.isHeavyHitter())
			{
				health.Value--;
				if (t is MeleeWeapon && (int)(t as MeleeWeapon).type == 2)
				{
					health.Value--;
				}
				if ((int)health <= 0)
				{
					if (breakSound != null)
					{
						location.playSound(breakSound);
					}
					releaseContents(t.getLastFarmerToUse().currentLocation, t.getLastFarmerToUse());
					t.getLastFarmerToUse().currentLocation.objects.Remove(tileLocation);
					int num = Game1.random.Next(4, 12);
					Color color = (((int)containerType == 120) ? Color.White : (((int)containerType == 122) ? new Color(109, 122, 80) : (((int)containerType == 174) ? new Color(107, 76, 83) : new Color(130, 80, 30))));
					for (int i = 0; i < num; i++)
					{
						Game1.multiplayer.broadcastSprites(t.getLastFarmerToUse().currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", (Game1.random.NextDouble() < 0.5) ? breakDebrisSource : breakDebrisSource2, 999f, 1, 0, tileLocation.Value * 64f + new Vector2(32f, 32f), flicker: false, Game1.random.NextDouble() < 0.5, (tileLocation.Y * 64f + 32f) / 10000f, 0.01f, color, 4f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 8f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 64f)
						{
							motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-10, -7)),
							acceleration = new Vector2(0f, 0.3f)
						});
					}
				}
				else if (hitSound != null)
				{
					shakeTimer = 300;
					location.playSound(hitSound);
					Game1.createRadialDebris(t.getLastFarmerToUse().currentLocation, ((int)containerType == 174) ? 14 : 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 7), resource: false, -1, item: false, ((int)containerType == 120) ? 10000 : (-1));
				}
			}
			return false;
		}

		public override bool onExplosion(Farmer who, GameLocation location)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			releaseContents(location, who);
			int num = Game1.random.Next(4, 12);
			Color color = (((int)containerType == 120) ? Color.White : (((int)containerType == 122) ? new Color(109, 122, 80) : (((int)containerType == 174) ? new Color(107, 76, 83) : new Color(130, 80, 30))));
			for (int i = 0; i < num; i++)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", (Game1.random.NextDouble() < 0.5) ? breakDebrisSource : breakDebrisSource2, 999f, 1, 0, tileLocation.Value * 64f + new Vector2(32f, 32f), flicker: false, Game1.random.NextDouble() < 0.5, (tileLocation.Y * 64f + 32f) / 10000f, 0.01f, color, 4f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 8f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 64f)
				{
					motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-10, -7)),
					acceleration = new Vector2(0f, 0.3f)
				});
			}
			return true;
		}

		public void releaseContents(GameLocation location, Farmer who)
		{
			Random random = new Random((int)tileLocation.X + (int)tileLocation.Y * 10000 + (int)Game1.stats.DaysPlayed);
			int num = (int)tileLocation.X;
			int num2 = (int)tileLocation.Y;
			int level = -1;
			int num3 = 0;
			if (location is MineShaft)
			{
				level = ((MineShaft)location).mineLevel;
				if (((MineShaft)location).isContainerPlatform(num, num2))
				{
					((MineShaft)location).updateMineLevelData(0, -1);
				}
				num3 = ((MineShaft)location).GetAdditionalDifficulty();
			}
			if (random.NextDouble() < 0.2)
			{
				return;
			}
			if (Game1.random.NextDouble() <= 0.05 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				Game1.createMultipleObjectDebris(890, num, num2, random.Next(1, 3), who.UniqueMultiplayerID, location);
			}
			if (num3 > 0)
			{
				if (!(random.NextDouble() < 0.15))
				{
					if (random.NextDouble() < 0.008)
					{
						Game1.createMultipleObjectDebris(858, num, num2, 1, location);
					}
					if (random.NextDouble() < 0.01)
					{
						Game1.createItemDebris(new Object(Vector2.Zero, 71), new Vector2(num, num2) * 64f + new Vector2(32f), 0);
					}
					if (random.NextDouble() < 0.01)
					{
						Game1.createMultipleObjectDebris(random.Next(918, 921), num, num2, 1, location);
					}
					if (random.NextDouble() < 0.01)
					{
						Game1.createMultipleObjectDebris(386, num, num2, random.Next(1, 4), location);
					}
					switch (random.Next(17))
					{
					case 0:
						Game1.createMultipleObjectDebris(382, num, num2, random.Next(1, 3), location);
						break;
					case 1:
						Game1.createMultipleObjectDebris(380, num, num2, random.Next(1, 4), location);
						break;
					case 2:
						Game1.createMultipleObjectDebris(62, num, num2, 1, location);
						break;
					case 3:
						Game1.createMultipleObjectDebris(390, num, num2, random.Next(2, 6), location);
						break;
					case 4:
						Game1.createMultipleObjectDebris(80, num, num2, random.Next(2, 3), location);
						break;
					case 5:
						Game1.createMultipleObjectDebris((who.timesReachedMineBottom > 0) ? 84 : ((Game1.random.NextDouble() < 0.5) ? 92 : 370), num, num2, random.Next(2, 4), location);
						break;
					case 6:
						Game1.createMultipleObjectDebris(70, num, num2, 1, location);
						break;
					case 7:
						Game1.createMultipleObjectDebris(390, num, num2, random.Next(2, 6), location);
						break;
					case 8:
						Game1.createMultipleObjectDebris(random.Next(218, 245), num, num2, 1, location);
						break;
					case 9:
						Game1.createMultipleObjectDebris((Game1.whichFarm == 6) ? 920 : 749, num, num2, 1, location);
						break;
					case 10:
						Game1.createMultipleObjectDebris(286, num, num2, 1, location);
						break;
					case 11:
						Game1.createMultipleObjectDebris(378, num, num2, random.Next(1, 4), location);
						break;
					case 12:
						Game1.createMultipleObjectDebris(384, num, num2, random.Next(1, 4), location);
						break;
					case 13:
						Game1.createMultipleObjectDebris(287, num, num2, 1, location);
						break;
					}
				}
				return;
			}
			switch ((int)containerType)
			{
			case 118:
				if (random.NextDouble() < 0.65)
				{
					if (random.NextDouble() < 0.8)
					{
						switch (random.Next(9))
						{
						case 0:
							Game1.createMultipleObjectDebris(382, num, num2, random.Next(1, 3), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(378, num, num2, random.Next(1, 4), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(390, num, num2, random.Next(2, 6), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris(388, num, num2, random.Next(2, 3), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris((who.timesReachedMineBottom > 0) ? 80 : ((Game1.random.NextDouble() < 0.5) ? 92 : 370), num, num2, random.Next(2, 4), location);
							break;
						case 6:
							Game1.createMultipleObjectDebris(388, num, num2, random.Next(2, 6), location);
							break;
						case 7:
							Game1.createMultipleObjectDebris(390, num, num2, random.Next(2, 6), location);
							break;
						case 8:
							Game1.createMultipleObjectDebris(770, num, num2, 1, location);
							break;
						case 2:
							break;
						}
					}
					else
					{
						switch (random.Next(4))
						{
						case 0:
							Game1.createMultipleObjectDebris(78, num, num2, random.Next(1, 3), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(78, num, num2, random.Next(1, 3), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris(78, num, num2, random.Next(1, 3), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(535, num, num2, random.Next(1, 3), location);
							break;
						}
					}
				}
				else if (random.NextDouble() < 0.4)
				{
					switch (random.Next(5))
					{
					case 0:
						Game1.createMultipleObjectDebris(66, num, num2, 1, location);
						break;
					case 1:
						Game1.createMultipleObjectDebris(68, num, num2, 1, location);
						break;
					case 2:
						Game1.createMultipleObjectDebris(709, num, num2, 1, location);
						break;
					case 3:
						Game1.createMultipleObjectDebris(535, num, num2, 1, location);
						break;
					case 4:
						Game1.createItemDebris(MineShaft.getSpecialItemForThisMineLevel(level, num, num2), new Vector2(num, num2) * 64f + new Vector2(32f, 32f), random.Next(4), location);
						break;
					}
				}
				break;
			case 120:
				if (random.NextDouble() < 0.65)
				{
					if (random.NextDouble() < 0.8)
					{
						switch (random.Next(9))
						{
						case 0:
							Game1.createMultipleObjectDebris(382, num, num2, random.Next(1, 3), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(380, num, num2, random.Next(1, 4), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(378, num, num2, random.Next(2, 6), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris(388, num, num2, random.Next(2, 6), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris((who.timesReachedMineBottom > 0) ? 84 : ((Game1.random.NextDouble() < 0.5) ? 92 : 371), num, num2, random.Next(2, 4), location);
							break;
						case 6:
							Game1.createMultipleObjectDebris(390, num, num2, random.Next(2, 4), location);
							break;
						case 7:
							Game1.createMultipleObjectDebris(390, num, num2, random.Next(2, 6), location);
							break;
						case 8:
							Game1.createMultipleObjectDebris(770, num, num2, 1, location);
							break;
						case 2:
							break;
						}
					}
					else
					{
						switch (random.Next(4))
						{
						case 0:
							Game1.createMultipleObjectDebris(78, num, num2, random.Next(1, 3), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(536, num, num2, random.Next(1, 3), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris(78, num, num2, random.Next(1, 3), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(78, num, num2, random.Next(1, 3), location);
							break;
						}
					}
				}
				else if (random.NextDouble() < 0.4)
				{
					switch (random.Next(5))
					{
					case 0:
						Game1.createMultipleObjectDebris(62, num, num2, 1, location);
						break;
					case 1:
						Game1.createMultipleObjectDebris(70, num, num2, 1, location);
						break;
					case 2:
						Game1.createMultipleObjectDebris(709, num, num2, random.Next(1, 4), location);
						break;
					case 3:
						Game1.createMultipleObjectDebris(536, num, num2, 1, location);
						break;
					case 4:
						Game1.createItemDebris(MineShaft.getSpecialItemForThisMineLevel(level, num, num2), new Vector2(num, num2) * 64f + new Vector2(32f, 32f), random.Next(4), location);
						break;
					}
				}
				break;
			case 122:
			case 124:
				if (random.NextDouble() < 0.65)
				{
					if (random.NextDouble() < 0.8)
					{
						switch (random.Next(8))
						{
						case 0:
							Game1.createMultipleObjectDebris(382, num, num2, random.Next(1, 3), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(384, num, num2, random.Next(1, 4), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(380, num, num2, random.Next(2, 6), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris(378, num, num2, random.Next(2, 6), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris(390, num, num2, random.Next(2, 6), location);
							break;
						case 6:
							Game1.createMultipleObjectDebris(388, num, num2, random.Next(2, 6), location);
							break;
						case 7:
							Game1.createMultipleObjectDebris(881, num, num2, random.Next(2, 6), location);
							break;
						case 2:
							break;
						}
					}
					else
					{
						switch (random.Next(4))
						{
						case 0:
							Game1.createMultipleObjectDebris(78, num, num2, random.Next(1, 3), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(537, num, num2, random.Next(1, 3), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris((who.timesReachedMineBottom > 0) ? 82 : 78, num, num2, random.Next(1, 3), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(78, num, num2, random.Next(1, 3), location);
							break;
						}
					}
				}
				else if (random.NextDouble() < 0.4)
				{
					switch (random.Next(6))
					{
					case 0:
						Game1.createMultipleObjectDebris(60, num, num2, 1, location);
						break;
					case 1:
						Game1.createMultipleObjectDebris(64, num, num2, 1, location);
						break;
					case 2:
						Game1.createMultipleObjectDebris(709, num, num2, random.Next(1, 4), location);
						break;
					case 3:
						Game1.createMultipleObjectDebris(749, num, num2, 1, location);
						break;
					case 4:
						Game1.createItemDebris(MineShaft.getSpecialItemForThisMineLevel(level, num, num2), new Vector2(num, num2) * 64f + new Vector2(32f, 32f), random.Next(4), location);
						break;
					case 5:
						Game1.createMultipleObjectDebris(688, num, num2, 1, location);
						break;
					}
				}
				break;
			case 174:
				if (random.NextDouble() < 0.1)
				{
					Game1.player.team.RequestLimitedNutDrops("VolcanoBarrel", location, num * 64, num2 * 64, 5);
				}
				if (location is VolcanoDungeon && (int)(location as VolcanoDungeon).level == 5 && num == 34)
				{
					Game1.createItemDebris(new Object(851, 1, isRecipe: false, -1, 2), new Vector2(num, num2) * 64f, 1);
				}
				else if (random.NextDouble() < 0.75)
				{
					if (random.NextDouble() < 0.8)
					{
						switch (random.Next(7))
						{
						case 0:
							Game1.createMultipleObjectDebris(382, num, num2, random.Next(1, 3), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(384, num, num2, random.Next(1, 4), location);
							break;
						case 2:
							location.characters.Add(new DwarvishSentry(new Vector2(num, num2) * 64f));
							break;
						case 3:
							Game1.createMultipleObjectDebris(380, num, num2, random.Next(2, 6), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris(378, num, num2, random.Next(2, 6), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris(66, num, num2, 1, location);
							break;
						case 6:
							Game1.createMultipleObjectDebris(709, num, num2, random.Next(2, 6), location);
							break;
						}
					}
					else
					{
						switch (random.Next(5))
						{
						case 0:
							Game1.createMultipleObjectDebris(78, num, num2, random.Next(1, 3), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(749, num, num2, random.Next(1, 3), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris(60, num, num2, 1, location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(64, num, num2, 1, location);
							break;
						case 4:
							Game1.createMultipleObjectDebris(68, num, num2, 1, location);
							break;
						}
					}
				}
				else if (random.NextDouble() < 0.4)
				{
					switch (random.Next(9))
					{
					case 0:
						Game1.createMultipleObjectDebris(72, num, num2, 1, location);
						break;
					case 1:
						Game1.createMultipleObjectDebris(831, num, num2, random.Next(1, 4), location);
						break;
					case 2:
						Game1.createMultipleObjectDebris(833, num, num2, random.Next(1, 3), location);
						break;
					case 3:
						Game1.createMultipleObjectDebris(749, num, num2, 1, location);
						break;
					case 4:
						Game1.createMultipleObjectDebris(386, num, num2, 1, location);
						break;
					case 5:
						Game1.createMultipleObjectDebris(848, num, num2, 1, location);
						break;
					case 6:
						Game1.createMultipleObjectDebris(856, num, num2, 1, location);
						break;
					case 7:
						Game1.createMultipleObjectDebris(886, num, num2, 1, location);
						break;
					case 8:
						Game1.createMultipleObjectDebris(688, num, num2, 1, location);
						break;
					}
				}
				else
				{
					location.characters.Add(new DwarvishSentry(new Vector2(num, num2) * 64f));
				}
				break;
			}
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			if (shakeTimer > 0)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
			}
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			Vector2 vector = getScale();
			vector *= 4f;
			Vector2 vector2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			Rectangle destinationRectangle = new Rectangle((int)(vector2.X - vector.X / 2f), (int)(vector2.Y - vector.Y / 2f), (int)(64f + vector.X), (int)(128f + vector.Y / 2f));
			if (shakeTimer > 0)
			{
				int num = shakeTimer / 100 + 1;
				destinationRectangle.X += Game1.random.Next(-num, num + 1);
				destinationRectangle.Y += Game1.random.Next(-num, num + 1);
			}
			spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, Object.getSourceRectForBigCraftable(showNextIndex ? (base.ParentSheetIndex + 1) : base.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f) + (((int)parentSheetIndex == 105) ? 0.0015f : 0f));
		}
	}
}
