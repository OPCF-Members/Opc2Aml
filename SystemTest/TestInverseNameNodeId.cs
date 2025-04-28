using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Opc.Ua;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;


namespace SystemTest
{
    [TestClass]
    public class TestInverseNameNodeId
    {
        // Test is to address issue 94 - 
        // The Namespaces of BrowseNames differ between the generated AML file and the original Nodeset file.
        // Test Both NodeId NamespaceUris and BrowseNameUris

        private const string FxIdPrefix = "nsu%3Dhttp%3A%2F%2Fopcfoundation.org%2FUA%2FFX%2FAC%2F%3Bi%3D";
        private const string AmlFxTestPrefix = "nsu%3Dhttp%3A%2F%2Fopcfoundation.org%2FUA%2FFX%2FAML%2FTESTING%2FAmlFxTest%2F%3Bi%3D";
        public const string TestAmlUri = "http://opcfoundation.org/UA/FX/AML/TESTING/AmlFxTest/";
        private const string FxAcUri = "http://opcfoundation.org/UA/FX/AC/";
        private const string DiUri = "http://opcfoundation.org/UA/DI/";

        CAEXDocument m_document = null;

        #region Tests

        [TestMethod]
        public void TestAll()
        {
            CAEXDocument document = GetDocument();

            int counter = 0;
            foreach( InterfaceClassLibType classLibType in document.InterfaceClassLib )
            {
                foreach( InterfaceFamilyType interfaceType in classLibType )
                {
                    AttributeType inverse = interfaceType.Attribute["InverseName"];
                    if ( inverse != null )
                    {
                        Assert.IsNull(inverse.Attribute["NodeId"]);
                        counter++;
                    }
                }
            }
            Console.WriteLine("Tested " + counter.ToString() + " interface types"); 
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow("41", TestHelper.Uris.Root, DisplayName = "GeneratesEvent")]
        //        [DataRow("6467", TestHelper.Uris.Di, DisplayName = "ConnectsToParent = Does not have inverse name!")]
        [DataRow("6031", TestHelper.Uris.Di, DisplayName = "IsOnline")]
        [DataRow("35", TestHelper.Uris.Ac, DisplayName = "HasPart")]

        public void TestInterfaceEntity(string nodeId, TestHelper.Uris uriEnum)
        {
            InterfaceFamilyType testObject = GetTestObject( nodeId, uriEnum );

            // Node Id Attribute should be correct.
            AttributeType nodeIdAttribute = GetAttribute(testObject.Attribute, "NodeId");
            AttributeType root = GetAttribute(nodeIdAttribute.Attribute, "RootNodeId");

            Assert.AreEqual(
                TestHelper.GetUri(uriEnum), 
                GetAttributeValue(root.Attribute, "NamespaceUri"));

            Assert.AreEqual( nodeId, GetAttributeValue(root.Attribute, "NumericId"));

            AttributeType inverse = GetAttribute(testObject.Attribute, "InverseName");

            // Inverse Name should not have nodeId
            Assert.IsNull(inverse.Attribute["NodeId"], "Inverse Name should not have a nodeId");
        }

        #endregion

        #region Helpers

        public string GetAttributeValue(AttributeSequence sequence, string attributeName)
        {
            AttributeType attribute = GetAttribute( sequence, attributeName );
            Assert.IsNotNull( attribute.Value );
            return attribute.Value;
        }

        public AttributeType GetAttribute( AttributeSequence sequence, string attributeName )
        {
            AttributeType attribute = sequence[attributeName];
            Assert.IsNotNull(attribute);
            return attribute;
        }

        private CAEXDocument GetDocument()
        {
            if (m_document == null)
            {
                m_document = TestHelper.GetReadOnlyDocument("AmlFxTest.xml.amlx");
            }
            Assert.IsNotNull(m_document, "Unable to retrieve Document");
            return m_document;
        }

        public InterfaceFamilyType GetTestObject(string nodeId, TestHelper.Uris uriEnum)
        {
            CAEXDocument document = GetDocument();
            string id = TestHelper.BuildAmlId( "f", uriEnum, nodeId);
            CAEXObject initialObject = document.FindByID(id);
            Assert.IsNotNull(initialObject, "Unable to find Initial Object");
            InterfaceFamilyType theObject = initialObject as InterfaceFamilyType;
            Assert.IsNotNull(theObject, "Unable to Cast Initial Object");
            return theObject;
        }

        #endregion
    }
}