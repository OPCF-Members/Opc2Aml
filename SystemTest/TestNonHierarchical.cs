using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using System;
using System.Reflection.Metadata;
using System.Collections.Generic;

namespace SystemTest
{
    [TestClass]
    public class TestNonHierarchical
    {
        CAEXDocument m_document = null;
        private const string AttributeName = "RefSystemUnitPath";

        #region Tests

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow("", TestHelper.Uris.Root, Opc.Ua.Variables.AlarmConditionType_EnabledState,
            "HasTrueSubState",
            TestHelper.Uris.Root, Opc.Ua.Variables.AlarmConditionType_ActiveState,
            DisplayName = "SystemUnitClass Enabled To ActiveState")]
        [DataRow("", TestHelper.Uris.Root, Opc.Ua.Variables.AlarmConditionType_EnabledState,
            "HasTrueSubState",
            TestHelper.Uris.Root, Opc.Ua.Variables.AlarmConditionType_SuppressedState,
            DisplayName = "SystemUnitClass Enabled To SuppressedState")]
        [DataRow("", TestHelper.Uris.Root, Opc.Ua.Variables.AlarmConditionType_EnabledState,
            "HasTrueSubState",
            TestHelper.Uris.Root, Opc.Ua.Objects.AlarmConditionType_ShelvingState,
            DisplayName = "SystemUnitClass Enabled To ShelvingState")]

        [DataRow("", TestHelper.Uris.Root, Opc.Ua.Methods.AcknowledgeableConditionType_Acknowledge,
            "AlwaysGeneratesEvent",
            TestHelper.Uris.Root, Opc.Ua.ObjectTypes.AuditConditionAcknowledgeEventType,
            DisplayName = "SystemUnitClass Acknowledge To Auditing Event")]
        [DataRow("CertificateGroupType_CertificateExpired_Acknowledge_", 
            TestHelper.Uris.Root, Opc.Ua.Methods.CertificateGroupType_CertificateExpired_Acknowledge,
            "AlwaysGeneratesEvent",
            TestHelper.Uris.Root, Opc.Ua.ObjectTypes.AuditConditionAcknowledgeEventType,
            DisplayName = "CertificateExpired Acknowledge To Auditing Event")]
        [DataRow("CertificateGroupType_TrustListOutOfDate_Acknowledge_", 
            TestHelper.Uris.Root, Opc.Ua.Methods.CertificateGroupType_TrustListOutOfDate_Acknowledge,
            "AlwaysGeneratesEvent",
            TestHelper.Uris.Root, Opc.Ua.ObjectTypes.AuditConditionAcknowledgeEventType,
            DisplayName = "TrustListOutOfDate Acknowledge To Auditing Event")]


        [DataRow("", TestHelper.Uris.AmlFxTest, 7012u,
            "AlwaysGeneratesEvent",
            TestHelper.Uris.Root, Opc.Ua.ObjectTypes.AuditConditionAcknowledgeEventType,
            DisplayName = "Defined Acknowledge To Auditing Event")]


        [DataRow("", TestHelper.Uris.AmlFxTest, 7005u,
            "AlwaysGeneratesEvent",
            TestHelper.Uris.Root, Opc.Ua.ObjectTypes.AuditConditionAcknowledgeEventType,
            DisplayName = "Derived Acknowledge To Auditing Event")]

        [DataRow("", TestHelper.Uris.AmlFxTest, 7004u,
            "AlwaysGeneratesEvent",
            TestHelper.Uris.Root, Opc.Ua.ObjectTypes.AuditConditionAcknowledgeEventType,
            DisplayName = "Derived Custom Acknowledge To Auditing Event")]


        [DataRow("", TestHelper.Uris.AmlFxTest, 5009u,
            "IsHostedBy",
            TestHelper.Uris.AmlFxTest, 5008u,
            DisplayName = "Instance Hosts")]

        public void TestReference(string prefix, TestHelper.Uris testUri, uint testNodeId, string referenceName,
            TestHelper.Uris targetUri, uint targetNodeId)
        {
            SystemUnitClassType testObject = GetInternalElement( prefix, testUri, testNodeId);
            AttributeType holder = GetReferenceAttribute(testObject, referenceName, -1); 
            Dictionary<int, string> references = new Dictionary<int, string>();
            CAEXDocument document = GetDocument(); 
            
            string reverseName = GetInverseInterfaceName(referenceName);

            string targetNodeIdString = targetNodeId.ToString();
            AttributeType testAttribute = null;
            foreach (AttributeType attribute in holder.Attribute)
            {
                if ( attribute.Value.EndsWith( targetNodeIdString ) )
                {
                    // Step One Done
                    testAttribute = attribute;
                    break;
                }
            }
            Assert.IsNotNull(testAttribute);
            CAEXObject caexObject = document.FindByID(testAttribute.Value);
            Assert.IsNotNull(caexObject);
            SystemUnitClassType target = caexObject as SystemUnitClassType;
            Assert.IsNotNull(target);
            AttributeType reverseHolder = GetReferenceAttribute(target, reverseName, -1);
            string testNodeIdString = testNodeId.ToString();
            AttributeType originalAttribute = null;
            foreach (AttributeType attribute in reverseHolder.Attribute)
            {
                if (attribute.Value.EndsWith(testNodeIdString))
                {
                    // Step One Done
                    originalAttribute = attribute;
                    break;
                }
            }

            Assert.IsNotNull(originalAttribute);
            CAEXObject originalObject = document.FindByID(originalAttribute.Value);
            Assert.IsNotNull(originalObject);
            SystemUnitClassType originalSystemUnitClass = originalObject as SystemUnitClassType;
            Assert.IsNotNull(originalSystemUnitClass);

            // OriginalSystemUnitClass should be the same as our original object
            Assert.AreEqual(testObject.ID, originalSystemUnitClass.ID);

            bool wait = true;
            

        }

