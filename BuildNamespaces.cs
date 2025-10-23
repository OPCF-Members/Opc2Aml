using Aml.Engine.CAEX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Opc2Aml
{
    internal class BuildNamespaces
    {
        private CAEXDocument _document = null;
        AmlHelper _amlHelper = null;
        List<NamespaceInfo> _namespaceInfo = null;

        public List<NamespaceInfo> NamespaceInfo
        {
            get
            {
                return _namespaceInfo;
            }
        }

        public bool Run( CAEXDocument document)
        {
            _amlHelper = new AmlHelper( document );

            bool success = false;
            _document = document;
            if( _document != null )
            {
                Dictionary<string, HashSet<string>> initialSet = new Dictionary<string, HashSet<string>>();

                foreach( SystemUnitClassLibType systemUnitClass in _document.CAEXFile.SystemUnitClassLib )
                {
                    string typeKeyName = GetTypeKeyName( systemUnitClass.Name, AmlHelper.SUCPrefix );
                    Add( initialSet,typeKeyName );

                    foreach( SystemUnitClassType type in systemUnitClass )
                    {
                        Add( initialSet, typeKeyName, type );
                    }
                }

                foreach( RoleClassLibType roleClass in _document.CAEXFile.RoleClassLib )
                {
                    string typeKeyName = GetTypeKeyName( roleClass.Name, AmlHelper.RCLPrefix );
                    Add( initialSet, typeKeyName );

                    foreach( RoleFamilyType type in roleClass )
                    {
                        Add( initialSet, typeKeyName, type );
                    }
                }

                foreach( InterfaceClassLibType interfaceType in _document.CAEXFile.InterfaceClassLib )
                {
                    string typeKeyName = GetTypeKeyName( interfaceType.Name, AmlHelper.ICLPrefix );
                    Add( initialSet, typeKeyName );

                    foreach( InterfaceFamilyType type in interfaceType )
                    {
                        Add( initialSet, typeKeyName, type );
                    }
                }

                foreach( AttributeTypeLibType attribute in _document.CAEXFile.AttributeTypeLib )
                {
                    string typeKeyName = GetTypeKeyName( attribute.Name, AmlHelper.ATLPrefix );
                    Add( initialSet, typeKeyName );

                    foreach( AttributeFamilyType type in attribute )
                    {
                        Add( initialSet, typeKeyName, type );
                    }
                }

                IterateInstances(initialSet);

                Print( initialSet );

                Dictionary<string, HashSet<string>> compiled = new Dictionary<string, HashSet<string>>();

                foreach( KeyValuePair<string, HashSet<string>> pair in initialSet )
                {
                    HashSet<string> dependencies = new HashSet<string>();
                    AddCompiled( pair.Key, dependencies, initialSet );
                    compiled.Add( pair.Key, dependencies );
                }

                Print( compiled );

                List<NamespaceInfo> buildNamespaces = new List<NamespaceInfo>();

                // find the one with the most dependencies, the least dependencies
                foreach( KeyValuePair<string, HashSet<string>> pair in compiled )
                {
                    buildNamespaces.Add( new NamespaceInfo( pair.Key, pair.Value ) );
                }

                List<NamespaceInfo> alphabetic = buildNamespaces.OrderBy( uri => uri.Uri ).ToList();
                List<NamespaceInfo> ordered = alphabetic.OrderBy( count => count.Count() ).ToList();

                int lastIndex = ordered.Count - 1;
                for( ushort index = 0; index < ordered.Count; index++ )
                {
                    if( index == 0 )
                    {
                        ordered[ index ].Index = index;
                    }
                    else if( index == lastIndex )
                    {
                        ordered[ index ].Index = 1;
                    }
                    else
                    {
                        ordered[ index ].Index = (ushort)(index + 1);
                    }
                }

                _namespaceInfo = alphabetic.OrderBy( index => index.Index ).ToList();

                foreach( NamespaceInfo buildNamespace in _namespaceInfo )
                {
                    Debug.WriteLine( buildNamespace.Uri + " " + buildNamespace.Count() + " Index: " + buildNamespace.Index );
                }

                if ( _namespaceInfo.Count > 0 )
                {
                    success = true;
                }
            }

            return success;
        }

        public void AddCompiled( string uri, HashSet<string> set,
            Dictionary<string, HashSet<string>> source )
        {
            if ( source.TryGetValue( uri, out HashSet<string> dependencies ) )
            {
                foreach( string dependentUri in dependencies )
                {
                    set.Add( dependentUri );
                    AddCompiled( dependentUri, set, source );
                }
            }   
        }

        public void Add( Dictionary<string, HashSet<string>> initialSet, 
            string typeNamespaceUri, object type = null)
        {
            if( !initialSet.ContainsKey( typeNamespaceUri ) )
            {
                initialSet.Add( typeNamespaceUri, new HashSet<string>() );
            }

            if( type != null )
            {
                string baseNameSpace = _amlHelper.FindBaseNamespace( type );
                if ( !String.IsNullOrEmpty( baseNameSpace ) )
                {
                    if( baseNameSpace != typeNamespaceUri )
                    {
                        initialSet[ typeNamespaceUri ].Add( baseNameSpace );
                    }
                }
            }
        }

        public void Print( Dictionary<string, HashSet<string>> map)
        {
            foreach( KeyValuePair<string, HashSet<string>> pair in map )
            {
                Debug.WriteLine( pair.Key );
                foreach( string uri in pair.Value )
                {
                    Debug.WriteLine( "\t" + uri );
                }
            }
        }

        private void IterateInstances( Dictionary<string, HashSet<string>> initialSet )
        {
            SystemUnitClassType rootFolder = _amlHelper.GetRootFolder();

            if( rootFolder != null )
            {
                IterateInstances( initialSet, rootFolder );
            }
        }

        // Replacing this will be big
        private void IterateInstances( Dictionary<string, HashSet<string>> initialSet, 
            SystemUnitClassType systemUnitClassType )
        {
            AttributeType nodeId = _amlHelper.GetRootNodeIdAttribute( systemUnitClassType );
            if( nodeId != null )
            {
                AttributeType namespaceUri = nodeId.Attribute[ "NamespaceUri" ];
                if( namespaceUri != null )
                {
                    Add( initialSet, namespaceUri.Value );
                }
                Add( initialSet, namespaceUri.Value, systemUnitClassType );
            }

            foreach( SystemUnitClassType internalElement in systemUnitClassType.InternalElement )
            {
                IterateInstances( initialSet, internalElement );
            }
        }

        private string GetTypeKeyName( string name, string prefix )
        {
            string key = name;

            if( name.StartsWith( prefix ) )
            {
                key = name.Replace( prefix, string.Empty );
            }

            if( key.StartsWith( AmlHelper.MetaModelName ) ||
                key.StartsWith( "AutomationML" ) )
            {
                key = Opc.Ua.Namespaces.OpcUa;
            }

            return key;
        }


    }
}
