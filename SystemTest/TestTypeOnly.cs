using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Opc.Ua;
using System.Diagnostics;
using Org.BouncyCastle.Utilities.IO;
using System.Net.NetworkInformation;


namespace SystemTest
{
    [TestClass, Timeout(TestHelper.UnitTestTimeout )]
    public class TestTypeOnly
    {
        CAEXDocument m_document = null;
        private const string TypeOnly = "OpcUa:TypeOnly";

        #region Tests

        [TestMethod]
        public void TestLibraryAttributes()
        {
            CAEXDocument document = GetDocument();

            int attributesTested = 0;
            foreach( AttributeTypeLibType libType in document.CAEXFile.AttributeTypeLib )
            {
                if( libType.Name.StartsWith( "ATL_http", StringComparison.OrdinalIgnoreCase ) )
                {
                    foreach( AttributeFamilyType attribute in libType )
                    {
                        if( attribute.Name.StartsWith( "ListOf" ) )
                        {
                            TestForNonExistingNodeId( attribute.Attribute );
                        }
                        else
                        {
                            TestForExistingNodeId( attribute.Attribute, hasAdditionalInformation: true );
                        }
                        attributesTested++;
                    }
                }
            }

            int interfacesTested = 0;
            foreach( InterfaceClassLibType libType in document.CAEXFile.InterfaceClassLib )
            {
                if( libType.Name.StartsWith( "ICL_http", StringComparison.OrdinalIgnoreCase ) )
                {
                    foreach( InterfaceFamilyType interfaceFamilyType in libType )
                    {
                        TestForExistingNodeId( interfaceFamilyType.Attribute, hasAdditionalInformation: true );
                        interfacesTested++;
                    }
                }
            }

            int roleClassesTested = 0;
            foreach( RoleClassLibType libType in document.CAEXFile.RoleClassLib )
            {
                if( libType.Name.StartsWith( "RCL_http", StringComparison.OrdinalIgnoreCase ) )
                {
                    foreach( RoleFamilyType role in libType )
                    {
                        TestForNonExistingNodeId( role.Attribute );
                        roleClassesTested++;
                    }
                }
            }

            Debug.WriteLine( $"Attributes tested: {attributesTested} Interfaces {interfacesTested} RoleClasses {roleClassesTested}" );
        }

        private int count = 0;

        [TestMethod]
        public void TestForNodeId()
        {
            CAEXDocument document = GetDocument();
            count = 0;

            foreach( SystemUnitClassLibType libType in document.CAEXFile.SystemUnitClassLib )
            {
                if( libType.Name.StartsWith( "SUC_http", StringComparison.OrdinalIgnoreCase ) )
                {
                    foreach( SystemUnitClassType systemUnitClass in libType )
                    {
                        TestSystemUnitClassType( systemUnitClass );
                    }
                }
            }
            Debug.WriteLine( $"Types: {count}" );
            count = 0;

            foreach( InstanceHierarchyType libType in document.CAEXFile.InstanceHierarchy )
            {
                foreach( SystemUnitClassType systemUnitClass in libType )
                {
                    TestSystemUnitClassType( systemUnitClass );
                }
            }
            Debug.WriteLine( $"Instances: {count}" );

        }

        private void TestSystemUnitClassType( SystemUnitClassType entity )
        {
            foreach( InternalElementType internalElement in entity.InternalElement )
            {
                TestSystemUnitClassType( internalElement );
            }

            TestForExistingNodeId( entity.Attribute, hasAdditionalInformation: false );
            TypeOnlyTest( entity.Attribute );

            foreach( AttributeType attributes in entity.Attribute )
            {
                // Expect a nodeId on the initial level.
                TestAttributesNodeId( attributes.Attribute );
            }

            TestExternalInterfaces( entity.ExternalInterface );
            count++;
        }

        private void TestExternalInterfaces( ExternalInterfaceSequence externalInterfaces )
        {
            foreach( ExternalInterfaceType externalInterface in externalInterfaces )
            {
                TestAttributesNodeId( externalInterface.Attribute );
                TypeOnlyTest( externalInterface.Attribute );
            }
        }

        private void TestAttributesNodeId( AttributeSequence attributes )
        {
            foreach( AttributeType attribute in attributes )
            {
                TestForNonExistingNodeId( attribute.Attribute );
            }
        }


