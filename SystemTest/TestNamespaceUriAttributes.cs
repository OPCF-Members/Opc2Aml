using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Opc.Ua;


namespace SystemTest
{
    [TestClass]
    public class TestNamespaceUriAttributes
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

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow( false, "3", FxAcUri, DisplayName = "FxAssetType" )]
        [DataRow( false, "175", FxAcUri, DisplayName = "Manufacturer" )]
        [DataRow( true, "5008", TestAmlUri, DisplayName = "Asset Instance" )]
        [DataRow( true, "6008", TestAmlUri, DisplayName = "Manufacturer Instance")]
        public void TestNodeIds( bool instance, string nodeIdentifier, string expectedUri)
        {
            SystemUnitClassType objectToTest = GetTestObject( instance, nodeIdentifier );
            AttributeType nodeId = objectToTest.Attribute[ "NodeId" ];
            Assert.IsNotNull( nodeId );
            AttributeType rootNodeId = nodeId.Attribute[ "RootNodeId" ];
            Assert.IsNotNull( rootNodeId );
            AttributeType namespaceUri = rootNodeId.Attribute[ "NamespaceUri" ];
            Assert.IsNotNull( namespaceUri );
            Assert.AreEqual( expectedUri, namespaceUri.Value );

            AttributeType identifier = rootNodeId.Attribute[ "NumericId" ];
            Assert.IsNotNull( identifier );
            Assert.AreEqual( nodeIdentifier, identifier.Value );
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow(false, "3", FxAcUri, DisplayName = "FxAssetType")]
        [DataRow(false, "175", DiUri, DisplayName = "Manufacturer")]
        [DataRow(true, "5008", TestAmlUri, DisplayName = "Asset Instance")]
        [DataRow(true, "6008", DiUri, DisplayName = "Manufacturer Instance")]
        public void TestBrowseNames(bool instance, string nodeIdentifier, string expectedUri)
        {
            SystemUnitClassType objectToTest = GetTestObject(instance, nodeIdentifier);
            AttributeType browseName = objectToTest.Attribute["BrowseName"];
            Assert.IsNotNull(browseName);
            AttributeType namespaceUri = browseName.Attribute["NamespaceURI"];
            Assert.IsNotNull(namespaceUri);
            Assert.AreEqual(expectedUri, namespaceUri.Value);
        }

        #endregion

        #region Helpers

        private CAEXDocument GetDocument()
        {
            if( m_document == null )
            {
                m_document = TestHelper.GetReadOnlyDocument( "AmlFxTest.xml.amlx" );
            }
            Assert.IsNotNull( m_document, "Unable to retrieve Document" );
            return m_document;
        }

        public SystemUnitClassType GetTestObject(bool instance, string nodeId)
        {
            CAEXDocument document = GetDocument();

            string rootName = FxIdPrefix;
            if ( instance )
            {
                rootName = AmlFxTestPrefix;
            }
            CAEXObject initialObject = document.FindByID(rootName + nodeId);
            Assert.IsNotNull(initialObject, "Unable to find Initial Object");
            SystemUnitClassType theObject = initialObject as SystemUnitClassType;
            Assert.IsNotNull(theObject, "Unable to Cast Initial Object");
            return theObject;
        }

        #endregion
    }
}
