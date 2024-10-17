using Opc.Ua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MarkdownProcessor;

using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Aml.Engine.Adapter;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Reflection.Metadata;
using System.Data;

namespace Opc2Aml
{
    public class Reverse
    {
        public readonly string RootHardCodeFileName = "Opc.Ua.NodeSet2.xml.amlx";
        public readonly string HardCodeFileName = "AmlFxTest.xml.amlx";

        private ModelManager _manager = null;
        private CAEXDocument _document = null;
        private AutomationMLContainer _container = null;

        public void Run( string fileName )
        {
            FileInfo fileInfo = new FileInfo( fileName );
            if( !fileInfo.Exists )
            {
                Utils.LogError( "File not found: " + fileName );
                return;
            }

            _container = new AutomationMLContainer( fileInfo.FullName,
                FileMode.Open, FileAccess.Read );

            if( _container != null )
            {
                _document = CAEXDocument.LoadFromStream( _container.RootDocumentStream() );

                if( _document != null )
                {
                    _manager = new ModelManager();

                    Run();

                    _document.Unload();
                }
                else
                {
                    Utils.LogError( "Unable to open Aml Document " + fileName );
                    return;
                }

                _container.Dispose();
            }
            else
            {
                Utils.LogError( "Unable to open Aml Container " + fileName );
                return;
            }
        }

        public void Run()
        {
            BuildNamespaces buildNamespaces = new BuildNamespaces( );
            if ( buildNamespaces.Run( _document ) )
            {
                List<NamespaceInfo> namespaces = buildNamespaces.NamespaceInfo;

                ModelInfo defaultModel = null;

                _manager.ModelNamespaceIndexes = new List<ModelInfo>();
                foreach( NamespaceInfo namespaceInfo in namespaces )
                {
                    _manager.NamespaceUris.Append( namespaceInfo.Uri );

                    MarkdownProcessor.ModelInfo modelInfo = new MarkdownProcessor.ModelInfo( );
                    modelInfo.NamespaceUri = namespaceInfo.Uri;
                    modelInfo.NamespaceIndex = (ushort)namespaceInfo.Index;
                    if ( modelInfo.NamespaceIndex == 1 )
                    {
                        _manager.DefaultModel = modelInfo;
                    }
                    _manager.ModelNamespaceIndexes.Add(modelInfo);
                }

                foreach( ModelInfo modelInfo in _manager.ModelNamespaceIndexes )
                {
                    _manager.Models.Add( modelInfo.NamespaceUri, modelInfo );
                }


                // Need something that can build a namespace array from the document
                // I already have code that can read the namespaces.
                // But I need a list of dependencies.
                Attributes();
                Roles();
                Interfaces();
                SystemUnitClasses();
                Instances();

            }
        }

        private void Instances()
        {
            foreach( InstanceHierarchyType libType in _document.CAEXFile.InstanceHierarchy )
            {
                Utils.LogTrace( "Instance Library: " + libType.Name );

                foreach( SystemUnitClassType type in libType )
                {
                    new AmlHelper( _document ).GetBasePathValue( type );
                    WalkSystemUnitClass( type, type.Name );
                }
            }
        }

        private void SystemUnitClasses()
        {
            foreach( SystemUnitClassLibType libType in _document.CAEXFile.SystemUnitClassLib )
            {
                Utils.LogTrace( "System Unit Class Library: " + libType.Name );

                foreach( SystemUnitClassType type in libType )
                {
                    new AmlHelper( _document ).FindBaseNamespace( type );
                    Utils.LogTrace( "\tSystem Unit Class: " + type.Name );

                    WalkSystemUnitClass( type, type.Name );
                }
            }
        }

        private void Attributes()
        {
            foreach( AttributeTypeLibType libType in _document.CAEXFile.AttributeTypeLib )
            {
                Utils.LogTrace( "Attribute Library: " + libType.Name );

                foreach( AttributeFamilyType type in libType )
                {
                    if ( type.ID != null )
                    {
                        bool wait = true;
                    }
                    new AmlHelper( _document ).FindBaseNamespace( type );

                    Utils.LogTrace( "\tAttribute: " + type.Name );
                }
            }
        }

        private void Roles()
        {
            foreach( RoleClassLibType libType in _document.CAEXFile.RoleClassLib )
            {
                Utils.LogTrace( "Role CLass Library: " + libType.Name );

                foreach( RoleClassType type in libType )
                {
                    new AmlHelper( _document ).FindBaseNamespace( type );

                    Utils.LogTrace( "\tRole: " + type.Name );

                }
            }
        }

        private void Interfaces()
        {
            foreach( InterfaceClassLibType libType in _document.CAEXFile.InterfaceClassLib )
            {
                Utils.LogTrace( "Interface Library: " + libType.Name );

                foreach( InterfaceClassType type in libType )
                {
                    new AmlHelper( _document ).FindBaseNamespace( type );

                    Utils.LogTrace( "\tInterface: " + type.Name );
                }
            }
        }

        private void WalkSystemUnitClass( SystemUnitClassType element, string title, int level = 0 )
        {
            Utils.LogTrace( "Internal Element: " + element.Name + " [" + title + "]" );

            foreach( InternalElementType internalElement in element.InternalElement )
            {
                WalkSystemUnitClass( internalElement, title + "_" + internalElement.Name, level + 1 );
            }
        }

    }
}
