using System;
using System.Xml.Serialization;

namespace Assets.Schema
{
    [Serializable]
    public class ImageLayer
    {
        [XmlAttribute("name")]
        public string Name { get; private set; }

        [XmlAttribute("offsetx")]
        public double OffsetX { get; private set; }

        [XmlAttribute("offsety")]
        public double OffsetY { get; private set; }

        [XmlAttribute("opacity")]
        public float Opacity { get; private set; }

        [XmlAttribute("visible")]
        public bool Visible { get; private set; }
    }
}