        [TestMethod]
        public void TestForStructureField()
        {
            CAEXDocument document = GetDocument();
            count = 0;

            foreach( SystemUnitClassLibType libType in document.CAEXFile.SystemUnitClassLib )
            {
                if( libType.Name.StartsWith( "SUC_http", StringComparison.OrdinalIgnoreCase ) )
                {
                    foreach( SystemUnitClassType systemUnitClass in libType )
                    {
                        TestSystemUnitClassStructureField( systemUnitClass );
                    }
                }
            }
            Debug.WriteLine( $"Types: {count}" );
            count = 0;

            foreach( InstanceHierarchyType libType in document.CAEXFile.InstanceHierarchy )
            {
                foreach( SystemUnitClassType systemUnitClass in libType )
                {
                    TestSystemUnitClassStructureField( systemUnitClass );
                }
            }
            Debug.WriteLine( $"Instances: {count}" );

        }

        private void TestSystemUnitClassStructureField( SystemUnitClassType entity )
        {
            foreach( InternalElementType internalElement in entity.InternalElement )
            {
                TestSystemUnitClassStructureField( internalElement );
            }

            TypeOnlyTest( entity.Attribute );
            TestAttributesStructureFields( entity.Attribute );

            TestExternalInterfacesStructureField( entity.ExternalInterface );
            count++;
        }

        private void TestExternalInterfacesStructureField( ExternalInterfaceSequence externalInterfaces )
        {
            foreach( ExternalInterfaceType externalInterface in externalInterfaces )
            {
                TestAttributesStructureField( externalInterface.Attribute );
                TypeOnlyTest( externalInterface.Attribute );
            }
        }


        private void TestAttributesStructureField( AttributeSequence attributes)
        {
            foreach( AttributeType attribute in attributes )
            {
                TestForNonExistingNodeId( attribute.Attribute );
            }
        }

        private void TestAttributesStructureFields( AttributeSequence attributes )
        {
            foreach( AttributeType attribute in attributes )
            {
                Assert.IsNull( attribute.Attribute[ "StructureFieldDefinition" ] );

                TestAttributesStructureFields( attribute.Attribute );
            }
        }

        private void TypeOnlyTest( AttributeSequence attributes )
        {
            foreach( AttributeType attribute in attributes )
            {
                if ( attribute.AdditionalInformation != null )
                {
                    foreach( object info in attribute.AdditionalInformation )
                    {
                        if( info.GetType().Name == "String" )
                        {
                            string value = (string)info;
                            Assert.AreNotEqual( value, TypeOnly );
                        }
                    }
                }

                TypeOnlyTest( attribute.Attribute );

            }
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

        public SystemUnitClassType GetTestObject( string nodeId, bool foundationRoot = false )
        {
            CAEXDocument document = GetDocument();
            string rootName = TestHelper.GetRootName();
            if( foundationRoot )
            {
                rootName = TestHelper.GetOpcRootName();
            }
            CAEXObject initialObject = document.FindByID( rootName + nodeId );
            Assert.IsNotNull( initialObject, "Unable to find Initial Object" );
            SystemUnitClassType theObject = initialObject as SystemUnitClassType;
            Assert.IsNotNull( theObject, "Unable to Cast Initial Object" );
            return theObject;
        }

        private void TestForExistingNodeId( AttributeSequence attributes, bool hasAdditionalInformation )
        {
            AttributeType nodeId = attributes[ "NodeId" ];
            Assert.IsNotNull( nodeId );
            if( hasAdditionalInformation )
            {
                Assert.IsNotNull( nodeId.AdditionalInformation );
                bool found = false;
                foreach( object info in nodeId.AdditionalInformation )
                {
                    if( info.GetType().Name == "String" )
                    {
                        string value = (string)info;
                        if( value == TypeOnly )
                        {
                            found = true;
                            break;
                        }
                    }
                }
                Assert.IsTrue( found );
            }
            else
            {
                if( nodeId.AdditionalInformation != null )
                {
                    foreach( object info in nodeId.AdditionalInformation )
                    {
                        if( info.GetType().Name == "String" )
                        {
                            string value = (string)info;
                            Assert.AreNotEqual( value, TypeOnly );
                        }
                    }
                }
            }

            Assert.IsNull( nodeId.Attribute[ "ServerInstanceUri" ] );
            Assert.IsNull( nodeId.Attribute[ "Alias" ] );
            Assert.IsNull( nodeId.Attribute[ "BrowsePath" ] );

            AttributeType rootNodeId = nodeId.Attribute[ "RootNodeId" ];
            Assert.IsNotNull( rootNodeId );

            Assert.AreEqual( 2, rootNodeId.Attribute.Count );
            Assert.IsNotNull( rootNodeId.Attribute[ "NamespaceUri" ] );
        }

        private void TestForNonExistingNodeId( AttributeSequence attributes )
        {
            if( attributes != null )
            {
                Assert.IsNull( attributes[ "NodeId" ] );
            }
        }



        #endregion
    }
}
