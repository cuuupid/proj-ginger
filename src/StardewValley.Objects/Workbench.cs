using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;

namespace StardewValley.Objects
{
	public class Workbench : Object
	{
		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(mutex.NetFields);
		}

		public Workbench()
		{
		}

		public Workbench(Vector2 position)
			: base(position, 208)
		{
			Name = "Workbench";
			type.Value = "Crafting";
			bigCraftable.Value = true;
			canBeSetDown.Value = true;
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			List<Chest> nearby_chests = new List<Chest>();
			Vector2[] array = new Vector2[8]
			{
				new Vector2(-1f, 1f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(-1f, 0f),
				new Vector2(1f, 0f),
				new Vector2(-1f, -1f),
				new Vector2(0f, -1f),
				new Vector2(1f, -1f)
			};
			for (int i = 0; i < array.Length; i++)
			{
				if (who.currentLocation is FarmHouse)
				{
					int tileIndexAt = who.currentLocation.getTileIndexAt((int)(tileLocation.X + array[i].X), (int)(tileLocation.Y + array[i].Y), "Buildings");
					if (tileIndexAt == 173)
					{
						nearby_chests.Add((who.currentLocation as FarmHouse).fridge.Value);
						continue;
					}
				}
				if (who.currentLocation is IslandFarmHouse)
				{
					int tileIndexAt2 = who.currentLocation.getTileIndexAt((int)(tileLocation.X + array[i].X), (int)(tileLocation.Y + array[i].Y), "Buildings");
					if (tileIndexAt2 == 173)
					{
						nearby_chests.Add((who.currentLocation as IslandFarmHouse).fridge.Value);
						continue;
					}
				}
				Vector2 key = new Vector2((int)(tileLocation.X + array[i].X), (int)(tileLocation.Y + array[i].Y));
				Object @object = null;
				if (who.currentLocation.objects.ContainsKey(key))
				{
					@object = who.currentLocation.objects[key];
				}
				if (@object != null && @object is Chest && (@object as Chest).SpecialChestType == Chest.SpecialChestTypes.None)
				{
					nearby_chests.Add(@object as Chest);
				}
			}
			List<NetMutex> list = new List<NetMutex>();
			foreach (Chest item in nearby_chests)
			{
				list.Add(item.mutex);
			}
			if (!mutex.IsLocked())
			{
				MultipleMutexRequest multipleMutexRequest = null;
				multipleMutexRequest = new MultipleMutexRequest(list, delegate
				{
					mutex.RequestLock(delegate
					{
						Game1.activeClickableMenu = new CraftingPageMobile(0, 0, 0, 0, cooking: false, 300, nearby_chests);
						Game1.activeClickableMenu.exitFunction = delegate
						{
							mutex.ReleaseLock();
							multipleMutexRequest.ReleaseLocks();
						};
					}, delegate
					{
						multipleMutexRequest.ReleaseLocks();
					});
				}, delegate
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"));
				});
			}
			return true;
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			mutex.Update(environment);
			base.updateWhenCurrentLocation(time, environment);
		}
	}
}
