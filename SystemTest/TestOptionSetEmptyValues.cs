using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Aml.Engine.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemTest
{
    [TestClass]
    public class TestOptionSetEmptyValues
    {
        #region Tests

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestOperationalHealthOptionSet()
        {
            AttributeTypeLibType attributeLibrary = GetFxAcAttributes();
            AttributeFamilyType attributeFamilyType = attributeLibrary[ "OperationalHealthOptionSet" ];
            TestOptionSet( attributeFamilyType );
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestAggregatedHealthOptionSet()
        {
            AttributeTypeLibType attributeLibrary = GetFxAcAttributes();
            AttributeFamilyType attributeFamilyType = attributeLibrary[ "AggregatedHealthDataType" ];
            Assert.IsNotNull( attributeFamilyType );
            AttributeType attributeType = attributeFamilyType.Attribute[ "AggregatedOperationalHealth" ];
            TestOptionSet( attributeType );
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestInstance()
        {
            CAEXDocument document = GetDocument();
            CAEXObject instance = document.FindByID( "nsu%3Dhttp%3A%2F%2Fopcfoundation.org%2FUA%2FFX%2FAML%2FTESTING%2FAmlFxTest%2F%3Bi%3D6003" );
            Assert.IsNotNull( instance );
            InternalElementType internalElementType = instance as InternalElementType; 
            Assert.IsNotNull( internalElementType );
            AttributeType value = internalElementType.Attribute[ "Value" ];
            Assert.IsNotNull( value );
            TestOptionSet( value );
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow("Active", "0")]
        [DataRow("Unacknowledged", "1")]
        [DataRow("Unconfirmed", "2")]
        public void TestFieldDefinitions(string attributeName, string attributeValue)
        {
            CAEXDocument document = GetDocument("TestAml.xml.amlx");
            string amlId = TestHelper.BuildAmlId("", TestHelper.Uris.Test, "3009");
            CAEXObject initialObject = document.FindByID(amlId);
            Assert.IsNotNull(initialObject, "Unable to find Initial Object");
            AttributeFamilyType theObject = initialObject as AttributeFamilyType;
            Assert.IsNotNull(theObject, "Unable to Cast Initial Object");


            AttributeType fieldDefinition = GetAttribute( theObject.Attribute, "OptionSetFieldDefinition");
            Assert.IsNotNull(fieldDefinition.AdditionalInformation);
            Assert.AreEqual(1, fieldDefinition.AdditionalInformation.Count);
            Assert.AreEqual("OpcUa:TypeOnly", fieldDefinition.AdditionalInformation[0]);

            AttributeType attribute = GetAttribute(fieldDefinition, attributeName);

            Assert.IsNull(attribute.Attribute["NodeId"]);
            Assert.IsNull(attribute.Attribute["IsAbstract"]);
            AttributeType valueAttribute = GetAttribute(attribute, "Value");
            Assert.AreEqual( valueAttribute.Value, attributeValue);

            Assert.IsNull(valueAttribute.Attribute[ "NodeId" ] );
            Assert.IsNull(valueAttribute.Attribute[ "ValidBits" ]);
        }



        #endregion

        #region Helpers

        private CAEXDocument GetDocument(string fileName = "AmlFxTest.xml.amlx")
        {
            CAEXDocument document = TestHelper.GetReadOnlyDocument( fileName );
            Assert.IsNotNull(document, "Unable to retrieve Document" );
            return document;
        }

        public void TestOptionSet( AttributeTypeType attributeFamilyType )
        {
            Assert.IsNotNull( attributeFamilyType );
            Assert.IsTrue( attributeFamilyType.Attribute.Count >= 4 );
            ValidateOptionSetNames( attributeFamilyType );
        }

        public void ValidateOptionSetNames( AttributeTypeType attributeFamilyType )
        {
            HashSet<string> names = GetOptionSetNames( attributeFamilyType );
            Assert.AreEqual( true, names.Contains( "OperationalWarning" ) );
            Assert.AreEqual( true, names.Contains( "OperationalError" ) );
            Assert.AreEqual( true, names.Contains( "SubOperationalWarning" ) );
            Assert.AreEqual( true, names.Contains( "SubOperationalError" ) );
        }

        public HashSet<string> GetOptionSetNames( AttributeTypeType attributeFamilyType )
        {
            HashSet<string> strings = new HashSet<string>();

            foreach( AttributeType attribute in attributeFamilyType.Attribute )
            {
                strings.Add( attribute.Name );
            }

            return strings;
        }

        public AttributeTypeLibType GetFxAcAttributes()
        {
            CAEXDocument document = GetDocument();
            AttributeTypeLibType fxAcAttributes = document.CAEXFile.AttributeTypeLib[
                "ATL_http://opcfoundation.org/UA/FX/AC/" ];
            Assert.IsNotNull( fxAcAttributes );
            return fxAcAttributes;
        }

        public AttributeType GetAttribute(AttributeType attributeType, string attributeName)
        {
            Assert.IsNotNull(attributeType, "AttributeType is null");
            return GetAttribute(attributeType.Attribute, attributeName);
        }

        public AttributeType GetAttribute(AttributeSequence attributes, string attributeName)
        {
            Assert.IsNotNull(attributes, "AttributeType is null");
            AttributeType result = attributes[attributeName];
            Assert.IsNotNull(result, "Unable to find Attribute " + attributeName);
            return result;
        }


        #endregion

    }
}
