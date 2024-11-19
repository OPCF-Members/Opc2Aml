using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Aml.Engine.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

using Opc.Ua;
using System.Xml.Linq;

namespace SystemTest
{
    [TestClass]
    public class InternalElements
    {
        CAEXDocument m_document = null;

        #region Tests

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void Validation()
        {
            var lookupService = LookupService.Register();
            var service = ValidatorService.Register();

            IEnumerable<ValidationElement> issues = service.ValidateAll(GetDocument());
            if (issues != null)
            {
                Assert.AreEqual(0, issues.AliasReferenceValidationResults().Count(), "AliasReferenceValidationResults");
                Assert.AreEqual(0, issues.ExternalReferenceValidationResults().Count(), "ExternalReferenceValidationResults");
                Assert.AreEqual(0, issues.ClassPathValidationResults().Count(), "ClassPathValidationResults");
                Assert.AreEqual(0, issues.IDReferenceValidationResults().Count(), "IDReferenceValidationResults");
                Assert.AreEqual(0, issues.PathReferenceValidationResults().Count(), "PathReferenceValidationResults");
                Assert.AreEqual(0, issues.NameReferenceValidationResults().Count(), "NameReferenceValidationResults");
                Assert.AreEqual(0, issues.NameValidationResults().Count(), "NameValidationResults");
                Assert.AreEqual(0, issues.IDValidationResults().Count(), "IDValidationResults");
                Assert.AreEqual(0, issues.MetaDataValidationResults().Count(), "MetaDataValidationResults");
                Assert.AreEqual(0, issues.InvalidAutomationMLVersion().Count(), "InvalidAutomationMLVersion");
                Assert.AreEqual(0, issues.InvalidSchemaVersion().Count(), "InvalidSchemaVersion");
                Assert.AreEqual(0, issues.MissingDocumentSourceInformation().Count(), "MissingDocumentSourceInformation");
                Assert.AreEqual(0, issues.InvalidIDs().Count(), "InvalidIDs");
                Assert.AreEqual(0, issues.NotUniquePath().Count(), "NotUniquePath");
                Assert.AreEqual(0, issues.Repairable().Count(), "Repairable");
                Assert.AreEqual(0, issues.NotRepairable().Count(), "NotRepairable");
                Assert.AreEqual(0, issues.UnresolvedIDRefs().Count(), "UnresolvedIDRefs");
                Assert.AreEqual(0, issues.UnresolvedPathRefs().Count(), "UnresolvedPathRefs");
                Assert.AreEqual(0, issues.EmptyPathRefs().Count(), "EmptyPathRefs");
                Assert.AreEqual(0, issues.EmptyIDRefs().Count(), "EmptyIDRefs");
                Assert.AreEqual(0, issues.MissingIDs().Count(), "MissingIDs");
                Assert.AreEqual(0, issues.NotUniqueIDs().Count(), "NotUniqueIDs");
                Assert.AreEqual(0, issues.UnidentifiedAlias().Count(), "UnidentifiedAlias");
                Assert.AreEqual(0, issues.UselessAlias().Count(), "UselessAlias");
                Assert.AreEqual(0, issues.Count(), "TotalIssues");
            }
            else
            {
                Assert.Fail("Unable to get validation results");
            }
        }


