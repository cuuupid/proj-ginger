using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Tools;

namespace StardewValley
{
	public class Fence : Object
	{
		public const int debrisPieces = 4;

		public static int fencePieceWidth = 16;

		public static int fencePieceHeight = 32;

		public const int gateClosedPosition = 0;

		public const int gateOpenedPosition = 88;

		public const int sourceRectForSoloGate = 17;

		public const int globalHealthMultiplier = 2;

		public const int N = 1000;

		public const int E = 100;

		public const int S = 500;

		public const int W = 10;

		public new const int wood = 1;

		public new const int stone = 2;

		public const int steel = 3;

		public const int gate = 4;

		public new const int gold = 5;

		[XmlIgnore]
		public Lazy<Texture2D> fenceTexture;

		public static Dictionary<int, int> fenceDrawGuide;

		[XmlElement("health")]
		public new readonly NetFloat health = new NetFloat();

		[XmlElement("maxHealth")]
		public readonly NetFloat maxHealth = new NetFloat();

		[XmlElement("whichType")]
		public readonly NetInt whichType = new NetInt();

		[XmlElement("gatePosition")]
		public readonly NetInt gatePosition = new NetInt();

		public int gateMotion;

		[XmlElement("isGate")]
		public readonly NetBool isGate = new NetBool();

		[XmlIgnore]
		public readonly NetBool repairQueued = new NetBool();

		public bool gateOpen => (int)gatePosition == 88;

