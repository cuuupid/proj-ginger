using System;
using System.Collections.Generic;
using Netcode;
using StardewValley.Network;

namespace StardewValley.Util
{
	public class SynchronizedShopStock : INetObject<NetFields>
	{
		public enum SynchedShop
		{
			Krobus,
			TravelingMerchant,
			Sandy,
			Saloon
		}

		private readonly NetIntDictionary<int, NetInt> lastDayUpdated = new NetIntDictionary<int, NetInt>();

		private readonly NetStringDictionary<int, NetInt> sharedKrobusStock = new NetStringDictionary<int, NetInt>();

		private readonly NetStringDictionary<int, NetInt> sharedSandyStock = new NetStringDictionary<int, NetInt>();

		private readonly NetStringDictionary<int, NetInt> sharedTravelingMerchantStock = new NetStringDictionary<int, NetInt>();

		private readonly NetStringDictionary<int, NetInt> sharedSaloonStock = new NetStringDictionary<int, NetInt>();

		public NetFields NetFields { get; } = new NetFields();


		public SynchronizedShopStock()
		{
			initNetFields();
		}

		private void initNetFields()
		{
			NetFields.AddFields(lastDayUpdated, sharedKrobusStock, sharedSandyStock, sharedTravelingMerchantStock, sharedSaloonStock);
		}

		private NetStringDictionary<int, NetInt> getSharedStock(SynchedShop shop)
		{
			switch (shop)
			{
			case SynchedShop.Krobus:
				return sharedKrobusStock;
			case SynchedShop.Sandy:
				return sharedSandyStock;
			case SynchedShop.TravelingMerchant:
				return sharedTravelingMerchantStock;
			case SynchedShop.Saloon:
				return sharedSaloonStock;
			default:
				Console.WriteLine("Tried to get shared stock for invalid shop " + shop);
				return null;
			}
		}

		private int getLastDayUpdated(SynchedShop shop)
		{
			if (!lastDayUpdated.ContainsKey((int)shop))
			{
				lastDayUpdated[(int)shop] = -1;
			}
			return lastDayUpdated[(int)shop];
		}

		private int setLastDayUpdated(SynchedShop shop, int value)
		{
			if (!lastDayUpdated.ContainsKey((int)shop))
			{
				lastDayUpdated[(int)shop] = 0;
			}
			return lastDayUpdated[(int)shop] = value;
		}

		public void OnItemPurchased(SynchedShop shop, ISalable item, int amount)
		{
			NetStringDictionary<int, NetInt> sharedStock = getSharedStock(shop);
			string standardDescriptionFromItem = Utility.getStandardDescriptionFromItem(item as Item, 1);
			if (sharedStock.ContainsKey(standardDescriptionFromItem) && sharedStock[standardDescriptionFromItem] != 2147483647 && (!(item is Object) || !(item as Object).IsRecipe))
			{
				sharedStock[standardDescriptionFromItem] -= amount;
			}
		}

		public void UpdateLocalStockWithSyncedQuanitities(SynchedShop shop, Dictionary<ISalable, int[]> localStock, Dictionary<string, Func<bool>> conditionalItemFilters = null)
		{
			List<Item> list = new List<Item>();
			NetStringDictionary<int, NetInt> sharedStock = getSharedStock(shop);
			int num = getLastDayUpdated(shop);
			if (num != Game1.Date.TotalDays)
			{
				setLastDayUpdated(shop, Game1.Date.TotalDays);
				sharedStock.Clear();
				foreach (Item key in localStock.Keys)
				{
					string standardDescriptionFromItem = Utility.getStandardDescriptionFromItem(key, 1);
					sharedStock.Add(standardDescriptionFromItem, localStock[key][1]);
					if (sharedStock[standardDescriptionFromItem] != 2147483647)
					{
						key.Stack = sharedStock[standardDescriptionFromItem];
					}
				}
			}
			else
			{
				list.Clear();
				foreach (Item key2 in localStock.Keys)
				{
					string standardDescriptionFromItem2 = Utility.getStandardDescriptionFromItem(key2, 1);
					if (sharedStock.ContainsKey(standardDescriptionFromItem2) && sharedStock[standardDescriptionFromItem2] > 0)
					{
						localStock[key2][1] = sharedStock[standardDescriptionFromItem2];
						if (sharedStock[standardDescriptionFromItem2] != 2147483647)
						{
							key2.Stack = sharedStock[standardDescriptionFromItem2];
						}
					}
					else
					{
						list.Add(key2);
					}
				}
				foreach (Item item4 in list)
				{
					localStock.Remove(item4);
				}
			}
			list.Clear();
			if (conditionalItemFilters == null)
			{
				return;
			}
			foreach (Item key3 in localStock.Keys)
			{
				string standardDescriptionFromItem3 = Utility.getStandardDescriptionFromItem(key3, 1);
				if (conditionalItemFilters.ContainsKey(standardDescriptionFromItem3) && !conditionalItemFilters[standardDescriptionFromItem3]())
				{
					list.Add(key3);
				}
			}
			foreach (Item item5 in list)
			{
				localStock.Remove(item5);
			}
		}
	}
}
