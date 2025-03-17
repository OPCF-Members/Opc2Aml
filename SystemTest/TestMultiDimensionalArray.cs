using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Opc.Ua;


namespace SystemTest
{
    [TestClass]
    public class TestMultiDimensionalArray
    {
        CAEXDocument m_document = null;

        #region Tests

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow( "6134", "3", "3,3,3", false, DisplayName = "Three Dimensions, no Value" )]
        [DataRow( "6126", "1", "10", true, DisplayName = "One Dimension" )]
        [DataRow( "6127", "2", "2,5", true, DisplayName = "Two Dimensions" )]
        [DataRow( "6129", "3", "3,3,3", true, DisplayName = "Three Dimensions" )]
        [DataRow( "6128", "5", "2,2,2,2,2", true, DisplayName = "Five Dimensions" )]
        [DataRow( "2994", "-1", "", false, true, DisplayName = "Auditing" )]
        public void TestArray( string nodeId, string valueRank, string arrayDimensions, 
            bool expectValues, bool foundationRoot = false)
        {
            SystemUnitClassType objectToTest = GetTestObject(nodeId, foundationRoot);
            Assert.IsNotNull( objectToTest, "Unable to find nodeId" );
            
            AttributeType valueRankAttribute = objectToTest.Attribute[ "ValueRank" ];
            Assert.IsNotNull( valueRankAttribute, "Unable to find ValueRank" );
            Assert.AreEqual( valueRank, valueRankAttribute.Value );

            AttributeType arrayDimensionsAttribute = objectToTest.Attribute[ "ArrayDimensions" ];
            if (string.IsNullOrEmpty(arrayDimensions))
            {
                Assert.IsNull(arrayDimensionsAttribute);
            }
            else
            {
                Assert.IsNotNull(arrayDimensionsAttribute, "Unable to find ArrayDimensions");
            }

            AttributeType valueAttribute = objectToTest.Attribute[ "Value" ];
            Assert.IsNotNull( valueAttribute, "Unable to find Value Attribute" );


            string[] arrayDimensionList = arrayDimensions.Split( ',' );
            int arrayDimensionCount = 0;
            foreach( string dimension in arrayDimensionList )
            {
                int dimensionValue;
                if ( int.TryParse( dimension, out dimensionValue ) )
                {
                    if( arrayDimensionCount == 0 )
                    {
                        arrayDimensionCount = dimensionValue;
                    }
                    else
                    {
                        arrayDimensionCount *= dimensionValue;
                    }
                }
            }

            if ( expectValues )
            {
                for( int index = 0; index < arrayDimensionCount; index++ )
                {
                    AttributeType elementAttribute = valueAttribute.Attribute[ index.ToString() ];
                    Assert.IsNotNull( elementAttribute, "Unable to find index Element value" );
                    Assert.AreEqual( index.ToString(), elementAttribute.Value );
                }
            }
            else
            {
                Assert.AreEqual( 0, valueAttribute.Attribute.Count );
            }
        }


        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestMethodClassAttributes()
        {
            CAEXDocument document = GetDocument();
            CAEXObject initialObject = document.FindByID( "686619c7-0101-4869-b398-aa0f98bc5f54" );
            Assert.IsNotNull( initialObject );
            SystemUnitClassType objectToTest = initialObject as SystemUnitClassType;
            Assert.IsNotNull( objectToTest );

            AttributeType browseNameAttribute = objectToTest.Attribute[ "BrowseName" ];
            Assert.IsNotNull( browseNameAttribute );

            AttributeType namespaceUriAttribute = browseNameAttribute.Attribute[ "NamespaceUri" ];
            Assert.IsNotNull( namespaceUriAttribute );
            Assert.AreEqual( "xs:anyURI", namespaceUriAttribute.AttributeDataType );

            AttributeType nameAttribute = browseNameAttribute.Attribute[ "Name" ];
            Assert.IsNotNull( nameAttribute );
            Assert.AreEqual( "xs:string", nameAttribute.AttributeDataType );
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow( "2330", "", false, true, DisplayName = "SUC HistoryServerCapabilitiesType should not have ArrayDimensions" )]
        [DataRow( "24186", "", false, true, DisplayName = "SUC FailureCode should not have ArrayDimensions" )]
        [DataRow( "24187", "0,8", true, true, DisplayName = "SUC FailureSystemIdentifier ArrayDimensions" )]
        [DataRow( "5013", "", false, false, DisplayName = "Folder should not have ArrayDimensions" )]
        [DataRow( "6190", "", false, false, DisplayName = "Scalar Value should not have ArrayDimension elements" )]
        [DataRow( "6126", "10", true, false, DisplayName = "Single ArrayDimension element" )]
        [DataRow( "6129", "3,3,3", true, false, DisplayName = "Three ArrayDimension elements" )]
        [DataRow( "6128", "2,2,2,2,2", true, false, DisplayName = "Five ArrayDimension elements" )]

