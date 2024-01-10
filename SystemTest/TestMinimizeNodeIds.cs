using Aml.Engine.Adapter;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemTest
{
    [TestClass]

    public class TestMinimizeNodeIds
    {
        CAEXDocument m_document = null;

        private readonly string ReferenceType = "ATL_OpcAmlMetaModel/ExplicitNodeId";
        private readonly string[] NodeId_IdAttributeNames = { "NumericId", "StringId", "GuidId", "OpaqueId" };

        private int EmptyNamespaceValueCounter = 0;
        private int EmptyNamespaceValidCounter = 0;
        private int EmptyCounter = 0;

        private int MinimizedCounter = 0;
        private int NamespaceValueCounter = 0;
        private int NamespaceValidCounter = 0;

        List<string> NodeIdIssues = new List<string>();

        [TestCleanup]
        public void TestCleanup()
        {
            bool wait = true;
        }
            #region Tests

            [TestMethod]
        public void TestAllMinimizedNodeIds()
        {
            CAEXDocument document = GetDocument();

            List<string> output = new List<string>();

            var instanceHierarchy = m_document.CAEXFile.InstanceHierarchy;

            foreach( InstanceHierarchyType type in instanceHierarchy )
            {
                foreach( SystemUnitClassType internalElement in type )
                {
                    WalkSystemUnitClass( internalElement, output, internalElement.Name );
                }
            }

            foreach( SystemUnitClassLibType systemUnitClassLib in document.CAEXFile.SystemUnitClassLib )
            {
                foreach( SystemUnitClassType internalElement in systemUnitClassLib )
                {
                    WalkSystemUnitClass( internalElement, output, internalElement.Name );
                }
            }

            string proper = "Correct Empty count " + EmptyCounter.ToString() + 
                " Correct Minimized count " + MinimizedCounter.ToString();

            string EmptyError = string.Empty;
            string Error = string.Empty;
            if ( EmptyNamespaceValueCounter + EmptyNamespaceValidCounter > 0 )
            {
                EmptyError = "Empty Namespace Value Error " + EmptyNamespaceValueCounter.ToString() + 
                    " Valid Error " + EmptyNamespaceValidCounter.ToString();
            }
            if( NamespaceValueCounter + NamespaceValidCounter > 0 )
            {
                Error = "Namespace Value Error " + NamespaceValueCounter.ToString() +
                    " Valid Error " + NamespaceValidCounter.ToString();
            }

            output.Prepend( proper );
            if ( EmptyError != string.Empty )
            {
                output.Append( EmptyError );
            }

            if( Error != string.Empty )
            {
                output.Append( Error );
            }

            TestHelper.WriteFile( "MinimizedNodeIds.txt", output );

            Assert.AreEqual(0, EmptyNamespaceValueCounter);
            Assert.AreEqual( 0, EmptyNamespaceValidCounter );
            Assert.AreEqual( 0, NamespaceValueCounter );
            Assert.AreEqual( 0, NamespaceValidCounter );
        }

        private void WalkSystemUnitClass( SystemUnitClassType element, List<string> output, string title, int level = 0)
        {
            //output.Add( Spaces( level ) + "Starting " + title);

            foreach (  InternalElementType internalElement in element.InternalElement )
            {
                WalkSystemUnitClass( internalElement, output, title + "_" + internalElement.Name, level + 1);
            }

            WalkAttributes( element.Attribute, output, title, "Attributes" );

            foreach( ExternalInterfaceType externalInterface in element.ExternalInterface )
            {
                WalkAttributes( externalInterface.Attribute, output, title, "ExternalInterface " + externalInterface.Name );
            }
        }

        private void WalkAttributes( AttributeSequence attribute, List<string> output, string title, string attributeTitle, int level = 0 )
        {
            if( attribute != null )
            {
                foreach( AttributeType attributeType in attribute )
                {
                    WalkAttribute( attributeType, output, title, attributeTitle, level );
                }
            }
        }

        private void WalkAttribute( AttributeType attribute, List<string>output, string title, string attributeTitle, int level = 0 )
        {
            string attributeName = attributeTitle + "[" + attribute.Name + "]";
            //output.Add( Spaces( level ) + title + " : " + attributeName );

            if( attribute.RefAttributeType != null && attribute.RefAttributeType.Equals( ReferenceType ) )
            {
                ValidateExplicitNodeId( attribute, output, title, attributeName );
            }
            else
            {
                WalkAttributes( attribute.Attribute, output, title, attributeName, level + 1 );
            }
        }


        private void ValidateExplicitNodeId( AttributeType attributeType, List<string> output, string title, string attributeTitle )
        {
            string attributeName = title + ":" + attributeTitle;
            if ( attributeType.Attribute !=  null )
            {
                int validCounter = 0;
                int valueCounter = 0;
                foreach ( string idType in NodeId_IdAttributeNames )
                {
                    AttributeType id = attributeType.Attribute[ idType ];
                    if ( id != null )
                    {
                        validCounter++;
                        if ( id.Value != null && id.Value.Length > 0 )
                        {
                            valueCounter++;
                        }
                    }
                }

                string namespaceUri = GetAttributeValue( attributeType, "NamespaceUri" );

                if ( namespaceUri == string.Empty )
                {
                    // All attributes should be valid, with no value
                    if ( validCounter == NodeId_IdAttributeNames.Length )
                    {
                        if ( valueCounter == 0 )
                        {
                            EmptyCounter++;
                        }
                        else
                        {
                            output.Add( attributeName + " has no namespace, has all attributes, but " + valueCounter.ToString() + " has a value" );
                            EmptyNamespaceValueCounter++;
                        }
                    }
                    else
                    {
                        output.Add( attributeName + " has no namespace, but has  " + validCounter.ToString() + " id attributes" );
                        EmptyNamespaceValidCounter++;
                    }
                }
                else
                {
                    if( validCounter == 1 )
                    {
                        if( valueCounter == 1 )
                        {
                            MinimizedCounter++;
                        }
                        else
                        {
                            output.Add( attributeName + " has namespace, one attribute but " + 
                                valueCounter.ToString() + " value" );
                            NamespaceValueCounter++;
                        }
                    }
                    else
                    {
                        output.Add( attributeName + " has namespace, but has  " + validCounter.ToString() + " attributes" );
                        NamespaceValidCounter++;
                    }
                }
            }
            else
            {
                Assert.Fail( "ValidateExplicitNodeId Unable to get Attribute " + title + " : " + attributeTitle );
            }
        }

        private string GetAttributeValue( AttributeType attribute, string attributeTitle )
        {
            string attributeValue = string.Empty;
            if ( attribute.Attribute != null )
            {
                AttributeType value = attribute.Attribute[ attributeTitle ];
                if ( value != null && value.Value != null )
                {
                    attributeValue = value.Value;
                }
            }
            
            return attributeValue;
        }

        private string Spaces( int level )
        {
            StringBuilder spaces = new StringBuilder();
            for( int index = 0; index < level; index++ )
            {
                spaces.Append( " " );
            }

            return spaces.ToString();
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

        #endregion


    }
}
