using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Opc.Ua;
using System.Reflection.Metadata;
using Opc.Ua.Gds;
using System;
using System.Xml.Linq;
using System.Linq;
using Newtonsoft.Json.Linq;
using Aml.Engine.Adapter;
using static Opc.Ua.RelativePathFormatter;

namespace SystemTest
{
    [TestClass]
    public class TestComplexNonRootNamespace
    {
        #region Tests

        private CAEXDocument m_document = null;

        private const string InstanceLevel = "http://opcfoundation.org/UA/FX/AML/TESTING/InstanceLevel/";
        private const string LevelOne = "http://opcfoundation.org/UA/FX/AML/TESTING/LevelOne/";
        private const string LevelTwo = "http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/";
        private const string RootLevel = "http://opcfoundation.org/UA/";

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestLevelOne()
        {
            AttributeType value = InitialGetValueAttribute( "LevelOne" );

            #region Test Basics

            TestValue( value, "Bool", "true", "xs:boolean" );
            TestValue( value, "SByte", "2", "xs:Byte" );
            TestValue( value, "Int16", "3", "xs:short" );
            TestValue( value, "UInt16", "4", "xs:unsignedShort" );
            TestValue( value, "Int32", "5", "xs:int" );
            TestValue( value, "UInt32", "6", "xs:unsignedInt" );
            TestValue( value, "Int64", "7", "xs:long" );
            TestValue( value, "UInt64", "8", "xs:unsignedLong" );
            TestValue( value, "Float", "9.1", "xs:float" );
            TestValue( value, "Double", "10.2", "xs:double" );
            TestValue( value, "String", "Eleven point three", "xs:string" );
            TestValue( value, "DateTime", "2011-12-12T00:12:12-07:00", "xs:dateTime" );
            TestValue( value, "Guid", "13131313-1313-1313-1313-131313131313", "xs:string" );
            TestValue( value, "ByteString", "MTQxNDE0MTQ=", "xs:base64Binary" );
            
            TestValue( value, "XmlElement", 
                "<TheFifteenthElement xmlns=\"http://opcfoundation.org/UA/FX/AML/TESTING/LevelOne/Types.xsd\">Fifteen</TheFifteenthElement>",
                "xs:string" );

            ValidateNodeId( value, "NodeId", GetUri( 2 ), new NodeId( 16 ) );
            ValidateNodeId( value, "ExpandedNodeId", GetUri( 2 ), new NodeId( 17 ) );

            TestValue( value, "StatusCode", "2149056512", "xs:unsignedInt" );
            ValidateQualifiedName( value, "QualifiedName", InstanceLevel, "Nineteen" );
            ValidateLocalizedText( value, "LocalizedText", "Twenty" );

            AttributeType dataValueAttribute = GetAttribute( value, "DataValue", validateSubAttributes: true );
            TestValue( dataValueAttribute, "Value", "12345", "xs:int" );
            TestValue( dataValueAttribute, "StatusCode", "2165637120", "xs:unsignedInt" );
            TestValue( dataValueAttribute, "SourceTimestamp", "2023-09-13T14:39:08-06:00", "xs:dateTime" );
            TestValue( dataValueAttribute, "ServerTimestamp", "2023-09-13T14:39:08-06:00", "xs:dateTime" );

            AttributeType diagnosticAttribute = GetAttribute( value, "DiagnosticInfo", validateSubAttributes: true );
            TestValue( diagnosticAttribute, "SymbolicId", "11", "xs:int" );
            TestValue( diagnosticAttribute, "NamespaceUri", "22", "xs:int" );
            TestValue( diagnosticAttribute, "Locale", "33", "xs:int" );
            TestValue( diagnosticAttribute, "LocalizedText", "44", "xs:int" );
            TestValue( diagnosticAttribute, "AdditionalInfo", "Diagnostic Information", "xs:string" );
            TestValue( diagnosticAttribute, "InnerStatusCode", "2165637121", "xs:unsignedInt" );

            AttributeType innerDiagnosticAttribute = GetAttribute( diagnosticAttribute, "InnerDiagnosticInfo", validateSubAttributes: true );
            TestValue( innerDiagnosticAttribute, "SymbolicId", "12", "xs:int" );
            TestValue( innerDiagnosticAttribute, "NamespaceUri", "23", "xs:int" );
            TestValue( innerDiagnosticAttribute, "Locale", "34", "xs:int" );
            TestValue( innerDiagnosticAttribute, "LocalizedText", "45", "xs:int" );
            TestValue( innerDiagnosticAttribute, "AdditionalInfo", "Even More Diagnostic Information", "xs:string" );
            TestValue( innerDiagnosticAttribute, "InnerStatusCode", "2165637122", "xs:unsignedInt" );

            #endregion

            #region DataSet Extension

            AttributeType dataSetAttribute = GetAttribute( value, "DataSet", validateSubAttributes: true );

            #region Simple

            TestValue( dataSetAttribute, "Name", "Data Set Name", "xs:string" );
            ValidateLocalizedText( dataSetAttribute, "Description", "Data Set Description" );
            TestValue( dataSetAttribute, "DataSetClassId", "98769876-9876-9876-9876-987698769876", "xs:string" );
            AttributeType configurationVersionAttribute = GetAttribute( dataSetAttribute, "ConfigurationVersion", validateSubAttributes: true );
            TestValue( configurationVersionAttribute, "MajorVersion", "54", "xs:unsignedInt" );
            TestValue( configurationVersionAttribute, "MinorVersion", "32", "xs:unsignedInt" );

            #region Namespaces Variable

            AttributeType namespaces = GetAttribute( dataSetAttribute, "Namespaces", validateSubAttributes: true );
            Assert.AreEqual( 3, namespaces.Attribute.Count, "Invalid namespace count" );
            TestValue( namespaces, "1", GetUri( 3 ), "xs:string" );

            #endregion

            #endregion

            #region Complex

            TestDataSetComplex( value );

            //#region StructureDataTypes

            //AttributeType structureDataTypes = GetAttribute( dataSetAttribute, "StructureDataTypes", validateSubAttributes: true );
            //Assert.AreEqual( 3, structureDataTypes.Attribute.Count, "Invalid Structure count" );
            //AttributeType structureDescriptionType = GetAttribute( structureDataTypes, "1", validateSubAttributes: true );

            //ValidateNodeId( structureDescriptionType, "DataTypeId", GetUri( 2 ), new NodeId( 2223 ) );
            //ValidateQualifiedName( structureDescriptionType, "Name", GetUri( 2 ), "Structure Description Two" );

            //AttributeType structureDefinitionType = GetAttribute( structureDescriptionType, "StructureDefinition", validateSubAttributes: true );
            //ValidateNodeId( structureDefinitionType, "DefaultEncodingId", RootLevel, new NodeId( 8 ) );
            //ValidateNodeId( structureDefinitionType, "BaseDataType", RootLevel, new NodeId( 9 ) );
            //TestValue( structureDefinitionType, "StructureType", "UnionWithSubtypedValues", "xs:string" );

            //#endregion

            //#region EnumDataTypes

            //AttributeType enumDataTypes = GetAttribute( dataSetAttribute, "EnumDataTypes", validateSubAttributes: true );
            //Assert.AreEqual( 3, enumDataTypes.Attribute.Count, "Invalid Enum Structure count" );
            //AttributeType enumDataType = GetAttribute( enumDataTypes, "1", validateSubAttributes: true );

            //ValidateNodeId( enumDataType, "DataTypeId", GetUri( 3 ), new NodeId( 2346 ) );
            //ValidateQualifiedName( enumDataType, "Name", GetUri( 3 ), "Another Enum Description" );
            //TestValue( enumDataType, "BuiltInType", "Int16", "xs:string" );

            //AttributeType enumDefinitions = GetAttribute( enumDataType, "EnumDefinition", validateSubAttributes: true );
            //AttributeType enumFields = GetAttribute( enumDefinitions, "Fields", validateSubAttributes: true );
            //Assert.AreEqual( 3, enumFields.Attribute.Count, "Invalid Enum Fields count" );

            //AttributeType enumField = GetAttribute( enumFields, "1", validateSubAttributes: true );
            //TestValue( enumField, "Value", "56", "xs:long" );
            //TestValue( enumField, "Name", "Fifty Six", "xs:string" );
            //ValidateLocalizedText( enumField, "DisplayName", "Fifty Six" );
            //ValidateLocalizedText( enumField, "Description", "Fifty Six" );

            //#endregion

            //#region SimpleDataTypes

            //AttributeType simpleDataTypes = GetAttribute( dataSetAttribute, "SimpleDataTypes", validateSubAttributes: true );
            //Assert.AreEqual( 3, simpleDataTypes.Attribute.Count, "Invalid Simple Data Type count" );
            //AttributeType simpleDataType = GetAttribute( simpleDataTypes, "1", validateSubAttributes: true );

            //ValidateNodeId( simpleDataType, "DataTypeId", GetUri( 3 ), new NodeId( 891 ) );
            //ValidateQualifiedName( simpleDataType, "Name", GetUri( 1 ), "EvenMoreSimple" );
            //ValidateNodeId( simpleDataType, "BaseDataType", GetUri( 0 ), new NodeId( 8 ) );
            //TestValue( simpleDataType, "BuiltInType", "Int64", "xs:string" );

            //#endregion

            //#region Fields

            //AttributeType fields = GetAttribute( dataSetAttribute, "Fields", validateSubAttributes: true );
            //Assert.AreEqual( 3, fields.Attribute.Count, "Invalid Field count" );
            //AttributeType field = GetAttribute( fields, "1", validateSubAttributes: true );

            //TestValue( field, "Name", "Field Two", "xs:string" );
            //ValidateLocalizedText( field, "Description", "Field Two" );
            //// Unsure on what type this should actually be.  Boolean does not make sense, as there could be multiple fields
            //// This doesn't work at all.  FieldFlags is UInt16
            ////TestValue( field, "FieldFlags", "1", "xs:unsignedShort" );
            //TestValue( field, "BuiltInType", "Float", "xs:string" );
            //ValidateNodeId( field, "DataType", GetUri( 0 ), new NodeId( 10 ) );
            //TestValue( field, "ValueRank", "1", "xs:int" );
            //ValidateArrayDimensions( field, new string[] { "3" } );
            //TestValue( field, "MaxStringLength", "333", "xs:unsignedInt" );
            //TestValue( field, "DataSetFieldId", "12341234-1234-1234-1234-123412341234", "xs:string" );

            //AttributeType properties = GetAttribute( field, "Properties", validateSubAttributes: true );
            //Assert.AreEqual( 2, properties.Attribute.Count, "Invalid Properties count" );
            //AttributeType stringProperty = GetAttribute( properties, "0", validateSubAttributes: true );
            //ValidateQualifiedName( stringProperty, "Key", GetUri( 0 ), "PropertyOne" );
            //TestValue( stringProperty, "Value", "PropertyOne", "xs:string" );

            //AttributeType intProperty = GetAttribute( properties, "1", validateSubAttributes: true );
            //ValidateQualifiedName( intProperty, "Key", GetUri( 2 ), "PropertyTwo" );
            //TestValue( intProperty, "Value", "2", "xs:int" );

            //#endregion

            #endregion

            #endregion

            #region PublishedData Extension

            AttributeType publishedDataAttribute = GetAttribute( value, "PublishedData", validateSubAttributes: true );

            ValidateNodeId( publishedDataAttribute, "PublishedVariable", GetUri(1 ), new NodeId( 987 ) );
            TestValue( publishedDataAttribute, "AttributeId", "13", "xs:unsignedInt" );
            TestValue( publishedDataAttribute, "SamplingIntervalHint", "666", "xs:double" );
            TestValue( publishedDataAttribute, "DeadbandType", "5", "xs:unsignedInt" );
            TestValue( publishedDataAttribute, "DeadbandValue", "4", "xs:double" );
            TestValue( publishedDataAttribute, "IndexRange", "4:5", "xs:string" );
            TestValue( publishedDataAttribute, "SubstituteValue", "123456789", "xs:unsignedLong" );

            AttributeType metaDataAttributes = GetAttribute( publishedDataAttribute, "MetaDataProperties", validateSubAttributes: true );

            Assert.AreEqual( 3, metaDataAttributes.Attribute.Count, "Invalid Structure count" );
            ValidateQualifiedName( metaDataAttributes, "1", GetUri( 3 ), "Two" );

            #endregion
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestLevelTwo()
        {
            AttributeType value = InitialGetValueAttribute( "LevelTwo" );

            TestComprehensiveArray( value );

            return;
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestTopLevel()
        {
            AttributeType topLevel = InitialGetValueAttribute( "TopLevel" );
            AttributeType arrayAsScalar = GetAttribute( topLevel, "ArrayAsScalar", validateSubAttributes : true );

            TestComprehensiveArray( arrayAsScalar );

            AttributeType scalarAsArray = GetMiddleArrayElement( topLevel, "ScalarAsArray" );

            TestComprehensiveScalar( scalarAsArray );
        }

        private void TestComprehensiveArray( AttributeType value )
        {
            #region ComprehensiveScalarType

            AttributeType complexType = GetMiddleArrayElement( value, "LevelOne" );

            TestComprehensiveScalar( complexType );

            #endregion

            #region Other Array Values

            TestValueMidArray( value, "Int16", "1", "xs:short" );
            TestValueMidArray( value, "UInt16", "5", "xs:unsignedShort" );
            TestValueMidArray( value, "DateTime", "2008-01-01T00:00:00-07:00", "xs:dateTime" );
            TestValueMidArray( value, "Guid", "11001100-1100-1100-1100-110011001100", "xs:string" );
            TestValueMidArray( value, "ByteString", "MTQ=", "xs:base64Binary" );

            AttributeType nodeIds = GetAttribute( value, "NodeId", validateSubAttributes: true );
            ValidateNodeId( nodeIds, "1", GetUri( 2 ), new NodeId( 2 ) );

            AttributeType qualifiedNames = GetAttribute( value, "QualifiedName", validateSubAttributes: true );
            ValidateQualifiedName( qualifiedNames, "1", GetUri( 2 ), "Two" );
            AttributeType localizedTexts = GetAttribute( value, "LocalizedText", validateSubAttributes: true );
            ValidateLocalizedText( localizedTexts, "1", "Two" );

            #endregion

        }

        private void TestComprehensiveScalar( AttributeType value )
        {
            ValidateQualifiedName( value, "QualifiedName", GetUri( 0 ), "Array Element Two" );
            ValidateLocalizedText( value, "LocalizedText", "Array Element Two" );

            AttributeType dataSetType = GetAttribute( value, "DataSet", validateSubAttributes: true );

            AttributeType namespacesType = GetAttribute( dataSetType, "Namespaces", validateSubAttributes: true );
            TestValue( namespacesType, "1", "http://opcfoundation.org/UA/FX/AML/TESTING/Unavailable/4/", "xs:string" );
            TestValue( dataSetType, "DataSetClassId", "24682468-2468-2468-2468-246824682468", "xs:string" );
            AttributeType configurationVersionType = GetAttribute( dataSetType, "ConfigurationVersion", validateSubAttributes: true );
            TestValue( configurationVersionType, "MajorVersion", "3", "xs:unsignedInt" );
            TestValue( configurationVersionType, "MinorVersion", "4", "xs:unsignedInt" );

            AttributeType publishedDataType = GetAttribute( value, "PublishedData", validateSubAttributes: true );
            TestValue( publishedDataType, "SubstituteValue", "Array Element Two Substitute", "xs:string" );

            TestDataSetComplex( value );
        }

        private void TestDataSetComplex( AttributeType value )
        {
            AttributeType dataSetAttribute = GetAttribute( value, "DataSet", validateSubAttributes: true );

            #region StructureDataTypes

            AttributeType structureDataTypes = GetAttribute( dataSetAttribute, "StructureDataTypes", validateSubAttributes: true );
            Assert.AreEqual( 3, structureDataTypes.Attribute.Count, "Invalid Structure count" );
            AttributeType structureDescriptionType = GetAttribute( structureDataTypes, "1", validateSubAttributes: true );

            ValidateNodeId( structureDescriptionType, "DataTypeId", GetUri( 2 ), new NodeId( 2223 ) );
            ValidateQualifiedName( structureDescriptionType, "Name", GetUri( 2 ), "Structure Description Two" );

            AttributeType structureDefinitionType = GetAttribute( structureDescriptionType, "StructureDefinition", validateSubAttributes: true );
            ValidateNodeId( structureDefinitionType, "DefaultEncodingId", RootLevel, new NodeId( 8 ) );
            ValidateNodeId( structureDefinitionType, "BaseDataType", RootLevel, new NodeId( 9 ) );
            TestValue( structureDefinitionType, "StructureType", "UnionWithSubtypedValues", "xs:string" );

            #endregion

            #region EnumDataTypes

            AttributeType enumDataTypes = GetAttribute( dataSetAttribute, "EnumDataTypes", validateSubAttributes: true );
            Assert.AreEqual( 3, enumDataTypes.Attribute.Count, "Invalid Enum Structure count" );
            AttributeType enumDataType = GetAttribute( enumDataTypes, "1", validateSubAttributes: true );

            ValidateNodeId( enumDataType, "DataTypeId", GetUri( 3 ), new NodeId( 2346 ) );
            ValidateQualifiedName( enumDataType, "Name", GetUri( 3 ), "Another Enum Description" );
            TestValue( enumDataType, "BuiltInType", "Int16", "xs:string" );

            AttributeType enumDefinitions = GetAttribute( enumDataType, "EnumDefinition", validateSubAttributes: true );
            AttributeType enumFields = GetAttribute( enumDefinitions, "Fields", validateSubAttributes: true );
            Assert.AreEqual( 3, enumFields.Attribute.Count, "Invalid Enum Fields count" );

            AttributeType enumField = GetAttribute( enumFields, "1", validateSubAttributes: true );
            TestValue( enumField, "Value", "56", "xs:long" );
            TestValue( enumField, "Name", "Fifty Six", "xs:string" );
            ValidateLocalizedText( enumField, "DisplayName", "Fifty Six" );
            ValidateLocalizedText( enumField, "Description", "Fifty Six" );

            #endregion

            #region SimpleDataTypes

            AttributeType simpleDataTypes = GetAttribute( dataSetAttribute, "SimpleDataTypes", validateSubAttributes: true );
            Assert.AreEqual( 3, simpleDataTypes.Attribute.Count, "Invalid Simple Data Type count" );
            AttributeType simpleDataType = GetAttribute( simpleDataTypes, "1", validateSubAttributes: true );

            ValidateNodeId( simpleDataType, "DataTypeId", GetUri( 3 ), new NodeId( 891 ) );
            ValidateQualifiedName( simpleDataType, "Name", GetUri( 1 ), "EvenMoreSimple" );
            ValidateNodeId( simpleDataType, "BaseDataType", GetUri( 0 ), new NodeId( 8 ) );
            TestValue( simpleDataType, "BuiltInType", "Int64", "xs:string" );

            #endregion

            #region Fields

            AttributeType fields = GetAttribute( dataSetAttribute, "Fields", validateSubAttributes: true );
            Assert.AreEqual( 3, fields.Attribute.Count, "Invalid Field count" );
            AttributeType field = GetAttribute( fields, "1", validateSubAttributes: true );

            TestValue( field, "Name", "Field Two", "xs:string" );
            ValidateLocalizedText( field, "Description", "Field Two" );
            // Unsure on what type this should actually be.  Boolean does not make sense, as there could be multiple fields
            // This doesn't work at all.  FieldFlags is UInt16
            //TestValue( field, "FieldFlags", "1", "xs:unsignedShort" );
            TestValue( field, "BuiltInType", "Float", "xs:string" );
            ValidateNodeId( field, "DataType", GetUri( 0 ), new NodeId( 10 ) );
            TestValue( field, "ValueRank", "1", "xs:int" );
            ValidateArrayDimensions( field, new string[] { "3" } );
            TestValue( field, "MaxStringLength", "333", "xs:unsignedInt" );
            TestValue( field, "DataSetFieldId", "12341234-1234-1234-1234-123412341234", "xs:string" );

            AttributeType properties = GetAttribute( field, "Properties", validateSubAttributes: true );
            Assert.AreEqual( 2, properties.Attribute.Count, "Invalid Properties count" );
            AttributeType stringProperty = GetAttribute( properties, "0", validateSubAttributes: true );
            ValidateQualifiedName( stringProperty, "Key", GetUri( 0 ), "PropertyOne" );
            TestValue( stringProperty, "Value", "PropertyOne", "xs:string" );

            AttributeType intProperty = GetAttribute( properties, "1", validateSubAttributes: true );
            ValidateQualifiedName( intProperty, "Key", GetUri( 2 ), "PropertyTwo" );
            TestValue( intProperty, "Value", "2", "xs:int" );

            #endregion

        }

        public AttributeType GetMiddleArrayElement( AttributeType attribute, string target )
        {
            AttributeType elements = GetAttribute( attribute, target, validateSubAttributes: true );
            Assert.AreEqual( 3, elements.Attribute.Count );
            return GetAttribute( elements, "1", validateSubAttributes: true );
        }

        public void TestValueMidArray( AttributeType source, string target, string value, string type )
        {
            AttributeType attribute = GetAttribute( source, target, validateSubAttributes: false );
            Assert.IsNotNull( attribute );
            Assert.AreEqual( 3, attribute.Attribute.Count );
            TestValue( attribute, "1", value, type );
        }

        public void TestValue( AttributeType source, string target, string value, string type )
        {
            AttributeType attribute = GetAttribute( source, target, validateSubAttributes: false );
            Assert.IsNotNull( attribute );
            Assert.AreEqual( value, attribute.Value, ignoreCase: true );
            Assert.AreEqual( type, attribute.AttributeDataType );
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestFieldTargetData()
        {
            SystemUnitClassType objectToTest = GetObjectFolder();

            SystemUnitClassType variable = objectToTest.InternalElement[ "FieldTargetData" ];
            Assert.IsNotNull( variable );
            AttributeType value = GetAttribute( variable, "Value", validateSubAttributes: true );

            Assert.AreEqual( 3, value.Attribute.Count, "Invalid FieldTargetData Array count" );

            AttributeType middleArrayValue = GetAttribute( value, "1", validateSubAttributes: false );

            AttributeType id = GetAttribute( middleArrayValue, "DataSetFieldId", validateSubAttributes: false );
            Assert.AreEqual( "0f4e5db8-ae59-4950-a1cc-6e519094e606", id.Value, ignoreCase: true );
            Assert.AreEqual( "xs:string", id.AttributeDataType );

            AttributeType receiveRange = GetAttribute( middleArrayValue, "ReceiverIndexRange", validateSubAttributes: false );
            Assert.AreEqual( "3:5", receiveRange.Value);
            Assert.AreEqual( "xs:string", receiveRange.AttributeDataType );

            ValidateNodeId( middleArrayValue, "TargetNodeId", GetUri( 2 ), new NodeId( 6080 ) );

            AttributeType attributeId = GetAttribute( middleArrayValue, "AttributeId", validateSubAttributes: false );
            Assert.AreEqual( "13", attributeId.Value );
            Assert.AreEqual( "xs:unsignedInt", attributeId.AttributeDataType );

            AttributeType writeRange = GetAttribute( middleArrayValue, "WriteIndexRange", validateSubAttributes: false );
            Assert.AreEqual( "4:7", writeRange.Value );
            Assert.AreEqual( "xs:string", writeRange.AttributeDataType );

            AttributeType overrideHandling = GetAttribute( middleArrayValue, "OverrideValueHandling", validateSubAttributes: false );
            Assert.AreEqual( "LastUsableValue", overrideHandling.Value );
            Assert.AreEqual( "xs:string", overrideHandling.AttributeDataType );

            AttributeType overrideValue = GetAttribute( middleArrayValue, "OverrideValue", validateSubAttributes: false );
            
            Assert.AreEqual( "123.45", overrideValue.Value );
            // Doesn't work yet as the datatype isn't defined by spec.
            // Still an Issue
            //Assert.AreEqual( "", overrideValue.AttributeDataType );
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestPublishedVariableData()
        {
            SystemUnitClassType objectToTest = GetObjectFolder();

            SystemUnitClassType variable = objectToTest.InternalElement[ "PublishedVariableData" ];
            Assert.IsNotNull( variable );

            AttributeType value = GetAttribute( variable, "Value", validateSubAttributes: true );

            Assert.AreEqual( 5, value.Attribute.Count, "Invalid PublishedVariableData Array count" );

            AttributeType middleArrayValue = GetAttribute( value, "2", validateSubAttributes: false );
            ValidateNodeId( middleArrayValue, "PublishedVariable", GetUri( 2 ), new NodeId( 6077 ) );

            AttributeType attributeId = GetAttribute( middleArrayValue, "AttributeId", validateSubAttributes: false );
            Assert.AreEqual( "13", attributeId.Value );
            Assert.AreEqual( "xs:unsignedInt", attributeId.AttributeDataType );

            AttributeType samplingIntervalHint = GetAttribute( middleArrayValue, "SamplingIntervalHint", validateSubAttributes: false );
            Assert.AreEqual( "567.89", samplingIntervalHint.Value );
            Assert.AreEqual( "xs:double", samplingIntervalHint.AttributeDataType );

            AttributeType deadbandType = GetAttribute( middleArrayValue, "DeadbandType", validateSubAttributes: false );
            Assert.AreEqual( "23", deadbandType.Value );
            Assert.AreEqual( "xs:unsignedInt", deadbandType.AttributeDataType );

            AttributeType deadbandValue = GetAttribute( middleArrayValue, "DeadbandValue", validateSubAttributes: false );
            Assert.AreEqual( "5", deadbandValue.Value );
            Assert.AreEqual( "xs:double", deadbandValue.AttributeDataType );

            AttributeType indexRange = GetAttribute( middleArrayValue, "IndexRange", validateSubAttributes: false );
            Assert.AreEqual( "1", indexRange.Value );
            Assert.AreEqual( "xs:string", indexRange.AttributeDataType );

            AttributeType substituteValue = GetAttribute( middleArrayValue, "SubstituteValue", validateSubAttributes: false );
            Assert.AreEqual( "subme", substituteValue.Value );
            // Doesn't work yet as the datatype isn't defined by spec.
            // Still an Issue
            //Assert.AreEqual( "xs:string", substituteValue.AttributeDataType );

            AttributeType metaDataProperties = GetAttribute( middleArrayValue, "MetaDataProperties", validateSubAttributes: true );
            Assert.AreEqual( 2, metaDataProperties.Attribute.Count );

            AttributeType metaDataProperty = GetAttribute( metaDataProperties, "1", validateSubAttributes: true );
            ValidateQualifiedName( metaDataProperties, "1", RootLevel, "anothername" );
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        public void TestSimpleAbstractStructure()
        {
            AttributeType value = GetExtentionObjectValue("LevelOneStructure");
            {
                AttributeType abstractionOne = GetAttribute(value, "AbstractionOne", validateSubAttributes: false);
                AttributeType parent = GetAttribute(abstractionOne, "ParentOne", validateSubAttributes: false);
                Assert.AreEqual("101.1", parent.Value);
                Assert.AreEqual("xs:double", parent.AttributeDataType);
                AttributeType child = GetAttribute(abstractionOne, "ChildOneAMember", validateSubAttributes: false);
                Assert.AreEqual("101", child.Value);
                Assert.AreEqual("xs:int", child.AttributeDataType);
            }

            {
                AttributeType abstractionTwo = GetAttribute(value, "AbstractionTwo", validateSubAttributes: false);
                AttributeType parent = GetAttribute(abstractionTwo, "ParentTwo", validateSubAttributes: false);
                Assert.AreEqual("202", parent.Value);
                Assert.AreEqual("xs:int", parent.AttributeDataType);
                AttributeType child = GetAttribute(abstractionTwo, "ChildTwoBMember", validateSubAttributes: false);
                Assert.AreEqual("Two Hundred Two", child.Value);
                Assert.AreEqual("xs:string", child.AttributeDataType);
            }
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        public void TestComplexAbstractStructure()
        {
            AttributeType value = GetExtentionObjectValue("LevelTwoStructureArray");
            Assert.AreEqual(2, value.Attribute.Count);

            #region FirstElement - Simple Cases

            {
                AttributeType arrayElement = GetAttribute(value, "0", validateSubAttributes: false);
                Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionStructureTwo]",
                    arrayElement.RefAttributeType);
                Assert.AreEqual(2, arrayElement.Attribute.Count);

                #region An Array of AbstractionOne

                {
                    AttributeType firstArray = GetAttribute(arrayElement, "AbstractionOneArray", validateSubAttributes: false);
                    Assert.AreEqual(2, firstArray.Attribute.Count);
                    {
                        AttributeType subArrayElement = GetAttribute(firstArray, "0", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneA]",
                            subArrayElement.RefAttributeType);
                        AttributeType parent = GetAttribute(subArrayElement, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("1", parent.Value);
                        Assert.AreEqual("xs:double", parent.AttributeDataType);
                        AttributeType child = GetAttribute(subArrayElement, "ChildOneAMember", validateSubAttributes: false);
                        Assert.AreEqual("2", child.Value);
                        Assert.AreEqual("xs:int", child.AttributeDataType);
                    }
                    {
                        AttributeType subArrayElement = GetAttribute(firstArray, "1", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneB]",
                            subArrayElement.RefAttributeType);
                        AttributeType parent = GetAttribute(subArrayElement, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("3", parent.Value);
                        Assert.AreEqual("xs:double", parent.AttributeDataType);
                        AttributeType child = GetAttribute(subArrayElement, "ChildOneBMember", validateSubAttributes: false);
                        Assert.AreEqual("4", child.Value);
                        Assert.AreEqual("xs:double", child.AttributeDataType);
                    }
                }

                #endregion

                #region An Array of AbstractionTwo

                {
                    AttributeType secondArray = GetAttribute(arrayElement, "AbstractionTwoArray", validateSubAttributes: false);
                    Assert.AreEqual(2, secondArray.Attribute.Count);
                    {
                        AttributeType subArrayElement = GetAttribute(secondArray, "0", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildTwoA]",
                            subArrayElement.RefAttributeType);
                        AttributeType parent = GetAttribute(subArrayElement, "ParentTwo", validateSubAttributes: false);
                        Assert.AreEqual("5", parent.Value);
                        Assert.AreEqual("xs:int", parent.AttributeDataType);
                        AttributeType child = GetAttribute(subArrayElement, "ChildTwoAMember", validateSubAttributes: false);
                        Assert.AreEqual("6", child.Value);
                        Assert.AreEqual("xs:double", child.AttributeDataType);
                    }
                    {
                        AttributeType subArrayElement = GetAttribute(secondArray, "1", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildTwoB]",
                            subArrayElement.RefAttributeType);
                        AttributeType parent = GetAttribute(subArrayElement, "ParentTwo", validateSubAttributes: false);
                        Assert.AreEqual("7", parent.Value);
                        Assert.AreEqual("xs:int", parent.AttributeDataType);
                        AttributeType child = GetAttribute(subArrayElement, "ChildTwoBMember", validateSubAttributes: false);
                        Assert.AreEqual("Eight", child.Value);
                        Assert.AreEqual("xs:string", child.AttributeDataType);
                    }
                }

                #endregion

            }

            #endregion

            #region Second Element - Complex Nested Cases

            {
                AttributeType arrayElement = GetAttribute(value, "1", validateSubAttributes: false);
                Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionStructureTwo]",
                    arrayElement.RefAttributeType);
                Assert.AreEqual(2, arrayElement.Attribute.Count);

                #region An Array of AbstractionOne - Nested Cases

                {
                    AttributeType firstArray = GetAttribute(arrayElement, "AbstractionOneArray", validateSubAttributes: false);
                    Assert.AreEqual(4, firstArray.Attribute.Count);
                    {
                        AttributeType subArrayElement = GetAttribute(firstArray, "0", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneA]",
                            subArrayElement.RefAttributeType);
                        AttributeType parent = GetAttribute(subArrayElement, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("9", parent.Value);
                        Assert.AreEqual("xs:double", parent.AttributeDataType);
                        AttributeType child = GetAttribute(subArrayElement, "ChildOneAMember", validateSubAttributes: false);
                        Assert.AreEqual("10", child.Value);
                        Assert.AreEqual("xs:int", child.AttributeDataType);
                    }
                    {
                        AttributeType subArrayElement = GetAttribute(firstArray, "1", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneB]",
                            subArrayElement.RefAttributeType);
                        AttributeType parent = GetAttribute(subArrayElement, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("11", parent.Value);
                        Assert.AreEqual("xs:double", parent.AttributeDataType);
                        AttributeType child = GetAttribute(subArrayElement, "ChildOneBMember", validateSubAttributes: false);
                        Assert.AreEqual("12", child.Value);
                        Assert.AreEqual("xs:double", child.AttributeDataType);
                    }

                    #region ChildC re-adds AbstractionOne using AbstractionSub, adding the possibility of many levels of nesting 

                    {
                        AttributeType subArrayElement = GetAttribute(firstArray, "2", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneC]",
                            subArrayElement.RefAttributeType);
                        AttributeType parent = GetAttribute(subArrayElement, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("13", parent.Value);
                        Assert.AreEqual("xs:double", parent.AttributeDataType);
                        AttributeType child = GetAttribute(subArrayElement, "ChildOneCMember", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelOne/]/[AbstractionSub]",
                            child.RefAttributeType);
                        AttributeType subLevelOne = GetAttribute(child, "AbstractionOne", validateSubAttributes: false);
                        AttributeType subLevelOneParent = GetAttribute(subLevelOne, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("14", subLevelOneParent.Value);
                        Assert.AreEqual("xs:double", subLevelOneParent.AttributeDataType);
                        AttributeType subLevelOneChild = GetAttribute(subLevelOne, "ChildOneCMember", validateSubAttributes: false);
                        AttributeType subLevelTwo = GetAttribute(subLevelOneChild, "AbstractionOne", validateSubAttributes: false);
                        AttributeType subLevelTwoParent = GetAttribute(subLevelTwo, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("15", subLevelTwoParent.Value);
                        Assert.AreEqual("xs:double", subLevelTwoParent.AttributeDataType);
                        AttributeType subLevelTwoChild = GetAttribute(subLevelTwo, "ChildOneDMember", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelOne/]/[ListOfAbstractionOne]",
                            subLevelTwoChild.RefAttributeType);
                        Assert.AreEqual(2, subLevelTwoChild.Attribute.Count);
                        {
                            AttributeType dArrayElement = GetAttribute(subLevelTwoChild, "0", validateSubAttributes: false);
                            Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneA]",
                                dArrayElement.RefAttributeType);
                            AttributeType dParent = GetAttribute(dArrayElement, "ParentOne", validateSubAttributes: false);
                            Assert.AreEqual("16", dParent.Value);
                            Assert.AreEqual("xs:double", dParent.AttributeDataType);
                            AttributeType dChild = GetAttribute(dArrayElement, "ChildOneAMember", validateSubAttributes: false);
                            Assert.AreEqual("17", dChild.Value);
                            Assert.AreEqual("xs:int", dChild.AttributeDataType);
                        }
                        {
                            AttributeType dArrayElement = GetAttribute(subLevelTwoChild, "1", validateSubAttributes: false);
                            Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneB]",
                                dArrayElement.RefAttributeType);
                            AttributeType dParent = GetAttribute(dArrayElement, "ParentOne", validateSubAttributes: false);
                            Assert.AreEqual("18", dParent.Value);
                            Assert.AreEqual("xs:double", dParent.AttributeDataType);
                            AttributeType dChild = GetAttribute(dArrayElement, "ChildOneBMember", validateSubAttributes: false);
                            Assert.AreEqual("19", dChild.Value);
                            Assert.AreEqual("xs:double", dChild.AttributeDataType);
                        }
                    }

