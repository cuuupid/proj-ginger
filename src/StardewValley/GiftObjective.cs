using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley
{
	public class GiftObjective : OrderObjective
	{
		public enum LikeLevels
		{
			None,
			Hated,
			Disliked,
			Neutral,
			Liked,
			Loved
		}

		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		[XmlElement("minimumLikeLevel")]
		public NetEnum<LikeLevels> minimumLikeLevel = new NetEnum<LikeLevels>(LikeLevels.None);

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			if (data.ContainsKey("AcceptedContextTags"))
			{
				acceptableContextTagSets.Add(order.Parse(data["AcceptedContextTags"]));
			}
			if (data.ContainsKey("MinimumLikeLevel"))
			{
				minimumLikeLevel.Value = (LikeLevels)Enum.Parse(typeof(LikeLevels), data["MinimumLikeLevel"]);
			}
		}

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(acceptableContextTagSets, minimumLikeLevel);
		}

		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = _order;
			order.onGiftGiven = (Action<Farmer, NPC, Item>)Delegate.Combine(order.onGiftGiven, new Action<Farmer, NPC, Item>(OnGiftGiven));
		}

		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = _order;
			order.onGiftGiven = (Action<Farmer, NPC, Item>)Delegate.Remove(order.onGiftGiven, new Action<Farmer, NPC, Item>(OnGiftGiven));
		}

		public virtual void OnGiftGiven(Farmer farmer, NPC npc, Item item)
		{
			bool flag = true;
			foreach (string acceptableContextTagSet in acceptableContextTagSets)
			{
				flag = false;
				bool flag2 = false;
				string[] array = acceptableContextTagSet.Split(',');
				foreach (string text in array)
				{
					bool flag3 = false;
					string[] array2 = text.Split('/');
					foreach (string text2 in array2)
					{
						if (item.HasContextTag(text2.Trim()))
						{
							flag3 = true;
							break;
						}
					}
					if (!flag3)
					{
						flag2 = true;
					}
				}
				if (!flag2)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
			if (minimumLikeLevel.Value > LikeLevels.None)
			{
				int giftTasteForThisItem = npc.getGiftTasteForThisItem(item);
				LikeLevels likeLevels = LikeLevels.None;
				switch (giftTasteForThisItem)
				{
				case 6:
					likeLevels = LikeLevels.Hated;
					break;
				case 4:
					likeLevels = LikeLevels.Disliked;
					break;
				case 8:
					likeLevels = LikeLevels.Neutral;
					break;
				case 2:
					likeLevels = LikeLevels.Liked;
					break;
				case 0:
					likeLevels = LikeLevels.Loved;
					break;
				}
				if (likeLevels < minimumLikeLevel.Value)
				{
					return;
				}
			}
			IncrementCount(1);
		}
	}
}
