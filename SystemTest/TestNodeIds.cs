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


        #region Tests
        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow( "GuidNodeIdWithActualGuidId", "0EB66E95-DCED-415F-B8EC-43ED3F0C759B", IdType.Guid )]
        [DataRow( "NumericNodeIdWithActualNumericId", "12345", IdType.Numeric )]
        [DataRow( "OpaqueNodeIdWithActualOpaqueId", "T3BhcXVlTm9kZUlk", IdType.Opaque )]
        [DataRow( "StringNodeIdWithActualStringId", "StringNodeId", IdType.String )]
        [DataRow( "ThereIsNoValue", "ThereIsNoValue", IdType.String, false )]
        public void TestNodeIdentifierTypes( string internalElementName, string nodeId, IdType idType, bool hasValue = true )
        {
            InternalElementType testInternalElement = findInternalElementByName( internalElementName );
            Assert.IsNotNull( testInternalElement, "Could not find test object" );

            var nodeIdAttribute = testInternalElement.Attribute.FirstOrDefault( childElement => childElement.Name == "NodeId" );
            Assert.IsNotNull( nodeIdAttribute, "Unable to find nodeId attribute" );
            var rootNodeIdAttribute = nodeIdAttribute.Attribute.FirstOrDefault( childElement => childElement.Name == "RootNodeId" );
            Assert.IsNotNull( rootNodeIdAttribute, "Unable to find rootNodeId attribute" );

            string idName = Enum.GetName( typeof( IdType ), idType ) + "Id";

            AttributeType specificAttribute = rootNodeIdAttribute.Attribute.FirstOrDefault( 
                childElement => childElement.Name == idName );
            Assert.IsNotNull( specificAttribute, "Unable to find " + idName + " attribute" );
            Assert.AreEqual( nodeId, specificAttribute.Value.ToString(), true );
            ValidateNodeId( rootNodeIdAttribute, idType );

            AttributeType valueAttribute = testInternalElement.Attribute[ "Value" ];
            Assert.IsNotNull( valueAttribute, "Could not find test value object" );
            AttributeType valueRootNodeId = valueAttribute.Attribute[ "RootNodeId" ];
            Assert.IsNotNull( valueRootNodeId, "Could not find test value object" );

            AttributeType specificValueAttribute = valueRootNodeId.Attribute.FirstOrDefault( 
                childElement => childElement.Name == idName );
            Assert.IsNotNull( specificValueAttribute, "Unable to find " + idName + " value attribute" );
            if( hasValue )
            {
                Assert.AreEqual( nodeId, specificValueAttribute.Value.ToString(), true );
                ValidateNodeId( valueRootNodeId, idType );
            }
            else
            {
                ValidNoNode( valueRootNodeId );
            }
        }

        private void ValidateNodeId( AttributeType rootNodeId, IdType idType )
        {
            AttributeType namespaceUri = rootNodeId.Attribute[ "NamespaceUri" ];
            Assert.IsNotNull( namespaceUri );
            Assert.IsNotNull( namespaceUri.Value );
            Assert.AreNotEqual( 0, namespaceUri.Value.Length );

            AttributeType numericId = rootNodeId.Attribute[ "NumericId" ];
            AttributeType stringId = rootNodeId.Attribute[ "StringId" ];
            AttributeType guidId = rootNodeId.Attribute[ "GuidId" ];
            AttributeType opaqueId = rootNodeId.Attribute[ "OpaqueId" ];

            switch( idType )
            {
                case IdType.Numeric:
                    Assert.IsNotNull( numericId );
                    Assert.IsNotNull( numericId.Value );
                    Assert.AreNotEqual( 0, numericId.Value.Length );
                    Assert.IsNull( stringId );
                    Assert.IsNull( guidId );
                    Assert.IsNull( opaqueId );
                    break;

                case IdType.String:
                    Assert.IsNull( numericId );
                    Assert.IsNotNull( stringId );
                    Assert.IsNotNull( stringId.Value );
                    Assert.AreNotEqual( 0, stringId.Value.Length );
                    Assert.IsNull( guidId );
                    Assert.IsNull( opaqueId );
                    break;

                case IdType.Guid:
                    Assert.IsNull( numericId );
                    Assert.IsNull( stringId );
                    Assert.IsNotNull( guidId );
                    Assert.IsNotNull( guidId.Value );
                    Assert.AreNotEqual( 0, guidId.Value.Length );
                    Assert.IsNull( opaqueId );
                    break;

                case IdType.Opaque:
                    Assert.IsNull( numericId );
                    Assert.IsNull( stringId );
                    Assert.IsNull( guidId );
                    Assert.IsNotNull( opaqueId );
                    Assert.IsNotNull( opaqueId.Value );
                    Assert.AreNotEqual( 0, opaqueId.Value.Length );
                    break;
            }
        }

        private void ValidNoNode( AttributeType rootNodeId )
        {
            ValidateAttributeEmpty( rootNodeId, "NamespaceUri" );
            ValidateAttributeEmpty( rootNodeId, "NumericId" );
            ValidateAttributeEmpty( rootNodeId, "StringId" );
            ValidateAttributeEmpty( rootNodeId, "GuidId" );
            ValidateAttributeEmpty( rootNodeId, "OpaqueId" );
        }

        private void ValidateAttributeEmpty( AttributeType source, string attributeName )
        {
            AttributeType attribute = source.Attribute[ attributeName ];
            Assert.IsNotNull( attribute );
            Assert.IsNull( attribute.Value );
        }

        #endregion

        #region Helpers

        public InternalElementType? findInternalElementByName( string internelElemantName )
        {
            foreach( var instanceHierarchy in GetDocument().CAEXFile.InstanceHierarchy )
            {
                // browse all InternalElements deep and find element with name "FxRoot"
                foreach( var internalElement in instanceHierarchy.Descendants<InternalElementType>() )
                {
                    if( internalElement.Name.Equals( internelElemantName ) ) return internalElement;
                }
            }

            return null;

        }

        private CAEXDocument GetDocument()
        {
            if( m_document == null )
            {
                m_document = TestHelper.GetReadOnlyDocument( "TestAml.xml.amlx" );
            }
            Assert.IsNotNull( m_document, "Unable to retrieve Document" );
            return m_document;
        }

        #endregion
    }
}
