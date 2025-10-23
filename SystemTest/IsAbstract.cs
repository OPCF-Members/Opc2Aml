using Aml.Engine.AmlObjects;
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
        const string NodeSetFile = "Modified.Opc.Ua.NodeSet2.xml";
        const string NodeSetFileContainer = NodeSetFile + ".amlx";

        CAEXDocument m_document = null;

        Dictionary<int, string> AbstractNodeIds = new Dictionary<int, string>();
        Dictionary<int, bool> Confirm = new Dictionary<int, bool>();
        List<string> ShouldBeAbstract = new List<string>();
        List<string> ShouldNotBeAbstract = new List<string>();
        List<string> BadId = new List<string>();

        enum CheckType
        {
            SucType,
            InstanceType,
            InterfaceType,
            ExternalInterfaceType
        };

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestAllIsAbstract()
        {
            DirectoryInfo outputDirectoryInfo = TestHelper.GetOpc2AmlDirectory();
            FileInfo nodeSetFileInfo = new FileInfo( Path.Combine( outputDirectoryInfo.FullName, NodeSetFile ) );
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

            foreach( InstanceHierarchyType type in GetDocument().CAEXFile.InstanceHierarchy )
            {
                foreach( InternalElementType internalElement in type.InternalElement )
                {
                    SystemUnitClassType classType = internalElement as SystemUnitClassType;
                    if( classType != null )
                    {
                        WalkHierarchy( classType, instances: true );
                    }
                }
            }

            foreach( SystemUnitClassLibType type in GetDocument().CAEXFile.SystemUnitClassLib )
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
                                WalkHierarchy( classType, instances: false );
                            }
                        }
                    }
                }
            }

            foreach( InterfaceClassLibType type in GetDocument().CAEXFile.InterfaceClassLib )
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
                                IsInterfaceAbstract( interfaceType ), CheckType.InterfaceType );
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
                    ShouldBeAbstract.Add( name + " [" + entry.Key + 
                        "] Should have been marked abstract, but was not" );
                }
            }

            // Make sure all files are written before checks are done

            int shouldNotBeAbstractCount = ShouldNotBeAbstract.Count;
            int badIdCount = BadId.Count;
            int shouldBeAbstractCount = ShouldBeAbstract.Count;

            WriteTestFile( outputDirectoryInfo, "ShouldNotBeAbstract.txt", ShouldNotBeAbstract );
            WriteTestFile( outputDirectoryInfo, "BadIds.txt", BadId );
            WriteTestFile( outputDirectoryInfo, "ShouldBeAbstract.txt", ShouldBeAbstract );

            Assert.AreEqual( 0, shouldBeAbstractCount, "There were " + shouldBeAbstractCount.ToString() +
                " shouldBeAbstract errors - check ShouldBeAbstract.txt" );
            Assert.AreEqual( 0, badIdCount, "There were " + badIdCount.ToString() +
                " bad Id errors - check StillBadIds.txt" );
            Assert.AreEqual( 0, shouldNotBeAbstractCount, "There were " + shouldNotBeAbstractCount.ToString() +
                " shouldNotBeAbstract errors - check ShouldNotBeAbstract.txt" );
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow (TestHelper.Uris.Test, 6182u, DisplayName = "StringNodeId has ExpandedNodeId Value")]
        [DataRow(TestHelper.Uris.Root, 2994u, DisplayName = "Auditing has Boolean Value")]
        public void TestInstanceIsAbstract(TestHelper.Uris uri, uint nodeId )
        {
            SystemUnitClassType element = GetObject(uri, nodeId) as SystemUnitClassType;
            Assert.IsNull(element.Attribute["IsAbstract"], "Instances should not have IsAbstract");
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow(TestHelper.Uris.Root, 62u, false, DisplayName = "BaseVariableType")]
        [DataRow(TestHelper.Uris.Root, 58u, true, DisplayName = "BaseObjectType")]
        public void TestSystemUnitClassIsAbstract(TestHelper.Uris uri, uint nodeId, bool isNull)
        {
            SystemUnitClassType element = GetObject(uri, nodeId) as SystemUnitClassType;
            AttributeType isAbstract = element.Attribute["IsAbstract"];
            if (isNull)
            {
                Assert.IsNull(isAbstract);
            }
            else
            {
                Assert.IsNotNull(isAbstract);
                Assert.IsNotNull(isAbstract.Value);
                Assert.AreEqual("true", isAbstract.Value);
                Assert.AreEqual(0, isAbstract.Attribute.Count);
            }
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow(TestHelper.Uris.Root, 24u, false, DisplayName = "BaseDataType")]
        [DataRow(TestHelper.Uris.Root, 1u, true, DisplayName = "Boolean")]
        public void TestAttributeIsAbstract(TestHelper.Uris uri, uint nodeId, bool isNull)
        {
            AttributeFamilyType element = GetObject(uri, nodeId) as AttributeFamilyType;
            AttributeType isAbstract = element.Attribute["IsAbstract"];
            if (isNull)
            {
                Assert.IsNull(isAbstract);
            }
            else
            {
                Assert.IsNotNull(isAbstract);
                Assert.IsNotNull(isAbstract.Value);
                Assert.AreEqual("true", isAbstract.Value);
                Assert.AreEqual(0, isAbstract.Attribute.Count);
            }
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow(false, DisplayName = "Aggregates")]
        [DataRow(true, DisplayName = "Inverse")]
        public void TestInterfaceIsAbstract(bool isNull)
        {
            InterfaceFamilyType element = GetObject(TestHelper.Uris.Root, 44, "f") as InterfaceFamilyType;
            AttributeSequence attributes = element.Attribute;
            if ( isNull )
            {
                InterfaceFamilyType inverse = element.InterfaceClass["AggregatedBy"];
                Assert.IsNotNull(inverse);
                attributes = inverse.Attribute;
            }

            AttributeType isAbstract = attributes["IsAbstract"];
            if (isNull)
            {
                Assert.IsNull(isAbstract);
            }
            else
            {
                Assert.IsNotNull(isAbstract);
                Assert.IsNotNull(isAbstract.Value);
                Assert.AreEqual("true", isAbstract.Value);
                Assert.AreEqual(0, isAbstract.Attribute.Count);
            }
        }


        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        public void TestNodeIdInIsAbstract()
        {
            CAEXDocument document = GetDocument("AmlFxTest.xml.amlx");
            int counter = 0;
            foreach( AttributeTypeLibType libType in document.AttributeTypeLib )
            {
                foreach (AttributeFamilyType familyType in libType)
                {
                    AttributeType isAbstract = familyType.Attribute["IsAbstract"];
                    if ( isAbstract != null )
                    {
                        AttributeType nodeId = isAbstract.Attribute["NodeId"];
                        if (nodeId != null)
                        {
                            Console.WriteLine(familyType.Name + " has NodeId in IsAbstract");
                            counter++;
                            // Error
                        }
                    }
                }
            }
            Assert.AreEqual(0, counter, "There were " + counter.ToString() +
                " IsAbstract attributes with NodeId sub-attributes");
        }

        public void WriteTestFile( DirectoryInfo outputDirectory, string fileName, List<string> output)
        {
            if( output.Count == 0 )
            {
                output.Add( "No Errors found" );
            }

            FileInfo writeFile = new FileInfo(
                Path.Combine( outputDirectory.FullName, fileName ) );

            TestHelper.WriteFile( writeFile.FullName, output );
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

        private void WalkHierarchy( SystemUnitClassType classType, bool instances )
        {
            CheckType checkType = CheckType.SucType;
            if ( instances )
            {
                checkType = CheckType.InstanceType;
            }

            Check( classType.Name, classType.ID, IsSucAbstract( classType ), checkType );

            foreach( ExternalInterfaceType externalInterface in classType.ExternalInterface)
            {
                Check( externalInterface.Name, externalInterface.ID,
                    IsExternalInterfaceAbstract( externalInterface ), CheckType.ExternalInterfaceType );
            }

            foreach( InternalElementType internalElement in classType.InternalElement )
            {
                WalkHierarchy( internalElement, instances );
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

        private bool IsExternalInterfaceAbstract( ExternalInterfaceType interfaceType )
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

        private void Check( string name, string id, bool isAbstract, CheckType checkType )
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
                            string prefix = "";

                            if ( checkType == CheckType.InstanceType )
                            {
                                prefix = "Instance ";
                            }
                            else if( checkType == CheckType.ExternalInterfaceType )
                            {
                                prefix = "External Interface ";
                            }
                            else if( checkType == CheckType.InterfaceType )
                            {
                                prefix = "Interface ";
                            }
                            else if( checkType == CheckType.SucType )
                            {
                                prefix = "System Unit Class ";
                            }

                            ShouldNotBeAbstract.Add( prefix + name + " [" + id + "] Marked Abstract when it should not be" );
                        }
                    }
                }
                else
                {
                    // UaMethodNodeClass is not in a nodeset.
                    if( !name.Equals( "UaMethodNodeClass" ) )
                    {
                        BadId.Add( "Unable to check " + name + " [" + id + "]" );
                    }
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

        public CAEXObject GetObject( TestHelper.Uris uri, uint nodeId, string prefix = "" )
        {
            CAEXDocument document = TestHelper.GetReadOnlyDocument("TestAml.xml.amlx");
            string amlNodeId = TestHelper.BuildAmlId(prefix, uri, nodeId.ToString());

            CAEXObject initialObject = document.FindByID(amlNodeId);

            Assert.IsNotNull(initialObject, "Unable to find Initial Object");

            return initialObject;

        }

        private CAEXDocument GetDocument( string fileName = NodeSetFileContainer )
        {
            CAEXDocument document = TestHelper.GetReadOnlyDocument( NodeSetFileContainer );
            
            Assert.IsNotNull( document, "Unable to retrieve Document " + fileName );
            return document;
        }

    }
}
