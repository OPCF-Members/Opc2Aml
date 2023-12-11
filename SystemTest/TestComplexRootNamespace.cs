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
    public class TestComplexRootNamespace
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
                    if (fileInfo.Name.Equals("InstanceLevel.xml.amlx"))
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


        private const string InstanceLevel = "http://opcfoundation.org/UA/FX/AML/TESTING/InstanceLevel/";
        private const string LevelOne = "http://opcfoundation.org/UA/FX/AML/TESTING/LevelOne/";
        private const string LevelTwo = "http://opcfoundation.org/UA/FX/AML/TESTING/LevelTwo/";
        private const string RootLevel = "http://opcfoundation.org/UA/";

        [TestMethod]
        public void TestDataSetMetaData()
        {
            SystemUnitClassType objectToTest = GetObjectFolder();

            SystemUnitClassType variable = objectToTest.InternalElement[ "DataSetMetaData" ];
            Assert.IsNotNull( variable );

            AttributeType value = GetAttribute( variable, "Value", validateSubAttributes: true );

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
            // Of course, this is wrong
            Assert.AreEqual( "10", builtIntype.Value );
            Assert.AreEqual( "xs:unsignedByte", builtIntype.AttributeDataType );
            // it should be 
            //Assert.AreEqual( "Float", builtIntype.Value );
            //Assert.AreEqual( "xs:string", builtIntype.AttributeDataType );


            #endregion
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

            Assert.AreEqual( expected, localizedTextAttribute.Name );
            Assert.AreEqual( "xs:string", localizedTextAttribute.AttributeDataType );
        }

        #endregion
    }
}
