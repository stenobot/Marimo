using System;
using System.Xml.Serialization;

namespace Assets.Schema
{
    [Serializable]
    public class Tile
    {
        [XmlAttribute("id")]
        public int Id { get; private set; }

        [XmlElement("image")]
        public Image Image { get; private set; }
    }
}
