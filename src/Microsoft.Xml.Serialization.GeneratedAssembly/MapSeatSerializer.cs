using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Xml.Serialization.GeneratedAssembly
{
	public sealed class MapSeatSerializer : XmlSerializer1
	{
		public override bool CanDeserialize(XmlReader xmlReader)
		{
			return xmlReader.IsStartElement("MapSeat", "");
		}

		protected override void Serialize(object objectToSerialize, XmlSerializationWriter writer)
		{
			((XmlSerializationWriter1)writer).Write427_MapSeat(objectToSerialize);
		}

		protected override object Deserialize(XmlSerializationReader reader)
		{
			return ((XmlSerializationReader1)reader).Read429_MapSeat();
		}
	}
}
