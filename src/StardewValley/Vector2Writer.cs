using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace StardewValley
{
	public class Vector2Writer : XmlSerializationWriter
	{
		public void WriteVector2(Vector2 vec)
		{
			XmlWriter xmlWriter = base.Writer;
			xmlWriter.WriteStartElement("Vector2");
			xmlWriter.WriteStartElement("X");
			xmlWriter.WriteValue(vec.X);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("Y");
			xmlWriter.WriteValue(vec.Y);
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
		}

		protected override void InitCallbacks()
		{
		}
	}
}
