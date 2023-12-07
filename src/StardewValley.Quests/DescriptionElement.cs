using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Monsters;

namespace StardewValley.Quests
{
	public class DescriptionElement : INetObject<NetFields>
	{
		public static XmlSerializer serializer = SaveGame.GetSerializer(typeof(DescriptionElement));

		public string xmlKey;

		public List<object> param;

		[XmlIgnore]
		public NetFields NetFields { get; } = new NetFields();


		public static implicit operator DescriptionElement(string key)
		{
			return new DescriptionElement(key);
		}

		public DescriptionElement()
		{
			xmlKey = string.Empty;
			param = new List<object>();
		}

		public DescriptionElement(string key)
		{
			xmlKey = key;
			param = new List<object>();
		}

		public DescriptionElement(string key, object param1)
		{
			xmlKey = key;
			param = new List<object>();
			param.Add(param1);
		}

		public DescriptionElement(string key, List<object> paramlist)
		{
			xmlKey = key;
			param = new List<object>();
			foreach (object item in paramlist)
			{
				param.Add(item);
			}
		}

		public DescriptionElement(string key, object param1, object param2)
		{
			xmlKey = key;
			param = new List<object>();
			param.Add(param1);
			param.Add(param2);
		}

		public DescriptionElement(string key, object param1, object param2, object param3)
		{
			xmlKey = key;
			param = new List<object>();
			param.Add(param1);
			param.Add(param2);
			param.Add(param3);
		}

		public string loadDescriptionElement()
		{
			DescriptionElement descriptionElement = new DescriptionElement(xmlKey, param);
			string text = "";
			for (int i = 0; i < descriptionElement.param.Count; i++)
			{
				if (descriptionElement.param[i] is DescriptionElement)
				{
					DescriptionElement descriptionElement2 = descriptionElement.param[i] as DescriptionElement;
					descriptionElement.param[i] = descriptionElement2.loadDescriptionElement();
				}
				if (descriptionElement.param[i] is Object)
				{
					Game1.objectInformation.TryGetValue((descriptionElement.param[i] as Object).parentSheetIndex, out var value);
					descriptionElement.param[i] = value.Split('/')[4];
				}
				if (descriptionElement.param[i] is Monster)
				{
					DescriptionElement descriptionElement3;
					if ((descriptionElement.param[i] as Monster).name.Equals("Frost Jelly"))
					{
						descriptionElement3 = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13772");
						descriptionElement.param[i] = descriptionElement3.loadDescriptionElement();
					}
					else
					{
						descriptionElement3 = new DescriptionElement("Data\\Monsters:" + (descriptionElement.param[i] as Monster).name);
						descriptionElement.param[i] = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? (descriptionElement3.loadDescriptionElement().Split('/').Last() + "s") : descriptionElement3.loadDescriptionElement().Split('/').Last());
					}
					descriptionElement.param[i] = descriptionElement3.loadDescriptionElement().Split('/').Last();
				}
				if (descriptionElement.param[i] is NPC)
				{
					DescriptionElement descriptionElement4 = new DescriptionElement("Data\\NPCDispositions:" + (descriptionElement.param[i] as NPC).name);
					descriptionElement.param[i] = descriptionElement4.loadDescriptionElement().Split('/').Last();
				}
			}
			if (descriptionElement.xmlKey == "")
			{
				return string.Empty;
			}
			switch (descriptionElement.param.Count)
			{
			default:
				text = Game1.content.LoadString(descriptionElement.xmlKey);
				if (xmlKey.Contains("Dialogue.cs.7") || xmlKey.Contains("Dialogue.cs.8"))
				{
					text = Game1.content.LoadString(descriptionElement.xmlKey).Replace("/", " ");
					text = ((text[0] == ' ') ? text.Substring(1) : text);
				}
				break;
			case 1:
				text = Game1.content.LoadString(descriptionElement.xmlKey, descriptionElement.param[0]);
				break;
			case 2:
				text = Game1.content.LoadString(descriptionElement.xmlKey, descriptionElement.param[0], descriptionElement.param[1]);
				break;
			case 3:
				text = Game1.content.LoadString(descriptionElement.xmlKey, descriptionElement.param[0], descriptionElement.param[1], descriptionElement.param[2]);
				break;
			case 4:
				text = Game1.content.LoadString(descriptionElement.xmlKey, descriptionElement.param[0], descriptionElement.param[1], descriptionElement.param[2], descriptionElement.param[3]);
				break;
			}
			return text;
		}
	}
}
