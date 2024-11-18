using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Opc.Ua;


namespace SystemTest
{
    [TestClass]
    public class TestServerDiagnostics
    {


        // Ensure that SystemDiagnostics is proper
        // No values for any point except EnabledFlag

        // Therefore I need values in my test document

        CAEXDocument m_document = null;

        #region Tests

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestEnabledFlag()
        {
            SystemUnitClassType testObject = GetTestObject( "2294", true );
            AttributeType attributeType = testObject.Attribute[ "Value" ];
            Assert.IsNotNull( attributeType );
            Assert.AreEqual("true", attributeType.Value, true );
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]

        [DataRow( "6224", DisplayName = "SessionDiagnosticsDataType" )]
        [DataRow( "6225", DisplayName = "SessionSecurityDiagnosticsDataType" )]
        [DataRow( "6226", DisplayName = "SubscriptionDiagnosticsDataType" )]
        public void TestChildVariable(string nodeIdString)
        {
            SystemUnitClassType testObject = GetTestObject( nodeIdString, false );
            AttributeType valueAttribute = testObject.Attribute[ "Value" ];
            Assert.IsNotNull( valueAttribute );
            AttributeType sessionIdAttribute = valueAttribute.Attribute[ "SessionId" ];
            Assert.IsNotNull( sessionIdAttribute );
            AttributeType rootNodeIdAttribute = sessionIdAttribute.Attribute[ "RootNodeId" ];
            Assert.IsNotNull( rootNodeIdAttribute );
            AttributeType namespaceUriAttribute = rootNodeIdAttribute.Attribute[ "NamespaceUri" ];
            Assert.IsNotNull( namespaceUriAttribute );
            if ( namespaceUriAttribute.Value != null )
            {
                if ( namespaceUriAttribute.Value.Length > 0 )
                {
                    Assert.Fail( "Unexpected value" );
                }
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
