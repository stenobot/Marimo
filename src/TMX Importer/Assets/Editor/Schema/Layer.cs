using System;
using System.Xml;
using System.Xml.Serialization;

namespace Assets.Schema
{
    [Serializable]
    public class Layer
    {
        #region Public variables
        [XmlAttribute("name")]
        public string Name { get; private set; }

        [XmlAttribute("width")]
        public int Width { get; private set; }

        [XmlAttribute("height")]
        public int Height { get; private set; }

        [XmlElement("data")]
        public Data Data;

        #endregion
    }
}
