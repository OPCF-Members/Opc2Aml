using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SystemTest
{
    [TestClass]

    public class Ids
    {
        CAEXDocument m_document = null;
        AutomationMLContainer m_container = null;

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

        #region Tests

        [TestMethod]
        public void TestForGuids()
        {
            Dictionary<Guid, string> interfaces = new Dictionary<Guid, string>();
            Dictionary<Guid, string> elements = new Dictionary<Guid, string>();

            foreach (InstanceHierarchyType type in m_document.CAEXFile.InstanceHierarchy)
            {
                foreach (InternalElementType internalElement in type.InternalElement)
                {
                    SystemUnitClassType classType = internalElement as SystemUnitClassType;
                    if (classType != null)
                    {
                        WalkInstanceHierarchy(classType, interfaces, elements);
                    }
                }
            }

            foreach (SystemUnitClassLibType type in m_document.CAEXFile.SystemUnitClassLib)
            {
                CAEXEnumerable<CAEXBasicObject> descendants = type.Descendants() as CAEXEnumerable<CAEXBasicObject>;
                if (descendants != null)
                {
                    foreach (CAEXBasicObject descendant in descendants)
                    {
                        SystemUnitFamilyType familyType = descendant as SystemUnitFamilyType;
                        if (familyType != null)
                        {
                            SystemUnitClassType classType = familyType as SystemUnitClassType;
                            if (classType != null)
                            {
                                WalkInstanceHierarchy(classType, interfaces, elements);
                            }
                        }
                    }
                }
            }

            List<string> output = new List<string>();

            PrepareOutput("ExternalInterface", interfaces, output);
            PrepareOutput("InternalElements", elements, output);

            DirectoryInfo outputDirectoryInfo = TestHelper.GetOpc2AmlDirectory();
            FileInfo modifiedNodeSet = new FileInfo(
                Path.Combine(outputDirectoryInfo.FullName, "Guids.txt"));

            TestHelper.WriteFile(modifiedNodeSet.FullName, output);

            Assert.AreEqual(0, interfaces.Count, "External Interfaces have guids");
            Assert.AreEqual(0, interfaces.Count, "Internal Interfaces have guids");
        }

        private void PrepareOutput(string title,
            Dictionary<Guid, string> dictionary,
            List<string> output)
        {
            foreach (KeyValuePair<Guid, string> entry in dictionary)
            {
                output.Add(title + " Path " + entry.Value + " has guid Id " + entry.Key.ToString());
            }
        }

        private void WalkInstanceHierarchy(SystemUnitClassType classType,
            Dictionary<Guid, string> interfaces,
            Dictionary<Guid, string> elements,
            string path = "")
        {
            string next = classType.Name;
            if (path.Length > 0)
            {
                next = path + "_" + classType.Name;
            }

            foreach (ExternalInterfaceType externalInterface in classType.ExternalInterface)
            {
                Guid potentialGuid;
                if ( Guid.TryParse(externalInterface.ID, out potentialGuid ) )
                {
                    interfaces.Add(potentialGuid, path);
                }
            }

            foreach (InternalElementType internalElement in classType.InternalElement)
            {
                Guid potentialGuid;
                if (Guid.TryParse(internalElement.ID, out potentialGuid))
                {
                    elements.Add(potentialGuid, path);
                }
                WalkInstanceHierarchy(internalElement, interfaces, elements, next);
            }
        }

        #endregion
    }
}
