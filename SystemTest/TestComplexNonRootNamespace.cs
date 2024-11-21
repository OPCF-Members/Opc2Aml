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
            
            string unknownDataType = "";
            TestValue( value, "XmlElement", 
                "<TheFifteenthElement xmlns=\"http://opcfoundation.org/UA/FX/AML/TESTING/LevelOne/Types.xsd\">Fifteen</TheFifteenthElement>",
                unknownDataType );

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
            AttributeType namespaceUriAttribute = GetAttribute( qualifiedNameAttribute, "NamespaceURI", validateSubAttributes: false );

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
