using System;
using System.Xml.Serialization;

namespace Assets.Schema
{
    [Serializable]
    public class TileOffset
    {
        [XmlAttribute("x")]
        public int X { get; private set; }

        [XmlAttribute("y")]
        public int Y { get; private set; }
    }
}
