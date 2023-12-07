using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;

namespace StardewValley.Events
{
	public class SoundInTheNightEvent : FarmEvent, INetObject<NetFields>
	{
		public const int cropCircle = 0;

		public const int meteorite = 1;

		public const int dogs = 2;

		public const int owl = 3;

		public const int earthquake = 4;

		private readonly NetInt behavior = new NetInt();

		private int timer;

		private string soundName;

		private string message;

		private bool playedSound;

		private bool showedMessage;

		private Vector2 targetLocation;

		private Building targetBuilding;

		public NetFields NetFields { get; } = new NetFields();


		public SoundInTheNightEvent()
		{
			NetFields.AddField(behavior);
		}

		public SoundInTheNightEvent(int which)
			: this()
		{
			behavior.Value = which;
		}

		public bool setUp()
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			Farm farm = Game1.getLocationFromName("Farm") as Farm;
			farm.updateMap();
			switch ((int)behavior)
			{
			case 0:
			{
				soundName = "UFO";
				message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_UFO");
				int num;
				for (num = 50; num > 0; num--)
				{
					targetLocation = new Vector2(random.Next(5, farm.map.GetLayer("Back").TileWidth - 4), random.Next(5, farm.map.GetLayer("Back").TileHeight - 4));
					if (farm.isTileLocationTotallyClearAndPlaceable(targetLocation))
					{
						break;
					}
				}
				if (num <= 0)
				{
					return true;
				}
				break;
			}
			case 1:
			{
				soundName = "Meteorite";
				message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_Meteorite");
				targetLocation = new Vector2(random.Next(5, farm.map.GetLayer("Back").TileWidth - 20), random.Next(5, farm.map.GetLayer("Back").TileHeight - 4));
				for (int i = (int)targetLocation.X; (float)i <= targetLocation.X + 1f; i++)
				{
					for (int j = (int)targetLocation.Y; (float)j <= targetLocation.Y + 1f; j++)
					{
						Vector2 tile = new Vector2(i, j);
						if (!farm.isTileOpenBesidesTerrainFeatures(tile) || !farm.isTileOpenBesidesTerrainFeatures(new Vector2(tile.X + 1f, tile.Y)) || !farm.isTileOpenBesidesTerrainFeatures(new Vector2(tile.X + 1f, tile.Y - 1f)) || !farm.isTileOpenBesidesTerrainFeatures(new Vector2(tile.X, tile.Y - 1f)) || farm.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Water", "Back") != null || farm.doesTileHaveProperty((int)tile.X + 1, (int)tile.Y, "Water", "Back") != null)
						{
							return true;
						}
					}
				}
				break;
			}
			case 2:
				soundName = "dogs";
				if (random.NextDouble() < 0.5)
				{
					return true;
				}
				foreach (Building building in farm.buildings)
				{
					if (building.indoors.Value != null && building.indoors.Value is AnimalHouse && !building.animalDoorOpen && (building.indoors.Value as AnimalHouse).animalsThatLiveHere.Count > (building.indoors.Value as AnimalHouse).animals.Count() && random.NextDouble() < (double)(1f / (float)farm.buildings.Count))
					{
						targetBuilding = building;
						break;
					}
				}
				if (targetBuilding == null)
				{
					return true;
				}
				return false;
			case 3:
			{
				soundName = "owl";
				int num;
				for (num = 50; num > 0; num--)
				{
					targetLocation = new Vector2(random.Next(5, farm.map.GetLayer("Back").TileWidth - 4), random.Next(5, farm.map.GetLayer("Back").TileHeight - 4));
					if (farm.isTileLocationTotallyClearAndPlaceable(targetLocation))
					{
						break;
					}
				}
				if (num <= 0)
				{
					return true;
				}
				break;
			}
			case 4:
				soundName = "thunder_small";
				message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_Earthquake");
				break;
			}
			Game1.freezeControls = true;
			return false;
		}

		public bool tickUpdate(GameTime time)
		{
			timer += time.ElapsedGameTime.Milliseconds;
			if (timer > 1500 && !playedSound)
			{
				if (soundName != null && !soundName.Equals(""))
				{
					Game1.playSound(soundName);
					playedSound = true;
				}
				if (!playedSound && message != null)
				{
					Game1.drawObjectDialogue(message);
					Game1.globalFadeToClear();
					showedMessage = true;
				}
			}
			if (timer > 7000 && !showedMessage)
			{
				Game1.pauseThenMessage(10, message, showProgressBar: false);
				showedMessage = true;
			}
			if (showedMessage && playedSound)
			{
				Game1.freezeControls = false;
				return true;
			}
			return false;
		}

		public void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
		}

		public void makeChangesToLocation()
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Farm farm = Game1.getLocationFromName("Farm") as Farm;
			switch ((int)behavior)
			{
			case 0:
			{
				Object @object = new Object(targetLocation, 96);
				@object.minutesUntilReady.Value = 24000 - Game1.timeOfDay;
				farm.objects.Add(targetLocation, @object);
				break;
			}
			case 1:
				if (farm.terrainFeatures.ContainsKey(targetLocation))
				{
					farm.terrainFeatures.Remove(targetLocation);
				}
				if (farm.terrainFeatures.ContainsKey(targetLocation + new Vector2(1f, 0f)))
				{
					farm.terrainFeatures.Remove(targetLocation + new Vector2(1f, 0f));
				}
				if (farm.terrainFeatures.ContainsKey(targetLocation + new Vector2(1f, 1f)))
				{
					farm.terrainFeatures.Remove(targetLocation + new Vector2(1f, 1f));
				}
				if (farm.terrainFeatures.ContainsKey(targetLocation + new Vector2(0f, 1f)))
				{
					farm.terrainFeatures.Remove(targetLocation + new Vector2(0f, 1f));
				}
				farm.resourceClumps.Add(new ResourceClump(622, 2, 2, targetLocation));
				break;
			case 2:
			{
				AnimalHouse animalHouse = targetBuilding.indoors.Value as AnimalHouse;
				long num = 0L;
				foreach (long item in animalHouse.animalsThatLiveHere)
				{
					if (!animalHouse.animals.ContainsKey(item))
					{
						num = item;
						break;
					}
				}
				if (!Game1.getFarm().animals.ContainsKey(num))
				{
					break;
				}
				Game1.getFarm().animals.Remove(num);
				animalHouse.animalsThatLiveHere.Remove(num);
				{
					foreach (KeyValuePair<long, FarmAnimal> pair in Game1.getFarm().animals.Pairs)
					{
						pair.Value.moodMessage.Value = 5;
					}
					break;
				}
			}
			case 3:
				farm.objects.Add(targetLocation, new Object(targetLocation, 95));
				break;
			}
		}

		public void drawAboveEverything(SpriteBatch b)
		{
		}
	}
}
