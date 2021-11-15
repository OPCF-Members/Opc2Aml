using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Opc.Ua;

namespace MarkdownProcessor.NodeSet
{
    partial class UANode
    {
        [XmlIgnore]
        public NodeId DecodedNodeId { get; set; }

        [XmlIgnore]
        public QualifiedName DecodedBrowseName { get; set; }

        [XmlIgnore]
        public NodeClass NodeClass { get; set; }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(BrowseName))
            {
                return NodeId;
            }

            return BrowseName;
        }
    }

    partial class UAVariable
    {
        [XmlIgnore]
        public NodeId DecodedDataType { get; set; }

        [XmlIgnore]
        public Variant DecodedValue { get; set; }
    }

    partial class UAVariableType
    {
        [XmlIgnore]
        public NodeId DecodedDataType { get; set; }

        [XmlIgnore]
        public Variant DecodedValue { get; set; }
    }

    partial class DataTypeField
    {
        [XmlIgnore]
        public NodeId DecodedDataType { get; set; }
    }
}