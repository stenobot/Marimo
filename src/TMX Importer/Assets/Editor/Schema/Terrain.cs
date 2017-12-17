using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Assets.Schema
{
    [Serializable]
    public class Terrain
    {
        [XmlAttribute("name")]
        public string Name { get; private set; }

        [XmlAttribute("tile")]
        public UInt32 Tile { get; private set; }
    }
}