        public void TestArrayDimensions( string nodeId, string expected,
            bool expectedToBeFound, bool foundationRoot )
        {
            SystemUnitClassType objectToTest = GetTestObject( nodeId, foundationRoot );

            AttributeType attributeType = objectToTest.Attribute[ "ArrayDimensions" ];

            if( expectedToBeFound )
            {
                Assert.IsNotNull( attributeType, "ArrayDimensions not found" );
                Assert.AreEqual( "[ATL_http://opcfoundation.org/UA/]/[ListOfUInt32]", attributeType.RefAttributeType );
                if( attributeType.Value != null )
                {
                    Assert.AreEqual( 0, attributeType.Value.Length );
                }

                if( expected.Length > 0 )
                {
                    string[] parts = expected.Split( ',' );
                    for( int index = 0; index < parts.Length; index++ )
                    {
                        AttributeType indexedAttribute = attributeType.Attribute[ index.ToString() ];
                        Assert.IsNotNull( indexedAttribute, "ArrayDimensions index " + index.ToString() + " not found" );
                        Assert.AreEqual( parts[ index ], indexedAttribute.Value );
                        Assert.AreEqual( "xs:unsignedInt", indexedAttribute.AttributeDataType );
                    }
                }
                else
                {
                    Assert.AreEqual( 0, attributeType.Attribute.Count );
                }
            }
            else
            {
                Assert.IsNull( attributeType, "Unexpected ArrayDimensions found" );
            }
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow( "2330", "", false, true, DisplayName = "SUC HistoryServerCapabilitiesType should not have ValueRank" )]
        [DataRow( "24186", "-1", true, true, DisplayName = "SUC FailureCode should have Scalar ValueRank" )]
        [DataRow( "24187", "2", true, true, DisplayName = "SUC FailureSystemIdentifier" )]
        [DataRow( "5013", "", false, false, DisplayName = "Folder should not have ValueRank" )]
        [DataRow( "6190", "-1", true, false, DisplayName = "Scalar Value" )]
        [DataRow( "6126", "1", true, false, DisplayName = "Single ArrayDimension element" )]
        [DataRow( "6129", "3", true, false, DisplayName = "Three ArrayDimension elements" )]
        [DataRow( "6128", "5", true, false, DisplayName = "Five ArrayDimension elements" )]

        public void TestValueRank( string nodeId, string expected,
            bool expectedToBeFound, bool foundationRoot )
        {
            SystemUnitClassType objectToTest = GetTestObject( nodeId, foundationRoot );

            AttributeType attributeType = objectToTest.Attribute[ "ValueRank" ];

            if( expectedToBeFound )
            {
                Assert.IsNotNull( attributeType, "ValueRank not found" );
                Assert.IsNotNull( attributeType.Value, "ValueRank not found" );
                Assert.AreEqual( "xs:int", attributeType.AttributeDataType );
                Assert.AreEqual( expected, attributeType.Value );
                Assert.AreEqual( "[ATL_http://opcfoundation.org/UA/]/[Int32]", attributeType.RefAttributeType );
            }
            else
            {
                Assert.IsNull( attributeType, "Unexpected ValueRank found" );
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
