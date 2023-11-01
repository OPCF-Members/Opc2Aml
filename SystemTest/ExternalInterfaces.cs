using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemTest
{
    [TestClass]

    public class ExternalInterfaces
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
                    if (fileInfo.Name.Equals("TestAml.xml.amlx"))
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


        

        // This test is more about studying the file output, but has validity as a real test        
        private int LinkInternalElementDifferentCount;
        [TestMethod]
        public void DifferencesBetweenLinksAndInternalElements()
        {
            LinkInternalElementDifferentCount = 0;
            // Walk the Instances, compare InternalElements agains the number of InternalLinks
            var instanceHierarchy = m_document.CAEXFile.InstanceHierarchy;
            List<string> output = new List<string>();
            foreach( InstanceHierarchyType type in instanceHierarchy )
            {
                foreach (InternalElementType internalElement in type.InternalElement)
                {
                    output.Add("Root Level " + type.Name + 
                        " walking the instance Hierarchy for internalElement " + internalElement.Name);
                    WalkInstanceHierarchy(internalElement, output, internalElement.Name);
                }
            }
            TestHelper.WriteFile("WalkInstanceHierarchy.txt", output);

            Assert.AreEqual(0, LinkInternalElementDifferentCount);
        }

        private void WalkInstanceHierarchy(InternalElementType instance, List<string> output, string title, int level = 0)
        {
            StringBuilder spaces = new StringBuilder();
            for (int index = 0; index < level; index++)
            {
                spaces.Append(" ");
            }

            output.Add(spaces + "Starting " + title);
            foreach (  InternalElementType internalElement in instance.InternalElement )
            {
                WalkInstanceHierarchy(internalElement, output, title + "_" + internalElement.Name, level + 1);
            }

            if ( instance.Count() == instance.InternalLink.Count )
            {
                if (instance.Count() > 0)
                {
                    output.Add(spaces + "Valid " + instance.Name + " has equal counts for InternalElements and InternalLinks [" +
                        instance.Count().ToString() + "]");
                }
            }
            else
            {
                LinkInternalElementDifferentCount++;
                output.Add(spaces + "INVALID Counts " + instance.Name + " has ["+ instance.Count() + "] InternalElements and ["
                    + instance.InternalLink.Count + "] InternalLinks" );
            }
        }

        #endregion

        #region Helpers

        #endregion


    }
}
