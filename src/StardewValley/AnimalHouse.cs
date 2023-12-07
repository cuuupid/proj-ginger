using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Events;
using StardewValley.Network;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley
{
	public class AnimalHouse : GameLocation, IAnimalLocation
	{
		[XmlElement("animals")]
		public readonly NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = new NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>();

		[XmlElement("animalLimit")]
		public readonly NetInt animalLimit = new NetInt(4);

		public readonly NetLongList animalsThatLiveHere = new NetLongList();

		[XmlElement("incubatingEgg")]
		public readonly NetPoint incubatingEgg = new NetPoint();

		private readonly List<KeyValuePair<long, FarmAnimal>> _tempAnimals = new List<KeyValuePair<long, FarmAnimal>>();

		[XmlIgnore]
		public bool hasShownIncubatorBuildingFullMessage;

		public NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> Animals => animals;

		public AnimalHouse()
		{
		}

		public AnimalHouse(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(animals, animalLimit, animalsThatLiveHere, incubatingEgg);
		}

		public void updateWhenNotCurrentLocation(Building parentBuilding, GameTime time)
		{
			if (!Game1.currentLocation.Equals(this))
			{
				for (int num = animals.Count() - 1; num >= 0; num--)
				{
					FarmAnimal value = animals.Pairs.ElementAt(num).Value;
					value.updateWhenNotCurrentLocation(parentBuilding, time, this);
				}
			}
		}

		public void incubator()
		{
			if (incubatingEgg.Y <= 0 && Game1.player.ActiveObject != null && Game1.player.ActiveObject.Category == -5)
			{
				incubatingEgg.X = 2;
				incubatingEgg.Y = Game1.player.ActiveObject.ParentSheetIndex;
				map.GetLayer("Front").Tiles[1, 2].TileIndex += ((Game1.player.ActiveObject.ParentSheetIndex != 180 && Game1.player.ActiveObject.ParentSheetIndex != 182) ? 1 : 2);
				Game1.throwActiveObjectDown();
				hasShownIncubatorBuildingFullMessage = false;
			}
			else if (Game1.player.ActiveObject == null && incubatingEgg.Y > 0)
			{
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_RemoveEgg_Question"), createYesNoResponses(), "RemoveIncubatingEgg");
			}
		}

		public bool isFull()
		{
			return animalsThatLiveHere.Count >= (int)animalLimit;
		}

		public bool CheckPetAnimal(Vector2 position, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (!pair.Value.wasPet && pair.Value.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
				{
					pair.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public bool CheckPetAnimal(Microsoft.Xna.Framework.Rectangle rect, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (!pair.Value.wasPet && pair.Value.GetBoundingBox().Intersects(rect))
				{
					pair.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public bool CheckInspectAnimal(Vector2 position, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if ((bool)pair.Value.wasPet && pair.Value.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
				{
					pair.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public bool CheckInspectAnimal(Microsoft.Xna.Framework.Rectangle rect, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if ((bool)pair.Value.wasPet && pair.Value.GetBoundingBox().Intersects(rect))
				{
					pair.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
			if (who.ActiveObject != null && who.ActiveObject.Name.Equals("Hay") && doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Trough", "Back") != null && !objects.ContainsKey(new Vector2(tileLocation.X, tileLocation.Y)))
			{
				objects.Add(new Vector2(tileLocation.X, tileLocation.Y), (Object)who.ActiveObject.getOne());
				who.reduceActiveItemByOne();
				who.currentLocation.playSound("coin");
				Game1.haltAfterCheck = false;
				return true;
			}
			bool flag = base.checkAction(tileLocation, viewport, who);
			if (!flag)
			{
				if (CheckPetAnimal(rect, who))
				{
					return true;
				}
				if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true) && CheckInspectAnimal(rect, who))
				{
					return true;
				}
			}
			return flag;
		}

		public override bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (pair.Value.getTileLocation().Equals(tileLocation))
				{
					return true;
				}
			}
			return base.isTileOccupiedForPlacement(tileLocation, toPlace);
		}

		protected override void resetSharedState()
		{
			resetPositionsOfAllAnimals();
			foreach (Object value in objects.Values)
			{
				if (!value.bigCraftable || !value.Name.Contains("Incubator") || value.heldObject.Value == null || (int)value.minutesUntilReady > 0)
				{
					continue;
				}
				if (!isFull())
				{
					string text = "??";
					switch (value.heldObject.Value.ParentSheetIndex)
					{
					case 305:
						text = Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_Hatch_VoidEgg");
						break;
					case 174:
					case 176:
					case 180:
					case 182:
						text = Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_Hatch_RegularEgg");
						break;
					case 442:
						text = Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_Hatch_DuckEgg");
						break;
					case 107:
						text = Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_Hatch_DinosaurEgg");
						break;
					case 289:
						text = Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_Hatch_Ostrich");
						break;
					case 928:
						text = Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_Hatch_GoldenEgg");
						break;
					}
					currentEvent = new Event("none/-1000 -1000/farmer 2 9 0/pause 250/message \"" + text + "\"/pause 500/animalNaming/pause 500/end");
					break;
				}
				if (!hasShownIncubatorBuildingFullMessage)
				{
					hasShownIncubatorBuildingFullMessage = true;
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_HouseFull"));
				}
			}
			base.resetSharedState();
		}

		public Building getBuilding()
		{
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building.indoors.Value != null && building.indoors.Value.Equals(this))
				{
					return building;
				}
			}
			return null;
		}

		public void addNewHatchedAnimal(string name)
		{
			bool flag = false;
			foreach (Object value in objects.Values)
			{
				if (!value.bigCraftable || !value.Name.Contains("Incubator") || value.heldObject.Value == null || (int)value.minutesUntilReady > 0 || isFull())
				{
					continue;
				}
				flag = true;
				string text = "??";
				if (value.heldObject.Value == null)
				{
					text = "White Chicken";
				}
				else
				{
					switch (value.heldObject.Value.ParentSheetIndex)
					{
					case 305:
						text = "Void Chicken";
						break;
					case 174:
					case 176:
						text = "White Chicken";
						break;
					case 180:
					case 182:
						text = "Brown Chicken";
						break;
					case 442:
						text = "Duck";
						break;
					case 107:
						text = "Dinosaur";
						break;
					case 289:
						text = "Ostrich";
						break;
					case 928:
						text = "Golden Chicken";
						break;
					}
				}
				FarmAnimal farmAnimal = new FarmAnimal(text, Game1.multiplayer.getNewID(), Game1.player.uniqueMultiplayerID);
				farmAnimal.Name = name;
				farmAnimal.displayName = name;
				Building building2 = (farmAnimal.home = getBuilding());
				farmAnimal.homeLocation.Value = new Vector2((int)building2.tileX, (int)building2.tileY);
				farmAnimal.setRandomPosition(farmAnimal.home.indoors);
				(building2.indoors.Value as AnimalHouse).animals.Add(farmAnimal.myID, farmAnimal);
				(building2.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(farmAnimal.myID);
				value.heldObject.Value = null;
				value.ParentSheetIndex = 101;
				if (text == "Ostrich")
				{
					value.ParentSheetIndex = 254;
				}
				break;
			}
			if (!flag && Game1.farmEvent != null && Game1.farmEvent is QuestionEvent)
			{
				FarmAnimal farmAnimal2 = new FarmAnimal((Game1.farmEvent as QuestionEvent).animal.type.Value, Game1.multiplayer.getNewID(), Game1.player.uniqueMultiplayerID);
				farmAnimal2.Name = name;
				farmAnimal2.displayName = name;
				farmAnimal2.parentId.Value = (Game1.farmEvent as QuestionEvent).animal.myID;
				Building building4 = (farmAnimal2.home = getBuilding());
				farmAnimal2.homeLocation.Value = new Vector2((int)building4.tileX, (int)building4.tileY);
				(Game1.farmEvent as QuestionEvent).forceProceed = true;
				farmAnimal2.setRandomPosition(farmAnimal2.home.indoors);
				(building4.indoors.Value as AnimalHouse).animals.Add(farmAnimal2.myID, farmAnimal2);
				(building4.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(farmAnimal2.myID);
			}
			Game1.exitActiveMenu();
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (character != null && !character.Equals(pair.Value) && position.Intersects(pair.Value.GetBoundingBox()) && (!isFarmer || !Game1.player.GetBoundingBox().Intersects(pair.Value.GetBoundingBox())))
				{
					if (isFarmer && (character as Farmer).TemporaryPassableTiles.Intersects(position))
					{
						break;
					}
					pair.Value.farmerPushing();
					return true;
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				_tempAnimals.Add(pair);
			}
			foreach (KeyValuePair<long, FarmAnimal> tempAnimal in _tempAnimals)
			{
				if (tempAnimal.Value.updateWhenCurrentLocation(time, this))
				{
					animals.Remove(tempAnimal.Key);
				}
			}
			_tempAnimals.Clear();
		}

		public void resetPositionsOfAllAnimals()
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				pair.Value.setRandomPosition(this);
			}
		}

		public override bool dropObject(Object obj, Vector2 location, xTile.Dimensions.Rectangle viewport, bool initialPlacement, Farmer who = null)
		{
			Vector2 key = new Vector2((int)(location.X / 64f), (int)(location.Y / 64f));
			if (obj.Name.Equals("Hay") && doesTileHaveProperty((int)key.X, (int)key.Y, "Trough", "Back") != null)
			{
				if (!objects.ContainsKey(key))
				{
					objects.Add(key, obj);
					return true;
				}
				return false;
			}
			return base.dropObject(obj, location, viewport, initialPlacement);
		}

		public void feedAllAnimals()
		{
			int num = 0;
			for (int i = 0; i < map.Layers[0].LayerWidth; i++)
			{
				for (int j = 0; j < map.Layers[0].LayerHeight; j++)
				{
					if (doesTileHaveProperty(i, j, "Trough", "Back") != null)
					{
						Vector2 key = new Vector2(i, j);
						if (!objects.ContainsKey(key) && (int)Game1.getFarm().piecesOfHay > 0)
						{
							objects.Add(key, new Object(178, 1));
							num++;
							Game1.getFarm().piecesOfHay.Value--;
						}
						if (num >= (int)animalLimit)
						{
							return;
						}
					}
				}
			}
		}

		public override void DayUpdate(int dayOfMonth)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				pair.Value.dayUpdate(this);
			}
			base.DayUpdate(dayOfMonth);
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if (t is MeleeWeapon)
			{
				foreach (FarmAnimal value in animals.Values)
				{
					if (value.GetBoundingBox().Intersects((t as MeleeWeapon).mostRecentArea))
					{
						value.hitWithWeapon(t as MeleeWeapon);
					}
				}
			}
			return false;
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				pair.Value.draw(b);
			}
		}
	}
}
