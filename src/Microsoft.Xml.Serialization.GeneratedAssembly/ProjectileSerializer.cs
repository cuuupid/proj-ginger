using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Xml.Serialization.GeneratedAssembly
{
	public sealed class ProjectileSerializer : XmlSerializer1
	{
		public override bool CanDeserialize(XmlReader xmlReader)
		{
			return xmlReader.IsStartElement("Projectile", "");
		}

		protected override void Serialize(object objectToSerialize, XmlSerializationWriter writer)
		{
			((XmlSerializationWriter1)writer).Write566_Projectile(objectToSerialize);
		}

		protected override object Deserialize(XmlSerializationReader reader)
		{
			return ((XmlSerializationReader1)reader).Read568_Projectile();
		}
	}
}
