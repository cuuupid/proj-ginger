using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Objects
{
	public class WoodChipper : Object
	{
		public const int CHIP_TIME = 1000;

		public readonly NetRef<Object> depositedItem = new NetRef<Object>();

		protected bool _isAnimatingChip;

		public int nextSmokeTime;

		public int nextShakeTime;

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(depositedItem);
			depositedItem.fieldChangeVisibleEvent += OnDepositedItemChange;
		}

		public void OnDepositedItemChange(NetRef<Object> field, Object old_value, Object new_value)
		{
			if (Game1.gameMode != 6 && new_value != null)
			{
				shakeTimer = 1000;
				_isAnimatingChip = true;
			}
		}

		public WoodChipper()
		{
		}

		public WoodChipper(Vector2 position)
			: base(position, 211)
		{
			Name = "Wood Chipper";
			type.Value = "Crafting";
			bigCraftable.Value = true;
			canBeSetDown.Value = true;
		}

		public override void addWorkingAnimation(GameLocation environment)
		{
			if (environment != null && environment.farmers.Any() && Game1.random.NextDouble() < 0.35)
			{
				for (int i = 0; i < 8; i++)
				{
					environment.temporarySprites.Add(new TemporaryAnimatedSprite(47, tileLocation.Value * 64f + new Vector2(0f, -76 + Game1.random.Next(-48, 0)), new Color(200, 110, 17), 8, flipped: false, 50f, 0, -1, 0.003f + Math.Max(0f, ((tileLocation.Y + 1f) * 64f - 24f) / 10000f) + tileLocation.X * 1E-05f)
					{
						delayBeforeAnimationStart = i * 100
					});
				}
				environment.playSound("woodchipper_occasional");
				shakeTimer = 1500;
			}
		}

		public override bool performObjectDropInAction(Item dropped_in_item, bool probe, Farmer who)
		{
			Object @object = dropped_in_item as Object;
			if (heldObject.Value != null || depositedItem.Value != null)
			{
				return false;
			}
			if (@object == null)
			{
				return false;
			}
			KeyValuePair<int, int> resultItem = GetResultItem(@object);
			if (resultItem.Key >= 0)
			{
				heldObject.Value = new Object(resultItem.Key, resultItem.Value);
			}
			if (!probe)
			{
				if (resultItem.Key >= 0)
				{
					depositedItem.Value = dropped_in_item.getOne() as Object;
					minutesUntilReady.Value = 180;
					shakeTimer = 1800;
					Game1.playSound("woodchipper");
					for (int i = 0; i < 12; i++)
					{
						who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(47, tileLocation.Value * 64f + new Vector2(0f, -76 + Game1.random.Next(-48, 0)), new Color(200, 110, 17), 8, flipped: false, 50f, 0, -1, 0.003f + Math.Max(0f, ((tileLocation.Y + 1f) * 64f - 24f) / 10000f) + tileLocation.X * 1E-05f)
						{
							delayBeforeAnimationStart = i * 100
						});
					}
					return true;
				}
				return false;
			}
			return true;
		}

		public KeyValuePair<int, int> GetResultItem(Object deposited_item)
		{
			int key = -1;
			int value = 0;
			if (Utility.IsNormalObjectAtParentSheetIndex(deposited_item, 709))
			{
				if (Game1.random.NextDouble() <= 0.019999999552965164)
				{
					switch (Game1.random.Next(0, 3))
					{
					case 0:
						key = 724;
						break;
					case 1:
						key = 725;
						break;
					case 2:
						key = 726;
						break;
					}
					value = 1;
				}
				else
				{
					key = 388;
					value = ((!(Game1.random.NextDouble() <= 0.10000000149011612)) ? Game1.random.Next(5, 11) : Game1.random.Next(15, 21));
				}
			}
			else if (Utility.IsNormalObjectAtParentSheetIndex(deposited_item, 169))
			{
				key = 388;
				value = Game1.random.Next(5, 10);
			}
			return new KeyValuePair<int, int>(key, value);
		}

		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			tileLocation.Value = new Vector2(x / 64, y / 64);
			return true;
		}

		public override void performRemoveAction(Vector2 tileLocation, GameLocation environment)
		{
			base.performRemoveAction(tileLocation, environment);
		}

		public override void actionOnPlayerEntry()
		{
			if (depositedItem.Value != null)
			{
				_ = base.MinutesUntilReady;
				_ = 0;
			}
			base.actionOnPlayerEntry();
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (who.IsLocalPlayer && heldObject.Value != null && readyForHarvest.Value)
			{
				if (!justCheckingForActivity)
				{
					Object value = heldObject.Value;
					heldObject.Value = null;
					if (who.isMoving())
					{
						Game1.haltAfterCheck = false;
					}
					if (!who.addItemToInventoryBool(value))
					{
						heldObject.Value = value;
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
						return false;
					}
					Game1.playSound("coin");
					readyForHarvest.Value = false;
					depositedItem.Value = null;
					heldObject.Value = null;
					AttemptAutoLoad(who);
				}
				return true;
			}
			return base.checkForAction(who, justCheckingForActivity);
		}

		public override void onReadyForHarvest(GameLocation environment)
		{
			base.onReadyForHarvest(environment);
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			if (depositedItem.Value != null && base.MinutesUntilReady > 0)
			{
				nextShakeTime -= time.ElapsedGameTime.Milliseconds;
				nextSmokeTime -= time.ElapsedGameTime.Milliseconds;
				if (nextSmokeTime <= 0)
				{
					nextSmokeTime = Game1.random.Next(3000, 6000);
				}
				if (nextShakeTime <= 0)
				{
					nextShakeTime = Game1.random.Next(1000, 2000);
					if (shakeTimer <= 0)
					{
						_isAnimatingChip = false;
						shakeTimer = 0;
					}
				}
			}
			base.updateWhenCurrentLocation(time, environment);
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (isTemporarilyInvisible)
			{
				return;
			}
			Vector2 one = Vector2.One;
			one *= 4f;
			Vector2 vector = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			Rectangle destinationRectangle = new Rectangle((int)(vector.X - one.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(vector.Y - one.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + one.X), (int)(128f + one.Y / 2f));
			float num = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
			spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, Object.getSourceRectForBigCraftable(base.ParentSheetIndex + (readyForHarvest.Value ? 1 : 0)), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, num);
			if (shakeTimer > 0)
			{
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, new Rectangle(destinationRectangle.X, destinationRectangle.Y + 4, destinationRectangle.Width, 60), new Rectangle(80, 833, 16, 15), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, num + 0.0035f);
			}
			if (depositedItem.Value != null && shakeTimer > 0 && _isAnimatingChip)
			{
				float t = 1f - (float)shakeTimer / 1000f;
				Vector2 vector2 = vector + new Vector2(32f, 32f);
				Vector2 vector3 = vector2 + new Vector2(0f, -16f);
				Vector2 position = default(Vector2);
				position.X = Utility.Lerp(vector3.X, vector2.X, t);
				position.Y = Utility.Lerp(vector3.Y, vector2.Y, t);
				position.X += Game1.random.Next(-1, 2) * 2;
				position.Y += Game1.random.Next(-1, 2) * 2;
				float num2 = Utility.Lerp(1f, 0.75f, t);
				spriteBatch.Draw(Game1.objectSpriteSheet, position, GameLocation.getSourceRectForObject(depositedItem.Value.ParentSheetIndex), Color.White * alpha, 0f, new Vector2(8f, 8f), 4f * num2, depositedItem.Value.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, num + 0.00175f);
			}
			if (depositedItem.Value != null && base.MinutesUntilReady > 0)
			{
				int num3 = (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 200.0) / 50;
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, vector + new Vector2(6f, 17f) * 4f, new Rectangle(80 + num3 % 2 * 8, 848 + num3 / 2 * 7, 8, 7), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, num + 1E-05f);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, vector + new Vector2(3f, 9f) * 4f + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)), new Rectangle(51, 841, 10, 6), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, num + 1E-05f);
			}
			if (!readyForHarvest)
			{
				return;
			}
			float num4 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 - 8, (float)(y * 64 - 96 - 16) + num4)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-06f + tileLocation.X / 10000f + (((int)parentSheetIndex == 105) ? 0.0015f : 0f));
			if (heldObject.Value != null)
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + num4)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, heldObject.Value.parentSheetIndex, 16, 16), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-05f + tileLocation.X / 10000f + (((int)parentSheetIndex == 105) ? 0.0015f : 0f));
				if (heldObject.Value is ColoredObject)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + num4)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (int)heldObject.Value.parentSheetIndex + 1, 16, 16), (heldObject.Value as ColoredObject).color.Value * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)((y + 1) * 64) / 10000f + 1E-05f + tileLocation.X / 10000f + (((int)parentSheetIndex == 105) ? 0.0015f : 1E-05f));
				}
			}
		}
	}
}
