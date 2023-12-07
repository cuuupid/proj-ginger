using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Xml.Serialization.GeneratedAssembly
{
	public sealed class DonateObjectiveSerializer : XmlSerializer1
	{
		public override bool CanDeserialize(XmlReader xmlReader)
		{
			return xmlReader.IsStartElement("DonateObjective", "");
		}

		protected override void Serialize(object objectToSerialize, XmlSerializationWriter writer)
		{
			((XmlSerializationWriter1)writer).Write433_DonateObjective(objectToSerialize);
		}

		protected override object Deserialize(XmlSerializationReader reader)
		{
			return ((XmlSerializationReader1)reader).Read435_DonateObjective();
		}
	}
}