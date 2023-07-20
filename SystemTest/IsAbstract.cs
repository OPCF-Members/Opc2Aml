﻿using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace SystemTest
{
    [TestClass]
    public class IsAbstract
    {
        // Test only works on Base Nodeset, Expects ns=0;
        // Test reads the nodeset file, finds everything that should be abstract,
        // Then walks the Amlx, and verifies both that the attribute is properly set, and properly not set
        const string NodeSetFile = "Opc.Ua.NodeSet2.xml";

        CAEXDocument m_document = null;
        AutomationMLContainer m_container = null;

        #region Initialize

        [TestInitialize]
        public void TestInitialize()
        {
            if( m_document == null )
            {
                foreach( FileInfo fileInfo in TestHelper.RetrieveFiles() )
                {
                    if( fileInfo.Name.Equals( NodeSetFile + ".amlx" ) )
                    {
                        m_container = new AutomationMLContainer( fileInfo.FullName,
                            System.IO.FileMode.Open, FileAccess.Read );
                        Assert.IsNotNull( m_container, "Unable to find container" );
                        CAEXDocument document = CAEXDocument.LoadFromStream( m_container.RootDocumentStream() );
                        Assert.IsNotNull( document, "Unable to find document" );
                        m_document = document;
                    }
                }
            }

            Assert.IsNotNull( m_document, "Unable to retrieve Document" );
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if( m_document != null )
            {
                m_document.Unload();
            }
            m_container.Dispose();

        }

        #endregion

        Dictionary<int, string> AbstractNodeIds = new Dictionary<int, string>();
        Dictionary<int, bool> Confirm = new Dictionary<int, bool>();
        List<string> Output = new List<string>();
        List<string> ShouldNotBeAbstract = new List<string>();
        List<string> BadId = new List<string>();

        [TestMethod]
        public void TestAllIsAbstract()
        {
            string nodeSetFile = "Opc.Ua.NodeSet2.xml";

            DirectoryInfo outputDirectoryInfo = TestHelper.GetOpc2AmlDirectory();
            FileInfo nodeSetFileInfo = new FileInfo( Path.Combine( outputDirectoryInfo.FullName, nodeSetFile ) );
            Assert.IsTrue( nodeSetFileInfo.Exists );

            #region Read nodeset

            XmlDocument doc = new XmlDocument();
            doc.Load( nodeSetFileInfo.FullName );

            List<XmlNode> abstractNodes = new List<XmlNode>();

            foreach( XmlNode child in doc.ChildNodes )
            {
                Recurse( child, abstractNodes );
            }

            foreach( XmlNode abstractNode in abstractNodes )
            {
                // Attributes can't have attributes
                if( !abstractNode.Name.Equals( "UADataType" ) )
                {
                    string browseName = getAttribute( abstractNode, "BrowseName" );
                    string nodeId = getAttribute( abstractNode, "NodeId" );
                    int numeric = ExtractNodeId( nodeId );
                    AbstractNodeIds.Add( numeric, browseName );
                    Confirm.Add( numeric, false );
                }
            }

            #endregion

            #region Walk Everything

            foreach( InstanceHierarchyType type in m_document.CAEXFile.InstanceHierarchy )
            {
                foreach( InternalElementType internalElement in type.InternalElement )
                {
                    SystemUnitClassType classType = internalElement as SystemUnitClassType;
                    if( classType != null )
                    {
                        WalkHierarchy( classType );
                    }
                }
            }

            foreach( SystemUnitClassLibType type in m_document.CAEXFile.SystemUnitClassLib )
            {
                CAEXEnumerable<CAEXBasicObject> descendants = type.Descendants() as CAEXEnumerable<CAEXBasicObject>;
                if( descendants != null )
                {
                    foreach( CAEXBasicObject descendant in descendants )
                    {
                        SystemUnitFamilyType familyType = descendant as SystemUnitFamilyType;
                        if( familyType != null )
                        {
                            SystemUnitClassType classType = familyType as SystemUnitClassType;
                            if( classType != null )
                            {
                                WalkHierarchy( classType );
                            }
                        }
                    }
                }
            }

            foreach( InterfaceClassLibType type in m_document.CAEXFile.InterfaceClassLib )
            {
                CAEXEnumerable<CAEXBasicObject> descendants = type.Descendants() as CAEXEnumerable<CAEXBasicObject>;
                if( descendants != null )
                {
                    foreach( CAEXBasicObject descendant in descendants )
                    {
                        InterfaceFamilyType interfaceType = descendant as InterfaceFamilyType;
                        if( interfaceType != null )
                        {
                            Check( interfaceType.Name, interfaceType.ID, 
                                IsInterfaceAbstract( interfaceType ) );
                        }
                    }
                }
            }

            #endregion

            foreach( KeyValuePair<int, bool> entry in Confirm )
            {
                if( !entry.Value )
                {
                    string name = AbstractNodeIds[ entry.Key ];
                    Output.Add( name + " [" + entry.Key + "] Should have been marked abstract, but was not" );
                }
            }

            #region Bugs to be logged

            if( ShouldNotBeAbstract.Count == 0 )
            {
                ShouldNotBeAbstract.Add( "No Errors found" );
            }

            FileInfo shouldNotBeAbstract = new FileInfo(
                Path.Combine( outputDirectoryInfo.FullName, "ShouldNotBeAbstract.txt" ) );

            TestHelper.WriteFile( shouldNotBeAbstract.FullName, ShouldNotBeAbstract );

            if( BadId.Count == 0 )
            {
                BadId.Add( "No Errors found" );
            }

            FileInfo stillBadIds = new FileInfo(
                Path.Combine( outputDirectoryInfo.FullName, "StillBadIds.txt" ) );

            TestHelper.WriteFile( stillBadIds.FullName, BadId );

            #endregion

            FileInfo abstractErrorsFile = new FileInfo(
                Path.Combine( outputDirectoryInfo.FullName, "AbstractErrors.txt" ) );

            if( Output.Count > 0 )
            {
                TestHelper.WriteFile( abstractErrorsFile.FullName, Output );
                Assert.Fail( "There were " + Output.Count.ToString() +
                    " errors - check AbstractErrors.txt" );
            }
            else
            {
                if ( abstractErrorsFile.Exists )
                {
                    File.Delete( abstractErrorsFile.FullName );
                }
            }
        }

        public void Recurse( XmlNode xmlNode, List<XmlNode> abstractNodes )
        {
            foreach( XmlNode child in xmlNode.ChildNodes )
            {
                Recurse( child, abstractNodes );
            }

            string isAbstract = getAttribute( xmlNode, "IsAbstract" );
            if( isAbstract.Length > 0 )
            {
                abstractNodes.Add( xmlNode );
            }
        }

        string getAttribute( XmlNode node, string attributeId )
        {
            string value = "";

            if( node.Attributes != null )
            {
                XmlAttribute idAttribute = node.Attributes[ attributeId ];
                if( idAttribute != null )
                {
                    value = idAttribute.Value;
                }
            }

            return value;
        }

        private void WalkHierarchy( SystemUnitClassType classType )
        {
            Check( classType.Name, classType.ID, IsSucAbstract( classType ) );

            foreach( InternalElementType internalElement in classType.InternalElement )
            {
                WalkHierarchy( internalElement );
            }
        }

        private bool IsSucAbstract( SystemUnitClassType classType )
        {
            AttributeType isAbstractAttribute = classType.Attribute[ "IsAbstract" ];

            return IsAttributeAbstract( isAbstractAttribute );
        }

        private bool IsInterfaceAbstract( InterfaceFamilyType interfaceType )
        {
            AttributeType isAbstractAttribute = interfaceType.Attribute[ "IsAbstract" ];

            return IsAttributeAbstract( isAbstractAttribute );
        }

        private bool IsAttributeAbstract( AttributeType attribute )
        {
            bool isAbstract = false;

            if( attribute != null )
            {
                if( attribute.Value.Equals( "true" ) )
                {
                    isAbstract = true;
                }
            }

            return isAbstract;
        }

        private void Check( string name, string id, bool isAbstract )
        {
            if( id != null )
            {
                int numeric = ExtractNodeId( id );
                if( numeric > 0 )
                {
                    if( isAbstract )
                    {
                        if( AbstractNodeIds.ContainsKey( numeric ) )
                        {
                            if( Confirm.ContainsKey( numeric ) )
                            {
                                Confirm[ numeric ] = true;
                            }
                        }
                        else
                        {
                            ShouldNotBeAbstract.Add( name + " [" + id + "] Marked Abstract when it should not be" );
                        }
                    }
                }
                else
                {
                    BadId.Add( "Unable to check " + name + " [" + id + "]" );
                }
            }
        }


        // Only works for Numeric NodeIds, expects only base Nodeset.
        public int ExtractNodeId( string id )
        {
            int numeric = -1;
            try
            {
                string decoded = WebUtility.UrlDecode( id );
                string[] parts = decoded.Split( ";" );
                string workWith = decoded;
                if( parts.Length >= 2 )
                {
                    workWith = parts.Last();
                }

                string isolate = workWith.Replace( "i=", "" );
                try
                {
                    numeric = int.Parse( isolate );
                }
                catch( Exception ex )
                {
                    //Don't care, log message elsewhere
                }
            }
            catch(Exception more)
            {
                bool wait = true;
            }

            return numeric;
        }
    }
}