		public bool isSoloGate
		{
			get
			{
				if ((bool)isGate)
				{
					GameLocation currentLocation = Game1.currentLocation;
					Vector2 key = tileLocation;
					key.X += 1f;
					if (currentLocation.objects.ContainsKey(key) && currentLocation.objects[key] is Fence && ((Fence)currentLocation.objects[key]).countsForDrawing(whichType))
					{
						return false;
					}
					key.X -= 2f;
					if (currentLocation.objects.ContainsKey(key) && currentLocation.objects[key] is Fence && ((Fence)currentLocation.objects[key]).countsForDrawing(whichType))
					{
						return false;
					}
					key.X += 1f;
					key.Y += 1f;
					if (currentLocation.objects.ContainsKey(key) && currentLocation.objects[key] is Fence && ((Fence)currentLocation.objects[key]).countsForDrawing(whichType))
					{
						return false;
					}
					key.Y -= 2f;
					if (currentLocation.objects.ContainsKey(key) && currentLocation.objects[key] is Fence && ((Fence)currentLocation.objects[key]).countsForDrawing(whichType))
					{
						return false;
					}
					return true;
				}
				return false;
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(health, maxHealth, whichType, gatePosition, isGate, repairQueued);
		}

		public Fence(Vector2 tileLocation, int whichType, bool isGate)
			: this()
		{
			this.whichType.Value = whichType;
			ResetHealth((float)Game1.random.Next(-100, 101) / 100f);
			price.Value = whichType;
			this.isGate.Value = isGate;
			base.tileLocation.Value = tileLocation;
			canBeSetDown.Value = true;
			canBeGrabbed.Value = true;
			price.Value = 1;
			if (isGate)
			{
				health.Value *= 2f;
			}
			base.Type = "Crafting";
			boundingBox.Value = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		public virtual void ResetHealth(float amount_adjustment)
		{
			float num = GetBaseHealthForType(whichType);
			if ((int)whichType == 4)
			{
				amount_adjustment = 0f;
			}
			health.Value = num + amount_adjustment;
			switch ((int)whichType)
			{
			case 1:
				base.name = "Wood Fence";
				base.ParentSheetIndex = -5;
				break;
			case 2:
				base.name = "Stone Fence";
				base.ParentSheetIndex = -6;
				break;
			case 3:
				base.name = "Iron Fence";
				base.ParentSheetIndex = -7;
				break;
			case 4:
				base.name = "Gate";
				base.ParentSheetIndex = -9;
				break;
			case 5:
				base.name = "Hardwood Fence";
				base.ParentSheetIndex = -8;
				break;
			}
			health.Value *= 2f;
			maxHealth.Value = health.Value;
		}

		public virtual int GetBaseHealthForType(int fence_type)
		{
			return (int)whichType switch
			{
				1 => 28, 
				2 => 60, 
				3 => 125, 
				4 => 100, 
				5 => 280, 
				_ => 100, 
			};
		}

		public Fence()
		{
			fenceTexture = new Lazy<Texture2D>(loadFenceTexture);
			if (fenceDrawGuide == null)
			{
				populateFenceDrawGuide();
			}
			price.Value = 1;
		}

		public virtual void repair()
		{
			ResetHealth((float)Game1.random.Next(-100, 101) / 100f);
		}

		public static void populateFenceDrawGuide()
		{
			fenceDrawGuide = new Dictionary<int, int>();
			fenceDrawGuide.Add(0, 5);
			fenceDrawGuide.Add(10, 9);
			fenceDrawGuide.Add(100, 10);
			fenceDrawGuide.Add(1000, 3);
			fenceDrawGuide.Add(500, 5);
			fenceDrawGuide.Add(1010, 8);
			fenceDrawGuide.Add(1100, 6);
			fenceDrawGuide.Add(1500, 3);
			fenceDrawGuide.Add(600, 0);
			fenceDrawGuide.Add(510, 2);
			fenceDrawGuide.Add(110, 7);
			fenceDrawGuide.Add(1600, 0);
			fenceDrawGuide.Add(1610, 4);
			fenceDrawGuide.Add(1510, 2);
			fenceDrawGuide.Add(1110, 7);
			fenceDrawGuide.Add(610, 4);
		}

		public virtual void PerformRepairIfNecessary()
		{
			if (Game1.IsMasterGame && repairQueued.Value)
			{
				ResetHealth(GetRepairHealthAdjustment());
				repairQueued.Value = false;
			}
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			PerformRepairIfNecessary();
			int num = gatePosition.Get();
			num += gateMotion;
			if (num == 88)
			{
				int drawSum = getDrawSum(environment);
				if (drawSum != 110 && drawSum != 1500 && drawSum != 1000 && drawSum != 500 && drawSum != 100 && drawSum != 10)
				{
					toggleGate(Game1.player, open: false);
				}
			}
			gatePosition.Set(num);
			if (num >= 88 || num <= 0)
			{
				gateMotion = 0;
			}
			heldObject.Get()?.updateWhenCurrentLocation(time, environment);
		}

		public int getDrawSum(GameLocation location)
		{
			int num = 0;
			Vector2 key = tileLocation;
			key.X += 1f;
			if (location.objects.ContainsKey(key) && location.objects[key] is Fence && ((Fence)location.objects[key]).countsForDrawing(whichType))
			{
				num += 100;
			}
			key.X -= 2f;
			if (location.objects.ContainsKey(key) && location.objects[key] is Fence && ((Fence)location.objects[key]).countsForDrawing(whichType))
			{
				num += 10;
			}
			key.X += 1f;
			key.Y += 1f;
			if (location.objects.ContainsKey(key) && location.objects[key] is Fence && ((Fence)location.objects[key]).countsForDrawing(whichType))
			{
				num += 500;
			}
			key.Y -= 2f;
			if (location.objects.ContainsKey(key) && location.objects[key] is Fence && ((Fence)location.objects[key]).countsForDrawing(whichType))
			{
				num += 1000;
			}
			return num;
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (!justCheckingForActivity && who != null && who.currentLocation.objects.ContainsKey(new Vector2(who.getTileX(), who.getTileY() - 1)) && who.currentLocation.objects.ContainsKey(new Vector2(who.getTileX(), who.getTileY() + 1)) && who.currentLocation.objects.ContainsKey(new Vector2(who.getTileX() + 1, who.getTileY())) && who.currentLocation.objects.ContainsKey(new Vector2(who.getTileX() - 1, who.getTileY())))
			{
				performToolAction(null, who.currentLocation);
			}
			if ((float)health <= 1f)
			{
				return false;
			}
			if ((bool)isGate)
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				int drawSum = getDrawSum(who.currentLocation);
				if ((bool)isGate)
				{
					toggleGate(who, (int)gatePosition == 0);
				}
				return true;
			}
			if (justCheckingForActivity)
			{
				return false;
			}
			foreach (Vector2 adjacentTileLocation in Utility.getAdjacentTileLocations(tileLocation))
			{
				if (who.currentLocation.objects.ContainsKey(adjacentTileLocation) && who.currentLocation.objects[adjacentTileLocation] is Fence && (bool)((Fence)who.currentLocation.objects[adjacentTileLocation]).isGate)
				{
					((Fence)who.currentLocation.objects[adjacentTileLocation]).checkForAction(who);
					return true;
				}
			}
			return (float)health <= 0f;
		}

		public virtual void toggleGate(GameLocation location, bool open, bool is_toggling_counterpart = false, Farmer who = null)
		{
			if ((float)health <= 1f)
			{
				return;
			}
			int drawSum = getDrawSum(location);
			if (drawSum == 110 || drawSum == 1500 || drawSum == 1000 || drawSum == 500 || drawSum == 100 || drawSum == 10)
			{
				who?.TemporaryPassableTiles.Add(new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64));
				if (open)
				{
					gatePosition.Value = 88;
				}
				else
				{
					gatePosition.Value = 0;
				}
				if (!is_toggling_counterpart)
				{
					location.playSound("doorClose");
				}
			}
			else
			{
				who?.TemporaryPassableTiles.Add(new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64));
				gatePosition.Value = 0;
			}
			if (is_toggling_counterpart)
			{
				return;
			}
			switch (drawSum)
			{
			case 100:
			{
				Vector2 key2 = tileLocation + new Vector2(-1f, 0f);
				if (Game1.currentLocation.objects.ContainsKey(key2) && Game1.currentLocation.objects[key2] is Fence && (bool)((Fence)Game1.currentLocation.objects[key2]).isGate && ((Fence)Game1.currentLocation.objects[key2]).getDrawSum(Game1.currentLocation) == 10)
				{
					((Fence)Game1.currentLocation.objects[key2]).toggleGate(location, (int)gatePosition != 0, is_toggling_counterpart: true, who);
				}
				break;
			}
			case 10:
			{
				Vector2 key4 = tileLocation + new Vector2(1f, 0f);
				if (Game1.currentLocation.objects.ContainsKey(key4) && Game1.currentLocation.objects[key4] is Fence && (bool)((Fence)Game1.currentLocation.objects[key4]).isGate && ((Fence)Game1.currentLocation.objects[key4]).getDrawSum(Game1.currentLocation) == 100)
				{
					((Fence)Game1.currentLocation.objects[key4]).toggleGate(location, (int)gatePosition != 0, is_toggling_counterpart: true, who);
				}
				break;
			}
			case 1000:
			{
				Vector2 key3 = tileLocation + new Vector2(0f, 1f);
				if (Game1.currentLocation.objects.ContainsKey(key3) && Game1.currentLocation.objects[key3] is Fence && (bool)((Fence)Game1.currentLocation.objects[key3]).isGate && ((Fence)Game1.currentLocation.objects[key3]).getDrawSum(Game1.currentLocation) == 500)
				{
					((Fence)Game1.currentLocation.objects[key3]).toggleGate(location, (int)gatePosition != 0, is_toggling_counterpart: true, who);
				}
				break;
			}
			case 500:
			{
				Vector2 key = tileLocation + new Vector2(0f, -1f);
				if (Game1.currentLocation.objects.ContainsKey(key) && Game1.currentLocation.objects[key] is Fence && (bool)((Fence)Game1.currentLocation.objects[key]).isGate && ((Fence)Game1.currentLocation.objects[key]).getDrawSum(Game1.currentLocation) == 1000)
				{
					((Fence)Game1.currentLocation.objects[key]).toggleGate(location, (int)gatePosition != 0, is_toggling_counterpart: true, who);
				}
				break;
			}
			}
		}

