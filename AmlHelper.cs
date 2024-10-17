/* ========================================================================
 * Copyright (c) 2024 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Opc2Aml
{
    public class AmlHelper
    {

        CAEXDocument _Document = null;

        public static readonly string ATLPrefix = "ATL_";
        public static readonly string ICLPrefix = "ICL_";
        public static readonly string RCLPrefix = "RCL_";
        public static readonly string SUCPrefix = "SUC_";
        public static readonly string MetaModelName = "OpcAmlMetaModel";

        public static readonly string InstanceBasePath = "RefBaseSystemUnitPath";
        public static readonly string AttributeBasePath = "RefAttributeTypeUnitPath";
        public static readonly string SystemUnitBasePath = "RefSystemUnitPath";
        public static readonly string InterfaceBasePath = "RefSystemUnitPath";
        public static readonly string RoleClassBasePath = "RefSystemUnitPath";

        public const string FxAcNamespace = "http://opcfoundation.org/UA/FX/AC/";
        public static readonly string AutomationComponentPath = "[" + SUCPrefix + FxAcNamespace + "]/[AutomationComponentType]";

        public AmlHelper( CAEXDocument document )
        {
            _Document = document;
        }

        public CAEXDocument GetDocument() { return _Document; }


        public SystemUnitClassType GetRootFolder()
        {
            SystemUnitClassType rootObject = null;

            CAEXDocument document = GetDocument();

            if( document != null )
            {
                foreach( InstanceHierarchyType type in document.CAEXFile.InstanceHierarchy )
                {
                    foreach( SystemUnitClassType internalElement in type )
                    {
                        if( internalElement.Name == "Root" )
                        {
                            if( IsNodeId( internalElement.Attribute,
                                "NodeId", Opc.Ua.Namespaces.OpcUa,
                                Opc.Ua.ObjectIds.RootFolder ) )
                            {
                                rootObject = internalElement;
                            }
                        }
                    }
                }
            }

            return rootObject;
        }

        public SystemUnitClassType GetObjectsFolder()
        {
            SystemUnitClassType objectsFolder = null;

            SystemUnitClassType rootFolder = GetRootFolder();
            if ( rootFolder != null )
            {
                objectsFolder = rootFolder.InternalElement[ "Objects" ];
            }

            return objectsFolder;
        }

        public SystemUnitClassType GetFxFolder()
        {
            SystemUnitClassType fxFolder = null;

            SystemUnitClassType objectsFolder = GetObjectsFolder();
            if( objectsFolder != null )
            {
                fxFolder = objectsFolder.InternalElement[ "FxRoot" ];
            }

            return fxFolder;
        }

        public List<InternalElementType> GetAutomationComponents()
        {
            // This makes the assumption that there is one Automation Component

            List<InternalElementType> automationComponents = new List<InternalElementType>();

            SystemUnitClassType fxFolder = GetFxFolder();
            if( fxFolder != null )
            {
                foreach( InternalElementType internalElement in fxFolder.InternalElement )
                {
                    if ( IsAutomationComponent( internalElement ) )
                    {
                        automationComponents.Add( internalElement );
                    }
                }
            }

            return automationComponents;
        }

        public AttributeType GetRootNodeIdAttribute( SystemUnitClassType type )
        {
            AttributeType rootNodeAttribute = null;

            AttributeType nodeId = type.Attribute[ "NodeId" ];
            if( nodeId != null )
            {
                rootNodeAttribute = nodeId.Attribute[ "RootNodeId" ];
            }

            return rootNodeAttribute;
        }

        public bool IsAutomationComponent( InternalElementType component )
        {
            bool automationComponent = false;

            if ( component.RefBaseSystemUnitPath.Equals( AutomationComponentPath, 
                StringComparison.OrdinalIgnoreCase ) )
            {
                automationComponent = true;
            }
            else
            {
                CAEXDocument document = GetDocument();
                if ( document != null )
                {
                    CAEXObject caexObject = document.FindByPath( component.RefBaseSystemUnitPath );
                    if ( caexObject != null )
                    {
                        InternalElementType internalElementType = caexObject as InternalElementType;
                        if ( internalElementType != null )
                        {
                            automationComponent = IsAutomationComponent( internalElementType );
                        }
                    }
                }
            }
            
            return automationComponent;
        }

        public SystemUnitClassType GetServerObject()
        {
            SystemUnitClassType rootObject = null;

            CAEXDocument document = GetDocument();

            if( document != null )
            {
                foreach( InstanceHierarchyType type in document.CAEXFile.InstanceHierarchy )
                {
                    foreach( SystemUnitClassType internalElement in type )
                    {
                        if( internalElement.Name == "Root" )
                        {
                            if( IsNodeId( internalElement.Attribute,
                                "NodeId", Opc.Ua.Namespaces.OpcUa,
                                Opc.Ua.ObjectIds.RootFolder ) )
                            {
                                rootObject = internalElement;
                            }
                        }
                    }
                }
            }

            return rootObject;
        }

        public SystemUnitClassType FindInstance( string name, string refBaseSystemUnitPath, string uri, NodeId nodeId)
        {
            return null;
        }

        private void WalkSystemUnitClass( SystemUnitClassType element, string title, int level = 0 )
        {
            Debug.WriteLine( "WalkSystemUnitClass " + title );

            foreach( InternalElementType internalElement in element.InternalElement )
            {
                WalkSystemUnitClass( internalElement, title + "_" + internalElement.Name, level + 1 );
            }
        }

        public bool IsNodeId( AttributeSequence attribute, string name, string uri, NodeId nodeId )
        {
            bool foundNodeId = false;

            AttributeType nodeAttribute = GetAttribute( attribute, name );
            AttributeType rootAttribute = GetAttribute( nodeAttribute, "RootNodeId" );
            AttributeType namespaceUriAttribute = GetAttribute( rootAttribute, "NamespaceUri"  );

            if ( namespaceUriAttribute != null && namespaceUriAttribute.Value != null)
            {
                if ( namespaceUriAttribute.Value.Equals( uri, StringComparison.OrdinalIgnoreCase ) )
                {
                    string idValue = GetIdTypeName( nodeId.IdType );
                    AttributeType id = GetAttribute( rootAttribute, idValue );
                    if( id != null && id.Value != null )
                    {
                        if( id.Value.Equals( nodeId.Identifier.ToString(), StringComparison.OrdinalIgnoreCase ) )
                        {
                            foundNodeId = true;
                        }
                    }
                }
            }

            return foundNodeId;
        }

        public AttributeType GetAttribute( AttributeType attribute,
            string attributeName )
        {
            AttributeType attributeType = null;
            if ( attribute != null )
            {
                attributeType = attribute.Attribute[ attributeName ];
            }

            return attributeType;
        }

        public AttributeType GetAttribute( AttributeSequence attribute,
            string attributeName )
        {
            AttributeType attributeType = null;
            if( attribute != null )
            {
                attributeType = attribute[ attributeName ];
            }

            return attributeType;
        }

        public string GetIdTypeName( IdType idType )
        {
            string idName = Enum.GetName( typeof( IdType ), idType );
            
            if ( !String.IsNullOrEmpty( idName ) )
            {
                idName += "Id";
            }

            return idName;
        }


        public string FindBaseNamespace( object caexObject )
        {
            string namespaceUri = String.Empty;
            string initial = GetBasePathValue( caexObject );

            if ( initial != null && initial.StartsWith( "[" ) )
            {
                int location = initial.IndexOf( "http://" );
                if ( location >= 0 )
                {
                    string namespaceUriStart = initial.Substring( location );
                    int endLocation = namespaceUriStart.IndexOf( "]" );
                    if ( endLocation >= 0 )
                    {
                        namespaceUri = namespaceUriStart.Substring( 0, endLocation );
                    }
                }
            }

            return namespaceUri;
        }


        public string GetBasePathValue( object caexObject )
        {
            string value = String.Empty;
            string title = String.Empty;

            if ( caexObject is InternalElementType )
            {
                InternalElementType internalElement = caexObject as InternalElementType;
                value = internalElement.RefBaseSystemUnitPath;
                title = internalElement.Name;
            }
            else if ( caexObject is AttributeFamilyType )
            {
                AttributeFamilyType attribute = caexObject as AttributeFamilyType;
                value = attribute.RefAttributeType;
                title = attribute.Name;
            }
            else if ( caexObject is SystemUnitFamilyType )
            {
                SystemUnitFamilyType systemUnitClass = caexObject as SystemUnitFamilyType;
                value = systemUnitClass.RefBaseClassPath;
                title = systemUnitClass.Name;
            }
            else if ( caexObject is InterfaceFamilyType )
            {
                InterfaceFamilyType interfaceClass = caexObject as InterfaceFamilyType;
                value = interfaceClass.RefBaseClassPath;
                title = interfaceClass.Name;
            }
            else if ( caexObject is RoleFamilyType )
            {
                RoleFamilyType roleClass = caexObject as RoleFamilyType;
                value = roleClass.RefBaseClassPath;
                title = roleClass.Name;
            }
            else
            {
                bool unexpected = true;
            }

            return value;
        }
    }
}
