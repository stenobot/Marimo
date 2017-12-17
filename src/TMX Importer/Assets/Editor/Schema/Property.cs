using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Assets.Schema
{
    [Serializable]
    public class Property
    {
        [XmlAttribute("name")]
        public string Name { get; private set; }

        [XmlAttribute("value")]
        public string Value { get; private set; }

        [XmlAttribute("type")]
        public PropertyType Type { get; private set; }
    }
}
