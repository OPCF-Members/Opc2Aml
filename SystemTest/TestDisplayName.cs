using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;


namespace SystemTest
{
    [TestClass]
    public class TestDisplayName
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
        [DataRow("5001", "Enumeration Testing", true, DisplayName = "Instance Expected DisplayName")]
        [DataRow("5002", "", false, DisplayName = "Instance No DisplayName")]
        public void InstanceDisplayName(string nodeId, string expectedDisplayName, bool expectedToBeFound)
        {
            InternalElementType initialInternalElement = GetObjectToTest(nodeId);
            AttributeType displayNameAttribute = initialInternalElement.Attribute["DisplayName"];
            if (expectedToBeFound)
            {
                Assert.IsNotNull(displayNameAttribute, "DisplayName attribute not found");
                Assert.AreEqual(expectedDisplayName, displayNameAttribute.Value, "Unexpected value for DisplayName");
            }
            else
            {
                Assert.IsNull(displayNameAttribute, "Unexpected attribute found for DisplayName");
            }
        }

        [TestMethod]
        [DataRow("1007", "Test Connector Type Display Name", true, DisplayName = "Object Expected DisplayName")]
        [DataRow("1000", "", false, DisplayName = "Object No DisplayName")]
        public void ObjectDisplayName(string nodeId, string expectedDisplayName, bool expectedToBeFound)
        {
            SystemUnitFamilyType objectToTest = GetSystemUnitToTest(nodeId);
            AttributeType displayNameAttribute = objectToTest.Attribute["DisplayName"];
            if (expectedToBeFound)
            {
                Assert.IsNotNull(displayNameAttribute, "DisplayName attribute not found");
                Assert.AreEqual(expectedDisplayName, displayNameAttribute.Value, "Unexpected value for DisplayName");
            }
            else
            {
                Assert.IsNull(displayNameAttribute, "Unexpected attribute found for DisplayName");
            }
        }

        #endregion

        #region Helpers

        private CAEXDocument GetDocument()
        {
            Assert.IsNotNull(m_document, "Unable to retrieve Document");
            return m_document;
        }

        private CAEXFileType GetFile()
        {
            CAEXDocument document = GetDocument();
            Assert.IsNotNull(document.CAEXFile, "Unable to retrieve File");
            return document.CAEXFile;
        }

        private CAEXSequenceOfCAEXObjects<SystemUnitClassLibType> GetSystemUnitClasses()
        {
            CAEXFileType file = GetFile();
            Assert.IsNotNull(file.SystemUnitClassLib, "Unable to retrieve SystemUnitTypes");
            return file.SystemUnitClassLib;
        }

        private CAEXSequenceOfCAEXObjects<InstanceHierarchyType> GetInstances()
        {
            CAEXFileType file = GetFile();
            Assert.IsNotNull(file.InstanceHierarchy, "Unable to retrieve Instances");
            return file.InstanceHierarchy;
        }

        public InternalElementType GetObjectToTest(string nodeId)
        {
            CAEXDocument document = GetDocument();
            CAEXObject initialObject = document.FindByID(TestHelper.GetRootName() + nodeId);
            Assert.IsNotNull(initialObject, "Unable to find Initial Object");
            InternalElementType initialInternalElement = initialObject as InternalElementType;
            Assert.IsNotNull(initialInternalElement, "Unable to find Initial Object");
            return initialInternalElement;
        }

        public SystemUnitFamilyType GetSystemUnitToTest(string nodeId)
        {
            CAEXDocument document = GetDocument();
            InternalElementType objectToTest = null;
            CAEXObject initialObject = document.FindByID(TestHelper.GetRootName() + nodeId);
            Assert.IsNotNull(initialObject, "Unable to find Initial Object");
            SystemUnitFamilyType initialInternalElement = initialObject as SystemUnitFamilyType;
            Assert.IsNotNull(initialInternalElement, "Unable to find Initial Object");
            return initialInternalElement;
        }

        #endregion
    }
}
