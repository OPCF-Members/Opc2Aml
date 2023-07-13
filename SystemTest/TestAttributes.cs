using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Opc.Ua;


namespace SystemTest
{
    [TestClass]
    public class TestAttributes
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
        [DataRow("Description", "5001",
            "A Folder to store instances specifically for testing purposes",
            true, DisplayName = "Instance Expected Description")]
        [DataRow("Description", "5002", "", false, DisplayName = "Instance No Description")]
        [DataRow("Description", "1000",
            "Test a MultiStateValue with Predefined Values on the Type",
            true, DisplayName = "Object Expected Description")]
        [DataRow("Description", "1007", "", false, DisplayName = "Object No Description")]


        [DataRow("DisplayName", "5001", "Enumeration Testing", true, DisplayName = "Instance Expected DisplayName")]
        [DataRow("DisplayName", "5002", "", false, DisplayName = "Instance No DisplayName")]
        [DataRow("DisplayName", "1007", "Test Connector Type Display Name", true, DisplayName = "Object Expected DisplayName")]
        [DataRow("DisplayName", "1000", "", false, DisplayName = "Object No DisplayName")]

        [DataRow("Description", "7009",
            "This Method Has a Description",
            true, DisplayName = "Instance Method Expect Description")]
        [DataRow("Description", "7018", "", false, DisplayName = "Instance Method No Description")]
        [DataRow("Description", "7000",
            "This Method Has a Description",
            true, DisplayName = "Object Method Expect Description")]
        [DataRow("Description", "7001", "", false, DisplayName = "Object Method No Description")]
        [DataRow("IsAbstract", "2782", "true", true, true, DisplayName = "ConditionType should be Abstract")]
        [DataRow("IsAbstract", "2881", "", 
            false, true, DisplayName = "AcknowledgeableConditionType should not be Abstract")]

        public void TestAttribute(string attribute, string nodeId, 
            string expected, bool expectedToBeFound, bool foundationRoot = false)
        {
            SystemUnitClassType objectToTest = GetTestObject(nodeId, foundationRoot);
            AttributeType attributeType = objectToTest.Attribute[attribute];
            if (expectedToBeFound)
            {
                Assert.IsNotNull(attributeType, attribute + " attribute not found");
                Assert.AreEqual(expected, attributeType.Value, "Unexpected value for " + attribute);
            }
            else
            {
                Assert.IsNull(attributeType, "Unexpected attribute found for " + attribute);
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