        // Test is done on AcknowledgeableConditionType/AckedState and AlarmConditionType/SuppressedState
        // As both use TwoStateConditionType, and StateVariableType
        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow(
            Opc.Ua.Variables.AcknowledgeableConditionType_AckedState,
            Opc.Ua.Variables.AcknowledgeableConditionType_AckedState_Id,
            Opc.Ua.Variables.AcknowledgeableConditionType_AckedState_TransitionTime,
            Opc.Ua.Variables.AcknowledgeableConditionType_AckedState_TrueState,
            Opc.Ua.Variables.AcknowledgeableConditionType_AckedState_FalseState,
            "AcknowledgeableConditionType_AckedState",
            DisplayName = "AcknowledgeableConditionType")]
        [DataRow(
            Opc.Ua.Variables.AlarmConditionType_SuppressedState,
            Opc.Ua.Variables.AlarmConditionType_SuppressedState_Id,
            Opc.Ua.Variables.AlarmConditionType_SuppressedState_TransitionTime,
            Opc.Ua.Variables.AlarmConditionType_SuppressedState_TrueState,
            Opc.Ua.Variables.AlarmConditionType_SuppressedState_FalseState,
            "AlarmConditionType_SuppressedState",
            DisplayName = "AlarmConditionType")]
        public void TestDerivedTwoState(uint objectType, uint id, uint transitionTime, uint trueState, uint falseState, string prefix = "")
        {
            string root = TestHelper.GetOpcRootName();
            CAEXObject initialClass = GetDocument().FindByID(root + objectType.ToString());
            SystemUnitClassType classToTest = initialClass as SystemUnitClassType;
            Assert.IsNotNull(classToTest, "Unable to retrieve class to test");

            Dictionary<string, uint> expectedIds = new Dictionary<string, uint>();
            expectedIds.Add("Name", Opc.Ua.Variables.StateVariableType_Name);
            expectedIds.Add("Number", Opc.Ua.Variables.StateVariableType_Number);
            expectedIds.Add("EffectiveDisplayName", Opc.Ua.Variables.StateVariableType_EffectiveDisplayName);
            expectedIds.Add("EffectiveTransitionTime", Opc.Ua.Variables.TwoStateVariableType_EffectiveTransitionTime);
            expectedIds.Add("Id", id);
            expectedIds.Add("TransitionTime", transitionTime);
            expectedIds.Add("TrueState", trueState);
            expectedIds.Add("FalseState", falseState);

            foreach (KeyValuePair<string, uint> entry in expectedIds)
            {
                InternalElementType internalElement = classToTest.InternalElement[entry.Key];
                Assert.IsNotNull(internalElement, "Unable to retrieve " + entry.Key);

                string expectedId = String.Format("{0}_{1}_{2}{3}", prefix, entry.Key, root, entry.Value.ToString());

                Assert.AreEqual(expectedId, internalElement.ID, "Unexpected Id for " + entry.Key);
            }
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow(
            Opc.Ua.Variables.StateMachineType_CurrentState,
            Opc.Ua.Variables.StateVariableType_Name,
            Opc.Ua.Variables.StateVariableType_Number,
            Opc.Ua.Variables.StateVariableType_EffectiveDisplayName,
            Opc.Ua.Variables.StateMachineType_CurrentState_Id,
            "StateMachineType_CurrentState",
            false,
            DisplayName = "StateMachineType_CurrentState")]
        [DataRow(
            Opc.Ua.Variables.FiniteStateMachineType_CurrentState,
            Opc.Ua.Variables.StateVariableType_Name,
            Opc.Ua.Variables.StateVariableType_Number,
            Opc.Ua.Variables.StateVariableType_EffectiveDisplayName,
            Opc.Ua.Variables.FiniteStateMachineType_CurrentState_Id,
            "FiniteStateMachineType_CurrentState",
            false,
            DisplayName = "FiniteStateMachineType_CurrentState")]
        [DataRow(
            Opc.Ua.Variables.TemporaryFileTransferType_TransferState_Placeholder_CurrentState,
            Opc.Ua.Variables.StateVariableType_Name,
            Opc.Ua.Variables.StateVariableType_Number,
            Opc.Ua.Variables.StateVariableType_EffectiveDisplayName,
            Opc.Ua.Variables.TemporaryFileTransferType_TransferState_Placeholder_CurrentState_Id,
            "TemporaryFileTransferType_<TransferState>_CurrentState",
            true,
            DisplayName = "TemporaryFileTransferType_<TransferState>_CurrentState")]
        [DataRow(
            Opc.Ua.Variables.ExclusiveLimitAlarmType_LimitState_CurrentState,
            Opc.Ua.Variables.StateVariableType_Name,
            Opc.Ua.Variables.StateVariableType_Number,
            Opc.Ua.Variables.StateVariableType_EffectiveDisplayName,
            Opc.Ua.Variables.ExclusiveLimitAlarmType_LimitState_CurrentState_Id,
            "ExclusiveLimitAlarmType_LimitState_CurrentState",
            true,
            DisplayName = "ExclusiveLimitAlarmType_LimitState_CurrentState")]
        [DataRow(
            Opc.Ua.Variables.ProgramStateMachineType_CurrentState,
            Opc.Ua.Variables.StateVariableType_Name,
            Opc.Ua.Variables.ProgramStateMachineType_CurrentState_Number,
            Opc.Ua.Variables.StateVariableType_EffectiveDisplayName,
            Opc.Ua.Variables.ProgramStateMachineType_CurrentState_Id,
            "ProgramStateMachineType_CurrentState",
            false,
            DisplayName = "ProgramStateMachineType_CurrentState")]
        [DataRow(
            Opc.Ua.Variables.AlarmConditionType_ShelvingState_CurrentState,
            Opc.Ua.Variables.StateVariableType_Name,
            Opc.Ua.Variables.StateVariableType_Number,
            Opc.Ua.Variables.StateVariableType_EffectiveDisplayName,
            Opc.Ua.Variables.AlarmConditionType_ShelvingState_CurrentState_Id,
            "AlarmConditionType_ShelvingState_CurrentState",
            true,
            DisplayName = "AlarmConditionType_ShelvingState_CurrentState")]

