using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace StardewValley
{
	public class Vector2Reader : XmlSerializationReader
	{
		public Vector2 ReadVector2()
		{
			XmlReader xmlReader = base.Reader;
			xmlReader.ReadStartElement("Vector2");
			xmlReader.ReadStartElement("X");
			float x = xmlReader.ReadContentAsFloat();
			xmlReader.ReadEndElement();
			xmlReader.ReadStartElement("Y");
			float y = xmlReader.ReadContentAsFloat();
			xmlReader.ReadEndElement();
			xmlReader.ReadEndElement();
			return new Vector2(x, y);
		}

		protected override void InitCallbacks()
		{
		}

		protected override void InitIDs()
		{
		}
	}
}
