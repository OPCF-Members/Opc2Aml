﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System;
using Opc.Ua;

namespace SystemTest
{
    [TestClass]
    public class TestEnums
    {
        CAEXDocument m_document = null;

        #region Tests

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow("5015", "TrueState", "FalseState", true, DisplayName = "Default Parameters")]
        [DataRow("5016", "OverrideTrueState", "OverrideFalseState", true, DisplayName = "Override Default Parameters")]
        [DataRow("5017", "InstanceTrueState", "InstanceFalseState", false, DisplayName = "Override Parameters")]
        public void TwoStateInstances(string nodeId, string expectedTrue, string expectedFalse, bool expectedValue)
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



        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow("6012", "", "", false, DisplayName = "Has No Values")]
        [DataRow("6013", "TrueState", "FalseState", true, DisplayName = "Has Values")]
        public void TwoStateClasses(string nodeId, string expectedTrue, string expectedFalse, bool hasValues)
        {
            CAEXDocument document = GetDocument();
            CAEXObject initialClass = document.FindByID(TestHelper.GetRootName() + nodeId);
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

            InternalElementType valueAsText = classToTest.InternalElement["ValueAsText"];
            Assert.IsNotNull(valueAsText, "Unable to Find ValueAsText Property");
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow("5005", new string[] {"ZeroValue","OneValue"}, 1, DisplayName = "Default Parameters")]
        [DataRow("5007", new string[] { "OverrideZero", "OverrideOne" }, 1, DisplayName = "Override Default Parameters")]
        [DataRow("5006", new string[] { "InstanceZero", "InstanceOne" }, 0, DisplayName = "Override Parameters")]
        public void MultiStateVariableInstances(string nodeId, string[] values, int expectedIndex)
        {
            EnumInstanceTest(nodeId, values, expectedIndex,
                "Test_MultiStateValue", "EnumValues", "ListOfEnumValueType");

        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow("6009", new string[] { }, false, DisplayName = "Has No Values")]
        [DataRow("6008", new string[] { "ZeroValue", "OneValue" }, true, DisplayName = "Has Values")]
        public void MultiStateVariableClasses(string nodeId, string[] values, bool hasValues)
        {
            EnumClassTest(nodeId, values, hasValues, "EnumValues", "ListOfEnumValueType");
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow("5008", new string[] { "ZeroState", "OneState" }, 1, DisplayName = "Default Parameters")]
        [DataRow("5009", new string[] { "OverrideZero", "OverrideOne" }, 1, DisplayName = "Override Default Parameters")]
        [DataRow("5010", new string[] { "InstanceZero", "InstanceOne" }, 0, DisplayName = "Override Parameters")]
        public void MultiStateInstances(string nodeId, string[] values, int expectedIndex)
        {
            EnumInstanceTest(nodeId, values, expectedIndex,
                "Test_MultiState_", "EnumStrings", "ListOfLocalizedText");
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow("6010", new string[] { }, false, DisplayName = "Has No Values")]
        [DataRow("6011", new string[] { "ZeroState", "OneState" }, true, DisplayName = "Has Values")]
        public void MultiStateClasses(string nodeId, string[] values, bool hasValues)
        {
            EnumClassTest(nodeId, values, hasValues, "EnumStrings", "ListOfLocalizedText");
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]

        [DataRow( Opc.Ua.DataTypes.NodeClass, new NodeClass(), DisplayName = "EnumValue Constraints - NodeClass" )]
        [DataRow( Opc.Ua.DataTypes.MessageSecurityMode, new MessageSecurityMode(), DisplayName = "EnumString Constraints - MessageSecurityMode" )]

        public void TestConstraints( uint nodeId, object enumObject )
        {
            int[] enumValuesArray = (int[])Enum.GetValues( enumObject.GetType() );
            string[] enumStringsArray = (string[])Enum.GetNames( enumObject.GetType() );

            List<int> enumValues = enumValuesArray.ToList<int>();
            List<string> values = enumStringsArray.ToList<string>();

            Assert.AreEqual( enumValues.Count, values.Count );

            AttributeFamilyType objectToTest = GetTestAttribute( nodeId.ToString() );
            Assert.IsNotNull( objectToTest );
            Assert.AreEqual( "xs:string", objectToTest.AttributeDataType );


            AttributeValueRequirementType enumStringsConstraint = null;

            GetConstraints( objectToTest, out enumStringsConstraint );

            for( int index = 0; index < enumValues.Count; index++ )
            {
                Assert.AreEqual( values[ index ], enumStringsConstraint.NominalScaledType.ValueAttributes[ index ].Value );
            }
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]

        [DataRow(Opc.Ua.DataTypes.NodeClass, new NodeClass(), DisplayName = "NodeClass")]
        [DataRow(Opc.Ua.DataTypes.NegotiationStatus, new NegotiationStatus(), DisplayName = "NegotiationStatus")]
        [DataRow(Opc.Ua.DataTypes.MessageSecurityMode, new MessageSecurityMode(), DisplayName = "MessageSecurityMode")]

        public void TestFieldDefinition(uint nodeId, object enumObject)
        {
            int[] enumValuesArray = (int[])Enum.GetValues(enumObject.GetType());
            string[] enumStringsArray = (string[])Enum.GetNames(enumObject.GetType());

            List<int> enumValues = enumValuesArray.ToList<int>();
            List<string> values = enumStringsArray.ToList<string>();

            Assert.AreEqual(enumValues.Count, values.Count);

            AttributeFamilyType objectToTest = GetTestAttribute(nodeId.ToString());
            Assert.AreEqual("xs:string", objectToTest.AttributeDataType);

            // Build Field Definitions
            AttributeType enumFieldDefinition = objectToTest.Attribute["EnumFieldDefinition"];
            Assert.IsNotNull(enumFieldDefinition, "Unable to retrieve EnumFieldDefinition");
            Dictionary <string, int> fieldDefinitions = new Dictionary<string, int>();
            Assert.IsNotNull(enumFieldDefinition.AdditionalInformation);
            Assert.AreEqual(1, enumFieldDefinition.AdditionalInformation.Count);
            Assert.AreEqual("OpcUa:TypeOnly", enumFieldDefinition.AdditionalInformation[0]);

            foreach (AttributeType fieldDefinition in enumFieldDefinition.Attribute)
            {
                AttributeType nameAttribute = fieldDefinition.Attribute["Name"];
                Assert.IsNotNull(nameAttribute, "Field Definition Name is null");
                Assert.IsNotNull(nameAttribute.Value, "Field Definition Name Value is null");
                Assert.AreEqual(fieldDefinition.Name, nameAttribute.Value, "Field Definition Name does not match Attribute Name");
                AttributeType valueAttribute = fieldDefinition.Attribute["Value"];
                Assert.IsNotNull(valueAttribute, "Field Definition Value is null");
                Assert.IsNotNull(valueAttribute.Value, "Field Definition Value-Value is null");
                int fieldValue = int.Parse(valueAttribute.Value);
                fieldDefinitions.Add(fieldDefinition.Name, fieldValue);
            }

            AttributeValueRequirementType enumStringsConstraint = null;

            GetConstraints(objectToTest, out enumStringsConstraint);

            for (int index = 0; index < enumValues.Count; index++)
            {
                Assert.AreEqual(true, fieldDefinitions.ContainsKey(values[index]));
                Assert.AreEqual(enumValues[index], fieldDefinitions[values[index]]);
            }
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]

        [DataRow("NodeClassValue", "ObjectType", DisplayName = "NodeClass")]
        [DataRow("NegotiationStatusValue", "Failed", DisplayName = "NegotiationStatus")]

        public void TestEnumInstance(string elementName, string value)
        {
            // Strange implementation, but works with current test infrastructure
            InternalElementType objectToTest = GetObjectToTest("5001", elementName);
            AttributeType valueAttribute = objectToTest.Attribute["Value"];
            Assert.IsNotNull(valueAttribute, "Unable to retrieve Value Attribute");
            Assert.IsNotNull(valueAttribute.Value, "Value Attribute Value is null");
            Assert.AreEqual( value, valueAttribute.Value, "Unexpected Value for " + elementName);

            AttributeType fieldDefinition = valueAttribute.Attribute["EnumFieldDefinition"];
            Assert.IsNull(fieldDefinition, "Unexpected EnumFieldDefinition");
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]

        public void TestEnumStructure()
        {
            CAEXDocument doc = TestHelper.GetReadOnlyDocument("InstanceLevel.xml.amlx");
            string enumerationStructureId = TestHelper.BuildAmlId("", TestHelper.Uris.InstanceLevel, "6036");
            CAEXObject initialObject = doc.FindByID(enumerationStructureId);
            InternalElementType internalElement = initialObject as InternalElementType;
            Assert.IsNotNull(internalElement, "Unable to find Initial Object");

            // Must have correct values, and no EnumFieldDefinition
            AttributeType valueAttribute = internalElement.Attribute["Value"];
            Assert.IsNotNull(valueAttribute, "Unable to retrieve Value Attribute");
            Assert.IsNull(valueAttribute.Attribute["EnumFieldDefinition"], "Unexpected EnumFieldDefinition");
            // 
            AttributeType firstValue = valueAttribute.Attribute["One"];
            Assert.IsNotNull(firstValue, "Unable to retrieve First Value Attribute");
            Assert.IsNotNull(firstValue.Value, "First Value Attribute Value is null");
            Assert.AreEqual("ReferenceType", firstValue.Value, "Unexpected Value for First Value Attribute");

            AttributeType secondValue = valueAttribute.Attribute["Two"];
            Assert.IsNotNull(secondValue, "Unable to retrieve Second Value Attribute");
            {
                AttributeType element = secondValue.Attribute["0"];
                Assert.IsNotNull(element, "Unable to retrieve First Element Attribute");
                Assert.IsNotNull(element.Value, "First Element Attribute Value is null");
                Assert.AreEqual("Failed", element.Value, "Unexpected Value for First Element");
            }
            {
                AttributeType element = secondValue.Attribute["1"];
                Assert.IsNotNull(element, "Unable to retrieve Second Element Attribute");
                Assert.IsNotNull(element.Value, "Second Element Attribute Value is null");
                Assert.AreEqual("Complete", element.Value, "Unexpected Value for Second Element");
            }
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]

        public void TestEnumObject()
        {
            CAEXDocument doc = TestHelper.GetReadOnlyDocument("InstanceLevel.xml.amlx");
            string enumerationObjectId = TestHelper.BuildAmlId("", TestHelper.Uris.InstanceLevel, "5009");
            CAEXObject initialObject = doc.FindByID(enumerationObjectId);
            InternalElementType internalElement = initialObject as InternalElementType;
            Assert.IsNotNull(internalElement, "Unable to find Initial Object");

            {
                InternalElementType testObject = internalElement.InternalElement["One"];
                AttributeType valueAttribute = testObject.Attribute["Value"];
                Assert.IsNotNull(valueAttribute, "Unable to first Value Attribute");
                Assert.IsNull(valueAttribute.Attribute["EnumFieldDefinition"], "Unexpected EnumFieldDefinition");
                Assert.IsNotNull(valueAttribute.Value, "Unable to retrieve first Value-Value Attribute");
                Assert.AreEqual("Failed", valueAttribute.Value, "Unexpected Value for First Value Attribute");
            }
            {
                InternalElementType testObject = internalElement.InternalElement["Two"];
                AttributeType valueAttribute = testObject.Attribute["Value"];
                Assert.IsNotNull(valueAttribute, "Unable to first Value Attribute");
                Assert.IsNull(valueAttribute.Attribute["EnumFieldDefinition"], "Unexpected EnumFieldDefinition");
                {
                    AttributeType element = valueAttribute.Attribute["0"];
                    Assert.IsNotNull(element, "Unable to retrieve First Element Attribute");
                    Assert.IsNotNull(element.Value, "First Element Attribute Value is null");
                    Assert.AreEqual("Variable", element.Value, "Unexpected Value for First Element");
                }
                {
                    AttributeType element = valueAttribute.Attribute["1"];
                    Assert.IsNotNull(element, "Unable to retrieve Second Element Attribute");
                    Assert.IsNotNull(element.Value, "Second Element Attribute Value is null");
                    Assert.AreEqual("ObjectType", element.Value, "Unexpected Value for Second Element");
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
            CAEXObject initialObject = document.FindByID(TestHelper.GetRootName() + nodeId);
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

        public void EnumInstanceTest(string nodeId, string[] values, int expectedIndex,
            string objectLookup, string attributeLookup, string expectedAttributeReferenceName )
        {
            InternalElementType objectToTest = GetObjectToTest(nodeId, objectLookup);

            AttributeType boolValue = objectToTest.Attribute["Value"];
            Assert.IsNotNull(boolValue, "Unable to retrieve boolean value");

            string expectedBooleanValue = expectedIndex.ToString();
            Assert.AreEqual(expectedBooleanValue, boolValue.Value);

            InternalElementType enumElement = objectToTest.InternalElement[attributeLookup];

            Assert.IsNotNull(enumElement, "Unable to Find " + attributeLookup + " Property");

            AttributeType enumAttribute = enumElement.Attribute["Value"];
            Assert.IsNotNull(enumAttribute, "Unable to Find " + attributeLookup + " Attribute");
            Assert.IsNotNull(enumAttribute.AttributeTypeReference, 
                "Unable to Find AttributeTypeReference");
            Assert.IsNotNull(enumAttribute.AttributeTypeReference.Name, 
                "Unable to Find AttributeTypeReference Name");
            Assert.AreEqual(expectedAttributeReferenceName, enumAttribute.AttributeTypeReference.Name, 
                "Unexpected AttributeTypeReference Name");

            Assert.IsNotNull(enumAttribute.Constraint, "Unable to Find Constraint");
            Assert.AreEqual(1, enumAttribute.Constraint.Count, "Unexpected Constraint Count");

            AttributeValueRequirementType enumStringConstraint = null;

            GetConstraints( enumAttribute, out enumStringConstraint );

            AttributeValueRequirementType requirementType = enumStringConstraint;

            Assert.IsNotNull(requirementType, "Unable to Find Constraint RequirementType");
            Assert.IsNotNull(requirementType.NominalScaledType, "Unable to find Scaled Type");
            List<CaexValue> valueAttributes = requirementType.NominalScaledType.ValueAttributes;
            Assert.IsNotNull(valueAttributes, "Unable to find value attributes");
            Assert.AreEqual(values.Length, valueAttributes.Count, "Unexpected Constraint list Count");
            Assert.IsTrue(values.Length > expectedIndex, "Unexpected length for index");

            for (int index = 0; index < values.Length; index++)
            {
                CaexValue attributeValue = valueAttributes[index];
                string attributeString = attributeValue.Value as string;
                Assert.IsNotNull(attributeString, "Unable to find attribute string");
                string expectedValue = values[index];
                Assert.AreEqual(expectedValue, attributeString, "Unexpected attribute value");
            }

            string valueAsTextValue = values[expectedIndex];
            InternalElementType valueAsText = objectToTest.InternalElement["ValueAsText"];
            Assert.IsNotNull(valueAsText, "Unable to Find ValueAsText Property");
            AttributeType valueAsTextAttribute = valueAsText.Attribute["Value"];
            Assert.IsNotNull(valueAsTextAttribute, "Unable to Find ValueAsText Value Attribute");
            Assert.AreEqual(valueAsTextValue, valueAsTextAttribute.Value, "Unexpected Value as Text");

        }

        public void EnumClassTest(string nodeId, string[] values, bool hasValues,
            string attributeLookup, string expectedAttributeReferenceName)
        {
            CAEXDocument document = GetDocument();
            CAEXObject initialClass = document.FindByID(TestHelper.GetRootName() + nodeId);
            InternalElementType classToTest = initialClass as InternalElementType;
            Assert.IsNotNull(classToTest, "Unable to retrieve class to test");

            InternalElementType enumElement = classToTest.InternalElement[attributeLookup];
            Assert.IsNotNull(enumElement, "Unable to Find " + attributeLookup + " Property"); 

            AttributeType enumAttribute = enumElement.Attribute["Value"];
            Assert.IsNotNull(enumAttribute, "Unable to Find " + attributeLookup + " Attribute");
            Assert.IsNotNull(enumAttribute.AttributeTypeReference,
                "Unable to Find AttributeTypeReference");
            Assert.IsNotNull(enumAttribute.AttributeTypeReference.Name,
                "Unable to Find AttributeTypeReference Name");
            Assert.AreEqual(expectedAttributeReferenceName, enumAttribute.AttributeTypeReference.Name,
                "Unexpected AttributeTypeReference Name");

            Assert.IsNotNull(enumAttribute.Constraint, "Unable to Find Constraint");
            if ( hasValues)
            {
                Assert.AreEqual(1, enumAttribute.Constraint.Count, "Unexpected Constraint Count");

                AttributeValueRequirementType enumStringConstraint = null;

                GetConstraints( enumAttribute, out enumStringConstraint );

                AttributeValueRequirementType requirementType = enumStringConstraint;

                Assert.IsNotNull(requirementType, "Unable to Find Constraint RequirementType");
                Assert.IsNotNull(requirementType.NominalScaledType, "Unable to find Scaled Type");
                List<CaexValue> valueAttributes = requirementType.NominalScaledType.ValueAttributes;
                Assert.IsNotNull(valueAttributes, "Unable to find value attributes");
                Assert.AreEqual(values.Length, valueAttributes.Count, "Unexpected Constraint list Count");

                for (int index = 0; index < values.Length; index++)
                {
                    CaexValue attributeValue = valueAttributes[index];
                    string attributeString = attributeValue.Value as string;
                    Assert.IsNotNull(attributeString, "Unable to find attribute string");
                    string expectedValue = values[index];
                    Assert.AreEqual(expectedValue, attributeString, "Unexpected attribute value");
                }
            }
            else
            {
                Assert.AreEqual(0, enumAttribute.Constraint.Count, "Unexpected Constraint Count");
            }

            InternalElementType valueAsText = classToTest.InternalElement["ValueAsText"];
            Assert.IsNotNull(valueAsText, "Unable to Find ValueAsText Property");
        }

        public AttributeFamilyType GetTestAttribute( string nodeId )
        {
            CAEXDocument document = GetDocument();
            string rootName = TestHelper.GetOpcRootName();
            string desiredName = rootName + nodeId;
            Console.WriteLine( "Looking for " + rootName + nodeId );
            CAEXObject initialObject = document.FindByID( rootName + nodeId );
            if( initialObject == null )
            {
                Console.WriteLine( "Cant find, use back for " + desiredName );

                AttributeTypeLibType archie = document.CAEXFile.AttributeTypeLib[ "ATL_http://opcfoundation.org/UA/" ];

                if( archie != null )
                {
                    foreach( AttributeFamilyType familyType in archie )
                    {
                        if( familyType.ID.Equals( desiredName, StringComparison.OrdinalIgnoreCase ) )
                        {
                            initialObject = familyType;
                            break;
                        }
                    }
                }
            }

            Assert.IsNotNull( initialObject, "Unable to find Initial Object" );
            AttributeFamilyType theObject = initialObject as AttributeFamilyType;
            Assert.IsNotNull( theObject, "Unable to Cast Initial Object" );
            return theObject;
        }

        public void GetConstraints( AttributeTypeType attributeType, 
            out AttributeValueRequirementType stringValues )
        {
            AttributeValueRequirementType enumStringsConstraint = null;

            foreach( AttributeValueRequirementType valueRequirementType in attributeType.Constraint )
            {
                if( valueRequirementType.Name.EndsWith( "Constraint" ) )
                {
                    enumStringsConstraint = valueRequirementType;
                }
            }

            Assert.IsNotNull( enumStringsConstraint );
            Assert.IsNotNull( enumStringsConstraint.NominalScaledType );
            Assert.IsNotNull( enumStringsConstraint.NominalScaledType.ValueAttributes );

            stringValues = enumStringsConstraint;
        }


        #endregion
    }
}