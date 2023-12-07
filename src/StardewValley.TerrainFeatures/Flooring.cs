using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures
{
	public class Flooring : TerrainFeature
	{
		private struct NeighborLoc
		{
			public readonly Vector2 Offset;

			public readonly byte Direction;

			public readonly byte InvDirection;

			public NeighborLoc(Vector2 a, byte b, byte c)
			{
				Offset = a;
				Direction = b;
				InvDirection = c;
			}
		}

		private struct Neighbor
		{
			public readonly Flooring feature;

			public readonly byte direction;

			public readonly byte invDirection;

			public Neighbor(Flooring a, byte b, byte c)
			{
				feature = a;
				direction = b;
				invDirection = c;
			}
		}

		public const byte N = 1;

		public const byte E = 2;

		public const byte S = 4;

		public const byte W = 8;

		public const byte NE = 16;

		public const byte NW = 32;

		public const byte SE = 64;

		public const byte SW = 128;

		public const byte Cardinals = 15;

		public static readonly Vector2 N_Offset = new Vector2(0f, -1f);

		public static readonly Vector2 E_Offset = new Vector2(1f, 0f);

		public static readonly Vector2 S_Offset = new Vector2(0f, 1f);

		public static readonly Vector2 W_Offset = new Vector2(-1f, 0f);

		public static readonly Vector2 NE_Offset = new Vector2(1f, -1f);

		public static readonly Vector2 NW_Offset = new Vector2(-1f, -1f);

		public static readonly Vector2 SE_Offset = new Vector2(1f, 1f);

		public static readonly Vector2 SW_Offset = new Vector2(-1f, 1f);

		public const int wood = 0;

		public const int stone = 1;

		public const int ghost = 2;

		public const int iceTile = 3;

		public const int straw = 4;

		public const int gravel = 5;

		public const int boardwalk = 6;

		public const int colored_cobblestone = 7;

		public const int cobblestone = 8;

		public const int steppingStone = 9;

		public const int brick = 10;

		public const int plankFlooring = 11;

		public const int townFlooring = 12;

		public static Texture2D floorsTexture;

		public static Texture2D floorsTextureWinter;

		[InstancedStatic]
		public static Dictionary<byte, int> drawGuide;

		[InstancedStatic]
		public static List<int> drawGuideList;

		[XmlElement("whichFloor")]
		public readonly NetInt whichFloor = new NetInt();

		[XmlElement("whichView")]
		public readonly NetInt whichView = new NetInt();

		[XmlElement("isPathway")]
		public readonly NetBool isPathway = new NetBool();

		[XmlElement("isSteppingStone")]
		public readonly NetBool isSteppingStone = new NetBool();

		[XmlElement("drawContouredShadow")]
		public readonly NetBool drawContouredShadow = new NetBool();

		[XmlElement("cornerDecoratedBorders")]
		public readonly NetBool cornerDecoratedBorders = new NetBool();

		private byte neighborMask;

		private static readonly NeighborLoc[] _offsets = new NeighborLoc[8]
		{
			new NeighborLoc(N_Offset, 1, 4),
			new NeighborLoc(S_Offset, 4, 1),
			new NeighborLoc(E_Offset, 2, 8),
			new NeighborLoc(W_Offset, 8, 2),
			new NeighborLoc(NE_Offset, 16, 128),
			new NeighborLoc(NW_Offset, 32, 64),
			new NeighborLoc(SE_Offset, 64, 32),
			new NeighborLoc(SW_Offset, 128, 16)
		};

		private List<Neighbor> _neighbors = new List<Neighbor>();

		public Flooring()
			: base(needsTick: false)
		{
			base.NetFields.AddFields(whichFloor, whichView, isPathway, isSteppingStone, drawContouredShadow, cornerDecoratedBorders);
			loadSprite();
			if (drawGuide == null)
			{
				populateDrawGuide();
			}
		}

		public Flooring(int which)
			: this()
		{
			whichFloor.Value = which;
			ApplyFlooringFlags();
		}

		public virtual void ApplyFlooringFlags()
		{
			if ((int)whichFloor == 5 || (int)whichFloor == 6 || (int)whichFloor == 8 || (int)whichFloor == 7 || (int)whichFloor == 11)
			{
				isPathway.Value = true;
			}
			if ((int)whichFloor == 11 || (int)whichFloor == 12)
			{
				drawContouredShadow.Value = true;
			}
			if ((int)whichFloor == 12)
			{
				cornerDecoratedBorders.Value = true;
			}
			if ((int)whichFloor == 9)
			{
				whichView.Value = Game1.random.Next(16);
				isSteppingStone.Value = true;
				isPathway.Value = true;
			}
		}

		public override Rectangle getBoundingBox(Vector2 tileLocation)
		{
			return new Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 64, 64);
		}

		public static void populateDrawGuide()
		{
			drawGuide = new Dictionary<byte, int>();
			drawGuide.Add(0, 0);
			drawGuide.Add(6, 1);
			drawGuide.Add(14, 2);
			drawGuide.Add(12, 3);
			drawGuide.Add(4, 16);
			drawGuide.Add(7, 17);
			drawGuide.Add(15, 18);
			drawGuide.Add(13, 19);
			drawGuide.Add(5, 32);
			drawGuide.Add(3, 33);
			drawGuide.Add(11, 34);
			drawGuide.Add(9, 35);
			drawGuide.Add(1, 48);
			drawGuide.Add(2, 49);
			drawGuide.Add(10, 50);
			drawGuide.Add(8, 51);
			drawGuideList = new List<int>(drawGuide.Count);
			foreach (KeyValuePair<byte, int> item in drawGuide)
			{
				drawGuideList.Add(item.Value);
			}
		}

		public override void loadSprite()
		{
			if (floorsTexture == null)
			{
				try
				{
					floorsTexture = Game1.content.Load<Texture2D>("TerrainFeatures\\Flooring");
				}
				catch (Exception)
				{
				}
			}
			if (floorsTextureWinter == null)
			{
				try
				{
					floorsTextureWinter = Game1.content.Load<Texture2D>("TerrainFeatures\\Flooring_winter");
				}
				catch (Exception)
				{
				}
			}
			if ((int)whichFloor == 5 || (int)whichFloor == 6 || (int)whichFloor == 8 || (int)whichFloor == 7 || (int)whichFloor == 9)
			{
				isPathway.Value = true;
			}
			if ((int)whichFloor == 9)
			{
				isSteppingStone.Value = true;
			}
		}

		public override void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who, GameLocation location)
		{
			base.doCollisionAction(positionOfCollider, speedOfCollision, tileLocation, who, location);
			if (who != null && who is Farmer && location is Farm)
			{
				(who as Farmer).temporarySpeedBuff = 0.1f;
			}
		}

		public override bool isPassable(Character c = null)
		{
			return true;
		}

		public string getFootstepSound()
		{
			switch ((int)whichFloor)
			{
			case 5:
				return "dirtyHit";
			case 0:
			case 2:
			case 4:
			case 11:
				return "woodyStep";
			case 3:
			case 6:
				return "thudStep";
			case 1:
			case 10:
			case 12:
				return "stoneStep";
			default:
				return "stoneStep";
			}
		}

		private Texture2D getTexture()
		{
			if (Game1.GetSeasonForLocation(currentLocation)[0] == 'w' && (currentLocation == null || !currentLocation.isGreenhouse))
			{
				return floorsTextureWinter;
			}
			return floorsTexture;
		}

		public override bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location)
		{
			if (location == null)
			{
				location = Game1.currentLocation;
			}
			if ((t != null || damage > 0) && (damage > 0 || t is Pickaxe || t is Axe))
			{
				Game1.createRadialDebris(location, ((int)whichFloor == 0) ? 12 : 14, (int)tileLocation.X, (int)tileLocation.Y, 4, resource: false);
				int parentSheetIndex = -1;
				switch ((int)whichFloor)
				{
				case 4:
					location.playSound("axchop");
					parentSheetIndex = 401;
					break;
				case 2:
					location.playSound("axchop");
					parentSheetIndex = 331;
					break;
				case 6:
					location.playSound("axchop");
					parentSheetIndex = 405;
					break;
				case 11:
					location.playSound("axchop");
					parentSheetIndex = 840;
					break;
				case 0:
					location.playSound("axchop");
					parentSheetIndex = 328;
					break;
				case 3:
					location.playSound("hammer");
					parentSheetIndex = 333;
					break;
				case 5:
					location.playSound("hammer");
					parentSheetIndex = 407;
					break;
				case 9:
					location.playSound("hammer");
					parentSheetIndex = 415;
					break;
				case 7:
					location.playSound("hammer");
					parentSheetIndex = 409;
					break;
				case 8:
					location.playSound("hammer");
					parentSheetIndex = 411;
					break;
				case 10:
					location.playSound("hammer");
					parentSheetIndex = 293;
					break;
				case 1:
					location.playSound("hammer");
					parentSheetIndex = 329;
					break;
				case 12:
					location.playSound("hammer");
					parentSheetIndex = 841;
					break;
				}
				location.debris.Add(new Debris(new Object(parentSheetIndex, 1), tileLocation * 64f + new Vector2(32f, 32f)));
				return true;
			}
			return false;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
			int num = 1;
			int num2 = (int)whichFloor * 4 * 64;
			byte b = 0;
			Vector2 key = tileLocation;
			key.X += 1f;
			GameLocation locationFromName = Game1.getLocationFromName("Farm");
			if (locationFromName.terrainFeatures.ContainsKey(key) && locationFromName.terrainFeatures[key] is Flooring)
			{
				b = (byte)(b + 2);
			}
			key.X -= 2f;
			if (locationFromName.terrainFeatures.ContainsKey(key) && Game1.currentLocation.terrainFeatures[key] is Flooring)
			{
				b = (byte)(b + 8);
			}
			key.X += 1f;
			key.Y += 1f;
			if (Game1.currentLocation.terrainFeatures.ContainsKey(key) && locationFromName.terrainFeatures[key] is Flooring)
			{
				b = (byte)(b + 4);
			}
			key.Y -= 2f;
			if (locationFromName.terrainFeatures.ContainsKey(key) && locationFromName.terrainFeatures[key] is Flooring)
			{
				b = (byte)(b + 1);
			}
			num = drawGuide[b];
			spriteBatch.Draw(floorsTexture, positionOnScreen, new Rectangle(num % 16 * 16, num / 16 * 16 + num2, 16, 16), Color.White, 0f, Vector2.Zero, scale * 4f, SpriteEffects.None, layerDepth + positionOnScreen.Y / 20000f);
		}

		public override void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
		{
			if (cornerDecoratedBorders.Value)
			{
				int num = 6;
				if ((neighborMask & 9) == 9 && (neighborMask & 0x20) == 0)
				{
					spriteBatch.Draw(getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(64 - num + 64 * ((int)whichFloor % 4), 48 - num + (int)whichFloor / 4 * 64, num, num), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
				}
				if ((neighborMask & 3) == 3 && (neighborMask & 0x10) == 0)
				{
					spriteBatch.Draw(getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 64f - (float)(num * 4), tileLocation.Y * 64f)), new Rectangle(16 + 64 * ((int)whichFloor % 4), 48 - num + (int)whichFloor / 4 * 64, num, num), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f + (float)(int)whichFloor) / 20000f);
				}
				if ((neighborMask & 6) == 6 && (neighborMask & 0x40) == 0)
				{
					spriteBatch.Draw(getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 64f - (float)(num * 4), tileLocation.Y * 64f + 64f - (float)(num * 4))), new Rectangle(16 + 64 * ((int)whichFloor % 4), (int)whichFloor / 4 * 64, num, num), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
				}
				if ((neighborMask & 0xC) == 12 && (neighborMask & 0x80) == 0)
				{
					spriteBatch.Draw(getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f + 64f - (float)(num * 4))), new Rectangle(64 - num + 64 * ((int)whichFloor % 4), (int)whichFloor / 4 * 64, num, num), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
				}
			}
			else if (!isPathway)
			{
				if ((neighborMask & 9) == 9 && (neighborMask & 0x20) == 0)
				{
					spriteBatch.Draw(getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle(60 + 64 * ((int)whichFloor % 4), 44 + (int)whichFloor / 4 * 64, 4, 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
				}
				if ((neighborMask & 3) == 3 && (neighborMask & 0x10) == 0)
				{
					spriteBatch.Draw(getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 48f, tileLocation.Y * 64f)), new Rectangle(16 + 64 * ((int)whichFloor % 4), 44 + (int)whichFloor / 4 * 64, 4, 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f + (float)(int)whichFloor) / 20000f);
				}
				if ((neighborMask & 6) == 6 && (neighborMask & 0x40) == 0)
				{
					spriteBatch.Draw(getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 48f, tileLocation.Y * 64f + 48f)), new Rectangle(16 + 64 * ((int)whichFloor % 4), (int)whichFloor / 4 * 64, 4, 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
				}
				if ((neighborMask & 0xC) == 12 && (neighborMask & 0x80) == 0)
				{
					spriteBatch.Draw(getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f + 48f)), new Rectangle(60 + 64 * ((int)whichFloor % 4), (int)whichFloor / 4 * 64, 4, 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 2f + tileLocation.X / 10000f) / 20000f);
				}
				if (!drawContouredShadow.Value)
				{
					spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)(tileLocation.X * 64f) - 4 - Game1.viewport.X, (int)(tileLocation.Y * 64f) + 4 - Game1.viewport.Y, 64, 64), Color.Black * 0.33f);
				}
			}
			byte key = (byte)(neighborMask & 0xFu);
			int num2 = drawGuide[key];
			if ((bool)isSteppingStone)
			{
				num2 = drawGuideList[whichView.Value];
			}
			if ((bool)drawContouredShadow)
			{
				Color black = Color.Black;
				black.A = (byte)((float)(int)black.A * 0.33f);
				spriteBatch.Draw(getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)) + new Vector2(-4f, 4f), new Rectangle((int)whichFloor % 4 * 64 + num2 * 16 % 256, num2 / 16 * 16 + (int)whichFloor / 4 * 64, 16, 16), black, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-10f);
			}
			spriteBatch.Draw(getTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), new Rectangle((int)whichFloor % 4 * 64 + num2 * 16 % 256, num2 / 16 * 16 + (int)whichFloor / 4 * 64, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-09f);
		}

		public override bool tickUpdate(GameTime time, Vector2 tileLocation, GameLocation location)
		{
			base.NeedsUpdate = false;
			return false;
		}

		public override void dayUpdate(GameLocation environment, Vector2 tileLocation)
		{
		}

		public override bool seasonUpdate(bool onLoad)
		{
			return false;
		}

		private List<Neighbor> gatherNeighbors(GameLocation loc, Vector2 tilePos)
		{
			List<Neighbor> neighbors = _neighbors;
			neighbors.Clear();
			TerrainFeature value = null;
			Flooring flooring = null;
			NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = loc.terrainFeatures;
			NeighborLoc[] offsets = _offsets;
			for (int i = 0; i < offsets.Length; i++)
			{
				NeighborLoc neighborLoc = offsets[i];
				Vector2 vector = tilePos + neighborLoc.Offset;
				if (loc.map != null && !loc.isTileOnMap(vector))
				{
					Neighbor item = new Neighbor(null, neighborLoc.Direction, neighborLoc.InvDirection);
					neighbors.Add(item);
				}
				else if (terrainFeatures.TryGetValue(vector, out value) && value != null && value is Flooring flooring2 && flooring2.whichFloor == whichFloor)
				{
					Neighbor item2 = new Neighbor(flooring2, neighborLoc.Direction, neighborLoc.InvDirection);
					neighbors.Add(item2);
				}
			}
			return neighbors;
		}

		public void OnAdded(GameLocation loc, Vector2 tilePos)
		{
			List<Neighbor> list = gatherNeighbors(loc, tilePos);
			neighborMask = 0;
			foreach (Neighbor item in list)
			{
				neighborMask |= item.direction;
				if (item.feature != null)
				{
					item.feature.OnNeighborAdded(item.invDirection);
				}
			}
		}

		public void OnRemoved(GameLocation loc, Vector2 tilePos)
		{
			List<Neighbor> list = gatherNeighbors(loc, tilePos);
			neighborMask = 0;
			foreach (Neighbor item in list)
			{
				if (item.feature != null)
				{
					item.feature.OnNeighborRemoved(item.invDirection);
				}
			}
		}

		public void OnNeighborAdded(byte direction)
		{
			neighborMask |= direction;
		}

		public void OnNeighborRemoved(byte direction)
		{
			neighborMask = (byte)(neighborMask & ~direction);
		}
	}
}
