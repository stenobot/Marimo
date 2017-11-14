using System;
using System.Xml.Serialization;

namespace Assets.Schema
{
    [Serializable]
    public class Image
    {
        [XmlAttribute("width")]
        public int Width { get; private set; }

        [XmlAttribute("height")]
        public int Height { get; private set; }

        [XmlAttribute("source")]
        public string Source { get; private set; }
    }
}
