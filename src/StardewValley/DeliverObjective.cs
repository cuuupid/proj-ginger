using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley
{
	public class DeliverObjective : OrderObjective
	{
		[XmlElement("acceptableContextTagSets")]
		public NetStringList acceptableContextTagSets = new NetStringList();

		[XmlElement("targetName")]
		public NetString targetName = new NetString();

		[XmlElement("message")]
		public NetString message = new NetString();

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			if (data.ContainsKey("AcceptedContextTags"))
			{
				acceptableContextTagSets.Add(order.Parse(data["AcceptedContextTags"]));
			}
			if (data.ContainsKey("TargetName"))
			{
				targetName.Value = order.Parse(data["TargetName"]);
			}
			else
			{
				targetName.Value = _order.requester.Value;
			}
			if (data.ContainsKey("Message"))
			{
				message.Value = order.Parse(data["Message"]);
			}
			else
			{
				message.Value = "";
			}
		}

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(acceptableContextTagSets, targetName, message);
		}

		public override bool ShouldShowProgress()
		{
			return false;
		}

		protected override void _Register()
		{
			base._Register();
			SpecialOrder order = _order;
			order.onItemDelivered = (Func<Farmer, NPC, Item, int>)Delegate.Combine(order.onItemDelivered, new Func<Farmer, NPC, Item, int>(OnItemDelivered));
		}

		protected override void _Unregister()
		{
			base._Unregister();
			SpecialOrder order = _order;
			order.onItemDelivered = (Func<Farmer, NPC, Item, int>)Delegate.Remove(order.onItemDelivered, new Func<Farmer, NPC, Item, int>(OnItemDelivered));
		}

		public virtual int OnItemDelivered(Farmer farmer, NPC npc, Item item)
		{
			if (IsComplete())
			{
				return 0;
			}
			if (npc.Name != targetName.Value)
			{
				return 0;
			}
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
				return 0;
			}
			int num = GetMaxCount() - GetCount();
			int num2 = Math.Min(item.Stack, num);
			if (num2 < num)
			{
				return 0;
			}
			Item one = item.getOne();
			one.Stack = num2;
			_order.donatedItems.Add(one);
			item.Stack -= num2;
			IncrementCount(num2);
			if (!string.IsNullOrEmpty(message.Value))
			{
				npc.CurrentDialogue.Push(new Dialogue(message.Value, npc));
				Game1.drawDialogue(npc);
			}
			return num2;
		}
	}
}
