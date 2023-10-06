using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Opc.Ua;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Diagnostics;

namespace SystemTest
{
    [TestClass]
    public class TestDataTypes
    {
        CAEXDocument m_document = null;
        AutomationMLContainer m_container = null;

        #region Initialize

        [TestInitialize]
        public void TestInitialize()
        {
            if (m_document == null)
            {
                foreach (FileInfo fileInfo in TestHelper.RetrieveFiles())
                {
                    if (fileInfo.Name.Equals("TestEnums.xml.amlx"))
                    {
                        m_container = new AutomationMLContainer(fileInfo.FullName,
                            System.IO.FileMode.Open, FileAccess.Read);
                        Assert.IsNotNull(m_container, "Unable to find container");
                        CAEXDocument document = CAEXDocument.LoadFromStream(m_container.RootDocumentStream());
                        Assert.IsNotNull(document, "Unable to find document");
                        m_document = document;
                    }
                }
            }

            Assert.IsNotNull(m_document, "Unable to retrieve Document");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (m_document != null)
            {
                m_document.Unload();
            }
            m_container.Dispose();

        }

        #endregion


        #region Tests

        [TestMethod]
        [DataRow( "6192", "12345", "xs:int", "Value", DisplayName = "DataValue Value" )]
        [DataRow( "6192", "2165637120", "xs:unsignedInt", "StatusCode", DisplayName = "StatusCode" )]
        [DataRow( "6192", "2023-09-13T14:39:08-06:00", "xs:dateTime", "SourceTimestamp", DisplayName = "SourceTimestamp" )]

        [DataRow( "6191", "d23c82b6-1715-4951-acd1-84fb898d6b6c", "xs:string", DisplayName = "Guid" )]
        [DataRow( "6190", "http://opcfoundation.org/UA/FX/AML/TESTING", "xs:anyURI", "NamespaceURI", DisplayName = "Qualified NamespaceURI" )]
        [DataRow( "6190", "MyQualifiedName", "xs:string", "Name", DisplayName = "Qualified Name" )]

        [DataRow( "6139", "StringNodeId", "xs:string", "RootNodeId", "StringId", DisplayName = "NodeId String" )]
        [DataRow( "6142", "12345", "xs:long", "RootNodeId", "NumericId", DisplayName = "NodeId Numeric" )]
        [DataRow( "6180", "7e1b677a-6ae8-49fa-933e-769a4aa9c524", "xs:string", "RootNodeId", "GuidId", DisplayName = "NodeId Guid" )]
        [DataRow( "6143", "T3BhcXVlTm9kZUlk", "xs:base64Binary", "RootNodeId", "OpaqueId", DisplayName = "NodeId Opaque" )]

        [DataRow( "6182", "StringNodeId", "xs:string", "RootNodeId", "StringId", DisplayName = "Expanded NodeId String" )]
        [DataRow( "6181", "12345", "xs:long", "RootNodeId", "NumericId", DisplayName = "Expanded NodeId Numeric" )]
        [DataRow( "6183", "0eb66e95-dced-415f-b8ec-43ed3f0c759b", "xs:string", "RootNodeId", "GuidId", DisplayName = "Expanded NodeId Guid" )]
        [DataRow( "6184", "T3BhcXVlTm9kZUlk", "xs:base64Binary", "RootNodeId", "OpaqueId", DisplayName = "Expanded NodeId Opaque" )]


        [DataRow( "6195", "75", "xs:int", DisplayName = "Enumeration" )]

        [DataRow( "6196", "11", "xs:int", "Value", "SymbolicId", DisplayName = "DiagnosticInfo SymbolicId" )]
        [DataRow( "6196", "22", "xs:int", "Value", "NamespaceUri", DisplayName = "DiagnosticInfo NamespaceUri" )]
        [DataRow( "6196", "33", "xs:int", "Value", "Locale", DisplayName = "DiagnosticInfo Locale" )]
        [DataRow( "6196", "44", "xs:int", "Value", "LocalizedText", DisplayName = "DiagnosticInfo LocalizedText" )]
        [DataRow( "6196", "Embedded Diagnostic Information", "xs:string", "Value", "AdditionalInfo", DisplayName = "DiagnosticInfo AdditionalInfo" )]


        [DataRow( "6198", "123.456", "xs:float", "Real", DisplayName = "ExtensionObject Complex Number Real" )]
        [DataRow( "6198", "789.012", "xs:float", "Imaginary", DisplayName = "ExtensionObject Complex Number Imaginary" )]

        [DataRow( "6199", "VariableType", "xs:string", DisplayName = "Node Class StringValue" )]

        public void TestDataTypeAttribute(string nodeId, 
            string expectedValue, 
            string expectedType, 
            string attributeTwo= "",
            string attributeThree = "",
            string attributeFour = "")
        {
            SystemUnitClassType objectToTest = GetTestObject( nodeId );
            Assert.IsNotNull( objectToTest );

            List<string> attributes = new List<string>();
            attributes.Add( "Value" );

            if( attributeTwo.Length > 0 )
            {
                attributes.Add( attributeTwo );
            }
            if( attributeThree.Length > 0 )

            {
                attributes.Add( attributeThree );
            }
            if( attributeFour.Length > 0 )
            {
                attributes.Add( attributeFour );
            }

            AttributeType attribute = GetAttribute( objectToTest, attributes );
            Assert.IsNotNull( attribute );

            Assert.AreEqual( expectedValue, attribute.Value );
            Assert.AreEqual( expectedType, attribute.AttributeDataType );
        }


        #endregion

        #region Helpers

        private AttributeType GetAttribute( SystemUnitClassType objectToTest, List<string> attributes)
        {
            AttributeType working = null;
            for( int index = 0; index < attributes.Count; index++ )
            {
                AttributeType found = null;
                if ( working == null )
                {
                    found = objectToTest.Attribute[ attributes[index] ]; 
                }
                else
                {
                    found = working.Attribute[ attributes[index] ];
                }

                if ( found != null )
                {
                    working = found;
                }
                else
                {
                    working = null;
                    break;
                }
            }

            return working;
        }

        private CAEXDocument GetDocument()
        {
            Assert.IsNotNull(m_document, "Unable to retrieve Document");
            return m_document;
        }

        public SystemUnitClassType GetTestObject( string nodeId )
        {
            CAEXDocument document = GetDocument();
            string rootName = TestHelper.GetRootName();
            CAEXObject initialObject = document.FindByID( rootName + nodeId );

            Assert.IsNotNull(initialObject, "Unable to find Initial Object");
            SystemUnitClassType theObject = initialObject as SystemUnitClassType;
            Assert.IsNotNull(theObject, "Unable to Cast Initial Object");
            return theObject;
        }

        #endregion
    }
}
