using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using System.Linq;
using System;
using Opc.Ua;

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
                if ( attribute.Name != "NodeId")
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
        [DataRow("QosCategory","Name", "QosCategory")]
        [DataRow("QosCategory", "Description", "Quality of Service Category")]
        [DataRow("QosCategory", "ValueRank", "-1")]
        [DataRow("QosCategory", "MaxStringLength", "123")]
        [DataRow("QosCategory", "IsOptional", "true")]

        [DataRow("DatagramQos", "Name", "DatagramQos")]
        [DataRow("DatagramQos", "Description", "Transmit Quality of Service")]
        [DataRow("DatagramQos", "ValueRank", "2")]
        [DataRow("DatagramQos", "IsOptional", "false")]

        [DataRow("NoDescription", "Name", "NoDescription")]
        [DataRow("NoDescription", "Description", null)]
        [DataRow("NoDescription", "ValueRank", "-1")]
        [DataRow("NoDescription", "MaxStringLength", "321")]
        [DataRow("NoDescription", "IsOptional", "false")]

        public void TestAttributeValues(string variableName, 
            string attributeName, 
            string expectedValue)
        {
            AttributeValues(variableName, attributeName, expectedValue);
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        public void TestDescriptionLocale()
        {
            AttributeValues("QosCategory", "Description", "Quality of Service Category", "en");
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        public void TestArrayDimensions()
        {
            AttributeType structured = GetStructured(TestHelper.Uris.Test, 
                PublisherQosDataType, "DatagramQos");
            AttributeType attribute = GetAttribute(structured, "ArrayDimensions");
            AttributeType first = GetAttribute(attribute, "0");
            Assert.AreEqual("2", first.Value, "Unexpected value for ArrayDimensions[0].");
            AttributeType second = GetAttribute(attribute, "1");
            Assert.AreEqual("3", second.Value, "Unexpected value for ArrayDimensions[1].");
        }

        public void AttributeValues(string variableName,
            string attributeName,
            string expectedValue,
            string localeId = "")
        {
            AttributeFamilyType objectToTest = GetTestAttribute(TestHelper.Uris.Test,
                PublisherQosDataType);

            AttributeType variableAttribute = GetAttribute(objectToTest.Attribute, variableName);
            AttributeType structured = GetAttribute(variableAttribute, "StructureFieldDefinition");
            AttributeType attribute = GetAttribute(structured.Attribute, attributeName);
            Assert.AreEqual(expectedValue, attribute.Value,
                $"Unexpected value for {variableName}.{attributeName} in {structured.Name}.");  

            if (!string.IsNullOrEmpty(localeId))
            {
                AttributeType locale = GetAttribute(attribute.Attribute, localeId);
                Assert.AreEqual(expectedValue, locale.Value,
                    $"Unexpected locale value for {variableName}.{attributeName} in {structured.Name}.");
            }
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