using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;

namespace StardewValley.Menus
{
	public class FarmInfoPage : IClickableMenu
	{
		private string descriptionText = "";

		private string hoverText = "";

		private ClickableTextureComponent moneyIcon;

		private ClickableTextureComponent farmMap;

		private ClickableTextureComponent mapFarmer;

		private ClickableTextureComponent farmHouse;

		private List<ClickableTextureComponent> animals = new List<ClickableTextureComponent>();

		private List<ClickableTextureComponent> mapBuildings = new List<ClickableTextureComponent>();

		private List<MiniatureTerrainFeature> mapFeatures = new List<MiniatureTerrainFeature>();

		private Farm farm;

		private int mapX;

		private int mapY;

		public FarmInfoPage(int x, int y, int width, int height)
			: base(x, y, width, height)
		{
			moneyIcon = new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 32, (Game1.player.Money > 9999) ? 18 : 20, 16), Game1.player.Money + "g", "", Game1.debrisSpriteSheet, new Rectangle(88, 280, 16, 16), 1f);
			mapX = x + IClickableMenu.spaceToClearSideBorder + 128 + 32 + 16;
			mapY = y + IClickableMenu.spaceToClearTopBorder + 21 - 4;
			farmMap = new ClickableTextureComponent(new Rectangle(mapX, mapY, 20, 20), Game1.content.Load<Texture2D>("LooseSprites\\farmMap"), Rectangle.Empty, 1f);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			int num9 = 0;
			int num10 = 0;
			int num11 = 0;
			int num12 = 0;
			int num13 = 0;
			int num14 = 0;
			int num15 = 0;
			int num16 = 0;
			farm = (Farm)Game1.getLocationFromName("Farm");
			farmHouse = new ClickableTextureComponent("FarmHouse", new Rectangle(mapX + 443, mapY + 43, 80, 72), "FarmHouse", "", Game1.content.Load<Texture2D>("Buildings\\houses"), new Rectangle(0, 0, 160, 144), 0.5f);
			List<FarmAnimal> allFarmAnimals = farm.getAllFarmAnimals();
			foreach (FarmAnimal item in allFarmAnimals)
			{
				if (item.type.Value.Contains("Chicken"))
				{
					num++;
					num9 += (int)item.friendshipTowardFarmer;
					continue;
				}
				switch (item.type.Value)
				{
				case "Cow":
					num5++;
					num13 += (int)item.friendshipTowardFarmer;
					break;
				case "Duck":
					num2++;
					num11 += (int)item.friendshipTowardFarmer;
					break;
				case "Rabbit":
					num3++;
					num10 += (int)item.friendshipTowardFarmer;
					break;
				case "Sheep":
					num6++;
					num15 += (int)item.friendshipTowardFarmer;
					break;
				case "Goat":
					num7++;
					num14 += (int)item.friendshipTowardFarmer;
					break;
				case "Pig":
					num8++;
					num16 += (int)item.friendshipTowardFarmer;
					break;
				default:
					num4++;
					num12 += (int)item.friendshipTowardFarmer;
					break;
				}
			}
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64, 40, 32), num.ToString() ?? "", "Chickens" + ((num > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", num9 / num)) : ""), Game1.mouseCursors, new Rectangle(256, 64, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 36, 40, 32), num2.ToString() ?? "", "Ducks" + ((num2 > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", num11 / num2)) : ""), Game1.mouseCursors, new Rectangle(288, 64, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 72, 40, 32), num3.ToString() ?? "", "Rabbits" + ((num3 > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", num10 / num3)) : ""), Game1.mouseCursors, new Rectangle(256, 96, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 108, 40, 32), num5.ToString() ?? "", "Cows" + ((num5 > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", num13 / num5)) : ""), Game1.mouseCursors, new Rectangle(320, 64, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 144, 40, 32), num7.ToString() ?? "", "Goats" + ((num7 > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", num14 / num7)) : ""), Game1.mouseCursors, new Rectangle(352, 64, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 180, 40, 32), num6.ToString() ?? "", "Sheep" + ((num6 > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", num15 / num6)) : ""), Game1.mouseCursors, new Rectangle(352, 96, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 216, 40, 32), num8.ToString() ?? "", "Pigs" + ((num8 > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", num16 / num8)) : ""), Game1.mouseCursors, new Rectangle(320, 96, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 252, 40, 32), num4.ToString() ?? "", "???" + ((num4 > 0) ? (Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10425", num12 / num4)) : ""), Game1.mouseCursors, new Rectangle(288, 96, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 288, 40, 32), Game1.stats.CropsShipped.ToString() ?? "", Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10440"), Game1.mouseCursors, new Rectangle(480, 64, 32, 32), 1f));
			animals.Add(new ClickableTextureComponent("", new Rectangle(x + IClickableMenu.spaceToClearSideBorder + 32, y + IClickableMenu.spaceToClearTopBorder + 64 + 324, 40, 32), farm.buildings.Count().ToString() ?? "", Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmInfoPage.cs.10441"), Game1.mouseCursors, new Rectangle(448, 64, 32, 32), 1f));
			int num17 = 8;
			foreach (Building building in farm.buildings)
			{
				mapBuildings.Add(new ClickableTextureComponent("", new Rectangle(mapX + (int)building.tileX * num17, mapY + (int)building.tileY * num17 + ((int)building.tilesHigh + 1) * num17 - (int)((float)building.texture.Value.Height / 8f), (int)building.tilesWide * num17, (int)((float)building.texture.Value.Height / 8f)), "", building.buildingType, building.texture.Value, building.getSourceRectForMenu(), 0.125f));
			}
			foreach (KeyValuePair<Vector2, TerrainFeature> pair in farm.terrainFeatures.Pairs)
			{
				mapFeatures.Add(new MiniatureTerrainFeature(pair.Value, new Vector2(pair.Key.X * (float)num17 + (float)mapX, pair.Key.Y * (float)num17 + (float)mapY), pair.Key, 0.125f));
			}
			if (Game1.currentLocation is Farm)
			{
				mapFarmer = new ClickableTextureComponent("", new Rectangle(mapX + (int)(Game1.player.Position.X / 8f), mapY + (int)(Game1.player.Position.Y / 8f), 8, 12), "", Game1.player.Name, null, new Rectangle(0, 0, 64, 96), 0.125f);
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			descriptionText = "";
			hoverText = "";
			foreach (ClickableTextureComponent animal in animals)
			{
				if (animal.containsPoint(x, y))
				{
					hoverText = animal.hoverText;
					return;
				}
			}
			foreach (ClickableTextureComponent mapBuilding in mapBuildings)
			{
				if (mapBuilding.containsPoint(x, y))
				{
					hoverText = mapBuilding.hoverText;
					return;
				}
			}
			if (mapFarmer != null && mapFarmer.containsPoint(x, y))
			{
				hoverText = mapFarmer.hoverText;
			}
		}

		public override void draw(SpriteBatch b)
		{
			drawVerticalPartition(b, xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 128);
			moneyIcon.draw(b);
			foreach (ClickableTextureComponent animal in animals)
			{
				animal.draw(b);
			}
			farmMap.draw(b);
			foreach (ClickableTextureComponent mapBuilding in mapBuildings)
			{
				mapBuilding.draw(b);
			}
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
			farmMap.draw(b);
			foreach (ClickableTextureComponent mapBuilding2 in mapBuildings)
			{
				mapBuilding2.draw(b);
			}
			foreach (MiniatureTerrainFeature mapFeature in mapFeatures)
			{
				mapFeature.draw(b);
			}
			farmHouse.draw(b);
			if (mapFarmer != null)
			{
				Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2(mapFarmer.bounds.X - 16, mapFarmer.bounds.Y - 16), 0.99f, 2f, 2, Game1.player);
			}
			foreach (KeyValuePair<long, FarmAnimal> pair in farm.animals.Pairs)
			{
				b.Draw(pair.Value.Sprite.Texture, new Vector2(mapX + (int)(pair.Value.Position.X / 8f), mapY + (int)(pair.Value.Position.Y / 8f)), pair.Value.Sprite.SourceRect, Color.White, 0f, Vector2.Zero, 0.125f, SpriteEffects.None, 0.86f + pair.Value.Position.Y / 8f / 20000f + 0.0125f);
			}
			foreach (KeyValuePair<Vector2, Object> pair2 in farm.objects.Pairs)
			{
				pair2.Value.drawInMenu(b, new Vector2((float)mapX + pair2.Key.X * 8f, (float)mapY + pair2.Key.Y * 8f), 0.125f, 1f, 0.86f + ((float)mapY + pair2.Key.Y * 8f - 25f) / 20000f);
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
		}
	}
}
