using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Objects;

namespace StardewValley.Locations
{
	public class IslandShrine : IslandForestLocation
	{
		[XmlIgnore]
		public ItemPedestal northPedestal = new NetRef<ItemPedestal>();

		[XmlIgnore]
		public ItemPedestal southPedestal = new NetRef<ItemPedestal>();

		[XmlIgnore]
		public ItemPedestal eastPedestal = new NetRef<ItemPedestal>();

		[XmlIgnore]
		public ItemPedestal westPedestal = new NetRef<ItemPedestal>();

		[XmlIgnore]
		public NetEvent0 puzzleFinishedEvent = new NetEvent0();

		[XmlElement("puzzleFinished")]
		public NetBool puzzleFinished = new NetBool();

		public IslandShrine()
		{
		}

		public IslandShrine(string map, string name)
			: base(map, name)
		{
			AddMissingPedestals();
		}

		public override List<Vector2> GetAdditionalWalnutBushes()
		{
			List<Vector2> list = new List<Vector2>();
			list.Add(new Vector2(23f, 34f));
			return list;
		}

		public virtual void AddMissingPedestals()
		{
			Vector2 vector = new Vector2(0f, 0f);
			vector.X = 21f;
			vector.Y = 27f;
			IslandGemBird.GemBirdType gemBirdType = IslandGemBird.GemBirdType.Amethyst;
			Object objectAtTile = getObjectAtTile((int)vector.X, (int)vector.Y);
			gemBirdType = IslandGemBird.GetBirdTypeForLocation("IslandWest");
			if (objectAtTile == null)
			{
				westPedestal = new ItemPedestal(vector, null, lock_on_success: false, Color.White);
				objects.Add(vector, westPedestal);
				westPedestal.requiredItem.Value = new Object(Vector2.Zero, IslandGemBird.GetItemIndex(gemBirdType), 1);
				westPedestal.successColor.Value = new Color(0, 0, 0, 0);
			}
			else if (objectAtTile is ItemPedestal)
			{
				ItemPedestal itemPedestal = objectAtTile as ItemPedestal;
				int itemIndex = IslandGemBird.GetItemIndex(gemBirdType);
				if (itemPedestal.requiredItem.Value == null || itemPedestal.requiredItem.Value.ParentSheetIndex != itemIndex)
				{
					itemPedestal.requiredItem.Value = new Object(Vector2.Zero, itemIndex, 1);
					if (itemPedestal.heldObject.Value != null && itemPedestal.heldObject.Value.ParentSheetIndex != itemIndex)
					{
						itemPedestal.heldObject.Value = null;
					}
				}
			}
			vector.X = 27f;
			vector.Y = 27f;
			objectAtTile = getObjectAtTile((int)vector.X, (int)vector.Y);
			gemBirdType = IslandGemBird.GetBirdTypeForLocation("IslandEast");
			if (objectAtTile == null)
			{
				eastPedestal = new ItemPedestal(vector, null, lock_on_success: false, Color.White);
				objects.Add(vector, eastPedestal);
				eastPedestal.requiredItem.Value = new Object(Vector2.Zero, IslandGemBird.GetItemIndex(gemBirdType), 1);
				eastPedestal.successColor.Value = new Color(0, 0, 0, 0);
			}
			else if (objectAtTile is ItemPedestal)
			{
				ItemPedestal itemPedestal2 = objectAtTile as ItemPedestal;
				int itemIndex2 = IslandGemBird.GetItemIndex(gemBirdType);
				if (itemPedestal2.requiredItem.Value == null || itemPedestal2.requiredItem.Value.ParentSheetIndex != itemIndex2)
				{
					itemPedestal2.requiredItem.Value = new Object(Vector2.Zero, itemIndex2, 1);
					if (itemPedestal2.heldObject.Value != null && itemPedestal2.heldObject.Value.ParentSheetIndex != itemIndex2)
					{
						itemPedestal2.heldObject.Value = null;
					}
				}
			}
			vector.X = 24f;
			vector.Y = 28f;
			objectAtTile = getObjectAtTile((int)vector.X, (int)vector.Y);
			gemBirdType = IslandGemBird.GetBirdTypeForLocation("IslandSouth");
			if (objectAtTile == null)
			{
				southPedestal = new ItemPedestal(vector, null, lock_on_success: false, Color.White);
				objects.Add(vector, southPedestal);
				southPedestal.requiredItem.Value = new Object(Vector2.Zero, IslandGemBird.GetItemIndex(gemBirdType), 1);
				southPedestal.successColor.Value = new Color(0, 0, 0, 0);
			}
			else if (objectAtTile is ItemPedestal)
			{
				ItemPedestal itemPedestal3 = objectAtTile as ItemPedestal;
				int itemIndex3 = IslandGemBird.GetItemIndex(gemBirdType);
				if (itemPedestal3.requiredItem.Value == null || itemPedestal3.requiredItem.Value.ParentSheetIndex != itemIndex3)
				{
					itemPedestal3.requiredItem.Value = new Object(Vector2.Zero, itemIndex3, 1);
					if (itemPedestal3.heldObject.Value != null && itemPedestal3.heldObject.Value.ParentSheetIndex != itemIndex3)
					{
						itemPedestal3.heldObject.Value = null;
					}
				}
			}
			vector.X = 24f;
			vector.Y = 25f;
			objectAtTile = getObjectAtTile((int)vector.X, (int)vector.Y);
			gemBirdType = IslandGemBird.GetBirdTypeForLocation("IslandNorth");
			if (objectAtTile == null)
			{
				northPedestal = new ItemPedestal(vector, null, lock_on_success: false, Color.White);
				objects.Add(vector, northPedestal);
				northPedestal.requiredItem.Value = new Object(Vector2.Zero, IslandGemBird.GetItemIndex(gemBirdType), 1);
				northPedestal.successColor.Value = new Color(0, 0, 0, 0);
			}
			else
			{
				if (!(objectAtTile is ItemPedestal))
				{
					return;
				}
				ItemPedestal itemPedestal4 = objectAtTile as ItemPedestal;
				int itemIndex4 = IslandGemBird.GetItemIndex(gemBirdType);
				if (itemPedestal4.requiredItem.Value == null || itemPedestal4.requiredItem.Value.ParentSheetIndex != itemIndex4)
				{
					itemPedestal4.requiredItem.Value = new Object(Vector2.Zero, itemIndex4, 1);
					if (itemPedestal4.heldObject.Value != null && itemPedestal4.heldObject.Value.ParentSheetIndex != itemIndex4)
					{
						itemPedestal4.heldObject.Value = null;
					}
				}
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(puzzleFinished, puzzleFinishedEvent);
			puzzleFinishedEvent.onEvent += OnPuzzleFinish;
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.IsMasterGame)
			{
				AddMissingPedestals();
			}
		}

