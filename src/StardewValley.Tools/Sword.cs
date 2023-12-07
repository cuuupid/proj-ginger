using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley.Tools
{
	public class Sword : Tool
	{
		public const double baseCritChance = 0.02;

		public int whichUpgrade;

		public Sword()
		{
		}

		public Sword(string name, int spriteIndex)
			: base(name, 0, spriteIndex, spriteIndex, stackable: false)
		{
		}

		public override Item getOne()
		{
			Sword sword = new Sword(base.BaseName, base.InitialParentTileIndex);
			sword._GetOneFrom(this);
			return sword;
		}

		public void DoFunction(GameLocation location, int x, int y, int facingDirection, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			Vector2 vector = Vector2.Zero;
			Vector2 vector2 = Vector2.Zero;
			Rectangle rectangle = Rectangle.Empty;
			Rectangle boundingBox = who.GetBoundingBox();
			switch (facingDirection)
			{
			case 0:
				rectangle = new Rectangle(x - 64, boundingBox.Y - 64, 128, 64);
				vector = new Vector2(((Game1.random.NextDouble() < 0.5) ? rectangle.Left : rectangle.Right) / 64, rectangle.Top / 64);
				vector2 = new Vector2(rectangle.Center.X / 64, rectangle.Top / 64);
				break;
			case 2:
				rectangle = new Rectangle(x - 64, boundingBox.Bottom, 128, 64);
				vector = new Vector2(((Game1.random.NextDouble() < 0.5) ? rectangle.Left : rectangle.Right) / 64, rectangle.Center.Y / 64);
				vector2 = new Vector2(rectangle.Center.X / 64, rectangle.Center.Y / 64);
				break;
			case 1:
				rectangle = new Rectangle(boundingBox.Right, y - 64, 64, 128);
				vector = new Vector2(rectangle.Center.X / 64, ((Game1.random.NextDouble() < 0.5) ? rectangle.Top : rectangle.Bottom) / 64);
				vector2 = new Vector2(rectangle.Center.X / 64, rectangle.Center.Y / 64);
				break;
			case 3:
				rectangle = new Rectangle(boundingBox.Left - 64, y - 64, 64, 128);
				vector = new Vector2(rectangle.Left / 64, ((Game1.random.NextDouble() < 0.5) ? rectangle.Top : rectangle.Bottom) / 64);
				vector2 = new Vector2(rectangle.Left / 64, rectangle.Center.Y / 64);
				break;
			}
			int minDamage = ((whichUpgrade == 2) ? 3 : ((whichUpgrade == 4) ? 6 : whichUpgrade)) + 1;
			int maxDamage = 4 * (((whichUpgrade == 2) ? 3 : ((whichUpgrade == 4) ? 5 : whichUpgrade)) + 1);
			bool flag = location.damageMonster(rectangle, minDamage, maxDamage, isBomb: false, who);
			if (whichUpgrade == 4 && !flag)
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(352, Game1.random.Next(50, 120), 2, 1, new Vector2(rectangle.Center.X - 32, rectangle.Center.Y - 32) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), flicker: false, Game1.random.NextDouble() < 0.5));
			}
			bool flag2 = false;
			string text = "";
			if (!flag)
			{
				if (location.objects.ContainsKey(vector) && !location.Objects[vector].Name.Contains("Stone") && !location.Objects[vector].Name.Contains("Stick") && !location.Objects[vector].Name.Contains("Stump") && !location.Objects[vector].Name.Contains("Boulder") && !location.Objects[vector].Name.Contains("Lumber") && !location.Objects[vector].IsHoeDirt)
				{
					if (location.Objects[vector].Name.Contains("Weed"))
					{
						if (!(who.Stamina > 0f))
						{
							return;
						}
						Game1.stats.WeedsEliminated++;
						checkWeedForTreasure(vector, who);
						int num = location.Objects[vector].Category;
						text = ((num != -2) ? "cut" : "stoneCrack");
						location.removeObject(vector, showDestroyedObject: true);
					}
					else
					{
						location.objects[vector].performToolAction(this, location);
					}
				}
				if (location.objects.ContainsKey(vector2) && !location.Objects[vector2].Name.Contains("Stone") && !location.Objects[vector2].Name.Contains("Stick") && !location.Objects[vector2].Name.Contains("Stump") && !location.Objects[vector2].Name.Contains("Boulder") && !location.Objects[vector2].Name.Contains("Lumber") && !location.Objects[vector2].IsHoeDirt)
				{
					if (location.Objects[vector2].Name.Contains("Weed"))
					{
						if (!(who.Stamina > 0f))
						{
							return;
						}
						Game1.stats.WeedsEliminated++;
						checkWeedForTreasure(vector2, who);
					}
					else
					{
						location.objects[vector2].performToolAction(this, location);
					}
				}
			}
			bool flag3 = false;
			List<Vector2> listOfTileLocationsForBordersOfNonTileRectangle = Utility.getListOfTileLocationsForBordersOfNonTileRectangle(rectangle);
			foreach (Vector2 item in listOfTileLocationsForBordersOfNonTileRectangle)
			{
				if (location.terrainFeatures.ContainsKey(item) && location.terrainFeatures[item].performToolAction(this, 0, item, location))
				{
					location.terrainFeatures.Remove(item);
					flag3 = true;
				}
			}
			if (!text.Equals(""))
			{
				Game1.playSound(text);
			}
			base.CurrentParentTileIndex = base.IndexOfMenuItemView;
		}

		public void checkWeedForTreasure(Vector2 tileLocation, Farmer who)
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + (int)tileLocation.X * 13 + (int)tileLocation.Y * 29);
			if (random.NextDouble() < 0.07)
			{
				Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, random.Next(1, 3));
			}
			else if (random.NextDouble() < 0.02 + (double)who.LuckLevel / 10.0)
			{
				Game1.createDebris((random.NextDouble() < 0.5) ? 4 : 8, (int)tileLocation.X, (int)tileLocation.Y, random.Next(1, 4));
			}
			else if (random.NextDouble() < 0.006 + (double)who.LuckLevel / 20.0)
			{
				Game1.createObjectDebris(114, (int)tileLocation.X, (int)tileLocation.Y);
			}
		}

		protected override string loadDisplayName()
		{
			if (Name.Equals("Battered Sword"))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1205");
			}
			return whichUpgrade switch
			{
				2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14292"), 
				3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14294"), 
				4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14296"), 
				_ => Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14290"), 
			};
		}

		protected override string loadDescription()
		{
			return whichUpgrade switch
			{
				1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14291"), 
				2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14293"), 
				3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14295"), 
				4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Sword.cs.14297"), 
				_ => Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1206"), 
			};
		}

		public void upgrade(int which)
		{
			if (which > whichUpgrade)
			{
				whichUpgrade = which;
				switch (which)
				{
				case 1:
					Name = "Hero's Sword";
					base.IndexOfMenuItemView = 68;
					break;
				case 2:
					Name = "Holy Sword";
					base.IndexOfMenuItemView = 70;
					break;
				case 3:
					Name = "Dark Sword";
					base.IndexOfMenuItemView = 69;
					break;
				case 4:
					Name = "Galaxy Sword";
					base.IndexOfMenuItemView = 71;
					break;
				}
				displayName = null;
				base.description = null;
				base.UpgradeLevel = which;
			}
			base.CurrentParentTileIndex = base.IndexOfMenuItemView;
		}
	}
}
