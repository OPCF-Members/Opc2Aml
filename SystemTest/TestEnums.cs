using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Aml.Engine.Services;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using System.Reflection.Metadata;
using Aml.Engine.Adapter;

namespace SystemTest
{
    [TestClass]
    public class TestEnums
    {
        static List<string> TestFiles;
        static List<FileInfo> AmlxFiles;
        static List<FileInfo> AmlFiles;

        private const string RootName = "ns=http#\\\\opcfoundation.org\\UA\\FX\\AML\\TESTING;i=";

        CAEXDocument m_document = null;
        AutomationMLContainer m_container = null;

        #region Initialize

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            TestFiles = TestHelper.GetTestFileNames();
            TestHelper.PrepareUnconvertedXml(TestFiles);
            System.Threading.Thread.Sleep(1000);
            TestHelper.Execute();
            AmlFiles = TestHelper.ExtractAmlxFiles(TestFiles);
            Assert.AreNotEqual(0, AmlFiles.Count, "Unable to get Converted Aml files");
            AmlxFiles = TestHelper.GetAmlxFiles();
            Assert.AreNotEqual(0, AmlxFiles.Count, "Unable to get Converted Amlx files");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            if (m_document == null)
            {
                foreach (FileInfo fileInfo in AmlxFiles)
                {
                    if (fileInfo.Name.Equals("TestEnums.xml.amlx"))
                    {
                        m_container = new AutomationMLContainer(fileInfo.FullName,
                            System.IO.FileMode.Open, FileAccess.Read);
                        Assert.IsNotNull(m_container, "Unable to find container");
                        CAEXDocument document = CAEXDocument.LoadFromStream(m_container.RootDocumentStream());
                        Assert.IsNotNull(document, "Unable to find document");
                        m_document= document;
                    }
                }
            }

            Assert.IsNotNull(m_document, "Unable to retrieve Document");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if ( m_document != null)
            {
                m_document.Unload();
            }
            m_container.Dispose();

        }

        #endregion


        #region Tests

        [TestMethod]
        [DataRow("5015", "TrueState", "FalseState", true, DisplayName = "Default Parameters")]
        [DataRow("5016", "OverrideTrueState", "OverrideFalseState", true, DisplayName = "Override Default Parameters")]
        [DataRow("5017", "InstanceTrueState", "InstanceFalseState", false, DisplayName = "Override Parameters")]
        public void TestTwoStateInstances(string nodeId, string expectedTrue, string expectedFalse, bool expectedValue)
        {
            InternalElementType objectToTest = GetObjectToTest(nodeId, "Test_TwoState");

            AttributeType boolValue = objectToTest.Attribute["Value"];
            Assert.IsNotNull(boolValue, "Unable to retrieve boolean value");

            string expectedBooleanValue = expectedValue ? "true" : "false";
            Assert.AreEqual(expectedBooleanValue, boolValue.Value);

            InternalElementType trueState = objectToTest.InternalElement["TrueState"];
            InternalElementType falseState = objectToTest.InternalElement["FalseState"];
            InternalElementType valueAsText = objectToTest.InternalElement["ValueAsText"];

            Assert.IsNotNull(trueState, "Unable Find TrueState Property");
            Assert.IsNotNull(falseState, "Unable Find FalseState Property");
            Assert.IsNotNull(valueAsText, "Unable Find ValueAsText Property");

            Assert.AreEqual(expectedTrue, trueState.Attribute["Value"].Value, "Unexpected Value for True State");
            Assert.AreEqual(expectedFalse, falseState.Attribute["Value"].Value, "Unexpected Value for False State");

            string expectedValueAsText = expectedValue ? expectedTrue : expectedFalse;
            Assert.AreEqual(expectedValueAsText, valueAsText.Attribute["Value"].Value, "Unexpected value for ValueAsText");
        }



        [TestMethod]
        [DataRow("6012", "", "", false, DisplayName = "Has No Values")]
        [DataRow("6013", "TrueState", "FalseState", true, DisplayName = "Has Values")]
        public void TestTwoStateClasses(string nodeId, string expectedTrue, string expectedFalse, bool hasValues)
        {
            CAEXDocument document = GetDocument();
            CAEXObject initialClass = document.FindByID(RootName + nodeId);
            InternalElementType classToTest = initialClass as InternalElementType;
            Assert.IsNotNull(classToTest, "Unable to retrieve class to test");

            InternalElementType trueState = classToTest.InternalElement["TrueState"];
            InternalElementType falseState = classToTest.InternalElement["FalseState"];

            Assert.IsNotNull(trueState, "Unable Find TrueState Property");
            Assert.IsNotNull(falseState, "Unable Find FalseState Property");

            AttributeType trueValueAttribute = trueState.Attribute["Value"];
            AttributeType falseValueAttribute = falseState.Attribute["Value"];

            Assert.IsNotNull(trueValueAttribute, "Unable Find TrueState Attribute");
            Assert.IsNotNull(falseValueAttribute, "Unable Find FalseState Attribute");

            if ( hasValues)
            {
                Assert.AreEqual(expectedTrue, trueValueAttribute.Value, "Unexpected TrueState Value");
                Assert.AreEqual(expectedFalse, falseValueAttribute.Value, "Unexpected FalseState Value");
            }
            else
            {
                Assert.IsNull(trueValueAttribute.Value, "Unexpected TrueState Attribute");
                Assert.IsNull(falseValueAttribute.Value, "Unexpected FalseState Attribute");
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

        public InternalElementType GetObjectToTest(string nodeId, string search)
        {
            CAEXDocument document = GetDocument();
            InternalElementType objectToTest = null;
            CAEXObject initialObject = document.FindByID(RootName + nodeId);
            InternalElementType initialInternalElement = initialObject as InternalElementType;
            Assert.IsNotNull(initialInternalElement, "Unable to find Initial Object");
            foreach (InternalElementType element in initialInternalElement.InternalElement)
            {
                if (element.Name.StartsWith(search))
                {
                    objectToTest = element;
                    break;
                }

            }
            Assert.IsNotNull(objectToTest, "Unable to find Test Object");
            return objectToTest;
        }

        #endregion
    }
}