using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace StardewValley
{
	public class FarmActivity
	{
		protected NPC _character;

		public Vector2 activityPosition;

		public int activityDirection = 2;

		public float weight = 1f;

		protected float _age;

		protected bool _performingActivity;

		public virtual FarmActivity Initialize(NPC character, float activity_weight = 1f)
		{
			_character = character;
			weight = activity_weight;
			return this;
		}

		public virtual bool AttemptActivity(Farm farm)
		{
			if (_AttemptActivity(farm))
			{
				return true;
			}
			return false;
		}

		protected virtual bool _AttemptActivity(Farm farm)
		{
			return false;
		}

		public bool Update(GameTime time)
		{
			_age += (float)time.ElapsedGameTime.TotalSeconds;
			return _Update(time);
		}

		protected virtual bool _Update(GameTime time)
		{
			if (_age >= 10f)
			{
				return true;
			}
			return false;
		}

		public bool IsPerformingActivity()
		{
			return _performingActivity;
		}

		public void BeginActivity()
		{
			_character.faceDirection(activityDirection);
			_age = 0f;
			_performingActivity = true;
			_BeginActivity();
		}

		protected virtual void _BeginActivity()
		{
		}

		public void EndActivity()
		{
			_performingActivity = false;
			_EndActivity();
		}

		protected virtual void _EndActivity()
		{
		}

		public virtual bool IsTileBlockedFromSight(Vector2 tile)
		{
			return false;
		}

		public Rectangle GetFarmBounds(Farm farm)
		{
			return new Rectangle(0, 0, farm.map.Layers[0].LayerWidth, farm.map.Layers[0].LayerHeight);
		}

		public Object GetRandomObject(Farm farm, Func<Object, bool> validator = null)
		{
			List<Object> list = new List<Object>();
			foreach (Vector2 key in farm.objects.Keys)
			{
				Object @object = farm.objects[key];
				if (@object != null && (validator == null || validator(@object)))
				{
					list.Add(@object);
				}
			}
			return Utility.GetRandom(list);
		}

		public TerrainFeature GetRandomTerrainFeature(Farm farm, Func<TerrainFeature, bool> validator = null)
		{
			List<TerrainFeature> list = new List<TerrainFeature>();
			foreach (Vector2 key in farm.terrainFeatures.Keys)
			{
				TerrainFeature terrainFeature = farm.terrainFeatures[key];
				if (terrainFeature != null && (validator == null || validator(terrainFeature)))
				{
					list.Add(terrainFeature);
				}
			}
			return Utility.GetRandom(list);
		}

		public HoeDirt GetRandomCrop(Farm farm, Func<Crop, bool> validator = null)
		{
			List<HoeDirt> list = new List<HoeDirt>();
			foreach (Vector2 key in farm.terrainFeatures.Keys)
			{
				TerrainFeature terrainFeature = farm.terrainFeatures[key];
				if (terrainFeature is HoeDirt hoeDirt && hoeDirt.crop != null && (validator == null || validator(hoeDirt.crop)))
				{
					list.Add(hoeDirt);
				}
			}
			return Utility.GetRandom(list);
		}

		public Vector2 GetNearbyTile(Farm farm, Vector2 tile)
		{
			return Utility.getRandomAdjacentOpenTile(tile, farm);
		}
	}
}
