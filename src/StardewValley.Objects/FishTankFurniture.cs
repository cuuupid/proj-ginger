using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Objects
{
	public class FishTankFurniture : StorageFurniture
	{
		public enum FishTankCategories
		{
			None,
			Swim,
			Ground,
			Decoration
		}

		public const int TANK_DEPTH = 10;

		public const int FLOOR_DECORATION_OFFSET = 4;

		public const int TANK_SORT_REGION = 20;

		[XmlIgnore]
		public List<Vector4> bubbles = new List<Vector4>();

		[XmlIgnore]
		public List<TankFish> tankFish = new List<TankFish>();

		[XmlIgnore]
		public NetEvent0 refreshFishEvent = new NetEvent0();

		[XmlIgnore]
		public bool fishDirty = true;

		[XmlIgnore]
		private Texture2D _aquariumTexture;

		[XmlIgnore]
		public List<KeyValuePair<Rectangle, Vector2>?> floorDecorations = new List<KeyValuePair<Rectangle, Vector2>?>();

		[XmlIgnore]
		public List<Vector2> decorationSlots = new List<Vector2>();

		[XmlIgnore]
		public List<int> floorDecorationIndices = new List<int>();

		public NetInt generationSeed = new NetInt();

		[XmlIgnore]
		public Item localDepositedItem;

		[XmlIgnore]
		protected int _currentDecorationIndex;

		protected Dictionary<Item, TankFish> _fishLookup = new Dictionary<Item, TankFish>();

		public FishTankFurniture()
		{
			generationSeed.Value = Game1.random.Next();
		}

		public FishTankFurniture(int which, Vector2 tile, int initialRotations)
			: base(which, tile, initialRotations)
		{
			generationSeed.Value = Game1.random.Next();
		}

		public FishTankFurniture(int which, Vector2 tile)
			: base(which, tile)
		{
			generationSeed.Value = Game1.random.Next();
		}

		public override void resetOnPlayerEntry(GameLocation environment, bool dropDown = false)
		{
			base.resetOnPlayerEntry(environment, dropDown);
			ResetFish();
			UpdateFish();
		}

		public virtual void ResetFish()
		{
			bubbles.Clear();
			tankFish.Clear();
			_fishLookup.Clear();
			UpdateFish();
		}

		public Texture2D GetAquariumTexture()
		{
			if (_aquariumTexture == null)
			{
				_aquariumTexture = Game1.content.Load<Texture2D>("LooseSprites\\AquariumFish");
			}
			return _aquariumTexture;
		}

		protected override void initNetFields()
		{
			base.NetFields.AddFields(generationSeed, refreshFishEvent);
			refreshFishEvent.onEvent += UpdateDecorAndFish;
			base.initNetFields();
		}

		public override Item getOne()
		{
			FishTankFurniture fishTankFurniture = new FishTankFurniture(parentSheetIndex, tileLocation);
			fishTankFurniture.drawPosition.Value = drawPosition;
			fishTankFurniture.defaultBoundingBox.Value = defaultBoundingBox;
			fishTankFurniture.boundingBox.Value = boundingBox;
			fishTankFurniture.currentRotation.Value = (int)currentRotation - 1;
			fishTankFurniture.isOn.Value = false;
			fishTankFurniture.rotations.Value = rotations;
			fishTankFurniture.rotate();
			fishTankFurniture._GetOneFrom(this);
			return fishTankFurniture;
		}

		public virtual int GetCapacityForCategory(FishTankCategories category)
		{
			int tilesWide = getTilesWide();
			switch (category)
			{
			case FishTankCategories.Swim:
				return tilesWide - 1;
			case FishTankCategories.Ground:
				return tilesWide - 1;
			case FishTankCategories.Decoration:
				if (tilesWide <= 2)
				{
					return 1;
				}
				return -1;
			default:
				return 0;
			}
		}

		public FishTankCategories GetCategoryFromItem(Item item)
		{
			Dictionary<int, string> aquariumData = GetAquariumData();
			if (!CanBeDeposited(item))
			{
				return FishTankCategories.None;
			}
			if (aquariumData.ContainsKey(item.ParentSheetIndex))
			{
				string text = aquariumData[item.ParentSheetIndex];
				string[] array = text.Split('/');
				switch (array[1])
				{
				case "crawl":
				case "ground":
				case "front_crawl":
				case "static":
					return FishTankCategories.Ground;
				default:
					return FishTankCategories.Swim;
				}
			}
			return FishTankCategories.Decoration;
		}

		public bool HasRoomForThisItem(Item item)
		{
			if (!CanBeDeposited(item))
			{
				return false;
			}
			FishTankCategories categoryFromItem = GetCategoryFromItem(item);
			int num = GetCapacityForCategory(categoryFromItem);
			if (item is Hat)
			{
				num = 999;
			}
			if (num < 0)
			{
				foreach (Item heldItem in heldItems)
				{
					if (heldItem != null && heldItem.ParentSheetIndex == item.ParentSheetIndex)
					{
						return false;
					}
				}
				return true;
			}
			int num2 = 0;
			foreach (Item heldItem2 in heldItems)
			{
				if (heldItem2 != null)
				{
					if (GetCategoryFromItem(heldItem2) == categoryFromItem)
					{
						num2++;
					}
					if (num2 >= num)
					{
						return false;
					}
				}
			}
			return true;
		}

		public override string GetShopMenuContext()
		{
			return "FishTank";
		}

		public override void ShowMenu()
		{
			ShowShopMenu();
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if (mutex.IsLocked())
			{
				return true;
			}
			if ((who.ActiveObject != null || (who.CurrentItem != null && who.CurrentItem is Hat)) && localDepositedItem == null && CanBeDeposited(who.CurrentItem))
			{
				if (!HasRoomForThisItem(who.CurrentItem))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishTank_Full"));
					return true;
				}
				GameLocation location = who.currentLocation;
				localDepositedItem = who.CurrentItem.getOne();
				who.CurrentItem.Stack--;
				if (who.CurrentItem.Stack <= 0 || who.CurrentItem is Hat)
				{
					who.removeItemFromInventory(who.CurrentItem);
					who.showNotCarrying();
				}
				mutex.RequestLock(delegate
				{
					location.playSound("dropItemInWater");
					heldItems.Add(localDepositedItem);
					localDepositedItem = null;
					refreshFishEvent.Fire();
					mutex.ReleaseLock();
				}, delegate
				{
					localDepositedItem = who.addItemToInventory(localDepositedItem);
					if (localDepositedItem != null)
					{
						Game1.createItemDebris(localDepositedItem, new Vector2(base.TileLocation.X + (float)getTilesWide() / 2f + 0.5f, base.TileLocation.Y + 0.5f) * 64f, -1, location);
					}
					localDepositedItem = null;
				});
				return true;
			}
			mutex.RequestLock(delegate
			{
				ShowMenu();
			});
			return true;
		}

		public virtual bool CanBeDeposited(Item item)
		{
			if (item == null)
			{
				return false;
			}
			if (!(item is Hat) && !Utility.IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex))
			{
				return false;
			}
			if (item.ParentSheetIndex == 152 || item.ParentSheetIndex == 393 || item.ParentSheetIndex == 390)
			{
				return true;
			}
			if (item is Hat)
			{
				int num = 0;
				int num2 = 0;
				foreach (Item heldItem in heldItems)
				{
					if (heldItem is Hat)
					{
						num2++;
					}
					else if (heldItem is Object && (int)heldItem.parentSheetIndex == 397)
					{
						num++;
					}
				}
				if (num2 < num)
				{
					return true;
				}
				return false;
			}
			if (!GetAquariumData().ContainsKey(item.ParentSheetIndex))
			{
				return false;
			}
			return true;
		}

		public override void DayUpdate(GameLocation location)
		{
			ResetFish();
			base.DayUpdate(location);
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			if (Game1.currentLocation == environment)
			{
				if (fishDirty)
				{
					fishDirty = false;
					UpdateDecorAndFish();
				}
				foreach (TankFish item in tankFish)
				{
					item.Update(time);
				}
				for (int i = 0; i < bubbles.Count; i++)
				{
					Vector4 value = bubbles[i];
					value.W += 0.05f;
					if (value.W > 1f)
					{
						value.W = 1f;
					}
					value.Y += value.W;
					bubbles[i] = value;
					if (value.Y >= (float)GetTankBounds().Height)
					{
						bubbles.RemoveAt(i);
						i--;
					}
				}
			}
			base.updateWhenCurrentLocation(time, environment);
			refreshFishEvent.Poll();
		}

		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			generationSeed.Value = Game1.random.Next();
			fishDirty = true;
			return base.placementAction(location, x, y, who);
		}

		public Dictionary<int, string> GetAquariumData()
		{
			return Game1.content.Load<Dictionary<int, string>>("Data\\AquariumFish");
		}

		public override bool onDresserItemWithdrawn(ISalable salable, Farmer who, int amount)
		{
			bool result = base.onDresserItemWithdrawn(salable, who, amount);
			refreshFishEvent.Fire();
			return result;
		}

		public virtual void UpdateFish()
		{
			List<Item> list = new List<Item>();
			Dictionary<int, string> aquariumData = GetAquariumData();
			foreach (Item heldItem in heldItems)
			{
				if (heldItem != null && Utility.IsNormalObjectAtParentSheetIndex(heldItem, heldItem.ParentSheetIndex) && aquariumData.ContainsKey(heldItem.ParentSheetIndex))
				{
					list.Add(heldItem);
				}
			}
			List<Item> list2 = new List<Item>();
			foreach (Item key in _fishLookup.Keys)
			{
				if (!heldItems.Contains(key))
				{
					list2.Add(key);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				Item item = list[i];
				if (!_fishLookup.ContainsKey(item))
				{
					TankFish tankFish = new TankFish(this, item);
					this.tankFish.Add(tankFish);
					_fishLookup[item] = tankFish;
				}
			}
			foreach (Item item2 in list2)
			{
				this.tankFish.Remove(_fishLookup[item2]);
				heldItems.Remove(item2);
			}
		}

		public virtual void UpdateDecorAndFish()
		{
			Random random = new Random(generationSeed.Value);
			UpdateFish();
			decorationSlots.Clear();
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < getTilesWide(); j++)
				{
					Vector2 item = default(Vector2);
					if (i % 2 == 0)
					{
						if (j == getTilesWide() - 1)
						{
							continue;
						}
						item.X = 16 + j * 16;
					}
					else
					{
						item.X = 8 + j * 16;
					}
					item.Y = 4f;
					item.Y += 3.3333333f * (float)i;
					decorationSlots.Add(item);
				}
			}
			floorDecorationIndices.Clear();
			floorDecorations.Clear();
			_currentDecorationIndex = 0;
			for (int k = 0; k < decorationSlots.Count; k++)
			{
				floorDecorationIndices.Add(k);
				floorDecorations.Add(null);
			}
			Utility.Shuffle(random, floorDecorationIndices);
			Random random2 = new Random(random.Next());
			bool flag = GetItemCount(393) > 0;
			for (int l = 0; l < 1; l++)
			{
				if (flag)
				{
					AddFloorDecoration(new Rectangle(16 * random2.Next(0, 5), 256, 16, 16));
				}
				else
				{
					_AdvanceDecorationIndex();
				}
			}
			random2 = new Random(random.Next());
			bool flag2 = GetItemCount(152) > 0;
			for (int m = 0; m < 4; m++)
			{
				if (flag2)
				{
					AddFloorDecoration(new Rectangle(16 * random2.Next(0, 3), 288, 16, 16));
				}
				else
				{
					_AdvanceDecorationIndex();
				}
			}
			random2 = new Random(random.Next());
			bool flag3 = GetItemCount(390) > 0;
			for (int n = 0; n < 2; n++)
			{
				if (flag3)
				{
					AddFloorDecoration(new Rectangle(16 * random2.Next(0, 3), 272, 16, 16));
				}
				else
				{
					_AdvanceDecorationIndex();
				}
			}
		}

		public virtual void AddFloorDecoration(Rectangle source_rect)
		{
			if (_currentDecorationIndex != -1)
			{
				int index = floorDecorationIndices[_currentDecorationIndex];
				_AdvanceDecorationIndex();
				int num = (int)decorationSlots[index].X;
				int num2 = (int)decorationSlots[index].Y;
				if (num < source_rect.Width / 2)
				{
					num = source_rect.Width / 2;
				}
				if (num > GetTankBounds().Width / 4 - source_rect.Width / 2)
				{
					num = GetTankBounds().Width / 4 - source_rect.Width / 2;
				}
				KeyValuePair<Rectangle, Vector2> value = new KeyValuePair<Rectangle, Vector2>(source_rect, new Vector2(num, num2));
				floorDecorations[index] = value;
			}
		}

		protected virtual void _AdvanceDecorationIndex()
		{
			for (int i = 0; i < decorationSlots.Count; i++)
			{
				_currentDecorationIndex++;
				if (_currentDecorationIndex >= decorationSlots.Count)
				{
					_currentDecorationIndex = 0;
				}
				if (!floorDecorations[floorDecorationIndices[_currentDecorationIndex]].HasValue)
				{
					return;
				}
			}
			_currentDecorationIndex = 1;
		}

		public override void OnMenuClose()
		{
			refreshFishEvent.Fire();
			base.OnMenuClose();
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
		}

		public Vector2 GetFishSortRegion()
		{
			return new Vector2(GetBaseDrawLayer() + 1E-06f, GetGlassDrawLayer() - 1E-06f);
		}

		public float GetGlassDrawLayer()
		{
			return GetBaseDrawLayer() + 0.0001f;
		}

		public float GetBaseDrawLayer()
		{
			if ((int)furniture_type != 12)
			{
				return (float)(boundingBox.Value.Bottom - (((int)furniture_type == 6 || (int)furniture_type == 13) ? 48 : 8)) / 10000f;
			}
			return 2E-09f;
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			Vector2 vector = Vector2.Zero;
			if (isTemporarilyInvisible)
			{
				return;
			}
			Vector2 vector2 = drawPosition.Value;
			if (!Furniture.isDrawingLocationFurniture)
			{
				vector2 = new Vector2(x, y) * 64f;
				vector2.Y -= sourceRect.Height * 4 - boundingBox.Height;
			}
			if (shakeTimer > 0)
			{
				vector = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, vector2 + vector), new Rectangle(sourceRect.Value.X + sourceRect.Value.Width, sourceRect.Value.Y, sourceRect.Value.Width, sourceRect.Value.Height), Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, GetGlassDrawLayer());
			if (Furniture.isDrawingLocationFurniture)
			{
				int num = 0;
				for (int i = 0; i < this.tankFish.Count; i++)
				{
					TankFish tankFish = this.tankFish[i];
					float num2 = Utility.Lerp(GetFishSortRegion().Y, GetFishSortRegion().X, tankFish.zPosition / 20f);
					num2 += 1E-07f * (float)i;
					tankFish.Draw(spriteBatch, alpha, num2);
					if (tankFish.fishIndex != 86)
					{
						continue;
					}
					int num3 = 0;
					foreach (Item heldItem in heldItems)
					{
						if (heldItem is Hat)
						{
							if (num3 == num)
							{
								heldItem.drawInMenu(spriteBatch, Game1.GlobalToLocal(tankFish.GetWorldPosition() + new Vector2(-30 + (tankFish.facingLeft ? (-4) : 0), -55f)), 0.75f, 1f, num2 + 1E-08f, StackDrawType.Hide);
								num++;
								break;
							}
							num3++;
						}
					}
				}
				for (int j = 0; j < floorDecorations.Count; j++)
				{
					if (floorDecorations[j].HasValue)
					{
						KeyValuePair<Rectangle, Vector2> value = floorDecorations[j].Value;
						Vector2 value2 = value.Value;
						Rectangle key = value.Key;
						float layerDepth = Utility.Lerp(GetFishSortRegion().Y, GetFishSortRegion().X, value2.Y / 20f) - 1E-06f;
						spriteBatch.Draw(GetAquariumTexture(), Game1.GlobalToLocal(new Vector2((float)GetTankBounds().Left + value2.X * 4f, (float)(GetTankBounds().Bottom - 4) - value2.Y * 4f)), key, Color.White * alpha, 0f, new Vector2(key.Width / 2, key.Height - 4), 4f, SpriteEffects.None, layerDepth);
					}
				}
				foreach (Vector4 bubble in bubbles)
				{
					float layerDepth2 = Utility.Lerp(GetFishSortRegion().Y, GetFishSortRegion().X, bubble.Z / 20f) - 1E-06f;
					spriteBatch.Draw(GetAquariumTexture(), Game1.GlobalToLocal(new Vector2((float)GetTankBounds().Left + bubble.X, (float)(GetTankBounds().Bottom - 4) - bubble.Y - bubble.Z * 4f)), new Rectangle(0, 240, 16, 16), Color.White * alpha, 0f, new Vector2(8f, 8f), 4f * bubble.W, SpriteEffects.None, layerDepth2);
				}
			}
			base.draw(spriteBatch, x, y, alpha);
		}

		public int GetItemCount(int parent_sheet_index)
		{
			int num = 0;
			foreach (Item heldItem in heldItems)
			{
				if (Utility.IsNormalObjectAtParentSheetIndex(heldItem, parent_sheet_index))
				{
					num += heldItem.Stack;
				}
			}
			return num;
		}

		public virtual Rectangle GetTankBounds()
		{
			int num = defaultSourceRect.Value.Height / 16;
			int num2 = defaultSourceRect.Value.Width / 16;
			Rectangle result = new Rectangle((int)base.TileLocation.X * 64, (int)((base.TileLocation.Y - (float)getTilesHigh() - 1f) * 64f), num2 * 64, num * 64);
			result.X += 4;
			result.Width -= 8;
			result.Height -= 28;
			result.Y += 64;
			result.Height -= 64;
			return result;
		}
	}
}
