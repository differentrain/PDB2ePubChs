using System;
using System.Xml.Serialization;

namespace PDB2ePubChs.HaoduPdbFiles
{
    [Serializable]
    public sealed class ReplacedChar
    {
        [XmlAttribute(DataType = "string")]
        public string Org;
        [XmlAttribute(DataType = "string")]
        public string Rep;
    }
}
