using System;
using System.Xml.Serialization;

namespace Assets.Schema
{
    [Serializable]
    public enum OrientationType
    {
        [XmlEnum("orthogonal")]
        Orthogonal,
        [XmlEnum("isometric")]
        Isometric,
        [XmlEnum("staggered")]
        IsometricStaggered,
        [XmlEnum("hexagonal")]
        Hexagonal
    }

    [Serializable]
    public enum RenderOrderType
    {
        [XmlEnum("right-down")]
        RightDown,
        [XmlEnum("right-up")]
        RightUp,
        [XmlEnum("left-down")]
        LeftDown,
        [XmlEnum("left-up")]
        LeftUp
    }

    [Serializable]
    public enum EncodingType
    {
        [XmlEnum("base64")]
        Base64,
        [XmlEnum("csv")]
        CSV,
    }

    [Serializable]
    public enum CompressionType
    {
        None,
        [XmlAttribute("gzip")]
        GZip,
        [XmlAttribute("zlib")]
        ZLib
    }

    [Serializable]
    public enum PropertyType
    {
        [XmlEnum("string")]
        String,
        [XmlEnum("int")]
        Int,
        [XmlEnum("float")]
        Float,
        [XmlEnum("bool")]
        Bool,
        [XmlEnum("color")]
        Color,
        [XmlEnum("file")]
        File
    }

    [Serializable]
    public enum FormatType
    {
        [XmlEnum("png")]
        PNG
    }

    [Serializable]
    public enum StaggerAxisType
    {
        [XmlEnum("x")]
        X,
        [XmlEnum("y")]
        Y
    }

    [Serializable]
    public enum StaggerIndexType
    {
        [XmlEnum("even")]
        Even,
        [XmlEnum("odd")]
        Odd
    }
}