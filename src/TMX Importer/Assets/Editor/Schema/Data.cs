using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Schema
{
    [Serializable]
    public class Data : IXmlSerializable
    {
        #region Constants

        const string PROPERTY_COMPRESSION = "compression";
        const string PROPERTY_ENCODING = "encoding";

        #endregion

        #region Public variables

        /// <summary>
        /// MapData contains a 2-dimensional int array containing the tile indices for the layer
        /// </summary>
        public int[,] MapData;

        #endregion

        #region Private variables

        /// <summary>
        /// Encoding is private as it will be handled by the <see cref="IXmlSerializable"/> implementation
        /// </summary>
        [XmlAttribute("encoding")]
        private EncodingType Encoding;

        /// <summary>
        /// Compression is private as it will be handled by the <see cref="IXmlSerializable"/> implementation
        /// </summary>
        [XmlAttribute("compression")]
        private CompressionType Compression;

        /// <summary>
        /// Data is private as it will be parsed into <see cref="MapData"/> by the <see cref="IXmlSerializable"/> implementation
        /// </summary>
        [XmlText]
        private string m_data;

        #endregion

        #region IXmlSerializable implementation 

        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Overrides the XmlReader to parse the "data" field into <see cref="MapData"/>
        /// </summary>
        /// <param name="reader">The XmlReader used by the <see cref="XmlSerializer"/> for deserialization</param>
        public void ReadXml(XmlReader reader)
        {
            try
            {
                // Get attributes
                string compression = reader.GetAttribute("compression");
                string encoding = reader.GetAttribute("encoding");

                // Set attributes
                Compression = string.Equals(compression, CompressionType.GZip.ToString(), StringComparison.OrdinalIgnoreCase) ?
                    CompressionType.GZip :
                    (string.Equals(compression, CompressionType.ZLib.ToString(), StringComparison.OrdinalIgnoreCase) ?
                    CompressionType.ZLib :
                    CompressionType.None);
                Encoding = (EncodingType)Enum.Parse(typeof(EncodingType), encoding, true);

                // Get element string
                m_data = reader.ReadString();
                string[] rows = m_data.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                // Make sure there's rows before using the first one for measurement
                if (rows.Length > 0)
                {
                    // Get the first row so we know how large to initialize the map array
                    string[] firstRow = rows[0].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    MapData = new int[firstRow.Length, rows.Length];

                    // Iterate through each row, splitting the CSV data and moving it into the TileMap array
                    for (int row = 0; row < rows.Length; row++)
                    {
                        string[] rowDataStr = rows[row].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int col = 0; col < rowDataStr.Length; col++)
                        {
                            int parsedInt = -1;
                            if (int.TryParse(rowDataStr[0], out parsedInt))
                                MapData[col, row] = parsedInt;
                        }
                    }
                }

                reader.ReadEndElement();
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Overrides the XmlWriter to write the <see cref="MapData"/> as a string representation
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            //TODO: Split MapData out to its correct string representation for the writer
            writer.WriteValue(MapData);

            // Always write encoding
            writer.WriteAttributeString(PROPERTY_ENCODING, Encoding.ToString());

            // Only write compression if it is set
            if (Compression != CompressionType.None)
                writer.WriteAttributeString(PROPERTY_COMPRESSION, Compression.ToString());

            // Close the element
            writer.WriteEndElement();
        }

        #endregion
    }
}
