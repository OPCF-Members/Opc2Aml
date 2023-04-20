using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SystemTest
{
    [TestClass]
    public class XmlDocTests
    {
        private XmlDocument m_doc = null;

        [TestInitialize]
        public void TestInitialize()
        {
            TestHelper.RetrieveFiles();
            string testFileName = "TestEnums.xml";
            DirectoryInfo directory = TestHelper.GetExtractDirectory(testFileName + ".amlx");
            string path = Path.Combine(directory.FullName, testFileName + ".aml");
            FileInfo amlFileInfo = new FileInfo(path);
            if ( amlFileInfo.Exists )
            {
                m_doc = new XmlDocument();
                m_doc.Load(amlFileInfo.FullName);
            }
        }

        [TestMethod]
        public void InternalElementGuidCount()
        {
            foreach (XmlNode child in m_doc.ChildNodes)
            {
                if (child.Name == "CAEXFile")
                {
                    foreach(XmlNode subChild in child.ChildNodes)
                    {
                        Debug.WriteLine(subChild.Name);

                        if (subChild.Name == "InstanceHierarchy" ||
                            subChild.Name == "SystemUnitClassLib")
                        {
                            Assert.AreEqual(0, GetGuidCount(subChild), "Invalid " + subChild.Name + " Guid Count");
                        }
                    }
                }
            }
        }

        private int GetGuidCount(XmlNode rootNode)
        {
            Dictionary<string, XmlNode> guids = new Dictionary<string, XmlNode>();

            GetGuidCount(rootNode, guids);

            return guids.Count;
        }

        private void GetGuidCount(XmlNode node, Dictionary<string, XmlNode> guidMap)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "InternalElement")
                {
                    string idValue = getAttribute(child, "ID");

                    Guid possible;
                    if (Guid.TryParse(idValue, out possible))
                    {
                        guidMap.Add(child.Name, child);
                    }
                }
                GetGuidCount(child, guidMap);
            }
        }

        string getAttribute(XmlNode node, string attributeId)
        {
            string value = "";

            if (node.Attributes != null)
            {
                XmlAttribute idAttribute = node.Attributes[attributeId];
                if (idAttribute != null)
                {
                    value = idAttribute.Value;
                }
            }

            return value;
        }



    }
}