        #endregion

        #region Helpers

        private CAEXDocument GetDocument()
        {
            if( m_document == null )
            {
                m_document = TestHelper.GetReadOnlyDocument( "AmlFxTest.xml.amlx" );
            }
            Assert.IsNotNull( m_document, "Unable to retrieve Document" );
            return m_document;
        }

        public SystemUnitClassType GetInternalElement(string prefix, TestHelper.Uris uriId, uint nodeId)
        {
            CAEXDocument document = GetDocument();
            string amlId = prefix + TestHelper.BuildAmlId("", uriId, nodeId.ToString());
            Console.WriteLine("Looking for " + amlId);
            CAEXObject initialObject = document.FindByID(amlId);
            Assert.IsNotNull(initialObject, "Unable to find Initial Object");
            SystemUnitClassType theObject = initialObject as SystemUnitClassType;
            Assert.IsNotNull(theObject, "Unable to Cast Initial Object");

            return theObject;
        }

        public ExternalInterfaceType GetExternalInterface(
            SystemUnitClassType systemUnitClass, string externalInterfaceName )
        {
            ExternalInterfaceType externalInterface = systemUnitClass.ExternalInterface[externalInterfaceName];
            Assert.IsNotNull( externalInterface, "Unable to find ExternalInterface " + externalInterfaceName);

            return externalInterface;
        }

        public AttributeType GetReferenceAttribute(SystemUnitClassType systemUnitClass,
            string externalInterfaceName, int index)
        {
            ExternalInterfaceType externalInterface = GetExternalInterface(systemUnitClass,
                externalInterfaceName);
            AttributeType holder = GetAttribute(externalInterface.Attribute, AttributeName);
            if ( index >= 0 )
            {
                AttributeType indexed = GetAttribute(holder, index.ToString());
                return indexed;
            }

            return holder;
        }

        public AttributeFamilyType GetTestAttribute( TestHelper.Uris uriId, uint nodeId )
        {
            CAEXDocument document = GetDocument();
            string amlId = TestHelper.BuildAmlId("", uriId, nodeId.ToString() );
            Console.WriteLine( "Looking for " + amlId );
            CAEXObject initialObject = document.FindByID( amlId );
            Assert.IsNotNull( initialObject, "Unable to find Initial Object" );
            AttributeFamilyType theObject = initialObject as AttributeFamilyType;
            Assert.IsNotNull( theObject, "Unable to Cast Initial Object" );
            return theObject;
        }

        public string GetInverseInterfaceName( string forwardName )
        {
            // Only using root objects for this test
            string libraryId = "ICL_" + TestHelper.GetUri(TestHelper.Uris.Root);
            CAEXDocument document = GetDocument();
            CAEXObject baseObject =  document.InterfaceClassLib[libraryId];
            InterfaceClassLibType interfaceLib = baseObject as InterfaceClassLibType;
            Assert.IsNotNull(interfaceLib);
            InterfaceFamilyType interfaceClass = interfaceLib.InterfaceClass[forwardName];
            Assert.IsNotNull(interfaceClass);
            string inverseName = string.Empty;
            bool waiting = true;
            if (interfaceClass.InterfaceClass.Count > 0)
            {
                InterfaceClassType interfaceClassType = interfaceClass.InterfaceClass[0];
                inverseName = interfaceClassType.Name;
            }
            return inverseName;
        }

        public AttributeType GetAttribute(AttributeType attributeType, string attributeName)
        {
            Assert.IsNotNull(attributeType, "AttributeType is null");
            return GetAttribute(attributeType.Attribute, attributeName);
        }

        public AttributeType GetAttribute( AttributeSequence attributes, string attributeName)
        {
            Assert.IsNotNull(attributes, "AttributeType is null");
            AttributeType result = attributes[attributeName];
            Assert.IsNotNull(result, "Unable to find Attribute " + attributeName);
            return result;
        }

        public AttributeType GetStructured(TestHelper.Uris uriId, uint nodeId, string variableName)
        {
            AttributeFamilyType objectToTest = GetTestAttribute(uriId, nodeId);
            AttributeType variableAttribute = GetAttribute(objectToTest.Attribute, variableName);
            AttributeType structured = GetAttribute(variableAttribute, "StructureFieldDefinition");
            return structured;
        }

        #endregion
    }
}