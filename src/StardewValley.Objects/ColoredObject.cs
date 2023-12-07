using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Objects
{
	public class ColoredObject : Object
	{
		[XmlElement("color")]
		public readonly NetColor color = new NetColor();

		[XmlElement("colorSameIndexAsParentSheetIndex")]
		public readonly NetBool colorSameIndexAsParentSheetIndex = new NetBool();

		public bool ColorSameIndexAsParentSheetIndex
		{
			get
			{
				return colorSameIndexAsParentSheetIndex.Value;
			}
			set
			{
				colorSameIndexAsParentSheetIndex.Value = value;
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(color);
			base.NetFields.AddField(colorSameIndexAsParentSheetIndex);
		}

		public ColoredObject()
		{
		}

		public ColoredObject(int parentSheetIndex, int stack, Color color)
			: base(parentSheetIndex, stack)
		{
			this.color.Value = color;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color colorOverride, bool drawShadow)
		{
			if ((bool)isRecipe)
			{
				transparency = 0.5f;
				scaleSize *= 0.75f;
			}
			if ((bool)bigCraftable)
			{
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, location + new Vector2((int)((float)(base.itemSlotSize / 2) * scaleSize), (int)((float)(base.itemSlotSize / 2) * scaleSize)), Object.getSourceRectForBigCraftable(parentSheetIndex), Color.White * transparency, 0f, new Vector2(32f, 64f) * scaleSize, (scaleSize < 0.2f) ? scaleSize : (scaleSize / 2f), SpriteEffects.None, layerDepth);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, location + new Vector2((int)((float)(base.itemSlotSize / 2) * scaleSize), (int)((float)(base.itemSlotSize / 2) * scaleSize)), Object.getSourceRectForBigCraftable((int)parentSheetIndex + 1), color.Value * transparency, 0f, new Vector2(32f, 64f) * scaleSize, (scaleSize < 0.2f) ? scaleSize : (scaleSize / 2f), SpriteEffects.None, layerDepth + 2E-05f);
			}
			else
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2((int)((float)(base.itemSlotSize / 2) * scaleSize), (int)((float)(base.itemSlotSize / 2) * scaleSize)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2((int)((float)(base.itemSlotSize / 2) * scaleSize), (int)((float)(base.itemSlotSize / 2) * scaleSize)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (int)parentSheetIndex + 1, 16, 16), color.Value * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth + 2E-05f);
				if (drawStackNumber == StackDrawType.Draw && maximumStackSize() > 1 && (double)scaleSize > 0.3 && Stack != 2147483647 && Stack > 1)
				{
					Utility.drawTinyDigits(stack, spriteBatch, location + new Vector2((float)(base.itemSlotSize - Utility.getWidthOfTinyDigitString(stack, 3f * scaleSize)) - 3f * scaleSize, (float)base.itemSlotSize - 24f * scaleSize), 3f * scaleSize, layerDepth, Color.White);
				}
				if (drawStackNumber == StackDrawType.Draw && (int)quality > 0)
				{
					float num = 0f;
					spriteBatch.Draw(Game1.mouseCursors, location + new Vector2((float)(base.itemSlotSize / 2) - 30f * scaleSize, (float)base.itemSlotSize - 24f * scaleSize), ((int)quality < 4) ? new Rectangle(338 + ((int)quality - 1) * 8, 400, 8, 8) : new Rectangle(346, 391, 8, 8), Color.White * transparency, 0f, new Vector2(4f, 4f), 3f * scaleSize * (1f + num), SpriteEffects.None, layerDepth);
				}
			}
			if ((bool)isRecipe)
			{
				scaleSize *= 0.5f;
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(-12f * scaleSize, -20f * scaleSize), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 451, 16, 16), Color.White, 0f, Vector2.Zero, 4f * scaleSize, SpriteEffects.None, layerDepth + 0.0001f);
			}
		}

		public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
			base.drawWhenHeld(spriteBatch, objectPosition, f);
			float layerDepth = Math.Max(0f, (float)(f.getStandingY() + 10) / 10000f);
			spriteBatch.Draw(Game1.objectSpriteSheet, objectPosition, GameLocation.getSourceRectForObject(f.ActiveObject.ParentSheetIndex + 1), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
		}

		public override Item getOne()
		{
			ColoredObject coloredObject = new ColoredObject(parentSheetIndex, 1, color)
			{
				Quality = quality,
				Price = price,
				HasBeenInInventory = base.HasBeenInInventory,
				HasBeenPickedUpByFarmer = hasBeenPickedUpByFarmer,
				SpecialVariable = base.SpecialVariable
			};
			coloredObject.preserve.Set(preserve.Value);
			coloredObject.preservedParentSheetIndex.Set(preservedParentSheetIndex.Value);
			coloredObject.Name = Name;
			coloredObject.colorSameIndexAsParentSheetIndex.Value = colorSameIndexAsParentSheetIndex.Value;
			coloredObject._GetOneFrom(this);
			return coloredObject;
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if ((bool)bigCraftable)
			{
				Vector2 vector = getScale();
				Vector2 vector2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
				Rectangle destinationRectangle = new Rectangle((int)(vector2.X - vector.X / 2f), (int)(vector2.Y - vector.Y / 2f), (int)(64f + vector.X), (int)(128f + vector.Y / 2f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, Object.getSourceRectForBigCraftable(showNextIndex ? (base.ParentSheetIndex + 1) : base.ParentSheetIndex), Color.White, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, Object.getSourceRectForBigCraftable(base.ParentSheetIndex + 1), color, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
				if (Name.Equals("Loom") && (int)minutesUntilReady > 0)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16), Color.White, scale.X, new Vector2(32f, 32f), 1f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
				}
			}
			else if (!Game1.eventUp || Game1.currentLocation.IsFarm)
			{
				if (!ColorSameIndexAsParentSheetIndex)
				{
					if ((int)parentSheetIndex != 590)
					{
						spriteBatch.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 1E-07f);
					}
					Texture2D objectSpriteSheet = Game1.objectSpriteSheet;
					Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 32));
					Rectangle? sourceRectangle = GameLocation.getSourceRectForObject(base.ParentSheetIndex);
					Color white = Color.White;
					Vector2 origin = new Vector2(8f, 8f);
					_ = scale;
					spriteBatch.Draw(objectSpriteSheet, position, sourceRectangle, white, 0f, origin, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)getBoundingBox(new Vector2(x, y)).Bottom / 10000f);
				}
				Texture2D objectSpriteSheet2 = Game1.objectSpriteSheet;
				Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
				Rectangle? sourceRectangle2 = GameLocation.getSourceRectForObject(base.ParentSheetIndex + ((!colorSameIndexAsParentSheetIndex) ? 1 : 0));
				Color obj = color;
				Vector2 origin2 = new Vector2(8f, 8f);
				_ = scale;
				spriteBatch.Draw(objectSpriteSheet2, position2, sourceRectangle2, obj, 0f, origin2, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)getBoundingBox(new Vector2(x, y)).Bottom / 10000f);
			}
			if (Name != null && Name.Contains("Table") && heldObject.Value != null)
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (bigCraftable ? 48 : 21))), GameLocation.getSourceRectForObject(heldObject.Value.ParentSheetIndex), Color.White, 0f, Vector2.Zero, 1f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(y * 64 + 64 + 1) / 10000f);
			}
		}
	}
}
