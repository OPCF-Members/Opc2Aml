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
        [DataRow( "6134", "3", "3,3,3", "", DisplayName = "Three Dimensions, no Value" )]
        [DataRow( "6126", "1", "10", "{0|1|2|3|4|5|6|7|8|9}", DisplayName = "One Dimension" )]
        [DataRow( "6127", "2", "2,5", "{0|1|2|3|4|5|6|7|8|9}", DisplayName = "Two Dimensions" )]
        [DataRow( "6129", "3", "3,3,3", "{0|1|2|3|4|5|6|7|8|9|10|11|12|13|14|15|16|17|18|19|20|21|22|23|24|25|26}", DisplayName = "Three Dimensions" )]
        [DataRow( "6128", "5", "2,2,2,2,2", 
            "{0|1|2|3|4|5|6|7|8|9|10|11|12|13|14|15|16|17|18|19|20|21|22|23|24|25|26|27|28|29|30|31}", 
            DisplayName = "Five Dimensions" )]
        [DataRow( "2994", "-1", "", "", true, DisplayName = "Auditing" )]
        public void TestArray( string nodeId, string valueRank, string arrayDimensions, 
            string value, bool foundationRoot = false)
        {
            SystemUnitClassType objectToTest = GetTestObject(nodeId, foundationRoot);
            Assert.IsNotNull( objectToTest, "Unable to find nodeId" );
            
            AttributeType valueRankAttribute = objectToTest.Attribute[ "ValueRank" ];
            Assert.IsNotNull( valueRankAttribute, "Unable to find ValueRank" );
            Assert.AreEqual( valueRank, valueRankAttribute.Value );

            AttributeType arrayDimensionsAttribute = objectToTest.Attribute[ "ArrayDimensions" ];
            Assert.IsNotNull( arrayDimensionsAttribute, "Unable to find ArrayDimensions" );
            if( arrayDimensions.Length > 0 )
            {
                Assert.AreEqual( arrayDimensions, arrayDimensionsAttribute.Value );
            }
            else
            {
                Assert.IsNull( arrayDimensionsAttribute.Value );
            }

            AttributeType valueAttribute = objectToTest.Attribute[ "Value" ];
            Assert.IsNotNull( valueAttribute, "Unable to find ValueRank" );
            if ( value.Length > 0 )
            {
                Assert.AreEqual( value, valueAttribute.Value );
            }
            else
            {
                Assert.IsNull( valueAttribute.Value );
            }
        }

        #endregion

        #region Helpers

        private CAEXDocument GetDocument()
        {
            Assert.IsNotNull(m_document, "Unable to retrieve Document");
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
