using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Objects
{
	[XmlInclude(typeof(MeleeWeapon))]
	public class Chest : Object
	{
		public enum SpecialChestTypes
		{
			None,
			MiniShippingBin,
			JunimoChest,
			AutoLoader,
			Enricher,
			Mill
		}

		public const int capacity = 36;

		[XmlElement("currentLidFrame")]
		public readonly NetInt startingLidFrame = new NetInt(501);

		public readonly NetInt lidFrameCount = new NetInt(5);

		private int currentLidFrame;

		[XmlElement("frameCounter")]
		public readonly NetInt frameCounter = new NetInt(-1);

		[XmlElement("coins")]
		public readonly NetInt coins = new NetInt();

		public readonly NetObjectList<Item> items = new NetObjectList<Item>();

		public readonly NetLongDictionary<NetObjectList<Item>, NetRef<NetObjectList<Item>>> separateWalletItems = new NetLongDictionary<NetObjectList<Item>, NetRef<NetObjectList<Item>>>();

		[XmlElement("chestType")]
		public readonly NetString chestType = new NetString("");

		[XmlElement("tint")]
		public readonly NetColor tint = new NetColor(Color.White);

		[XmlElement("playerChoiceColor")]
		public readonly NetColor playerChoiceColor = new NetColor(Color.Black);

		[XmlElement("playerChest")]
		public readonly NetBool playerChest = new NetBool();

		[XmlElement("fridge")]
		public readonly NetBool fridge = new NetBool();

		[XmlElement("giftbox")]
		public readonly NetBool giftbox = new NetBool();

		[XmlElement("giftboxIndex")]
		public readonly NetInt giftboxIndex = new NetInt();

		[XmlElement("spriteIndexOverride")]
		public readonly NetInt bigCraftableSpriteIndex = new NetInt(-1);

		[XmlElement("dropContents")]
		public readonly NetBool dropContents = new NetBool(value: false);

		[XmlElement("synchronized")]
		public readonly NetBool synchronized = new NetBool(value: false);

		[XmlIgnore]
		protected int _shippingBinFrameCounter;

		[XmlIgnore]
		protected bool _farmerNearby;

		[XmlIgnore]
		public NetVector2 kickStartTile = new NetVector2(new Vector2(-1000f, -1000f));

		[XmlIgnore]
		public Vector2? localKickStartTile;

		[XmlIgnore]
		public float kickProgress = -1f;

		[XmlIgnore]
		public readonly NetEvent0 openChestEvent = new NetEvent0();

		[XmlElement("specialChestType")]
		public readonly NetEnum<SpecialChestTypes> specialChestType = new NetEnum<SpecialChestTypes>();

		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		[XmlIgnore]
		public SpecialChestTypes SpecialChestType
		{
			get
			{
				return specialChestType.Value;
			}
			set
			{
				specialChestType.Value = value;
			}
		}

		[XmlIgnore]
		public Color Tint
		{
			get
			{
				return tint;
			}
			set
			{
				tint.Value = value;
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(startingLidFrame, frameCounter, coins, items, chestType, tint, playerChoiceColor, playerChest, fridge, giftbox, giftboxIndex, mutex.NetFields, lidFrameCount, bigCraftableSpriteIndex, dropContents, openChestEvent.NetFields, synchronized, specialChestType, kickStartTile, separateWalletItems);
			openChestEvent.onEvent += performOpenChest;
			kickStartTile.fieldChangeVisibleEvent += delegate(NetVector2 field, Vector2 old_value, Vector2 new_value)
			{
				if (Game1.gameMode != 6 && new_value.X != -1000f && new_value.Y != -1000f)
				{
					localKickStartTile = kickStartTile;
					kickProgress = 0f;
				}
			};
		}

		public Chest()
		{
			Name = "Chest";
			type.Value = "interactive";
			boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
			createSlotsForCapacity();
		}

		public Chest(bool playerChest, Vector2 tileLocation, int parentSheetIndex = 130)
			: base(tileLocation, parentSheetIndex)
		{
			Name = "Chest";
			type.Value = "Crafting";
			if (playerChest)
			{
				this.playerChest.Value = playerChest;
				startingLidFrame.Value = parentSheetIndex + 1;
				bigCraftable.Value = true;
				canBeSetDown.Value = true;
			}
			createSlotsForCapacity();
		}

		public Chest(bool playerChest, int parentSheedIndex = 130)
			: base(Vector2.Zero, parentSheedIndex)
		{
			Name = "Chest";
			type.Value = "Crafting";
			if (playerChest)
			{
				this.playerChest.Value = playerChest;
				startingLidFrame.Value = parentSheedIndex + 1;
				bigCraftable.Value = true;
				canBeSetDown.Value = true;
				createSlotsForCapacity();
			}
			else
			{
				lidFrameCount.Value = 3;
			}
		}

		public Chest(Vector2 location)
		{
			tileLocation.Value = location;
			base.name = "Chest";
			type.Value = "interactive";
			boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
			createSlotsForCapacity();
		}

		public Chest(string type, Vector2 location, MineShaft mine)
		{
			tileLocation.Value = location;
			switch (type)
			{
			case "OreChest":
			{
				for (int i = 0; i < 8; i++)
				{
					items.Add(new Object(tileLocation, (Game1.random.NextDouble() < 0.5) ? 384 : 382, 1));
				}
				break;
			}
			case "dungeon":
				switch ((int)location.X % 5)
				{
				case 1:
					coins.Value = (int)location.Y % 3 + 2;
					break;
				case 2:
					items.Add(new Object(tileLocation, 382, (int)location.Y % 3 + 1));
					break;
				case 3:
					items.Add(new Object(tileLocation, (mine.getMineArea() == 0) ? 378 : ((mine.getMineArea() == 40) ? 380 : 384), (int)location.Y % 3 + 1));
					break;
				case 4:
					chestType.Value = "Monster";
					break;
				}
				break;
			case "Grand":
				tint.Value = new Color(150, 150, 255);
				coins.Value = (int)location.Y % 8 + 6;
				break;
			}
			base.name = "Chest";
			lidFrameCount.Value = 3;
			base.type.Value = "interactive";
			boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
			createSlotsForCapacity();
		}

		public Chest(int parent_sheet_index, Vector2 tile_location, int starting_lid_frame, int lid_frame_count)
			: base(tile_location, parent_sheet_index)
		{
			playerChest.Value = true;
			startingLidFrame.Value = starting_lid_frame;
			lidFrameCount.Value = lid_frame_count;
			bigCraftable.Value = true;
			canBeSetDown.Value = true;
			createSlotsForCapacity();
		}

		public Chest(int coins, List<Item> items, Vector2 location, bool giftbox = false, int giftboxIndex = 0)
		{
			base.name = "Chest";
			type.Value = "interactive";
			this.giftbox.Value = giftbox;
			this.giftboxIndex.Value = giftboxIndex;
			if (!this.giftbox.Value)
			{
				lidFrameCount.Value = 3;
			}
			if (items != null)
			{
				this.items.Set(items);
			}
			this.coins.Value = coins;
			tileLocation.Value = location;
			boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
			createSlotsForCapacity();
		}

		public void createSlotsForCapacity(bool force = false)
		{
			if (!((bool)playerChest || force))
			{
				return;
			}
			for (int i = 0; i < 36; i++)
			{
				if (items.Count < 36)
				{
					items.Add(null);
				}
			}
		}

		public int itemsCountExcludingNulls()
		{
			int num = 0;
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i] != null)
				{
					num++;
				}
			}
			return num;
		}

		public void resetLidFrame()
		{
			currentLidFrame = startingLidFrame;
		}

		public void fixLidFrame()
		{
			if (currentLidFrame == 0)
			{
				currentLidFrame = startingLidFrame;
			}
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
			{
				return;
			}
			if ((bool)playerChest)
			{
				if (GetMutex().IsLocked() && !GetMutex().IsLockHeld())
				{
					currentLidFrame = getLastLidFrame();
				}
				else if (!GetMutex().IsLocked())
				{
					currentLidFrame = startingLidFrame;
				}
			}
			else if (currentLidFrame == startingLidFrame.Value && GetMutex().IsLocked() && !GetMutex().IsLockHeld())
			{
				currentLidFrame = getLastLidFrame();
			}
		}

		public int getLastLidFrame()
		{
			return startingLidFrame.Value + lidFrameCount.Value - 1;
		}

		public override bool performObjectDropInAction(Item dropIn, bool probe, Farmer who)
		{
			return false;
		}

		public override bool performToolAction(Tool t, GameLocation location)
		{
			if (t != null && t.getLastFarmerToUse() != null && t.getLastFarmerToUse() != Game1.player)
			{
				return false;
			}
			if ((bool)playerChest)
			{
				if (t == null)
				{
					return false;
				}
				if (t is MeleeWeapon || !t.isHeavyHitter())
				{
					return false;
				}
				if (base.performToolAction(t, location))
				{
					Farmer player = t.getLastFarmerToUse();
					if (player != null)
					{
						Vector2 c = base.TileLocation;
						if (c.X == 0f && c.Y == 0f)
						{
							bool flag = false;
							foreach (KeyValuePair<Vector2, Object> pair in location.objects.Pairs)
							{
								if (pair.Value == this)
								{
									c.X = (int)pair.Key.X;
									c.Y = (int)pair.Key.Y;
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								c = player.GetToolLocation() / 64f;
								c.X = (int)c.X;
								c.Y = (int)c.Y;
							}
						}
						GetMutex().RequestLock(delegate
						{
							if (itemsCountExcludingNulls() == 0)
							{
								performRemoveAction(tileLocation, location);
								if (location.Objects.Remove(c) && type.Equals("Crafting") && (int)fragility != 2)
								{
									location.debris.Add(new Debris(bigCraftable ? (-base.ParentSheetIndex) : base.ParentSheetIndex, player.GetToolLocation(), new Vector2(player.GetBoundingBox().Center.X, player.GetBoundingBox().Center.Y)));
								}
							}
							else if (t != null && t.isHeavyHitter() && !(t is MeleeWeapon))
							{
								location.playSound("hammer");
								shakeTimer = 100;
								if (t != player.CurrentTool)
								{
									Vector2 zero = Vector2.Zero;
									zero = ((player.FacingDirection == 1) ? new Vector2(1f, 0f) : ((player.FacingDirection == 3) ? new Vector2(-1f, 0f) : ((player.FacingDirection != 0) ? new Vector2(0f, 1f) : new Vector2(0f, -1f))));
									if (base.TileLocation.X == 0f && base.TileLocation.Y == 0f && location.getObjectAtTile((int)c.X, (int)c.Y) == this)
									{
										base.TileLocation = c;
									}
									MoveToSafePosition(location, base.TileLocation, 0, zero);
								}
							}
							GetMutex().ReleaseLock();
						});
					}
				}
				return false;
			}
			if (t != null && t is Pickaxe && currentLidFrame == getLastLidFrame() && (int)frameCounter == -1 && isEmpty())
			{
				return true;
			}
			return false;
		}

		public void addContents(int coins, Item item)
		{
			this.coins.Value += coins;
			items.Add(item);
		}

		public bool MoveToSafePosition(GameLocation location, Vector2 tile_position, int depth = 0, Vector2? prioritize_direction = null)
		{
			List<Vector2> list = new List<Vector2>();
			list.AddRange(new Vector2[4]
			{
				new Vector2(1f, 0f),
				new Vector2(-1f, 0f),
				new Vector2(0f, -1f),
				new Vector2(0f, 1f)
			});
			Utility.Shuffle(Game1.random, list);
			if (prioritize_direction.HasValue)
			{
				list.Remove(-prioritize_direction.Value);
				list.Insert(0, -prioritize_direction.Value);
				list.Remove(prioritize_direction.Value);
				list.Insert(0, prioritize_direction.Value);
			}
			foreach (Vector2 item in list)
			{
				Vector2 vector = tile_position + item;
				if (canBePlacedHere(location, vector) && location.isTilePlaceable(vector))
				{
					if (location.objects.ContainsKey(base.TileLocation) && !location.objects.ContainsKey(vector))
					{
						location.objects.Remove(base.TileLocation);
						kickStartTile.Value = base.TileLocation;
						base.TileLocation = vector;
						location.objects[vector] = this;
						boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
					}
					return true;
				}
			}
			Utility.Shuffle(Game1.random, list);
			if (prioritize_direction.HasValue)
			{
				list.Remove(-prioritize_direction.Value);
				list.Insert(0, -prioritize_direction.Value);
				list.Remove(prioritize_direction.Value);
				list.Insert(0, prioritize_direction.Value);
			}
			if (depth < 3)
			{
				foreach (Vector2 item2 in list)
				{
					Vector2 tile_position2 = tile_position + item2;
					if (location.isPointPassable(new Location((int)(tile_position2.X + 0.5f) * 64, (int)(tile_position2.Y + 0.5f) * 64), Game1.viewport) && MoveToSafePosition(location, tile_position2, depth + 1, prioritize_direction))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			localKickStartTile = null;
			kickProgress = -1f;
			return base.placementAction(location, x, y, who);
		}

		public void destroyAndDropContents(Vector2 pointToDropAt, GameLocation location)
		{
			List<Item> list = new List<Item>();
			list.AddRange(items);
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
			{
				foreach (NetObjectList<Item> value in separateWalletItems.Values)
				{
					list.AddRange(value);
				}
			}
			if (list.Count > 0)
			{
				location.playSound("throwDownITem");
			}
			foreach (Item item in list)
			{
				if (item != null)
				{
					Game1.createItemDebris(item, pointToDropAt, Game1.random.Next(4), location);
				}
			}
			items.Clear();
			separateWalletItems.Clear();
			clearNulls();
		}

		public void dumpContents(GameLocation location)
		{
			if (synchronized.Value && (GetMutex().IsLocked() || !Game1.IsMasterGame) && !GetMutex().IsLockHeld())
			{
				return;
			}
			if (items.Count > 0 && !chestType.Equals("Monster") && items.Count >= 1 && (GetMutex().IsLockHeld() || !playerChest))
			{
				bool flag = Utility.IsNormalObjectAtParentSheetIndex(items[0], 434);
				if (location is FarmHouse)
				{
					FarmHouse farmHouse = location as FarmHouse;
					if (farmHouse.owner.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Objects:ParsnipSeedPackage_SomeoneElse"));
						return;
					}
					if (!flag)
					{
						Game1.player.addQuest(6);
						float num = Game1.DateTimeScale / Game1.options.zoomLevel;
						Vector2 vector = new Vector2((float)(Game1.dayTimeMoneyBox.questButton.bounds.X - 4) * num, (float)(Game1.dayTimeMoneyBox.questButton.bounds.Y + Game1.dayTimeMoneyBox.questButton.bounds.Height + 4) * num);
					}
				}
				if (flag)
				{
					string item = ((location is FarmHouse) ? "CF_Spouse" : "CF_Mines");
					if (!Game1.player.mailReceived.Contains(item))
					{
						Game1.player.eatObject(items[0] as Object, overrideFullness: true);
						Game1.player.mailReceived.Add(item);
					}
					items.Clear();
				}
				else if (dropContents.Value)
				{
					foreach (Item item3 in items)
					{
						if (item3 != null)
						{
							Game1.createItemDebris(item3, tileLocation.Value * 64f, -1, location);
						}
					}
					items.Clear();
					clearNulls();
					if (location is VolcanoDungeon)
					{
						if (bigCraftableSpriteIndex.Value == 223)
						{
							Game1.player.team.RequestLimitedNutDrops("VolcanoNormalChest", location, (int)tileLocation.Value.X * 64, (int)tileLocation.Value.Y * 64, 1);
						}
						else if (bigCraftableSpriteIndex.Value == 227)
						{
							Game1.player.team.RequestLimitedNutDrops("VolcanoRareChest", location, (int)tileLocation.Value.X * 64, (int)tileLocation.Value.Y * 64, 1);
						}
					}
				}
				else if (!synchronized.Value || GetMutex().IsLockHeld())
				{
					Item item2 = items[0];
					items[0] = null;
					items.RemoveAt(0);
					Game1.player.addItemByMenuIfNecessaryElseHoldUp(item2);
					if (location is Caldera)
					{
						Game1.player.mailReceived.Add("CalderaTreasure");
					}
					IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
					ItemGrabMenu grab_menu = activeClickableMenu as ItemGrabMenu;
					if (grab_menu != null)
					{
						ItemGrabMenu itemGrabMenu = grab_menu;
						itemGrabMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(itemGrabMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
						{
							grab_menu.DropRemainingItems();
						});
					}
				}
				if (Game1.mine != null)
				{
					Game1.mine.chestConsumed();
				}
			}
			if (chestType.Equals("Monster"))
			{
				Monster monsterForThisLevel = Game1.mine.getMonsterForThisLevel(Game1.CurrentMineLevel, (int)tileLocation.X, (int)tileLocation.Y);
				Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(new Point((int)tileLocation.X, (int)tileLocation.Y), 8f, Game1.player);
				monsterForThisLevel.xVelocity = velocityTowardPlayer.X;
				monsterForThisLevel.yVelocity = velocityTowardPlayer.Y;
				location.characters.Add(monsterForThisLevel);
				location.playSound("explosion");
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), flicker: false, Game1.random.NextDouble() < 0.5));
				location.objects.Remove(tileLocation);
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Chest.cs.12531"), Color.Red, 3500f));
			}
			else
			{
				Game1.player.gainExperience(5, 25 + Game1.CurrentMineLevel);
			}
			if ((bool)giftbox)
			{
				TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite("LooseSprites\\Giftbox", new Microsoft.Xna.Framework.Rectangle(0, (int)giftboxIndex * 32, 16, 32), 80f, 11, 1, tileLocation.Value * 64f - new Vector2(0f, 52f), flicker: false, flipped: false, tileLocation.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					destroyable = false,
					holdLastFrame = true
				};
				if (location.netObjects.ContainsKey(tileLocation) && location.netObjects[tileLocation] == this)
				{
					Game1.multiplayer.broadcastSprites(location, temporaryAnimatedSprite);
					location.removeObject(tileLocation, showDestroyedObject: false);
				}
				else
				{
					location.temporarySprites.Add(temporaryAnimatedSprite);
				}
			}
		}

		public NetMutex GetMutex()
		{
			if (specialChestType.Value == SpecialChestTypes.JunimoChest)
			{
				return Game1.player.team.junimoChestMutex;
			}
			return mutex;
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if ((bool)giftbox)
			{
				Game1.player.Halt();
				Game1.player.freezePause = 1000;
				who.currentLocation.playSound("Ship");
				dumpContents(who.currentLocation);
			}
			else if ((bool)playerChest)
			{
				if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
				{
					return false;
				}
				GetMutex().RequestLock(delegate
				{
					if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
					{
						OpenMiniShippingMenu();
					}
					else
					{
						frameCounter.Value = 5;
						Game1.playSound(fridge ? "doorCreak" : "openChest");
						Game1.player.Halt();
						Game1.player.freezePause = 1000;
					}
				});
			}
			else if (!playerChest)
			{
				if (currentLidFrame == startingLidFrame.Value && (int)frameCounter <= -1)
				{
					who.currentLocation.playSound("openChest");
					if (synchronized.Value)
					{
						GetMutex().RequestLock(delegate
						{
							openChestEvent.Fire();
						});
					}
					else
					{
						performOpenChest();
					}
				}
				else if (currentLidFrame == getLastLidFrame() && items.Count > 0 && !synchronized.Value)
				{
					Item item = items[0];
					items[0] = null;
					items.RemoveAt(0);
					if (Game1.mine != null)
					{
						Game1.mine.chestConsumed();
					}
					who.addItemByMenuIfNecessaryElseHoldUp(item);
					IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
					ItemGrabMenu grab_menu = activeClickableMenu as ItemGrabMenu;
					if (grab_menu != null)
					{
						ItemGrabMenu itemGrabMenu = grab_menu;
						itemGrabMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(itemGrabMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
						{
							grab_menu.DropRemainingItems();
						});
					}
				}
			}
			if (items.Count == 0 && (int)coins == 0 && !playerChest)
			{
				who.currentLocation.removeObject(tileLocation, showDestroyedObject: false);
				who.currentLocation.playSound("woodWhack");
			}
			return true;
		}

		public virtual void OpenMiniShippingMenu()
		{
			Game1.playSound("shwip");
			ShowMenu();
		}

		public virtual void performOpenChest()
		{
			frameCounter.Value = 5;
		}

		public virtual void grabItemFromChest(Item item, Farmer who)
		{
			if (who.couldInventoryAcceptThisItem(item))
			{
				NetObjectList<Item> itemsForPlayer = GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
				int num = itemsForPlayer.IndexOf(item);
				if (num >= 0)
				{
					itemsForPlayer[num] = null;
				}
				if (!playerChest && SpecialChestType != SpecialChestTypes.Mill)
				{
					clearNulls();
				}
				ShowMenu();
				((ItemGrabMenu)Game1.activeClickableMenu).enableGamePadControls = true;
				createSlotsForCapacity();
			}
		}

		public virtual Item addItem(Item item)
		{
			item.resetState();
			clearNulls();
			createSlotsForCapacity();
			NetObjectList<Item> itemsForPlayer = items;
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin || SpecialChestType == SpecialChestTypes.JunimoChest)
			{
				itemsForPlayer = GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
			}
			for (int i = 0; i < itemsForPlayer.Count; i++)
			{
				if (itemsForPlayer[i] != null && itemsForPlayer[i].canStackWith(item))
				{
					item.Stack = itemsForPlayer[i].addToStack(item);
					if (item.Stack <= 0)
					{
						return null;
					}
				}
				else if (itemsForPlayer[i] == null)
				{
					itemsForPlayer[i] = item;
					return null;
				}
			}
			if (itemsForPlayer.Count < GetActualCapacity())
			{
				itemsForPlayer.Add(item);
				return null;
			}
			return item;
		}

		public virtual int GetActualCapacity()
		{
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
			{
				return 9;
			}
			if (SpecialChestType == SpecialChestTypes.JunimoChest)
			{
				return 9;
			}
			if (SpecialChestType == SpecialChestTypes.Enricher)
			{
				return 1;
			}
			return 36;
		}

		public virtual void CheckAutoLoad(Farmer who)
		{
			if (who.currentLocation != null)
			{
				Object value = null;
				if (who.currentLocation.objects.TryGetValue(new Vector2(base.TileLocation.X, base.TileLocation.Y + 1f), out value))
				{
					value?.AttemptAutoLoad(who);
				}
			}
		}

		public virtual void ShowMenu()
		{
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
			{
				Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(Game1.player.UniqueMultiplayerID), reverseGrab: false, showReceivingMenu: true, Utility.highlightShippableObjects, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 1, fridge ? null : this, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, this);
			}
			else if (SpecialChestType == SpecialChestTypes.JunimoChest)
			{
				Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(Game1.player.UniqueMultiplayerID), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, this);
			}
			else if (SpecialChestType == SpecialChestTypes.AutoLoader)
			{
				ItemGrabMenu itemGrabMenu = new ItemGrabMenu(GetItemsForPlayer(Game1.player.UniqueMultiplayerID), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, this);
				itemGrabMenu.exitFunction = (IClickableMenu.onExit)Delegate.Combine(itemGrabMenu.exitFunction, (IClickableMenu.onExit)delegate
				{
					CheckAutoLoad(Game1.player);
				});
				Game1.activeClickableMenu = itemGrabMenu;
			}
			else if (SpecialChestType == SpecialChestTypes.Enricher)
			{
				ItemGrabMenu itemGrabMenu2 = (ItemGrabMenu)(Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(Game1.player.UniqueMultiplayerID), reverseGrab: false, showReceivingMenu: true, Object.HighlightFertilizers, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, this));
			}
			else
			{
				Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(Game1.player.UniqueMultiplayerID), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, null, -1, 3, null, allowStack: true, null, rearrangeGrangeOnExit: false, null, this);
			}
		}

		public virtual void grabItemFromInventory(Item item, Farmer who)
		{
			if (item.Stack == 0)
			{
				item.Stack = 1;
			}
			Item item2 = addItem(item);
			if (item2 == null)
			{
				who.removeItemFromInventory(item);
			}
			else
			{
				item2 = who.addItemToInventory(item2);
			}
			if (!playerChest)
			{
				clearNulls();
			}
			int num = ((Game1.activeClickableMenu.currentlySnappedComponent != null) ? Game1.activeClickableMenu.currentlySnappedComponent.myID : (-1));
			ShowMenu();
			(Game1.activeClickableMenu as ItemGrabMenu).heldItem = item2;
			if (num != -1)
			{
				Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(num);
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
			((ItemGrabMenu)Game1.activeClickableMenu).enableGamePadControls = true;
			createSlotsForCapacity();
		}

		public NetObjectList<Item> GetItemsForPlayer(long id)
		{
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin && Game1.player.team.useSeparateWallets.Value && SpecialChestType == SpecialChestTypes.MiniShippingBin && Game1.player.team.useSeparateWallets.Value)
			{
				if (!separateWalletItems.ContainsKey(id))
				{
					separateWalletItems[id] = new NetObjectList<Item>();
				}
				return separateWalletItems[id];
			}
			if (SpecialChestType == SpecialChestTypes.JunimoChest)
			{
				return Game1.player.team.junimoChest;
			}
			return items;
		}

		public virtual bool isEmpty()
		{
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin && Game1.player.team.useSeparateWallets.Value)
			{
				foreach (NetObjectList<Item> value in separateWalletItems.Values)
				{
					for (int num = value.Count() - 1; num >= 0; num--)
					{
						if (value[num] != null)
						{
							return false;
						}
					}
				}
				return true;
			}
			if (SpecialChestType == SpecialChestTypes.JunimoChest)
			{
				NetObjectList<Item> itemsForPlayer = GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
				for (int num2 = itemsForPlayer.Count - 1; num2 >= 0; num2--)
				{
					if (itemsForPlayer[num2] != null)
					{
						return false;
					}
				}
				return true;
			}
			for (int num3 = items.Count - 1; num3 >= 0; num3--)
			{
				if (items[num3] != null)
				{
					return false;
				}
			}
			return true;
		}

		public virtual void clearNulls()
		{
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin || SpecialChestType == SpecialChestTypes.JunimoChest)
			{
				NetObjectList<Item> itemsForPlayer = GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
				for (int num = itemsForPlayer.Count - 1; num >= 0; num--)
				{
					if (itemsForPlayer[num] == null)
					{
						itemsForPlayer.RemoveAt(num);
					}
				}
				return;
			}
			for (int num2 = items.Count - 1; num2 >= 0; num2--)
			{
				if (items[num2] == null)
				{
					items.RemoveAt(num2);
				}
			}
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			if (synchronized.Value)
			{
				openChestEvent.Poll();
			}
			if (localKickStartTile.HasValue)
			{
				if (Game1.currentLocation == environment)
				{
					if (kickProgress == 0f)
					{
						if (Utility.isOnScreen((localKickStartTile.Value + new Vector2(0.5f, 0.5f)) * 64f, 64))
						{
							Game1.playSound("clubhit");
						}
						shakeTimer = 100;
					}
				}
				else
				{
					localKickStartTile = null;
					kickProgress = -1f;
				}
				if (kickProgress >= 0f)
				{
					float num = 0.25f;
					kickProgress += (float)(time.ElapsedGameTime.TotalSeconds / (double)num);
					if (kickProgress >= 1f)
					{
						kickProgress = -1f;
						localKickStartTile = null;
					}
				}
			}
			else
			{
				kickProgress = -1f;
			}
			fixLidFrame();
			mutex.Update(environment);
			if (shakeTimer > 0)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
				if (shakeTimer <= 0)
				{
					health = 10;
				}
			}
			if ((bool)playerChest)
			{
				if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
				{
					UpdateFarmerNearby(environment);
					if (_shippingBinFrameCounter > -1)
					{
						_shippingBinFrameCounter--;
						if (_shippingBinFrameCounter <= 0)
						{
							_shippingBinFrameCounter = 5;
							if (_farmerNearby && currentLidFrame < getLastLidFrame())
							{
								currentLidFrame++;
							}
							else if (!_farmerNearby && currentLidFrame > startingLidFrame.Value)
							{
								currentLidFrame--;
							}
							else
							{
								_shippingBinFrameCounter = -1;
							}
						}
					}
					if (Game1.activeClickableMenu == null && GetMutex().IsLockHeld())
					{
						GetMutex().ReleaseLock();
					}
				}
				else if ((int)frameCounter > -1 && currentLidFrame < getLastLidFrame() + 1)
				{
					frameCounter.Value--;
					if ((int)frameCounter <= 0 && GetMutex().IsLockHeld())
					{
						if (currentLidFrame == getLastLidFrame())
						{
							createSlotsForCapacity();
							ShowMenu();
							((ItemGrabMenu)Game1.activeClickableMenu).enableGamePadControls = true;
							frameCounter.Value = -1;
						}
						else
						{
							frameCounter.Value = 5;
							currentLidFrame++;
						}
					}
				}
				else if ((((int)frameCounter == -1 && currentLidFrame > (int)startingLidFrame) || currentLidFrame >= getLastLidFrame()) && Game1.activeClickableMenu == null && GetMutex().IsLockHeld())
				{
					GetMutex().ReleaseLock();
					currentLidFrame = getLastLidFrame();
					frameCounter.Value = 2;
					environment.localSound("doorCreakReverse");
				}
			}
			else
			{
				if ((int)frameCounter <= -1 || currentLidFrame > getLastLidFrame())
				{
					return;
				}
				frameCounter.Value--;
				if ((int)frameCounter > 0)
				{
					return;
				}
				if (currentLidFrame == getLastLidFrame())
				{
					dumpContents(environment);
					frameCounter.Value = -1;
					return;
				}
				frameCounter.Value = 10;
				currentLidFrame++;
				if (currentLidFrame == getLastLidFrame())
				{
					frameCounter.Value += 5;
				}
			}
		}

		public virtual void UpdateFarmerNearby(GameLocation location, bool animate = true)
		{
			bool flag = false;
			foreach (Farmer farmer in location.farmers)
			{
				if (Math.Abs((float)farmer.getTileX() - tileLocation.X) <= 1f && Math.Abs((float)farmer.getTileY() - tileLocation.Y) <= 1f)
				{
					flag = true;
					break;
				}
			}
			if (flag == _farmerNearby)
			{
				return;
			}
			_farmerNearby = flag;
			_shippingBinFrameCounter = 5;
			if (!animate)
			{
				_shippingBinFrameCounter = -1;
				if (_farmerNearby)
				{
					currentLidFrame = getLastLidFrame();
				}
				else
				{
					currentLidFrame = startingLidFrame.Value;
				}
			}
			else if (Game1.gameMode != 6)
			{
				if (_farmerNearby)
				{
					location.localSound("doorCreak");
				}
				else
				{
					location.localSound("doorCreakReverse");
				}
			}
		}

		public override void actionOnPlayerEntry()
		{
			fixLidFrame();
			if (specialChestType.Value == SpecialChestTypes.MiniShippingBin)
			{
				UpdateFarmerNearby(Game1.currentLocation, animate: false);
			}
			kickProgress = -1f;
			localKickStartTile = null;
			if (!playerChest && items.Count == 0 && (int)coins == 0)
			{
				currentLidFrame = getLastLidFrame();
			}
		}

		public virtual void SetBigCraftableSpriteIndex(int sprite_index, int starting_lid_frame = -1, int lid_frame_count = 3)
		{
			bigCraftableSpriteIndex.Value = sprite_index;
			if (starting_lid_frame >= 0)
			{
				startingLidFrame.Value = starting_lid_frame;
			}
			else
			{
				startingLidFrame.Value = sprite_index + 1;
			}
			lidFrameCount.Value = lid_frame_count;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			float num = x;
			float num2 = y;
			if (localKickStartTile.HasValue)
			{
				num = Utility.Lerp(localKickStartTile.Value.X, num, kickProgress);
				num2 = Utility.Lerp(localKickStartTile.Value.Y, num2, kickProgress);
			}
			float num3 = Math.Max(0f, ((num2 + 1f) * 64f - 24f) / 10000f) + num * 1E-05f;
			if (localKickStartTile.HasValue)
			{
				spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((num + 0.5f) * 64f, (num2 + 0.5f) * 64f)), Game1.shadowTexture.Bounds, Color.Black * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.0001f);
				num2 -= (float)Math.Sin((double)kickProgress * Math.PI) * 0.5f;
			}
			if ((bool)playerChest && (base.ParentSheetIndex == 130 || base.ParentSheetIndex == 232))
			{
				if (playerChoiceColor.Value.Equals(Color.Black))
				{
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (num2 - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, base.ParentSheetIndex, 16, 32), tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, num3);
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (num2 - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame, 16, 32), tint.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, num3 + 1E-05f);
					return;
				}
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64f, (num2 - 1f) * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, (base.ParentSheetIndex == 130) ? 168 : base.ParentSheetIndex, 16, 32), playerChoiceColor.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, num3);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64f, num2 * 64f + 20f)), new Microsoft.Xna.Framework.Rectangle(0, ((base.ParentSheetIndex == 130) ? 168 : base.ParentSheetIndex) / 8 * 32 + 53, 16, 11), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, num3 + 2E-05f);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64f, (num2 - 1f) * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, (base.ParentSheetIndex == 130) ? (currentLidFrame + 46) : (currentLidFrame + 8), 16, 32), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, num3 + 2E-05f);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64f, (num2 - 1f) * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, (base.ParentSheetIndex == 130) ? (currentLidFrame + 38) : currentLidFrame, 16, 32), playerChoiceColor.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, num3 + 1E-05f);
				return;
			}
			if ((bool)playerChest)
			{
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (num2 - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, base.ParentSheetIndex, 16, 32), tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, num3);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (num2 - 1f) * 64f)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame, 16, 32), tint.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, num3 + 1E-05f);
				return;
			}
			if ((bool)giftbox)
			{
				spriteBatch.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(16f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 5f, SpriteEffects.None, 1E-07f);
				if (items.Count > 0 || (int)coins > 0)
				{
					int y2 = (int)giftboxIndex * 32;
					spriteBatch.Draw(Game1.giftboxTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), num2 * 64f - 52f)), new Microsoft.Xna.Framework.Rectangle(0, y2, 16, 32), tint, 0f, Vector2.Zero, 4f, SpriteEffects.None, num3);
				}
				return;
			}
			int tilePosition = 500;
			Texture2D texture2D = Game1.objectSpriteSheet;
			int height = 16;
			int num4 = 0;
			if (bigCraftableSpriteIndex.Value >= 0)
			{
				tilePosition = bigCraftableSpriteIndex.Value;
				texture2D = Game1.bigCraftableSpriteSheet;
				height = 32;
				num4 = -64;
			}
			if (bigCraftableSpriteIndex.Value < 0)
			{
				spriteBatch.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(16f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 5f, SpriteEffects.None, 1E-07f);
			}
			spriteBatch.Draw(texture2D, Game1.GlobalToLocal(Game1.viewport, new Vector2(num * 64f, num2 * 64f + (float)num4)), Game1.getSourceRectForStandardTileSheet(texture2D, tilePosition, 16, height), tint, 0f, Vector2.Zero, 4f, SpriteEffects.None, num3);
			Vector2 globalPosition = new Vector2(num * 64f, num2 * 64f + (float)num4);
			if (bigCraftableSpriteIndex.Value < 0)
			{
				switch (currentLidFrame)
				{
				case 501:
					globalPosition.Y -= 32f;
					break;
				case 502:
					globalPosition.Y -= 40f;
					break;
				case 503:
					globalPosition.Y -= 60f;
					break;
				}
			}
			spriteBatch.Draw(texture2D, Game1.GlobalToLocal(Game1.viewport, globalPosition), Game1.getSourceRectForStandardTileSheet(texture2D, currentLidFrame, 16, height), tint, 0f, Vector2.Zero, 4f, SpriteEffects.None, num3 + 1E-05f);
		}

		public virtual void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f, bool local = false)
		{
			if ((bool)playerChest)
			{
				if (playerChoiceColor.Equals(Color.Black))
				{
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - 1) * 64)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, parentSheetIndex, 16, 32), tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.89f : ((float)(y * 64 + 4) / 10000f));
					return;
				}
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, (base.ParentSheetIndex == 130) ? 168 : base.ParentSheetIndex, 16, 32), playerChoiceColor.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.9f : ((float)(y * 64 + 4) / 10000f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, (base.ParentSheetIndex == 130) ? (currentLidFrame + 38) : currentLidFrame, 16, 32), playerChoiceColor.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.9f : ((float)(y * 64 + 5) / 10000f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y + 20) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 + 20)), new Microsoft.Xna.Framework.Rectangle(0, ((base.ParentSheetIndex == 130) ? 168 : base.ParentSheetIndex) / 8 * 32 + 53, 16, 11), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.91f : ((float)(y * 64 + 6) / 10000f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, (base.ParentSheetIndex == 130) ? (currentLidFrame + 46) : (currentLidFrame + 8), 16, 32), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.91f : ((float)(y * 64 + 6) / 10000f));
			}
		}
	}
}
