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
    public class TestInstanceNodeIdAttributes
    {
        CAEXDocument m_document = null;

        #region Tests

        [TestMethod]
        [DataRow( "6144", TestHelper.TestAmlUri, false, DisplayName = "EventId variable should have instance NodeId" )]
        [DataRow( "6130", TestHelper.TestAmlUri, false, DisplayName = "InputArguments should have instance NodeId" )]
        [DataRow( "2255", Opc.Ua.Namespaces.OpcUa, true, DisplayName = "Namespace variable should have instance NodeId" )]
        public void TestInstanceNodeIds( string nodeIdentifier, string expectedUri, bool foundationRoot )
        {
            SystemUnitClassType objectToTest = GetTestObject( nodeIdentifier, foundationRoot );
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

        public SystemUnitClassType GetTestObject(string nodeId, bool foundationRoot = false)
        {
            CAEXDocument document = GetDocument();
            string rootName = TestHelper.GetRootName();
            if ( foundationRoot )
            {
                rootName = TestHelper.GetOpcRootName();
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