		public void toggleGate(Farmer who, bool open, bool is_toggling_counterpart = false)
		{
			toggleGate(who.currentLocation, open, is_toggling_counterpart, who);
		}

		public override void performRemoveAction(Vector2 tileLocation, GameLocation environment)
		{
			base.ParentSheetIndex = GetItemParentSheetIndex();
			base.performRemoveAction(tileLocation, environment);
		}

		public override void dropItem(GameLocation location, Vector2 origin, Vector2 destination)
		{
			location.debris.Add(new Debris(GetItemParentSheetIndex(), origin, destination));
		}

		public virtual int GetItemParentSheetIndex()
		{
			if ((bool)isGate)
			{
				return 325;
			}
			return (int)whichType switch
			{
				1 => 322, 
				5 => 298, 
				2 => 323, 
				3 => 324, 
				_ => 322, 
			};
		}

		public override bool performToolAction(Tool t, GameLocation location)
		{
			if (heldObject.Value != null && t != null && !(t is MeleeWeapon) && t.isHeavyHitter())
			{
				Item value = heldObject.Value;
				heldObject.Value.performRemoveAction(tileLocation, location);
				heldObject.Value = null;
				Game1.createItemDebris(value.getOne(), base.TileLocation * 64f, -1);
				location.playSound("axchop");
				return false;
			}
			if ((bool)isGate && t != null && (t is Axe || t is Pickaxe))
			{
				location.playSound("axchop");
				Game1.createObjectDebris(325, (int)tileLocation.X, (int)tileLocation.Y, Game1.player.UniqueMultiplayerID, Game1.player.currentLocation);
				location.objects.Remove(tileLocation);
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, 6, resource: false);
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
			}
			if (((int)whichType == 1 || (int)whichType == 5) && (t == null || t is Axe))
			{
				location.playSound("axchop");
				location.objects.Remove(tileLocation);
				for (int i = 0; i < 4; i++)
				{
					location.temporarySprites.Add(new CosmeticDebris(fenceTexture.Value, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), (float)Game1.random.Next(-5, 5) / 100f, (float)Game1.random.Next(-64, 64) / 30f, (float)Game1.random.Next(-800, -100) / 100f, (int)((tileLocation.Y + 1f) * 64f), new Rectangle(32 + Game1.random.Next(2) * 16 / 2, 96 + Game1.random.Next(2) * 16 / 2, 8, 8), Color.White, (Game1.soundBank != null) ? Game1.soundBank.GetCue("shiny4") : null, null, 0, 200));
				}
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, 6, resource: false);
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
				if ((float)maxHealth - (float)health < 0.5f)
				{
					switch ((int)whichType)
					{
					case 1:
						location.debris.Add(new Debris(new Object(322, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
						break;
					case 5:
						location.debris.Add(new Debris(new Object(298, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
						break;
					}
				}
			}
			else if (((int)whichType == 2 || (int)whichType == 3) && (t == null || t is Pickaxe))
			{
				location.playSound("hammer");
				location.objects.Remove(tileLocation);
				for (int j = 0; j < 4; j++)
				{
					location.temporarySprites.Add(new CosmeticDebris(fenceTexture.Value, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), (float)Game1.random.Next(-5, 5) / 100f, (float)Game1.random.Next(-64, 64) / 30f, (float)Game1.random.Next(-800, -100) / 100f, (int)((tileLocation.Y + 1f) * 64f), new Rectangle(32 + Game1.random.Next(2) * 16 / 2, 96 + Game1.random.Next(2) * 16 / 2, 8, 8), Color.White, (Game1.soundBank != null) ? Game1.soundBank.GetCue("shiny4") : null, null, 0, 200));
				}
				Game1.createRadialDebris(location, 14, (int)tileLocation.X, (int)tileLocation.Y, 6, resource: false);
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
				if ((float)maxHealth - (float)health < 0.5f)
				{
					switch ((int)whichType)
					{
					case 2:
						location.debris.Add(new Debris(new Object(323, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
						break;
					case 3:
						location.debris.Add(new Debris(new Object(324, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
						break;
					}
				}
			}
			return false;
		}

		public override bool minutesElapsed(int minutes, GameLocation l)
		{
			if (!Game1.IsMasterGame)
			{
				return false;
			}
			PerformRepairIfNecessary();
			if (!Game1.getFarm().isBuildingConstructed("Gold Clock"))
			{
				health.Value -= (float)minutes / 1440f;
				if ((float)health <= -1f && (Game1.timeOfDay <= 610 || Game1.timeOfDay > 1800))
				{
					return true;
				}
			}
			return false;
		}

		public override void actionOnPlayerEntry()
		{
			base.actionOnPlayerEntry();
			if (heldObject.Value != null)
			{
				heldObject.Value.actionOnPlayerEntry();
				heldObject.Value.isOn.Value = true;
				heldObject.Value.initializeLightSource(tileLocation);
			}
		}

		public override bool performObjectDropInAction(Item dropIn, bool probe, Farmer who)
		{
			if ((int)dropIn.parentSheetIndex == 325)
			{
				if (probe)
				{
					return false;
				}
				if (!isGate)
				{
					Vector2 vector = tileLocation;
					int drawSum = getDrawSum(who.currentLocation);
					if (drawSum == 1500 || drawSum == 110 || drawSum == 1000 || drawSum == 10 || drawSum == 100 || drawSum == 500)
					{
						Vector2 vector2 = default(Vector2);
						switch (drawSum)
						{
						case 10:
							vector2 = tileLocation + new Vector2(1f, 0f);
							if (Game1.currentLocation.objects.ContainsKey(vector2) && Game1.currentLocation.objects[vector2] is Fence && (bool)((Fence)Game1.currentLocation.objects[vector2]).isGate)
							{
								int drawSum3 = ((Fence)Game1.currentLocation.objects[vector2]).getDrawSum(Game1.currentLocation);
								if (drawSum3 != 100 && drawSum3 != 110)
								{
									return false;
								}
							}
							break;
						case 100:
							vector2 = tileLocation + new Vector2(-1f, 0f);
							if (Game1.currentLocation.objects.ContainsKey(vector2) && Game1.currentLocation.objects[vector2] is Fence && (bool)((Fence)Game1.currentLocation.objects[vector2]).isGate)
							{
								int drawSum4 = ((Fence)Game1.currentLocation.objects[vector2]).getDrawSum(Game1.currentLocation);
								if (drawSum4 != 10 && drawSum4 != 110)
								{
									return false;
								}
							}
							break;
						case 1000:
							vector2 = tileLocation + new Vector2(0f, 1f);
							if (Game1.currentLocation.objects.ContainsKey(vector2) && Game1.currentLocation.objects[vector2] is Fence && (bool)((Fence)Game1.currentLocation.objects[vector2]).isGate)
							{
								int drawSum5 = ((Fence)Game1.currentLocation.objects[vector2]).getDrawSum(Game1.currentLocation);
								if (drawSum5 != 500 && drawSum5 != 1500)
								{
									return false;
								}
							}
							break;
						case 500:
							vector2 = tileLocation + new Vector2(0f, -1f);
							if (Game1.currentLocation.objects.ContainsKey(vector2) && Game1.currentLocation.objects[vector2] is Fence && (bool)((Fence)Game1.currentLocation.objects[vector2]).isGate)
							{
								int drawSum2 = ((Fence)Game1.currentLocation.objects[vector2]).getDrawSum(Game1.currentLocation);
								if (drawSum2 != 1000 && drawSum2 != 1500)
								{
									return false;
								}
							}
							break;
						}
						List<Vector2> list = new List<Vector2>();
						list.Add(tileLocation + new Vector2(1f, 0f));
						list.Add(tileLocation + new Vector2(-1f, 0f));
						list.Add(tileLocation + new Vector2(0f, -1f));
						list.Add(tileLocation + new Vector2(0f, 1f));
						foreach (Vector2 item in list)
						{
							if (!(item == vector2) && Game1.currentLocation.objects.ContainsKey(item) && Game1.currentLocation.objects[item] is Fence && (bool)((Fence)Game1.currentLocation.objects[item]).isGate && Game1.currentLocation.objects[item].type.Value == type.Value)
							{
								return false;
							}
						}
						isGate.Value = true;
						who.currentLocation.playSound("axe");
						return true;
					}
				}
			}
			else if ((int)dropIn.parentSheetIndex == 93 && heldObject.Value == null && !isGate)
			{
				heldObject.Value = new Torch(tileLocation, 93);
				heldObject.Value.name = "Torch";
				if (!probe)
				{
					who.currentLocation.playSound("axe");
					heldObject.Value.initializeLightSource(tileLocation);
				}
				return true;
			}
			if ((float)health <= 1f && !repairQueued.Value && CanRepairWithThisItem(dropIn))
			{
				if (probe)
				{
					return true;
				}
				string repairSound = GetRepairSound();
				if (repairSound != null && repairSound != "")
				{
					who.currentLocation.playSound(repairSound);
				}
				repairQueued.Value = true;
				return true;
			}
			return base.performObjectDropInAction(dropIn, probe, who);
		}

		public virtual float GetRepairHealthAdjustment()
		{
			return whichType.Value switch
			{
				1 => (float)Game1.random.Next(-500, 500) / 100f, 
				2 => (float)Game1.random.Next(-500, 600) / 100f, 
				3 => (float)Game1.random.Next(-500, 700) / 100f, 
				5 => (float)Game1.random.Next(-2000, 2000) / 100f, 
				_ => 0f, 
			};
		}

		public virtual string GetRepairSound()
		{
			return whichType.Value switch
			{
				1 => "axe", 
				2 => "stoneStep", 
				3 => "hammer", 
				5 => "axe", 
				_ => "", 
			};
		}

		public virtual bool CanRepairWithThisItem(Item item)
		{
			if ((float)health > 1f)
			{
				return false;
			}
			if (!(item is Object))
			{
				return false;
			}
			if ((int)whichType == 1 && Utility.IsNormalObjectAtParentSheetIndex(item, 322))
			{
				return true;
			}
			if ((int)whichType == 2 && Utility.IsNormalObjectAtParentSheetIndex(item, 323))
			{
				return true;
			}
			if ((int)whichType == 3 && Utility.IsNormalObjectAtParentSheetIndex(item, 324))
			{
				return true;
			}
			if ((int)whichType == 5 && Utility.IsNormalObjectAtParentSheetIndex(item, 298))
			{
				return true;
			}
			return false;
		}

		public override bool performDropDownAction(Farmer who)
		{
			Vector2 value = new Vector2((int)(Game1.player.GetDropLocation().X / 64f), (int)(Game1.player.GetDropLocation().Y / 64f));
			tileLocation.Value = value;
			return false;
		}

		public virtual Texture2D loadFenceTexture()
		{
			int val = whichType.Value;
			if (whichType.Value == 4)
			{
				val = 1;
				isGate.Value = true;
			}
			return Game1.content.Load<Texture2D>("LooseSprites\\Fence" + Math.Max(1, val));
		}

		public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
			spriteBatch.Draw(fenceTexture.Value, objectPosition - new Vector2(0f, 64f), new Rectangle(5 * fencePieceWidth % fenceTexture.Value.Bounds.Width, 5 * fencePieceWidth / fenceTexture.Value.Bounds.Width * fencePieceHeight, fencePieceWidth, fencePieceHeight), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)(f.getStandingY() + 1) / 10000f);
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scale, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			location.Y -= 64f * scale;
			int num = 1;
			int drawSum = getDrawSum(Game1.currentLocation);
			num = fenceDrawGuide[drawSum];
			if ((bool)isGate)
			{
				switch (drawSum)
				{
				case 110:
					spriteBatch.Draw(fenceTexture.Value, location + new Vector2(6f, 6f), new Rectangle(0, 512, 88, 24), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
					return;
				case 1500:
					spriteBatch.Draw(fenceTexture.Value, location + new Vector2(6f, 6f), new Rectangle(112, 512, 16, 64), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
					return;
				}
			}
			spriteBatch.Draw(fenceTexture.Value, location + new Vector2(32f, 32f) * scale, Game1.getArbitrarySourceRect(fenceTexture.Value, 64, 128, num), color * transparency, 0f, new Vector2(32f, 32f) * scale, scale, SpriteEffects.None, layerDepth);
		}

		public bool countsForDrawing(int type)
		{
			if (((float)health > 1f || repairQueued.Value) && !isGate)
			{
				if (type != (int)whichType)
				{
					return type == 4;
				}
				return true;
			}
			return false;
		}

		public override bool isPassable()
		{
			if ((bool)isGate)
			{
				return (int)gatePosition >= 88;
			}
			return false;
		}

		public override void draw(SpriteBatch b, int x, int y, float alpha = 1f)
		{
			int num = 1;
			if ((float)health > 1f || repairQueued.Value)
			{
				int drawSum = getDrawSum(Game1.currentLocation);
				num = fenceDrawGuide[drawSum];
				if ((bool)isGate)
				{
					Vector2 vector = new Vector2(0f, 0f);
					Vector2 vector2 = tileLocation;
					vector2 = tileLocation + new Vector2(-1f, 0f);
					switch (drawSum)
					{
					case 10:
						b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, vector + new Vector2(x * 64 - 16, y * 64 - 128)), new Rectangle(((int)gatePosition == 88) ? 24 : 0, 192, 24, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
						return;
					case 100:
						b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, vector + new Vector2(x * 64 - 16, y * 64 - 128)), new Rectangle(((int)gatePosition == 88) ? 24 : 0, 240, 24, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
						return;
					case 1000:
						b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, vector + new Vector2(x * 64 + 20, y * 64 - 64 - 20)), new Rectangle(((int)gatePosition == 88) ? 24 : 0, 288, 24, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 - 32 + 2) / 10000f);
						return;
					case 500:
						b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, vector + new Vector2(x * 64 + 20, y * 64 - 64 - 20)), new Rectangle(((int)gatePosition == 88) ? 24 : 0, 320, 24, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 96 - 1) / 10000f);
						return;
					case 110:
						b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, vector + new Vector2(x * 64 - 16, y * 64 - 64)), new Rectangle(((int)gatePosition == 88) ? 24 : 0, 128, 24, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32 + 1) / 10000f);
						return;
					case 1500:
						b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, vector + new Vector2(x * 64 + 20, y * 64 - 64 - 20)), new Rectangle(((int)gatePosition == 88) ? 16 : 0, 160, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 - 32 + 2) / 10000f);
						b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, vector + new Vector2(x * 64 + 20, y * 64 - 64 + 44)), new Rectangle(((int)gatePosition == 88) ? 16 : 0, 176, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 96 - 1) / 10000f);
						return;
					}
					num = 17;
				}
				else if (heldObject.Value != null)
				{
					Vector2 zero = Vector2.Zero;
					switch (drawSum)
					{
					case 10:
						if (whichType.Value == 2)
						{
							zero.X = -4f;
						}
						else if (whichType.Value == 3)
						{
							zero.X = 8f;
						}
						else
						{
							zero.X = 0f;
						}
						break;
					case 100:
						if (whichType.Value == 2)
						{
							zero.X = 0f;
						}
						else if (whichType.Value == 3)
						{
							zero.X = -8f;
						}
						else
						{
							zero.X = -4f;
						}
						break;
					}
					if ((int)whichType == 2)
					{
						zero.Y = 16f;
					}
					else if ((int)whichType == 3)
					{
						zero.Y -= 8f;
					}
					if ((int)whichType == 3)
					{
						zero.X -= 2f;
					}
					heldObject.Value.draw(b, x * 64 + (int)zero.X, (y - 1) * 64 - 16 + (int)zero.Y, (float)(y * 64 + 64) / 10000f, 1f);
				}
			}
			b.Draw(fenceTexture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64)), new Rectangle(num * fencePieceWidth % fenceTexture.Value.Bounds.Width, num * fencePieceWidth / fenceTexture.Value.Bounds.Width * fencePieceHeight, fencePieceWidth, fencePieceHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 32) / 10000f);
		}
	}
}
