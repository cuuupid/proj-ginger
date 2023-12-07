using System;
using System.Collections.Generic;
using Netcode;

namespace StardewValley
{
	public class ResetEventReward : OrderReward
	{
		public NetIntList resetEvents = new NetIntList();

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(resetEvents);
		}

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string text = order.Parse(data["ResetEvents"]);
			string[] array = text.Split(' ');
			string[] array2 = array;
			foreach (string value in array2)
			{
				resetEvents.Add(Convert.ToInt32(value));
			}
		}

		public override void Grant()
		{
			foreach (int resetEvent in resetEvents)
			{
				Game1.player.eventsSeen.Remove(resetEvent);
			}
		}
	}
}
