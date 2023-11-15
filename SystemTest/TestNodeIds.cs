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
    public class TestNodeIds
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

        #endregion


        #region Tests
        [TestMethod]
        [DataRow("GuidNodeIdWithAcutalGuidId", "0EB66E95-DCED-415F-B8EC-43ED3F0C759B", IdType.Guid)]
        [DataRow("NumericNodeIdWithActualNumericId", "12345", IdType.Numeric)]
        [DataRow("OpaqueNodeIdWithActualOpaqueId", "T3BhcXVlTm9kZUlk", IdType.Opaque)]
        [DataRow("StringNodeIdWithActualStringId", "StringNodeId", IdType.String)]
        public void TestNodeIdentifierTypes(string internelElemantName, string nodeId, IdType idType)
        {
            InternalElementType testInternalElement = findInternalElementByName(internelElemantName);
            Assert.IsNotNull(testInternalElement, "Could not find test object");
            var nodeIdAttribute = testInternalElement.Attribute.FirstOrDefault(childElement => childElement.Name == "NodeId");
            Assert.IsNotNull(nodeIdAttribute, "Unable to find nodeId attribute");
            var rootNodeIdAttribute = nodeIdAttribute.Attribute.FirstOrDefault(childElement => childElement.Name == "RootNodeId");
            Assert.IsNotNull(rootNodeIdAttribute, "Unable to find rootNodeId attribute");
            switch (idType)
            {
                case IdType.Opaque:
                    var opaqueNodeIdAttribute = rootNodeIdAttribute.Attribute.FirstOrDefault(childElement => childElement.Name == "OpaqueId");
                    Assert.IsNotNull(opaqueNodeIdAttribute, "Unable to find opaqueNodeId attribute");
                    Assert.AreEqual(nodeId, opaqueNodeIdAttribute.Value.ToString(), true);
                    break;

                case IdType.String:
                    var stringNodeIdAttribute = rootNodeIdAttribute.Attribute.FirstOrDefault(childElement => childElement.Name == "StringId");
                    Assert.IsNotNull(stringNodeIdAttribute, "Unable to find stringNodeId attribute");
                    Assert.AreEqual(nodeId, stringNodeIdAttribute.Value.ToString());
                    break;

                case IdType.Guid:
                    var guidNodeIdAttribute = rootNodeIdAttribute.Attribute.FirstOrDefault(childElement => childElement.Name == "GuidId");
                    Assert.IsNotNull(guidNodeIdAttribute, "Unable to find guidNodeId attribute");
                    Assert.AreEqual(nodeId, guidNodeIdAttribute.Value.ToString(), true);
                    break;

                case IdType.Numeric:
                    var numericNodeIdAttribute = rootNodeIdAttribute.Attribute.FirstOrDefault(childElement => childElement.Name == "NumericId");
                    Assert.IsNotNull(numericNodeIdAttribute, "Unable to find numericNodeId attribute");
                    Assert.AreEqual(nodeId, numericNodeIdAttribute.Value.ToString());
                    break;
            }
        }
        #endregion

        #region Helpers

        public InternalElementType? findInternalElementByName(string internelElemantName)
        {
            foreach (var instanceHierarchy in m_document.CAEXFile.InstanceHierarchy)
            {
                // browse all InternalElements deep and find element with name "FxRoot"
                foreach (var internalElement in instanceHierarchy.Descendants<InternalElementType>())
                {
                    if (internalElement.Name.Equals(internelElemantName)) return internalElement;
                }
            }

            return null;

        }

        #endregion
    }
}