		public override void MakeMapModifications(bool force = false)
		{
			base.MakeMapModifications(force);
			if (puzzleFinished.Value)
			{
				ApplyFinishedTiles();
			}
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			if (l is IslandShrine)
			{
				IslandShrine islandShrine = l as IslandShrine;
				northPedestal = islandShrine.getObjectAtTile((int)northPedestal.TileLocation.X, (int)northPedestal.TileLocation.Y) as ItemPedestal;
				southPedestal = islandShrine.getObjectAtTile((int)southPedestal.TileLocation.X, (int)southPedestal.TileLocation.Y) as ItemPedestal;
				eastPedestal = islandShrine.getObjectAtTile((int)eastPedestal.TileLocation.X, (int)eastPedestal.TileLocation.Y) as ItemPedestal;
				westPedestal = islandShrine.getObjectAtTile((int)westPedestal.TileLocation.X, (int)westPedestal.TileLocation.Y) as ItemPedestal;
				puzzleFinished.Value = islandShrine.puzzleFinished.Value;
			}
		}

		public void OnPuzzleFinish()
		{
			if (Game1.IsMasterGame)
			{
				Game1.createItemDebris(new Object(73, 1), new Vector2(24f, 19f) * 64f, -1, this);
				Game1.createItemDebris(new Object(73, 1), new Vector2(24f, 19f) * 64f, -1, this);
				Game1.createItemDebris(new Object(73, 1), new Vector2(24f, 19f) * 64f, -1, this);
				Game1.createItemDebris(new Object(73, 1), new Vector2(24f, 19f) * 64f, -1, this);
				Game1.createItemDebris(new Object(73, 1), new Vector2(24f, 19f) * 64f, -1, this);
			}
			if (Game1.currentLocation == this)
			{
				Game1.playSound("boulderBreak");
				Game1.playSound("secret1");
				Game1.flashAlpha = 1f;
				ApplyFinishedTiles();
			}
		}

		public virtual void ApplyFinishedTiles()
		{
			setMapTileIndex(23, 19, 142, "AlwaysFront", 2);
			setMapTileIndex(24, 19, 143, "AlwaysFront", 2);
			setMapTileIndex(25, 19, 144, "AlwaysFront", 2);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (Game1.IsMasterGame && !puzzleFinished.Value && northPedestal.match.Value && southPedestal.match.Value && eastPedestal.match.Value && westPedestal.match.Value)
			{
				Game1.player.team.MarkCollectedNut("IslandShrinePuzzle");
				puzzleFinishedEvent.Fire();
				puzzleFinished.Value = true;
				northPedestal.locked.Value = true;
				northPedestal.heldObject.Value = null;
				southPedestal.locked.Value = true;
				southPedestal.heldObject.Value = null;
				eastPedestal.locked.Value = true;
				eastPedestal.heldObject.Value = null;
				westPedestal.locked.Value = true;
				westPedestal.heldObject.Value = null;
			}
		}
	}
}
