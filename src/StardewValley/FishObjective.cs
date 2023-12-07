using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley
{
	public class FishObjective : OrderObjective
	{
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(acceptableContextTagSets);
		}

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			if (data.ContainsKey("AcceptedContextTags"))
			{
				acceptableContextTagSets.Add(order.Parse(data["AcceptedContextTags"]));
			}
		}

		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = _order;
			order.onFishCaught = (Action<Farmer, Item>)Delegate.Combine(order.onFishCaught, new Action<Farmer, Item>(OnFishCaught));
		}

		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = _order;
			order.onFishCaught = (Action<Farmer, Item>)Delegate.Remove(order.onFishCaught, new Action<Farmer, Item>(OnFishCaught));
		}

		public virtual void OnFishCaught(Farmer farmer, Item fish_item)
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
						if (fish_item.HasContextTag(text2.Trim()))
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
					IncrementCount(fish_item.Stack);
					break;
				}
			}
		}
	}
}
