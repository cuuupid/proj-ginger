using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Mobile;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley
{
	[XmlInclude(typeof(Fence))]
	[XmlInclude(typeof(Torch))]
	[XmlInclude(typeof(SpecialItem))]
	[XmlInclude(typeof(Wallpaper))]
	[XmlInclude(typeof(Boots))]
	[XmlInclude(typeof(Hat))]
	[XmlInclude(typeof(ItemPedestal))]
	[XmlInclude(typeof(Clothing))]
	[XmlInclude(typeof(CombinedRing))]
	[XmlInclude(typeof(Ring))]
	[XmlInclude(typeof(TV))]
	[XmlInclude(typeof(CrabPot))]
	[XmlInclude(typeof(Chest))]
	[XmlInclude(typeof(Workbench))]
	[XmlInclude(typeof(MiniJukebox))]
	[XmlInclude(typeof(Phone))]
	[XmlInclude(typeof(StorageFurniture))]
	[XmlInclude(typeof(FishTankFurniture))]
	[XmlInclude(typeof(BedFurniture))]
	[XmlInclude(typeof(WoodChipper))]
	[XmlInclude(typeof(Cask))]
	[XmlInclude(typeof(SwitchFloor))]
	[XmlInclude(typeof(ColoredObject))]
	[XmlInclude(typeof(IndoorPot))]
	[XmlInclude(typeof(Sign))]
	public class Object : Item
	{
		public enum PreserveType
		{
			Wine,
			Jelly,
			Pickle,
			Juice,
			Roe,
			AgedRoe
		}

		public enum HoneyType
		{
			Wild = -1,
			Poppy = 376,
			Tulip = 591,
			SummerSpangle = 593,
			FairyRose = 595,
			BlueJazz = 597
		}

		public const int copperBar = 334;

		public const int ironBar = 335;

		public const int goldBar = 336;

		public const int iridiumBar = 337;

		public const int wood = 388;

		public const int stone = 390;

		public const int copper = 378;

		public const int iron = 380;

		public const int coal = 382;

		public const int gold = 384;

		public const int iridium = 386;

		public const int inedible = -300;

		public const int GreensCategory = -81;

		public const int GemCategory = -2;

		public const int VegetableCategory = -75;

		public const int FishCategory = -4;

		public const int EggCategory = -5;

		public const int MilkCategory = -6;

		public const int CookingCategory = -7;

		public const int CraftingCategory = -8;

		public const int BigCraftableCategory = -9;

		public const int FruitsCategory = -79;

		public const int SeedsCategory = -74;

		public const int mineralsCategory = -12;

		public const int flowersCategory = -80;

		public const int meatCategory = -14;

		public const int metalResources = -15;

		public const int buildingResources = -16;

		public const int sellAtPierres = -17;

		public const int sellAtPierresAndMarnies = -18;

		public const int fertilizerCategory = -19;

		public const int junkCategory = -20;

		public const int baitCategory = -21;

		public const int tackleCategory = -22;

		public const int sellAtFishShopCategory = -23;

		public const int furnitureCategory = -24;

		public const int ingredientsCategory = -25;

		public const int artisanGoodsCategory = -26;

		public const int syrupCategory = -27;

		public const int monsterLootCategory = -28;

		public const int equipmentCategory = -29;

		public const int clothingCategorySortValue = -94;

		public const int hatCategory = -95;

		public const int ringCategory = -96;

		public const int weaponCategory = -98;

		public const int bootsCategory = -97;

		public const int toolCategory = -99;

		public const int clothingCategory = -100;

		public const int objectInfoNameIndex = 0;

		public const int objectInfoPriceIndex = 1;

		public const int objectInfoEdibilityIndex = 2;

		public const int objectInfoTypeIndex = 3;

		public const int objectInfoDisplayNameIndex = 4;

		public const int objectInfoDescriptionIndex = 5;

		public const int objectInfoMiscIndex = 6;

		public const int objectInfoBuffTypesIndex = 7;

		public const int objectInfoBuffDurationIndex = 8;

		public const int WeedsIndex = 0;

		public const int StoneIndex = 2;

		public const int StickIndex = 4;

		public const int DryDirtTileIndex = 6;

		public const int WateredTileIndex = 7;

		public const int StumpTopLeftIndex = 8;

		public const int BoulderTopLeftIndex = 10;

		public const int StumpBottomLeftIndex = 12;

		public const int BoulderBottomLeftIndex = 14;

		public const int WildHorseradishIndex = 16;

		public const int TulipIndex = 18;

		public const int LeekIndex = 20;

		public const int DandelionIndex = 22;

		public const int ParsnipIndex = 24;

		public const int HandCursorIndex = 26;

		public const int WaterAnimationIndex = 28;

		public const int LumberIndex = 30;

		public const int mineStoneGrey1Index = 32;

		public const int mineStoneBlue1Index = 34;

		public const int mineStoneBlue2Index = 36;

		public const int mineStoneGrey2Index = 38;

		public const int mineStoneBrown1Index = 40;

		public const int mineStoneBrown2Index = 42;

		public const int mineStonePurpleIndex = 44;

		public const int mineStoneMysticIndex = 46;

		public const int mineStoneSnow1 = 48;

		public const int mineStoneSnow2 = 50;

		public const int mineStoneSnow3 = 52;

		public const int mineStonePurpleSnowIndex = 54;

		public const int mineStoneRed1Index = 56;

		public const int mineStoneRed2Index = 58;

		public const int emeraldIndex = 60;

		public const int aquamarineIndex = 62;

		public const int rubyIndex = 64;

		public const int amethystClusterIndex = 66;

		public const int topazIndex = 68;

		public const int sapphireIndex = 70;

		public const int diamondIndex = 72;

		public const int prismaticShardIndex = 74;

		public const int snowHoedDirtIndex = 76;

		public const int beachHoedDirtIndex = 77;

		public const int caveCarrotIndex = 78;

		public const int quartzIndex = 80;

		public const int bobberIndex = 133;

		public const int stardrop = 434;

		public const int spriteSheetTileSize = 16;

		public const int lowQuality = 0;

		public const int medQuality = 1;

		public const int highQuality = 2;

		public const int bestQuality = 4;

		public const int copperPerBar = 10;

		public const int ironPerBar = 10;

		public const int goldPerBar = 10;

		public const int iridiumPerBar = 10;

		public const float wobbleAmountWhenWorking = 10f;

		public const int fragility_Removable = 0;

		public const int fragility_Delicate = 1;

		public const int fragility_Indestructable = 2;

		[XmlElement("tileLocation")]
		public readonly NetVector2 tileLocation = new NetVector2();

		[XmlElement("owner")]
		public readonly NetLong owner = new NetLong();

		[XmlElement("type")]
		public readonly NetString type = new NetString();

		[XmlElement("canBeSetDown")]
		public readonly NetBool canBeSetDown = new NetBool(value: false);

		[XmlElement("canBeGrabbed")]
		public readonly NetBool canBeGrabbed = new NetBool(value: true);

		[XmlElement("isHoedirt")]
		public readonly NetBool isHoedirt = new NetBool(value: false);

		[XmlElement("isSpawnedObject")]
		public readonly NetBool isSpawnedObject = new NetBool(value: false);

		[XmlElement("questItem")]
		public readonly NetBool questItem = new NetBool(value: false);

		[XmlElement("questId")]
		public readonly NetInt questId = new NetInt(0);

		[XmlElement("isOn")]
		public readonly NetBool isOn = new NetBool(value: true);

		[XmlElement("fragility")]
		public readonly NetInt fragility = new NetInt(0);

		private bool isActive;

		[XmlElement("price")]
		public readonly NetInt price = new NetInt();

		[XmlElement("edibility")]
		public readonly NetInt edibility = new NetInt(-300);

		[XmlElement("stack")]
		public readonly NetInt stack = new NetInt(1);

		[XmlElement("quality")]
		public readonly NetInt quality = new NetInt(0);

		[XmlElement("bigCraftable")]
		public readonly NetBool bigCraftable = new NetBool();

		[XmlElement("setOutdoors")]
		public readonly NetBool setOutdoors = new NetBool();

		[XmlElement("setIndoors")]
		public readonly NetBool setIndoors = new NetBool();

		[XmlElement("readyForHarvest")]
		public readonly NetBool readyForHarvest = new NetBool();

		[XmlElement("showNextIndex")]
		public readonly NetBool showNextIndex = new NetBool();

		[XmlElement("flipped")]
		public readonly NetBool flipped = new NetBool();

		[XmlElement("hasBeenPickedUpByFarmer")]
		public readonly NetBool hasBeenPickedUpByFarmer = new NetBool();

		[XmlElement("isRecipe")]
		public readonly NetBool isRecipe = new NetBool();

		[XmlElement("isLamp")]
		public readonly NetBool isLamp = new NetBool();

		[XmlElement("heldObject")]
		public readonly NetRef<Object> heldObject = new NetRef<Object>();

		[XmlElement("minutesUntilReady")]
		public readonly NetIntDelta minutesUntilReady = new NetIntDelta();

		[XmlElement("boundingBox")]
		public readonly NetRectangle boundingBox = new NetRectangle();

		public Vector2 scale;

		[XmlElement("uses")]
		public readonly NetInt uses = new NetInt();

		[XmlIgnore]
		private readonly NetRef<LightSource> netLightSource = new NetRef<LightSource>();

		[XmlIgnore]
		public bool isTemporarilyInvisible;

		[XmlIgnore]
		protected NetBool _destroyOvernight = new NetBool(value: false);

		[XmlElement("orderData")]
		public readonly NetString orderData = new NetString();

		[XmlIgnore]
		public static Chest autoLoadChest;

		[XmlIgnore]
		public int shakeTimer;

		[XmlIgnore]
		public int lastNoteBlockSoundTime;

		[XmlIgnore]
		public ICue internalSound;

		[XmlElement("preserve")]
		public readonly NetNullableEnum<PreserveType> preserve = new NetNullableEnum<PreserveType>();

		[XmlElement("preservedParentSheetIndex")]
		public readonly NetInt preservedParentSheetIndex = new NetInt();

		[XmlElement("honeyType")]
		public readonly NetNullableEnum<HoneyType> honeyType = new NetNullableEnum<HoneyType>();

		[XmlIgnore]
		public string displayName;

		protected int health = 10;

		private Dictionary<Vector2, bool> _redGreenSquareDict = new Dictionary<Vector2, bool>();

		private int _lastQuantity;

		public bool destroyOvernight
		{
			get
			{
				return _destroyOvernight.Value;
			}
			set
			{
				_destroyOvernight.Value = value;
			}
		}

		[XmlIgnore]
		public LightSource lightSource
		{
			get
			{
				return netLightSource;
			}
			set
			{
				netLightSource.Value = value;
			}
		}

		[XmlIgnore]
		public Vector2 TileLocation
		{
			get
			{
				return tileLocation;
			}
			set
			{
				tileLocation.Value = value;
			}
		}

		[XmlIgnore]
		public string name
		{
			get
			{
				return netName.Value;
			}
			set
			{
				netName.Value = value;
			}
		}

		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				if (Game1.objectInformation != null)
				{
					displayName = loadDisplayName();
					if (orderData.Value != null && orderData.Value == "QI_COOKING")
					{
						displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Fresh_Prefix", displayName);
					}
				}
				return displayName + (isRecipe ? (((CraftingRecipe.craftingRecipes.ContainsKey(displayName) && CraftingRecipe.craftingRecipes[displayName].Split('/')[2].Split(' ').Count() > 1) ? (" x" + CraftingRecipe.craftingRecipes[displayName].Split('/')[2].Split(' ')[1]) : "") + Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12657")) : "");
			}
			set
			{
				displayName = value;
			}
		}

		[XmlIgnore]
		public override string Name
		{
			get
			{
				return name + (isRecipe ? " Recipe" : "");
			}
			set
			{
				name = value;
			}
		}

		[XmlIgnore]
		public string Type
		{
			get
			{
				return type;
			}
			set
			{
				type.Value = value;
			}
		}

		[XmlIgnore]
		public override int Stack
		{
			get
			{
				return Math.Max(0, stack);
			}
			set
			{
				stack.Value = Math.Min(Math.Max(0, value), (value == 2147483647) ? value : maximumStackSize());
			}
		}

		[XmlIgnore]
		public int Quality
		{
			get
			{
				return quality;
			}
			set
			{
				quality.Value = value;
			}
		}

		[XmlIgnore]
		public bool CanBeSetDown
		{
			get
			{
				return canBeSetDown;
			}
			set
			{
				canBeSetDown.Value = value;
			}
		}

		[XmlIgnore]
		public bool CanBeGrabbed
		{
			get
			{
				return canBeGrabbed;
			}
			set
			{
				canBeGrabbed.Value = value;
			}
		}

		[XmlIgnore]
		public bool HasBeenPickedUpByFarmer
		{
			get
			{
				return hasBeenPickedUpByFarmer;
			}
			set
			{
				hasBeenPickedUpByFarmer.Value = value;
			}
		}

		[XmlIgnore]
		public bool IsHoeDirt => isHoedirt;

		[XmlIgnore]
		public bool IsOn
		{
			get
			{
				return isOn;
			}
			set
			{
				isOn.Value = value;
			}
		}

		[XmlIgnore]
		public bool IsSpawnedObject
		{
			get
			{
				return isSpawnedObject;
			}
			set
			{
				isSpawnedObject.Value = value;
			}
		}

		[XmlIgnore]
		public bool IsRecipe
		{
			get
			{
				return isRecipe;
			}
			set
			{
				isRecipe.Value = value;
			}
		}

		[XmlIgnore]
		public bool Flipped
		{
			get
			{
				return flipped;
			}
			set
			{
				flipped.Value = value;
			}
		}

		[XmlIgnore]
		public int Price
		{
			get
			{
				return price;
			}
			set
			{
				price.Value = value;
			}
		}

		[XmlIgnore]
		public int Edibility
		{
			get
			{
				return edibility;
			}
			set
			{
				edibility.Value = value;
			}
		}

		[XmlIgnore]
		public int Fragility
		{
			get
			{
				return fragility;
			}
			set
			{
				fragility.Value = value;
			}
		}

		[XmlIgnore]
		public Vector2 Scale
		{
			get
			{
				return scale;
			}
			set
			{
				scale = value;
			}
		}

		[XmlIgnore]
		public int MinutesUntilReady
		{
			get
			{
				return minutesUntilReady;
			}
			set
			{
				minutesUntilReady.Value = value;
			}
		}

		protected virtual void initNetFields()
		{
			base.NetFields.AddFields(tileLocation, owner, type, canBeSetDown, canBeGrabbed, isHoedirt, isSpawnedObject, questItem, questId, isOn, fragility, price, edibility, stack, quality, uses, bigCraftable, setOutdoors, setIndoors, readyForHarvest, showNextIndex, flipped, hasBeenPickedUpByFarmer, isRecipe, isLamp, heldObject, minutesUntilReady, boundingBox, preserve, preservedParentSheetIndex, honeyType, netLightSource, orderData, _destroyOvernight);
		}

		public Object()
		{
			initNetFields();
		}

		public Object(Vector2 tileLocation, int parentSheetIndex, bool isRecipe = false)
			: this()
		{
			this.isRecipe.Value = isRecipe;
			this.tileLocation.Value = tileLocation;
			base.ParentSheetIndex = parentSheetIndex;
			canBeSetDown.Value = true;
			bigCraftable.Value = true;
			Game1.bigCraftablesInformation.TryGetValue(parentSheetIndex, out var value);
			if (value != null)
			{
				string[] array = value.Split('/');
				name = array[0];
				price.Value = Convert.ToInt32(array[1]);
				edibility.Value = Convert.ToInt32(array[2]);
				string[] array2 = array[3].Split(' ');
				type.Value = array2[0];
				if (array2.Length > 1)
				{
					base.Category = Convert.ToInt32(array2[1]);
				}
				setOutdoors.Value = Convert.ToBoolean(array[5]);
				setIndoors.Value = Convert.ToBoolean(array[6]);
				fragility.Value = Convert.ToInt32(array[7]);
				isLamp.Value = array.Length > 8 && array[8].Equals("true");
			}
			initializeLightSource(this.tileLocation);
			boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		public Object(int parentSheetIndex, int initialStack, bool isRecipe = false, int price = -1, int quality = 0)
			: this(Vector2.Zero, parentSheetIndex, initialStack)
		{
			this.isRecipe.Value = isRecipe;
			if (price != -1)
			{
				this.price.Value = price;
			}
			this.quality.Value = quality;
		}

		public Object(Vector2 tileLocation, int parentSheetIndex, int initialStack)
			: this(tileLocation, parentSheetIndex, null, canBeSetDown: true, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
		{
			stack.Value = initialStack;
		}

		public Object(Vector2 tileLocation, int parentSheetIndex, string Givenname, bool canBeSetDown, bool canBeGrabbed, bool isHoedirt, bool isSpawnedObject)
			: this()
		{
			this.tileLocation.Value = tileLocation;
			base.ParentSheetIndex = parentSheetIndex;
			Game1.objectInformation.TryGetValue(parentSheetIndex, out var value);
			try
			{
				if (value != null)
				{
					string[] array = value.Split('/');
					name = array[0];
					price.Value = Convert.ToInt32(array[1]);
					edibility.Value = Convert.ToInt32(array[2]);
					string[] array2 = array[3].Split(' ');
					type.Value = array2[0];
					if (array2.Length > 1)
					{
						base.Category = Convert.ToInt32(array2[1]);
					}
				}
			}
			catch (Exception)
			{
			}
			if (name == null && Givenname != null)
			{
				name = Givenname;
			}
			else if (name == null)
			{
				name = "Error Item";
			}
			this.canBeSetDown.Value = canBeSetDown;
			this.canBeGrabbed.Value = canBeGrabbed;
			this.isHoedirt.Value = isHoedirt;
			this.isSpawnedObject.Value = isSpawnedObject;
			if (Game1.random.NextDouble() < 0.5 && parentSheetIndex > 52 && (parentSheetIndex < 8 || parentSheetIndex > 15) && (parentSheetIndex < 384 || parentSheetIndex > 391))
			{
				flipped.Value = true;
			}
			if (name.Contains("Block"))
			{
				scale = new Vector2(1f, 1f);
			}
			if (parentSheetIndex == 449 || name.Contains("Weed") || name.Contains("Twig"))
			{
				fragility.Value = 2;
			}
			else if (name.Contains("Fence"))
			{
				scale = new Vector2(10f, 0f);
				canBeSetDown = false;
			}
			else if (name.Contains("Stone"))
			{
				switch (parentSheetIndex)
				{
				case 8:
					minutesUntilReady.Value = 4;
					break;
				case 10:
					minutesUntilReady.Value = 8;
					break;
				case 12:
					minutesUntilReady.Value = 16;
					break;
				case 14:
					minutesUntilReady.Value = 12;
					break;
				case 25:
					minutesUntilReady.Value = 8;
					break;
				default:
					minutesUntilReady.Value = 1;
					break;
				}
			}
			if (parentSheetIndex >= 75 && parentSheetIndex <= 77)
			{
				isSpawnedObject = false;
			}
			initializeLightSource(this.tileLocation);
			if (base.Category == -22)
			{
				scale.Y = 1f;
			}
			boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		protected override void _PopulateContextTags(HashSet<string> tags)
		{
			base._PopulateContextTags(tags);
			if (quality.Value == 0)
			{
				tags.Add("quality_none");
			}
			else if (quality.Value == 1)
			{
				tags.Add("quality_silver");
			}
			else if (quality.Value == 2)
			{
				tags.Add("quality_gold");
			}
			else if (quality.Value == 4)
			{
				tags.Add("quality_iridium");
			}
			if (orderData.Value == "QI_COOKING")
			{
				tags.Add("quality_qi");
			}
			if (preserve != null && preserve.Value.HasValue)
			{
				if (preserve.Value == PreserveType.Jelly)
				{
					tags.Add("jelly_item");
				}
				else if (preserve.Value == PreserveType.Juice)
				{
					tags.Add("juice_item");
				}
				else if (preserve.Value == PreserveType.Wine)
				{
					tags.Add("wine_item");
				}
				else if (preserve.Value == PreserveType.Pickle)
				{
					tags.Add("pickle_item");
				}
			}
			if (preservedParentSheetIndex.Value > 0)
			{
				tags.Add("preserve_sheet_index_" + preservedParentSheetIndex.Value);
			}
		}

		protected virtual string loadDisplayName()
		{
			if (preserve.Value.HasValue)
			{
				Game1.objectInformation.TryGetValue(preservedParentSheetIndex, out var value);
				if (!string.IsNullOrEmpty(value))
				{
					string[] array = value.Split('/');
					string sub = array[4];
					switch (preserve.Value)
					{
					case PreserveType.Wine:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12730", sub);
					case PreserveType.Jelly:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12739", sub);
					case PreserveType.Pickle:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12735", sub);
					case PreserveType.Juice:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12726", sub);
					case PreserveType.Roe:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Roe_DisplayName", sub);
					case PreserveType.AgedRoe:
						if (preservedParentSheetIndex.Value > 0)
						{
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:AgedRoe_DisplayName", sub);
						}
						break;
					}
				}
			}
			else
			{
				if (name != null && name.Contains("Honey"))
				{
					_ = preservedParentSheetIndex.Value;
					if (preservedParentSheetIndex.Value == -1)
					{
						if (Name == "Honey")
						{
							Name = "Wild Honey";
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12750");
					}
					if (preservedParentSheetIndex.Value == 0)
					{
						Game1.objectInformation.TryGetValue(parentSheetIndex, out var value2);
						if (!string.IsNullOrEmpty(value2))
						{
							string[] array2 = value2.Split('/');
							return array2[4];
						}
					}
					string sub2 = Game1.objectInformation[preservedParentSheetIndex.Value].Split('/')[4];
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12760", sub2);
				}
				if ((bool)bigCraftable)
				{
					Game1.bigCraftablesInformation.TryGetValue(parentSheetIndex, out var value3);
					if (!string.IsNullOrEmpty(value3))
					{
						string[] array3 = value3.Split('/');
						return array3[array3.Length - 1];
					}
				}
				else
				{
					Game1.objectInformation.TryGetValue(parentSheetIndex, out var value4);
					if (!string.IsNullOrEmpty(value4))
					{
						string[] array4 = value4.Split('/');
						return array4[4];
					}
				}
			}
			return name;
		}

		public Vector2 getLocalPosition(xTile.Dimensions.Rectangle viewport)
		{
			return new Vector2(tileLocation.X * 64f - (float)viewport.X, tileLocation.Y * 64f - (float)viewport.Y);
		}

		public static Microsoft.Xna.Framework.Rectangle getSourceRectForBigCraftable(int index)
		{
			return new Microsoft.Xna.Framework.Rectangle(index % (Game1.bigCraftableSpriteSheet.Width / 16) * 16, index * 16 / Game1.bigCraftableSpriteSheet.Width * 16 * 2, 16, 32);
		}

		public virtual bool performToolAction(Tool t, GameLocation location)
		{
			if (isTemporarilyInvisible)
			{
				return false;
			}
			if ((bool)bigCraftable && (int)parentSheetIndex == 165 && heldObject.Value != null && heldObject.Value is Chest && !(heldObject.Value as Chest).isEmpty())
			{
				(heldObject.Value as Chest).clearNulls();
				if (t != null && t.isHeavyHitter() && !(t is MeleeWeapon))
				{
					location.playSound("hammer");
					shakeTimer = 100;
				}
				return false;
			}
			if (t == null)
			{
				if (location.objects.ContainsKey(tileLocation) && location.objects[tileLocation].Equals(this))
				{
					if (location.farmers.Count > 0)
					{
						Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 10), resource: false);
					}
					location.objects.Remove(tileLocation);
				}
				return false;
			}
			if (name.Equals("Stone") && t is Pickaxe)
			{
				int num = 1;
				switch ((int)t.upgradeLevel)
				{
				case 1:
					num = 2;
					break;
				case 2:
					num = 3;
					break;
				case 3:
					num = 4;
					break;
				case 4:
					num = 5;
					break;
				}
				if (((int)parentSheetIndex == 12 && (int)t.upgradeLevel == 1) || (((int)parentSheetIndex == 12 || (int)parentSheetIndex == 14) && (int)t.upgradeLevel == 0))
				{
					num = 0;
					location.playSound("crafting");
				}
				minutesUntilReady.Value -= num;
				if ((int)minutesUntilReady <= 0)
				{
					return true;
				}
				location.playSound("hammer");
				shakeTimer = 100;
				return false;
			}
			if (name.Equals("Stone") && t is Pickaxe)
			{
				return false;
			}
			if (name.Equals("Boulder") && ((int)t.upgradeLevel != 4 || !(t is Pickaxe)))
			{
				if (t.isHeavyHitter())
				{
					location.playSound("hammer");
				}
				return false;
			}
			if (name.Contains("Weeds") && t.isHeavyHitter())
			{
				if (base.ParentSheetIndex != 319 && base.ParentSheetIndex != 320 && base.ParentSheetIndex != 321 && t.getLastFarmerToUse() != null)
				{
					foreach (BaseEnchantment enchantment in t.getLastFarmerToUse().enchantments)
					{
						enchantment.OnCutWeed(tileLocation, location, t.getLastFarmerToUse());
					}
				}
				cutWeed(t.getLastFarmerToUse(), location);
				return true;
			}
			if (name.Contains("Twig") && t is Axe)
			{
				fragility.Value = 2;
				location.playSound("axchop");
				t.getLastFarmerToUse().currentLocation.debris.Add(new Debris(new Object(388, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 10), resource: false);
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
				return true;
			}
			if (name.Contains("SupplyCrate") && t.isHeavyHitter())
			{
				MinutesUntilReady -= (int)t.upgradeLevel + 1;
				if (MinutesUntilReady <= 0)
				{
					fragility.Value = 2;
					location.playSound("barrelBreak");
					Random random = new Random((int)Game1.uniqueIDForThisGame + (int)tileLocation.X * 777 + (int)tileLocation.Y * 7);
					int houseUpgradeLevel = t.getLastFarmerToUse().HouseUpgradeLevel;
					int xTile = (int)tileLocation.X;
					int yTile = (int)tileLocation.Y;
					switch (houseUpgradeLevel)
					{
					case 0:
						switch (random.Next(6))
						{
						case 0:
							Game1.createMultipleObjectDebris(770, xTile, yTile, random.Next(3, 6), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(371, xTile, yTile, random.Next(5, 8), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris(535, xTile, yTile, random.Next(2, 5), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(241, xTile, yTile, random.Next(1, 3), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris(395, xTile, yTile, random.Next(1, 3), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris(286, xTile, yTile, random.Next(3, 6), location);
							break;
						}
						break;
					case 1:
						switch (random.Next(9))
						{
						case 0:
							Game1.createMultipleObjectDebris(770, xTile, yTile, random.Next(3, 6), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(371, xTile, yTile, random.Next(5, 8), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris(749, xTile, yTile, random.Next(2, 5), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(253, xTile, yTile, random.Next(1, 3), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris(237, xTile, yTile, random.Next(1, 3), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris(246, xTile, yTile, random.Next(4, 8), location);
							break;
						case 6:
							Game1.createMultipleObjectDebris(247, xTile, yTile, random.Next(2, 5), location);
							break;
						case 7:
							Game1.createMultipleObjectDebris(245, xTile, yTile, random.Next(4, 8), location);
							break;
						case 8:
							Game1.createMultipleObjectDebris(287, xTile, yTile, random.Next(3, 6), location);
							break;
						}
						break;
					default:
						switch (random.Next(8))
						{
						case 0:
							Game1.createMultipleObjectDebris(770, xTile, yTile, random.Next(3, 6), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(920, xTile, yTile, random.Next(5, 8), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris(749, xTile, yTile, random.Next(2, 5), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(253, xTile, yTile, random.Next(2, 4), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris(random.Next(904, 906), xTile, yTile, random.Next(1, 3), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris(246, xTile, yTile, random.Next(4, 8), location);
							Game1.createMultipleObjectDebris(247, xTile, yTile, random.Next(2, 5), location);
							Game1.createMultipleObjectDebris(245, xTile, yTile, random.Next(4, 8), location);
							break;
						case 6:
							Game1.createMultipleObjectDebris(275, xTile, yTile, 2, location);
							break;
						case 7:
							Game1.createMultipleObjectDebris(288, xTile, yTile, random.Next(3, 6), location);
							break;
						}
						break;
					}
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 10), resource: false);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
					return true;
				}
				shakeTimer = 200;
				location.playSound("woodWhack");
				return false;
			}
			if ((int)parentSheetIndex == 590)
			{
				if (t is Hoe)
				{
					location.digUpArtifactSpot((int)tileLocation.X, (int)tileLocation.Y, t.getLastFarmerToUse());
					if (!location.terrainFeatures.ContainsKey(tileLocation))
					{
						location.makeHoeDirt(tileLocation, ignoreChecks: true);
					}
					location.playSound("hoeHit");
					if (location.objects.ContainsKey(tileLocation))
					{
						location.objects.Remove(tileLocation);
					}
				}
				return false;
			}
			if ((int)fragility == 2)
			{
				return false;
			}
			if (type != null && type.Equals("Crafting") && !(t is MeleeWeapon) && t.isHeavyHitter())
			{
				if (t is Hoe && IsSprinkler())
				{
					return false;
				}
				location.playSound("hammer");
				if ((int)fragility == 1)
				{
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(3, 6), resource: false);
					Game1.createRadialDebris(location, 14, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(3, 6), resource: false);
					DelayedAction.functionAfterDelay(delegate
					{
						Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(2, 5), resource: false);
						Game1.createRadialDebris(location, 14, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(2, 5), resource: false);
					}, 80);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
					performRemoveAction(tileLocation, location);
					if (location.objects.ContainsKey(tileLocation))
					{
						location.objects.Remove(tileLocation);
					}
					return false;
				}
				if (name.Contains("Tapper") && t.getLastFarmerToUse().currentLocation.terrainFeatures.ContainsKey(tileLocation) && t.getLastFarmerToUse().currentLocation.terrainFeatures[tileLocation] is Tree)
				{
					(t.getLastFarmerToUse().currentLocation.terrainFeatures[tileLocation] as Tree).tapped.Value = false;
				}
				if (Name == "Ostrich Incubator")
				{
					if (heldObject.Value != null)
					{
						base.ParentSheetIndex--;
						t.getLastFarmerToUse().currentLocation.debris.Add(new Debris((Object)heldObject, tileLocation.Value * 64f + new Vector2(32f, 32f)));
						heldObject.Value = null;
						return true;
					}
					return true;
				}
				if ((bool)bigCraftable && base.ParentSheetIndex == 21 && heldObject.Value != null)
				{
					t.getLastFarmerToUse().currentLocation.debris.Add(new Debris((Object)heldObject, tileLocation.Value * 64f + new Vector2(32f, 32f)));
					heldObject.Value = null;
				}
				if (IsSprinkler() && heldObject.Value != null)
				{
					if (heldObject.Value.heldObject.Value != null)
					{
						Chest chest = heldObject.Value.heldObject.Value as Chest;
						if (chest != null)
						{
							chest.GetMutex().RequestLock(delegate
							{
								List<Item> list = new List<Item>(chest.items);
								chest.items.Clear();
								foreach (Item item in list)
								{
									if (item != null)
									{
										t.getLastFarmerToUse().currentLocation.debris.Add(new Debris(item, tileLocation.Value * 64f + new Vector2(32f, 32f)));
									}
								}
								Object value = heldObject.Value;
								heldObject.Value = null;
								t.getLastFarmerToUse().currentLocation.debris.Add(new Debris(value, tileLocation.Value * 64f + new Vector2(32f, 32f)));
								chest.GetMutex().ReleaseLock();
							});
						}
						return false;
					}
					t.getLastFarmerToUse().currentLocation.debris.Add(new Debris((Object)heldObject, tileLocation.Value * 64f + new Vector2(32f, 32f)));
					heldObject.Value = null;
					return false;
				}
				if (heldObject.Value != null && (bool)readyForHarvest)
				{
					t.getLastFarmerToUse().currentLocation.debris.Add(new Debris((Object)heldObject, tileLocation.Value * 64f + new Vector2(32f, 32f)));
				}
				if ((int)parentSheetIndex == 157)
				{
					base.ParentSheetIndex = 156;
					heldObject.Value = null;
					minutesUntilReady.Value = -1;
				}
				if (name.Contains("Seasonal"))
				{
					base.ParentSheetIndex -= base.ParentSheetIndex % 4;
				}
				return true;
			}
			return false;
		}

		protected virtual void cutWeed(Farmer who, GameLocation location = null)
		{
			if (location == null && who != null)
			{
				location = who.currentLocation;
			}
			Color color = Color.Green;
			string text = "cut";
			int rowInAnimationTexture = 50;
			fragility.Value = 2;
			int num = -1;
			if (Game1.random.NextDouble() < 0.5)
			{
				num = 771;
			}
			else if (Game1.random.NextDouble() < 0.05)
			{
				num = 770;
			}
			switch ((int)parentSheetIndex)
			{
			case 678:
				color = new Color(228, 109, 159);
				break;
			case 679:
				color = new Color(253, 191, 46);
				break;
			case 313:
			case 314:
			case 315:
				color = new Color(84, 101, 27);
				break;
			case 316:
			case 317:
			case 318:
				color = new Color(109, 49, 196);
				break;
			case 319:
				color = new Color(30, 216, 255);
				text = "breakingGlass";
				rowInAnimationTexture = 47;
				location.playSound("drumkit2");
				num = -1;
				break;
			case 320:
				color = new Color(175, 143, 255);
				text = "breakingGlass";
				rowInAnimationTexture = 47;
				location.playSound("drumkit2");
				num = -1;
				break;
			case 321:
				color = new Color(73, 255, 158);
				text = "breakingGlass";
				rowInAnimationTexture = 47;
				location.playSound("drumkit2");
				num = -1;
				break;
			case 792:
			case 793:
			case 794:
				num = 770;
				break;
			case 882:
			case 883:
			case 884:
				color = new Color(30, 97, 68);
				if (Game1.MasterPlayer.hasOrWillReceiveMail("islandNorthCaveOpened") && Game1.random.NextDouble() < 0.1 && !Game1.MasterPlayer.hasOrWillReceiveMail("gotMummifiedFrog"))
				{
					Game1.addMailForTomorrow("gotMummifiedFrog", noLetter: true, sendToEveryone: true);
					num = 828;
				}
				else if (Game1.random.NextDouble() < 0.01)
				{
					num = 828;
				}
				else if (Game1.random.NextDouble() < 0.08)
				{
					num = 831;
				}
				break;
			}
			if (text.Equals("breakingGlass") && Game1.random.NextDouble() < 0.0025)
			{
				num = 338;
			}
			location.playSound(text);
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(rowInAnimationTexture, tileLocation.Value * 64f, color));
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(rowInAnimationTexture, tileLocation.Value * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), color * 0.75f)
			{
				scale = 0.75f,
				flipped = true
			});
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(rowInAnimationTexture, tileLocation.Value * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), color * 0.75f)
			{
				scale = 0.75f,
				delayBeforeAnimationStart = 50
			});
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(rowInAnimationTexture, tileLocation.Value * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), color * 0.75f)
			{
				scale = 0.75f,
				flipped = true,
				delayBeforeAnimationStart = 100
			});
			if (!text.Equals("breakingGlass"))
			{
				if (Game1.random.NextDouble() < 1E-05)
				{
					location.debris.Add(new Debris(new Hat(40), tileLocation.Value * 64f + new Vector2(32f, 32f)));
				}
				if (Game1.random.NextDouble() <= 0.01 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
				{
					location.debris.Add(new Debris(new Object(890, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
				}
			}
			if (num != -1)
			{
				location.debris.Add(new Debris(new Object(num, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
			}
			if (Game1.random.NextDouble() < 0.02)
			{
				location.addJumperFrog(tileLocation);
			}
			if (who.currentLocation.HasUnlockedAreaSecretNotes(who) && Game1.random.NextDouble() < 0.009)
			{
				Object @object = location.tryToCreateUnseenSecretNote(who);
				if (@object != null)
				{
					Game1.createItemDebris(@object, new Vector2(tileLocation.X + 0.5f, tileLocation.Y + 0.75f) * 64f, Game1.player.facingDirection, location);
				}
			}
		}

		public virtual bool isAnimalProduct()
		{
			if (base.Category != -18 && base.Category != -5 && base.Category != -6)
			{
				return (int)parentSheetIndex == 430;
			}
			return true;
		}

		public virtual bool onExplosion(Farmer who, GameLocation location)
		{
			if (who == null)
			{
				return false;
			}
			if (name.Contains("Weed"))
			{
				fragility.Value = 0;
				cutWeed(who, location);
				location.removeObject(tileLocation, showDestroyedObject: false);
			}
			if (name.Contains("Twig"))
			{
				fragility.Value = 0;
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 10), resource: false);
				location.debris.Add(new Debris(new Object(388, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
			}
			if (name.Contains("Stone"))
			{
				fragility.Value = 0;
			}
			performRemoveAction(tileLocation, location);
			return true;
		}

		public virtual bool canBeShipped()
		{
			if (!bigCraftable && type != null && !type.Equals("Quest") && canBeTrashed() && !(this is Furniture))
			{
				return !(this is Wallpaper);
			}
			return false;
		}

		public virtual void ApplySprinkler(GameLocation location, Vector2 tile)
		{
			if (!(location.doesTileHavePropertyNoNull((int)tile.X, (int)tile.Y, "NoSprinklers", "Back") == "T") && location.terrainFeatures.ContainsKey(tile) && location.terrainFeatures[tile] is HoeDirt && (int)(location.terrainFeatures[tile] as HoeDirt).state != 2)
			{
				(location.terrainFeatures[tile] as HoeDirt).state.Value = 1;
			}
		}

		public virtual void ApplySprinklerAnimation(GameLocation location)
		{
			int modifiedRadiusForSprinkler = GetModifiedRadiusForSprinkler();
			if (modifiedRadiusForSprinkler >= 0)
			{
				switch (modifiedRadiusForSprinkler)
				{
				case 0:
				{
					int delayBeforeAnimationStart = Game1.random.Next(1000);
					location.temporarySprites.Add(new TemporaryAnimatedSprite(29, tileLocation.Value * 64f + new Vector2(0f, -48f), Color.White * 0.5f, 4, flipped: false, 60f, 100)
					{
						delayBeforeAnimationStart = delayBeforeAnimationStart,
						id = tileLocation.X * 4000f + tileLocation.Y
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite(29, tileLocation.Value * 64f + new Vector2(48f, 0f), Color.White * 0.5f, 4, flipped: false, 60f, 100)
					{
						rotation = (float)Math.PI / 2f,
						delayBeforeAnimationStart = delayBeforeAnimationStart,
						id = tileLocation.X * 4000f + tileLocation.Y
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite(29, tileLocation.Value * 64f + new Vector2(0f, 48f), Color.White * 0.5f, 4, flipped: false, 60f, 100)
					{
						rotation = (float)Math.PI,
						delayBeforeAnimationStart = delayBeforeAnimationStart,
						id = tileLocation.X * 4000f + tileLocation.Y
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite(29, tileLocation.Value * 64f + new Vector2(-48f, 0f), Color.White * 0.5f, 4, flipped: false, 60f, 100)
					{
						rotation = 4.712389f,
						delayBeforeAnimationStart = delayBeforeAnimationStart,
						id = tileLocation.X * 4000f + tileLocation.Y
					});
					break;
				}
				case 1:
					location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1984, 192, 192), 60f, 3, 100, tileLocation.Value * 64f + new Vector2(-64f, -64f), flicker: false, flipped: false)
					{
						color = Color.White * 0.4f,
						delayBeforeAnimationStart = Game1.random.Next(1000),
						id = tileLocation.X * 4000f + tileLocation.Y
					});
					break;
				default:
				{
					float num = (float)modifiedRadiusForSprinkler / 2f;
					location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2176, 320, 320), 60f, 4, 100, tileLocation.Value * 64f + new Vector2(32f, 32f) + new Vector2(-160f, -160f) * num, flicker: false, flipped: false)
					{
						color = Color.White * 0.4f,
						delayBeforeAnimationStart = Game1.random.Next(1000),
						id = tileLocation.X * 4000f + tileLocation.Y,
						scale = num
					});
					break;
				}
				}
			}
		}

		public virtual List<Vector2> GetSprinklerTiles()
		{
			int modifiedRadiusForSprinkler = GetModifiedRadiusForSprinkler();
			if (modifiedRadiusForSprinkler == 0)
			{
				return Utility.getAdjacentTileLocations(tileLocation);
			}
			if (modifiedRadiusForSprinkler > 0)
			{
				List<Vector2> list = new List<Vector2>();
				for (int i = (int)tileLocation.X - modifiedRadiusForSprinkler; (float)i <= tileLocation.X + (float)modifiedRadiusForSprinkler; i++)
				{
					for (int j = (int)tileLocation.Y - modifiedRadiusForSprinkler; (float)j <= tileLocation.Y + (float)modifiedRadiusForSprinkler; j++)
					{
						if (i != 0 || j != 0)
						{
							list.Add(new Vector2(i, j));
						}
					}
				}
				return list;
			}
			return new List<Vector2>();
		}

		public virtual bool IsInSprinklerRangeBroadphase(Vector2 target)
		{
			int num = GetModifiedRadiusForSprinkler();
			if (num == 0)
			{
				num = 1;
			}
			if (Math.Abs(target.X - TileLocation.X) <= (float)num)
			{
				return Math.Abs(target.Y - TileLocation.Y) <= (float)num;
			}
			return false;
		}

		public virtual void DayUpdate(GameLocation location)
		{
			health = 10;
			if (IsSprinkler() && (!Game1.IsRainingHere(location) || !location.isOutdoors))
			{
				int modifiedRadiusForSprinkler = GetModifiedRadiusForSprinkler();
				if (modifiedRadiusForSprinkler >= 0)
				{
					location.postFarmEventOvernightActions.Add(delegate
					{
						if (!Game1.player.team.SpecialOrderRuleActive("NO_SPRINKLER"))
						{
							List<Vector2> sprinklerTiles = GetSprinklerTiles();
							foreach (Vector2 item in sprinklerTiles)
							{
								ApplySprinkler(location, item);
							}
							ApplySprinklerAnimation(location);
						}
					});
				}
			}
			if ((bool)bigCraftable)
			{
				switch ((int)parentSheetIndex)
				{
				case 231:
					if (!Game1.IsRainingHere(location) && location.IsOutdoors)
					{
						MinutesUntilReady -= 2400;
						if (MinutesUntilReady <= 0)
						{
							readyForHarvest.Value = true;
						}
					}
					break;
				case 246:
					heldObject.Value = new Object(395, 1);
					readyForHarvest.Value = true;
					break;
				case 272:
				{
					if (!(location is AnimalHouse))
					{
						break;
					}
					AnimalHouse animalHouse = location as AnimalHouse;
					foreach (KeyValuePair<long, FarmAnimal> pair in animalHouse.animals.Pairs)
					{
						pair.Value.pet(Game1.player, is_auto_pet: true);
					}
					break;
				}
				case 165:
				{
					if (location == null || !(location is AnimalHouse))
					{
						break;
					}
					AnimalHouse animalHouse2 = location as AnimalHouse;
					foreach (KeyValuePair<long, FarmAnimal> pair2 in animalHouse2.animals.Pairs)
					{
						if ((byte)pair2.Value.harvestType == 1 && (int)pair2.Value.currentProduce > 0 && (int)pair2.Value.currentProduce != 430 && heldObject.Value != null && heldObject.Value is Chest && (heldObject.Value as Chest).addItem(new Object(Vector2.Zero, pair2.Value.currentProduce.Value, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Quality = pair2.Value.produceQuality
						}) == null)
						{
							Utility.RecordAnimalProduce(pair2.Value, pair2.Value.currentProduce);
							pair2.Value.currentProduce.Value = -1;
							if ((bool)pair2.Value.showDifferentTextureWhenReadyForHarvest)
							{
								pair2.Value.Sprite.LoadTexture("Animals\\Sheared" + pair2.Value.type.Value);
							}
							showNextIndex.Value = true;
						}
					}
					break;
				}
				case 157:
					if ((int)minutesUntilReady <= 0 && heldObject.Value != null && location.canSlimeHatchHere())
					{
						GreenSlime greenSlime = null;
						Vector2 position = new Vector2((int)tileLocation.X, (int)tileLocation.Y + 1) * 64f;
						switch ((int)heldObject.Value.parentSheetIndex)
						{
						case 680:
							greenSlime = new GreenSlime(position, 0);
							break;
						case 413:
							greenSlime = new GreenSlime(position, 40);
							break;
						case 437:
							greenSlime = new GreenSlime(position, 80);
							break;
						case 439:
							greenSlime = new GreenSlime(position, 121);
							break;
						case 857:
							greenSlime = new GreenSlime(position, 121);
							greenSlime.makeTigerSlime();
							break;
						}
						if (greenSlime != null)
						{
							Game1.showGlobalMessage(greenSlime.cute ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12689") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12691"));
							Vector2 vector = Utility.recursiveFindOpenTileForCharacter(greenSlime, location, tileLocation + new Vector2(0f, 1f), 10, allowOffMap: false);
							greenSlime.setTilePosition((int)vector.X, (int)vector.Y);
							location.characters.Add(greenSlime);
							heldObject.Value = null;
							base.ParentSheetIndex = 156;
							minutesUntilReady.Value = -1;
						}
					}
					break;
				case 10:
					if (location.GetSeasonForLocation().Equals("winter"))
					{
						heldObject.Value = null;
						readyForHarvest.Value = false;
						showNextIndex.Value = false;
						minutesUntilReady.Value = -1;
					}
					else if (heldObject.Value == null)
					{
						heldObject.Value = new Object(Vector2.Zero, 340, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 4);
					}
					break;
				case 108:
				case 109:
					base.ParentSheetIndex = 108;
					if (Game1.currentSeason.Equals("winter") || Game1.currentSeason.Equals("fall"))
					{
						base.ParentSheetIndex = 109;
					}
					break;
				case 117:
					heldObject.Value = new Object(167, 1);
					break;
				case 104:
					if (Game1.currentSeason.Equals("winter"))
					{
						minutesUntilReady.Value = 9999;
					}
					else
					{
						minutesUntilReady.Value = -1;
					}
					break;
				case 127:
				{
					NPC todaysBirthdayNPC = Utility.getTodaysBirthdayNPC(Game1.currentSeason, Game1.dayOfMonth);
					minutesUntilReady.Value = 1;
					if (todaysBirthdayNPC != null)
					{
						heldObject.Value = todaysBirthdayNPC.getFavoriteItem();
						break;
					}
					int num2 = 80;
					switch (Game1.random.Next(4))
					{
					case 0:
						num2 = 72;
						break;
					case 1:
						num2 = 337;
						break;
					case 2:
						num2 = 749;
						break;
					case 3:
						num2 = 336;
						break;
					}
					heldObject.Value = new Object(num2, 1);
					break;
				}
				case 160:
					minutesUntilReady.Value = 1;
					heldObject.Value = new Object(386, Game1.random.Next(2, 9));
					break;
				case 280:
					minutesUntilReady.Value = 1;
					heldObject.Value = new Object(74, 1);
					break;
				case 164:
					if (!(location is Town))
					{
						break;
					}
					if (Game1.random.NextDouble() < 0.9)
					{
						if (Game1.getLocationFromName("ManorHouse").isTileLocationTotallyClearAndPlaceable(22, 6))
						{
							if (!Game1.player.hasOrWillReceiveMail("lewisStatue"))
							{
								Game1.mailbox.Add("lewisStatue");
							}
							rot();
							Game1.getLocationFromName("ManorHouse").objects.Add(new Vector2(22f, 6f), new Object(Vector2.Zero, 164));
						}
					}
					else if (Game1.getLocationFromName("AnimalShop").isTileLocationTotallyClearAndPlaceable(11, 6))
					{
						if (!Game1.player.hasOrWillReceiveMail("lewisStatue"))
						{
							Game1.mailbox.Add("lewisStatue");
						}
						rot();
						Game1.getLocationFromName("AnimalShop").objects.Add(new Vector2(11f, 6f), new Object(Vector2.Zero, 164));
					}
					break;
				case 128:
					if (heldObject.Value == null)
					{
						int num = 404;
						num = ((Game1.random.NextDouble() < 0.025) ? 422 : ((Game1.random.NextDouble() < 0.075) ? 281 : ((Game1.random.NextDouble() < 0.09) ? 257 : ((!(Game1.random.NextDouble() < 0.15)) ? 404 : 420))));
						heldObject.Value = new Object(num, 1);
						Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 2);
					}
					break;
				}
				if (name.Contains("Seasonal"))
				{
					int num3 = base.ParentSheetIndex - base.ParentSheetIndex % 4;
					base.ParentSheetIndex = num3 + Utility.getSeasonNumber(Game1.currentSeason);
				}
			}
			if ((bool)bigCraftable)
			{
				return;
			}
			string seasonForLocation = location.GetSeasonForLocation();
			switch ((int)parentSheetIndex)
			{
			case 746:
				if (seasonForLocation.Equals("winter"))
				{
					rot();
				}
				break;
			case 784:
			case 785:
				if (Game1.dayOfMonth == 1 && !seasonForLocation.Equals("spring") && (bool)location.isOutdoors)
				{
					base.ParentSheetIndex++;
				}
				break;
			case 674:
			case 675:
				if (Game1.dayOfMonth == 1 && seasonForLocation.Equals("summer") && (bool)location.isOutdoors)
				{
					base.ParentSheetIndex += 2;
				}
				break;
			case 676:
			case 677:
				if (Game1.dayOfMonth == 1 && seasonForLocation.Equals("fall") && (bool)location.isOutdoors)
				{
					base.ParentSheetIndex += 2;
				}
				break;
			}
		}

		public virtual void rot()
		{
			Random random = new Random(Game1.year * 999 + Game1.dayOfMonth + Utility.getSeasonNumber(Game1.currentSeason));
			base.ParentSheetIndex = random.Next(747, 749);
			price.Value = 0;
			quality.Value = 0;
			name = "Rotten Plant";
			displayName = null;
			lightSource = null;
			bigCraftable.Value = false;
		}

		public override void actionWhenBeingHeld(Farmer who)
		{
			if (Game1.eventUp && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
			{
				if (lightSource != null && who.currentLocation != null && who.currentLocation.hasLightSource((int)who.UniqueMultiplayerID))
				{
					who.currentLocation.removeLightSource((int)who.UniqueMultiplayerID);
				}
				base.actionWhenBeingHeld(who);
				return;
			}
			if (lightSource != null && (!bigCraftable || (bool)isLamp) && who.currentLocation != null)
			{
				if (!who.currentLocation.hasLightSource((int)who.UniqueMultiplayerID))
				{
					who.currentLocation.sharedLights[(int)who.UniqueMultiplayerID] = new LightSource(lightSource.textureIndex, lightSource.position, lightSource.radius, lightSource.color, (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.uniqueMultiplayerID);
				}
				who.currentLocation.repositionLightSource((int)who.UniqueMultiplayerID, who.position + new Vector2(32f, -64f));
			}
			base.actionWhenBeingHeld(who);
		}

		public override void actionWhenStopBeingHeld(Farmer who)
		{
			if (lightSource != null && who.currentLocation != null && who.currentLocation.hasLightSource((int)who.UniqueMultiplayerID))
			{
				who.currentLocation.removeLightSource((int)who.UniqueMultiplayerID);
			}
			base.actionWhenStopBeingHeld(who);
		}

		public virtual void ConsumeInventoryItem(Farmer who, int parent_sheet_index, int amount)
		{
			IList<Item> items = who.Items;
			if (autoLoadChest != null)
			{
				items = autoLoadChest.items;
			}
			for (int num = items.Count - 1; num >= 0; num--)
			{
				if (Utility.IsNormalObjectAtParentSheetIndex(items[num], parent_sheet_index))
				{
					items[num].Stack--;
					if (items[num].Stack <= 0)
					{
						if (who.ActiveObject == items[num])
						{
							who.ActiveObject = null;
						}
						items[num] = null;
					}
					break;
				}
			}
		}

		public virtual void ConsumeInventoryItem(Farmer who, Item drop_in, int amount)
		{
			drop_in.Stack -= amount;
			if (drop_in.Stack > 0)
			{
				return;
			}
			if (autoLoadChest != null)
			{
				bool flag = false;
				for (int i = 0; i < autoLoadChest.items.Count; i++)
				{
					if (autoLoadChest.items[i] == drop_in)
					{
						autoLoadChest.items[i] = null;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					autoLoadChest.clearNulls();
				}
			}
			else
			{
				who.removeItemFromInventory(drop_in);
			}
		}

		public virtual int GetTallyOfObject(Farmer who, int index, bool big_craftable)
		{
			if (autoLoadChest != null)
			{
				int num = 0;
				{
					foreach (Item item in autoLoadChest.items)
					{
						if (item != null && item is Object && (item as Object).ParentSheetIndex == index && (bool)(item as Object).bigCraftable == big_craftable)
						{
							num += item.Stack;
						}
					}
					return num;
				}
			}
			return who.getTallyOfObject(index, big_craftable);
		}

		public virtual Object GetDeconstructorOutput(Item item)
		{
			if (!CraftingRecipe.craftingRecipes.ContainsKey(item.Name))
			{
				return null;
			}
			if (CraftingRecipe.craftingRecipes[item.Name].Split('/')[2].Split(' ').Count() > 1)
			{
				return null;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 710))
			{
				return new Object(334, 2);
			}
			string[] array = CraftingRecipe.craftingRecipes[item.Name].Split('/')[0].Split(' ');
			List<Object> list = new List<Object>();
			for (int i = 0; i < array.Count(); i += 2)
			{
				list.Add(new Object(Convert.ToInt32(array[i]), Convert.ToInt32(array[i + 1])));
			}
			if (list.Count == 0)
			{
				return null;
			}
			list.Sort((Object a, Object b) => a.sellToStorePrice(-1L) * a.Stack - b.sellToStorePrice(-1L) * b.Stack);
			return list.Last();
		}

		public virtual bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
		{
			if (isTemporarilyInvisible)
			{
				return false;
			}
			if (dropInItem is Object)
			{
				Object @object = dropInItem as Object;
				if (IsSprinkler() && heldObject.Value == null && (Utility.IsNormalObjectAtParentSheetIndex(dropInItem, 915) || Utility.IsNormalObjectAtParentSheetIndex(dropInItem, 913)))
				{
					if (probe)
					{
						return true;
					}
					if (who.currentLocation is MineShaft || (who.currentLocation is VolcanoDungeon && Utility.IsNormalObjectAtParentSheetIndex(dropInItem, 913)))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					Object object2 = @object.getOne() as Object;
					if (Utility.IsNormalObjectAtParentSheetIndex(object2, 913) && object2.heldObject.Value == null)
					{
						Chest chest = new Chest();
						chest.SpecialChestType = Chest.SpecialChestTypes.Enricher;
						object2.heldObject.Value = chest;
					}
					who.currentLocation.playSound("axe");
					heldObject.Value = object2;
					minutesUntilReady.Value = -1;
					return true;
				}
				if (dropInItem is Wallpaper)
				{
					return false;
				}
				if (@object != null && (int)@object.parentSheetIndex == 872 && autoLoadChest == null)
				{
					if (Name == "Ostrich Incubator" || Name == "Slime Incubator" || Name == "Incubator")
					{
						return false;
					}
					if (MinutesUntilReady > 0)
					{
						if (probe)
						{
							return true;
						}
						Utility.addSprinklesToLocation(who.currentLocation, (int)tileLocation.X, (int)tileLocation.Y, 1, 2, 400, 40, Color.White);
						Game1.playSound("yoba");
						MinutesUntilReady = 10;
						who.reduceActiveItemByOne();
						DelayedAction.functionAfterDelay(delegate
						{
							minutesElapsed(10, who.currentLocation);
						}, 50);
					}
				}
				if (heldObject.Value != null && !name.Equals("Recycling Machine") && !name.Equals("Crystalarium"))
				{
					return false;
				}
				if (@object != null && (bool)@object.bigCraftable && !name.Equals("Deconstructor"))
				{
					return false;
				}
				if ((bool)bigCraftable && !probe && @object != null && heldObject.Value == null)
				{
					scale.X = 5f;
				}
				if (probe && MinutesUntilReady > 0)
				{
					return false;
				}
				if (name.Equals("Incubator"))
				{
					if (heldObject.Value == null && @object.ParentSheetIndex != 289 && (@object.Category == -5 || Utility.IsNormalObjectAtParentSheetIndex(@object, 107)))
					{
						heldObject.Value = new Object(@object.parentSheetIndex, 1);
						if (!probe)
						{
							who.currentLocation.playSound("coin");
							minutesUntilReady.Value = 9000 * (((int)@object.parentSheetIndex != 107) ? 1 : 2);
							if (who.professions.Contains(2))
							{
								minutesUntilReady.Value /= 2;
							}
							if (@object.ParentSheetIndex == 180 || @object.ParentSheetIndex == 182 || @object.ParentSheetIndex == 305)
							{
								base.ParentSheetIndex += 2;
							}
							else
							{
								base.ParentSheetIndex++;
							}
							if (who != null && who.currentLocation != null && who.currentLocation is AnimalHouse)
							{
								(who.currentLocation as AnimalHouse).hasShownIncubatorBuildingFullMessage = false;
							}
						}
						return true;
					}
				}
				else if (name.Equals("Ostrich Incubator"))
				{
					if (heldObject.Value == null && (int)@object.parentSheetIndex == 289)
					{
						heldObject.Value = new Object(@object.parentSheetIndex, 1);
						if (!probe)
						{
							who.currentLocation.playSound("coin");
							minutesUntilReady.Value = 15000;
							if (who.professions.Contains(2))
							{
								minutesUntilReady.Value /= 2;
							}
							base.ParentSheetIndex++;
							if (who != null && who.currentLocation != null && who.currentLocation is AnimalHouse)
							{
								(who.currentLocation as AnimalHouse).hasShownIncubatorBuildingFullMessage = false;
							}
						}
						return true;
					}
				}
				else if (name.Equals("Slime Incubator"))
				{
					if (heldObject.Value == null && @object.name.Contains("Slime Egg"))
					{
						heldObject.Value = new Object(@object.parentSheetIndex, 1);
						if (!probe)
						{
							who.currentLocation.playSound("coin");
							minutesUntilReady.Value = 4000;
							if (who.professions.Contains(2))
							{
								minutesUntilReady.Value /= 2;
							}
							base.ParentSheetIndex++;
						}
						return true;
					}
				}
				else if (name.Equals("Deconstructor"))
				{
					Object deconstructorOutput = GetDeconstructorOutput(@object);
					if (deconstructorOutput != null)
					{
						heldObject.Value = new Object(@object.parentSheetIndex, 1);
						if (!probe)
						{
							heldObject.Value = deconstructorOutput;
							MinutesUntilReady = 60;
							Game1.playSound("furnace");
							return true;
						}
						return true;
					}
					if (!probe)
					{
						if (autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Deconstructor_fail"));
						}
						return false;
					}
				}
				else if (name.Equals("Bone Mill"))
				{
					int num = 0;
					switch ((int)@object.parentSheetIndex)
					{
					case 579:
					case 580:
					case 581:
					case 582:
					case 583:
					case 584:
					case 585:
					case 586:
					case 587:
					case 588:
					case 589:
					case 820:
					case 821:
					case 822:
					case 823:
					case 824:
					case 825:
					case 826:
					case 827:
					case 828:
						num = 1;
						break;
					case 881:
						num = 5;
						break;
					}
					if (num == 0)
					{
						return false;
					}
					if (probe)
					{
						return true;
					}
					if (@object.Stack < num)
					{
						if (autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:bonemill_5"));
						}
						return false;
					}
					int num2 = -1;
					int num3 = 1;
					switch (Game1.random.Next(4))
					{
					case 0:
						num2 = 466;
						num3 = 3;
						break;
					case 1:
						num2 = 465;
						num3 = 5;
						break;
					case 2:
						num2 = 369;
						num3 = 10;
						break;
					case 3:
						num2 = 805;
						num3 = 5;
						break;
					}
					if (Game1.random.NextDouble() < 0.1)
					{
						num3 *= 2;
					}
					heldObject.Value = new Object(num2, num3);
					if (!probe)
					{
						ConsumeInventoryItem(who, @object, num);
						minutesUntilReady.Value = 240;
						who.currentLocation.playSound("skeletonStep");
						DelayedAction.playSoundAfterDelay("skeletonHit", 150);
					}
				}
				else if (name.Equals("Keg"))
				{
					switch ((int)@object.parentSheetIndex)
					{
					case 262:
						heldObject.Value = new Object(Vector2.Zero, 346, "Beer", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							heldObject.Value.name = "Beer";
							who.currentLocation.playSound("Ship");
							who.currentLocation.playSound("bubbles");
							minutesUntilReady.Value = 1750;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 304:
						heldObject.Value = new Object(Vector2.Zero, 303, "Pale Ale", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							heldObject.Value.name = "Pale Ale";
							who.currentLocation.playSound("Ship");
							who.currentLocation.playSound("bubbles");
							minutesUntilReady.Value = 2250;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 815:
						heldObject.Value = new Object(Vector2.Zero, 614, "Green Tea", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							heldObject.Value.name = "Green Tea";
							who.currentLocation.playSound("Ship");
							who.currentLocation.playSound("bubbles");
							minutesUntilReady.Value = 180;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Lime * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 433:
						if (@object.Stack < 5 && !probe)
						{
							if (autoLoadChest == null)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12721"));
							}
							return false;
						}
						heldObject.Value = new Object(Vector2.Zero, 395, "Coffee", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							heldObject.Value.name = "Coffee";
							who.currentLocation.playSound("Ship");
							who.currentLocation.playSound("bubbles");
							ConsumeInventoryItem(who, @object, 4);
							minutesUntilReady.Value = 120;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.DarkGray * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 340:
						heldObject.Value = new Object(Vector2.Zero, 459, "Mead", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							heldObject.Value.name = "Mead";
							who.currentLocation.playSound("Ship");
							who.currentLocation.playSound("bubbles");
							minutesUntilReady.Value = 600;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					}
					switch (@object.Category)
					{
					case -75:
						heldObject.Value = new Object(Vector2.Zero, 350, @object.Name + " Juice", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						heldObject.Value.Price = (int)((double)@object.Price * 2.25);
						if (!probe)
						{
							heldObject.Value.name = @object.Name + " Juice";
							heldObject.Value.preserve.Value = PreserveType.Juice;
							heldObject.Value.preservedParentSheetIndex.Value = @object.parentSheetIndex;
							who.currentLocation.playSound("bubbles");
							who.currentLocation.playSound("Ship");
							minutesUntilReady.Value = 6000;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.White * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case -79:
						heldObject.Value = new Object(Vector2.Zero, 348, @object.Name + " Wine", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						heldObject.Value.Price = @object.Price * 3;
						if (!probe)
						{
							heldObject.Value.name = @object.Name + " Wine";
							heldObject.Value.preserve.Value = PreserveType.Wine;
							heldObject.Value.preservedParentSheetIndex.Value = @object.parentSheetIndex;
							who.currentLocation.playSound("Ship");
							who.currentLocation.playSound("bubbles");
							minutesUntilReady.Value = 10000;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Lavender * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					}
				}
				else if (name.Equals("Preserves Jar"))
				{
					switch (@object.Category)
					{
					case -75:
						heldObject.Value = new Object(Vector2.Zero, 342, "Pickled " + @object.Name, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						heldObject.Value.Price = 50 + @object.Price * 2;
						if (!probe)
						{
							heldObject.Value.name = "Pickled " + @object.Name;
							heldObject.Value.preserve.Value = PreserveType.Pickle;
							heldObject.Value.preservedParentSheetIndex.Value = @object.parentSheetIndex;
							who.currentLocation.playSound("Ship");
							minutesUntilReady.Value = 4000;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.White * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case -79:
						heldObject.Value = new Object(Vector2.Zero, 344, @object.Name + " Jelly", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						heldObject.Value.Price = 50 + @object.Price * 2;
						if (!probe)
						{
							minutesUntilReady.Value = 4000;
							heldObject.Value.name = @object.Name + " Jelly";
							heldObject.Value.preserve.Value = PreserveType.Jelly;
							heldObject.Value.preservedParentSheetIndex.Value = @object.parentSheetIndex;
							who.currentLocation.playSound("Ship");
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.LightBlue * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					}
					switch ((int)@object.parentSheetIndex)
					{
					case 829:
						heldObject.Value = new Object(Vector2.Zero, 342, "Pickled " + @object.Name, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						heldObject.Value.Price = 50 + @object.Price * 2;
						if (!probe)
						{
							heldObject.Value.name = "Pickled " + @object.Name;
							heldObject.Value.preserve.Value = PreserveType.Pickle;
							heldObject.Value.preservedParentSheetIndex.Value = @object.parentSheetIndex;
							who.currentLocation.playSound("Ship");
							minutesUntilReady.Value = 4000;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.White * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 812:
					{
						if ((int)@object.preservedParentSheetIndex == 698)
						{
							heldObject.Value = new Object(445, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 6000;
								who.currentLocation.playSound("Ship");
								Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.LightBlue * 0.75f, 1f, 0f, 0f, 0f)
								{
									alphaFade = 0.005f
								});
							}
							return true;
						}
						Object object3 = null;
						object3 = ((!(@object is ColoredObject coloredObject)) ? new Object(447, 1) : new ColoredObject(447, 1, coloredObject.color));
						heldObject.Value = object3;
						heldObject.Value.Price = @object.Price * 2;
						if (!probe)
						{
							minutesUntilReady.Value = 4000;
							heldObject.Value.name = "Aged " + @object.Name;
							heldObject.Value.preserve.Value = PreserveType.AgedRoe;
							heldObject.Value.Category = -26;
							heldObject.Value.preservedParentSheetIndex.Value = @object.preservedParentSheetIndex;
							who.currentLocation.playSound("Ship");
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.LightBlue * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					}
					}
				}
				else if (name.Equals("Cheese Press"))
				{
					int num4 = 1;
					switch (@object.ParentSheetIndex)
					{
					case 436:
						heldObject.Value = new Object(Vector2.Zero, 426, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Stack = num4
						};
						if (!probe)
						{
							minutesUntilReady.Value = 200;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 438:
						heldObject.Value = new Object(Vector2.Zero, 426, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Quality = 2,
							Stack = num4
						};
						if (!probe)
						{
							minutesUntilReady.Value = 200;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 184:
						heldObject.Value = new Object(Vector2.Zero, 424, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Stack = num4
						};
						if (!probe)
						{
							minutesUntilReady.Value = 200;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 186:
						heldObject.Value = new Object(Vector2.Zero, 424, "Cheese (=)", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Quality = 2,
							Stack = num4
						};
						if (!probe)
						{
							minutesUntilReady.Value = 200;
							who.currentLocation.playSound("Ship");
						}
						return true;
					}
				}
				else if (name.Equals("Mayonnaise Machine"))
				{
					switch (@object.ParentSheetIndex)
					{
					case 289:
						heldObject.Value = new Object(Vector2.Zero, 306, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							who.currentLocation.playSound("Ship");
							heldObject.Value.Stack = 10;
							heldObject.Value.Quality = @object.Quality;
						}
						return true;
					case 174:
					case 182:
						heldObject.Value = new Object(Vector2.Zero, 306, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Quality = 2
						};
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 176:
					case 180:
						heldObject.Value = new Object(Vector2.Zero, 306, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 442:
						heldObject.Value = new Object(Vector2.Zero, 307, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 305:
						heldObject.Value = new Object(Vector2.Zero, 308, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 107:
						heldObject.Value = new Object(Vector2.Zero, 807, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 928:
						heldObject.Value = new Object(Vector2.Zero, 306, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Quality = 2
						};
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							heldObject.Value.Stack = 3;
							who.currentLocation.playSound("Ship");
						}
						return true;
					}
				}
				else if (name.Equals("Loom"))
				{
					float num5 = (((int)@object.quality == 0) ? 0f : (((int)@object.quality == 2) ? 0.25f : (((int)@object.quality == 4) ? 0.5f : 0.1f)));
					int num6 = ((!(Game1.random.NextDouble() <= (double)num5)) ? 1 : 2);
					int num7 = @object.ParentSheetIndex;
					if (num7 == 440)
					{
						heldObject.Value = new Object(Vector2.Zero, 428, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Stack = num6
						};
						if (!probe)
						{
							minutesUntilReady.Value = 240;
							who.currentLocation.playSound("Ship");
						}
						return true;
					}
				}
				else if (name.Equals("Oil Maker"))
				{
					switch (@object.ParentSheetIndex)
					{
					case 270:
						heldObject.Value = new Object(Vector2.Zero, 247, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 1000;
							who.currentLocation.playSound("bubbles");
							who.currentLocation.playSound("sipTea");
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 421:
						heldObject.Value = new Object(Vector2.Zero, 247, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 60;
							who.currentLocation.playSound("bubbles");
							who.currentLocation.playSound("sipTea");
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 430:
						heldObject.Value = new Object(Vector2.Zero, 432, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 360;
							who.currentLocation.playSound("bubbles");
							who.currentLocation.playSound("sipTea");
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 431:
						heldObject.Value = new Object(247, 1);
						if (!probe)
						{
							minutesUntilReady.Value = 3200;
							who.currentLocation.playSound("bubbles");
							who.currentLocation.playSound("sipTea");
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					}
				}
				else if (name.Equals("Seed Maker"))
				{
					if (@object != null && (int)@object.parentSheetIndex == 433)
					{
						return false;
					}
					if (@object != null && (int)@object.parentSheetIndex == 771)
					{
						return false;
					}
					Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Crops");
					bool flag = false;
					int num8 = -1;
					foreach (KeyValuePair<int, string> item in dictionary)
					{
						if (Convert.ToInt32(item.Value.Split('/')[3]) == @object.ParentSheetIndex)
						{
							flag = true;
							num8 = item.Key;
							break;
						}
					}
					if (flag)
					{
						Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)tileLocation.X + (int)tileLocation.Y * 77 + Game1.timeOfDay);
						heldObject.Value = new Object(num8, random.Next(1, 4));
						if (!probe)
						{
							if (random.NextDouble() < 0.005)
							{
								heldObject.Value = new Object(499, 1);
							}
							else if (random.NextDouble() < 0.02)
							{
								heldObject.Value = new Object(770, random.Next(1, 5));
							}
							minutesUntilReady.Value = 20;
							who.currentLocation.playSound("Ship");
							DelayedAction.playSoundAfterDelay("dirtyHit", 250);
						}
						return true;
					}
				}
				else if (name.Equals("Crystalarium"))
				{
					if ((@object.Category == -2 || @object.Category == -12) && @object.ParentSheetIndex != 74 && (heldObject.Value == null || heldObject.Value.ParentSheetIndex != @object.ParentSheetIndex) && (heldObject.Value == null || (int)minutesUntilReady > 0))
					{
						heldObject.Value = (Object)@object.getOne();
						if (!probe)
						{
							who.currentLocation.playSound("select");
							minutesUntilReady.Value = getMinutesForCrystalarium(@object.ParentSheetIndex);
						}
						return true;
					}
				}
				else if (name.Equals("Recycling Machine"))
				{
					if (@object.ParentSheetIndex >= 168 && @object.ParentSheetIndex <= 172 && heldObject.Value == null)
					{
						Random random2 = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay + (int)tileLocation.X * 200 + (int)tileLocation.Y);
						switch (@object.ParentSheetIndex)
						{
						case 168:
							heldObject.Value = new Object((random2.NextDouble() < 0.3) ? 382 : ((random2.NextDouble() < 0.3) ? 380 : 390), random2.Next(1, 4));
							break;
						case 169:
							heldObject.Value = new Object((random2.NextDouble() < 0.25) ? 382 : 388, random2.Next(1, 4));
							break;
						case 170:
							heldObject.Value = new Object(338, 1);
							break;
						case 171:
							heldObject.Value = new Object(338, 1);
							break;
						case 172:
							heldObject.Value = ((random2.NextDouble() < 0.1) ? new Object(428, 1) : new Torch(Vector2.Zero, 3));
							break;
						}
						if (!probe)
						{
							who.currentLocation.playSound("trashcan");
							minutesUntilReady.Value = 60;
							Game1.stats.PiecesOfTrashRecycled++;
						}
						return true;
					}
				}
				else if (name.Equals("Furnace"))
				{
					if (who.IsLocalPlayer && GetTallyOfObject(who, 382, big_craftable: false) <= 0)
					{
						if (!probe && who.IsLocalPlayer && autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12772"));
						}
						return false;
					}
					if (heldObject.Value == null)
					{
						if ((int)@object.stack < 5 && (int)@object.parentSheetIndex != 80 && (int)@object.parentSheetIndex != 82 && (int)@object.parentSheetIndex != 330 && (int)@object.parentSheetIndex != 458)
						{
							if (!probe && who.IsLocalPlayer && autoLoadChest == null)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12777"));
							}
							return false;
						}
						int num9 = 5;
						switch (@object.ParentSheetIndex)
						{
						case 378:
							heldObject.Value = new Object(Vector2.Zero, 334, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 30;
							}
							break;
						case 380:
							heldObject.Value = new Object(Vector2.Zero, 335, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 120;
							}
							break;
						case 384:
							heldObject.Value = new Object(Vector2.Zero, 336, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 300;
							}
							break;
						case 386:
							heldObject.Value = new Object(Vector2.Zero, 337, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 480;
							}
							break;
						case 80:
							heldObject.Value = new Object(Vector2.Zero, 338, "Refined Quartz", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
							if (!probe)
							{
								minutesUntilReady.Value = 90;
								num9 = 1;
							}
							break;
						case 82:
							heldObject.Value = new Object(338, 3);
							if (!probe)
							{
								minutesUntilReady.Value = 90;
								num9 = 1;
							}
							break;
						case 458:
							heldObject.Value = new Object(277, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 10;
								num9 = 1;
							}
							break;
						case 909:
							heldObject.Value = new Object(910, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 560;
							}
							break;
						default:
							return false;
						}
						if (probe)
						{
							return true;
						}
						who.currentLocation.playSound("furnace");
						initializeLightSource(tileLocation);
						showNextIndex.Value = true;
						Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(30, tileLocation.Value * 64f + new Vector2(0f, -16f), Color.White, 4, flipped: false, 50f, 10, 64, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f)
						{
							alphaFade = 0.005f
						});
						ConsumeInventoryItem(who, 382, 1);
						@object.Stack -= num9;
						if (@object.Stack <= 0)
						{
							return true;
						}
						return false;
					}
					if (probe)
					{
						return true;
					}
				}
				else if (name.Equals("Geode Crusher"))
				{
					if (who.IsLocalPlayer && GetTallyOfObject(who, 382, big_craftable: false) <= 0)
					{
						if (!probe && who.IsLocalPlayer && autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12772"));
						}
						return false;
					}
					if (heldObject.Value == null)
					{
						if (!Utility.IsGeode(@object, disallow_special_geodes: true))
						{
							return false;
						}
						Object object4 = (Object)Utility.getTreasureFromGeode(@object);
						if (object4 == null)
						{
							return false;
						}
						heldObject.Value = object4;
						if (!probe)
						{
							Game1.stats.GeodesCracked++;
							minutesUntilReady.Value = 60;
						}
						if (probe)
						{
							return true;
						}
						showNextIndex.Value = true;
						Utility.addSmokePuff(who.currentLocation, tileLocation.Value * 64f + new Vector2(4f, -48f), 200);
						Utility.addSmokePuff(who.currentLocation, tileLocation.Value * 64f + new Vector2(-16f, -56f), 300);
						Utility.addSmokePuff(who.currentLocation, tileLocation.Value * 64f + new Vector2(16f, -52f), 400);
						Utility.addSmokePuff(who.currentLocation, tileLocation.Value * 64f + new Vector2(32f, -56f), 200);
						Utility.addSmokePuff(who.currentLocation, tileLocation.Value * 64f + new Vector2(40f, -44f), 500);
						Game1.playSound("drumkit4");
						Game1.playSound("stoneCrack");
						DelayedAction.playSoundAfterDelay("steam", 200);
						ConsumeInventoryItem(who, 382, 1);
						@object.Stack--;
						if (@object.Stack <= 0)
						{
							return true;
						}
					}
					else if (probe)
					{
						return true;
					}
				}
				else if (name.Equals("Charcoal Kiln"))
				{
					if (who.IsLocalPlayer && ((int)@object.parentSheetIndex != 388 || @object.Stack < 10))
					{
						if (!probe && who.IsLocalPlayer && autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12783"));
						}
						return false;
					}
					if (heldObject.Value == null && !probe && (int)@object.parentSheetIndex == 388 && @object.Stack >= 10)
					{
						ConsumeInventoryItem(who, @object, 10);
						who.currentLocation.playSound("openBox");
						DelayedAction.playSoundAfterDelay("fireball", 50);
						showNextIndex.Value = true;
						Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(27, tileLocation.Value * 64f + new Vector2(-16f, -128f), Color.White, 4, flipped: false, 50f, 10, 64, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f)
						{
							alphaFade = 0.005f
						});
						heldObject.Value = new Object(382, 1);
						minutesUntilReady.Value = 30;
					}
					else if (heldObject.Value == null && probe && (int)@object.parentSheetIndex == 388 && @object.Stack >= 10)
					{
						heldObject.Value = new Object();
						return true;
					}
				}
				else if (name.Equals("Slime Egg-Press"))
				{
					if (who.IsLocalPlayer && ((int)@object.parentSheetIndex != 766 || @object.Stack < 100))
					{
						if (!probe && who.IsLocalPlayer && autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12787"));
						}
						return false;
					}
					if (heldObject.Value == null && !probe && (int)@object.parentSheetIndex == 766 && @object.Stack >= 100)
					{
						ConsumeInventoryItem(who, @object, 100);
						who.currentLocation.playSound("slimeHit");
						DelayedAction.playSoundAfterDelay("bubbles", 50);
						Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -160f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Lime, 1f, 0f, 0f, 0f)
						{
							alphaFade = 0.005f
						});
						int num10 = 680;
						if (Game1.random.NextDouble() < 0.05)
						{
							num10 = 439;
						}
						else if (Game1.random.NextDouble() < 0.1)
						{
							num10 = 437;
						}
						else if (Game1.random.NextDouble() < 0.25)
						{
							num10 = 413;
						}
						heldObject.Value = new Object(num10, 1);
						minutesUntilReady.Value = 1200;
					}
					else if (heldObject.Value == null && probe && (int)@object.parentSheetIndex == 766 && @object.Stack >= 100)
					{
						heldObject.Value = new Object();
						return true;
					}
				}
				else if (name.Contains("Feed Hopper") && @object.ParentSheetIndex == 178)
				{
					if (probe)
					{
						heldObject.Value = new Object();
						return true;
					}
					if (Utility.numSilos() <= 0)
					{
						if (autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:NeedSilo"));
						}
						return false;
					}
					who.currentLocation.playSound("Ship");
					DelayedAction.playSoundAfterDelay("grassyStep", 100);
					if (@object.Stack == 0)
					{
						@object.Stack = 1;
					}
					int num11 = (Game1.getLocationFromName("Farm") as Farm).piecesOfHay;
					int num12 = (Game1.getLocationFromName("Farm") as Farm).tryToAddHay(@object.Stack);
					int num13 = (Game1.getLocationFromName("Farm") as Farm).piecesOfHay;
					if (num11 <= 0 && num13 > 0)
					{
						showNextIndex.Value = true;
					}
					else if (num13 <= 0)
					{
						showNextIndex.Value = false;
					}
					@object.Stack = num12;
					if (num12 <= 0)
					{
						return true;
					}
				}
				if (name.Contains("Table") && heldObject.Value == null && !@object.bigCraftable && !@object.Name.Contains("Table"))
				{
					heldObject.Value = (Object)@object.getOne();
					if (!probe)
					{
						who.currentLocation.playSound("woodyStep");
					}
					return true;
				}
				_ = heldObject.Value;
				return false;
			}
			return false;
		}

		public virtual void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			Object @object = heldObject.Get();
			if ((bool)readyForHarvest && @object == null)
			{
				readyForHarvest.Value = false;
			}
			LightSource lightSource = netLightSource.Get();
			if (lightSource != null && (bool)isOn && !environment.hasLightSource(lightSource.Identifier))
			{
				environment.sharedLights[lightSource.identifier] = lightSource.Clone();
			}
			if (@object != null)
			{
				if (@object.ParentSheetIndex == 913 && IsSprinkler() && @object.heldObject.Value is Chest)
				{
					Chest chest = @object.heldObject.Value as Chest;
					chest.mutex.Update(environment);
					if (Game1.activeClickableMenu == null && chest.GetMutex().IsLockHeld())
					{
						chest.GetMutex().ReleaseLock();
					}
				}
				lightSource = @object.netLightSource.Get();
				if (lightSource != null && !environment.hasLightSource(lightSource.Identifier))
				{
					environment.sharedLights[lightSource.identifier] = lightSource.Clone();
				}
			}
			if (shakeTimer > 0)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
				if (shakeTimer <= 0)
				{
					health = 10;
				}
			}
			if (parentSheetIndex.Get() == 590 && Game1.random.NextDouble() < 0.01)
			{
				shakeTimer = 100;
			}
			if (bigCraftable.Get() && name.Equals("Slime Ball", StringComparison.Ordinal))
			{
				base.ParentSheetIndex = 56 + (int)(time.TotalGameTime.TotalMilliseconds % 600.0 / 100.0);
			}
		}

		public virtual void actionOnPlayerEntry()
		{
			isTemporarilyInvisible = false;
			health = 10;
			if (name != null && name.Contains("Feed Hopper"))
			{
				showNextIndex.Value = (int)(Game1.getLocationFromName("Farm") as Farm).piecesOfHay > 0;
			}
		}

		public override bool canBeTrashed()
		{
			if ((bool)questItem)
			{
				return false;
			}
			if (!bigCraftable && (int)parentSheetIndex == 460)
			{
				return false;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 911))
			{
				return false;
			}
			return base.canBeTrashed();
		}

		public virtual bool isForage(GameLocation location)
		{
			if (base.Category != -79 && base.Category != -81 && base.Category != -80 && base.Category != -75 && !(location is Beach) && (int)parentSheetIndex != 430)
			{
				return base.Category == -23;
			}
			return true;
		}

		public virtual void initializeLightSource(Vector2 tileLocation, bool mineShaft = false)
		{
			if (name == null)
			{
				return;
			}
			int identifier = (int)(tileLocation.X * 2000f + tileLocation.Y);
			if (this is Furniture && (int)(this as Furniture).furniture_type == 14 && (bool)(this as Furniture).isOn)
			{
				lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 64f), 2.5f, new Color(0, 80, 160), identifier, LightSource.LightContext.None, 0L);
			}
			else if (this is Furniture && (int)(this as Furniture).furniture_type == 16 && (bool)(this as Furniture).isOn)
			{
				lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 64f), 1.5f, new Color(0, 80, 160), identifier, LightSource.LightContext.None, 0L);
			}
			else if ((bool)bigCraftable)
			{
				if (this is Torch && (bool)isOn)
				{
					float num = -64f;
					if (Name.Contains("Campfire"))
					{
						num = 32f;
					}
					lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + num), 2.5f, new Color(0, 80, 160), identifier, LightSource.LightContext.None, 0L);
				}
				else if ((bool)isLamp)
				{
					lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 64f), 3f, new Color(0, 40, 80), identifier, LightSource.LightContext.None, 0L);
				}
				else if ((name.Equals("Furnace") && (int)minutesUntilReady > 0) || name.Equals("Bonfire"))
				{
					lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f), 1.5f, Color.DarkCyan, identifier, LightSource.LightContext.None, 0L);
				}
				else if (name.Equals("Strange Capsule"))
				{
					lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f), 1f, Color.HotPink * 0.75f, identifier, LightSource.LightContext.None, 0L);
				}
			}
			else
			{
				if (!Utility.IsNormalObjectAtParentSheetIndex(this, parentSheetIndex) && !(this is Torch))
				{
					return;
				}
				if ((int)parentSheetIndex == 93 || (int)parentSheetIndex == 94 || (int)parentSheetIndex == 95)
				{
					Color color = Color.White;
					switch ((int)parentSheetIndex)
					{
					case 93:
						color = new Color(1, 1, 1) * 0.9f;
						break;
					case 94:
						color = Color.Yellow;
						break;
					case 95:
						color = new Color(70, 0, 150) * 0.9f;
						break;
					}
					lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 16f, tileLocation.Y * 64f + 16f), mineShaft ? 1.5f : 1.25f, color, identifier, LightSource.LightContext.None, 0L);
				}
				else if (Utility.IsNormalObjectAtParentSheetIndex(this, 746))
				{
					lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 48f), 0.5f, new Color(1, 1, 1) * 0.65f, identifier, LightSource.LightContext.None, 0L);
				}
			}
		}

		public virtual void performRemoveAction(Vector2 tileLocation, GameLocation environment)
		{
			if (lightSource != null)
			{
				environment.removeLightSource(lightSource.identifier);
				environment.removeLightSource((int)Game1.player.UniqueMultiplayerID);
			}
			if ((bool)bigCraftable)
			{
				if ((base.ParentSheetIndex == 105 || (int)parentSheetIndex == 264) && environment != null && environment.terrainFeatures != null && environment.terrainFeatures.ContainsKey(tileLocation) && environment.terrainFeatures[tileLocation] is Tree)
				{
					Tree tree = environment.terrainFeatures[tileLocation] as Tree;
					tree.tapped.Value = false;
				}
				if ((int)parentSheetIndex == 126 && (int)quality != 0)
				{
					Game1.createItemDebris(new Hat((int)quality - 1), tileLocation * 64f, (Game1.player.FacingDirection + 2) % 4);
				}
				quality.Value = 0;
			}
			if (name != null && name.Contains("Sprinkler"))
			{
				environment.removeTemporarySpritesWithID((int)tileLocation.X * 4000 + (int)tileLocation.Y);
			}
			if (name.Contains("Seasonal") && bigCraftable.Value)
			{
				base.ParentSheetIndex -= base.ParentSheetIndex % 4;
			}
			_redGreenSquareDict.Remove(tileLocation);
		}

		public virtual void dropItem(GameLocation location, Vector2 origin, Vector2 destination)
		{
			if ((type.Equals("Crafting") || Type.Equals("interactive")) && (int)fragility != 2)
			{
				location.debris.Add(new Debris(bigCraftable ? (-base.ParentSheetIndex) : base.ParentSheetIndex, origin, destination));
			}
			_redGreenSquareDict.Remove(origin);
		}

		public virtual bool isPassable()
		{
			if (isTemporarilyInvisible)
			{
				return true;
			}
			if ((bool)bigCraftable)
			{
				return false;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, parentSheetIndex))
			{
				switch ((int)parentSheetIndex)
				{
				case 93:
				case 286:
				case 287:
				case 288:
				case 293:
				case 297:
				case 328:
				case 329:
				case 331:
				case 333:
				case 401:
				case 405:
				case 407:
				case 409:
				case 411:
				case 415:
				case 590:
				case 840:
				case 841:
					return true;
				}
			}
			if (base.Category == -74 || base.Category == -19)
			{
				if (isSapling())
				{
					return false;
				}
				int num = parentSheetIndex;
				if ((uint)(num - 301) <= 1u || num == 473)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public virtual void reloadSprite()
		{
			initializeLightSource(tileLocation);
		}

		public virtual void consumeRecipe(Farmer who)
		{
			if ((bool)isRecipe)
			{
				if (base.Category == -7)
				{
					who.cookingRecipes.Add(name, 0);
				}
				else
				{
					who.craftingRecipes.Add(name, 0);
				}
			}
		}

		public virtual Microsoft.Xna.Framework.Rectangle getBoundingBox(Vector2 tileLocation)
		{
			Microsoft.Xna.Framework.Rectangle value = boundingBox.Value;
			Microsoft.Xna.Framework.Rectangle rectangle = value;
			if ((this is Torch && !bigCraftable) || (int)parentSheetIndex == 590)
			{
				rectangle.X = (int)tileLocation.X * 64 + 24;
				rectangle.Y = (int)tileLocation.Y * 64 + 24;
			}
			else
			{
				rectangle.X = (int)tileLocation.X * 64;
				rectangle.Y = (int)tileLocation.Y * 64;
			}
			if (rectangle != value)
			{
				boundingBox.Set(value);
			}
			return rectangle;
		}

		public override bool canBeGivenAsGift()
		{
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 911))
			{
				return false;
			}
			if (!bigCraftable && !(this is Furniture))
			{
				return !(this is Wallpaper);
			}
			return false;
		}

		public virtual bool performDropDownAction(Farmer who)
		{
			if (who == null)
			{
				who = Game1.getFarmer(owner);
			}
			if (name.Equals("Worm Bin"))
			{
				if (heldObject.Value == null)
				{
					heldObject.Value = new Object(685, Game1.random.Next(2, 6));
					minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
				}
				return false;
			}
			if (name.Equals("Bee House"))
			{
				if (heldObject.Value == null)
				{
					heldObject.Value = new Object(Vector2.Zero, 340, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
					minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 4);
				}
				return false;
			}
			if (name.Equals("Solar Panel"))
			{
				if (heldObject.Value == null)
				{
					heldObject.Value = new Object(Vector2.Zero, 787, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
					minutesUntilReady.Value = 16800;
				}
				return false;
			}
			if (name.Contains("Strange Capsule"))
			{
				minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 3);
			}
			else if (name.Contains("Feed Hopper"))
			{
				showNextIndex.Value = false;
				if ((int)(Game1.getLocationFromName("Farm") as Farm).piecesOfHay >= 0)
				{
					showNextIndex.Value = true;
				}
			}
			return false;
		}

		private void totemWarp(Farmer who)
		{
			for (int i = 0; i < 12; i++)
			{
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, Game1.random.NextDouble() < 0.5));
			}
			who.currentLocation.playSound("wand");
			Game1.displayFarmer = false;
			Game1.player.temporarilyInvincible = true;
			Game1.player.temporaryInvincibilityTimer = -2000;
			Game1.player.freezePause = 1000;
			Game1.flashAlpha = 1f;
			DelayedAction.fadeAfterDelay(totemWarpForReal, 1000);
			new Microsoft.Xna.Framework.Rectangle(who.GetBoundingBox().X, who.GetBoundingBox().Y, 64, 64).Inflate(192, 192);
			int num = 0;
			for (int num2 = who.getTileX() + 8; num2 >= who.getTileX() - 8; num2--)
			{
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(6, new Vector2(num2, who.getTileY()) * 64f, Color.White, 8, flipped: false, 50f)
				{
					layerDepth = 1f,
					delayBeforeAnimationStart = num * 25,
					motion = new Vector2(-0.25f, 0f)
				});
				num++;
			}
		}

		private void totemWarpForReal()
		{
			switch ((int)parentSheetIndex)
			{
			case 688:
			{
				int default_x = 48;
				int default_y = 7;
				if (Game1.whichFarm == 5)
				{
					default_x = 48;
					default_y = 39;
				}
				else if (Game1.whichFarm == 6)
				{
					default_x = 82;
					default_y = 29;
				}
				Point mapPropertyPosition = Game1.getFarm().GetMapPropertyPosition("WarpTotemEntry", default_x, default_y);
				Game1.warpFarmer("Farm", mapPropertyPosition.X, mapPropertyPosition.Y, flip: false);
				break;
			}
			case 689:
				Game1.warpFarmer("Mountain", 31, 20, flip: false);
				break;
			case 690:
				Game1.warpFarmer("Beach", 20, 4, flip: false);
				break;
			case 261:
				Game1.warpFarmer("Desert", 35, 43, flip: false);
				break;
			case 886:
				Game1.warpFarmer("IslandSouth", 11, 11, flip: false);
				break;
			}
			Game1.fadeToBlackAlpha = 0.99f;
			Game1.screenGlow = false;
			Game1.player.temporarilyInvincible = false;
			Game1.player.temporaryInvincibilityTimer = 0;
			Game1.displayFarmer = true;
		}

		public void MonsterMusk(Farmer who)
		{
			who.FarmerSprite.PauseForSingleAnimation = false;
			who.FarmerSprite.StopAnimation();
			who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[4]
			{
				new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false)
			});
			who.currentLocation.playSound("croak");
			Game1.buffsDisplay.addOtherBuff(new Buff(24));
		}

		public override string[] ModifyItemBuffs(string[] buffs)
		{
			if (buffs != null && base.Category == -7)
			{
				int num = 0;
				if (Quality != 0)
				{
					num = 1;
				}
				if (num > 0)
				{
					int result = 0;
					for (int i = 0; i < buffs.Length; i++)
					{
						if (i != 9 && buffs[i] != "0" && int.TryParse(buffs[i], out result))
						{
							result += num;
							buffs[i] = result.ToString();
						}
					}
				}
			}
			return base.ModifyItemBuffs(buffs);
		}

		private void rainTotem(Farmer who)
		{
			GameLocation.LocationContext locationContext = Game1.currentLocation.GetLocationContext();
			if (locationContext == GameLocation.LocationContext.Default)
			{
				if (!Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
				{
					Game1.netWorldState.Value.WeatherForTomorrow = (Game1.weatherForTomorrow = 1);
					Game1.pauseThenMessage(2000, Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822"), showProgressBar: false);
				}
			}
			else
			{
				Game1.netWorldState.Value.GetWeatherForLocation(locationContext).weatherForTomorrow.Value = 1;
				Game1.pauseThenMessage(2000, Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822"), showProgressBar: false);
			}
			Game1.screenGlow = false;
			who.currentLocation.playSound("thunder");
			who.canMove = false;
			Game1.screenGlowOnce(Color.SlateBlue, hold: false);
			Game1.player.faceDirection(2);
			Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
			{
				new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
			});
			for (int i = 0; i < 6; i++)
			{
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 2f, 0.01f, 0f, 0f)
				{
					motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -2f),
					delayBeforeAnimationStart = i * 200
				});
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
				{
					motion = new Vector2((float)Game1.random.Next(-30, -10) / 10f, -1f),
					delayBeforeAnimationStart = 100 + i * 200
				});
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
				{
					motion = new Vector2((float)Game1.random.Next(10, 30) / 10f, -1f),
					delayBeforeAnimationStart = 200 + i * 200
				});
			}
			Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(parentSheetIndex, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
			{
				motion = new Vector2(0f, -7f),
				acceleration = new Vector2(0f, 0.1f),
				scaleChange = 0.015f,
				alpha = 1f,
				alphaFade = 0.0075f,
				shakeIntensity = 1f,
				initialPosition = Game1.player.Position + new Vector2(0f, -96f),
				xPeriodic = true,
				xPeriodicLoopTime = 1000f,
				xPeriodicRange = 4f,
				layerDepth = 1f
			});
			DelayedAction.playSoundAfterDelay("rainsound", 2000);
		}

		public virtual bool performUseAction(GameLocation location)
		{
			if (!Game1.player.canMove || isTemporarilyInvisible)
			{
				return false;
			}
			bool flag = !Game1.eventUp && !Game1.isFestival() && !Game1.fadeToBlack && !Game1.player.swimming && !Game1.player.bathingClothes && !Game1.player.onBridge.Value;
			if (name != null && name.Contains("Totem"))
			{
				if (flag)
				{
					switch ((int)parentSheetIndex)
					{
					case 681:
						rainTotem(Game1.player);
						return true;
					case 261:
					case 688:
					case 689:
					case 690:
					case 886:
					{
						Game1.player.jitterStrength = 1f;
						Color glowColor = (((int)parentSheetIndex == 681) ? Color.SlateBlue : (((int)parentSheetIndex == 688) ? Color.LimeGreen : (((int)parentSheetIndex == 689) ? Color.OrangeRed : (((int)parentSheetIndex == 261) ? new Color(255, 200, 0) : Color.LightBlue))));
						location.playSound("warrior");
						Game1.player.faceDirection(2);
						Game1.player.CanMove = false;
						Game1.player.temporarilyInvincible = true;
						Game1.player.temporaryInvincibilityTimer = -4000;
						Game1.changeMusicTrack("none");
						if ((int)parentSheetIndex == 681)
						{
							Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
							{
								new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false),
								new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, rainTotem, behaviorAtEndOfFrame: true)
							});
						}
						else
						{
							Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
							{
								new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false),
								new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, totemWarp, behaviorAtEndOfFrame: true)
							});
						}
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(parentSheetIndex, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
						{
							motion = new Vector2(0f, -1f),
							scaleChange = 0.01f,
							alpha = 1f,
							alphaFade = 0.0075f,
							shakeIntensity = 1f,
							initialPosition = Game1.player.Position + new Vector2(0f, -96f),
							xPeriodic = true,
							xPeriodicLoopTime = 1000f,
							xPeriodicRange = 4f,
							layerDepth = 1f
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(parentSheetIndex, 9999f, 1, 999, Game1.player.Position + new Vector2(-64f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
						{
							motion = new Vector2(0f, -0.5f),
							scaleChange = 0.005f,
							scale = 0.5f,
							alpha = 1f,
							alphaFade = 0.0075f,
							shakeIntensity = 1f,
							delayBeforeAnimationStart = 10,
							initialPosition = Game1.player.Position + new Vector2(-64f, -96f),
							xPeriodic = true,
							xPeriodicLoopTime = 1000f,
							xPeriodicRange = 4f,
							layerDepth = 0.9999f
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(parentSheetIndex, 9999f, 1, 999, Game1.player.Position + new Vector2(64f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
						{
							motion = new Vector2(0f, -0.5f),
							scaleChange = 0.005f,
							scale = 0.5f,
							alpha = 1f,
							alphaFade = 0.0075f,
							delayBeforeAnimationStart = 20,
							shakeIntensity = 1f,
							initialPosition = Game1.player.Position + new Vector2(64f, -96f),
							xPeriodic = true,
							xPeriodicLoopTime = 1000f,
							xPeriodicRange = 4f,
							layerDepth = 0.9988f
						});
						Game1.screenGlowOnce(glowColor, hold: false);
						Utility.addSprinklesToLocation(location, Game1.player.getTileX(), Game1.player.getTileY(), 16, 16, 1300, 20, Color.White, null, motionTowardCenter: true);
						return true;
					}
					}
				}
			}
			else
			{
				if (name != null && name.Contains("Secret Note"))
				{
					int num = ((!name.Contains('#')) ? 1 : Convert.ToInt32(name.Split('#')[1]));
					if (!Game1.player.secretNotesSeen.Contains(num))
					{
						Game1.player.secretNotesSeen.Add(num);
						if (num == 23 && !Game1.player.eventsSeen.Contains(2120303))
						{
							Game1.player.addQuest(29);
						}
						else if (num == 10 && !Game1.player.mailReceived.Contains("qiCave"))
						{
							Game1.player.addQuest(30);
						}
					}
					Game1.activeClickableMenu = new LetterViewerMenu(num);
					return true;
				}
				if (name != null && name.Contains("Journal Scrap"))
				{
					int num2 = ((!name.Contains('#')) ? 1 : Convert.ToInt32(name.Split('#')[1]));
					num2 += GameLocation.JOURNAL_INDEX;
					if (!Game1.player.secretNotesSeen.Contains(num2))
					{
						Game1.player.secretNotesSeen.Add(num2);
					}
					Game1.activeClickableMenu = new LetterViewerMenu(num2);
					return true;
				}
			}
			if (base.ParentSheetIndex == 911)
			{
				if (!flag)
				{
					return false;
				}
				switch (Utility.GetHorseWarpRestrictionsForFarmer(Game1.player).FirstOrDefault())
				{
				case 1:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_NoHorse"));
					break;
				case 2:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_InvalidLocation"));
					break;
				case 3:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_NoClearance"));
					break;
				case 4:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_InUse"));
					break;
				case 0:
				{
					Horse horse = null;
					foreach (NPC character in Game1.player.currentLocation.characters)
					{
						if (character is Horse)
						{
							Horse horse2 = character as Horse;
							if (horse2.getOwner() == Game1.player)
							{
								horse = horse2;
								break;
							}
						}
					}
					if (horse == null || Math.Abs(Game1.player.getTileX() - horse.getTileX()) > 1 || Math.Abs(Game1.player.getTileY() - horse.getTileY()) > 1)
					{
						Game1.player.faceDirection(2);
						Game1.soundBank.PlayCue("horse_flute");
						Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[6]
						{
							new FarmerSprite.AnimationFrame(98, 400, secondaryArm: true, flip: false),
							new FarmerSprite.AnimationFrame(99, 200, secondaryArm: true, flip: false),
							new FarmerSprite.AnimationFrame(100, 200, secondaryArm: true, flip: false),
							new FarmerSprite.AnimationFrame(99, 200, secondaryArm: true, flip: false),
							new FarmerSprite.AnimationFrame(98, 400, secondaryArm: true, flip: false),
							new FarmerSprite.AnimationFrame(99, 200, secondaryArm: true, flip: false)
						});
						Game1.player.freezePause = 1500;
						DelayedAction.functionAfterDelay(delegate
						{
							switch (Utility.GetHorseWarpRestrictionsForFarmer(Game1.player).FirstOrDefault())
							{
							case 0:
								Game1.player.team.requestHorseWarpEvent.Fire(Game1.player.UniqueMultiplayerID);
								break;
							case 1:
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_NoHorse"));
								break;
							case 2:
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_InvalidLocation"));
								break;
							case 3:
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_NoClearance"));
								break;
							}
						}, 1500);
					}
					stack.Value += 1;
					return true;
				}
				default:
					stack.Value += 1;
					return true;
				}
			}
			if (base.ParentSheetIndex == 879)
			{
				if (!flag)
				{
					return false;
				}
				Game1.player.faceDirection(2);
				Game1.player.freezePause = 1750;
				Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
				{
					new FarmerSprite.AnimationFrame(57, 750, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, MonsterMusk, behaviorAtEndOfFrame: true)
				});
				for (int i = 0; i < 3; i++)
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(16f, -64 + 32 * i), Color.Purple)
					{
						motion = new Vector2(Utility.RandomFloat(-1f, 1f), -0.5f),
						scaleChange = 0.005f,
						scale = 0.5f,
						alpha = 1f,
						alphaFade = 0.0075f,
						shakeIntensity = 1f,
						delayBeforeAnimationStart = 100 * i,
						layerDepth = 0.9999f,
						positionFollowsAttachedCharacter = true,
						attachedCharacter = Game1.player
					});
				}
				location.playSound("steam");
				return true;
			}
			_ = name;
			return false;
		}

		public override Color getCategoryColor()
		{
			if (this is Furniture)
			{
				return new Color(100, 25, 190);
			}
			if (type != null && type.Equals("Arch"))
			{
				return new Color(110, 0, 90);
			}
			switch (base.Category)
			{
			case -12:
			case -2:
				return new Color(110, 0, 90);
			case -75:
				return Color.Green;
			case -4:
				return Color.DarkBlue;
			case -7:
				return new Color(220, 60, 0);
			case -79:
				return Color.DeepPink;
			case -74:
				return Color.Brown;
			case -19:
				return Color.SlateGray;
			case -21:
				return Color.DarkRed;
			case -22:
				return Color.DarkCyan;
			case -24:
				return Color.Plum;
			case -20:
				return Color.DarkGray;
			case -27:
			case -26:
				return new Color(0, 155, 111);
			case -8:
				return new Color(148, 61, 40);
			case -18:
			case -14:
			case -6:
			case -5:
				return new Color(255, 0, 100);
			case -80:
				return new Color(219, 54, 211);
			case -28:
				return new Color(50, 10, 70);
			case -16:
			case -15:
				return new Color(64, 102, 114);
			case -81:
				return new Color(10, 130, 50);
			default:
				return Color.Black;
			}
		}

		public override string getCategoryName()
		{
			if (this is Furniture)
			{
				if ((this as Furniture).placementRestriction == 1)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Outdoors");
				}
				if ((this as Furniture).placementRestriction == 2)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Decoration");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12847");
			}
			if (type.Value != null && type.Value.Equals("Arch"))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12849");
			}
			switch (base.Category)
			{
			case -12:
			case -2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12850");
			case -75:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12851");
			case -4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12852");
			case -25:
			case -7:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12853");
			case -79:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12854");
			case -74:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12855");
			case -19:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12856");
			case -21:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12857");
			case -22:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12858");
			case -24:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12859");
			case -20:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12860");
			case -27:
			case -26:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12862");
			case -8:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12863");
			case -18:
			case -14:
			case -6:
			case -5:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12864");
			case -80:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12866");
			case -28:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12867");
			case -16:
			case -15:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12868");
			case -81:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12869");
			default:
				return "";
			}
		}

		public virtual bool isActionable(Farmer who)
		{
			if (!isTemporarilyInvisible)
			{
				return checkForAction(who, justCheckingForActivity: true);
			}
			return false;
		}

		public int getHealth()
		{
			return health;
		}

		public void setHealth(int health)
		{
			this.health = health;
		}

		protected virtual void grabItemFromAutoGrabber(Item item, Farmer who)
		{
			if (heldObject.Value != null && heldObject.Value is Chest)
			{
				if (who.couldInventoryAcceptThisItem(item))
				{
					Game1.activeClickableMenu = new ItemGrabMenu((heldObject.Value as Chest).items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, (heldObject.Value as Chest).grabItemFromInventory, null, grabItemFromAutoGrabber, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, this, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, this);
				}
				if ((heldObject.Value as Chest).isEmpty())
				{
					showNextIndex.Value = false;
				}
				if ((heldObject.Value as Chest).itemsCountExcludingNulls() == 1)
				{
					showNextIndex.Value = false;
				}
			}
		}

		public static bool HighlightFertilizers(Item i)
		{
			if (i is Object)
			{
				return (int)(i as Object).category == -19;
			}
			return false;
		}

		private void AttachToSprinklerAttachment(Item i, Farmer who)
		{
			if (i != null && i is Object && IsSprinkler() && heldObject.Value != null)
			{
				who.removeItemFromInventory(i);
				heldObject.Value.heldObject.Value = i as Object;
				if (Game1.player.ActiveObject == null)
				{
					Game1.player.showNotCarrying();
					Game1.player.Halt();
				}
			}
		}

		public override int healthRecoveredOnConsumption()
		{
			if (Edibility < 0)
			{
				return 0;
			}
			if (base.ParentSheetIndex == 874)
			{
				return (int)((float)staminaRecoveredOnConsumption() * 0.68f);
			}
			return (int)((float)staminaRecoveredOnConsumption() * 0.45f);
		}

		public override int staminaRecoveredOnConsumption()
		{
			return (int)Math.Ceiling((double)Edibility * 2.5) + Quality * Edibility;
		}

		public void PlaySingingStone()
		{
			if (Game1.soundBank != null)
			{
				ICue cue = Game1.soundBank.GetCue("crystal");
				int num = Game1.random.Next(2400);
				num -= num % 100;
				cue.SetVariable("Pitch", num);
				shakeTimer = 100;
				cue.Play();
			}
		}

		public virtual bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (isTemporarilyInvisible)
			{
				return true;
			}
			if (!justCheckingForActivity && who != null && who.currentLocation.isObjectAtTile(who.getTileX(), who.getTileY() - 1) && who.currentLocation.isObjectAtTile(who.getTileX(), who.getTileY() + 1) && who.currentLocation.isObjectAtTile(who.getTileX() + 1, who.getTileY()) && who.currentLocation.isObjectAtTile(who.getTileX() - 1, who.getTileY()) && !who.currentLocation.getObjectAtTile(who.getTileX(), who.getTileY() - 1).isPassable() && !who.currentLocation.getObjectAtTile(who.getTileX(), who.getTileY() + 1).isPassable() && !who.currentLocation.getObjectAtTile(who.getTileX() - 1, who.getTileY()).isPassable() && !who.currentLocation.getObjectAtTile(who.getTileX() + 1, who.getTileY()).isPassable())
			{
				performToolAction(null, who.currentLocation);
			}
			if ((bool)bigCraftable)
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				switch ((int)parentSheetIndex)
				{
				case 231:
					if (readyForHarvest.Value && who.IsLocalPlayer)
					{
						Object value = heldObject.Value;
						heldObject.Value = null;
						if (!who.addItemToInventoryBool(value))
						{
							heldObject.Value = value;
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
							return false;
						}
						heldObject.Value = new Object(Vector2.Zero, 787, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						minutesUntilReady.Value = 16800;
						Game1.playSound("coin");
						readyForHarvest.Value = false;
						return true;
					}
					break;
				case 247:
					Game1.activeClickableMenu = new TailoringMenu();
					return true;
				case 165:
					if (heldObject.Value != null && heldObject.Value is Chest && !(heldObject.Value as Chest).isEmpty())
					{
						if (justCheckingForActivity)
						{
							return true;
						}
						(heldObject.Value as Chest).createSlotsForCapacity(force: true);
						Game1.activeClickableMenu = new ItemGrabMenu((heldObject.Value as Chest).items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, (heldObject.Value as Chest).grabItemFromInventory, null, grabItemFromAutoGrabber, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, this);
						return true;
					}
					break;
				case 239:
					shakeTimer = 500;
					who.currentLocation.localSound("DwarvishSentry");
					who.freezePause = 500;
					DelayedAction.functionAfterDelay(delegate
					{
						int totalCrops = Game1.getFarm().getTotalCrops();
						int totalOpenHoeDirt = Game1.getFarm().getTotalOpenHoeDirt();
						int totalCropsReadyForHarvest = Game1.getFarm().getTotalCropsReadyForHarvest();
						int totalUnwateredCrops = Game1.getFarm().getTotalUnwateredCrops();
						int totalGreenhouseCropsReadyForHarvest = Game1.getFarm().getTotalGreenhouseCropsReadyForHarvest();
						int totalForageItems = Game1.getFarm().getTotalForageItems();
						int numberOfMachinesReadyForHarvest = Game1.getFarm().getNumberOfMachinesReadyForHarvest();
						bool flag3 = Game1.getFarm().doesFarmCaveNeedHarvesting();
						Game1.multipleDialogues(new string[1] { Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_Intro", Game1.player.farmName.Value) + "^--------------^" + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_PiecesHay", (Game1.getLocationFromName("Farm") as Farm).piecesOfHay, Utility.numSilos() * 240) + "  ^" + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalCrops", totalCrops) + "  ^" + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsReadyForHarvest", totalCropsReadyForHarvest) + "  ^" + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsUnwatered", totalUnwateredCrops) + "  ^" + ((totalGreenhouseCropsReadyForHarvest != -1) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsReadyForHarvest_Greenhouse", totalGreenhouseCropsReadyForHarvest) + "  ^") : "") + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalOpenHoeDirt", totalOpenHoeDirt) + "  ^" + (Game1.getFarm().SpawnsForage() ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalForage", totalForageItems) + "  ^") : "") + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_MachinesReady", numberOfMachinesReadyForHarvest) + "  ^" + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_FarmCave", flag3 ? Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes") : Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "  " });
					}, 500);
					return true;
				case 238:
				{
					if (justCheckingForActivity)
					{
						return true;
					}
					Vector2 vector = Vector2.Zero;
					Vector2 vector2 = Vector2.Zero;
					foreach (KeyValuePair<Vector2, Object> pair in who.currentLocation.objects.Pairs)
					{
						if ((bool)pair.Value.bigCraftable && pair.Value.ParentSheetIndex == 238)
						{
							if (vector.Equals(Vector2.Zero))
							{
								vector = pair.Key;
							}
							else if (vector2.Equals(Vector2.Zero))
							{
								vector2 = pair.Key;
								break;
							}
						}
					}
					if (vector2.Equals(Vector2.Zero))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MiniObelisk_NeedsPair"));
						return false;
					}
					Vector2 vector3 = ((!(Vector2.Distance(who.getTileLocation(), vector) > Vector2.Distance(who.getTileLocation(), vector2))) ? vector2 : vector);
					List<Vector2> list = new List<Vector2>
					{
						new Vector2(vector3.X, vector3.Y + 1f),
						new Vector2(vector3.X - 1f, vector3.Y),
						new Vector2(vector3.X + 1f, vector3.Y),
						new Vector2(vector3.X, vector3.Y - 1f)
					};
					foreach (Vector2 v in list)
					{
						if (who.currentLocation.isTileLocationTotallyClearAndPlaceableIgnoreFloors(v))
						{
							for (int i = 0; i < 12; i++)
							{
								who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, Game1.random.NextDouble() < 0.5));
							}
							who.currentLocation.playSound("wand");
							Game1.displayFarmer = false;
							Game1.player.freezePause = 800;
							Game1.flashAlpha = 1f;
							DelayedAction.fadeAfterDelay(delegate
							{
								who.setTileLocation(v);
								Game1.displayFarmer = true;
								Game1.globalFadeToClear();
							}, 800);
							new Microsoft.Xna.Framework.Rectangle(who.GetBoundingBox().X, who.GetBoundingBox().Y, 64, 64).Inflate(192, 192);
							int num = 0;
							for (int num2 = who.getTileX() + 8; num2 >= who.getTileX() - 8; num2--)
							{
								who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(num2, who.getTileY()) * 64f, Color.White, 8, flipped: false, 50f)
								{
									layerDepth = 1f,
									delayBeforeAnimationStart = num * 25,
									motion = new Vector2(-0.25f, 0f)
								});
								num++;
							}
							return true;
						}
					}
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MiniObelisk_NeedsSpace"));
					return false;
				}
				}
			}
			if (name.Equals("Prairie King Arcade System"))
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				Game1.currentLocation.showPrairieKingMenu();
				return true;
			}
			if (name.Equals("Junimo Kart Arcade System"))
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				Response[] answerChoices = new Response[3]
				{
					new Response("Progress", Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12873")),
					new Response("Endless", Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12875")),
					new Response("Exit", Game1.content.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11738"))
				};
				who.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Menu"), answerChoices, "MinecartGame");
				return true;
			}
			if (name.Equals("Staircase"))
			{
				if (who.currentLocation is MineShaft && (who.currentLocation as MineShaft).shouldCreateLadderOnThisLevel())
				{
					if (justCheckingForActivity)
					{
						return true;
					}
					Game1.enterMine(Game1.CurrentMineLevel + 1);
					Game1.playSound("stairsdown");
				}
			}
			else
			{
				if (name.Equals("Slime Ball"))
				{
					if (justCheckingForActivity)
					{
						return true;
					}
					who.currentLocation.objects.Remove(tileLocation);
					DelayedAction.playSoundAfterDelay("slimedead", 40);
					DelayedAction.playSoundAfterDelay("slimeHit", 100);
					who.currentLocation.playSound("slimeHit");
					Random random = new Random((int)Game1.stats.daysPlayed + (int)Game1.uniqueIDForThisGame + (int)tileLocation.X * 77 + (int)tileLocation.Y * 777 + 2);
					Game1.createMultipleObjectDebris(766, (int)tileLocation.X, (int)tileLocation.Y, random.Next(10, 21), 1f + ((who.FacingDirection == 2) ? 0f : ((float)Game1.random.NextDouble())));
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, tileLocation.Value * 64f, Color.Lime, 10)
					{
						interval = 70f,
						holdLastFrame = true,
						alphaFade = 0.01f
					}, who.currentLocation);
					Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(44, tileLocation.Value * 64f + new Vector2(-16f, 0f), Color.Lime, 10)
					{
						interval = 70f,
						delayBeforeAnimationStart = 0,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
					Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(44, tileLocation.Value * 64f + new Vector2(0f, 16f), Color.Lime, 10)
					{
						interval = 70f,
						delayBeforeAnimationStart = 100,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
					Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(44, tileLocation.Value * 64f + new Vector2(16f, 0f), Color.Lime, 10)
					{
						interval = 70f,
						delayBeforeAnimationStart = 200,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
					while (random.NextDouble() < 0.33)
					{
						Game1.createObjectDebris(557, (int)tileLocation.X, (int)tileLocation.Y, who.UniqueMultiplayerID);
					}
					return true;
				}
				if (name.Equals("Furnace") && who.ActiveObject == null && !readyForHarvest)
				{
					if (heldObject.Value != null)
					{
						return true;
					}
				}
				else
				{
					if (name.Contains("Table"))
					{
						if (heldObject.Value != null)
						{
							if (justCheckingForActivity)
							{
								return true;
							}
							Object value2 = heldObject.Value;
							heldObject.Value = null;
							if (who.isMoving())
							{
								Game1.haltAfterCheck = false;
							}
							if (who.addItemToInventoryBool(value2))
							{
								Game1.playSound("coin");
							}
							else
							{
								heldObject.Value = value2;
							}
							return true;
						}
						if (name.Equals("Tile Table"))
						{
							if (justCheckingForActivity)
							{
								return true;
							}
							base.ParentSheetIndex++;
							if ((int)parentSheetIndex == 322)
							{
								base.ParentSheetIndex -= 9;
								return false;
							}
							return true;
						}
						return false;
					}
					if (name.Contains("Stool"))
					{
						if (justCheckingForActivity)
						{
							return true;
						}
						base.ParentSheetIndex++;
						if ((int)parentSheetIndex == 305)
						{
							base.ParentSheetIndex -= 9;
							return false;
						}
						return true;
					}
					if ((bool)bigCraftable && (name.Contains("Chair") || name.Contains("Painting") || name.Equals("House Plant")))
					{
						if (justCheckingForActivity)
						{
							return true;
						}
						base.ParentSheetIndex++;
						int num3 = -1;
						int num4 = -1;
						switch (name)
						{
						case "Red Chair":
							num3 = 4;
							num4 = 44;
							break;
						case "Patio Chair":
							num3 = 4;
							num4 = 52;
							break;
						case "Dark Chair":
							num3 = 4;
							num4 = 60;
							break;
						case "Wood Chair":
							num3 = 4;
							num4 = 24;
							break;
						case "House Plant":
							num3 = 8;
							num4 = 0;
							break;
						case "Painting":
							num3 = 8;
							num4 = 32;
							break;
						}
						if ((int)parentSheetIndex == num4 + num3)
						{
							base.ParentSheetIndex -= num3;
							return false;
						}
						return true;
					}
					if (name.Equals("Flute Block"))
					{
						if (justCheckingForActivity)
						{
							return true;
						}
						preservedParentSheetIndex.Value = (preservedParentSheetIndex.Value + 100) % 2400;
						shakeTimer = 200;
						if (Game1.soundBank != null)
						{
							if (internalSound != null)
							{
								internalSound.Stop(AudioStopOptions.Immediate);
								internalSound = Game1.soundBank.GetCue("flute");
							}
							else
							{
								internalSound = Game1.soundBank.GetCue("flute");
							}
							internalSound.SetVariable("Pitch", preservedParentSheetIndex.Value);
							internalSound.Play();
						}
						scale.Y = 1.3f;
						shakeTimer = 200;
						return true;
					}
					if (name.Equals("Drum Block"))
					{
						if (justCheckingForActivity)
						{
							return true;
						}
						preservedParentSheetIndex.Value = (preservedParentSheetIndex.Value + 1) % 7;
						shakeTimer = 200;
						if (Game1.soundBank != null)
						{
							if (internalSound != null)
							{
								internalSound.Stop(AudioStopOptions.Immediate);
								internalSound = Game1.soundBank.GetCue("drumkit" + preservedParentSheetIndex.Value);
							}
							else
							{
								internalSound = Game1.soundBank.GetCue("drumkit" + preservedParentSheetIndex.Value);
							}
							internalSound.Play();
						}
						scale.Y = 1.3f;
						shakeTimer = 200;
						return true;
					}
					if (IsSprinkler())
					{
						if (heldObject.Value != null && heldObject.Value.ParentSheetIndex == 913)
						{
							if (justCheckingForActivity)
							{
								return true;
							}
							if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
							{
								return false;
							}
							if (heldObject.Value.heldObject.Value is Chest)
							{
								Chest chest = heldObject.Value.heldObject.Value as Chest;
								chest.GetMutex().RequestLock(delegate
								{
									chest.ShowMenu();
								});
							}
						}
					}
					else
					{
						if (IsScarecrow())
						{
							if (justCheckingForActivity)
							{
								return true;
							}
							if ((int)parentSheetIndex == 126 && who.CurrentItem != null && who.CurrentItem is Hat)
							{
								shakeTimer = 100;
								if ((int)quality != 0)
								{
									Game1.createItemDebris(new Hat((int)quality - 1), tileLocation.Value * 64f, (who.FacingDirection + 2) % 4);
								}
								quality.Value = (int)(who.CurrentItem as Hat).which + 1;
								who.items[who.CurrentToolIndex] = null;
								who.currentLocation.playSound("dirtyHit");
								return true;
							}
							if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
							{
								return false;
							}
							shakeTimer = 100;
							if (base.SpecialVariable == 0)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12926"));
							}
							else
							{
								Game1.drawObjectDialogue((base.SpecialVariable == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12927") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12929", base.SpecialVariable));
							}
							return true;
						}
						if (name.Equals("Singing Stone"))
						{
							if (justCheckingForActivity)
							{
								return true;
							}
							PlaySingingStone();
							return true;
						}
						if (name.Contains("Feed Hopper") && who.ActiveObject == null)
						{
							if (justCheckingForActivity)
							{
								return true;
							}
							if (who.freeSpotsInInventory() > 0)
							{
								int num5 = (Game1.getLocationFromName("Farm") as Farm).piecesOfHay;
								if (num5 > 0)
								{
									bool flag = false;
									if (who.currentLocation is AnimalHouse)
									{
										int count = (who.currentLocation as AnimalHouse).animalsThatLiveHere.Count;
										int val = Math.Min(count, num5);
										val = Math.Max(1, val);
										AnimalHouse animalHouse = who.currentLocation as AnimalHouse;
										int num6 = animalHouse.numberOfObjectsWithName("Hay");
										val = Math.Min(val, (int)animalHouse.animalLimit - num6);
										if (val != 0 && Game1.player.couldInventoryAcceptThisObject(178, val))
										{
											(Game1.getLocationFromName("Farm") as Farm).piecesOfHay.Value -= Math.Max(1, val);
											who.addItemToInventoryBool(new Object(178, val));
											Game1.playSound("shwip");
											flag = true;
										}
									}
									else if (Game1.player.couldInventoryAcceptThisObject(178, 1))
									{
										(Game1.getLocationFromName("Farm") as Farm).piecesOfHay.Value--;
										who.addItemToInventoryBool(new Object(178, 1));
										Game1.playSound("shwip");
									}
									if ((int)(Game1.getLocationFromName("Farm") as Farm).piecesOfHay <= 0)
									{
										showNextIndex.Value = false;
									}
									if (flag)
									{
										return true;
									}
								}
								else
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12942"));
								}
							}
							else
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
							}
						}
					}
				}
			}
			Object value3 = heldObject.Value;
			if ((bool)readyForHarvest)
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				if (who.isMoving())
				{
					Game1.haltAfterCheck = false;
				}
				bool flag2 = false;
				if (name.Equals("Bee House"))
				{
					int value4 = -1;
					string text = "Wild";
					int num7 = 0;
					Crop crop2 = Utility.findCloseFlower(who.currentLocation, tileLocation, 5, (Crop crop) => (!crop.forageCrop.Value) ? true : false);
					if (crop2 != null)
					{
						text = Game1.objectInformation[crop2.indexOfHarvest].Split('/')[0];
						value4 = crop2.indexOfHarvest.Value;
						num7 = Convert.ToInt32(Game1.objectInformation[crop2.indexOfHarvest].Split('/')[1]) * 2;
					}
					if (heldObject.Value != null)
					{
						heldObject.Value.name = text + " Honey";
						heldObject.Value.displayName = loadDisplayName();
						heldObject.Value.Price = Convert.ToInt32(Game1.objectInformation[340].Split('/')[1]) + num7;
						heldObject.Value.preservedParentSheetIndex.Value = value4;
						if (Game1.GetSeasonForLocation(Game1.currentLocation).Equals("winter"))
						{
							heldObject.Value = null;
							readyForHarvest.Value = false;
							showNextIndex.Value = false;
							return false;
						}
						if (who.IsLocalPlayer)
						{
							Object value5 = heldObject.Value;
							heldObject.Value = null;
							if (!who.addItemToInventoryBool(value5))
							{
								heldObject.Value = value5;
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
								return false;
							}
						}
						Game1.playSound("coin");
						flag2 = true;
					}
				}
				else if (who.IsLocalPlayer)
				{
					heldObject.Value = null;
					if (!who.addItemToInventoryBool(value3))
					{
						heldObject.Value = value3;
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
						return false;
					}
					Game1.playSound("coin");
					flag2 = true;
					switch (name)
					{
					case "Keg":
						Game1.stats.BeveragesMade++;
						break;
					case "Preserves Jar":
						Game1.stats.PreservesMade++;
						break;
					case "Cheese Press":
						if (value3.ParentSheetIndex == 426)
						{
							Game1.stats.GoatCheeseMade++;
						}
						else
						{
							Game1.stats.CheeseMade++;
						}
						break;
					}
				}
				if (name.Equals("Crystalarium"))
				{
					minutesUntilReady.Value = getMinutesForCrystalarium(value3.ParentSheetIndex);
					heldObject.Value = (Object)value3.getOne();
				}
				else if (name.Contains("Tapper"))
				{
					if (who.currentLocation.terrainFeatures.ContainsKey(tileLocation) && who.currentLocation.terrainFeatures[tileLocation] is Tree)
					{
						Tree tree = who.currentLocation.terrainFeatures[tileLocation] as Tree;
						tree.UpdateTapperProduct(this, value3);
					}
				}
				else
				{
					heldObject.Value = null;
				}
				readyForHarvest.Value = false;
				showNextIndex.Value = false;
				if (name.Equals("Bee House") && !Game1.GetSeasonForLocation(who.currentLocation).Equals("winter"))
				{
					heldObject.Value = new Object(Vector2.Zero, 340, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
					minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 4);
				}
				else if (name.Equals("Worm Bin"))
				{
					heldObject.Value = new Object(685, Game1.random.Next(2, 6));
					minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 1);
				}
				if (flag2)
				{
					AttemptAutoLoad(who);
				}
				return true;
			}
			return false;
		}

		public virtual bool IsScarecrow()
		{
			if (HasContextTag("crow_scare"))
			{
				return true;
			}
			return Name.Contains("arecrow");
		}

		public virtual int GetRadiusForScarecrow()
		{
			foreach (string contextTag in GetContextTags())
			{
				if (contextTag != null && contextTag.StartsWith("crow_scare_radius_"))
				{
					string s = contextTag.Substring("crow_scare_radius".Length + 1);
					int result = 0;
					if (int.TryParse(s, out result))
					{
						return result;
					}
				}
			}
			if (Name.Contains("Deluxe"))
			{
				return 17;
			}
			return 9;
		}

		public virtual void AttemptAutoLoad(Farmer who)
		{
			Object value = null;
			if (!who.currentLocation.objects.TryGetValue(new Vector2(TileLocation.X, TileLocation.Y - 1f), out value) || value == null || !(value is Chest))
			{
				return;
			}
			Chest chest = value as Chest;
			if (chest.specialChestType.Value != Chest.SpecialChestTypes.AutoLoader)
			{
				return;
			}
			chest.GetMutex().RequestLock(delegate
			{
				chest.GetMutex().ReleaseLock();
				Object value2 = heldObject.Value;
				heldObject.Value = null;
				foreach (Item item in chest.items)
				{
					autoLoadChest = chest;
					bool flag = performObjectDropInAction(item, probe: true, who);
					heldObject.Value = value2;
					if (flag)
					{
						if (performObjectDropInAction(item, probe: false, who))
						{
							ConsumeInventoryItem(who, item, 1);
						}
						autoLoadChest = null;
						return;
					}
				}
				autoLoadChest = null;
				heldObject.Value = value2;
			});
		}

		public virtual void farmerAdjacentAction(GameLocation location)
		{
			if (name == null || isTemporarilyInvisible)
			{
				return;
			}
			if (name.Equals("Flute Block") && (internalSound == null || ((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds - lastNoteBlockSoundTime >= 1000 && !internalSound.IsPlaying)) && !Game1.dialogueUp)
			{
				if (Game1.soundBank != null)
				{
					internalSound = Game1.soundBank.GetCue("flute");
					internalSound.SetVariable("Pitch", preservedParentSheetIndex.Value);
					internalSound.Play();
				}
				scale.Y = 1.3f;
				shakeTimer = 200;
				lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
				if (location is IslandSouthEast)
				{
					(location as IslandSouthEast).OnFlutePlayed(preservedParentSheetIndex.Value);
				}
			}
			else if (name.Equals("Drum Block") && (internalSound == null || (Game1.currentGameTime.TotalGameTime.TotalMilliseconds - (double)lastNoteBlockSoundTime >= 1000.0 && !internalSound.IsPlaying)) && !Game1.dialogueUp)
			{
				if (Game1.soundBank != null)
				{
					internalSound = Game1.soundBank.GetCue("drumkit" + preservedParentSheetIndex.Value);
					internalSound.Play();
				}
				scale.Y = 1.3f;
				shakeTimer = 200;
				lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
			}
			else
			{
				if (!name.Equals("Obelisk"))
				{
					return;
				}
				scale.X += 1f;
				if (scale.X > 30f)
				{
					base.ParentSheetIndex = (((int)parentSheetIndex == 29) ? 30 : 29);
					scale.X = 0f;
					scale.Y += 2f;
				}
				if (!(scale.Y >= 20f) || !(Game1.random.NextDouble() < 0.0001) || location.characters.Count >= 4)
				{
					return;
				}
				Vector2 vector = Game1.player.getTileLocation();
				Vector2[] adjacentTilesOffsets = Character.AdjacentTilesOffsets;
				foreach (Vector2 vector2 in adjacentTilesOffsets)
				{
					Vector2 vector3 = vector + vector2;
					if (!location.isTileOccupied(vector3) && location.isTilePassable(new Location((int)vector3.X, (int)vector3.Y), Game1.viewport) && location.isCharacterAtTile(vector3) == null)
					{
						if (Game1.random.NextDouble() < 0.1)
						{
							location.characters.Add(new GreenSlime(vector3 * new Vector2(64f, 64f)));
						}
						else if (Game1.random.NextDouble() < 0.5)
						{
							location.characters.Add(new ShadowGuy(vector3 * new Vector2(64f, 64f)));
						}
						else
						{
							location.characters.Add(new ShadowGirl(vector3 * new Vector2(64f, 64f)));
						}
						((Monster)location.characters[location.characters.Count - 1]).moveTowardPlayerThreshold.Value = 4;
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(352, 400f, 2, 1, vector3 * new Vector2(64f, 64f), flicker: false, flipped: false));
						location.playSound("shadowpeep");
						break;
					}
				}
			}
		}

		public virtual void addWorkingAnimation(GameLocation environment)
		{
			if (environment == null || !environment.farmers.Any())
			{
				return;
			}
			switch (name)
			{
			case "Keg":
			{
				Color color = Color.DarkGray;
				if (heldObject.Value.Name.Contains("Wine"))
				{
					color = Color.Lavender;
				}
				else if (heldObject.Value.Name.Contains("Juice"))
				{
					color = Color.White;
				}
				else if (heldObject.Value.name.Equals("Beer"))
				{
					color = Color.Yellow;
				}
				Game1.multiplayer.broadcastSprites(environment, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, color * 0.75f, 1f, 0f, 0f, 0f)
				{
					alphaFade = 0.005f
				});
				environment.playSound("bubbles");
				break;
			}
			case "Preserves Jar":
			{
				Color color = Color.White;
				if (heldObject.Value.Name.Contains("Pickled"))
				{
					color = Color.White;
				}
				else if (heldObject.Value.Name.Contains("Jelly"))
				{
					color = Color.LightBlue;
				}
				Game1.multiplayer.broadcastSprites(environment, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, color * 0.75f, 1f, 0f, 0f, 0f)
				{
					alphaFade = 0.005f
				});
				break;
			}
			case "Oil Maker":
				Game1.multiplayer.broadcastSprites(environment, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow, 1f, 0f, 0f, 0f)
				{
					alphaFade = 0.005f
				});
				break;
			case "Furnace":
				if (Game1.random.NextDouble() < 0.5)
				{
					Game1.multiplayer.broadcastSprites(environment, new TemporaryAnimatedSprite(30, tileLocation.Value * 64f + new Vector2(0f, -16f), Color.White, 4, flipped: false, 50f, 10, 64, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f)
					{
						alphaFade = 0.005f,
						light = true,
						lightcolor = Color.Black
					});
					environment.playSound("fireball");
				}
				break;
			case "Slime Egg-Press":
				Game1.multiplayer.broadcastSprites(environment, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -160f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Lime, 1f, 0f, 0f, 0f)
				{
					alphaFade = 0.005f
				});
				break;
			}
		}

		public virtual void onReadyForHarvest(GameLocation environment)
		{
		}

		public virtual bool minutesElapsed(int minutes, GameLocation environment)
		{
			if (heldObject.Value != null && !name.Contains("Table") && (!bigCraftable || (int)parentSheetIndex != 165))
			{
				if (name.Equals("Bee House") && !environment.IsOutdoors)
				{
					return false;
				}
				if (IsSprinkler())
				{
					return false;
				}
				if ((bool)bigCraftable && (int)parentSheetIndex == 231)
				{
					return false;
				}
				if (Game1.IsMasterGame)
				{
					minutesUntilReady.Value -= minutes;
				}
				if ((int)minutesUntilReady <= 0 && !name.Contains("Incubator"))
				{
					if (!readyForHarvest)
					{
						environment.playSound("dwop");
					}
					readyForHarvest.Value = true;
					minutesUntilReady.Value = 0;
					onReadyForHarvest(environment);
					showNextIndex.Value = false;
					if (name.Equals("Bee House") || name.Equals("Loom") || name.Equals("Mushroom Box"))
					{
						showNextIndex.Value = true;
					}
					if (lightSource != null)
					{
						environment.removeLightSource(lightSource.identifier);
						lightSource = null;
					}
				}
				if (!readyForHarvest && Game1.random.NextDouble() < 0.33)
				{
					addWorkingAnimation(environment);
				}
			}
			else if ((bool)bigCraftable)
			{
				switch ((int)parentSheetIndex)
				{
				case 29:
				case 30:
					showNextIndex.Value = (int)parentSheetIndex == 29;
					scale.Y = Math.Max(0f, scale.Y -= minutes / 2 + 1);
					break;
				case 96:
				case 97:
					minutesUntilReady.Value -= minutes;
					showNextIndex.Value = (int)parentSheetIndex == 96;
					if ((int)minutesUntilReady <= 0)
					{
						performRemoveAction(tileLocation, environment);
						environment.objects.Remove(tileLocation);
						environment.objects.Add(tileLocation, new Object(tileLocation, 98));
						if (!Game1.MasterPlayer.mailReceived.Contains("Capsule_Broken"))
						{
							Game1.MasterPlayer.mailReceived.Add("Capsule_Broken");
						}
					}
					break;
				case 141:
				case 142:
					showNextIndex.Value = (int)parentSheetIndex == 141;
					break;
				case 83:
					showNextIndex.Value = false;
					environment.removeLightSource((int)(tileLocation.X * 797f + tileLocation.Y * 13f + 666f));
					break;
				}
			}
			return false;
		}

		public override string checkForSpecialItemHoldUpMeessage()
		{
			if (!bigCraftable)
			{
				if (type != null && type.Equals("Arch"))
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12993");
				}
				switch ((int)parentSheetIndex)
				{
				case 102:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12994");
				case 535:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12995");
				}
			}
			else
			{
				int num = parentSheetIndex;
				if (num == 160)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12996");
				}
			}
			return base.checkForSpecialItemHoldUpMeessage();
		}

		public virtual bool countsForShippedCollection()
		{
			if (type == null || type.Contains("Arch") || (bool)bigCraftable)
			{
				return false;
			}
			if ((int)parentSheetIndex == 433)
			{
				return true;
			}
			switch (base.Category)
			{
			case -74:
			case -29:
			case -24:
			case -22:
			case -21:
			case -20:
			case -19:
			case -14:
			case -12:
			case -8:
			case -7:
			case -2:
			case 0:
				return false;
			default:
				return isIndexOkForBasicShippedCategory(parentSheetIndex);
			}
		}

		public static bool isIndexOkForBasicShippedCategory(int index)
		{
			switch (index)
			{
			case 434:
				return false;
			case 889:
			case 928:
				return false;
			default:
				return true;
			}
		}

		public static bool isPotentialBasicShippedCategory(int index, string category)
		{
			int result = 0;
			int.TryParse(category, out result);
			if (index == 433)
			{
				return true;
			}
			switch (result)
			{
			case -74:
			case -29:
			case -24:
			case -22:
			case -21:
			case -20:
			case -19:
			case -14:
			case -12:
			case -8:
			case -7:
			case -2:
				return false;
			case 0:
				return false;
			default:
				return isIndexOkForBasicShippedCategory(index);
			}
		}

		public virtual Vector2 getScale()
		{
			if (base.Category == -22)
			{
				return Vector2.Zero;
			}
			if (!bigCraftable)
			{
				scale.Y = Math.Max(4f, scale.Y - 0.04f);
				return scale;
			}
			if ((heldObject.Value == null && (int)minutesUntilReady <= 0) || (bool)readyForHarvest || (int)parentSheetIndex == 10 || name.Contains("Table") || (int)parentSheetIndex == 105 || (int)parentSheetIndex == 264 || (int)parentSheetIndex == 165 || (int)parentSheetIndex == 231)
			{
				return Vector2.Zero;
			}
			if (name.Equals("Loom"))
			{
				scale.X = (float)((double)(scale.X + 0.04f) % (Math.PI * 2.0));
				return Vector2.Zero;
			}
			scale.X -= 0.1f;
			scale.Y += 0.1f;
			if (scale.X <= 0f)
			{
				scale.X = 10f;
			}
			if (scale.Y >= 10f)
			{
				scale.Y = 0f;
			}
			return new Vector2(Math.Abs(scale.X - 5f), Math.Abs(scale.Y - 5f));
		}

		public virtual void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
			if ((bool)f.ActiveObject.bigCraftable)
			{
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, objectPosition, getSourceRectForBigCraftable(f.ActiveObject.ParentSheetIndex), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
				return;
			}
			spriteBatch.Draw(Game1.objectSpriteSheet, objectPosition, GameLocation.getSourceRectForObject(f.ActiveObject.ParentSheetIndex), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
			if (f.ActiveObject == null || !f.ActiveObject.Name.Contains("="))
			{
				return;
			}
			spriteBatch.Draw(Game1.objectSpriteSheet, objectPosition + new Vector2(32f, 32f), GameLocation.getSourceRectForObject(f.ActiveObject.ParentSheetIndex), Color.White, 0f, new Vector2(32f, 32f), 4f + Math.Abs(Game1.starCropShimmerPause) / 8f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
			if (!(Math.Abs(Game1.starCropShimmerPause) <= 0.05f) || !(Game1.random.NextDouble() < 0.97))
			{
				Game1.starCropShimmerPause += 0.04f;
				if (Game1.starCropShimmerPause >= 0.8f)
				{
					Game1.starCropShimmerPause = -0.8f;
				}
			}
		}

		public virtual void drawPlacementBounds(SpriteBatch spriteBatch, GameLocation location)
		{
			if (isPlaceable() && !(this is Wallpaper))
			{
				int x = (int)Game1.GetPlacementGrabTile().X * 64;
				int y = (int)Game1.GetPlacementGrabTile().Y * 64;
				Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
				if (Game1.isCheckingNonMousePlacement)
				{
					Vector2 nearbyValidPlacementPosition = Utility.GetNearbyValidPlacementPosition(Game1.player, location, this, x, y);
					x = (int)nearbyValidPlacementPosition.X;
					y = (int)nearbyValidPlacementPosition.Y;
				}
				if (Utility.isThereAnObjectHereWhichAcceptsThisItem(location, this, x, y) || DrawRedGreenRectangleForPlacing(spriteBatch, location) || !Game1.options.greenSquaresGuide || base.ParentSheetIndex == 286 || base.ParentSheetIndex == 287 || base.ParentSheetIndex == 288)
				{
					return;
				}
				int num = Math.Max(0, Game1.viewport.X / 64);
				int num2 = Math.Min((Game1.viewport.X + Game1.viewport.Width) / 64, location.map.Layers[0].LayerWidth - 1);
				int num3 = Math.Max(0, Game1.viewport.Y / 64);
				int num4 = Math.Min((Game1.viewport.Y + Game1.viewport.Height) / 64, location.map.Layers[0].LayerHeight - 1);
				if ((bool)bigCraftable && (int)parentSheetIndex == 71 && (!(location is MineShaft) || (location as MineShaft).mineLevel == 120))
				{
					return;
				}
				if (_lastQuantity != Stack)
				{
					_lastQuantity = Stack;
					ClearRedGreenSquareDict();
				}
				for (int i = num; i <= num2; i++)
				{
					for (int j = num3; j <= num4; j++)
					{
						Vector2 key = new Vector2(i, j);
						if (!_redGreenSquareDict.TryGetValue(key, out var value))
						{
							value = itemCanBePlaced(location, this, key);
							_redGreenSquareDict[key] = value;
						}
						if (!value)
						{
							continue;
						}
						bool flag = true;
						if ((int)parentSheetIndex == 685)
						{
							CrabPot crabPot = FetchCrabPot(location, new Vector2(i, j));
							if (crabPot != null)
							{
								flag = false;
								if (crabPot.bait.Value == null)
								{
									spriteBatch.Draw(Game1.mouseCursors, crabPot.OffsetPosition, new Microsoft.Xna.Framework.Rectangle(value ? 194 : 210, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
								}
							}
						}
						if (flag)
						{
							spriteBatch.Draw(Game1.mouseCursors, new Vector2(i * 64 - Game1.viewport.X, j * 64 - Game1.viewport.Y), new Microsoft.Xna.Framework.Rectangle(value ? 194 : 210, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
						}
					}
				}
			}
			else
			{
				if (!(this is Wallpaper))
				{
					return;
				}
				int num5 = Math.Max(1, Game1.viewport.X / 64);
				int num6 = Math.Min((Game1.viewport.X + Game1.viewport.Width) / 64, location.map.Layers[0].LayerWidth - 2);
				int num7 = Math.Max(1, Game1.viewport.Y / 64);
				int num8 = Math.Min((Game1.viewport.Y + Game1.viewport.Height) / 64, location.map.Layers[0].LayerHeight - 2);
				for (int k = num5; k <= num6; k++)
				{
					for (int l = num7; l <= num8; l++)
					{
						bool flag2 = itemCanBePlaced(location, this, new Vector2(k, l));
						if (flag2)
						{
							spriteBatch.Draw(Game1.mouseCursors, new Vector2(k * 64 - Game1.viewport.X, l * 64 - Game1.viewport.Y), new Microsoft.Xna.Framework.Rectangle(flag2 ? 194 : 210, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
						}
					}
				}
			}
		}

		public static CrabPot FetchCrabPot(GameLocation gameLocation, Vector2 tileLocation)
		{
			if (gameLocation.objects.ContainsKey(tileLocation))
			{
				gameLocation.objects.TryGetValue(tileLocation, out var value);
				if (value != null && (int)value.parentSheetIndex == 710)
				{
					return (CrabPot)value;
				}
			}
			return null;
		}

		private bool itemCanBePlaced(GameLocation gameLocation, Object svObject, Vector2 tileLocation)
		{
			if (Utility.isThereAnObjectHereWhichAcceptsThisItem(gameLocation, svObject, (int)tileLocation.X, (int)tileLocation.Y) && Utility.withinRadiusOfPlayer((int)tileLocation.X, (int)tileLocation.Y, 1, Game1.player))
			{
				return true;
			}
			if (gameLocation is BathHousePool)
			{
				return false;
			}
			if ((int)svObject.parentSheetIndex == 685 && gameLocation.objects.ContainsKey(tileLocation))
			{
				gameLocation.objects.TryGetValue(tileLocation, out var value);
				if (value != null && (int)value.parentSheetIndex == 710)
				{
					return true;
				}
			}
			if ((int)svObject.parentSheetIndex == 284 || (int)svObject.parentSheetIndex == 262)
			{
				if (gameLocation is BuildableGameLocation)
				{
					foreach (Building building in ((BuildableGameLocation)gameLocation).buildings)
					{
						if (building is Mill && tileLocation.X == (float)((int)building.tileX + 1) && tileLocation.Y == (float)((int)building.tileY + 1))
						{
							return true;
						}
					}
				}
				return false;
			}
			if (svObject is Wallpaper && !(gameLocation is DecoratableLocation))
			{
				return false;
			}
			if (gameLocation is AnimalHouse && ((AnimalHouse)gameLocation).uniqueName.Value.Contains("Coop") && tileLocation.X == 2f && (tileLocation.Y == 1f || tileLocation.Y == 8f || tileLocation.Y == 9f))
			{
				return false;
			}
			if ((gameLocation is MineShaft && !((MineShaft)gameLocation).isTileClearForMineObjects(tileLocation)) || (gameLocation is VolcanoDungeon && !((VolcanoDungeon)gameLocation).isTileClearForMineObjects(tileLocation)) || !svObject.canBePlacedHere(gameLocation, tileLocation) || ((gameLocation is MineShaft || gameLocation is VolcanoDungeon) && (svObject.ParentSheetIndex == 130 || svObject.ParentSheetIndex == 248 || svObject.ParentSheetIndex == 256 || svObject.ParentSheetIndex == 275)))
			{
				return false;
			}
			if (svObject.canBePlacedHere(gameLocation, tileLocation))
			{
				if (!svObject.isPassable())
				{
					foreach (Farmer farmer in gameLocation.farmers)
					{
						if (farmer != Game1.player && farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64)))
						{
							return false;
						}
					}
				}
				if ((int)svObject.parentSheetIndex == 105)
				{
					if (gameLocation.terrainFeatures.ContainsKey(tileLocation) && gameLocation.terrainFeatures[tileLocation] is Tree && (int)(gameLocation.terrainFeatures[tileLocation] as Tree).growthStage >= 5 && !(gameLocation.terrainFeatures[tileLocation] as Tree).stump && !gameLocation.objects.ContainsKey(tileLocation))
					{
						return true;
					}
					return false;
				}
				if (gameLocation is FarmHouse && ((FarmHouse)gameLocation).upgradeLevel >= 2 && tileLocation.Y == 5f && (tileLocation.X == 22f || tileLocation.X == 23f || tileLocation.X == 26f || tileLocation.X == 27f))
				{
					return false;
				}
				if (gameLocation is DecoratableLocation)
				{
					DecoratableLocation decoratableLocation = (DecoratableLocation)gameLocation;
					foreach (Furniture item in decoratableLocation.furniture)
					{
						if (item.name.Contains("Table") && svObject is Furniture && !(svObject is TV) && (int)((Furniture)svObject).furniture_type == 8)
						{
							if (tileLocation.X >= item.tileLocation.X && tileLocation.X < item.tileLocation.X + (float)(item.boundingBox.Width / 64) && tileLocation.Y >= item.tileLocation.Y && tileLocation.Y < item.tileLocation.Y + (float)(item.boundingBox.Height / 64))
							{
								return true;
							}
						}
						else if ((int)item.furniture_type == 12 && tileLocation.X >= item.tileLocation.X && tileLocation.X < item.tileLocation.X + (float)(item.boundingBox.Width / 64) && tileLocation.Y >= item.tileLocation.Y && tileLocation.Y < item.tileLocation.Y + (float)(item.boundingBox.Height / 64))
						{
							if (svObject is Wallpaper && !(svObject as Wallpaper).isFloor)
							{
								return false;
							}
							return true;
						}
					}
				}
				if (gameLocation.terrainFeatures.ContainsKey(tileLocation) && gameLocation.terrainFeatures[tileLocation] is HoeDirt && (svObject.ParentSheetIndex == 599 || svObject.ParentSheetIndex == 621 || svObject.ParentSheetIndex == 645))
				{
					return true;
				}
				if ((int)svObject.category == -74 && gameLocation.terrainFeatures.ContainsKey(tileLocation) && gameLocation.terrainFeatures[tileLocation] is HoeDirt && ((HoeDirt)gameLocation.terrainFeatures[tileLocation]).canPlantThisSeedHere(svObject.ParentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y))
				{
					return true;
				}
				if (name.Contains("Sapling"))
				{
					Vector2 key = default(Vector2);
					for (int i = (int)tileLocation.X - 2; i <= (int)tileLocation.X + 2; i++)
					{
						for (int j = (int)tileLocation.Y - 2; j <= (int)tileLocation.Y + 2; j++)
						{
							key.X = i;
							key.Y = j;
							if (gameLocation.terrainFeatures.ContainsKey(key) && (gameLocation.terrainFeatures[key] is Tree || gameLocation.terrainFeatures[key] is FruitTree))
							{
								return false;
							}
						}
					}
					if (gameLocation.terrainFeatures.ContainsKey(tileLocation))
					{
						if (gameLocation.terrainFeatures[tileLocation] is HoeDirt)
						{
							return (gameLocation.terrainFeatures[tileLocation] as HoeDirt).crop == null;
						}
						return false;
					}
					if (!(gameLocation is Farm) || (gameLocation.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back") == null && !gameLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back").Equals("Grass")) || gameLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "NoSpawn", "Back").Equals("Tree"))
					{
						if (gameLocation.IsGreenhouse)
						{
							if (gameLocation.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Diggable", "Back") == null)
							{
								return gameLocation.doesTileHavePropertyNoNull((int)tileLocation.X, (int)tileLocation.Y, "Type", "Back").Equals("Stone");
							}
							return true;
						}
						return false;
					}
					return true;
				}
				if (gameLocation.isTilePlaceable(tileLocation, svObject) && svObject.isPlaceable())
				{
					if ((int)svObject.parentSheetIndex == 710)
					{
						return CrabPot.CanPlaceHere(gameLocation, tileLocation);
					}
					if (gameLocation.isTileOnMap(tileLocation) && gameLocation.isTilePassable(new Location((int)tileLocation.X, (int)tileLocation.Y), Game1.viewport))
					{
						if (!gameLocation.isTileOccupied(tileLocation) || (gameLocation.terrainFeatures.ContainsKey(tileLocation) && gameLocation.terrainFeatures[tileLocation] is HoeDirt && ((int)svObject.category == -74 || (int)svObject.category == -19 || (int)svObject.parentSheetIndex == 286 || (int)svObject.parentSheetIndex == 287 || (int)svObject.parentSheetIndex == 288)))
						{
							if (gameLocation is DecoratableLocation)
							{
								foreach (Furniture item2 in ((DecoratableLocation)gameLocation).furniture)
								{
									if (item2.getBoundingBox(item2.tileLocation).Intersects(new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64)))
									{
										return false;
									}
								}
							}
							if (svObject is Wallpaper && gameLocation is DecoratableLocation)
							{
								return ((Wallpaper)svObject).canReallyBePlacedHere((DecoratableLocation)gameLocation, tileLocation);
							}
							if (gameLocation is DecoratableLocation)
							{
								if (svObject.Category == -9 && ((DecoratableLocation)gameLocation).isTileOnWall((int)tileLocation.X, (int)tileLocation.Y))
								{
									return false;
								}
								if (((DecoratableLocation)gameLocation).isTileOnWall((int)tileLocation.X, (int)tileLocation.Y) && !(svObject is Wallpaper) && (!(svObject is Furniture) || ((int)((Furniture)svObject).furniture_type != 6 && (int)((Furniture)svObject).furniture_type != 13)))
								{
									return false;
								}
							}
							else if (gameLocation is DecoratableLocation && ((DecoratableLocation)gameLocation).isTileOnWall((int)tileLocation.X, (int)tileLocation.Y) && !(svObject is Wallpaper) && (!(svObject is Furniture) || ((int)((Furniture)svObject).furniture_type != 6 && (int)((Furniture)svObject).furniture_type != 13)))
							{
								return false;
							}
							return true;
						}
						if (!gameLocation.isTileOccupied(tileLocation) || (gameLocation.terrainFeatures.ContainsKey(tileLocation) && gameLocation.terrainFeatures[tileLocation] is HoeDirt && (int)svObject.category == -74))
						{
							return true;
						}
					}
				}
				else if (gameLocation is FarmHouse && !gameLocation.isTilePlaceable(tileLocation, svObject))
				{
					return false;
				}
				if (gameLocation is DecoratableLocation && svObject.Category != -9 && !(svObject is Wallpaper))
				{
					return true;
				}
			}
			return false;
		}

		public override void drawInMenuWithStackNumber(SpriteBatch spriteBatch, Vector2 location, float scaleSize, int stackNumber = -1)
		{
			drawInMenuWithColour(spriteBatch, location, scaleSize, 1f, 0.001f, StackDrawType.Draw, Color.White, drawShadow: true, stackNumber);
		}

		public void drawInMenuWithColour(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow = true, int stackNumber = -1)
		{
			if ((bool)isRecipe)
			{
				transparency = 0.5f;
			}
			if ((bool)bigCraftable)
			{
				Microsoft.Xna.Framework.Rectangle sourceRectForBigCraftable = getSourceRectForBigCraftable(parentSheetIndex);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2), sourceRectForBigCraftable, Color.White * transparency, 0f, new Vector2(8f, 16f), 4f * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize / 2f)), SpriteEffects.None, layerDepth);
				if (color != Color.White)
				{
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2), sourceRectForBigCraftable, color * transparency, 0f, new Vector2(8f, 16f), 4f * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize / 2f)), SpriteEffects.None, layerDepth);
				}
			}
			else
			{
				if (scaleSize == 1f && (int)parentSheetIndex != 590)
				{
					spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), Game1.shadowTexture.Bounds, Color.White * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
				}
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2((int)((float)(base.itemSlotSize / 2) * scaleSize), (int)((float)(base.itemSlotSize / 2) * scaleSize)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
				if (color != Color.White)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2((int)((float)(base.itemSlotSize / 2) * scaleSize), (int)((float)(base.itemSlotSize / 2) * scaleSize)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
				}
				if ((int)quality > 0 && drawStackNumber != 0)
				{
					float num = (((int)quality < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1f) * 0.05f));
					spriteBatch.Draw(Game1.mouseCursors, location + new Vector2((float)(base.itemSlotSize / 2) - 30f * scaleSize, (float)base.itemSlotSize - 24f * scaleSize), ((int)quality < 4) ? new Microsoft.Xna.Framework.Rectangle(338 + ((int)quality - 1) * 8, 400, 8, 8) : new Microsoft.Xna.Framework.Rectangle(346, 391, 8, 8), Color.White * transparency, 0f, new Vector2(4f, 4f), 3f * scaleSize, SpriteEffects.None, layerDepth);
				}
				if ((int)category == -22)
				{
					float num2 = ((float)(FishingRod.maxTackleUses - uses.Value) + 0f) / (float)FishingRod.maxTackleUses;
					spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)(location.X + 8f), (int)(location.Y + (float)base.itemSlotSize - 16f), (int)((float)(base.itemSlotSize - 16) * num2), 8), Utility.getRedToGreenLerpColor(num2));
				}
			}
			if (((drawStackNumber == StackDrawType.Draw && maximumStackSize() > 1 && Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && Stack != 2147483647 && maximumStackSize() > 1 && (double)scaleSize > 0.3 && Stack != 2147483647 && Stack > 1)
			{
				Utility.drawTinyDigits((stackNumber == -1) ? ((int)stack) : stackNumber, spriteBatch, location + new Vector2((float)(base.itemSlotSize - Utility.getWidthOfTinyDigitString((stackNumber == -1) ? ((int)stack) : stackNumber, 3f * scaleSize)) - 3f * scaleSize, (float)base.itemSlotSize - 24f * scaleSize), 3f * scaleSize, layerDepth, Color.White);
			}
			if ((bool)isRecipe)
			{
				scaleSize *= 0.5f;
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(-12f * scaleSize, -20f * scaleSize), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 451, 16, 16), Color.White, 0f, Vector2.Zero, 4f * scaleSize, SpriteEffects.None, layerDepth + 0.0001f);
			}
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			drawInMenuWithColour(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
		}

		public virtual void drawAsProp(SpriteBatch b)
		{
			if (isTemporarilyInvisible)
			{
				return;
			}
			int num = (int)tileLocation.X;
			int num2 = (int)tileLocation.Y;
			if ((bool)bigCraftable)
			{
				Vector2 vector = getScale();
				vector *= 4f;
				Vector2 vector2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64, num2 * 64 - 64));
				b.Draw(destinationRectangle: new Microsoft.Xna.Framework.Rectangle((int)(vector2.X - vector.X / 2f), (int)(vector2.Y - vector.Y / 2f), (int)(64f + vector.X), (int)(128f + vector.Y / 2f)), texture: Game1.bigCraftableSpriteSheet, sourceRectangle: getSourceRectForBigCraftable(showNextIndex ? (base.ParentSheetIndex + 1) : base.ParentSheetIndex), color: Color.White, rotation: 0f, origin: Vector2.Zero, effects: SpriteEffects.None, layerDepth: Math.Max(0f, (float)((num2 + 1) * 64 - 1) / 10000f) + (((int)parentSheetIndex == 105 || (int)parentSheetIndex == 264) ? 0.0015f : 0f));
				if (Name.Equals("Loom") && (int)minutesUntilReady > 0)
				{
					b.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435), Color.White, scale.X, new Vector2(32f, 32f), 1f, SpriteEffects.None, Math.Max(0f, (float)((num2 + 1) * 64 - 1) / 10000f + 0.0001f));
				}
				return;
			}
			if ((int)parentSheetIndex != 590 && (int)parentSheetIndex != 742)
			{
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)getBoundingBox(new Vector2(num, num2)).Bottom / 15000f);
			}
			Texture2D objectSpriteSheet = Game1.objectSpriteSheet;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64 + 32, num2 * 64 + 32));
			Microsoft.Xna.Framework.Rectangle? sourceRectangle = GameLocation.getSourceRectForObject(base.ParentSheetIndex);
			Color white = Color.White;
			Vector2 origin = new Vector2(8f, 8f);
			_ = scale;
			b.Draw(objectSpriteSheet, position, sourceRectangle, white, 0f, origin, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)getBoundingBox(new Vector2(num, num2)).Bottom / 10000f);
		}

		public virtual void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (isTemporarilyInvisible)
			{
				return;
			}
			if ((bool)bigCraftable)
			{
				Vector2 vector = getScale();
				vector *= 4f;
				Vector2 vector2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
				Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)(vector2.X - vector.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(vector2.Y - vector.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + vector.X), (int)(128f + vector.Y / 2f));
				float num = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
				if (base.ParentSheetIndex == 105 || base.ParentSheetIndex == 264)
				{
					num = Math.Max(0f, (float)((y + 1) * 64 + 2) / 10000f) + (float)x / 1000000f;
				}
				if ((int)parentSheetIndex == 272)
				{
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, getSourceRectForBigCraftable(base.ParentSheetIndex + 1), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, num);
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, vector2 + new Vector2(8.5f, 12f) * 4f, getSourceRectForBigCraftable(base.ParentSheetIndex + 2), Color.White * alpha, (float)Game1.currentGameTime.TotalGameTime.TotalSeconds * -1.5f, new Vector2(7.5f, 15.5f), 4f, SpriteEffects.None, num + 1E-05f);
					return;
				}
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, getSourceRectForBigCraftable(showNextIndex ? (base.ParentSheetIndex + 1) : base.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, num);
				if (Name.Equals("Loom") && (int)minutesUntilReady > 0)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16), Color.White * alpha, scale.X, new Vector2(8f, 8f), 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64) / 10000f + 0.0001f + (float)x * 1E-05f));
				}
				if ((bool)isLamp && Game1.isDarkOut())
				{
					spriteBatch.Draw(Game1.mouseCursors, vector2 + new Vector2(-32f, -32f), new Microsoft.Xna.Framework.Rectangle(88, 1779, 32, 32), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x / 1000000f);
				}
				if ((int)parentSheetIndex == 126 && (int)quality != 0)
				{
					spriteBatch.Draw(FarmerRenderer.hatsTexture, vector2 + new Vector2(-3f, -6f) * 4f, new Microsoft.Xna.Framework.Rectangle(((int)quality - 1) * 20 % FarmerRenderer.hatsTexture.Width, ((int)quality - 1) * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x * 1E-05f);
				}
			}
			else if (!Game1.eventUp || (Game1.CurrentEvent != null && !Game1.CurrentEvent.isTileWalkedOn(x, y)))
			{
				if ((int)parentSheetIndex == 590)
				{
					Texture2D mouseCursors = Game1.mouseCursors;
					Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
					Microsoft.Xna.Framework.Rectangle? sourceRectangle = new Microsoft.Xna.Framework.Rectangle(368 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1200.0 <= 400.0) ? ((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 / 100.0) * 16) : 0), 32, 16, 16);
					Color color = Color.White * alpha;
					Vector2 origin = new Vector2(8f, 8f);
					_ = scale;
					spriteBatch.Draw(mouseCursors, position, sourceRectangle, color, 0f, origin, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(isPassable() ? getBoundingBox(new Vector2(x, y)).Top : getBoundingBox(new Vector2(x, y)).Bottom) / 10000f);
					return;
				}
				if ((int)fragility != 2)
				{
					spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 51 + 4)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)getBoundingBox(new Vector2(x, y)).Bottom / 15000f);
				}
				Texture2D objectSpriteSheet = Game1.objectSpriteSheet;
				Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
				Microsoft.Xna.Framework.Rectangle? sourceRectangle2 = GameLocation.getSourceRectForObject(base.ParentSheetIndex);
				Color color2 = Color.White * alpha;
				Vector2 origin2 = new Vector2(8f, 8f);
				_ = scale;
				spriteBatch.Draw(objectSpriteSheet, position2, sourceRectangle2, color2, 0f, origin2, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(isPassable() ? getBoundingBox(new Vector2(x, y)).Top : getBoundingBox(new Vector2(x, y)).Bottom) / 10000f);
				if (heldObject.Value != null && IsSprinkler())
				{
					Vector2 vector3 = Vector2.Zero;
					if (heldObject.Value.ParentSheetIndex == 913)
					{
						vector3 = new Vector2(0f, -20f);
					}
					Texture2D objectSpriteSheet2 = Game1.objectSpriteSheet;
					Vector2 position3 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)) + vector3);
					Microsoft.Xna.Framework.Rectangle? sourceRectangle3 = GameLocation.getSourceRectForObject(heldObject.Value.ParentSheetIndex + 1);
					Color color3 = Color.White * alpha;
					Vector2 origin3 = new Vector2(8f, 8f);
					_ = scale;
					spriteBatch.Draw(objectSpriteSheet2, position3, sourceRectangle3, color3, 0f, origin3, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(isPassable() ? getBoundingBox(new Vector2(x, y)).Top : getBoundingBox(new Vector2(x, y)).Bottom) / 10000f + 1E-05f);
				}
			}
			if (!readyForHarvest)
			{
				return;
			}
			float num2 = (float)((y + 1) * 64) / 10000f + tileLocation.X / 50000f;
			if ((int)parentSheetIndex == 105 || (int)parentSheetIndex == 264)
			{
				num2 += 0.02f;
			}
			float num3 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 - 8, (float)(y * 64 - 96 - 16) + num3)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, num2 + 1E-06f);
			if (heldObject.Value != null)
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + num3)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, heldObject.Value.parentSheetIndex, 16, 16), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, num2 + 1E-05f);
				if (heldObject.Value is ColoredObject)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + num3)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (int)heldObject.Value.parentSheetIndex + 1, 16, 16), (heldObject.Value as ColoredObject).color.Value * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, num2 + 1.1E-05f);
				}
			}
		}

		public virtual void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1f)
		{
			if (isTemporarilyInvisible)
			{
				return;
			}
			if ((bool)bigCraftable)
			{
				Vector2 vector = getScale();
				vector *= 4f;
				Vector2 vector2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
				Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)(vector2.X - vector.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(vector2.Y - vector.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + vector.X), (int)(128f + vector.Y / 2f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, getSourceRectForBigCraftable(showNextIndex ? (base.ParentSheetIndex + 1) : base.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
				if (Name.Equals("Loom") && (int)minutesUntilReady > 0)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(vector2) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16), Color.White * alpha, scale.X, new Vector2(8f, 8f), 4f, SpriteEffects.None, layerDepth);
				}
				if ((bool)isLamp && Game1.isDarkOut())
				{
					spriteBatch.Draw(Game1.mouseCursors, vector2 + new Vector2(-32f, -32f), new Microsoft.Xna.Framework.Rectangle(88, 1779, 32, 32), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				}
			}
			else if (!Game1.eventUp || !Game1.CurrentEvent.isTileWalkedOn(xNonTile / 64, yNonTile / 64))
			{
				if ((int)parentSheetIndex != 590 && (int)fragility != 2)
				{
					spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32, yNonTile + 51 + 4)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, layerDepth - 1E-06f);
				}
				Texture2D objectSpriteSheet = Game1.objectSpriteSheet;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), yNonTile + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
				Microsoft.Xna.Framework.Rectangle? sourceRectangle = GameLocation.getSourceRectForObject(base.ParentSheetIndex);
				Color color = Color.White * alpha;
				Vector2 origin = new Vector2(8f, 8f);
				_ = scale;
				spriteBatch.Draw(objectSpriteSheet, position, sourceRectangle, color, 0f, origin, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
			}
		}

		private int getMinutesForCrystalarium(int whichGem)
		{
			return whichGem switch
			{
				80 => 420, 
				60 => 3000, 
				68 => 1120, 
				70 => 2400, 
				64 => 3000, 
				62 => 2240, 
				66 => 1360, 
				72 => 7200, 
				82 => 1300, 
				84 => 1120, 
				86 => 800, 
				_ => 5000, 
			};
		}

		public override int maximumStackSize()
		{
			if (base.ParentSheetIndex == 911)
			{
				return 1;
			}
			if (base.Category == -22)
			{
				return 1;
			}
			return 999;
		}

		public override int addToStack(Item otherStack)
		{
			int num = maximumStackSize();
			if (num != 1)
			{
				stack.Value += otherStack.Stack;
				if (otherStack is Object)
				{
					Object @object = otherStack as Object;
					if (IsSpawnedObject && !@object.IsSpawnedObject)
					{
						IsSpawnedObject = false;
					}
				}
				if ((int)stack > num)
				{
					int result = (int)stack - num;
					stack.Value = num;
					return result;
				}
				return 0;
			}
			return otherStack.Stack;
		}

		public virtual void hoverAction()
		{
		}

		public virtual bool clicked(Farmer who)
		{
			return false;
		}

		public override Item getOne()
		{
			if ((bool)bigCraftable)
			{
				int num = base.ParentSheetIndex;
				if (name.Contains("Seasonal"))
				{
					int num2 = base.ParentSheetIndex - base.ParentSheetIndex % 4;
					num = num2;
				}
				Object @object = new Object(tileLocation, num)
				{
					IsRecipe = isRecipe,
					name = name,
					DisplayName = DisplayName,
					SpecialVariable = base.SpecialVariable
				};
				@object._GetOneFrom(this);
				return @object;
			}
			Object object2 = new Object(tileLocation, parentSheetIndex, 1)
			{
				Scale = scale,
				Quality = quality,
				IsSpawnedObject = isSpawnedObject,
				IsRecipe = isRecipe,
				Stack = 1,
				SpecialVariable = base.SpecialVariable,
				Price = price,
				name = name,
				DisplayName = DisplayName,
				HasBeenInInventory = base.HasBeenInInventory,
				HasBeenPickedUpByFarmer = HasBeenPickedUpByFarmer
			};
			object2.uses.Value = uses.Value;
			object2.questItem.Value = questItem;
			object2.questId.Value = questId;
			object2.preserve.Value = preserve.Value;
			object2.preservedParentSheetIndex.Value = preservedParentSheetIndex.Value;
			object2._GetOneFrom(this);
			return object2;
		}

		public override void _GetOneFrom(Item source)
		{
			orderData.Value = (source as Object).orderData.Value;
			owner.Value = (source as Object).owner.Value;
			base._GetOneFrom(source);
		}

		public override bool canBePlacedHere(GameLocation l, Vector2 tile)
		{
			if ((int)parentSheetIndex == 710)
			{
				if (CrabPot.IsValidCrabPotLocationTile(l, (int)tile.X, (int)tile.Y))
				{
					return true;
				}
				return false;
			}
			if (((int)parentSheetIndex == 105 || (int)parentSheetIndex == 264) && (bool)bigCraftable && l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is Tree && !l.objects.ContainsKey(tile))
			{
				return true;
			}
			if ((int)parentSheetIndex == 805 && l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is Tree)
			{
				return true;
			}
			if (name != null && name.Contains("Bomb") && (!l.isTileOccupiedForPlacement(tile, this) || l.isTileOccupiedByFarmer(tile) != null))
			{
				return true;
			}
			if (isWildTreeSeed(parentSheetIndex))
			{
				if (!l.isTileOccupiedForPlacement(tile, this))
				{
					return canPlaceWildTreeSeed(l, tile);
				}
				return false;
			}
			if (((int)category == -74 || (int)category == -19) && !l.isTileHoeDirt(tile) && !bigCraftable.Value)
			{
				switch ((int)parentSheetIndex)
				{
				case 69:
				case 292:
				case 309:
				case 310:
				case 311:
				case 628:
				case 629:
				case 630:
				case 631:
				case 632:
				case 633:
				case 835:
				case 891:
					if (!l.isTileOccupiedForPlacement(tile, this))
					{
						if (!l.CanPlantTreesHere(parentSheetIndex, (int)tile.X, (int)tile.Y))
						{
							return l.isOutdoors;
						}
						return true;
					}
					return false;
				case 251:
					if (!l.isTileOccupiedForPlacement(tile, this))
					{
						if (!l.isOutdoors)
						{
							if (l.IsGreenhouse)
							{
								return l.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null;
							}
							return false;
						}
						return true;
					}
					return false;
				default:
					return false;
				}
			}
			if ((int)category == -19 && l.isTileHoeDirt(tile))
			{
				if ((int)parentSheetIndex == 805)
				{
					return false;
				}
				if (l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is HoeDirt && (int)(l.terrainFeatures[tile] as HoeDirt).fertilizer != 0)
				{
					return false;
				}
				if (l.objects.ContainsKey(tile) && l.objects[tile] is IndoorPot && (int)(l.objects[tile] as IndoorPot).hoeDirt.Value.fertilizer != 0)
				{
					return false;
				}
			}
			if (l != null)
			{
				Vector2 vector = tile * 64f * 64f;
				vector.X += 32f;
				vector.Y += 32f;
				foreach (Furniture item in l.furniture)
				{
					if ((int)item.furniture_type == 11 && item.getBoundingBox(item.tileLocation).Contains((int)vector.X, (int)vector.Y) && item.heldObject.Value == null)
					{
						return true;
					}
					if (item.getBoundingBox(item.TileLocation).Intersects(new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64)) && !item.isPassable() && !item.AllowPlacementOnThisTile((int)tile.X, (int)tile.Y))
					{
						return false;
					}
				}
			}
			return !l.isTileOccupiedForPlacement(tile, this);
		}

		public override bool isPlaceable()
		{
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 681) || Utility.IsNormalObjectAtParentSheetIndex(this, 688) || Utility.IsNormalObjectAtParentSheetIndex(this, 689) || Utility.IsNormalObjectAtParentSheetIndex(this, 690) || Utility.IsNormalObjectAtParentSheetIndex(this, 261) || Utility.IsNormalObjectAtParentSheetIndex(this, 886))
			{
				return false;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 896))
			{
				return false;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 911))
			{
				return false;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 879))
			{
				return false;
			}
			_ = base.Category;
			if (type.Value != null && (base.Category == -8 || base.Category == -9 || type.Value.Equals("Crafting") || isSapling() || (int)parentSheetIndex == 710 || base.Category == -74 || base.Category == -19) && ((int)edibility < 0 || (int)parentSheetIndex == 292 || (int)parentSheetIndex == 891))
			{
				return true;
			}
			return false;
		}

		public bool IsConsideredReadyMachineForComputer()
		{
			if (bigCraftable.Value && heldObject.Value != null)
			{
				if (!(heldObject.Value is Chest))
				{
					return minutesUntilReady.Value <= 0;
				}
				if (!(heldObject.Value as Chest).isEmpty())
				{
					return true;
				}
			}
			return false;
		}

		public virtual bool isSapling()
		{
			if (bigCraftable.Value)
			{
				return false;
			}
			if (!(GetType() == typeof(Object)))
			{
				return false;
			}
			if (name.Contains("Sapling"))
			{
				return true;
			}
			return false;
		}

		public static bool isWildTreeSeed(int index)
		{
			if (index != 309 && index != 310 && index != 311 && index != 292)
			{
				return index == 891;
			}
			return true;
		}

		private bool canPlaceWildTreeSeed(GameLocation location, Vector2 tile)
		{
			bool flag = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null;
			string text = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NoSpawn", "Back");
			bool flag2 = text != null && (text.Equals("Tree") || text.Equals("All") || text.Equals("True"));
			bool flag3 = location is Farm || location.CanPlantTreesHere(parentSheetIndex, (int)tile.X, (int)tile.Y);
			bool flag4 = location.objects.ContainsKey(tile) || (location.terrainFeatures.ContainsKey(tile) && !(location.terrainFeatures[tile] is HoeDirt));
			return (flag3 || flag) && !flag2 && !flag4;
		}

		public virtual bool IsSprinkler()
		{
			if (GetBaseRadiusForSprinkler() >= 0)
			{
				return true;
			}
			return false;
		}

		public virtual int GetModifiedRadiusForSprinkler()
		{
			int num = GetBaseRadiusForSprinkler();
			if (num < 0)
			{
				return -1;
			}
			if (heldObject.Value != null && Utility.IsNormalObjectAtParentSheetIndex((Object)heldObject, 915))
			{
				num++;
			}
			return num;
		}

		public virtual int GetBaseRadiusForSprinkler()
		{
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 599))
			{
				return 0;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 621))
			{
				return 1;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 645))
			{
				return 2;
			}
			return -1;
		}

		public virtual bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			Vector2 placementTile = new Vector2(x / 64, y / 64);
			health = 10;
			_redGreenSquareDict.Remove(placementTile);
			if (who != null)
			{
				owner.Value = who.UniqueMultiplayerID;
			}
			else
			{
				owner.Value = Game1.player.UniqueMultiplayerID;
			}
			if (!bigCraftable && !(this is Furniture))
			{
				if (IsSprinkler() && location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "NoSprinklers", "Back") == "T")
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NoSprinklers"));
					return false;
				}
				switch (base.ParentSheetIndex)
				{
				case 926:
					if (location.objects.ContainsKey(placementTile) || location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Torch(placementTile, 278, bigCraftable: true)
					{
						Fragility = 1,
						destroyOvernight = true
					});
					Utility.addSmokePuff(location, new Vector2(x, y));
					Utility.addSmokePuff(location, new Vector2(x + 16, y + 16));
					Utility.addSmokePuff(location, new Vector2(x + 32, y));
					Utility.addSmokePuff(location, new Vector2(x + 48, y + 16));
					Utility.addSmokePuff(location, new Vector2(x + 32, y + 32));
					Game1.playSound("fireball");
					return true;
				case 292:
				case 309:
				case 310:
				case 311:
				case 891:
				{
					if (!canPlaceWildTreeSeed(location, placementTile))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13021"));
						return false;
					}
					Vector2 key = default(Vector2);
					for (int i = x / 64 - 2; i <= x / 64 + 2; i++)
					{
						for (int j = y / 64 - 2; j <= y / 64 + 2; j++)
						{
							key.X = i;
							key.Y = j;
							if (location.terrainFeatures.ContainsKey(key) && location.terrainFeatures[key] is FruitTree)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060_Fruit"));
								return false;
							}
						}
					}
					int which = 1;
					switch ((int)parentSheetIndex)
					{
					case 292:
						which = 8;
						break;
					case 310:
						which = 2;
						break;
					case 311:
						which = 3;
						break;
					case 891:
						which = 7;
						break;
					}
					location.terrainFeatures.Remove(placementTile);
					location.terrainFeatures.Add(placementTile, new Tree(which, 0));
					location.playSound("dirtyHit");
					return true;
				}
				case 286:
				{
					foreach (TemporaryAnimatedSprite temporarySprite in location.temporarySprites)
					{
						if (temporarySprite.position.Equals(placementTile * 64f))
						{
							return false;
						}
					}
					int num = Game1.random.Next();
					location.playSound("thudStep");
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(parentSheetIndex, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
					{
						shakeIntensity = 0.5f,
						shakeIntensityChange = 0.002f,
						extraInfoForEndBehavior = num,
						endFunction = location.removeTemporarySpritesWithID
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 3f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
					{
						id = num
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 3f) * 4f, flicker: true, flipped: true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 100,
						id = num
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 3f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 200,
						id = num
					});
					location.netAudio.StartPlaying("fuse");
					return true;
				}
				case 287:
				{
					foreach (TemporaryAnimatedSprite temporarySprite2 in location.temporarySprites)
					{
						if (temporarySprite2.position.Equals(placementTile * 64f))
						{
							return false;
						}
					}
					int num = Game1.random.Next();
					location.playSound("thudStep");
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(parentSheetIndex, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
					{
						shakeIntensity = 0.5f,
						shakeIntensityChange = 0.002f,
						extraInfoForEndBehavior = num,
						endFunction = location.removeTemporarySpritesWithID
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
					{
						id = num
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 100,
						id = num
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 200,
						id = num
					});
					location.netAudio.StartPlaying("fuse");
					return true;
				}
				case 288:
				{
					foreach (TemporaryAnimatedSprite temporarySprite3 in location.temporarySprites)
					{
						if (temporarySprite3.position.Equals(placementTile * 64f))
						{
							return false;
						}
					}
					int num = Game1.random.Next();
					location.playSound("thudStep");
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(parentSheetIndex, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
					{
						shakeIntensity = 0.5f,
						shakeIntensityChange = 0.002f,
						extraInfoForEndBehavior = num,
						endFunction = location.removeTemporarySpritesWithID
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
					{
						id = num
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 100,
						id = num
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 200,
						id = num
					});
					location.netAudio.StartPlaying("fuse");
					return true;
				}
				case 297:
					if (location.objects.ContainsKey(placementTile) || location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Grass(1, 4));
					location.playSound("dirtyHit");
					return true;
				case 298:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Fence(placementTile, 5, isGate: false));
					location.playSound("axe");
					return true;
				case 322:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Fence(placementTile, 1, isGate: false));
					location.playSound("axe");
					return true;
				case 323:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Fence(placementTile, 2, isGate: false));
					location.playSound("stoneStep");
					return true;
				case 324:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Fence(placementTile, 3, isGate: false));
					location.playSound("hammer");
					return true;
				case 325:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Fence(placementTile, 4, isGate: true));
					location.playSound("axe");
					return true;
				case 328:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(0));
					location.playSound("axchop");
					return true;
				case 329:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(1));
					location.playSound("thudStep");
					return true;
				case 331:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(2));
					location.playSound("axchop");
					return true;
				case 333:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(3));
					location.playSound("thudStep");
					return true;
				case 401:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(4));
					location.playSound("thudStep");
					return true;
				case 293:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(10));
					location.playSound("thudStep");
					return true;
				case 405:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(6));
					location.playSound("woodyStep");
					return true;
				case 407:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(5));
					location.playSound("dirtyHit");
					return true;
				case 409:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(7));
					location.playSound("stoneStep");
					return true;
				case 415:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(9));
					location.playSound("stoneStep");
					return true;
				case 411:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(8));
					location.playSound("stoneStep");
					return true;
				case 840:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(11));
					location.playSound("stoneStep");
					return true;
				case 841:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(12));
					location.playSound("stoneStep");
					return true;
				case 93:
				{
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.removeLightSource((int)(tileLocation.X * 2000f + tileLocation.Y));
					location.removeLightSource((int)(long)Game1.player.uniqueMultiplayerID);
					Torch torch2 = new Torch(placementTile, 1);
					torch2.placementAction(location, x, y, (who == null) ? Game1.player : who);
					return true;
				}
				case 94:
				{
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					Torch torch = new Torch(placementTile, 1, 94);
					torch.placementAction(location, x, y, who);
					return true;
				}
				case 710:
				{
					if (!CrabPot.IsValidCrabPotLocationTile(location, (int)placementTile.X, (int)placementTile.Y))
					{
						return false;
					}
					CrabPot crabPot = new CrabPot(placementTile);
					crabPot.placementAction(location, x, y, who);
					return true;
				}
				case 805:
					if (location.terrainFeatures.ContainsKey(placementTile) && location.terrainFeatures[placementTile] is Tree)
					{
						return (location.terrainFeatures[placementTile] as Tree).fertilize(location);
					}
					return false;
				}
			}
			else
			{
				switch (base.ParentSheetIndex)
				{
				case 37:
				case 38:
				case 39:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Sign(placementTile, base.ParentSheetIndex));
					location.playSound("axe");
					return true;
				case 62:
					location.objects.Add(placementTile, new IndoorPot(placementTile));
					break;
				case 71:
					if (location is MineShaft)
					{
						if ((location as MineShaft).shouldCreateLadderOnThisLevel() && (location as MineShaft).recursiveTryToCreateLadderDown(placementTile))
						{
							MineShaft.numberOfCraftedStairsUsedThisRun++;
							return true;
						}
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					}
					return false;
				case 130:
				case 232:
					if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					location.objects.Add(placementTile, new Chest(playerChest: true, placementTile, parentSheetIndex)
					{
						shakeTimer = 50
					});
					location.playSound(((int)parentSheetIndex == 130) ? "axe" : "hammer");
					return true;
				case 163:
					location.objects.Add(placementTile, new Cask(placementTile));
					location.playSound("hammer");
					break;
				case 165:
				{
					Object @object = new Object(placementTile, 165);
					@object.heldObject.Value = new Chest();
					location.objects.Add(placementTile, @object);
					location.playSound("axe");
					return true;
				}
				case 208:
					location.objects.Add(placementTile, new Workbench(placementTile));
					location.playSound("axe");
					return true;
				case 209:
				{
					MiniJukebox miniJukebox = this as MiniJukebox;
					if (miniJukebox == null)
					{
						miniJukebox = new MiniJukebox(placementTile);
					}
					location.objects.Add(placementTile, miniJukebox);
					miniJukebox.RegisterToLocation(location);
					location.playSound("hammer");
					return true;
				}
				case 211:
				{
					WoodChipper woodChipper = this as WoodChipper;
					if (woodChipper == null)
					{
						woodChipper = new WoodChipper(placementTile);
					}
					woodChipper.placementAction(location, x, y);
					location.objects.Add(placementTile, woodChipper);
					location.playSound("hammer");
					return true;
				}
				case 214:
				{
					Phone phone = this as Phone;
					if (phone == null)
					{
						phone = new Phone(placementTile);
					}
					location.objects.Add(placementTile, phone);
					location.playSound("hammer");
					return true;
				}
				case 216:
				{
					if (location.objects.ContainsKey(placementTile))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					if (!(location is FarmHouse) && !(location is IslandFarmHouse))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					if (location is FarmHouse && (location as FarmHouse).upgradeLevel < 1)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:MiniFridge_NoKitchen"));
						return false;
					}
					Chest chest3 = new Chest(216, placementTile, 217, 2)
					{
						shakeTimer = 50
					};
					chest3.fridge.Value = true;
					location.objects.Add(placementTile, chest3);
					location.playSound("hammer");
					return true;
				}
				case 143:
				case 144:
				case 145:
				case 146:
				case 147:
				case 148:
				case 149:
				case 150:
				case 151:
				{
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					Torch torch3 = new Torch(placementTile, parentSheetIndex, bigCraftable: true)
					{
						shakeTimer = 25
					};
					torch3.placementAction(location, x, y, who);
					return true;
				}
				case 105:
				case 264:
					if (location.terrainFeatures.ContainsKey(placementTile) && location.terrainFeatures[placementTile] is Tree)
					{
						Tree tree = location.terrainFeatures[placementTile] as Tree;
						if ((int)tree.growthStage >= 5 && !tree.stump && !location.objects.ContainsKey(placementTile))
						{
							Object object2 = (Object)getOne();
							object2.heldObject.Value = null;
							object2.tileLocation.Value = placementTile;
							location.objects.Add(placementTile, object2);
							tree.tapped.Value = true;
							tree.UpdateTapperProduct(object2);
							location.playSound("axe");
							return true;
						}
					}
					return false;
				case 248:
					if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					location.objects.Add(placementTile, new Chest(playerChest: true, placementTile, parentSheetIndex)
					{
						shakeTimer = 50,
						SpecialChestType = Chest.SpecialChestTypes.MiniShippingBin
					});
					location.playSound("axe");
					return true;
				case 238:
				{
					if (!(location is Farm))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:OnlyPlaceOnFarm"));
						return false;
					}
					Vector2 vector = Vector2.Zero;
					Vector2 vector2 = Vector2.Zero;
					foreach (KeyValuePair<Vector2, Object> pair in location.objects.Pairs)
					{
						if ((bool)pair.Value.bigCraftable && pair.Value.ParentSheetIndex == 238)
						{
							if (vector.Equals(Vector2.Zero))
							{
								vector = pair.Key;
							}
							else if (vector2.Equals(Vector2.Zero))
							{
								vector2 = pair.Key;
								break;
							}
						}
					}
					if (!vector.Equals(Vector2.Zero) && !vector2.Equals(Vector2.Zero))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:OnlyPlaceTwo"));
						return false;
					}
					break;
				}
				case 254:
					if (!(location is AnimalHouse) || !(location as AnimalHouse).name.Contains("Barn"))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MustBePlacedInBarn"));
						return false;
					}
					break;
				case 256:
					if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					location.objects.Add(placementTile, new Chest(playerChest: true, placementTile, parentSheetIndex)
					{
						shakeTimer = 50,
						SpecialChestType = Chest.SpecialChestTypes.JunimoChest
					});
					location.playSound("axe");
					return true;
				case 275:
				{
					if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					Chest chest2 = new Chest(playerChest: true, placementTile, parentSheetIndex)
					{
						shakeTimer = 50,
						SpecialChestType = Chest.SpecialChestTypes.AutoLoader
					};
					chest2.lidFrameCount.Value = 2;
					location.objects.Add(placementTile, chest2);
					location.playSound("axe");
					return true;
				}
				}
			}
			if (base.Category == -19 && location.terrainFeatures.ContainsKey(placementTile) && location.terrainFeatures[placementTile] is HoeDirt && (location.terrainFeatures[placementTile] as HoeDirt).crop != null && (base.ParentSheetIndex == 369 || base.ParentSheetIndex == 368) && (int)(location.terrainFeatures[placementTile] as HoeDirt).crop.currentPhase != 0)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916"));
				return false;
			}
			if (isSapling())
			{
				if ((int)parentSheetIndex != 251)
				{
					Vector2 key2 = default(Vector2);
					for (int k = x / 64 - 2; k <= x / 64 + 2; k++)
					{
						for (int l = y / 64 - 2; l <= y / 64 + 2; l++)
						{
							key2.X = k;
							key2.Y = l;
							if (location.terrainFeatures.ContainsKey(key2) && (location.terrainFeatures[key2] is Tree || location.terrainFeatures[key2] is FruitTree))
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060"));
								return false;
							}
						}
					}
					if (FruitTree.IsGrowthBlocked(new Vector2(x / 64, y / 64), location))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:FruitTree_PlacementWarning", DisplayName));
						return false;
					}
				}
				if (location.terrainFeatures.ContainsKey(placementTile))
				{
					if (!(location.terrainFeatures[placementTile] is HoeDirt) || (location.terrainFeatures[placementTile] as HoeDirt).crop != null)
					{
						return false;
					}
					location.terrainFeatures.Remove(placementTile);
				}
				if ((location is Farm && (location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Diggable", "Back") != null || location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Grass") || location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Dirt")) && !location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "NoSpawn", "Back").Equals("Tree")) || (location.CanPlantTreesHere(parentSheetIndex, (int)placementTile.X, (int)placementTile.Y) && (location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Diggable", "Back") != null || location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Stone"))))
				{
					location.playSound("dirtyHit");
					DelayedAction.playSoundAfterDelay("coin", 100);
					if ((int)parentSheetIndex == 251)
					{
						location.terrainFeatures.Add(placementTile, new Bush(placementTile, 3, location));
						return true;
					}
					bool greenHouseTree = location.IsGreenhouse || (((int)parentSheetIndex == 69 || (int)parentSheetIndex == 835) && location is IslandWest);
					location.terrainFeatures.Add(placementTile, new FruitTree(parentSheetIndex)
					{
						GreenHouseTree = greenHouseTree,
						GreenHouseTileTree = location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Stone")
					});
					return true;
				}
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13068"));
				return false;
			}
			if (base.Category == -74 || base.Category == -19)
			{
				if (location.terrainFeatures.ContainsKey(placementTile) && location.terrainFeatures[placementTile] is HoeDirt)
				{
					if (((HoeDirt)location.terrainFeatures[placementTile]).canPlantThisSeedHere(who.ActiveObject.ParentSheetIndex, (int)placementTile.X, (int)placementTile.Y, who.ActiveObject.Category == -19))
					{
						if (((HoeDirt)location.terrainFeatures[placementTile]).plant(who.ActiveObject.ParentSheetIndex, (int)placementTile.X, (int)placementTile.Y, who, who.ActiveObject.Category == -19, location) && who.IsLocalPlayer)
						{
							if (base.Category == -74)
							{
								foreach (Object value in location.Objects.Values)
								{
									if (!value.IsSprinkler() || value.heldObject.Value == null || value.heldObject.Value.ParentSheetIndex != 913 || !value.IsInSprinklerRangeBroadphase(placementTile) || !value.GetSprinklerTiles().Contains(placementTile))
									{
										continue;
									}
									Chest chest = value.heldObject.Value.heldObject.Value as Chest;
									if (chest == null || chest.items.Count <= 0 || chest.items[0] == null || chest.GetMutex().IsLocked())
									{
										continue;
									}
									chest.GetMutex().RequestLock(delegate
									{
										if (chest.items.Count > 0 && chest.items[0] != null)
										{
											Item item = chest.items[0];
											if (item.Category == -19 && ((HoeDirt)location.terrainFeatures[placementTile]).plant(item.ParentSheetIndex, (int)placementTile.X, (int)placementTile.Y, who, isFertilizer: true, location))
											{
												item.Stack--;
												if (item.Stack <= 0)
												{
													chest.items[0] = null;
												}
											}
										}
										chest.GetMutex().ReleaseLock();
									});
									break;
								}
							}
							Game1.haltAfterCheck = false;
							return true;
						}
						return false;
					}
					return false;
				}
				return false;
			}
			if (!performDropDownAction(who))
			{
				Object object3 = (Object)getOne();
				bool flag = false;
				if (object3.GetType() == typeof(Furniture))
				{
					Furniture furnitureInstance = Furniture.GetFurnitureInstance(parentSheetIndex, new Vector2(x / 64, y / 64));
					if (furnitureInstance.GetType() != object3.GetType())
					{
						object3 = new StorageFurniture(parentSheetIndex, new Vector2(x / 64, y / 64));
						(object3 as Furniture).currentRotation.Value = (this as Furniture).currentRotation.Value;
						(object3 as Furniture).updateRotation();
						flag = true;
					}
				}
				object3.shakeTimer = 50;
				object3.tileLocation.Value = placementTile;
				object3.performDropDownAction(who);
				if (object3.name.Contains("Seasonal"))
				{
					int num2 = object3.ParentSheetIndex - object3.ParentSheetIndex % 4;
					object3.ParentSheetIndex = num2 + Utility.getSeasonNumber(Game1.currentSeason);
				}
				if (location.objects.ContainsKey(placementTile))
				{
					if (location.objects[placementTile].ParentSheetIndex != (int)parentSheetIndex)
					{
						Game1.createItemDebris(location.objects[placementTile], placementTile * 64f, Game1.random.Next(4));
						location.objects[placementTile] = object3;
					}
				}
				else if (object3 is Furniture)
				{
					if (flag)
					{
						location.furniture.Add(object3 as Furniture);
					}
					else
					{
						location.furniture.Add(this as Furniture);
					}
				}
				else
				{
					location.objects.Add(placementTile, object3);
				}
				object3.initializeLightSource(placementTile);
			}
			location.playSound("woodyStep");
			return true;
		}

		public override bool actionWhenPurchased()
		{
			if (type.Value != null && type.Contains("Blueprint"))
			{
				string item = name.Substring(name.IndexOf(' ') + 1);
				if (!Game1.player.blueprints.Contains(name))
				{
					Game1.player.blueprints.Add(item);
				}
				return true;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 434))
			{
				if (!Game1.isFestival())
				{
					Game1.player.mailReceived.Add("CF_Sewer");
				}
				else
				{
					Game1.player.mailReceived.Add("CF_Fair");
				}
				Game1.exitActiveMenu();
				Game1.player.eatObject(this, overrideFullness: true);
				return true;
			}
			if (base.actionWhenPurchased())
			{
				return true;
			}
			return isRecipe;
		}

		public override bool canBePlacedInWater()
		{
			return (int)parentSheetIndex == 710;
		}

		public virtual bool needsToBeDonated()
		{
			if (!bigCraftable && type != null && (type.Equals("Minerals") || type.Equals("Arch")))
			{
				return !(Game1.getLocationFromName("ArchaeologyHouse") as LibraryMuseum).museumAlreadyHasArtifact(parentSheetIndex);
			}
			return false;
		}

		public override string getDescription()
		{
			if ((bool)isRecipe)
			{
				if (base.Category == -7)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13073", loadDisplayName());
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13074", loadDisplayName());
			}
			if (needsToBeDonated())
			{
				return Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13078"), Game1.smallFont, getDescriptionWidth());
			}
			if ((bool)bigCraftable && !Game1.bigCraftablesInformation.ContainsKey(parentSheetIndex))
			{
				return "";
			}
			return Game1.parseText(bigCraftable ? Game1.bigCraftablesInformation[parentSheetIndex].Split('/')[4] : (Game1.objectInformation.ContainsKey(parentSheetIndex) ? Game1.objectInformation[parentSheetIndex].Split('/')[5] : "???"), Game1.smallFont, getDescriptionWidth());
		}

		public virtual int sellToStorePrice(long specificPlayerID = -1L)
		{
			if (this is Fence)
			{
				return price;
			}
			if (base.Category == -22)
			{
				return (int)((float)(int)price * (1f + (float)(int)quality * 0.25f) * (((float)(FishingRod.maxTackleUses - uses.Value) + 0f) / (float)FishingRod.maxTackleUses));
			}
			float startPrice = (int)((float)(int)price * (1f + (float)Quality * 0.25f));
			startPrice = getPriceAfterMultipliers(startPrice, specificPlayerID);
			if ((int)parentSheetIndex == 493)
			{
				startPrice /= 2f;
			}
			if (startPrice > 0f)
			{
				startPrice = Math.Max(1f, startPrice * Game1.MasterPlayer.difficultyModifier);
			}
			return (int)startPrice;
		}

		public override int salePrice()
		{
			if (this is Fence)
			{
				return price;
			}
			if ((bool)isRecipe)
			{
				return (int)price * 10;
			}
			switch ((int)parentSheetIndex)
			{
			case 388:
				if (Game1.year <= 1)
				{
					return 10;
				}
				return 50;
			case 390:
				if (Game1.year <= 1)
				{
					return 20;
				}
				return 100;
			case 382:
				if (Game1.year <= 1)
				{
					return 120;
				}
				return 250;
			case 378:
				if (Game1.year <= 1)
				{
					return 80;
				}
				return 160;
			case 380:
				if (Game1.year <= 1)
				{
					return 150;
				}
				return 250;
			case 384:
				if (Game1.year <= 1)
				{
					return 350;
				}
				return 750;
			default:
			{
				float num = (int)((float)((int)price * 2) * (1f + (float)(int)quality * 0.25f));
				if ((int)category == -74 || isSapling())
				{
					num = (int)Math.Max(1f, num * Game1.MasterPlayer.difficultyModifier);
				}
				return (int)num;
			}
			}
		}

		protected virtual float getPriceAfterMultipliers(float startPrice, long specificPlayerID = -1L)
		{
			bool flag = false;
			if (name != null && (name.ToLower().Contains("mayonnaise") || name.ToLower().Contains("cheese") || name.ToLower().Contains("cloth") || name.ToLower().Contains("wool")))
			{
				flag = true;
			}
			float num = 1f;
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (Game1.player.useSeparateWallets)
				{
					if (specificPlayerID == -1)
					{
						if (allFarmer.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID || !allFarmer.isActive())
						{
							continue;
						}
					}
					else if (allFarmer.UniqueMultiplayerID != specificPlayerID)
					{
						continue;
					}
				}
				else if (!allFarmer.isActive())
				{
					continue;
				}
				float num2 = 1f;
				if (allFarmer.professions.Contains(0) && (flag || base.Category == -5 || base.Category == -6 || base.Category == -18))
				{
					num2 *= 1.2f;
				}
				if (allFarmer.professions.Contains(1) && (base.Category == -75 || base.Category == -80 || (base.Category == -79 && !isSpawnedObject)))
				{
					num2 *= 1.1f;
				}
				if (allFarmer.professions.Contains(4) && base.Category == -26)
				{
					num2 *= 1.4f;
				}
				if (allFarmer.professions.Contains(6) && base.Category == -4)
				{
					num2 *= (allFarmer.professions.Contains(8) ? 1.5f : 1.25f);
				}
				if (allFarmer.professions.Contains(12) && (int)parentSheetIndex != 388)
				{
					_ = (int)parentSheetIndex;
					_ = 709;
				}
				if (allFarmer.professions.Contains(15) && base.Category == -27)
				{
					num2 *= 1.25f;
				}
				if (allFarmer.professions.Contains(20) && (((int)parentSheetIndex >= 334 && (int)parentSheetIndex <= 337) || (int)parentSheetIndex == 910))
				{
					num2 *= 1.5f;
				}
				if (allFarmer.professions.Contains(23) && (base.Category == -2 || base.Category == -12))
				{
					num2 *= 1.3f;
				}
				if (allFarmer.eventsSeen.Contains(2120303) && ((int)parentSheetIndex == 296 || (int)parentSheetIndex == 410))
				{
					num2 *= 3f;
				}
				if (allFarmer.eventsSeen.Contains(3910979) && (int)parentSheetIndex == 399)
				{
					num2 *= 5f;
				}
				num = Math.Max(num, num2);
			}
			return startPrice * num;
		}

		public void ClearRedGreenSquareDict()
		{
			_redGreenSquareDict.Clear();
		}

		public bool DrawRedGreenRectangleForPlacing(SpriteBatch spriteBatch, GameLocation location)
		{
			if (Game1.currentLocation.tapToMove.furniture != null)
			{
				return false;
			}
			if (Game1.options.weaponControl == 8 || (Game1.options.weaponControl == 4 && !Game1.currentLocation.tapToMove.TapHoldActive) || Game1.options.weaponControl == 2 || Game1.options.weaponControl == 3)
			{
				Vector2 grabTile = VirtualJoypad.GrabTile;
				int x = (int)grabTile.X * 64;
				int y = (int)grabTile.Y * 64;
				bool flag = Utility.playerCanPlaceItemHere(location, this, x, y, Game1.player) || (Utility.isThereAnObjectHereWhichAcceptsThisItem(location, this, x, y) && Utility.withinRadiusOfPlayer(x, y, 1, Game1.player));
				Game1.isCheckingNonMousePlacement = false;
				int num = boundingBox.Width / 64;
				int num2 = boundingBox.Height / 64;
				for (int i = (int)grabTile.X; i < (int)grabTile.X + num; i++)
				{
					for (int j = (int)grabTile.Y; j < (int)grabTile.Y + num2; j++)
					{
						spriteBatch.Draw(Game1.mouseCursors, new Vector2(i * 64 - Game1.viewport.X, j * 64 - Game1.viewport.Y), new Microsoft.Xna.Framework.Rectangle(flag ? 194 : 210, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
					}
				}
				return true;
			}
			return false;
		}
	}
}
