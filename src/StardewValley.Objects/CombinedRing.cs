using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Menus;
using StardewValley.Monsters;

namespace StardewValley.Objects
{
	public class CombinedRing : Ring
	{
		public NetList<Ring, NetRef<Ring>> combinedRings = new NetList<Ring, NetRef<Ring>>();

		public CombinedRing()
		{
			base.NetFields.AddField(combinedRings);
		}

		public CombinedRing(int parent_sheet_index)
			: base(880)
		{
			base.NetFields.AddField(combinedRings);
		}

		public virtual void UpdateDescription()
		{
			loadDisplayFields();
		}

		protected override bool loadDisplayFields()
		{
			base.loadDisplayFields();
			description = "";
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.getDescription();
				description = description + combinedRing.description + "\n\n";
			}
			description = description.Trim();
			return true;
		}

		public override bool GetsEffectOfRing(int ring_index)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				if (combinedRing.GetsEffectOfRing(ring_index))
				{
					return true;
				}
			}
			return base.GetsEffectOfRing(ring_index);
		}

		public override Item getOne()
		{
			CombinedRing combinedRing = new CombinedRing(indexInTileSheet);
			combinedRing._GetOneFrom(this);
			return combinedRing;
		}

		public override void _GetOneFrom(Item source)
		{
			combinedRings.Clear();
			foreach (Ring combinedRing in (source as CombinedRing).combinedRings)
			{
				Ring item = combinedRing.getOne() as Ring;
				combinedRings.Add(item);
			}
			loadDisplayFields();
			base._GetOneFrom(source);
		}

		public override int GetEffectsOfRingMultiplier(int ring_index)
		{
			int num = 0;
			foreach (Ring combinedRing in combinedRings)
			{
				num += combinedRing.GetEffectsOfRingMultiplier(ring_index);
			}
			return num;
		}

		public override void onDayUpdate(Farmer who, GameLocation location)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.onDayUpdate(who, location);
			}
			base.onDayUpdate(who, location);
		}

		public override void onEquip(Farmer who, GameLocation location)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.onEquip(who, location);
			}
			base.onEquip(who, location);
		}

		public override void onLeaveLocation(Farmer who, GameLocation environment)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.onLeaveLocation(who, environment);
			}
			base.onLeaveLocation(who, environment);
		}

		public override void onMonsterSlay(Monster m, GameLocation location, Farmer who)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.onMonsterSlay(m, location, who);
			}
			base.onMonsterSlay(m, location, who);
		}

		public override void onUnequip(Farmer who, GameLocation location)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.onUnequip(who, location);
			}
			base.onUnequip(who, location);
		}

		public override void onNewLocation(Farmer who, GameLocation environment)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.onNewLocation(who, environment);
			}
			base.onNewLocation(who, environment);
		}

		public void FixCombinedRing()
		{
			if (base.ParentSheetIndex != 880)
			{
				string[] array = Game1.objectInformation[880].Split('/');
				base.Category = -96;
				Name = array[0];
				price.Value = Convert.ToInt32(array[1]);
				indexInTileSheet.Value = 880;
				base.ParentSheetIndex = indexInTileSheet;
				loadDisplayFields();
			}
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			if (combinedRings.Count >= 2)
			{
				float num = scaleSize;
				scaleSize = 1f;
				location.Y -= (num - 1f) * 32f;
				Rectangle sourceRectForStandardTileSheet = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, combinedRings[0].indexInTileSheet, 16, 16);
				sourceRectForStandardTileSheet.X += 5;
				sourceRectForStandardTileSheet.Y += 7;
				sourceRectForStandardTileSheet.Width = 4;
				sourceRectForStandardTileSheet.Height = 6;
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2) * scaleSize + new Vector2(-12f, 8f) * scaleSize, sourceRectForStandardTileSheet, color * transparency, 0f, new Vector2(0.75f, 2.1f) * 0.4f * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				sourceRectForStandardTileSheet.X++;
				sourceRectForStandardTileSheet.Y += 4;
				sourceRectForStandardTileSheet.Width = 3;
				sourceRectForStandardTileSheet.Height = 1;
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2) * scaleSize + new Vector2(-8f, 4f) * scaleSize, sourceRectForStandardTileSheet, color * transparency, 0f, new Vector2(0.75f, 2.1f) * 0.4f * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				sourceRectForStandardTileSheet = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, combinedRings[1].indexInTileSheet, 16, 16);
				sourceRectForStandardTileSheet.X += 9;
				sourceRectForStandardTileSheet.Y += 7;
				sourceRectForStandardTileSheet.Width = 4;
				sourceRectForStandardTileSheet.Height = 6;
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2) * scaleSize + new Vector2(4f, 8f) * scaleSize, sourceRectForStandardTileSheet, color * transparency, 0f, new Vector2(0.75f, 2.1f) * 0.4f * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				sourceRectForStandardTileSheet.Y += 4;
				sourceRectForStandardTileSheet.Width = 3;
				sourceRectForStandardTileSheet.Height = 1;
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(base.itemSlotSize / 2, base.itemSlotSize / 2) * scaleSize + new Vector2(4f, 4f) * scaleSize, sourceRectForStandardTileSheet, color * transparency, 0f, new Vector2(0.75f, 2.1f) * 0.4f * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				Color? dyeColor = TailoringMenu.GetDyeColor(combinedRings[0]);
				Color? dyeColor2 = TailoringMenu.GetDyeColor(combinedRings[1]);
				Color color2 = Color.Red;
				Color color3 = Color.Blue;
				if (dyeColor.HasValue)
				{
					color2 = dyeColor.Value;
				}
				if (dyeColor2.HasValue)
				{
					color3 = dyeColor2.Value;
				}
				base.drawInMenu(spriteBatch, location + new Vector2(-5f, -1f), scaleSize, transparency, layerDepth, drawStackNumber, Utility.Get2PhaseColor(color2, color3), drawShadow);
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(base.itemSlotSize / 2 - 20 + 1, base.itemSlotSize / 2 + 4 - 1) * scaleSize, new Rectangle(263, 579, 4, 2), Utility.Get2PhaseColor(color2, color3, 0, 1f, 1125f) * transparency, -(float)Math.PI / 2f, new Vector2(2f, 1.5f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(base.itemSlotSize / 2 + 16 + 1, base.itemSlotSize / 2 + 4 - 1) * scaleSize, new Rectangle(263, 579, 4, 2), Utility.Get2PhaseColor(color2, color3, 0, 1f, 375f) * transparency, (float)Math.PI / 2f, new Vector2(2f, 1.5f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(base.itemSlotSize / 2 - 1, base.itemSlotSize / 2 + 20 + 1) * scaleSize, new Rectangle(263, 579, 4, 2), Utility.Get2PhaseColor(color2, color3, 0, 1f, 750f) * transparency, (float)Math.PI, new Vector2(2f, 1.5f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
			}
			else
			{
				base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
			}
		}

		public override void update(GameTime time, GameLocation environment, Farmer who)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.update(time, environment, who);
			}
			base.update(time, environment, who);
		}
	}
}
