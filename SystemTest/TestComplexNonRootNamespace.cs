using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Opc.Ua;
using System.Reflection.Metadata;
using Opc.Ua.Gds;
using System;

namespace SystemTest
{
    [TestClass]
    public class TestComplexNonRootNamespace
    {

        #region Initialize


        #endregion


        #region Tests

        private CAEXDocument m_document = null;

        private const string InstanceLevel = "http://opcfoundation.org/UA/FX/AML/TESTING/InstanceLevel/";
        private const string LevelOne = "http://opcfoundation.org/UA/FX/AML/TESTING/LevelOne/";
        private const string LevelTwo = "http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/";
        private const string RootLevel = "http://opcfoundation.org/UA/";

        [TestMethod]
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

            ValidateNodeId( value, "NodeId", LevelOne, new NodeId( 16 ) );
            ValidateNodeId( value, "ExpandedNodeId", LevelOne, new NodeId( 17 ) );

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



            return;

            //ValidateNodeId( structureDataType, "DataTypeId", InstanceLevel, new NodeId( 99 ) );


            #endregion


            #region DataSet Extension

            #endregion

            #region PublishedData Extension

            #endregion

            #region Namespaces Variable

            AttributeType namespaces = GetAttribute( value, "Namespaces", validateSubAttributes: true );
            Assert.AreEqual( 3, namespaces.Attribute.Count, "Invalid namespace count" );
            AttributeType namespaceAttribute = GetAttribute( namespaces, "0", validateSubAttributes: false );
            Assert.AreEqual( LevelOne, namespaceAttribute.Value );
            Assert.AreEqual( "xs:string", namespaceAttribute.AttributeDataType );
            namespaceAttribute = GetAttribute( namespaces, "1", validateSubAttributes: false );
            Assert.AreEqual( LevelTwo, namespaceAttribute.Value );
            Assert.AreEqual( "xs:string", namespaceAttribute.AttributeDataType );
            namespaceAttribute = GetAttribute( namespaces, "2", validateSubAttributes: false );
            Assert.AreEqual( InstanceLevel, namespaceAttribute.Value );
            Assert.AreEqual( "xs:string", namespaceAttribute.AttributeDataType );

            #endregion

            #region Structure Data Type

            AttributeType structureDataTypes = GetAttribute( value, "StructureDataTypes", validateSubAttributes: true );
            Assert.AreEqual( 1, structureDataTypes.Attribute.Count, "Invalid namespace count" );
            AttributeType structureDataType = GetAttribute( structureDataTypes, "0", validateSubAttributes: true );

            ValidateNodeId( structureDataType, "DataTypeId", InstanceLevel, new NodeId( 99 ) );
            ValidateQualifiedName( structureDataType, "Name", RootLevel, "Structure Description One" );

            AttributeType structureDefinition = GetAttribute( structureDataType, "StructureDefinition", validateSubAttributes: true );
            ValidateNodeId( structureDefinition, "DefaultEncodingId", LevelOne, new NodeId( "EncodingOne" ) );
            ValidateNodeId( structureDefinition, "BaseDataType", LevelOne, new NodeId( "BaseDataTypeOne" ) );

            AttributeType structureType = GetAttribute( structureDefinition, "StructureType", validateSubAttributes: false );
            Assert.AreEqual( "Structure", structureType.Value );
            Assert.AreEqual( "xs:string", structureType.AttributeDataType );

            AttributeType structureFields = GetAttribute( structureDefinition, "Fields", validateSubAttributes: true );
            Assert.AreEqual( 2, structureFields.Attribute.Count, "Invalid namespace count" );

            AttributeType testOneStructureField = GetAttribute( structureFields, "1", validateSubAttributes: true );
            AttributeType structureFieldName = GetAttribute( testOneStructureField, "Name", validateSubAttributes: false );
            Assert.AreEqual( "StructureFieldTwo", structureFieldName.Value );

            AttributeType structureFieldDescription = GetAttribute( testOneStructureField, "Description", validateSubAttributes: false );
            Assert.AreEqual( "Structure Field Two Description", structureFieldDescription.Value );

            ValidateNodeId( testOneStructureField, "DataType", RootLevel, Opc.Ua.DataTypeIds.String );

            #endregion

            #region Fields

            AttributeType fields = GetAttribute( value, "Fields", validateSubAttributes: true );
            Assert.AreEqual( 4, fields.Attribute.Count, "Invalid Fields count" );

            AttributeType field = GetAttribute( fields, "2", validateSubAttributes: true );

            AttributeType fieldName = GetAttribute( field, "Name", validateSubAttributes: false );
            Assert.AreEqual( "FloatArray", fieldName.Value );

            AttributeType fieldDescription = GetAttribute( field, "Description", validateSubAttributes: false );
            Assert.AreEqual( "FieldMetaData Description for FloatArray", fieldDescription.Value );

            ValidateNodeId( field, "DataType", RootLevel, Opc.Ua.DataTypeIds.Float );

            AttributeType valueRank = GetAttribute( field, "ValueRank", validateSubAttributes: false );
            Assert.AreEqual( "1", valueRank.Value );

            AttributeType arrayDimensions = GetAttribute( field, "ArrayDimensions", validateSubAttributes: true );
            AttributeType arrayDimension = GetAttribute( arrayDimensions, "0", validateSubAttributes: false );
            Assert.AreEqual( "5", arrayDimension.Value );

            AttributeType builtIntype = GetAttribute( field, "BuiltInType", validateSubAttributes: false );
            Assert.AreEqual( "Float", builtIntype.Value );
            Assert.AreEqual( "xs:string", builtIntype.AttributeDataType );

            #endregion
        }

        public void TestValue( AttributeType source, string target, string value, string type )
        {
            AttributeType attribute = GetAttribute( source, target, validateSubAttributes: false );
            Assert.IsNotNull( attribute );
            Assert.AreEqual( value, attribute.Value, ignoreCase: true );
            Assert.AreEqual( type, attribute.AttributeDataType );
        }

        [TestMethod]
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

            ValidateNodeId( middleArrayValue, "TargetNodeId", LevelOne, new NodeId( 6080 ) );

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

        [TestMethod]
        public void TestPublishedVariableData()
        {
            SystemUnitClassType objectToTest = GetObjectFolder();

            SystemUnitClassType variable = objectToTest.InternalElement[ "PublishedVariableData" ];
            Assert.IsNotNull( variable );

            AttributeType value = GetAttribute( variable, "Value", validateSubAttributes: true );

            Assert.AreEqual( 5, value.Attribute.Count, "Invalid PublishedVariableData Array count" );

            AttributeType middleArrayValue = GetAttribute( value, "2", validateSubAttributes: false );
            ValidateNodeId( middleArrayValue, "PublishedVariable", LevelOne, new NodeId( 6077 ) );

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

        #endregion
    }
}