        public void TestDerivedCurrentState(uint objectType,
            uint name, uint number, uint effectiveDisplayName, uint id,
            string prefix, bool usePrefixInLookup)
        {
            string root = TestHelper.GetOpcRootName();
            string lookup = root + objectType.ToString();
            if (usePrefixInLookup)
            {
                lookup = prefix + "_" + lookup;
            }

            CAEXObject initialClass = GetDocument().FindByID(lookup);
            SystemUnitClassType classToTest = initialClass as SystemUnitClassType;
            Assert.IsNotNull(classToTest, "Unable to retrieve class to test");

            Dictionary<string, uint> expectedIds = new Dictionary<string, uint>();
            expectedIds.Add("Name", name);
            expectedIds.Add("Number", number);
            expectedIds.Add("EffectiveDisplayName", effectiveDisplayName);
            expectedIds.Add("Id", id);

            foreach (KeyValuePair<string, uint> entry in expectedIds)
            {
                InternalElementType internalElement = classToTest.InternalElement[entry.Key];
                Assert.IsNotNull(internalElement, "Unable to retrieve " + entry.Key);

                string expectedId = String.Format("{0}_{1}_{2}{3}", prefix, entry.Key, root, entry.Value.ToString());

                Assert.AreEqual(expectedId, internalElement.ID, "Unexpected Id for " + entry.Key);
            }
        }

        // Objects/Server/PublishSubscribe/SecurityGroups is an instance that has been improved to have nodeIds as IDs.
        // This is an object picked semi-randomly as an Id test for InternalElements
        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestSecurityGroupInstance()
        {
            string root = TestHelper.GetOpcRootName();
            CAEXObject initialClass = GetDocument().FindByID(root +
                Opc.Ua.Objects.PublishSubscribe_SecurityGroups.ToString());
            InternalElementType classToTest = initialClass as InternalElementType;
            Assert.IsNotNull(classToTest, "Unable to retrieve class to test");

            Dictionary<string, string> expectedIds = new Dictionary<string, string>();
            expectedIds.Add("AddSecurityGroup", root + Opc.Ua.Methods.PublishSubscribe_SecurityGroups_AddSecurityGroup.ToString());
            expectedIds.Add("RemoveSecurityGroup", root + Opc.Ua.Methods.PublishSubscribe_SecurityGroups_RemoveSecurityGroup.ToString());

            foreach (KeyValuePair<string, string> entry in expectedIds)
            {
                InternalElementType internalElement = classToTest.InternalElement[entry.Key];
                Assert.IsNotNull(internalElement, "Unable to retrieve " + entry.Key);
                Assert.AreEqual(entry.Value, internalElement.ID, "Unexpected Id for " + entry.Key);
            }
        }


        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow(new string[]
        {
            "DescriptionMethod",
            "NoDescriptionMethod",
            "Name",
            "Test_MultiStateValue_No_Values_Kind",
            "Alarm",
            "AlarmSource"
        }, "5014", true, DisplayName = "OptionalAdditions Expected to be there")]
        [DataRow(new string[]
        {
            "Id"
        }, "5014", false, DisplayName = "OptionalAdditions Expected to be removed")]
        [DataRow(new string[]
        {
            "Definition",
            "EnumValues",
            "ValueAsText"
        }, "6136", true, DisplayName = "Kind Expected to be there")]
        [DataRow(new string[]
        {
            "ValuePrecision"
        }, "6136", false, DisplayName = "Kind Expected to be removed")]
        [DataRow(new string[]
        {
            "AudibleEnabled",
            "AudibleSound",
            "BaseHighLimit",
            "HighLimit",
            "HighState",
            "AckedState",
            "ActiveState",
            "AddComment"
        }, "5018", true, DisplayName = "Alarm Expected to be there")]
        [DataRow(new string[]
        {
            "BaseHighHighLimit",
            "BaseLowLimit",
            "BaseLowLowLimit",
            "HighHighLimit",
            "LowLimit",
            "LowLowLimit",
            "HighHighState",
            "LowState",
            "LowLowState",
            "LatchedState",
            "LocalTime",
            "MaxTimeShelved",
            "OutOfServiceState",
            "PlaceInService"
        }, "5018", false, DisplayName = "Alarm Expected to be removed")]
        public void TestInstanceInternalElements(string[] internalElements, string nodeId, bool expectedToBeFound)
        {
            // Test is specifically for Issue 34/35
            // https://github.com/OPCF-Members/Opc2Aml/issues/34
            // Instances should only add InternalElements with defined unique NodeIds
            // https://github.com/OPCF-Members/Opc2Aml/issues/35
            // Instances should add internal elements not defined by SystemUnitClass

            SystemUnitClassType objectToTest = GetTestObject(nodeId);
            Assert.IsNotNull(objectToTest);
            foreach (string internalElement in internalElements)
            {
                InternalElementType internalElementType = objectToTest.InternalElement[internalElement];
                if (expectedToBeFound)
                {
                    Assert.IsNotNull(internalElementType, internalElement + " internal element not found");
                }
                else
                {
                    Assert.IsNull(internalElementType, "Unexpected internal element found for " + internalElement);
                }
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

        public SystemUnitClassType GetTestObject(string nodeId)
        {
            CAEXDocument document = GetDocument();
            CAEXObject initialObject = document.FindByID(TestHelper.GetRootName() + nodeId);
            Assert.IsNotNull(initialObject, "Unable to find Initial Object");
            SystemUnitClassType theObject = initialObject as SystemUnitClassType;
            Assert.IsNotNull(theObject, "Unable to Cast Initial Object");
            return theObject;
        }

        #endregion

    }
}
