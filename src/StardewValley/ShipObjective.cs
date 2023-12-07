using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley
{
	public class ShipObjective : OrderObjective
	{
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		[XmlElement("useShipmentValue")]
		public NetBool useShipmentValue = new NetBool();

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			if (data.ContainsKey("AcceptedContextTags"))
			{
				acceptableContextTagSets.Add(order.Parse(data["AcceptedContextTags"]));
			}
			if (data.ContainsKey("UseShipmentValue") && data["UseShipmentValue"].ToLowerInvariant().Trim() == "true")
			{
				useShipmentValue.Value = true;
			}
		}

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(acceptableContextTagSets, useShipmentValue);
		}

		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = _order;
			order.onItemShipped = (Action<Farmer, Item, int>)Delegate.Combine(order.onItemShipped, new Action<Farmer, Item, int>(OnItemShipped));
		}

		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = _order;
			order.onItemShipped = (Action<Farmer, Item, int>)Delegate.Remove(order.onItemShipped, new Action<Farmer, Item, int>(OnItemShipped));
		}

		public virtual void OnItemShipped(Farmer farmer, Item item, int shipped_price)
		{
			foreach (string acceptableContextTagSet in acceptableContextTagSets)
			{
				bool flag = false;
				string[] array = acceptableContextTagSet.Split(',');
				foreach (string text in array)
				{
					bool flag2 = false;
					string[] array2 = text.Split('/');
					foreach (string text2 in array2)
					{
						if (item.HasContextTag(text2.Trim()))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					if (useShipmentValue.Value)
					{
						IncrementCount(shipped_price);
					}
					else
					{
						IncrementCount(item.Stack);
					}
					break;
				}
			}
		}
	}
}
