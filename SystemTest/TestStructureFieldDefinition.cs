using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using System;

namespace SystemTest
{
    [TestClass]
    public class TestStructureFieldDefinition
    {
        CAEXDocument m_document = null;

        #region Tests
        private const uint PublisherQosDataType = 3006;

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.AggregateConfiguration, DisplayName = "Root AggregateConfiguration")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, DisplayName = "AutomationComponent PublisherQosDataType")]

        public void TestUnwantedAttributes(TestHelper.Uris uriId, uint nodeId)
        {
            AttributeFamilyType objectToTest = GetTestAttribute(uriId, nodeId);

            foreach( AttributeType attribute in objectToTest.Attribute )
            {
                if ( attribute.Name != "NodeId" && attribute.Name != "PracticallyEmpty" )
                {
                    AttributeType structureAttribute = GetAttribute(attribute, "StructureFieldDefinition");
                    foreach(AttributeType definitionAttribute in structureAttribute.Attribute)
                    {
                        // Make sure there is no NodeId for the StructureFieldDefinition
                        // It's always Opc.Ua.DataTypes.StructureField (101)
                        Assert.IsFalse(definitionAttribute.Name.Contains("NodeId"));
                        // Make sure the structure field does not have a NodeId either.
                        // It's always the known datatype of the field.
                        Assert.IsNull(definitionAttribute.Attribute["NodeId"]);
                        if ( definitionAttribute.Name.Equals("Description"))
                        {
                            // Make sure there is no structure field Definition.
                            // It's always a known datatype of localized text
                            Assert.IsNull(definitionAttribute.Attribute["StructureFieldDefinition"],
                                "Unexpected StructureFieldDefinition found in Description: " + structureAttribute.Name);
                        }
                    }
                }
            }
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "QosCategory","Name", null, "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "QosCategory", "Description", "Quality of Service Category", "0", "en")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "QosCategory", "Description", "Catégorie de qualité de service", "1", "fr")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "QosCategory", "Description", "Kategorie „Dienstqualität“", "2", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "QosCategory", "ValueRank", "-1", "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "QosCategory", "ArrayDimensions", null, "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "QosCategory", "MaxStringLength", "123", "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "QosCategory", "IsOptional", "true", "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "QosCategory", "AllowSubTypes", null, "", "")]

        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "DatagramQos", "Name", null, "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "DatagramQos", "Description", "Transmit Quality of Service", "0", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "DatagramQos", "ArrayDimensions", "2", "0", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "DatagramQos", "ArrayDimensions", "3", "1", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "DatagramQos", "ValueRank", "2", "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "DatagramQos", "IsOptional", null, "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "DatagramQos", "AllowSubTypes", "true", "", "")]

        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "NoDescription", "Name", null, "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "NoDescription", "Description", null, "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "NoDescription", "ValueRank", null, "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "NoDescription", "ArrayDimensions", null, "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "NoDescription", "MaxStringLength", "321", "", "")]
        [DataRow(TestHelper.Uris.Test, PublisherQosDataType, "NoDescription", "IsOptional", null, "", "")]

        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.OptionSet, "Value", "Name", null, "", "")]
        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.OptionSet, "Value", "Description", null, "", "")]
        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.OptionSet, "Value", "ValueRank", "-1", "", "")]
        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.OptionSet, "Value", "ArrayDimensions", null, "", "")]
        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.OptionSet, "Value", "MaxStringLength", null, "", "")]
        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.OptionSet, "Value", "IsOptional", null, "", "")]

        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.OptionSet, "ValidBits", "Name", null, "", "")]
        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.OptionSet, "ValidBits", "Description", null, "", "")]
        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.OptionSet, "ValidBits", "ValueRank", "-1", "", "")]
        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.OptionSet, "ValidBits", "ArrayDimensions", null, "", "")]
        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.OptionSet, "ValidBits", "MaxStringLength", null, "", "")]
        [DataRow(TestHelper.Uris.Root, Opc.Ua.DataTypes.OptionSet, "ValidBits", "IsOptional", null, "", "")]

        public void TestAttributeValues(TestHelper.Uris uriId,
            uint nodeId,
            string variableName, 
            string attributeName, 
            string expectedValue,
            string arrayIndex,
            string localeId)
        {
            AttributeFamilyType objectToTest = GetTestAttribute(uriId, nodeId);

            AttributeType variableAttribute = GetAttribute(objectToTest.Attribute, variableName);
            AttributeType structured = GetAttribute(variableAttribute, "StructureFieldDefinition");

            if (string.IsNullOrEmpty(expectedValue))
            {
                // attributeName should not exist
                Assert.IsNull(structured.Attribute[attributeName],
                    $"Attribute {attributeName} exists in {variableName} when it should not.");
            }
            else
            {
                AttributeType attribute = GetAttribute(structured.Attribute, attributeName);

                if (!string.IsNullOrEmpty(arrayIndex))
                {
                    attribute = GetAttribute(attribute, arrayIndex);
                }

                Assert.AreEqual(expectedValue, attribute.Value,
                    $"Unexpected value for {variableName}.{attributeName} in {structured.Name}.");

                if (!string.IsNullOrEmpty(localeId))
                {
                    AttributeType locale = GetAttribute(attribute.Attribute, localeId);
                    Assert.AreEqual(expectedValue, locale.Value,
                        $"Unexpected locale value for {variableName}.{attributeName} in {structured.Name}.");
                }
            }
        }


        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        public void TestUnwantedStructureAttribute()
        {
            AttributeFamilyType objectToTest = GetTestAttribute(TestHelper.Uris.Test, PublisherQosDataType);
            AttributeType emptyAttribute = GetAttribute(objectToTest.Attribute, "PracticallyEmpty");
            Assert.IsNull(emptyAttribute.Attribute["StructureFieldDefinition"],
                "Unexpected StructureFieldDefinition found in PracticallyEmpty");
        }


        #endregion

        #region Helpers

        private CAEXDocument GetDocument()
        {
            if( m_document == null )
            {
                m_document = TestHelper.GetReadOnlyDocument( "TestAml.xml.amlx" );
            }
            Assert.IsNotNull( m_document, "Unable to retrieve Document" );
            return m_document;
        }

        public AttributeFamilyType GetTestAttribute( TestHelper.Uris uriId, uint nodeId )
        {
            CAEXDocument document = GetDocument();
            string amlId = TestHelper.BuildAmlId("", uriId, nodeId.ToString() );
            Console.WriteLine( "Looking for " + amlId );
            CAEXObject initialObject = document.FindByID( amlId );
            Assert.IsNotNull( initialObject, "Unable to find Initial Object" );
            AttributeFamilyType theObject = initialObject as AttributeFamilyType;
            Assert.IsNotNull( theObject, "Unable to Cast Initial Object" );
            return theObject;
        }

        public AttributeType GetAttribute(AttributeType attributeType, string attributeName)
        {
            Assert.IsNotNull(attributeType, "AttributeType is null");
            return GetAttribute(attributeType.Attribute, attributeName);
        }

        public AttributeType GetAttribute( AttributeSequence attributes, string attributeName)
        {
            Assert.IsNotNull(attributes, "AttributeType is null");
            AttributeType result = attributes[attributeName];
            Assert.IsNotNull(result, "Unable to find Attribute " + attributeName);
            return result;
        }

        public AttributeType GetStructured(TestHelper.Uris uriId, uint nodeId, string variableName)
        {
            AttributeFamilyType objectToTest = GetTestAttribute(uriId, nodeId);
            AttributeType variableAttribute = GetAttribute(objectToTest.Attribute, variableName);
            AttributeType structured = GetAttribute(variableAttribute, "StructureFieldDefinition");
            return structured;
        }

        #endregion
    }
}