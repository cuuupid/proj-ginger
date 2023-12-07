using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Xml.Serialization.GeneratedAssembly
{
	public sealed class MineInfoSerializer : XmlSerializer1
	{
		public override bool CanDeserialize(XmlReader xmlReader)
		{
			return xmlReader.IsStartElement("MineInfo", "");
		}

		protected override void Serialize(object objectToSerialize, XmlSerializationWriter writer)
		{
			((XmlSerializationWriter1)writer).Write740_MineInfo(objectToSerialize);
		}

		protected override object Deserialize(XmlSerializationReader reader)
		{
			return ((XmlSerializationReader1)reader).Read742_MineInfo();
		}
	}
}
