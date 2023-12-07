using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley
{
	public class FriendshipReward : OrderReward
	{
		[XmlElement("targetName")]
		public NetString targetName = new NetString();

		[XmlElement("amount")]
		public NetInt amount = new NetInt();

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(targetName, amount);
		}

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string data2 = order.requester;
			if (data.ContainsKey("TargetName"))
			{
				data2 = data["TargetName"];
			}
			data2 = order.Parse(data2);
			targetName.Value = data2;
			string data3 = "250";
			if (data.ContainsKey("Amount"))
			{
				data3 = data["Amount"];
			}
			data3 = order.Parse(data3);
			amount.Value = int.Parse(data3);
		}

		public override void Grant()
		{
			NPC characterFromName = Game1.getCharacterFromName(targetName.Value);
			if (characterFromName != null)
			{
				Game1.player.changeFriendship(amount.Value, characterFromName);
			}
		}
	}
}
