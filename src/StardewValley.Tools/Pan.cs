using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StardewValley.Tools
{
	public class Pan : Tool
	{
		[XmlIgnore]
		private readonly NetEvent0 finishEvent = new NetEvent0();

		public Pan()
			: base("Copper Pan", -1, 12, 12, stackable: false)
		{
		}

		public override Item getOne()
		{
			Pan pan = new Pan();
			CopyEnchantments(this, pan);
			pan._GetOneFrom(this);
			return pan;
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Pan.cs.14180");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Pan.cs.14181");
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(finishEvent);
			finishEvent.onEvent += doFinish;
		}

		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			base.CurrentParentTileIndex = 12;
			base.IndexOfMenuItemView = 12;
			bool flag = false;
			int num = 7;
			int num2 = num * 64;
			Rectangle value = new Rectangle(location.orePanPoint.X * 64 - num2 / 2, location.orePanPoint.Y * 64 - num2 / 2, num2, num2);
			if (value.Contains(x, y) && Utility.distance(who.getStandingX(), value.Center.X, who.getStandingY(), value.Center.Y) <= (float)num2)
			{
				flag = true;
			}
			who.lastClick = Vector2.Zero;
			x = (int)who.GetToolLocation().X;
			y = (int)who.GetToolLocation().Y;
			who.lastClick = new Vector2(x, y);
			if (location.orePanPoint != null && !location.orePanPoint.Equals(Point.Zero))
			{
				Rectangle boundingBox = who.GetBoundingBox();
				if (flag || boundingBox.Intersects(value))
				{
					who.faceDirection(2);
					who.FarmerSprite.animateOnce(303, 50f, 4);
					return true;
				}
			}
			who.forceCanMove();
			return true;
		}

		public static void playSlosh(Farmer who)
		{
			who.currentLocation.localSound("slosh");
		}

		public override void tickUpdate(GameTime time, Farmer who)
		{
			lastUser = who;
			base.tickUpdate(time, who);
			finishEvent.Poll();
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			x = (int)who.GetToolLocation().X;
			y = (int)who.GetToolLocation().Y;
			base.CurrentParentTileIndex = 12;
			base.IndexOfMenuItemView = 12;
			location.localSound("coin");
			who.addItemsByMenuIfNecessary(getPanItems(location, who));
			location.orePanPoint.Value = Point.Zero;
			finish();
		}

		private void finish()
		{
			finishEvent.Fire();
		}

		private void doFinish()
		{
			lastUser.CanMove = true;
			lastUser.UsingTool = false;
			lastUser.canReleaseTool = true;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.IndexOfMenuItemView = 12;
			base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
		}

		public List<Item> getPanItems(GameLocation location, Farmer who)
		{
			List<Item> list = new List<Item>();
			int num = 378;
			int num2 = -1;
			Random random = new Random(location.orePanPoint.X + location.orePanPoint.Y * 1000 + (int)Game1.stats.DaysPlayed);
			double num3 = random.NextDouble() - (double)(int)who.luckLevel * 0.001 - who.DailyLuck;
			if (num3 < 0.01)
			{
				num = 386;
			}
			else if (num3 < 0.241)
			{
				num = 384;
			}
			else if (num3 < 0.6)
			{
				num = 380;
			}
			int initialStack = random.Next(5) + 1 + (int)((random.NextDouble() + 0.1 + (double)((float)(int)who.luckLevel / 10f) + who.DailyLuck) * 2.0);
			int num4 = random.Next(5) + 1 + (int)((random.NextDouble() + 0.1 + (double)((float)(int)who.luckLevel / 10f)) * 2.0);
			num3 = random.NextDouble() - who.DailyLuck;
			if (num3 < 0.4 + (double)who.LuckLevel * 0.04)
			{
				num3 = random.NextDouble() - who.DailyLuck;
				num2 = 382;
				if (num3 < 0.02 + (double)who.LuckLevel * 0.002)
				{
					num2 = 72;
					num4 = 1;
				}
				else if (num3 < 0.1)
				{
					num2 = 60 + random.Next(5) * 2;
					num4 = 1;
				}
				else if (num3 < 0.36)
				{
					num2 = 749;
					num4 = Math.Max(1, num4 / 2);
				}
				else if (num3 < 0.5)
				{
					num2 = ((random.NextDouble() < 0.3) ? 82 : ((random.NextDouble() < 0.5) ? 84 : 86));
					num4 = 1;
				}
				if (num3 < (double)who.LuckLevel * 0.002)
				{
					list.Add(new Ring(859));
				}
			}
			list.Add(new Object(num, initialStack));
			if (num2 != -1)
			{
				list.Add(new Object(num2, num4));
			}
			if (location is IslandNorth && (bool)(Game1.getLocationFromName("IslandNorth") as IslandNorth).bridgeFixed && random.NextDouble() < 0.2)
			{
				list.Add(new Object(822, 1));
			}
			else if (location is IslandLocation && random.NextDouble() < 0.2)
			{
				list.Add(new Object(831, random.Next(2, 6)));
			}
			return list;
		}
	}
}
