using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.TerrainFeatures;

namespace StardewValley.Objects
{
	public class SwitchFloor : Object
	{
		public static Color successColor = Color.LightBlue;

		[XmlElement("onColor")]
		public readonly NetColor onColor = new NetColor();

		[XmlElement("offColor")]
		public readonly NetColor offColor = new NetColor();

		[XmlElement("readyToflip")]
		private readonly NetBool readyToflip = new NetBool(value: false);

		[XmlElement("finished")]
		public readonly NetBool finished = new NetBool(value: false);

		private int ticksToSuccess = -1;

		[XmlElement("glow")]
		private readonly NetFloat glow = new NetFloat(0f);

		public SwitchFloor()
		{
			base.NetFields.AddFields(onColor, offColor, readyToflip, finished, glow);
		}

		public SwitchFloor(Vector2 tileLocation, Color onColor, Color offColor, bool on)
			: this()
		{
			base.tileLocation.Value = tileLocation;
			this.onColor.Value = onColor;
			this.offColor.Value = offColor;
			isOn.Value = on;
			fragility.Value = 2;
			base.name = "Switch Floor";
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:SwitchFloor.cs.13097");
		}

		public void flip(GameLocation environment)
		{
			isOn.Value = !isOn;
			glow.Value = 0.65f;
			foreach (Vector2 adjacentTileLocation in Utility.getAdjacentTileLocations(tileLocation))
			{
				if (environment.objects.ContainsKey(adjacentTileLocation) && environment.objects[adjacentTileLocation] is SwitchFloor)
				{
					environment.objects[adjacentTileLocation].isOn.Value = !environment.objects[adjacentTileLocation].isOn;
					(environment.objects[adjacentTileLocation] as SwitchFloor).glow.Value = 0.3f;
				}
			}
			Game1.playSound("shiny4");
		}

		public void setSuccessCountdown(int ticks)
		{
			ticksToSuccess = ticks;
			glow.Value = 0.5f;
		}

		public void checkForCompleteness()
		{
			Queue<Vector2> queue = new Queue<Vector2>();
			HashSet<Vector2> hashSet = new HashSet<Vector2>();
			queue.Enqueue(tileLocation);
			Vector2 vector = default(Vector2);
			List<Vector2> list = new List<Vector2>();
			while (queue.Count > 0)
			{
				vector = queue.Dequeue();
				if (Game1.currentLocation.objects.ContainsKey(vector) && Game1.currentLocation.objects[vector] is SwitchFloor && (Game1.currentLocation.objects[vector] as SwitchFloor).isOn != isOn)
				{
					return;
				}
				hashSet.Add(vector);
				list = Utility.getAdjacentTileLocations(vector);
				for (int i = 0; i < list.Count; i++)
				{
					if (!hashSet.Contains(list[i]) && Game1.currentLocation.objects.ContainsKey(vector) && Game1.currentLocation.objects[vector] is SwitchFloor)
					{
						queue.Enqueue(list[i]);
					}
				}
				list.Clear();
			}
			int num = 5;
			foreach (Vector2 item in hashSet)
			{
				if (Game1.currentLocation.objects.ContainsKey(item) && Game1.currentLocation.objects[item] is SwitchFloor)
				{
					(Game1.currentLocation.objects[item] as SwitchFloor).setSuccessCountdown(num);
				}
				num += 2;
			}
			int coins = (int)Math.Sqrt(hashSet.Count) * 2;
			Vector2 vector2 = hashSet.Last();
			while (Game1.currentLocation.isTileOccupiedByFarmer(vector2) != null)
			{
				hashSet.Remove(vector2);
				if (hashSet.Count > 0)
				{
					vector2 = hashSet.Last();
				}
			}
			Game1.currentLocation.objects[vector2] = new Chest(coins, null, vector2);
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, vector2 * 64f, flicker: false, flipped: false));
			Game1.playSound("coin");
		}

		public override bool isPassable()
		{
			return true;
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			spriteBatch.Draw(Flooring.floorsTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64)), new Rectangle(0, 1280, 64, 64), finished ? successColor : ((Color)(isOn ? onColor : offColor)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
			if ((float)glow > 0f)
			{
				spriteBatch.Draw(Flooring.floorsTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64)), new Rectangle(0, 1280, 64, 64), Color.White * glow, 0f, Vector2.Zero, 4f, SpriteEffects.None, 2E-08f);
			}
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			if ((float)glow > 0f)
			{
				glow.Value -= 0.04f;
			}
			if (ticksToSuccess > 0)
			{
				ticksToSuccess--;
				if (ticksToSuccess == 0)
				{
					finished.Value = true;
					glow.Value += 0.2f;
					Game1.playSound("boulderCrack");
				}
			}
			else
			{
				if ((bool)finished)
				{
					return;
				}
				foreach (Farmer farmer in Game1.currentLocation.farmers)
				{
					if (farmer.getTileLocation().Equals(tileLocation))
					{
						if ((bool)readyToflip)
						{
							flip(Game1.currentLocation);
							checkForCompleteness();
						}
						readyToflip.Value = false;
						return;
					}
				}
				readyToflip.Value = true;
			}
		}
	}
}
