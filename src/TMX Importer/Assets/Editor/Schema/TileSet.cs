using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assets.Schema
{
    [Serializable]
    public class TileSet
    {
        #region Elements

        [XmlElement("tile")]
        public List<Tile> Tiles { get; private set; }
        
        #endregion

        #region Attributes

        [XmlAttribute("name")]
        public string Name { get; private set; }

        [XmlAttribute("firstgid")]
        public UInt32 FirstGid { get; private set; }

        [XmlAttribute("source")]
        public string Source { get; private set; }

        [XmlAttribute("tilewidth")]
        public UInt32 TileWidth { get; private set; }

        [XmlAttribute("tileheight")]
        public UInt32 TileHeight { get; private set; }

        [XmlAttribute("spacing")]
        public UInt32 Spacing { get; private set; }

        [XmlAttribute("margin")]
        public UInt32 Margin { get; private set; }

        [XmlAttribute("tilecount")]
        public int TileCount { get; private set; }

        [XmlAttribute("columns")]
        public int Columns { get; private set; }
        
        #endregion
    }
}
