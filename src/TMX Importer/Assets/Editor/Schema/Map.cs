using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Assets.Schema
{
    /// <summary>
    /// Based on Tiled's map schema which can be found here:
    /// https://github.com/bjorn/tiled/blob/a4db8de98c19ab09a3323d5cca237c5eda993aec/util/java/libtiled-java/src/main/resources/map.xsd
    /// </summary>
    [Serializable]
    [XmlRoot("map")]
    public class Map
    {
        #region elements

        [XmlElement("properties")]
        public List<Property> Properties { get; private set; }

        [XmlElement("tileset")]
        public List<TileSet> Tilesets { get; private set; }

        [XmlElement("layer")]
        public List<Layer> Layers { get; private set; }

        // TODO: Add ObjectGroup support
        //[XmlElementAttribute("objectgroup")]
        //public List<ObjectGroup> ObjectGroups { get; private set; }

        [XmlElement("imagelayer")]
        public ImageLayer ImageLayers { get; private set; }

        #endregion

        #region attributes

        [XmlAttribute("version")]
        public string Version { get; private set; }

        [XmlAttribute("orientation")]
        public OrientationType Orientation { get; private set; }

        [XmlAttribute("renderorder")]
        public RenderOrderType RenderOrder { get; private set; }

        [XmlAttribute("width")]
        public UInt32 Width { get; private set; }

        [XmlAttribute("height")]
        public UInt32 Height { get; private set; }

        [XmlAttribute("tilewidth")]
        public UInt32 TileWidth { get; private set; }

        [XmlAttribute("tileheight")]
        public UInt32 TileHeight { get; private set; }

        [XmlAttribute("hexsidelength")]
        public Int32 HexSideLength { get; private set; }

        [XmlAttribute("staggeraxis")]
        public StaggerAxisType StaggerAxis { get; private set; }

        [XmlAttribute("staggerindex")]
        public StaggerIndexType StaggerIndex { get; private set; }

        [XmlAttribute("backgroundcolor")]
        public string BackgroundColor { get; private set; }

        [XmlAttribute("nextobjectid")]
        public UInt32 NextObjectId { get; private set; }


        #endregion
    }
}
