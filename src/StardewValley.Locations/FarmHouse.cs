using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations
{
	public class FarmHouse : DecoratableLocation
	{
		public int farmerNumberOfOwner;

		[XmlElement("fireplaceOn")]
		public readonly NetBool fireplaceOn = new NetBool();

		[XmlElement("fridge")]
		public readonly NetRef<Chest> fridge = new NetRef<Chest>(new Chest(playerChest: true));

		[XmlIgnore]
		public readonly NetInt synchronizedDisplayedLevel = new NetInt(-1);

		public Point fridgePosition;

		[XmlIgnore]
		public Point spouseRoomSpot;

		[XmlIgnore]
		private LocalizedContentManager mapLoader;

		public List<Warp> cellarWarps;

		[XmlElement("cribStyle")]
		public readonly NetInt cribStyle = new NetInt(1);

		[XmlIgnore]
		public int previousUpgradeLevel = -1;

		private int currentlyDisplayedUpgradeLevel;

		private bool displayingSpouseRoom;

		[XmlIgnore]
		public virtual Farmer owner => Game1.MasterPlayer;

		[XmlIgnore]
		public virtual int upgradeLevel
		{
			get
			{
				if (owner == null)
				{
					return 0;
				}
				return owner.houseUpgradeLevel;
			}
			set
			{
				if (owner != null)
				{
					owner.houseUpgradeLevel.Value = value;
				}
			}
		}

		public FarmHouse()
		{
		}

		public FarmHouse(int ownerNumber = 1)
		{
			farmerNumberOfOwner = ownerNumber;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(fireplaceOn, fridge, cribStyle, synchronizedDisplayedLevel);
			fireplaceOn.fieldChangeVisibleEvent += delegate(NetBool field, bool oldValue, bool newValue)
			{
				Point fireplacePoint = getFireplacePoint();
				setFireplace(newValue, fireplacePoint.X, fireplacePoint.Y);
			};
			cribStyle.InterpolationEnabled = false;
			cribStyle.fieldChangeVisibleEvent += delegate
			{
				if (map != null)
				{
					if (_appliedMapOverrides != null && _appliedMapOverrides.Contains("crib"))
					{
						_appliedMapOverrides.Remove("crib");
					}
					UpdateChildRoom();
					ReadWallpaperAndFloorTileData();
					setWallpapers();
					setFloors();
				}
			};
		}

		public List<Child> getChildren()
		{
			return (from n in characters
				where n is Child
				select n as Child).ToList();
		}

		public int getChildrenCount()
		{
			int num = 0;
			foreach (NPC character in characters)
			{
				if (character is Child)
				{
					num++;
				}
			}
			return num;
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding);
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		public override bool isTileLocationTotallyClearAndPlaceable(Vector2 v)
		{
			return base.isTileLocationTotallyClearAndPlaceable(v);
		}

		public override void performTenMinuteUpdate(int timeOfDay)
		{
			base.performTenMinuteUpdate(timeOfDay);
			foreach (NPC character in characters)
			{
				if (character.isMarried())
				{
					if (character.getSpouse() == Game1.player)
					{
						character.checkForMarriageDialogue(timeOfDay, this);
					}
					if (Game1.IsMasterGame && Game1.timeOfDay >= 2200 && Game1.IsMasterGame && character.getTileLocationPoint() != getSpouseBedSpot(character.Name) && (timeOfDay == 2200 || (character.controller == null && timeOfDay % 100 % 30 == 0)))
					{
						Point spouseBedSpot = getSpouseBedSpot(character.Name);
						character.controller = null;
						PathFindController.endBehavior endBehaviorFunction = null;
						bool flag = GetSpouseBed() != null;
						if (flag)
						{
							endBehaviorFunction = spouseSleepEndFunction;
						}
						character.controller = new PathFindController(character, this, spouseBedSpot, 0, endBehaviorFunction);
						if (character.controller.pathToEndPoint == null || !isTileOnMap(character.controller.pathToEndPoint.Last().X, character.controller.pathToEndPoint.Last().Y))
						{
							character.controller = null;
						}
						else if (flag)
						{
							foreach (Furniture item in furniture)
							{
								if (item is BedFurniture && item.getBoundingBox(item.TileLocation).Intersects(new Microsoft.Xna.Framework.Rectangle(spouseBedSpot.X * 64, spouseBedSpot.Y * 64, 64, 64)))
								{
									(item as BedFurniture).ReserveForNPC();
									break;
								}
							}
						}
					}
				}
				if (character is Child)
				{
					(character as Child).tenMinuteUpdate();
				}
			}
		}

		public static void spouseSleepEndFunction(Character c, GameLocation location)
		{
			if (c == null || !(c is NPC))
			{
				return;
			}
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions");
			if (dictionary.ContainsKey(c.name.Value.ToLower() + "_sleep"))
			{
				(c as NPC).playSleepingAnimation();
			}
			foreach (Furniture item in location.furniture)
			{
				if (item is BedFurniture && item.getBoundingBox(item.TileLocation).Intersects(c.GetBoundingBox()))
				{
					(item as BedFurniture).ReserveForNPC();
					break;
				}
			}
		}

		public virtual Point getFrontDoorSpot()
		{
			foreach (Warp warp in warps)
			{
				if (warp.TargetName == "Farm")
				{
					if (this is Cabin)
					{
						return new Point(warp.TargetX, warp.TargetY);
					}
					if (warp.TargetX == 64 && warp.TargetY == 15)
					{
						return Game1.getFarm().GetMainFarmHouseEntry();
					}
					return new Point(warp.TargetX, warp.TargetY);
				}
			}
			return Game1.getFarm().GetMainFarmHouseEntry();
		}

		public virtual Point getPorchStandingSpot()
		{
			int num = farmerNumberOfOwner;
			if ((uint)num <= 1u)
			{
				Point mainFarmHouseEntry = Game1.getFarm().GetMainFarmHouseEntry();
				mainFarmHouseEntry.X += 2;
				return mainFarmHouseEntry;
			}
			return new Point(-1000, -1000);
		}

		public Point getKitchenStandingSpot()
		{
			switch (upgradeLevel)
			{
			case 1:
				return GetMapPropertyPosition("KitchenStandingLocation", 4, 5);
			case 2:
			case 3:
				return GetMapPropertyPosition("KitchenStandingLocation", 7, 14);
			default:
				return GetMapPropertyPosition("KitchenStandingLocation", -1000, -1000);
			}
		}

		public virtual BedFurniture GetSpouseBed()
		{
			if (owner.getSpouse() != null && owner.getSpouse().Name == "Krobus")
			{
				return null;
			}
			if (owner != null && owner.hasCurrentOrPendingRoommate())
			{
				BedFurniture bed = GetBed(BedFurniture.BedType.Single);
				if (bed != null)
				{
					return GetBed(BedFurniture.BedType.Single);
				}
			}
			return GetBed(BedFurniture.BedType.Double);
		}

		public Point getSpouseBedSpot(string spouseName)
		{
			if ((spouseName == "Krobus" && SocialPage.isRoommateOfAnyone(spouseName)) || GetSpouseBed() == null)
			{
				return GetSpouseRoomSpot();
			}
			BedFurniture spouseBed = GetSpouseBed();
			Point bedSpot = GetSpouseBed().GetBedSpot();
			if (spouseBed.bedType == BedFurniture.BedType.Double)
			{
				bedSpot.X++;
			}
			return bedSpot;
		}

		public Point GetSpouseRoomSpot()
		{
			if (upgradeLevel == 0)
			{
				return new Point(-1000, -1000);
			}
			return spouseRoomSpot;
		}

		public BedFurniture GetBed(BedFurniture.BedType bed_type = BedFurniture.BedType.Any, int index = 0)
		{
			foreach (Furniture item in furniture)
			{
				if (!(item is BedFurniture))
				{
					continue;
				}
				BedFurniture bedFurniture = item as BedFurniture;
				if (bed_type == BedFurniture.BedType.Any || bedFurniture.bedType == bed_type)
				{
					if (index == 0)
					{
						return bedFurniture;
					}
					index--;
				}
			}
			return null;
		}

		public Point GetPlayerBedSpot()
		{
			return GetPlayerBed()?.GetBedSpot() ?? getEntryLocation();
		}

		public BedFurniture GetPlayerBed()
		{
			if (upgradeLevel == 0)
			{
				return GetBed(BedFurniture.BedType.Single);
			}
			return GetBed(BedFurniture.BedType.Double);
		}

		public Point getBedSpot(BedFurniture.BedType bed_type = BedFurniture.BedType.Any)
		{
			return GetBed(bed_type)?.GetBedSpot() ?? new Point(-1000, -1000);
		}

		public Point getEntryLocation()
		{
			switch (upgradeLevel)
			{
			case 0:
				return GetMapPropertyPosition("EntryLocation", 3, 11);
			case 1:
				return GetMapPropertyPosition("EntryLocation", 9, 11);
			case 2:
			case 3:
				return GetMapPropertyPosition("EntryLocation", 12, 20);
			default:
				return new Point(-1000, -1000);
			}
		}

		public BedFurniture GetChildBed(int index)
		{
			return GetBed(BedFurniture.BedType.Child, index);
		}

		public Point GetChildBedSpot(int index)
		{
			return GetChildBed(index)?.GetBedSpot() ?? Point.Zero;
		}

		public override bool isTilePlaceable(Vector2 v, Item item = null)
		{
			if (isTileOnMap(v) && getTileIndexAt((int)v.X, (int)v.Y, "Back") == 0 && getTileSheetIDAt((int)v.X, (int)v.Y, "Back") == "indoor")
			{
				return false;
			}
			return base.isTilePlaceable(v, item);
		}

		public Point getRandomOpenPointInHouse(Random r, int buffer = 0, int tries = 30)
		{
			Point zero = Point.Zero;
			for (int i = 0; i < tries; i++)
			{
				zero = new Point(r.Next(map.Layers[0].LayerWidth), r.Next(map.Layers[0].LayerHeight));
				Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(zero.X - buffer, zero.Y - buffer, 1 + buffer * 2, 1 + buffer * 2);
				bool flag = false;
				for (int j = rectangle.X; j < rectangle.Right; j++)
				{
					for (int k = rectangle.Y; k < rectangle.Bottom; k++)
					{
						flag = getTileIndexAt(j, k, "Back") == -1 || !isTileLocationTotallyClearAndPlaceable(j, k) || isTileOnWall(j, k);
						if (getTileIndexAt(j, k, "Back") == 0 && getTileSheetIDAt(j, k, "Back") == "indoor")
						{
							flag = true;
						}
						if (flag)
						{
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (!flag)
				{
					return zero;
				}
			}
			return Point.Zero;
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action != null && who.IsLocalPlayer)
			{
				string[] array = action.Split(' ');
				string text = array[0];
				if (text == "kitchen")
				{
					ActivateKitchen(fridge);
					return true;
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null)
			{
				switch (map.GetLayer("Buildings").Tiles[tileLocation].TileIndex)
				{
				case 173:
					fridge.Value.fridge.Value = true;
					fridge.Value.checkForAction(who);
					return true;
				case 794:
				case 795:
				case 796:
				case 797:
					fireplaceOn.Value = !fireplaceOn;
					return true;
				case 2173:
					if (Game1.player.eventsSeen.Contains(463391) && Game1.player.spouse != null && Game1.player.spouse.Equals("Emily"))
					{
						TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(5858585);
						if (temporarySpriteByID != null && temporarySpriteByID is EmilysParrot)
						{
							(temporarySpriteByID as EmilysParrot).doAction();
						}
					}
					return true;
				}
			}
			if (base.checkAction(tileLocation, viewport, who))
			{
				return true;
			}
			return false;
		}

		public FarmHouse(string m, string name)
			: base(m, name)
		{
			ReadWallpaperAndFloorTileData();
			furniture.Add(new BedFurniture(BedFurniture.DEFAULT_BED_INDEX, new Vector2(9f, 8f)));
			if (Game1.getFarm().getMapProperty("FarmHouseFurniture") != "")
			{
				if (!int.TryParse(Game1.getFarm().getMapProperty("FarmHouseWallpaper"), out var result))
				{
					result = -1;
				}
				if (result >= 0)
				{
					setWallpaper(result, -1, persist: true);
				}
				if (!int.TryParse(Game1.getFarm().getMapProperty("FarmHouseFlooring"), out result))
				{
					result = -1;
				}
				if (result >= 0)
				{
					setFloor(result, -1, persist: true);
				}
				if (Game1.getFarm().getMapProperty("FarmHouseStarterSeedsPosition") != "")
				{
					int result2 = 3;
					int result3 = 7;
					string[] array = Game1.getFarm().getMapProperty("FarmHouseStarterSeedsPosition").Split(' ');
					if (array.Length == 2)
					{
						if (!int.TryParse(array[0], out result2))
						{
							result2 = 3;
						}
						if (!int.TryParse(array[1], out result3))
						{
							result3 = 7;
						}
					}
					objects.Add(new Vector2(result2, result3), new Chest(0, new List<Item>
					{
						new Object(472, 15)
					}, new Vector2(result2, result3), giftbox: true));
				}
				else
				{
					objects.Add(new Vector2(3f, 7f), new Chest(0, new List<Item>
					{
						new Object(472, 15)
					}, new Vector2(3f, 7f), giftbox: true));
				}
				string mapProperty = Game1.getFarm().getMapProperty("FarmHouseFurniture");
				string[] array2 = mapProperty.Split(' ');
				for (int i = 0; i < array2.Length; i += 4)
				{
					int result4 = -1;
					int result5 = 0;
					if (!int.TryParse(array2[i], out result4))
					{
						result4 = -1;
					}
					if (!int.TryParse(array2[i + 1], out var result6))
					{
						result4 = -1;
					}
					if (!int.TryParse(array2[i + 2], out var result7))
					{
						result4 = -1;
					}
					if (!int.TryParse(array2[i + 3], out result5))
					{
						result4 = -1;
					}
					if (result4 >= 0)
					{
						Furniture furnitureAt = GetFurnitureAt(new Vector2(result6, result7));
						if (furnitureAt != null)
						{
							furnitureAt.heldObject.Value = new Furniture(result4, new Vector2(result6, result7), result5);
						}
						else
						{
							furniture.Add(new Furniture(result4, new Vector2(result6, result7), result5));
						}
					}
				}
				return;
			}
			switch (Game1.whichFarm)
			{
			case 0:
				furniture.Add(new Furniture(1120, new Vector2(5f, 4f)));
				furniture.Last().heldObject.Value = new Furniture(1364, new Vector2(5f, 4f));
				furniture.Add(new Furniture(1376, new Vector2(1f, 10f)));
				furniture.Add(new Furniture(0, new Vector2(4f, 4f)));
				furniture.Add(new TV(1466, new Vector2(1f, 4f)));
				furniture.Add(new Furniture(1614, new Vector2(3f, 1f)));
				furniture.Add(new Furniture(1618, new Vector2(6f, 8f)));
				furniture.Add(new Furniture(1602, new Vector2(5f, 1f)));
				furniture.Add(new Furniture(1792, Utility.PointToVector2(getFireplacePoint())));
				objects.Add(new Vector2(3f, 7f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(3f, 7f), giftbox: true));
				break;
			case 1:
				setWallpaper(11, -1, persist: true);
				setFloor(1, -1, persist: true);
				furniture.Add(new Furniture(1122, new Vector2(1f, 6f)));
				furniture.Last().heldObject.Value = new Furniture(1367, new Vector2(1f, 6f));
				furniture.Add(new Furniture(3, new Vector2(1f, 5f)));
				furniture.Add(new TV(1680, new Vector2(5f, 4f)));
				furniture.Add(new Furniture(1673, new Vector2(1f, 1f)));
				furniture.Add(new Furniture(1673, new Vector2(3f, 1f)));
				furniture.Add(new Furniture(1676, new Vector2(5f, 1f)));
				furniture.Add(new Furniture(1737, new Vector2(6f, 8f)));
				furniture.Add(new Furniture(1742, new Vector2(5f, 5f)));
				furniture.Add(new Furniture(1792, Utility.PointToVector2(getFireplacePoint())));
				furniture.Add(new Furniture(1675, new Vector2(10f, 1f)));
				objects.Add(new Vector2(4f, 7f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(4f, 7f), giftbox: true));
				break;
			case 2:
				setWallpaper(92, -1, persist: true);
				setFloor(34, -1, persist: true);
				furniture.Add(new Furniture(1134, new Vector2(1f, 7f)));
				furniture.Last().heldObject.Value = new Furniture(1748, new Vector2(1f, 7f));
				furniture.Add(new Furniture(3, new Vector2(1f, 6f)));
				furniture.Add(new TV(1680, new Vector2(6f, 4f)));
				furniture.Add(new Furniture(1296, new Vector2(1f, 4f)));
				furniture.Add(new Furniture(1682, new Vector2(3f, 1f)));
				furniture.Add(new Furniture(1777, new Vector2(6f, 5f)));
				furniture.Add(new Furniture(1745, new Vector2(6f, 1f)));
				furniture.Add(new Furniture(1792, Utility.PointToVector2(getFireplacePoint())));
				furniture.Add(new Furniture(1747, new Vector2(5f, 4f)));
				furniture.Add(new Furniture(1296, new Vector2(10f, 4f)));
				objects.Add(new Vector2(4f, 7f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(4f, 7f), giftbox: true));
				break;
			case 3:
				setWallpaper(12, -1, persist: true);
				setFloor(18, -1, persist: true);
				furniture.Add(new Furniture(1218, new Vector2(1f, 6f)));
				furniture.Last().heldObject.Value = new Furniture(1368, new Vector2(1f, 6f));
				furniture.Add(new Furniture(1755, new Vector2(1f, 5f)));
				furniture.Add(new Furniture(1755, new Vector2(3f, 6f), 1));
				furniture.Add(new TV(1680, new Vector2(5f, 4f)));
				furniture.Add(new Furniture(1751, new Vector2(5f, 10f)));
				furniture.Add(new Furniture(1749, new Vector2(3f, 1f)));
				furniture.Add(new Furniture(1753, new Vector2(5f, 1f)));
				furniture.Add(new Furniture(1742, new Vector2(5f, 5f)));
				objects.Add(new Vector2(2f, 9f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(2f, 9f), giftbox: true));
				furniture.Add(new Furniture(1794, Utility.PointToVector2(getFireplacePoint())));
				break;
			case 4:
				setWallpaper(95, -1, persist: true);
				setFloor(4, -1, persist: true);
				furniture.Add(new TV(1680, new Vector2(1f, 4f)));
				furniture.Add(new Furniture(1628, new Vector2(1f, 5f)));
				furniture.Add(new Furniture(1393, new Vector2(3f, 4f)));
				furniture.Last().heldObject.Value = new Furniture(1369, new Vector2(3f, 4f));
				furniture.Add(new Furniture(1678, new Vector2(10f, 1f)));
				furniture.Add(new Furniture(1812, new Vector2(3f, 1f)));
				furniture.Add(new Furniture(1630, new Vector2(1f, 1f)));
				furniture.Add(new Furniture(1794, Utility.PointToVector2(getFireplacePoint())));
				furniture.Add(new Furniture(1811, new Vector2(6f, 1f)));
				furniture.Add(new Furniture(1389, new Vector2(10f, 4f)));
				objects.Add(new Vector2(4f, 7f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(4f, 7f), giftbox: true));
				furniture.Add(new Furniture(1758, new Vector2(1f, 10f)));
				break;
			case 5:
				setWallpaper(65, -1, persist: true);
				setFloor(5, -1, persist: true);
				furniture.Add(new TV(1466, new Vector2(1f, 4f)));
				furniture.Add(new Furniture(1792, Utility.PointToVector2(getFireplacePoint())));
				furniture.Add(new Furniture(1614, new Vector2(3f, 1f)));
				furniture.Add(new Furniture(1614, new Vector2(6f, 1f)));
				furniture.Add(new Furniture(1601, new Vector2(10f, 1f)));
				furniture.Add(new Furniture(202, new Vector2(3f, 4f), 1));
				furniture.Add(new Furniture(1124, new Vector2(4f, 4f), 1));
				furniture.Last().heldObject.Value = new Furniture(1379, new Vector2(5f, 4f));
				furniture.Add(new Furniture(202, new Vector2(6f, 4f), 3));
				furniture.Add(new Furniture(1378, new Vector2(10f, 4f)));
				furniture.Add(new Furniture(1377, new Vector2(1f, 9f)));
				furniture.Add(new Furniture(1445, new Vector2(1f, 10f)));
				furniture.Add(new Furniture(1618, new Vector2(2f, 9f)));
				objects.Add(new Vector2(3f, 7f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(3f, 7f), giftbox: true));
				break;
			case 6:
				setWallpaper(106, -1, persist: true);
				setFloor(35, -1, persist: true);
				furniture.Add(new TV(1680, new Vector2(4f, 4f)));
				furniture.Add(new Furniture(1614, new Vector2(7f, 1f)));
				furniture.Add(new Furniture(1294, new Vector2(3f, 4f)));
				furniture.Add(new Furniture(1283, new Vector2(1f, 4f)));
				furniture.Add(new Furniture(1614, new Vector2(8f, 1f)));
				furniture.Add(new Furniture(202, new Vector2(7f, 4f)));
				furniture.Add(new Furniture(1294, new Vector2(10f, 4f)));
				furniture.Add(new Furniture(6, new Vector2(2f, 6f), 1));
				furniture.Add(new Furniture(6, new Vector2(5f, 7f), 3));
				furniture.Add(new Furniture(1124, new Vector2(3f, 6f)));
				furniture.Last().heldObject.Value = new Furniture(1362, new Vector2(4f, 6f));
				objects.Add(new Vector2(8f, 6f), new Chest(0, new List<Item>
				{
					new Object(472, 15)
				}, new Vector2(8f, 6f), giftbox: true));
				furniture.Add(new Furniture(1228, new Vector2(2f, 9f)));
				break;
			}
		}

		public bool hasActiveFireplace()
		{
			for (int i = 0; i < furniture.Count(); i++)
			{
				if ((int)furniture[i].furniture_type == 14 && (bool)furniture[i].isOn)
				{
					return true;
				}
			}
			return false;
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			if (!Game1.IsMasterGame)
			{
				return;
			}
			foreach (NPC character in characters)
			{
				Farmer spouse = character.getSpouse();
				if (spouse == null || spouse != owner)
				{
					continue;
				}
				NPC nPC = character;
				if (nPC == null || Game1.timeOfDay >= 1500 || !(Game1.random.NextDouble() < 0.0006) || nPC.controller != null || nPC.Schedule != null || nPC.getTileLocation().Equals(Utility.PointToVector2(getSpouseBedSpot(Game1.player.spouse))) || base.furniture.Count <= 0)
				{
					continue;
				}
				Furniture furniture = base.furniture[Game1.random.Next(base.furniture.Count)];
				Microsoft.Xna.Framework.Rectangle rectangle = furniture.boundingBox;
				Vector2 v = new Vector2(rectangle.X / 64, rectangle.Y / 64);
				if (furniture.furniture_type.Value == 15 || furniture.furniture_type.Value == 12)
				{
					continue;
				}
				int i = 0;
				int finalFacingDirection = -3;
				for (; i < 3; i++)
				{
					int num = Game1.random.Next(-1, 2);
					int num2 = Game1.random.Next(-1, 2);
					v.X += num;
					if (num == 0)
					{
						v.Y += num2;
					}
					switch (num)
					{
					case -1:
						finalFacingDirection = 1;
						break;
					case 1:
						finalFacingDirection = 3;
						break;
					default:
						switch (num2)
						{
						case -1:
							finalFacingDirection = 2;
							break;
						case 1:
							finalFacingDirection = 0;
							break;
						}
						break;
					}
					if (isTileLocationTotallyClearAndPlaceable(v))
					{
						break;
					}
				}
				if (i < 3)
				{
					nPC.controller = new PathFindController(nPC, this, new Point((int)v.X, (int)v.Y), finalFacingDirection, eraseOldPathController: false, clearMarriageDialogues: false);
				}
			}
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (wasUpdated)
			{
				return;
			}
			base.UpdateWhenCurrentLocation(time);
			fridge.Value.updateWhenCurrentLocation(time, this);
			if (!Game1.player.isMarried() || Game1.player.spouse == null)
			{
				return;
			}
			NPC characterFromName = getCharacterFromName(Game1.player.spouse);
			if (characterFromName == null || characterFromName.isEmoting)
			{
				return;
			}
			Vector2 tileLocation = characterFromName.getTileLocation();
			Vector2[] adjacentTilesOffsets = Character.AdjacentTilesOffsets;
			foreach (Vector2 vector in adjacentTilesOffsets)
			{
				Vector2 vector2 = tileLocation + vector;
				NPC nPC = isCharacterAtTile(vector2);
				if (nPC != null && nPC.IsMonster && !nPC.Name.Equals("Cat"))
				{
					characterFromName.faceGeneralDirection(vector2 * new Vector2(64f, 64f));
					Game1.showSwordswipeAnimation(characterFromName.FacingDirection, characterFromName.Position, 60f, flip: false);
					localSound("swordswipe");
					characterFromName.shake(500);
					characterFromName.showTextAboveHead(Game1.content.LoadString("Strings\\Locations:FarmHouse_SpouseAttacked" + (Game1.random.Next(12) + 1)));
					((Monster)nPC).takeDamage(50, (int)Utility.getAwayFromPositionTrajectory(nPC.GetBoundingBox(), characterFromName.Position).X, (int)Utility.getAwayFromPositionTrajectory(nPC.GetBoundingBox(), characterFromName.Position).Y, isBomb: false, 1.0, Game1.player);
					if (((Monster)nPC).Health <= 0)
					{
						debris.Add(new Debris(nPC.Sprite.textureName, Game1.random.Next(6, 16), new Vector2(nPC.getStandingX(), nPC.getStandingY())));
						monsterDrop((Monster)nPC, nPC.getStandingX(), nPC.getStandingY(), owner);
						characters.Remove(nPC);
						Game1.stats.MonstersKilled++;
						Game1.player.changeFriendship(-10, characterFromName);
					}
					else
					{
						((Monster)nPC).shedChunks(4);
					}
					characterFromName.CurrentDialogue.Clear();
					characterFromName.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Data\\ExtraDialogue:Spouse_MonstersInHouse"), characterFromName));
				}
			}
		}

		public Point getFireplacePoint()
		{
			switch (upgradeLevel)
			{
			case 0:
				return new Point(8, 4);
			case 1:
				return new Point(26, 4);
			case 2:
			case 3:
				return new Point(2, 13);
			default:
				return new Point(-50, -50);
			}
		}

		public bool shouldShowSpouseRoom()
		{
			return owner.isMarried();
		}

		public virtual void showSpouseRoom()
		{
			bool flag = owner.isMarried() && owner.spouse != null;
			bool flag2 = displayingSpouseRoom;
			displayingSpouseRoom = flag;
			updateMap();
			if (flag2 && !displayingSpouseRoom)
			{
				Point spouseRoomCorner = GetSpouseRoomCorner();
				Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(spouseRoomCorner.X, spouseRoomCorner.Y, GetSpouseRoomWidth(), GetSpouseRoomHeight());
				rectangle.X--;
				List<Item> list = new List<Item>();
				Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(rectangle.X * 64, rectangle.Y * 64, rectangle.Width * 64, rectangle.Height * 64);
				List<Furniture> list2 = new List<Furniture>(furniture);
				foreach (Furniture item in list2)
				{
					if (item.getBoundingBox(item.tileLocation).Intersects(value))
					{
						if (item is StorageFurniture)
						{
							StorageFurniture storageFurniture = item as StorageFurniture;
							list.AddRange(storageFurniture.heldItems);
							storageFurniture.heldItems.Clear();
						}
						if (item.heldObject.Value != null)
						{
							list.Add((Object)item.heldObject);
							item.heldObject.Value = null;
						}
						list.Add(item);
						furniture.Remove(item);
					}
				}
				for (int i = rectangle.X; i <= rectangle.Right; i++)
				{
					for (int j = rectangle.Y; j <= rectangle.Bottom; j++)
					{
						Object @object = getObjectAtTile(i, j);
						if (@object == null || @object is Furniture)
						{
							continue;
						}
						@object.performRemoveAction(new Vector2(i, j), this);
						if (@object is Fence)
						{
							@object = new Object(Vector2.Zero, (@object as Fence).GetItemParentSheetIndex(), 1);
						}
						if (@object is IndoorPot)
						{
							IndoorPot indoorPot = @object as IndoorPot;
							if (indoorPot.hoeDirt.Value != null && indoorPot.hoeDirt.Value.crop != null)
							{
								indoorPot.hoeDirt.Value.destroyCrop(indoorPot.tileLocation, showAnimation: false, this);
							}
						}
						else if (@object is Chest)
						{
							Chest chest = @object as Chest;
							list.AddRange(chest.items);
							chest.items.Clear();
						}
						if (@object.heldObject != null)
						{
							@object.heldObject.Value = null;
						}
						@object.minutesUntilReady.Value = -1;
						if (@object.readyForHarvest.Value)
						{
							@object.readyForHarvest.Value = false;
						}
						list.Add(@object);
						objects.Remove(new Vector2(i, j));
					}
				}
				if (upgradeLevel >= 2)
				{
					Utility.createOverflowChest(this, new Vector2(24f, 22f), list);
				}
				else
				{
					Utility.createOverflowChest(this, new Vector2(21f, 10f), list);
				}
			}
			loadObjects();
			if (upgradeLevel == 3)
			{
				AddCellarTiles();
				createCellarWarps();
				if (!Game1.player.craftingRecipes.ContainsKey("Cask"))
				{
					Game1.player.craftingRecipes.Add("Cask", 0);
				}
			}
			if (flag)
			{
				loadSpouseRoom();
			}
		}

		public virtual void AddCellarTiles()
		{
			if (_appliedMapOverrides.Contains("cellar"))
			{
				_appliedMapOverrides.Remove("cellar");
			}
			ApplyMapOverride("FarmHouse_Cellar", "cellar");
		}

		public string GetCellarName()
		{
			int num = -1;
			if (owner != null)
			{
				foreach (int key in Game1.player.team.cellarAssignments.Keys)
				{
					if (Game1.player.team.cellarAssignments[key] == owner.UniqueMultiplayerID)
					{
						num = key;
					}
				}
			}
			if (num >= 0 && num <= 1)
			{
				return "Cellar";
			}
			return "Cellar" + num;
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (Game1.timeOfDay >= 2200 && owner.spouse != null && getCharacterFromName(owner.spouse) != null && !owner.isEngaged())
			{
				Game1.player.team.requestSpouseSleepEvent.Fire(owner.UniqueMultiplayerID);
			}
			if (Game1.timeOfDay >= 2000 && owner.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID && Game1.getFarm().farmers.Count <= 1)
			{
				Game1.player.team.requestPetWarpHomeEvent.Fire(owner.UniqueMultiplayerID);
			}
			if (!Game1.IsMasterGame)
			{
				return;
			}
			Farm farm = Game1.getFarm();
			for (int num = characters.Count - 1; num >= 0; num--)
			{
				if (characters[num] is Pet && (!isTileOnMap(characters[num].getTileX(), characters[num].getTileY()) || getTileIndexAt(characters[num].GetBoundingBox().Left / 64, characters[num].getTileY(), "Buildings") != -1 || getTileIndexAt(characters[num].GetBoundingBox().Right / 64, characters[num].getTileY(), "Buildings") != -1))
				{
					characters[num].faceDirection(2);
					Game1.warpCharacter(characters[num], "Farm", farm.GetPetStartLocation());
					break;
				}
			}
			for (int num2 = characters.Count - 1; num2 >= 0; num2--)
			{
				for (int num3 = num2 - 1; num3 >= 0; num3--)
				{
					if (num2 < characters.Count && num3 < characters.Count && (characters[num3].Equals(characters[num2]) || (characters[num3].Name.Equals(characters[num2].Name) && characters[num3].isVillager() && characters[num2].isVillager())) && num3 != num2)
					{
						characters.RemoveAt(num3);
					}
				}
				for (int num4 = farm.characters.Count - 1; num4 >= 0; num4--)
				{
					if (num2 < characters.Count && num4 < characters.Count && farm.characters[num4].Equals(characters[num2]))
					{
						farm.characters.RemoveAt(num4);
					}
				}
			}
		}

		public void UpdateForRenovation()
		{
			updateFarmLayout();
			setWallpapers();
			setFloors();
		}

		public void updateFarmLayout()
		{
			if (currentlyDisplayedUpgradeLevel != upgradeLevel)
			{
				setMapForUpgradeLevel(upgradeLevel);
			}
			_ApplyRenovations();
			if ((!displayingSpouseRoom && shouldShowSpouseRoom()) || (displayingSpouseRoom && !shouldShowSpouseRoom()))
			{
				showSpouseRoom();
			}
			UpdateChildRoom();
			ReadWallpaperAndFloorTileData();
		}

		protected virtual void _ApplyRenovations()
		{
			if (upgradeLevel >= 2)
			{
				if (_appliedMapOverrides.Contains("bedroom_open"))
				{
					_appliedMapOverrides.Remove("bedroom_open");
				}
				if (owner.mailReceived.Contains("renovation_bedroom_open"))
				{
					ApplyMapOverride("FarmHouse_Bedroom_Open", "bedroom_open");
				}
				else
				{
					ApplyMapOverride("FarmHouse_Bedroom_Normal", "bedroom_open");
				}
				if (_appliedMapOverrides.Contains("southernroom_open"))
				{
					_appliedMapOverrides.Remove("southernroom_open");
				}
				if (owner.mailReceived.Contains("renovation_southern_open"))
				{
					ApplyMapOverride("FarmHouse_SouthernRoom_Add", "southernroom_open");
				}
				else
				{
					ApplyMapOverride("FarmHouse_SouthernRoom_Remove", "southernroom_open");
				}
				if (_appliedMapOverrides.Contains("cornerroom_open"))
				{
					_appliedMapOverrides.Remove("cornerroom_open");
				}
				if (owner.mailReceived.Contains("renovation_corner_open"))
				{
					ApplyMapOverride("FarmHouse_CornerRoom_Add", "cornerroom_open");
					if (displayingSpouseRoom)
					{
						setMapTile(34, 9, 229, "Front", null, 2);
					}
				}
				else
				{
					ApplyMapOverride("FarmHouse_CornerRoom_Remove", "cornerroom_open");
					if (displayingSpouseRoom)
					{
						setMapTile(34, 9, 87, "Front", null, 2);
					}
				}
			}
			if (!map.Properties.ContainsKey("AdditionalRenovations"))
			{
				return;
			}
			string text = map.Properties["AdditionalRenovations"].ToString();
			string[] array = text.Split(',');
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				string[] array3 = text2.Trim().Split(' ');
				if (array3.Length < 4)
				{
					continue;
				}
				string text3 = array3[0];
				string item = array3[1];
				string map_name = array3[2];
				string map_name2 = array3[3];
				Microsoft.Xna.Framework.Rectangle? destination_rect = null;
				if (array3.Length >= 8)
				{
					try
					{
						Microsoft.Xna.Framework.Rectangle value = default(Microsoft.Xna.Framework.Rectangle);
						value.X = int.Parse(array3[4]);
						value.Y = int.Parse(array3[5]);
						value.Width = int.Parse(array3[6]);
						value.Height = int.Parse(array3[7]);
						destination_rect = value;
					}
					catch (Exception)
					{
						destination_rect = null;
					}
				}
				if (_appliedMapOverrides.Contains(text3))
				{
					_appliedMapOverrides.Remove(text3);
				}
				if (owner.mailReceived.Contains(item))
				{
					ApplyMapOverride(map_name, text3, null, destination_rect);
				}
				else
				{
					ApplyMapOverride(map_name2, text3, null, destination_rect);
				}
			}
		}

		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			updateFarmLayout();
			setWallpapers();
			setFloors();
			if (owner.getSpouse() != null && owner.getSpouse().name.Equals("Sebastian") && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
			{
				Point spouseRoomCorner = GetSpouseRoomCorner();
				spouseRoomCorner.X++;
				spouseRoomCorner.Y += 6;
				Vector2 vector = Utility.PointToVector2(spouseRoomCorner);
				removeTile((int)vector.X, (int)vector.Y - 1, "Front");
				removeTile((int)vector.X + 1, (int)vector.Y - 1, "Front");
				removeTile((int)vector.X + 2, (int)vector.Y - 1, "Front");
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (owner.isMarried() && owner.spouse != null && owner.spouse.Equals("Emily") && Game1.player.eventsSeen.Contains(463391))
			{
				Vector2 location = new Vector2(2064f, 160f);
				int num = upgradeLevel;
				if ((uint)(num - 2) <= 1u)
				{
					location = new Vector2(2448f, 736f);
				}
				temporarySprites.Add(new EmilysParrot(location));
			}
			if (Game1.player.currentLocation == null || (!Game1.player.currentLocation.Equals(this) && !Game1.player.currentLocation.name.Value.StartsWith("Cellar")))
			{
				Game1.player.Position = Utility.PointToVector2(getEntryLocation()) * 64f;
				Game1.xLocationAfterWarp = Game1.player.getTileX();
				Game1.yLocationAfterWarp = Game1.player.getTileY();
				Game1.player.currentLocation = this;
			}
			foreach (NPC character in characters)
			{
				if (character is Child)
				{
					(character as Child).resetForPlayerEntry(this);
				}
				if (Game1.IsMasterGame && Game1.timeOfDay >= 2000 && !(character is Pet))
				{
					character.controller = null;
					character.Halt();
				}
			}
			if (owner == Game1.player && Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).HasValue && Game1.player.team.IsMarried(Game1.player.UniqueMultiplayerID) && !Game1.player.mailReceived.Contains("CF_Spouse"))
			{
				Vector2 vector = Utility.PointToVector2(getEntryLocation()) + new Vector2(0f, -1f);
				Chest value = new Chest(0, new List<Item>
				{
					new Object(434, 1)
				}, vector, giftbox: true, 1);
				overlayObjects[vector] = value;
			}
			if (owner != null && !owner.activeDialogueEvents.ContainsKey("pennyRedecorating"))
			{
				int num2 = -1;
				if (owner.mailReceived.Contains("pennyQuilt0"))
				{
					num2 = 0;
				}
				else if (owner.mailReceived.Contains("pennyQuilt1"))
				{
					num2 = 1;
				}
				else if (owner.mailReceived.Contains("pennyQuilt2"))
				{
					num2 = 2;
				}
			}
			if (owner.Equals(Game1.player) && !Game1.player.activeDialogueEvents.ContainsKey("pennyRedecorating"))
			{
				int num3 = -1;
				if (Game1.player.mailReceived.Contains("pennyQuilt0"))
				{
					num3 = 0;
				}
				else if (Game1.player.mailReceived.Contains("pennyQuilt1"))
				{
					num3 = 1;
				}
				else if (Game1.player.mailReceived.Contains("pennyQuilt2"))
				{
					num3 = 2;
				}
				if (num3 != -1 && !Game1.player.mailReceived.Contains("pennyRefurbished"))
				{
					List<Object> list = new List<Object>();
					foreach (Furniture item in furniture)
					{
						if (!(item is BedFurniture))
						{
							continue;
						}
						BedFurniture bedFurniture = item as BedFurniture;
						if (bedFurniture.bedType == BedFurniture.BedType.Double)
						{
							int num4 = -1;
							if (owner.mailReceived.Contains("pennyQuilt0"))
							{
								num4 = 2058;
							}
							if (owner.mailReceived.Contains("pennyQuilt1"))
							{
								num4 = 2064;
							}
							if (owner.mailReceived.Contains("pennyQuilt2"))
							{
								num4 = 2070;
							}
							if (num4 != -1)
							{
								Vector2 tileLocation = bedFurniture.TileLocation;
								bedFurniture.performRemoveAction(bedFurniture.tileLocation, this);
								list.Add(bedFurniture);
								Guid guid = furniture.GuidOf(bedFurniture);
								furniture.Remove(guid);
								furniture.Add(new BedFurniture(num4, new Vector2(tileLocation.X, tileLocation.Y)));
							}
							break;
						}
					}
					Game1.player.mailReceived.Add("pennyRefurbished");
					Microsoft.Xna.Framework.Rectangle empty = Microsoft.Xna.Framework.Rectangle.Empty;
					empty = ((upgradeLevel < 2) ? new Microsoft.Xna.Framework.Rectangle(20, 1, 8, 10) : new Microsoft.Xna.Framework.Rectangle(23, 10, 11, 13));
					for (int i = empty.X; i <= empty.Right; i++)
					{
						for (int j = empty.Y; j <= empty.Bottom; j++)
						{
							if (getObjectAtTile(i, j) == null)
							{
								continue;
							}
							Object @object = null;
							@object = getObjectAtTile(i, j);
							if (@object != null && !(@object is Chest) && !(@object is StorageFurniture) && !(@object is IndoorPot) && !(@object is BedFurniture))
							{
								if (@object.Name != null && @object.Name.Contains("Table") && @object.heldObject.Value != null)
								{
									Object value2 = @object.heldObject.Value;
									@object.heldObject.Value = null;
									list.Add(value2);
								}
								@object.performRemoveAction(new Vector2(i, j), this);
								if (@object is Fence)
								{
									@object = new Object(Vector2.Zero, (@object as Fence).GetItemParentSheetIndex(), 1);
								}
								list.Add(@object);
								objects.Remove(new Vector2(i, j));
								if (@object is Furniture)
								{
									furniture.Remove(@object as Furniture);
								}
							}
						}
					}
					decoratePennyRoom(num3, list);
				}
			}
			if (owner.getSpouse() == null || !owner.getSpouse().name.Equals("Sebastian") || !Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
			{
				return;
			}
			Point spouseRoomCorner = GetSpouseRoomCorner();
			spouseRoomCorner.X++;
			spouseRoomCorner.Y += 6;
			Vector2 vector2 = Utility.PointToVector2(spouseRoomCorner);
			temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = Game1.mouseCursors,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(641, 1534, 48, 37),
				animationLength = 1,
				sourceRectStartingPos = new Vector2(641f, 1534f),
				interval = 5000f,
				totalNumberOfLoops = 9999,
				position = vector2 * 64f + new Vector2(0f, -5f) * 4f,
				scale = 4f,
				layerDepth = (vector2.Y + 2f + 0.1f) * 64f / 10000f
			});
			if (Game1.random.NextDouble() < 0.85)
			{
				Texture2D texture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
				base.TemporarySprites.Add(new SebsFrogs
				{
					texture = texture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(64, 224, 16, 16),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(64f, 224f),
					interval = 100f,
					totalNumberOfLoops = 9999,
					position = vector2 * 64f + new Vector2((Game1.random.NextDouble() < 0.5) ? 22 : 25, (!(Game1.random.NextDouble() < 0.5)) ? 1 : 2) * 4f,
					scale = 4f,
					flipped = (Game1.random.NextDouble() < 0.5),
					layerDepth = (vector2.Y + 2f + 0.11f) * 64f / 10000f,
					Parent = this
				});
			}
			if (!Game1.player.activeDialogueEvents.ContainsKey("sebastianFrog2") && Game1.random.NextDouble() < 0.5)
			{
				Texture2D texture2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
				base.TemporarySprites.Add(new SebsFrogs
				{
					texture = texture2,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(64, 240, 16, 16),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(64f, 240f),
					interval = 150f,
					totalNumberOfLoops = 9999,
					position = vector2 * 64f + new Vector2(8f, 3f) * 4f,
					scale = 4f,
					layerDepth = (vector2.Y + 2f + 0.11f) * 64f / 10000f,
					flipped = (Game1.random.NextDouble() < 0.5),
					pingPong = false,
					Parent = this
				});
				if (Game1.random.NextDouble() < 0.1 && Game1.timeOfDay > 610)
				{
					DelayedAction.playSoundAfterDelay("croak", 1000);
				}
			}
		}

		private void addFurnitureIfSpaceIsFreePenny(List<Object> objectsToStoreInChests, Furniture f, Furniture heldObject = null)
		{
			bool flag = false;
			foreach (Furniture item in furniture)
			{
				if (f.getBoundingBox(f.tileLocation).Intersects(item.getBoundingBox(item.tileLocation)))
				{
					flag = true;
					break;
				}
			}
			if (objects.ContainsKey(f.TileLocation))
			{
				flag = true;
			}
			if (!flag)
			{
				furniture.Add(f);
				if (heldObject != null)
				{
					furniture.Last().heldObject.Value = heldObject;
				}
			}
			else
			{
				objectsToStoreInChests.Add(f);
				if (heldObject != null)
				{
					objectsToStoreInChests.Add(heldObject);
				}
			}
		}

		private void addFurnitureIfSpaceIsFree(Furniture f, Furniture heldObject = null)
		{
			if (!objects.ContainsKey(f.TileLocation))
			{
				furniture.Add(f);
				if (heldObject != null)
				{
					furniture.Last().heldObject.Value = heldObject;
				}
			}
		}

		private void decoratePennyRoom(int whichStyle, List<Object> objectsToStoreInChests)
		{
			List<Chest> list = new List<Chest>();
			List<Vector2> list2 = new List<Vector2>();
			Color value = default(Color);
			switch (whichStyle)
			{
			case 0:
				if (upgradeLevel == 1)
				{
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1916, new Vector2(20f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1914, new Vector2(21f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1915, new Vector2(22f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1914, new Vector2(23f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1916, new Vector2(24f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1682, new Vector2(26f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1747, new Vector2(25f, 4f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1395, new Vector2(26f, 4f)), new Furniture(1363, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1443, new Vector2(27f, 4f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1664, new Vector2(27f, 5f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1978, new Vector2(21f, 6f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1124, new Vector2(26f, 9f)), new Furniture(1368, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(6, new Vector2(25f, 10f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1296, new Vector2(28f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1747, new Vector2(24f, 10f)));
					SetWallpaper("107", "Bedroom");
					SetFloor("2", "Bedroom");
					value = new Color(85, 85, 255);
					list2.Add(new Vector2(21f, 10f));
					list2.Add(new Vector2(22f, 10f));
				}
				else
				{
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1916, new Vector2(23f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1914, new Vector2(24f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1604, new Vector2(26f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1915, new Vector2(28f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1916, new Vector2(30f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1914, new Vector2(32f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1916, new Vector2(33f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1443, new Vector2(23f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1747, new Vector2(24f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1395, new Vector2(25f, 13f)), new Furniture(1363, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(714, new Vector2(31f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1443, new Vector2(33f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1978, new Vector2(27f, 15f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1664, new Vector2(32f, 15f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1664, new Vector2(23f, 17f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1124, new Vector2(31f, 21f)), new Furniture(1368, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(416, new Vector2(25f, 22f), 2));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1296, new Vector2(23f, 22f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(6, new Vector2(30f, 22f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1296, new Vector2(33f, 22f)));
					SetWallpaper("107", "Bedroom");
					SetFloor("2", "Bedroom");
					value = new Color(85, 85, 255);
					list2.Add(new Vector2(23f, 14f));
					list2.Add(new Vector2(24f, 14f));
				}
				break;
			case 1:
				if (upgradeLevel == 1)
				{
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1678, new Vector2(20f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1814, new Vector2(21f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1814, new Vector2(22f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1814, new Vector2(23f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1907, new Vector2(24f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1400, new Vector2(25f, 4f)), new Furniture(1365, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1866, new Vector2(26f, 4f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1909, new Vector2(27f, 6f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1451, new Vector2(21f, 6f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1138, new Vector2(27f, 9f)), new Furniture(1378, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(12, new Vector2(26f, 10f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1758, new Vector2(24f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1618, new Vector2(21f, 9f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1390, new Vector2(22f, 10f)));
					SetWallpaper("84", "Bedroom");
					SetFloor("35", "Bedroom");
					value = new Color(255, 85, 85);
					list2.Add(new Vector2(21f, 10f));
					list2.Add(new Vector2(23f, 10f));
				}
				else
				{
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1678, new Vector2(24f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1907, new Vector2(25f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1814, new Vector2(27f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1814, new Vector2(28f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1814, new Vector2(29f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1907, new Vector2(30f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1916, new Vector2(33f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1758, new Vector2(23f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1400, new Vector2(25f, 13f)), new Furniture(1365, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1390, new Vector2(31f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1866, new Vector2(32f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1387, new Vector2(23f, 14f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1909, new Vector2(32f, 14f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(719, new Vector2(23f, 15f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1451, new Vector2(27f, 15f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1909, new Vector2(23f, 17f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1389, new Vector2(32f, 19f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1377, new Vector2(33f, 19f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1758, new Vector2(26f, 20f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(424, new Vector2(27f, 20f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1618, new Vector2(29f, 20f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(536, new Vector2(32f, 20f), 3));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1138, new Vector2(23f, 21f)), new Furniture(1378, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1383, new Vector2(26f, 21f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1449, new Vector2(33f, 22f)));
					SetWallpaper("84", "Bedroom");
					SetFloor("35", "Bedroom");
					value = new Color(255, 85, 85);
					list2.Add(new Vector2(24f, 13f));
					list2.Add(new Vector2(28f, 15f));
				}
				break;
			case 2:
				if (upgradeLevel == 1)
				{
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1673, new Vector2(20f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1547, new Vector2(21f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1675, new Vector2(24f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1900, new Vector2(25f, 1f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1393, new Vector2(25f, 4f)), new Furniture(1367, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1798, new Vector2(26f, 4f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1902, new Vector2(25f, 5f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1751, new Vector2(22f, 6f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1122, new Vector2(26f, 9f)), new Furniture(1378, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(197, new Vector2(28f, 9f), 3));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(3, new Vector2(25f, 10f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1294, new Vector2(20f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1294, new Vector2(24f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1964, new Vector2(21f, 8f)));
					SetWallpaper("95", "Bedroom");
					SetFloor("1", "Bedroom");
					value = new Color(85, 85, 85);
					list2.Add(new Vector2(22f, 10f));
					list2.Add(new Vector2(23f, 10f));
				}
				else
				{
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1673, new Vector2(23f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1675, new Vector2(25f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1547, new Vector2(27f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1900, new Vector2(30f, 10f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1751, new Vector2(23f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1393, new Vector2(25f, 13f)), new Furniture(1367, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1798, new Vector2(32f, 13f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1902, new Vector2(31f, 14f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1964, new Vector2(27f, 15f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1294, new Vector2(23f, 16f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(3, new Vector2(31f, 19f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1294, new Vector2(23f, 20f)));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(1122, new Vector2(31f, 20f)), new Furniture(1369, Vector2.Zero));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(197, new Vector2(33f, 20f), 3));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(709, new Vector2(23f, 21f), 1));
					addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, new Furniture(3, new Vector2(32f, 22f), 2));
					SetWallpaper("95", "Bedroom");
					SetFloor("1", "Bedroom");
					value = new Color(85, 85, 85);
					list2.Add(new Vector2(24f, 13f));
					list2.Add(new Vector2(31f, 13f));
				}
				break;
			}
			if (objectsToStoreInChests != null)
			{
				foreach (Object objectsToStoreInChest in objectsToStoreInChests)
				{
					if (list.Count == 0)
					{
						list.Add(new Chest(playerChest: true));
					}
					bool flag = false;
					foreach (Chest item in list)
					{
						if (item.addItem(objectsToStoreInChest) == null)
						{
							flag = true;
						}
					}
					if (!flag)
					{
						Chest chest = new Chest(playerChest: true);
						list.Add(chest);
						chest.addItem(objectsToStoreInChest);
					}
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				Chest chest2 = list[i];
				chest2.playerChoiceColor.Value = value;
				Vector2 tileLocation = list2[Math.Min(i, list2.Count - 1)];
				PlaceInNearbySpace(tileLocation, chest2);
			}
		}

		public void PlaceInNearbySpace(Vector2 tileLocation, Object o)
		{
			if (o == null || tileLocation.Equals(Vector2.Zero))
			{
				return;
			}
			int i = 0;
			Queue<Vector2> queue = new Queue<Vector2>();
			HashSet<Vector2> hashSet = new HashSet<Vector2>();
			queue.Enqueue(tileLocation);
			Vector2 vector = Vector2.Zero;
			for (; i < 100; i++)
			{
				vector = queue.Dequeue();
				if (!isTileOccupiedForPlacement(vector) && isTileLocationTotallyClearAndPlaceable(vector) && !isOpenWater((int)vector.X, (int)vector.Y))
				{
					break;
				}
				hashSet.Add(vector);
				foreach (Vector2 adjacentTileLocation in Utility.getAdjacentTileLocations(vector))
				{
					if (!hashSet.Contains(adjacentTileLocation))
					{
						queue.Enqueue(adjacentTileLocation);
					}
				}
			}
			if (!vector.Equals(Vector2.Zero) && !isTileOccupiedForPlacement(vector) && !isOpenWater((int)vector.X, (int)vector.Y) && isTileLocationTotallyClearAndPlaceable(vector))
			{
				o.tileLocation.Value = vector;
				objects.Add(vector, o);
			}
		}

		public virtual void RefreshFloorObjectNeighbors()
		{
			foreach (Vector2 key in terrainFeatures.Keys)
			{
				TerrainFeature terrainFeature = terrainFeatures[key];
				if (terrainFeature is Flooring)
				{
					Flooring flooring = terrainFeature as Flooring;
					flooring.OnAdded(this, key);
				}
			}
		}

		public void moveObjectsForHouseUpgrade(int whichUpgrade)
		{
			previousUpgradeLevel = upgradeLevel;
			overlayObjects.Clear();
			switch (whichUpgrade)
			{
			case 0:
				if (upgradeLevel == 1)
				{
					shiftObjects(-6, 0);
				}
				break;
			case 1:
				if (upgradeLevel == 0)
				{
					shiftObjects(6, 0);
				}
				if (upgradeLevel == 2)
				{
					shiftObjects(-3, 0);
				}
				break;
			case 2:
			case 3:
				if (upgradeLevel == 1)
				{
					shiftObjects(3, 9);
					foreach (Furniture item in furniture)
					{
						if (item.tileLocation.X >= 10f && item.tileLocation.X <= 13f && item.tileLocation.Y >= 10f && item.tileLocation.Y <= 11f)
						{
							item.tileLocation.X -= 3f;
							item.boundingBox.X -= 192;
							item.tileLocation.Y -= 9f;
							item.boundingBox.Y -= 576;
							item.updateDrawPosition();
						}
					}
					moveFurniture(27, 13, 1, 4);
					moveFurniture(28, 13, 2, 4);
					moveFurniture(29, 13, 3, 4);
					moveFurniture(28, 14, 7, 4);
					moveFurniture(29, 14, 8, 4);
					moveFurniture(27, 14, 4, 4);
					moveFurniture(28, 15, 5, 4);
					moveFurniture(29, 16, 6, 4);
				}
				if (upgradeLevel == 0)
				{
					shiftObjects(9, 9);
				}
				break;
			}
		}

		protected override LocalizedContentManager getMapLoader()
		{
			if (mapLoader == null)
			{
				mapLoader = Game1.game1.xTileContent.CreateTemporary();
			}
			return mapLoader;
		}

		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			base.drawAboveFrontLayer(b);
			if (fridge.Value.mutex.IsLocked())
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(fridgePosition.X, fridgePosition.Y - 1) * 64f), new Microsoft.Xna.Framework.Rectangle(0, 192, 16, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((fridgePosition.Y + 1) * 64 + 1) / 10000f);
			}
		}

		public override void updateMap()
		{
			bool flag = owner.spouse != null && owner.isMarried();
			mapPath.Value = "Maps\\FarmHouse" + ((upgradeLevel == 0) ? "" : ((upgradeLevel == 3) ? "2" : (upgradeLevel.ToString() ?? ""))) + (flag ? "_marriage" : "");
			base.updateMap();
		}

		public virtual void setMapForUpgradeLevel(int level)
		{
			upgradeLevel = level;
			int value = synchronizedDisplayedLevel.Value;
			currentlyDisplayedUpgradeLevel = level;
			synchronizedDisplayedLevel.Value = level;
			bool flag = owner.isMarried() && owner.spouse != null;
			if (displayingSpouseRoom && !flag)
			{
				displayingSpouseRoom = false;
			}
			updateMap();
			RefreshFloorObjectNeighbors();
			if (flag)
			{
				showSpouseRoom();
			}
			loadObjects();
			if (level == 3)
			{
				AddCellarTiles();
				createCellarWarps();
				if (!Game1.player.craftingRecipes.ContainsKey("Cask"))
				{
					Game1.player.craftingRecipes.Add("Cask", 0);
				}
			}
			bool flag2 = false;
			if (previousUpgradeLevel == 0 && upgradeLevel >= 0)
			{
				flag2 = true;
			}
			if (previousUpgradeLevel >= 0)
			{
				if (previousUpgradeLevel < 2 && upgradeLevel >= 2)
				{
					for (int i = 0; i < map.Layers[0].TileWidth; i++)
					{
						for (int j = 0; j < map.Layers[0].TileHeight; j++)
						{
							if (doesTileHaveProperty(i, j, "DefaultChildBedPosition", "Back") != null)
							{
								int cHILD_BED_INDEX = BedFurniture.CHILD_BED_INDEX;
								base.furniture.Add(new BedFurniture(cHILD_BED_INDEX, new Vector2(i, j)));
								break;
							}
						}
					}
				}
				Furniture furniture = null;
				if (previousUpgradeLevel == 0)
				{
					foreach (Furniture item in base.furniture)
					{
						if (item is BedFurniture && (item as BedFurniture).bedType == BedFurniture.BedType.Single)
						{
							furniture = item;
							break;
						}
					}
				}
				else
				{
					foreach (Furniture item2 in base.furniture)
					{
						if (item2 is BedFurniture && (item2 as BedFurniture).bedType == BedFurniture.BedType.Double)
						{
							furniture = item2;
							break;
						}
					}
				}
				if (upgradeLevel != 3 || flag2)
				{
					for (int k = 0; k < map.Layers[0].TileWidth; k++)
					{
						for (int l = 0; l < map.Layers[0].TileHeight; l++)
						{
							if (doesTileHaveProperty(k, l, "DefaultBedPosition", "Back") == null)
							{
								continue;
							}
							int bed_index = BedFurniture.DEFAULT_BED_INDEX;
							if (previousUpgradeLevel != 1 || furniture == null || (furniture.tileLocation.X == 24f && furniture.tileLocation.Y == 12f))
							{
								if (furniture != null)
								{
									bed_index = furniture.ParentSheetIndex;
								}
								if (previousUpgradeLevel == 0 && furniture != null)
								{
									furniture.performRemoveAction(furniture.tileLocation, this);
									Guid guid = base.furniture.GuidOf(furniture);
									base.furniture.Remove(guid);
									bed_index = Utility.GetDoubleWideVersionOfBed(bed_index);
									base.furniture.Add(new BedFurniture(bed_index, new Vector2(k, l)));
								}
								else if (furniture != null)
								{
									furniture.performRemoveAction(furniture.tileLocation, this);
									Guid guid2 = base.furniture.GuidOf(furniture);
									base.furniture.Remove(guid2);
									base.furniture.Add(new BedFurniture(furniture.ParentSheetIndex, new Vector2(k, l)));
								}
							}
							break;
						}
					}
				}
				previousUpgradeLevel = -1;
			}
			if (value != level)
			{
				lightGlows.Clear();
			}
			fridgePosition = default(Point);
			bool flag3 = false;
			for (int m = 0; m < map.GetLayer("Buildings").LayerWidth; m++)
			{
				for (int n = 0; n < map.GetLayer("Buildings").LayerHeight; n++)
				{
					if (map.GetLayer("Buildings").Tiles[m, n] != null)
					{
						int tileIndex = map.GetLayer("Buildings").Tiles[m, n].TileIndex;
						if (tileIndex == 173)
						{
							fridgePosition = new Point(m, n);
							flag3 = true;
							break;
						}
					}
				}
				if (flag3)
				{
					break;
				}
			}
			base.tapToMove.Init(this);
		}

		public void createCellarWarps()
		{
			updateCellarWarps();
		}

		public void updateCellarWarps()
		{
			Layer layer = map.GetLayer("Back");
			for (int i = 0; i < layer.LayerWidth; i++)
			{
				for (int j = 0; j < layer.LayerHeight; j++)
				{
					string text = doesTileHaveProperty(i, j, "TouchAction", "Back");
					if (text != null && text.StartsWith("Warp "))
					{
						string[] array = text.Split(' ');
						if (array.Length >= 2 && array[1].StartsWith("Cellar"))
						{
							array[1] = GetCellarName();
							setTileProperty(i, j, "Back", "TouchAction", string.Join(" ", array));
						}
					}
				}
			}
			if (cellarWarps == null)
			{
				return;
			}
			foreach (Warp cellarWarp in cellarWarps)
			{
				if (!warps.Contains(cellarWarp))
				{
					warps.Add(cellarWarp);
				}
				cellarWarp.TargetName = GetCellarName();
			}
		}

		public virtual int GetSpouseRoomWidth()
		{
			return 6;
		}

		public virtual int GetSpouseRoomHeight()
		{
			return 9;
		}

		public virtual Point GetSpouseRoomCorner()
		{
			if (upgradeLevel == 1)
			{
				return new Point(29, 1);
			}
			return new Point(35, 10);
		}

		public virtual void loadSpouseRoom()
		{
			NPC spouse = owner.getSpouse();
			spouseRoomSpot = GetSpouseRoomCorner();
			spouseRoomSpot.X += 3;
			spouseRoomSpot.Y += 4;
			if (spouse == null)
			{
				return;
			}
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\SpouseRooms");
			int num = -1;
			string text = "spouseRooms";
			if (dictionary != null && dictionary.ContainsKey(spouse.Name))
			{
				try
				{
					string text2 = dictionary[spouse.Name];
					string[] array = text2.Split('/');
					text = array[0];
					num = int.Parse(array[1]);
				}
				catch (Exception)
				{
				}
			}
			if (num == -1)
			{
				string text3 = spouse.Name;
				if (text3 != null)
				{
					switch (text3.Length)
					{
					case 7:
						switch (text3[0])
						{
						case 'A':
							if (text3 == "Abigail")
							{
								num = 0;
							}
							break;
						case 'E':
							if (text3 == "Elliott")
							{
								num = 8;
							}
							break;
						}
						break;
					case 5:
						switch (text3[0])
						{
						case 'P':
							if (text3 == "Penny")
							{
								num = 1;
							}
							break;
						case 'H':
							if (text3 == "Haley")
							{
								num = 3;
							}
							break;
						case 'S':
							if (text3 == "Shane")
							{
								num = 10;
							}
							break;
						case 'E':
							if (text3 == "Emily")
							{
								num = 11;
							}
							break;
						}
						break;
					case 4:
						switch (text3[0])
						{
						case 'L':
							if (text3 == "Leah")
							{
								num = 2;
							}
							break;
						case 'M':
							if (text3 == "Maru")
							{
								num = 4;
							}
							break;
						case 'A':
							if (text3 == "Alex")
							{
								num = 6;
							}
							break;
						}
						break;
					case 6:
						switch (text3[0])
						{
						case 'H':
							if (text3 == "Harvey")
							{
								num = 7;
							}
							break;
						case 'K':
							if (text3 == "Krobus")
							{
								num = 12;
							}
							break;
						}
						break;
					case 9:
						if (text3 == "Sebastian")
						{
							num = 5;
						}
						break;
					case 3:
						if (text3 == "Sam")
						{
							num = 9;
						}
						break;
					}
				}
			}
			int spouseRoomWidth = GetSpouseRoomWidth();
			int spouseRoomHeight = GetSpouseRoomHeight();
			Point spouseRoomCorner = GetSpouseRoomCorner();
			Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle(spouseRoomCorner.X, spouseRoomCorner.Y, spouseRoomWidth, spouseRoomHeight);
			Map map = Game1.game1.xTileContent.Load<Map>("Maps\\" + text);
			int num2 = map.Layers[0].LayerWidth / spouseRoomWidth;
			int num3 = map.Layers[0].LayerHeight / spouseRoomHeight;
			Point point = new Point(num % num2 * spouseRoomWidth, num / num2 * spouseRoomHeight);
			base.map.Properties.Remove("DayTiles");
			base.map.Properties.Remove("NightTiles");
			List<KeyValuePair<Point, Tile>> list = new List<KeyValuePair<Point, Tile>>();
			Layer layer = base.map.GetLayer("Front");
			for (int i = value.Left; i < value.Right; i++)
			{
				Point key = new Point(i, value.Bottom - 1);
				Tile tile = layer.Tiles[key.X, key.Y];
				if (tile != null)
				{
					list.Add(new KeyValuePair<Point, Tile>(key, tile));
				}
			}
			if (_appliedMapOverrides.Contains("spouse_room"))
			{
				_appliedMapOverrides.Remove("spouse_room");
			}
			ApplyMapOverride(text, "spouse_room", new Microsoft.Xna.Framework.Rectangle(point.X, point.Y, value.Width, value.Height), value);
			for (int j = 0; j < value.Width; j++)
			{
				for (int k = 0; k < value.Height; k++)
				{
					if (map.GetLayer("Buildings").Tiles[point.X + j, point.Y + k] != null)
					{
						adjustMapLightPropertiesForLamp(map.GetLayer("Buildings").Tiles[point.X + j, point.Y + k].TileIndex, value.X + j, value.Y + k, "Buildings");
					}
					if (k < value.Height - 1 && map.GetLayer("Front").Tiles[point.X + j, point.Y + k] != null)
					{
						adjustMapLightPropertiesForLamp(map.GetLayer("Front").Tiles[point.X + j, point.Y + k].TileIndex, value.X + j, value.Y + k, "Front");
					}
				}
			}
			bool flag = false;
			for (int l = value.Left; l < value.Right; l++)
			{
				for (int m = value.Top; m < value.Bottom; m++)
				{
					if (getTileIndexAt(new Point(l, m), "Paths") == 7)
					{
						flag = true;
						spouseRoomSpot = new Point(l, m);
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			Point point2 = GetSpouseRoomSpot();
			setTileProperty(point2.X, point2.Y, "Back", "NoFurniture", "T");
			foreach (KeyValuePair<Point, Tile> item in list)
			{
				layer.Tiles[item.Key.X, item.Key.Y] = item.Value;
			}
		}

		public virtual Microsoft.Xna.Framework.Rectangle? GetCribBounds()
		{
			if (upgradeLevel < 2)
			{
				return null;
			}
			return new Microsoft.Xna.Framework.Rectangle(15, 2, 3, 4);
		}

		public virtual Microsoft.Xna.Framework.Rectangle? GetBedBounds(int child_index = 0)
		{
			if (upgradeLevel < 2)
			{
				return null;
			}
			return child_index switch
			{
				0 => new Microsoft.Xna.Framework.Rectangle(22, 3, 2, 4), 
				1 => new Microsoft.Xna.Framework.Rectangle(26, 3, 2, 4), 
				_ => null, 
			};
		}

		public virtual Microsoft.Xna.Framework.Rectangle? GetChildBedBounds(int child_index = 0)
		{
			if (upgradeLevel < 2)
			{
				return null;
			}
			return child_index switch
			{
				0 => new Microsoft.Xna.Framework.Rectangle(22, 3, 2, 4), 
				1 => new Microsoft.Xna.Framework.Rectangle(26, 3, 2, 4), 
				_ => null, 
			};
		}

		public virtual void UpdateChildRoom()
		{
			Microsoft.Xna.Framework.Rectangle? cribBounds = GetCribBounds();
			if (cribBounds.HasValue)
			{
				if (_appliedMapOverrides.Contains("crib"))
				{
					_appliedMapOverrides.Remove("crib");
				}
				ApplyMapOverride("FarmHouse_Crib_" + cribStyle.Value, "crib", null, cribBounds);
			}
		}

		public void playerDivorced()
		{
			displayingSpouseRoom = false;
		}

		public virtual List<Microsoft.Xna.Framework.Rectangle> getForbiddenPetWarpTiles()
		{
			List<Microsoft.Xna.Framework.Rectangle> list = new List<Microsoft.Xna.Framework.Rectangle>();
			switch (upgradeLevel)
			{
			case 0:
				list.Add(new Microsoft.Xna.Framework.Rectangle(2, 8, 3, 4));
				break;
			case 1:
				list.Add(new Microsoft.Xna.Framework.Rectangle(8, 8, 3, 4));
				list.Add(new Microsoft.Xna.Framework.Rectangle(17, 8, 4, 3));
				break;
			case 2:
			case 3:
				list.Add(new Microsoft.Xna.Framework.Rectangle(11, 17, 3, 4));
				list.Add(new Microsoft.Xna.Framework.Rectangle(20, 17, 4, 3));
				list.Add(new Microsoft.Xna.Framework.Rectangle(12, 5, 4, 3));
				list.Add(new Microsoft.Xna.Framework.Rectangle(11, 7, 2, 6));
				break;
			}
			return list;
		}

		public bool canPetWarpHere(Vector2 tile_position)
		{
			List<Microsoft.Xna.Framework.Rectangle> forbiddenPetWarpTiles = getForbiddenPetWarpTiles();
			foreach (Microsoft.Xna.Framework.Rectangle item in forbiddenPetWarpTiles)
			{
				if (item.Contains((int)tile_position.X, (int)tile_position.Y))
				{
					return false;
				}
			}
			return true;
		}

		public override List<Microsoft.Xna.Framework.Rectangle> getWalls()
		{
			List<Microsoft.Xna.Framework.Rectangle> list = new List<Microsoft.Xna.Framework.Rectangle>();
			switch (upgradeLevel)
			{
			case 0:
				list.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 10, 3));
				break;
			case 1:
				list.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 17, 3));
				list.Add(new Microsoft.Xna.Framework.Rectangle(18, 6, 2, 2));
				list.Add(new Microsoft.Xna.Framework.Rectangle(20, 1, 9, 3));
				break;
			case 2:
			case 3:
			{
				list.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 12, 3));
				list.Add(new Microsoft.Xna.Framework.Rectangle(15, 1, 13, 3));
				list.Add(new Microsoft.Xna.Framework.Rectangle(13, 3, 2, 2));
				list.Add(new Microsoft.Xna.Framework.Rectangle(1, 10, 10, 3));
				list.Add(new Microsoft.Xna.Framework.Rectangle(13, 10, 8, 3));
				int num = (owner.hasOrWillReceiveMail("renovation_corner_open") ? (-3) : 0);
				if (owner.hasOrWillReceiveMail("renovation_bedroom_open"))
				{
					list.Add(new Microsoft.Xna.Framework.Rectangle(21, 15, 0, 2));
					list.Add(new Microsoft.Xna.Framework.Rectangle(21, 10, 13 + num, 3));
				}
				else
				{
					list.Add(new Microsoft.Xna.Framework.Rectangle(21, 15, 2, 2));
					list.Add(new Microsoft.Xna.Framework.Rectangle(23, 10, 11 + num, 3));
				}
				if (owner.hasOrWillReceiveMail("renovation_southern_open"))
				{
					list.Add(new Microsoft.Xna.Framework.Rectangle(23, 24, 3, 3));
					list.Add(new Microsoft.Xna.Framework.Rectangle(31, 24, 3, 3));
				}
				else
				{
					list.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
					list.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				}
				if (owner.hasOrWillReceiveMail("renovation_corner_open"))
				{
					list.Add(new Microsoft.Xna.Framework.Rectangle(30, 1, 9, 3));
					list.Add(new Microsoft.Xna.Framework.Rectangle(28, 3, 2, 2));
				}
				else
				{
					list.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
					list.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				}
				break;
			}
			}
			return list;
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			if (l is FarmHouse)
			{
				FarmHouse farmHouse = l as FarmHouse;
				cribStyle.Value = farmHouse.cribStyle.Value;
			}
			base.TransferDataFromSavedLocation(l);
		}

		public override List<Microsoft.Xna.Framework.Rectangle> getFloors()
		{
			List<Microsoft.Xna.Framework.Rectangle> list = new List<Microsoft.Xna.Framework.Rectangle>();
			switch (upgradeLevel)
			{
			case 0:
				list.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 10, 9));
				break;
			case 1:
				list.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 6, 9));
				list.Add(new Microsoft.Xna.Framework.Rectangle(7, 3, 11, 9));
				list.Add(new Microsoft.Xna.Framework.Rectangle(18, 8, 2, 2));
				list.Add(new Microsoft.Xna.Framework.Rectangle(20, 3, 9, 8));
				break;
			case 2:
			case 3:
				list.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 12, 6));
				list.Add(new Microsoft.Xna.Framework.Rectangle(15, 3, 13, 6));
				list.Add(new Microsoft.Xna.Framework.Rectangle(13, 5, 2, 2));
				list.Add(new Microsoft.Xna.Framework.Rectangle(0, 12, 10, 11));
				list.Add(new Microsoft.Xna.Framework.Rectangle(10, 12, 11, 9));
				if (owner.mailReceived.Contains("renovation_bedroom_open"))
				{
					list.Add(new Microsoft.Xna.Framework.Rectangle(21, 17, 0, 2));
					list.Add(new Microsoft.Xna.Framework.Rectangle(21, 12, 14, 11));
				}
				else
				{
					list.Add(new Microsoft.Xna.Framework.Rectangle(21, 17, 2, 2));
					list.Add(new Microsoft.Xna.Framework.Rectangle(23, 12, 12, 11));
				}
				if (owner.hasOrWillReceiveMail("renovation_southern_open"))
				{
					list.Add(new Microsoft.Xna.Framework.Rectangle(23, 26, 11, 8));
				}
				else
				{
					list.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				}
				if (owner.hasOrWillReceiveMail("renovation_corner_open"))
				{
					list.Add(new Microsoft.Xna.Framework.Rectangle(28, 5, 2, 3));
					list.Add(new Microsoft.Xna.Framework.Rectangle(30, 3, 9, 6));
				}
				else
				{
					list.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
					list.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				}
				break;
			}
			return list;
		}

		public virtual bool CanModifyCrib()
		{
			if (owner == null)
			{
				return false;
			}
			if (owner.isMarried())
			{
				Friendship spouseFriendship = owner.GetSpouseFriendship();
				if (spouseFriendship.DaysUntilBirthing != -1)
				{
					return false;
				}
			}
			foreach (Child child in owner.getChildren())
			{
				if (child.Age < 3)
				{
					return false;
				}
			}
			return true;
		}
	}
}