                    #endregion

                    #region ChildD is an array of AbstractionOne, adding the possibility of many levels of nesting 

                    {
                        AttributeType subArrayElement = GetAttribute(firstArray, "3", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneD]",
                            subArrayElement.RefAttributeType);
                        AttributeType parent = GetAttribute(subArrayElement, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("20", parent.Value);
                        Assert.AreEqual("xs:double", parent.AttributeDataType);
                        AttributeType child = GetAttribute(subArrayElement, "ChildOneDMember", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelOne/]/[ListOfAbstractionOne]",
                            child.RefAttributeType);
                        Assert.AreEqual(2, child.Attribute.Count);

                        {
                            AttributeType dElement = GetAttribute(child, "0", validateSubAttributes: false);
                            Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneA]",
                                dElement.RefAttributeType);
                            AttributeType dParent = GetAttribute(dElement, "ParentOne", validateSubAttributes: false);
                            Assert.AreEqual("21", dParent.Value);
                            Assert.AreEqual("xs:double", dParent.AttributeDataType);
                            AttributeType dChild = GetAttribute(dElement, "ChildOneAMember", validateSubAttributes: false);
                            Assert.AreEqual("22", dChild.Value);
                            Assert.AreEqual("xs:int", dChild.AttributeDataType);
                        }
                        {
                            AttributeType dElement = GetAttribute(child, "1", validateSubAttributes: false);
                            Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneB]",
                                dElement.RefAttributeType);
                            AttributeType dParent = GetAttribute(dElement, "ParentOne", validateSubAttributes: false);
                            Assert.AreEqual("23", dParent.Value);
                            Assert.AreEqual("xs:double", dParent.AttributeDataType);
                            AttributeType dChild = GetAttribute(dElement, "ChildOneBMember", validateSubAttributes: false);
                            Assert.AreEqual("24", dChild.Value);
                            Assert.AreEqual("xs:double", dChild.AttributeDataType);
                        }
                    }

                    #endregion
                }

                #endregion

                #region An Array of AbstractionTwo - Not as complicated, but has multiple instances of each derived type

                {
                    AttributeType secondArray = GetAttribute(arrayElement, "AbstractionTwoArray", validateSubAttributes: false);
                    Assert.AreEqual(4, secondArray.Attribute.Count);
                    {
                        AttributeType subArrayElement = GetAttribute(secondArray, "0", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildTwoA]",
                            subArrayElement.RefAttributeType);
                        AttributeType parent = GetAttribute(subArrayElement, "ParentTwo", validateSubAttributes: false);
                        Assert.AreEqual("25", parent.Value);
                        Assert.AreEqual("xs:int", parent.AttributeDataType);
                        AttributeType child = GetAttribute(subArrayElement, "ChildTwoAMember", validateSubAttributes: false);
                        Assert.AreEqual("26", child.Value);
                        Assert.AreEqual("xs:double", child.AttributeDataType);
                    }
                    {
                        AttributeType subArrayElement = GetAttribute(secondArray, "1", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildTwoB]",
                            subArrayElement.RefAttributeType);
                        AttributeType parent = GetAttribute(subArrayElement, "ParentTwo", validateSubAttributes: false);
                        Assert.AreEqual("27", parent.Value);
                        Assert.AreEqual("xs:int", parent.AttributeDataType);
                        AttributeType child = GetAttribute(subArrayElement, "ChildTwoBMember", validateSubAttributes: false);
                        Assert.AreEqual("Twenty Eight", child.Value);
                        Assert.AreEqual("xs:string", child.AttributeDataType);
                    }
                    {
                        AttributeType subArrayElement = GetAttribute(secondArray, "2", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildTwoA]",
                            subArrayElement.RefAttributeType);
                        AttributeType parent = GetAttribute(subArrayElement, "ParentTwo", validateSubAttributes: false);
                        Assert.AreEqual("29", parent.Value);
                        Assert.AreEqual("xs:int", parent.AttributeDataType);
                        AttributeType child = GetAttribute(subArrayElement, "ChildTwoAMember", validateSubAttributes: false);
                        Assert.AreEqual("30", child.Value);
                        Assert.AreEqual("xs:double", child.AttributeDataType);
                    }
                    {
                        AttributeType subArrayElement = GetAttribute(secondArray, "3", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildTwoB]",
                            subArrayElement.RefAttributeType);
                        AttributeType parent = GetAttribute(subArrayElement, "ParentTwo", validateSubAttributes: false);
                        Assert.AreEqual("31", parent.Value);
                        Assert.AreEqual("xs:int", parent.AttributeDataType);
                        AttributeType child = GetAttribute(subArrayElement, "ChildTwoBMember", validateSubAttributes: false);
                        Assert.AreEqual("Thirty Two", child.Value);
                        Assert.AreEqual("xs:string", child.AttributeDataType);
                    }
                }

                #endregion
            }

            #endregion
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        public void TestComplexAbstractObject()
        {
            SystemUnitClassType folder = GetExtentionObjectFolder();
            SystemUnitClassType levelOneType = GetObject(folder, "LevelOneType");
            SystemUnitClassType one = GetObject(levelOneType, "One");
            {
                AttributeType value = GetAttribute(one, "Value", validateSubAttributes: false);
                Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneA]",
                    value.RefAttributeType);
                AttributeType parent = GetAttribute(value, "ParentOne", validateSubAttributes: false);
                Assert.AreEqual("1101.1", parent.Value);
                Assert.AreEqual("xs:double", parent.AttributeDataType);
                AttributeType child = GetAttribute(value, "ChildOneAMember", validateSubAttributes: false);
                Assert.AreEqual("1101", child.Value);
                Assert.AreEqual("xs:int", child.AttributeDataType);

            }

            SystemUnitClassType two = GetObject(levelOneType, "Two");
            {
                AttributeType value = GetAttribute(two, "Value", validateSubAttributes: false);
                Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildTwoA]",
                    value.RefAttributeType);
                AttributeType parent = GetAttribute(value, "ParentTwo", validateSubAttributes: false);
                Assert.AreEqual("1201", parent.Value);
                Assert.AreEqual("xs:int", parent.AttributeDataType);
                AttributeType child = GetAttribute(value, "ChildTwoAMember", validateSubAttributes: false);
                Assert.AreEqual("1201.1", child.Value);
                Assert.AreEqual("xs:double", child.AttributeDataType);

            }

            SystemUnitClassType notInObjectDefinition = GetObject(levelOneType, "NotInObjectDefinition");
            {
                AttributeType value = GetAttribute(notInObjectDefinition, "Value", validateSubAttributes: false);
                Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneD]",
                    value.RefAttributeType);

                AttributeType parent = GetAttribute(value, "ParentOne", validateSubAttributes: false);
                Assert.AreEqual("1", parent.Value);
                Assert.AreEqual("xs:double", parent.AttributeDataType);
                AttributeType child = GetAttribute(value, "ChildOneDMember", validateSubAttributes: false);
                Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelOne/]/[ListOfAbstractionOne]",
                    child.RefAttributeType);
                Assert.AreEqual(2, child.Attribute.Count);

                #region First Array element is ChildC - Nested at least once
                {
                    AttributeType subArrayElement = GetAttribute(child, "0", validateSubAttributes: false);
                    Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneC]",
                        subArrayElement.RefAttributeType);
                    AttributeType subParent = GetAttribute(subArrayElement, "ParentOne", validateSubAttributes: false);
                    Assert.AreEqual("2", subParent.Value);
                    Assert.AreEqual("xs:double", subParent.AttributeDataType);
                    AttributeType subChild = GetAttribute(subArrayElement, "ChildOneCMember", validateSubAttributes: false);
                    Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelOne/]/[AbstractionSub]",
                        subChild.RefAttributeType);
                    AttributeType subLevelOne = GetAttribute(subChild, "AbstractionOne", validateSubAttributes: false);
                    AttributeType subLevelOneParent = GetAttribute(subLevelOne, "ParentOne", validateSubAttributes: false);
                    Assert.AreEqual("3", subLevelOneParent.Value);
                    Assert.AreEqual("xs:double", subLevelOneParent.AttributeDataType);
                    AttributeType subLevelOneChild = GetAttribute(subLevelOne, "ChildOneDMember", validateSubAttributes: false);

                    Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelOne/]/[ListOfAbstractionOne]",
                        subLevelOneChild.RefAttributeType);
                    Assert.AreEqual(2, subLevelOneChild.Attribute.Count);

                    {
                        AttributeType arrayElement = GetAttribute(subLevelOneChild, "0", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneC]",
                            arrayElement.RefAttributeType);
                        AttributeType arrayElementParent = GetAttribute(arrayElement, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("4", arrayElementParent.Value);
                        Assert.AreEqual("xs:double", arrayElementParent.AttributeDataType);
                        AttributeType arrayElementChild= GetAttribute(arrayElement, "ChildOneCMember", validateSubAttributes: false);
                        AttributeType subArrayElementChild = GetAttribute(arrayElementChild, "AbstractionOne", validateSubAttributes: false);

                        // Final Element

                        AttributeType finalParent = GetAttribute(subArrayElementChild, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("5", finalParent.Value);
                        Assert.AreEqual("xs:double", finalParent.AttributeDataType);
                        AttributeType finalChild = GetAttribute(subArrayElementChild, "ChildOneAMember", validateSubAttributes: false);
                        Assert.AreEqual("6", finalChild.Value);
                        Assert.AreEqual("xs:int", finalChild.AttributeDataType);

                    }

                    {
                        AttributeType arrayElement = GetAttribute(subLevelOneChild, "1", validateSubAttributes: false);
                        Assert.AreEqual("[ATL_http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/]/[AbstractionChildOneA]",
                            arrayElement.RefAttributeType);
                        AttributeType arrayElementParent = GetAttribute(arrayElement, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("7", arrayElementParent.Value);
                        Assert.AreEqual("xs:double", arrayElementParent.AttributeDataType);
                        AttributeType arrayElementChild = GetAttribute(arrayElement, "ChildOneAMember", validateSubAttributes: false);
                        Assert.AreEqual("8", arrayElementChild.Value);
                        Assert.AreEqual("xs:int", arrayElementChild.AttributeDataType);
                    }


                }

                #endregion

                #region Second Array Element is ChildD - Array
                {
                    AttributeType subArrayElement = GetAttribute(child, "1", validateSubAttributes: false);

                    AttributeType subParent = GetAttribute(subArrayElement, "ParentOne", validateSubAttributes: false);
                    Assert.AreEqual("9", subParent.Value);
                    Assert.AreEqual("xs:double", subParent.AttributeDataType);
                    AttributeType subChild = GetAttribute(subArrayElement, "ChildOneDMember", validateSubAttributes: false);

                    Assert.AreEqual(2, subChild.Attribute.Count);

                    {
                        AttributeType arrayElement = GetAttribute(subChild, "0", validateSubAttributes: false);
                        AttributeType arrayElementParent = GetAttribute(arrayElement, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("10", arrayElementParent.Value);
                        Assert.AreEqual("xs:double", arrayElementParent.AttributeDataType);
                        AttributeType arrayElementChild = GetAttribute(arrayElement, "ChildOneAMember", validateSubAttributes: false);
                        Assert.AreEqual("11", arrayElementChild.Value);
                        Assert.AreEqual("xs:int", arrayElementChild.AttributeDataType);
                    }

                    {
                        AttributeType arrayElement = GetAttribute(subChild, "1", validateSubAttributes: false);
                        AttributeType arrayElementParent = GetAttribute(arrayElement, "ParentOne", validateSubAttributes: false);
                        Assert.AreEqual("12", arrayElementParent.Value);
                        Assert.AreEqual("xs:double", arrayElementParent.AttributeDataType);
                        AttributeType arrayElementChild = GetAttribute(arrayElement, "ChildOneBMember", validateSubAttributes: false);
                        Assert.AreEqual("13", arrayElementChild.Value);
                        Assert.AreEqual("xs:double", arrayElementChild.AttributeDataType);
                    }

                }

                #endregion

            }



        }

        #endregion

        #region Helpers

        private CAEXDocument GetDocument()
        {
            if ( m_document == null )
            {
                m_document = TestHelper.GetReadOnlyDocument( "InstanceLevel.xml.amlx" );
            }
            Assert.IsNotNull(m_document, "Unable to retrieve Document");
            return m_document;
        }

        public AttributeType GetExtentionObjectValue( string objectName )
        {
            SystemUnitClassType extensionObjectsFolder = GetExtentionObjectFolder();
            SystemUnitClassType desiredObject = extensionObjectsFolder.InternalElement[objectName];
            Assert.IsNotNull(desiredObject, "Unable to find Extension Object");
            AttributeType value = desiredObject.Attribute["Value"];
            Assert.IsNotNull(value, "Unable to find Value Attribute");
            return value;
        }

        public SystemUnitClassType GetObject( SystemUnitClassType source, string desired)
        {
            SystemUnitClassType objectToTest = source.InternalElement[desired];
            Assert.IsNotNull(objectToTest, "Unable to find Object " + desired);
            return objectToTest;
        }


        public SystemUnitClassType GetExtentionObjectFolder()
        {
            SystemUnitClassType objectFolder = GetObjectFolder();
            SystemUnitClassType extensionObjectsFolder = objectFolder.InternalElement["ExtensionObjects"];
            Assert.IsNotNull(extensionObjectsFolder, "Unable to find Extension Object Folder");
            return extensionObjectsFolder;
        }

        public SystemUnitClassType GetObjectFolder()
        {
            CAEXDocument document = GetDocument();
            
            CAEXObject initialObject = document.FindByID( TestHelper.GetOpcRootName() + "85");
            Assert.IsNotNull(initialObject, "Unable to find Initial Object");
            SystemUnitClassType theObject = initialObject as SystemUnitClassType;
            Assert.IsNotNull(theObject, "Unable to Cast Initial Object");
            return theObject;
        }

        public AttributeType InitialGetValueAttribute(string rootVariableName)
        {
            SystemUnitClassType objectToTest = GetObjectFolder();

            SystemUnitClassType variable = objectToTest.InternalElement[ rootVariableName ];
            Assert.IsNotNull( variable );

            AttributeType value = GetAttribute( variable, "Value", validateSubAttributes: true );
            Assert.IsNotNull( value );

            return value;
        }

        public AttributeType GetAttribute( SystemUnitClassType variable, 
            string attributeName, 
            bool validateSubAttributes = true )
        {
            AttributeType attributeType = variable.Attribute[ attributeName ];
            return ValidateAttribute( variable.Name, attributeName, attributeType, validateSubAttributes );
        }

        public AttributeType GetAttribute( AttributeType attribute, 
            string attributeName,
            bool validateSubAttributes = true )
        {
            AttributeType attributeType = attribute.Attribute[ attributeName ];
            return ValidateAttribute( attribute.Name, attributeName, attributeType, validateSubAttributes );
        }

        public AttributeType ValidateAttribute( string name, 
            string attributeName, 
            AttributeType attribute, 
            bool validateSubAttributes )
        {
            Assert.IsNotNull( attribute, "Unable to find attribute " + attributeName + " in " + name );

            if( validateSubAttributes )
            {
                Assert.IsNotNull( attribute.Attribute, attributeName + " does not have any sub attributes" );
            }

            return attribute;
        }

        public string GetUri( int namespaceIndex )
        {
            string uri = "";

            switch(  namespaceIndex )
            {
                case 0:
                    uri = RootLevel;
                    break;

                case 1:
                    uri = InstanceLevel;
                    break;

                case 2:
                    uri = LevelOne;
                    break;

                case 3:
                    uri = LevelTwo;
                    break;
            }

            return uri;
        }

        public void ValidateNodeId( AttributeType attribute, string name, string uri, NodeId nodeId )
        {
            AttributeType nodeAttribute = GetAttribute( attribute, name, validateSubAttributes: true );
            AttributeType rootAttribute = GetAttribute( nodeAttribute, "RootNodeId", validateSubAttributes: true );
            AttributeType namespaceUriAttribute = GetAttribute( rootAttribute, "NamespaceUri", validateSubAttributes: false );

            Assert.AreEqual( uri, namespaceUriAttribute.Value );
            Assert.AreEqual( "xs:anyURI", namespaceUriAttribute.AttributeDataType );

            switch( nodeId.IdType )
            {
                case IdType.Numeric:
                    {
                        AttributeType id = GetAttribute( rootAttribute, "NumericId", validateSubAttributes: false );
                        Assert.AreEqual( nodeId.Identifier.ToString(), id.Value );
                        Assert.AreEqual( "xs:long", id.AttributeDataType );

                        break;
                    }

                case IdType.String:
                    {
                        AttributeType id = GetAttribute( rootAttribute, "StringId", validateSubAttributes: false );
                        Assert.AreEqual( nodeId.Identifier.ToString(), id.Value );
                        Assert.AreEqual( "xs:string", id.AttributeDataType );
                        break;
                    }

                case IdType.Guid:
                    {
                        AttributeType id = GetAttribute( rootAttribute, "GuidId", validateSubAttributes: false );
                        Assert.AreEqual( nodeId.Identifier.ToString(), id.Value );
                        Assert.AreEqual( "xs:string", id.AttributeDataType );
                        break;
                    }

                case IdType.Opaque:
                    {
                        AttributeType id = GetAttribute( rootAttribute, "OpaqueId", validateSubAttributes: false );
                        Assert.AreEqual( nodeId.Identifier.ToString(), id.Value );
                        Assert.AreEqual( "xs:base64Binary", id.AttributeDataType );
                        break;
                    }

            }
        }

        public void ValidateQualifiedName( AttributeType attribute, string name, string uri, string qualifiedName )
        {
            AttributeType qualifiedNameAttribute = GetAttribute( attribute, name, validateSubAttributes: true );
            AttributeType nameAttribute = GetAttribute( qualifiedNameAttribute, "Name", validateSubAttributes: false );
            AttributeType namespaceUriAttribute = GetAttribute( qualifiedNameAttribute, "NamespaceUri", validateSubAttributes: false );

            Assert.AreEqual( uri, namespaceUriAttribute.Value );
            Assert.AreEqual( "xs:anyURI", namespaceUriAttribute.AttributeDataType );

            Assert.AreEqual( qualifiedName, nameAttribute.Value );
            Assert.AreEqual( "xs:string", nameAttribute.AttributeDataType );
        }

        public void ValidateLocalizedText( AttributeType attribute, string name, string expected)
        {
            AttributeType localizedTextAttribute = GetAttribute( attribute, name, validateSubAttributes: false );

            Assert.AreEqual( expected, localizedTextAttribute.Value );
            Assert.AreEqual( "xs:string", localizedTextAttribute.AttributeDataType );
        }

        public void ValidateArrayDimensions( AttributeType attribute, string[] expected )
        {
            AttributeType arrayDimensions = GetAttribute( attribute, "ArrayDimensions", validateSubAttributes: false );
            if ( expected.Length > 0 )
            {
                Assert.AreEqual( expected.Length, arrayDimensions.Attribute.Count );
                for( int index = 0; index < expected.Length; index++ )
                {
                    TestValue( arrayDimensions, index.ToString(), expected[ index ], "xs:unsignedInt" );
                }
            }
        }

        #endregion
    }
}
