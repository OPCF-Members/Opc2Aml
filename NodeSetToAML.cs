/* ========================================================================
 * Copyright (c) 2005-2021 The OPC Foundation, Inc. All rights reserved.
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



using System;
using System.Collections.Generic;
using System.Text;
using Opc.Ua;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using UANode = MarkdownProcessor.NodeSet.UANode;
using UAType = MarkdownProcessor.NodeSet.UAType;
using UAVariable = MarkdownProcessor.NodeSet.UAVariable;
using UAInstance = MarkdownProcessor.NodeSet.UAInstance;
using Aml.Engine.Adapter;
using System.Xml.Linq;
using System.Linq;
using System.Diagnostics;
using System.Net;
using NodeSetToAmlUtils;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;
using System.Security.AccessControl;
using System.IO.Packaging;
using MarkdownProcessor.NodeSet;
using System.Xml;
using static MarkdownProcessor.ModelManager;
using System.Reflection.Metadata;

namespace MarkdownProcessor
{
    public class NodeSetToAML
    {
        private const string ATLPrefix = "ATL_";
        private const string ICLPrefix = "ICL_";
        private const string RCLPrefix = "RCL_";
        private const string SUCPrefix = "SUC_";
        private const string ListOf = "ListOf";
        private const string MetaModelName = "OpcAmlMetaModel";
        private const string UaBaseRole = "UaBaseRole";
        private const string MethodNodeClass = "UaMethodNodeClass";
        private const string RefClassConnectsToPath = "RefClassConnectsToPath";
        private const string IsSource = "IsSource";
        private const string ForwardPrefix = "f";
        private const string ReversePrefix = "r";
        private const string Enumeration = "Enumeration";
        private ModelManager m_modelManager;
        private CAEXDocument m_cAEXDocument;
        private AttributeTypeLibType m_atl_temp;
        private readonly NodeId PropertyTypeNodeId = new NodeId(68, 0);
        private readonly NodeId HasSubTypeNodeId = new NodeId(45, 0);
        private readonly NodeId HasPropertyNodeId = new NodeId(46, 0);
        private readonly NodeId QualifiedNameNodeId = new NodeId(20, 0);
        private readonly NodeId NodeIdNodeId = new NodeId(17, 0);
        private readonly NodeId ExpandedNodeIdNodeId = new NodeId(18, 0);
        private readonly NodeId RelativePathElementNodeId = new NodeId(537, 0);
        private readonly NodeId RelativePathNodeId = new NodeId(540, 0);

        private readonly NodeId GuidNodeId = new NodeId(14, 0);
        private readonly NodeId BaseDataTypeNodeId = new NodeId(24, 0);
        private readonly NodeId NumberNodeId = new NodeId(26, 0);
        private readonly NodeId OptionSetStructureNodeId = new NodeId(12755, 0);
        private readonly NodeId AggregatesNodeId = new NodeId(44, 0);
        private readonly NodeId HasInterfaceNodeId = new NodeId(17603, 0);
        private readonly NodeId HasTypeDefinitionNodeId = new NodeId(40, 0);
        private readonly NodeId HasModellingRuleNodeId = new NodeId(37, 0);
        private readonly NodeId BaseInterfaceNodeId = new NodeId(17602, 0);
        private readonly NodeId RootNodeId = new NodeId(84, 0);
        private readonly NodeId HierarchicalNodeId = new NodeId(33, 0);
        private readonly NodeId OrganizesNodeId = new NodeId(35, 0);
        private readonly NodeId TypesFolderNodeId = new NodeId(86, 0);
        private readonly NodeId TwoStateDiscreteNodeId = new NodeId(2373, 0);
        private readonly NodeId MultiStateDiscreteNodeId = new NodeId(2376, 0);
        private readonly NodeId MultiStateValueDiscreteNodeId = new NodeId(11238, 0);
        private readonly NodeId EnumerationNodeId = Opc.Ua.DataTypeIds.Enumeration;
        private readonly System.Xml.Linq.XNamespace defaultNS = "http://www.dke.de/CAEX";
        private const string uaNamespaceURI = "http://opcfoundation.org/UA/";
        private const string OpcLibInfoNamespace = "http://opcfoundation.org/UA/FX/2021/08/OpcUaLibInfo.xsd";
        private UANode structureNode;
        private readonly List<string> ExcludedDataTypeList = new List<string>() { "InstanceNode", "TypeNode" };
        private Dictionary<string, Dictionary<string,string>> LookupNames = new Dictionary<string, Dictionary<string, string>>();
        private HashSet<string> ExtensionObjectExclusions = null;
        private HashSet<string> EmptyExclusions = null;

        private List<string> PreventInfiniteRecursionList = new List<string>();
        private const int ua2xslookup_count = 18;
        private const int ua2xslookup_uaname = 0;
        private const int ua2xslookup_xsname = 1;
        private readonly string[,] ua2xsLookup = new string[ua2xslookup_count, 2] {
            { "Boolean" , "xs:boolean"  },
            { "SByte" , "xs:Byte"  },
            { "Byte" , "xs:unsignedByte"  },
            { "Int16" , "xs:short"  },
            { "UInt16" , "xs:unsignedShort"  },
            { "Int32" , "xs:int"  },
            { "UInt32" , "xs:unsignedInt"  },
            { "Int64" , "xs:long"  },
            { "UInt64" , "xs:unsignedLong"  },
            { "Float" , "xs:float"  },
            { "Double" , "xs:double"  },
            { "String" , "xs:string"  },
            { "DateTime" , "xs:dateTime"  },
            { "ByteString" , "xs:base64Binary"  },
            { "LocalizedText" , "xs:string"  },
            { "QualifiedName" , "xs:anyURI"  },
            { "StatusCode" , "xs:unsignedInt"  },
            { Enumeration , "xs:int"  }
        };

        public NodeSetToAML(ModelManager modelManager)
        {
            m_modelManager = modelManager;
        }

        public T FindNode<T>(NodeId sourceId) where T : UANode 
        {
            UANode node = null;
            node = m_modelManager.FindNode<T>(sourceId);
            // display error message and throw
            if( node == null )
                throw new Exception( "Can't find node: " + sourceId.ToString() + "   Is your nodeset file missing a <RequiredModel> element?");
            return node as T;
        }

        public void CreateAML(string modelPath, string modelName = null)
        {
            string modelUri = m_modelManager.LoadModel(modelPath, null, null);
            structureNode = m_modelManager.FindNodeByName("Structure");
            if (modelName == null)
                modelName = modelPath;

            m_cAEXDocument = CAEXDocument.New_CAEXDocument();
            var cAEXDocumentTemp = CAEXDocument.New_CAEXDocument();

            // adds the base libraries to the document
            AutomationMLInterfaceClassLibType.InterfaceClassLib(m_cAEXDocument);
            AutomationMLBaseRoleClassLibType.RoleClassLib(m_cAEXDocument);
            AutomationMLBaseAttributeTypeLibType.AttributeTypeLib(m_cAEXDocument);
            AutomationMLBaseAttributeTypeLibType.AttributeTypeLib(cAEXDocumentTemp);
            AutomationMLInterfaceClassLibType.InterfaceClassLib(cAEXDocumentTemp);

            // process each model in  order (with the base UA Model first )

            AddMetaModelLibraries(m_cAEXDocument);


            List<ModelInfo> MyModelInfoList = new List<ModelInfo>();
            // Get the models in the correct order for processing the base models first
            foreach (var modelInfo in m_modelManager.ModelNamespaceIndexes)
            {
                if (modelInfo != m_modelManager.DefaultModel)
                    MyModelInfoList.Add(modelInfo);
            }
            MyModelInfoList.Add(m_modelManager.DefaultModel);

            foreach (var modelInfo in MyModelInfoList)
            {
                AttributeTypeLibType atl = null;
                InterfaceClassLibType icl = null;
                RoleClassLibType rcl = null;
                SystemUnitClassLibType scl = null;


                m_atl_temp = cAEXDocumentTemp.CAEXFile.AttributeTypeLib.Append(ATLPrefix + modelInfo.NamespaceUri);

                AddLibraryHeaderInfo(m_atl_temp as CAEXBasicObject, modelInfo);

                // create the InterfaceClassLibrary
                var icl_temp = cAEXDocumentTemp.CAEXFile.InterfaceClassLib.Append(ICLPrefix + modelInfo.NamespaceUri);
                AddLibraryHeaderInfo(icl_temp as CAEXBasicObject, modelInfo);

                // Create the RoleClassLibrary
                var rcl_temp = cAEXDocumentTemp.CAEXFile.RoleClassLib.Append(RCLPrefix + modelInfo.NamespaceUri);
                // var rcl_temp = m_cAEXDocument.CAEXFile.RoleClassLib.Append(RCLPrefix + modelInfo.NamespaceUri);
                AddLibraryHeaderInfo(rcl_temp as CAEXBasicObject, modelInfo);

                // Create the SystemUnitClassLibrary
                var scl_temp =  cAEXDocumentTemp.CAEXFile.SystemUnitClassLib.Append(SUCPrefix + modelInfo.NamespaceUri);
                // var scl_temp = m_cAEXDocument.CAEXFile.SystemUnitClassLib.Append(SUCPrefix + modelInfo.NamespaceUri);
                AddLibraryHeaderInfo(scl_temp as CAEXBasicObject, modelInfo);

                SortedDictionary<string, AttributeFamilyType> SortedDataTypes = new SortedDictionary<string, AttributeFamilyType>();
                SortedDictionary<string, NodeId> SortedReferenceTypes = new SortedDictionary<string, NodeId>();
                SortedDictionary<string, NodeId> SortedObjectTypes = new SortedDictionary<string, NodeId>();  // also contains VariableTypes


                foreach (var node in modelInfo.Types)
                {


                    switch (node.Value.NodeClass)
                    {
                        case NodeClass.DataType:
                            var toAdd = ProcessDataType(node.Value);
                            if (toAdd != null)
                            {
                                if (!SortedDataTypes.ContainsKey(node.Value.DecodedBrowseName.Name))
                                    SortedDataTypes.Add(node.Value.DecodedBrowseName.Name, toAdd);
                            }
                            break;
                        case NodeClass.ReferenceType:
                            SortedReferenceTypes.Add(node.Value.DecodedBrowseName.Name, node.Value.DecodedNodeId);
                            break;
                        case NodeClass.ObjectType:
                        case NodeClass.VariableType:
                            if (!SortedObjectTypes.ContainsKey(node.Value.DecodedBrowseName.Name))
                                SortedObjectTypes.Add(node.Value.DecodedBrowseName.Name, node.Value.DecodedNodeId);
                            break;
                    }
                }



                foreach (var dicEntry in SortedDataTypes)
                {
                    if (atl == null)
                    {
                        atl = m_cAEXDocument.CAEXFile.AttributeTypeLib.Append(ATLPrefix + modelInfo.NamespaceUri);
                        AddLibraryHeaderInfo(atl as CAEXBasicObject, modelInfo);
                    }
                    atl.AttributeType.Insert(dicEntry.Value, false);  // insert into the AML document in alpha order
                }

                foreach (var dicEntry in SortedDataTypes)  // cteate the ListOf versions
                {
                    atl.AttributeType.Insert(CreateListOf(dicEntry.Value), false);  // insert into the AML document in alpha order
                }

                foreach (var refType in SortedReferenceTypes)
                {
                    ProcessReferenceType(ref icl_temp, refType.Value);
                }

                // reorder icl_temp an put in the real icl in alpha order

                foreach (var refType in SortedReferenceTypes)
                {
                    string iclpath = BuildLibraryReference(ICLPrefix, modelInfo.NamespaceUri, refType.Key);
                    InterfaceClassType ict = icl_temp.CAEXDocument.FindByPath(iclpath) as InterfaceClassType;

                    if (ict != null)
                    {

                        if (icl == null)
                        {
                            // Create the InterfaceClassLibrary
                            icl = m_cAEXDocument.CAEXFile.InterfaceClassLib.Append(ICLPrefix + modelInfo.NamespaceUri);
                            AddLibraryHeaderInfo(icl as CAEXBasicObject, modelInfo);
                        }
                        icl.Insert(ict, false);
                    }

                }



                foreach (var obType in SortedObjectTypes)
                {
                    FindOrAddSUC(ref scl_temp, ref rcl_temp, obType.Value);
                }

                // re-order the rcl_temp and scl_temp in alpha order into the real rcl and scl


                foreach (var obType in SortedObjectTypes)
                {
                    string sucpath = BuildLibraryReference(SUCPrefix, modelInfo.NamespaceUri, obType.Key);
                    SystemUnitFamilyType sft = scl_temp.CAEXDocument.FindByPath(sucpath) as SystemUnitFamilyType;
                    if (sft != null)
                    {
                        if (scl == null)
                        {
                            scl = m_cAEXDocument.CAEXFile.SystemUnitClassLib.Append(SUCPrefix + modelInfo.NamespaceUri);
                            AddLibraryHeaderInfo(scl as CAEXBasicObject, modelInfo);
                        }
                        scl.Insert(sft, false);
                    }

                    string rclpath = BuildLibraryReference(RCLPrefix, modelInfo.NamespaceUri, obType.Key);
                    var rft = rcl_temp.CAEXDocument.FindByPath(rclpath) as RoleFamilyType;
                    if (rft != null)
                    {
                        if (rcl == null)
                        {
                            rcl = m_cAEXDocument.CAEXFile.RoleClassLib.Append(RCLPrefix + modelInfo.NamespaceUri);
                            AddLibraryHeaderInfo(rcl as CAEXBasicObject, modelInfo);
                        }
                        rcl.Insert(rft, false);
                    }
                }
            }

            CreateInstances(); //  add the instances for each model


            // write out the AML file
            // var OutFilename = modelName + ".aml";
            // m_cAEXDocument.SaveToFile(OutFilename, true);
            var container = new AutomationMLContainer(modelName + ".amlx", System.IO.FileMode.Create);
            container.AddRoot(m_cAEXDocument.SaveToStream(true), new Uri("/" + modelName + ".aml", UriKind.Relative));
            container.Close();
        }

        private void AddLibraryHeaderInfo(CAEXBasicObject bo, ModelInfo modelInfo = null)
        {
            bo.Description = "AutomationML Library to support OPC UA meta model";
            bo.Copyright = "\xA9 OPC Foundation";

            if (modelInfo != null)
            {

                XNamespace ns = OpcLibInfoNamespace;
                bo.Description = "AutomationML Library auto-generated from OPC UA Nodeset file for OPC UA Namepace: " + modelInfo.NamespaceUri;
                var ab = new XElement(ns + "OpcUaLibInfo");

                var ai = new XElement(ns + "OpcUaNamespaceUri", modelInfo.NamespaceUri);
                ab.Add(ai);

                ai = new XElement(ns + "ModelVersion", modelInfo.NodeSet.Models[0].Version);
                ab.Add(ai);

                ai = new XElement(ns + "ModelPublicationDate", modelInfo.PublicationDate);
                ab.Add(ai);

                bo.AdditionalInformation.Append(ab);
            }

            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var aj = new XElement(defaultNS + "Nodeset2AmlToolVersion");
            aj.SetValue(version);
            bo.AdditionalInformation.Append(aj);

            var buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
            aj = new XElement(defaultNS + "Nodeset2AmlToolBuildDate");
            aj.SetValue(buildDate.ToUniversalTime());
            bo.AdditionalInformation.Append(aj);
        }


        private AttributeValueRequirementType BuiltInTypeConstraint()
        {
            AttributeValueRequirementType avrt = new AttributeValueRequirementType(new System.Xml.Linq.XElement(defaultNS + "Constraint"));
            avrt.Name = "BuiltInType Constraint";
            var res = avrt.New_NominalType();

            foreach (string name in Enum.GetNames(typeof(BuiltInType)))
            {
                res.RequiredValue.Append(name);
            }

            return avrt;
        }

        private AttributeValueRequirementType AttributeIdConstraint()
        {
            AttributeValueRequirementType avrt = new AttributeValueRequirementType(new System.Xml.Linq.XElement(defaultNS + "Constraint"));
            avrt.Name = "AttributeId Constraint";
            var res = avrt.New_NominalType();

            foreach (string name in Attributes.GetBrowseNames())
            {
                res.RequiredValue.Append(name);
            }

            return avrt;
        }

        private void AddMetaModelLibraries(CAEXDocument doc)
        {
            // add ModelingRuleType (that does not exist in UA) to ATL


            AttributeTypeLibType atl_meta = doc.CAEXFile.AttributeTypeLib.Append(ATLPrefix + MetaModelName);
            AddLibraryHeaderInfo(atl_meta as CAEXBasicObject);

            var added = new AttributeFamilyType(new System.Xml.Linq.XElement(defaultNS + "AttributeType"));
            AttributeType added2 = null;
            var att = added as AttributeTypeType;
            added.Name = "ModellingRuleType";

            att.AttributeDataType = "xs:string";
            AttributeValueRequirementType avrt = new AttributeValueRequirementType(new System.Xml.Linq.XElement(defaultNS + "Constraint"));
            avrt.Name = added.Name + " Constraint";
            var res = avrt.New_NominalType();
            res.RequiredValue.Append("ExposesItsArray");
            res.RequiredValue.Append("Mandatory");
            res.RequiredValue.Append("MandatoryPlaceholder");
            res.RequiredValue.Append("Optional");
            res.RequiredValue.Append("OptionalPlaceholder");
            att.Constraint.Insert(avrt);
            atl_meta.AttributeType.Insert(added, false);

            // add BuiltInType enumeration (that does not exist in UA) to ATL

            added = new AttributeFamilyType(new System.Xml.Linq.XElement(defaultNS + "AttributeType"));
            att = added as AttributeTypeType;
            added.Name = "BuiltInType";
            att.AttributeDataType = "xs:string";
            att.Constraint.Insert(BuiltInTypeConstraint());
            atl_meta.AttributeType.Insert(added, false);

            // add AttributeId enumeration (that does not exist in UA) to ATL
            added = new AttributeFamilyType(new System.Xml.Linq.XElement(defaultNS + "AttributeType"));
            att = added as AttributeTypeType;
            added.Name = "AttributeId";
            att.AttributeDataType = "xs:string";
            att.Constraint.Insert(AttributeIdConstraint());
            atl_meta.AttributeType.Insert(added, false);

            // add NamespaceUri 
            var NamespaceUriAttr = new AttributeFamilyType(new System.Xml.Linq.XElement(defaultNS + "AttributeType"));
            att = NamespaceUriAttr as AttributeTypeType;
            NamespaceUriAttr.Name = "NamespaceUri";
            att.AttributeDataType = "xs:anyURI";
            atl_meta.AttributeType.Insert(NamespaceUriAttr, false);

            // add ExplicitNodeId
            var ExplicitNodeIdAttr = new AttributeFamilyType(new System.Xml.Linq.XElement(defaultNS + "AttributeType"));
            att = ExplicitNodeIdAttr as AttributeTypeType;
            ExplicitNodeIdAttr.Name = "ExplicitNodeId";
            
            added2 = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
            added2.Name = "NamespaceUri";
            added2.RecreateAttributeInstance(NamespaceUriAttr);
            ExplicitNodeIdAttr.Insert(added2, false);

            added2 = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
            added2.Name= "NumericId";
            added2.AttributeDataType = "xs:long";
            ExplicitNodeIdAttr.Insert(added2, false);

            added2 = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
            added2.Name = "StringId";
            added2.AttributeDataType = "xs:string";
            ExplicitNodeIdAttr.Insert(added2, false);

            added2 = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
            added2.Name = "GuidId";
            added2.AttributeDataType = "xs:string";
            ExplicitNodeIdAttr.Insert(added2, false);

            added2 = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
            added2.Name = "OpaqueId";
            added2.AttributeDataType = "xs:base64Binary";
            ExplicitNodeIdAttr.Insert(added2, false);

            atl_meta.AttributeType.Insert(ExplicitNodeIdAttr, false);

            // add Alias
            var AliasAttr = new AttributeFamilyType(new System.Xml.Linq.XElement(defaultNS + "AttributeType"));
            att = AliasAttr as AttributeTypeType;
            AliasAttr.Name = "Alias";
            
            added2 = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
            added2.Name = "AliasName";
            added2.AttributeDataType = "xs:string";
            AliasAttr.Insert(added2, false);

            added2 = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
            added2.Name = "ReferenceTypeFilter";
            added2.RecreateAttributeInstance(ExplicitNodeIdAttr);
            AliasAttr.Insert(added2, false);

            atl_meta.AttributeType.Insert(AliasAttr, false);

            

            // add UABaseRole to the RCL
            var rcl_meta = m_cAEXDocument.CAEXFile.RoleClassLib.Append(RCLPrefix + MetaModelName);
            var br = rcl_meta.New_RoleClass(UaBaseRole);
            br.RefBaseClassPath = "AutomationMLBaseRoleClassLib/AutomationMLBaseRole";
            AddLibraryHeaderInfo(rcl_meta as CAEXBasicObject);

            // add meta model SUC
            var suc_meta = m_cAEXDocument.CAEXFile.SystemUnitClassLib.Append(SUCPrefix + MetaModelName);
            // add MethodNodeClass to the SUC
            // This will add a guid ID, as UaMethodNodeClass is not in the Nodeset file
            var methodNodeClass = suc_meta.New_SystemUnitClass( MethodNodeClass );
            // Give this a known repeatable ID, as there is no node ID for it.
            string methodNodeClassUniqueId = "686619c7-0101-4869-b398-aa0f98bc5f54";
            methodNodeClass.ID = methodNodeClassUniqueId;
            methodNodeClass.New_SupportedRoleClass( RCLPrefix + MetaModelName + "/" + UaBaseRole, false );
            // Issue 16 Add BrowseName to SUC UaMethodNodeClass
            // Manually add these to simulate what the AddModifyAttribute would do if it were possible
            AttributeType browseNameAttributeType = methodNodeClass.Attribute.Append( "BrowseName" );
            browseNameAttributeType.RefAttributeType = "[" + ATLPrefix + Opc.Ua.Namespaces.OpcUa + "]/[QualifiedName]";
            AttributeType namespaceUriAttributeType = browseNameAttributeType.Attribute.Append( "NamespaceURI" );
            namespaceUriAttributeType.AttributeDataType = "xs:anyURI";
            AttributeType nameAttributeType = browseNameAttributeType.Attribute.Append( "Name" );
            nameAttributeType.AttributeDataType = "xs:string";

            AddLibraryHeaderInfo( suc_meta as CAEXBasicObject);
        }

        private void AddBaseNodeClassAttributes( AttributeSequence seq, UANode uanode, UANode basenode = null)
        {
            // only set the value if different from the base node
            string baseuri = "";
            if (basenode != null )
              baseuri = m_modelManager.ModelNamespaceIndexes[basenode.DecodedBrowseName.NamespaceIndex].NamespaceUri;
            string myuri = m_modelManager.ModelNamespaceIndexes[uanode.DecodedBrowseName.NamespaceIndex].NamespaceUri;

            var browse = seq["BrowseName"];
            if (browse == null)
            {
                browse = AddModifyAttribute(seq, "BrowseName", "QualifiedName", Variant.Null);
            }

            if (myuri != baseuri)
            {
                var uriatt = browse.Attribute["NamespaceURI"];

                uriatt.Value = myuri;
            }

            if (uanode.DisplayName != null &&
                uanode.DisplayName.Length > 0 &&
                uanode.DisplayName[0].Value != uanode.DecodedBrowseName.Name)
            {
                AddModifyAttribute(seq, "DisplayName", "LocalizedText",
                    uanode.DisplayName[0].Value);
            }

            if (uanode.Description != null &&
                uanode.Description.Length > 0 &&
                uanode.Description[0].Value.Length > 0)
            {
                AddModifyAttribute(seq, "Description", "LocalizedText",
                    uanode.Description[0].Value);
            }

            UAType uaType = uanode as UAType;
            if (  uaType != null && uaType.IsAbstract )
            {
                AddModifyAttribute(seq, "IsAbstract", "Boolean", uaType.IsAbstract);
            }
        }

        private AttributeType AddModifyAttribute(AttributeSequence seq, string name, string refDataType, Variant val, bool bListOf = false, string sURI = uaNamespaceURI)
        {
            string sUADataType = refDataType;

            var DataTypeNode = m_modelManager.FindNode<UANode>(refDataType);
            if (DataTypeNode != null)
            {
                sUADataType = DataTypeNode.DecodedBrowseName.Name;
                sURI = m_modelManager.FindModelUri(DataTypeNode.DecodedNodeId);
            }

            string ListOfPrefix = "";
            if (bListOf == true)
                ListOfPrefix = ListOf;
            string path = BuildLibraryReference(ATLPrefix, sURI, ListOfPrefix + sUADataType);
            var ob = m_cAEXDocument.FindByPath(path);
            var at = ob as AttributeFamilyType;
            AttributeType a = seq[name];  //find the existing attribute with the name
            if (a == null)
            {
                if (bListOf == false && val.TypeInfo != null)  // look for reasons not to add the attribute because missing == default value
                {
                    if (name == "ValueRank" && val == -2 )
                        return null;
                    if (name == "IsAbstract" && val == false)
                        return null;
                    if (name == "IsSource" && val == false)
                        return null;
                    if (name == "Symmetric" && val == false)
                        return null;
                }
                    a = seq.Append(name);  // not found so create a new one
            }

            a.RecreateAttributeInstance(at);

            if (val.TypeInfo != null)
            {
                if (bListOf == true)
                {
                    string nodeName = "";
                    string referenceName = "";
                    CAEXWrapper reference = seq.CAEXOwner;
                    if (reference != null)
                    {
                        referenceName = reference.Name();
                        CAEXWrapper referenceParent = reference.CAEXParent;
                        if (referenceParent != null)
                        {
                            nodeName = referenceParent.Name();
                        }
                    }

                    bool addElements = true;

                    switch( val.TypeInfo.BuiltInType )
                    {
                        case BuiltInType.LocalizedText:
                            {
                                if( refDataType == "LocalizedText" && referenceName == "EnumStrings" )
                                {
                                    addElements = false;
                                    AttributeTypeType attributeType = a as AttributeTypeType;
                                    if( attributeType != null )
                                    {
                                        AttributeValueRequirementType stringValueRequirement = new AttributeValueRequirementType(
                                            new System.Xml.Linq.XElement( defaultNS + "Constraint" ) );
                                        stringValueRequirement.Name = nodeName + " Constraint";
                                        attributeType.AttributeDataType = "xs:string";
                                        NominalScaledTypeType stringValueNominalType = stringValueRequirement.New_NominalType();

                                        Opc.Ua.LocalizedText[] values = val.Value as Opc.Ua.LocalizedText[];
                                        if( values != null )
                                        {
                                            foreach( Opc.Ua.LocalizedText value in values )
                                            {
                                                stringValueNominalType.RequiredValue.Append( value.Text );
                                            }
                                            attributeType.Constraint.Insert( stringValueRequirement );
                                        }
                                    }
                                }

                                break;
                            }

                        case BuiltInType.ExtensionObject:
                            {
                                if( refDataType == "EnumValueType" && referenceName == "EnumValues" )
                                {
                                    addElements = false;
                                    AttributeTypeType attributeType = a as AttributeTypeType;
                                    if( attributeType != null )
                                    {
                                        AttributeValueRequirementType stringValueRequirement = new AttributeValueRequirementType(
                                            new System.Xml.Linq.XElement( defaultNS + "Constraint" ) );
                                        stringValueRequirement.Name = nodeName + " Constraint";
                                        attributeType.AttributeDataType = "xs:string";
                                        NominalScaledTypeType stringValueNominalType = stringValueRequirement.New_NominalType();

                                        ExtensionObject[] values = val.Value as ExtensionObject[];
                                        if( values != null )
                                        {
                                            foreach( ExtensionObject value in values )
                                            {
                                                EnumValueType enumValueType = value.Body as EnumValueType;
                                                if( enumValueType != null )
                                                {
                                                    stringValueNominalType.RequiredValue.Append( enumValueType.DisplayName.Text );
                                                }
                                            }
                                            attributeType.Constraint.Insert( stringValueRequirement );
                                        }
                                    }
                                }

                                break;
                            }
                    }

                    if( addElements )
                    {
                        IList valueAsList = val.Value as IList;
                        if( valueAsList != null )
                        {
                            for( int index = 0; index < valueAsList.Count; index++ )
                            {
                                Variant elementVariant = new Variant( valueAsList[ index ] );
                                bool elementListOf = elementVariant.TypeInfo.ValueRank >= ValueRanks.OneDimension;
                                AddModifyAttribute( a.Attribute, index.ToString(), refDataType, 
                                    elementVariant, elementListOf, sURI );
                            }
                        }
                    }
                }
                else
                {
                    switch (val.TypeInfo.BuiltInType)  // TODO -- consider supporting setting values for more complicated types (enums, structures, Qualified Names ...) and arrays
                    {
                        case BuiltInType.Boolean:
                        case BuiltInType.SByte:
                        case BuiltInType.Byte:
                        case BuiltInType.Int16:
                        case BuiltInType.UInt16:
                        case BuiltInType.Int32:
                        case BuiltInType.UInt32:
                        case BuiltInType.Int64:
                        case BuiltInType.UInt64:
                        case BuiltInType.Float:
                        case BuiltInType.Double:
                        case BuiltInType.String:
                        case BuiltInType.DateTime:
                        case BuiltInType.Guid:
                        case BuiltInType.XmlElement:
                        case BuiltInType.Number:
                        case BuiltInType.Integer:
                        case BuiltInType.UInteger:
                        case BuiltInType.Enumeration:
                            {
                                a.DefaultAttributeValue = a.AttributeValue = val;
                                break;
                            }

                        case BuiltInType.ByteString:
                            {
                                byte[] bytes = val.Value as byte[];
                                if ( bytes != null )
                                {
                                    string encoded = Convert.ToBase64String( bytes, 0, bytes.Length );
                                    a.DefaultAttributeValue = a.AttributeValue = new Variant( encoded.ToString() );
                                    //StringBuilder stringBuilder = new StringBuilder();
                                    //for( int index = 0; index < bytes.Length; index++ )
                                    //{
                                    //    stringBuilder.Append( (char)bytes[ index ] );
                                    //}
                                    //a.DefaultAttributeValue = a.AttributeValue = new Variant( stringBuilder.ToString() );
                                }

                                break;
                            }

                        case BuiltInType.NodeId:
                        case BuiltInType.ExpandedNodeId:
                            {
                                NodeId nodeId = null;
                                ExpandedNodeId expandedNodeId = null;
                                if( val.TypeInfo.BuiltInType == BuiltInType.NodeId )
                                {
                                    nodeId = val.Value as NodeId;
                                }
                                else
                                {
                                    expandedNodeId = val.Value as ExpandedNodeId;
                                    nodeId = ExpandedNodeId.ToNodeId( expandedNodeId, m_modelManager.NamespaceUris);
                                }

                                if ( nodeId != null )
                                {
                                    a.DefaultAttributeValue = a.AttributeValue = nodeId;
                                    AttributeType rootNodeId = a.Attribute[ "RootNodeId" ];
                                    if ( rootNodeId != null )
                                    {
                                        AttributeType namespaceUri = rootNodeId.Attribute[ "NamespaceUri" ];
                                        if ( namespaceUri != null )
                                        {
                                            namespaceUri.Value = m_modelManager.ModelNamespaceIndexes[ nodeId.NamespaceIndex ].NamespaceUri;
                                        }

                                        switch( nodeId.IdType )
                                        {
                                            case IdType.Numeric:
                                                {
                                                    AttributeType id = rootNodeId.Attribute[ "NumericId" ];
                                                    id.Value = nodeId.Identifier.ToString();
                                                    break;
                                                }

                                            case IdType.String:
                                                {
                                                    AttributeType id = rootNodeId.Attribute[ "StringId" ];
                                                    id.Value = nodeId.Identifier.ToString();
                                                    break;
                                                }

                                            case IdType.Guid:
                                                {
                                                    AttributeType id = rootNodeId.Attribute[ "GuidId" ];
                                                    id.Value = nodeId.Identifier.ToString();
                                                    break;
                                                }

                                            case IdType.Opaque:
                                                {
                                                    byte[] bytes = nodeId.Identifier as byte[];
                                                    if( bytes != null )
                                                    {
                                                        AttributeType id = rootNodeId.Attribute[ "OpaqueId" ];
                                                        string encoded = Convert.ToBase64String(
                                                            bytes, 0, bytes.Length );
                                                        id.Value = encoded;
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    a.DefaultValue = null;
                                    a.Value = null;
                                }

                                if ( expandedNodeId != null )
                                {
                                    if ( expandedNodeId.ServerIndex > 0 )
                                    {
                                        if( expandedNodeId.ServerIndex < m_modelManager.ModelNamespaceIndexes.Count )
                                        {
                                            string serverUri = m_modelManager.ModelNamespaceIndexes[ (int)expandedNodeId.ServerIndex ].NamespaceUri;
                                            AttributeType serverInstanceUri = a.Attribute[ "ServerInstanceUri" ];
                                            if ( serverInstanceUri != null )
                                            {
                                                serverInstanceUri.Value = serverUri;
                                            }
                                        }
                                    }
                                }

                                break;
                            }

                        case BuiltInType.StatusCode:
                            {
                                StatusCode statusCode = (StatusCode)val.Value;
                                a.DefaultAttributeValue = a.AttributeValue = statusCode.Code;

                                break;
                            }

                        case BuiltInType.QualifiedName:
                            {
                                a.DefaultAttributeValue = a.AttributeValue = val;

                                QualifiedName qualifiedName = val.Value as QualifiedName;
                                if( qualifiedName != null )
                                {
                                    AttributeType uri = a.Attribute[ "NamespaceURI" ];
                                    uri.Value = m_modelManager.ModelNamespaceIndexes[ qualifiedName.NamespaceIndex ].NamespaceUri;
                                    AttributeType nameAttribute = a.Attribute[ "Name" ];
                                    nameAttribute.Value = qualifiedName.Name;
                                    a.DefaultValue = null;
                                    a.Value = null;
                                }

                                break;
                            }

                        case BuiltInType.LocalizedText:
                            {
                                Opc.Ua.LocalizedText localizedText = (Opc.Ua.LocalizedText)val.Value;
                                if( localizedText != null && localizedText.Text != null )
                                {
                                    a.DefaultAttributeValue = a.AttributeValue = localizedText.Text;
                                }
                                break;
                            }

                        case BuiltInType.ExtensionObject:
                            {
                                ExtensionObject extensionObject = val.Value as ExtensionObject;
                                if ( extensionObject != null && extensionObject.Body != null )
                                {
                                    AddModifyAttributeObject( a, extensionObject.Body, extensionObject: true );
                                }

                                break;
                            }

                        case BuiltInType.DataValue:
                            {
                                DataValue dataValue = val.Value as DataValue;
                                if( dataValue != null )
                                {
                                    Variant actualValue = new Variant( dataValue.Value );
                                    NodeId dataTypeNodeId = Opc.Ua.TypeInfo.GetDataTypeId( actualValue.Value );

                                    bool actualListOf = actualValue.TypeInfo.ValueRank >= ValueRanks.OneDimension;

                                    AddModifyAttribute( a.Attribute, "Value", dataTypeNodeId, actualValue, actualListOf );
                                    AddModifyAttribute( a.Attribute, "StatusCode", "StatusCode",
                                        dataValue.StatusCode );
                                    AddModifyAttribute( a.Attribute, "SourceTimestamp", "DateTime",
                                        dataValue.SourceTimestamp );
                                    if( dataValue.SourcePicoseconds > 0 )
                                    {
                                        AddModifyAttribute( a.Attribute, "SourcePicoseconds", "UInt32",
                                            dataValue.SourcePicoseconds );
                                    }
                                    AddModifyAttribute( a.Attribute, "ServerTimestamp", "DateTime",
                                        dataValue.ServerTimestamp );
                                    if ( dataValue.ServerPicoseconds > 0 )
                                    {
                                        AddModifyAttribute( a.Attribute, "ServerPicoseconds", "UInt32",
                                            dataValue.ServerPicoseconds );
                                    }
                                }
                                break;
                            }

                        case BuiltInType.Variant:
                            {
                                Variant internalVariant = new Variant( val.Value );
                                NodeId dataTypeNodeId = Opc.Ua.TypeInfo.GetDataTypeId( internalVariant.Value );

                                bool internalListOf = internalVariant.TypeInfo.ValueRank >= ValueRanks.OneDimension;

                                AddModifyAttribute( a.Attribute, "Value", dataTypeNodeId, internalVariant, internalListOf );

                                break;
                            }

                        case BuiltInType.DiagnosticInfo:
                            {
                                AddModifyAttributeIterateObject( a, val );
                                break;
                            }
                        
                    }
                }
            }

            return a;
        }

        private void AddModifyAttributeIterateObject( AttributeType attribute, 
            Variant value )
        {
            AddModifyAttributeObject( attribute, value.Value, extensionObject: false );
        }

        private void AddModifyAttributeObject( AttributeType attribute, object value, bool extensionObject )
        {
            Type valueType = value.GetType();

            if( valueType.FullName.StartsWith( "Opc.Ua." ) )
            {
                PropertyInfo[] properties = value.GetType().GetProperties();

                HashSet<string> exclusions = GetExclusions( extensionObject );

                foreach( PropertyInfo property in properties )
                {
                    NodeId propertyNodeId = Opc.Ua.TypeInfo.GetDataTypeId( property.PropertyType );
                    var propertyValue = property.GetValue( value );
                    if( propertyValue != null )
                    {
                        if( !exclusions.Contains( property.Name ) )
                        {
                            AddModifyAttribute( attribute.Attribute,
                                property.Name, propertyNodeId,
                                new Variant( propertyValue ),
                                property.PropertyType.IsArray );
                        }
                    }
                }
            }
            else
            {
                XmlElement xmlElement = value as XmlElement;
                if ( xmlElement != null && attribute.RefAttributeType != null )
                {
                    Debug.WriteLine( "AddModifyAttributeObject for " + xmlElement.Name );

                    AttributeFamilyType definition = m_atl_temp.FindReferencedClass<AttributeFamilyType>( attribute.RefAttributeType );

                    if( definition != null )
                    {
                        foreach( AttributeType attributeField in definition.Attribute )
                        {
                            string refAttributeType = attributeField.RefAttributeType;

                            // Now find this...
                            AttributeFamilyType fieldDefinition = m_atl_temp.FindReferencedClass<AttributeFamilyType>( refAttributeType );
                            if( fieldDefinition != null )
                            {
                                UANode fieldDefinitionNode = m_modelManager.FindNodeByName( fieldDefinition.Name );
                                XmlElement attributeNode = xmlElement[ attributeField.Name ];

                                if( attributeNode != null && attributeNode.FirstChild != null && 
                                    fieldDefinitionNode != null && fieldDefinitionNode.DecodedNodeId != null )
                                {
                                    XmlDocument document = new XmlDocument();
                                    XmlElement elementToDecode = document.CreateElement( fieldDefinition.Name );
                                    elementToDecode.InnerText = attributeNode.FirstChild.InnerText;

                                    XmlDecoder xmlDecoder = CreateDecoder( elementToDecode );
                                    try
                                    {
                                        Opc.Ua.TypeInfo typeInfo = null;
                                        var decodedValue = xmlDecoder.ReadVariantContents( out typeInfo );
                                        if( decodedValue != null )
                                        {
                                            NodeId propertyNodeId = Opc.Ua.TypeInfo.GetDataTypeId( decodedValue );
                                            if( propertyNodeId != null )
                                            {
                                                Variant variantValue = new Variant( decodedValue );

                                                bool isList = variantValue.TypeInfo.ValueRank > ValueRanks.Scalar;

                                                AddModifyAttribute( attribute.Attribute,
                                                    attributeField.Name,
                                                    fieldDefinitionNode.DecodedNodeId,
                                                    variantValue,
                                                    isList );
                                            }
                                            else
                                            {
                                                Debug.WriteLine( "Unknown property Node Id " + propertyNodeId.ToString() + " for value " +
                                                    decodedValue.ToString() + " Property name " + fieldDefinition.Name );
                                            }
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        Debug.WriteLine( "Unable to decode Property name " + fieldDefinition.Name  + 
                                            " for " + xmlElement.Name + "[" + ex.Message + "]" );
                                    }
                                    xmlDecoder.Close();
                                }
                            }
                        }
                    }
                }
            }
        }

        private XmlDecoder CreateDecoder( XmlElement source )
        {
            ServiceMessageContext messageContext = new ServiceMessageContext();
            messageContext.NamespaceUris = m_modelManager.NamespaceUris;
            messageContext.ServerUris = m_modelManager.ServerUris;

            XmlDecoder decoder = new XmlDecoder( source, messageContext );

            decoder.SetMappingTables( m_modelManager.NamespaceUris, m_modelManager.ServerUris );

            return decoder;
        }


        private HashSet<string> GetExclusions( bool extensionObject )
        {
            HashSet<string> exclusions = null;
            if ( extensionObject )
            {
                if ( ExtensionObjectExclusions == null )
                {
                    ExtensionObjectExclusions = new HashSet<string>();
                    ExtensionObjectExclusions.Add( "BinaryEncodingId" );
                    ExtensionObjectExclusions.Add( "JsonEncodingId" );
                    ExtensionObjectExclusions.Add( "XmlEncodingId" );
                    ExtensionObjectExclusions.Add( "TypeId" );
                }
                exclusions = ExtensionObjectExclusions;
            }
            else
            {
                if ( EmptyExclusions == null )
                {
                    EmptyExclusions = new HashSet<string>();
                }
                exclusions = EmptyExclusions;
            }

            return exclusions;
    }

        private void UpdateDerived(ref InternalElementType internalElement, NodeId utilized, NodeId actual)
        { 
            // This should theoretically do all of it.  and run it through a consistent path.
            if (!utilized.Equals(actual))
            {
                List<string> referenceNames = new List<string>();

                List<ReferenceInfo> referenceList = m_modelManager.FindReferences(actual);

                if (referenceList != null)
                {
                    foreach (ReferenceInfo referenceInfo in referenceList)
                    {
                        if (referenceInfo.IsForward)
                        {
                            UANode foundNodeId = FindNode<UANode>(referenceInfo.TargetId);
                            if (foundNodeId.NodeClass == NodeClass.Variable)
                            {
                                UAVariable foundVariable = foundNodeId as UAVariable;
                                if (foundVariable != null)
                                {
                                    referenceNames.Add(foundVariable.BrowseName);
                                }
                            }
                        }
                    }
                }

                foreach (string referenceName in referenceNames)
                {
                    InternalElementType referenceElement = internalElement.InternalElement[referenceName];

                    if (referenceElement != null)
                    {
                        AttributeType valueAttribute = referenceElement.Attribute["Value"];
                        if (valueAttribute != null)
                        {
                            NodeId propertyId = m_modelManager.FindFirstTarget(actual, HasPropertyNodeId, true, referenceName);
                            if (propertyId != null)
                            {
                                var propertyNode = FindNode<UANode>(propertyId);
                                var propertyNodeValue = propertyNode as UAVariable;

                                if ( propertyNodeValue != null)
                                {
                                    bool bListOf = (propertyNodeValue.ValueRank >= ValueRanks.OneDimension );

                                    AddModifyAttribute( referenceElement.Attribute, 
                                        "Value",
                                        propertyNodeValue.DecodedDataType, 
                                        propertyNodeValue.DecodedValue, 
                                        bListOf );
                                }
                            }
                        }
                    }
                }
            }
        }

        private AttributeType AddModifyAttribute(AttributeSequence seq, string name, NodeId refDataType, Variant val, bool bListOf = false)
        {
            var DataTypeNode = FindNode<UANode>(refDataType);
            var sUADataType = DataTypeNode.DecodedBrowseName.Name;
            var sURI = m_modelManager.FindModelUri(DataTypeNode.DecodedNodeId);

            AttributeType returnAttributeType = null;

            if( m_modelManager.IsTypeOf( DataTypeNode.DecodedNodeId, EnumerationNodeId ) == true )
            {
                if( val.TypeInfo != null )
                {
                    int enumerationValue = -1;
                    if( val.TypeInfo.ValueRank == ValueRanks.Scalar )
                    {
                        enumerationValue = (int)val.Value;
                    }
                    else if( val.TypeInfo.ValueRank == ValueRanks.OneDimension )
                    {
                        int[] enumerationValues = (int[])val.Value;
                        if( enumerationValues.Length == 1 )
                        {
                            enumerationValue = enumerationValues[ 0 ];
                        }
                    }

                    if( enumerationValue >= 0 )
                    {
                        UADataType enumerationNode = FindNode<UADataType>( DataTypeNode.DecodedNodeId );
                        if( enumerationNode != null )
                        {
                            if( enumerationNode.Definition != null &&
                                enumerationNode.Definition.Field != null &&
                                enumerationNode.Definition.Field.Length > 0 )
                            {
                                foreach( DataTypeField field in enumerationNode.Definition.Field )
                                {
                                    if( field.Value == enumerationValue )
                                    {
                                        Variant enumerationAsString = new Variant( field.Name );

                                        returnAttributeType = AddModifyAttribute( seq,
                                            name,
                                            sUADataType,
                                            enumerationAsString,
                                            bListOf: false,
                                            sURI: sURI );

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if ( returnAttributeType == null )
            {
                returnAttributeType = AddModifyAttribute( seq, name, sUADataType, val, bListOf, sURI );
            }

            return returnAttributeType;
        }


        private AttributeType AddModifyAttribute(AttributeSequence seq, string name, NodeId refDataType)
        {
            return AddModifyAttribute(seq, name, refDataType, Variant.Null);
        }


        private void OverrideBooleanAttribute(AttributeSequence seq, string AttributeName, Boolean value)
        {
            var at = AddModifyAttribute(seq, AttributeName, "Boolean", value);
        }


        private string GetAttributeDataType(NodeId nodeId)
        {
            UANode baseNode;

            for (int i = 0; i < ua2xslookup_count; i++)
            {
                baseNode = m_modelManager.FindNodeByName(ua2xsLookup[i, ua2xslookup_uaname]);
                if (m_modelManager.IsTypeOf(nodeId, baseNode.DecodedNodeId))
                    return ua2xsLookup[i, ua2xslookup_xsname];
            }
            return "";
        }
        static private string BuildLibraryReference(string prefix, string namespaceURI, string elementName, string inverseName = null)
        {

            if (inverseName == null)
                return "[" + prefix + namespaceURI + "]/[" + elementName + "]";
            else
                return "[" + prefix + namespaceURI + "]/[" + elementName + "]/[" + inverseName + "]";

        }


        private string BaseRefFromNodeId(NodeId nodeId, string LibPrefix, bool UseInverseName = false, bool IsArray = false)
        {
            string NamespaceURI = m_modelManager.FindModelUri(nodeId);
            var BaseNode = m_modelManager.FindNode<UANode>(nodeId);
            if (BaseNode != null)
            {
                if (UseInverseName)
                {
                    var refnode = BaseNode as NodeSet.UAReferenceType;
                    if (refnode.InverseName != null)
                    {
                        if (BaseNode.DecodedBrowseName.Name != refnode.InverseName[0].Value)
                            return BuildLibraryReference(LibPrefix, NamespaceURI, BaseNode.DecodedBrowseName.Name, refnode.InverseName[0].Value);
                    }
                }
                if (IsArray == true)
                {
                    return BuildLibraryReference(LibPrefix, NamespaceURI, ListOf + BaseNode.DecodedBrowseName.Name);  //add ListOf
                }
                else
                {
                    return BuildLibraryReference(LibPrefix, NamespaceURI, BaseNode.DecodedBrowseName.Name);
                }
            }
            return "";
        }


        #region SUC
        void CopyAttributes(ref ExternalInterfaceType eit)
        {
            // var ob = eit.CAEXDocument.FindByPath(eit.RefBaseClassPath);
            var ob = m_cAEXDocument.FindByPath(eit.RefBaseClassPath);

            var iface = ob as InterfaceFamilyType;

            foreach (var e in iface.GetInheritedAttributes())
            {
                // No External Interface should have IsAbstract
                if( e.Name != "IsAbstract" )
                {
                    eit.Attribute.Insert( e );
                }
            }

        }



        ExternalInterfaceType FindOrAddSourceInterface(ref SystemUnitFamilyType suc, string uri, string name, NodeId nodeId )
        {
            string RefBaseClassPath = BuildLibraryReference(ICLPrefix, uri, name);
            var splitname = name.Split('/');
            var leafname = splitname[splitname.Length - 1];
            SystemUnitFamilyType test = suc;
            bool bFoundInParent = false;
            bool bFirst = true;
            while (test != null && bFoundInParent == false)
            {
                foreach (var iface in test.ExternalInterface)
                {
                    if (iface.RefBaseClassPath == RefBaseClassPath)
                    {
                        if (bFirst)
                            return iface;
                        bFoundInParent = true;
                        break;
                    }
                }
                bFirst = false;
                test = test.BaseClass;
            }
            if (bFoundInParent == true)  // make a unique name by appending the SUC name
                leafname += ":" + suc.Name;
            var rtn = suc.ExternalInterface.Append(leafname);
            rtn.ID = AmlIDFromNodeId(nodeId, leafname);
            rtn.RefBaseClassPath = RefBaseClassPath;
            CopyAttributes(ref rtn);
            return rtn;
        }



        ExternalInterfaceType FindOrAddInterface(ref InternalElementType suc, string uri, string name, NodeId nodeId = null)
        {
            var splitname = name.Split('/');
            var leafname = splitname[splitname.Length - 1];
            if( leafname[0] == '[')
                leafname = leafname.Substring(1);  //remove the leading [
            InternalElementType test = suc;

            foreach (var iface in test.ExternalInterface)
            {
                if (iface.Name == leafname)
                    return iface;
            }

            var rtn = suc.ExternalInterface.Append(leafname);
            rtn.RefBaseClassPath = BuildLibraryReference(ICLPrefix, uri, name);
            if( nodeId != null)
                rtn.ID = AmlIDFromNodeId(nodeId, leafname);
            CopyAttributes(ref rtn);
            return rtn;
        }



        private string AmlIDFromNodeId(NodeId nodeId, string prefix = null)
        {
            AmlExpandedNodeId a = new AmlExpandedNodeId(nodeId, m_modelManager.FindModelUri(nodeId), prefix);
            return a.ToString();
         }

        private InternalElementType CreateClassInstanceWithIDReplacement(string prefix, SystemUnitFamilyType child)
        {
            InternalElementType internalElementType = child.CreateClassInstance();
            
            CompareLinksToExternaInterfaces(child, internalElementType);

            InternalElementSequence originalInternalElements = child.InternalElement;
            InternalElementSequence createdInternalElements = internalElementType.InternalElement;

            UpdateInternalElementChildIds(prefix, originalInternalElements, createdInternalElements);
            UpdateInternalElementParentIds(prefix, child, createdInternalElements);

            UpdateExternalInterfaceChildIds(prefix, child, internalElementType);

            return internalElementType;
        }

        private void UpdateInternalElementChildIds(
            string prefix,
            InternalElementSequence original,
            InternalElementSequence requiresUpdate)
        {
            if (original != null && requiresUpdate != null)
            {
                foreach (InternalElementType originalInternalElement in original)
                {
                    InternalElementType internalElementRequiresUpdate =
                        requiresUpdate[originalInternalElement.Name];

                    if (internalElementRequiresUpdate != null)
                    {
                        string nodeIdString = IsolateNodeId(originalInternalElement.ID);
                        string childPrefix = prefix + originalInternalElement.Name + "_";

                        internalElementRequiresUpdate.ID = childPrefix + nodeIdString;

                        // Does not create infinite loop
                        UpdateInternalElementChildIds(childPrefix, 
                            originalInternalElement.InternalElement,
                            internalElementRequiresUpdate.InternalElement);
                    }
                }
            }
        }

        private void UpdateInternalElementParentIds(
            string prefix,
            SystemUnitClassType original,
            InternalElementSequence requiresUpdate)
        {
            if (original != null && requiresUpdate != null)
            {
                List<string> namesWithGuidIds = new List<string>();
                Dictionary<string, InternalElementType> elementsWithGuidIds = new Dictionary<string, InternalElementType>();
                foreach (InternalElementType updateInternalElement in requiresUpdate)
                {
                    Guid guidId;
                    if (Guid.TryParse(updateInternalElement.ID, out guidId))
                    {
                        namesWithGuidIds.Add(updateInternalElement.Name);
                        elementsWithGuidIds.Add(updateInternalElement.Name, updateInternalElement);
                    }
                }

                if ( namesWithGuidIds.Count > 0)
                {
                    if (original.Reference != null)
                    {
                        foreach(InternalElementType more in original.Reference)
                        {
                            if ( elementsWithGuidIds.ContainsKey(more.Name) )
                            {
                                InternalElementType type = elementsWithGuidIds[more.Name];
                                string nodeIdString = IsolateNodeId(more.ID);
                                string addPrefix = prefix + more.Name + "_";
                                type.ID = addPrefix + nodeIdString;
                                // Now Go Down the hierarchy
                                UpdateInternalElementChildIds(addPrefix, more.InternalElement, type.InternalElement);
                            }
                        }
                        
                        UpdateInternalElementParentIds(prefix, original.Reference, requiresUpdate);
                    }
                }
            }
        }

        private void UpdateExternalInterfaceChildIds(
            string prefix,
            SystemUnitClassType original,
            SystemUnitClassType requiresUpdate)
        {
            if (original != null && requiresUpdate != null)
            {
                Dictionary<string, string> linkMap = new Dictionary<string, string>();
                // This logic only updates the link if it is all found
                Dictionary<string, string> linkMap2 = new Dictionary<string, string>();
                int requiredCounter = 0;
                foreach (InternalLinkType requiresUpdateLink in requiresUpdate.InternalLink)
                {
                    Guid currentId;
                    if (Guid.TryParse(requiresUpdateLink.RefPartnerSideB, out currentId))
                    {
                        InternalLinkType originalLink = original.InternalLink[requiresUpdateLink.Name];
                        if (originalLink != null)
                        {
                            string newParentLink = CreateUpdatedLinkName(prefix, originalLink.RefPartnerSideA);
                            string newThisLink = CreateUpdatedLinkName(prefix, originalLink.RefPartnerSideB);

                            if (!linkMap2.ContainsKey(requiresUpdateLink.RefPartnerSideA))
                            {
                                linkMap2.Add(requiresUpdateLink.RefPartnerSideA, newParentLink);
                            }
                            linkMap2.Add(requiresUpdateLink.RefPartnerSideB, newThisLink);

                            requiresUpdateLink.RefPartnerSideA = newParentLink;
                            requiresUpdateLink.RefPartnerSideB = newThisLink;
                        }
                        else
                        {
                            requiredCounter++;
                        }
                    }
                }

                foreach (ExternalInterfaceType externalInterface in requiresUpdate.ExternalInterface)
                {
                    string updatedId = "";
                    if (linkMap2.TryGetValue(externalInterface.ID, out updatedId))
                    {
                        externalInterface.ID = updatedId;
                    }
                }

                foreach (InternalElementType internalElement in requiresUpdate.InternalElement)
                {
                    foreach (ExternalInterfaceType externalInterface in internalElement.ExternalInterface)
                    {
                        string updatedId = "";
                        if (linkMap2.TryGetValue(externalInterface.ID, out updatedId))
                        {
                            externalInterface.ID = updatedId;
                        }
                    }
                }

                foreach (InternalElementType internalElement in requiresUpdate.InternalElement)
                {
                    InternalElementType originalInternalElement = original.InternalElement[internalElement.Name];
                    if (originalInternalElement != null)
                    {
                        string childPrefix = prefix + internalElement.Name + "_";
                        UpdateExternalInterfaceChildIds(childPrefix, originalInternalElement, internalElement);
                    }
                }

                if (requiredCounter > 0)
                {
                    //  This is working, however a limit is required.
                    UpdateExternalInterfaceChildIds(prefix,
                        original.Reference as SystemUnitClassType,
                        requiresUpdate as SystemUnitClassType);
                }
            }
        }

        private string CreateUpdatedLinkName(string prefix, string refString)
        {
            string link = refString;

            // Parse the name out (HasProperty, ComponentOf)
            string name = GetNodeIdPrefix(refString);
            string nodeId = IsolateNodeId(refString);

            return name + prefix + nodeId;
        }


        private string GetTypeNamePath(SystemUnitClassType type)
        {
            string name = "";

            if ( type.CAEXParent != null)
            {
                var baseClassParent = type.CAEXParent as SystemUnitClassLibType;
                
                // This prevents going down a useless path
                if ( baseClassParent == null)
                {
                    if (type.Reference != null)
                    {
                        name = GetTypeNamePath(type.Reference);
                    }
                }
            }

            name += type.Name + "_";

            return name;
        }

        private string IsolateNodeId(string nodeIdString)
        {
            string nodeId = nodeIdString;

            if (!nodeIdString.StartsWith("nsu%3D") )
            {
                int startIndex = nodeIdString.IndexOf("nsu%3D");
                if (startIndex > 0)
                {
                    nodeId = nodeIdString.Substring(startIndex);
                }
            }
            return nodeId;
        }

        private string GetNodeIdPrefix(string nodeIdString)
        {
            string nodeId = IsolateNodeId(nodeIdString);

            int nodeIdIndex = nodeIdString.IndexOf(nodeId);

            string prefix = nodeIdString.Substring(0, nodeIdIndex);

            return prefix;
        }

        private void CompareLinksToExternaInterfaces(SystemUnitClassType child, SystemUnitClassType checkIt)
        {
            // The externalInterfaces should be RefA in the links
            Dictionary<string, bool> externalInterfaces = new Dictionary<string, bool>();
            foreach (ExternalInterfaceType externalInterface in checkIt.ExternalInterface)
            {
                externalInterfaces.Add(externalInterface.ID, false);
            }

            foreach(InternalLinkType internalLink in checkIt.InternalLink)
            {
                if (externalInterfaces.ContainsKey(internalLink.RefPartnerSideA))
                {
                    externalInterfaces[internalLink.RefPartnerSideA] = true;
                }
            }

            bool outputAll = false;
            foreach(KeyValuePair<string, bool> entry in externalInterfaces)
            {
                if (!entry.Value) 
                {
                    outputAll = true;
                }
            }

            if ( outputAll )
            {
                bool deleted = true;
                while (deleted)
                {
                    deleted = false;
                    foreach (ExternalInterfaceType externalInterface in checkIt.ExternalInterface)
                    {
                        bool found = externalInterfaces[externalInterface.ID];
                        // Debugging
                        //Debug.WriteLine("CompareLinksToExternaInterfaces " + checkIt.Name + " - " + externalInterface.Name +
                        //    " ID " + externalInterface.ID +
                        //    " [" + found.ToString() + "]");

                        if (!found)
                        {
                            checkIt.ExternalInterface.RemoveElement(externalInterface);
                            deleted = true;
                        }
                    }
                }
            }
        }

        string GetExternalInterfaceName(ExternalInterfaceSequence externalInterfaces, InternalLinkType internalLinkType, bool source)
        {
            string interfaceName = "";

            string internalLinkReference = internalLinkType.RefPartnerSideB;
            if ( source  )
            {
                internalLinkReference = internalLinkType.RefPartnerSideA;
            }

            foreach( ExternalInterfaceType externalInterfaceType in externalInterfaces)
            {
                if ( externalInterfaceType.ID == internalLinkReference )
                {
                    interfaceName = externalInterfaceType.Name;
                }
            }
            return interfaceName;
        }

        private struct InternalElementsAndLinks
        {
            public InternalElementType ElementType;
            public InternalLinkType LinkType;
        };

        InternalElementType GetReferenceInternalElement(
            ref SystemUnitClassLibType scl,
            ref RoleClassLibType rcl,
            SystemUnitFamilyType parent,
            NodeId typedefNodeId,
            NodeId targetId)
        {
            string pathToType = GetTypeNamePath(parent);

            SystemUnitFamilyType typeDefSuc = FindOrAddSUC(ref scl, ref rcl, typedefNodeId);
            string prefix = pathToType + typeDefSuc.Name + "_";

            SystemUnitFamilyType targetChild = null;

            if (!typedefNodeId.Equals(targetId))
            {
                targetChild = FindOrAddSUC(ref scl, ref rcl, targetId);
                prefix = pathToType + targetChild.Name + "_";
            }

            var typeDefSucCreated = CreateClassInstanceWithIDReplacement(prefix, typeDefSuc);

            if (typedefNodeId.Equals(targetId))
            {
                // No Work Required
                return typeDefSucCreated;
            }

            var targetCreated = CreateClassInstanceWithIDReplacement(prefix, targetChild);

            Dictionary<string, InternalElementsAndLinks> typeDefDictionary = new Dictionary<string, InternalElementsAndLinks>();
            Dictionary<string, InternalElementsAndLinks> targetDictionary = new Dictionary<string, InternalElementsAndLinks>();

            foreach (InternalElementType internalElementType in targetCreated.InternalElement)
            {
                InternalLinkType internalLinkType = targetCreated.InternalLink[internalElementType.Name];

                // Never seen null
                if (internalLinkType != null) 
                {
                    InternalElementsAndLinks addThis = new InternalElementsAndLinks();
                    addThis.ElementType = internalElementType;
                    addThis.LinkType = internalLinkType;
                    targetDictionary.Add(internalElementType.Name, addThis);
                }
            }

            foreach (InternalElementType internalElementType in typeDefSucCreated.InternalElement)
            {
                InternalLinkType internalLinkType = typeDefSucCreated.InternalLink[internalElementType.Name];
                // Never seen null
                if (internalLinkType != null)
                {
                    InternalElementsAndLinks addThis = new InternalElementsAndLinks();
                    addThis.ElementType = internalElementType;
                    addThis.LinkType = internalLinkType;
                    typeDefDictionary.Add(internalElementType.Name, addThis);
                }
            }

            Dictionary<string, ExternalInterfaceType> usedInterfaces = new Dictionary<string, ExternalInterfaceType>();
            Dictionary<string, InternalElementsAndLinks> usedInternalElements = new Dictionary<string, InternalElementsAndLinks>();

            foreach (KeyValuePair<string, InternalElementsAndLinks> entry in typeDefDictionary)
            {
                InternalLinkType internalLinkType = entry.Value.LinkType;
                foreach (ExternalInterfaceType externalInterfaceType in typeDefSucCreated.ExternalInterface)
                {
                    if (internalLinkType.RefPartnerSideA.Equals(externalInterfaceType.ID))
                    {
                        if (!usedInterfaces.ContainsKey(externalInterfaceType.Name))
                        {
                            usedInterfaces.Add(externalInterfaceType.Name, externalInterfaceType);
                        }

                        ExternalInterfaceType externalInterface = usedInterfaces[externalInterfaceType.Name];
                        
                        internalLinkType.RefPartnerSideA = externalInterface.ID;
                        usedInternalElements.Add(entry.Key, entry.Value);
                        break;
                    }
                }
            }

            foreach (KeyValuePair<string, InternalElementsAndLinks> entry in targetDictionary)
            {
                InternalLinkType internalLinkType = entry.Value.LinkType;
                foreach (ExternalInterfaceType externalInterfaceType in targetCreated.ExternalInterface)
                {
                    if (internalLinkType.RefPartnerSideA.Equals(externalInterfaceType.ID))
                    {
                        if (!usedInterfaces.ContainsKey(externalInterfaceType.Name))
                        {
                            usedInterfaces.Add(externalInterfaceType.Name, externalInterfaceType);
                        }
                        ExternalInterfaceType externalInterface = usedInterfaces[externalInterfaceType.Name];
                        internalLinkType.RefPartnerSideA = externalInterface.ID;
                        if (usedInternalElements.ContainsKey(entry.Key))
                        {
                            usedInternalElements[entry.Key] = entry.Value;
                        }
                        else
                        {
                            usedInternalElements.Add(entry.Key, entry.Value);
                        }

                        break;
                    }
                }
            }

            // Rebuild.
            typeDefSucCreated.ExternalInterface.Remove();
            typeDefSucCreated.InternalLink.Remove();
            typeDefSucCreated.InternalElement.Remove();

            foreach (ExternalInterfaceType externalInterface in usedInterfaces.Values)
            {
                typeDefSucCreated.ExternalInterface.Insert(externalInterface);
            }

            foreach (KeyValuePair<string, InternalElementsAndLinks> entry in usedInternalElements)
            {
                typeDefSucCreated.InternalElement.Insert(entry.Value.ElementType);
                typeDefSucCreated.InternalLink.Insert(entry.Value.LinkType);
            }

            CompareLinksToExternaInterfaces(typeDefSucCreated, typeDefSucCreated);

            return typeDefSucCreated;
        }

        private void RebuildExternalInterfaces(
            SystemUnitFamilyType parent,
            SystemUnitClassType systemUnitClass)
        {
            string pathToType = GetTypeNamePath(parent);
            string prefix = pathToType + systemUnitClass.Name + "_";

            RebuildExternalInterfaces(prefix, systemUnitClass);
        }


        private void RebuildExternalInterfaces(
            string prefix,
            SystemUnitClassType systemUnitClass )
        {
            // The purpose here is to modify the InternalLinks and ExternalInterfaces to be more readable
            // from the perspective of the systemUnitClass itself.

            string named = ";" + prefix + WebUtility.UrlDecode(systemUnitClass.ID);

            Dictionary<string, string> oldIdToNewName = new Dictionary<string, string>();
            Dictionary<string,string>oldToNewName = new Dictionary<string, string>();
            Dictionary<string, ExternalInterfaceType> newTypes = new Dictionary<string, ExternalInterfaceType>();

            foreach ( ExternalInterfaceType externalInterface in systemUnitClass.ExternalInterface )
            {
                AttributeType sourceType = externalInterface.Attribute["IsSource"];
                if ( sourceType != null )
                {
                    if(sourceType.Value.Equals("true"))
                    {
                        string[] splitName = externalInterface.Name.Split(":");
                        if ( splitName.Length > 0 )
                        {
                            string newName = splitName[0];
                            if (!oldToNewName.ContainsKey(externalInterface.Name) )
                            {
                                oldIdToNewName.Add(externalInterface.ID, newName);
                                oldToNewName.Add(externalInterface.Name, newName);
                                if (!newTypes.ContainsKey(newName))
                                {
                                    ExternalInterfaceType replace = (ExternalInterfaceType)externalInterface.Copy(deepCopy: true);
                                    replace.Name = newName;
                                    replace.ID = WebUtility.UrlEncode(newName + named);
                                    newTypes.Add(newName, replace);
                                }
                            }
                        }
                    }
                }
            }

            foreach( InternalLinkType internalLink in systemUnitClass.InternalLink)
            {
                string newName;
                if ( oldIdToNewName.TryGetValue(internalLink.RefPartnerSideA, out newName) )
                {
                    ExternalInterfaceType newType;
                    if ( newTypes.TryGetValue(newName, out newType) )
                    {
                        internalLink.RefPartnerSideA = newType.ID;
                    }
                }
            }

            // Now wipe the externalInterfaces and add all the new ones
            systemUnitClass.ExternalInterface.Remove();
            foreach( ExternalInterfaceType externalInterface in newTypes.Values )
            {
                systemUnitClass.ExternalInterface.Insert( externalInterface );
            }
        }

        private void UpdateIsAbstract( UANode targetNode, SystemUnitClassType systemUnitClass )
        {
            bool isTargetAbstract = false;
            UAType targetType = targetNode as UAType;
            if( targetType != null && targetType.IsAbstract )
            {
                isTargetAbstract = true;
            }

            if( !isTargetAbstract )
            {
                AttributeType isAbstractAttribute = systemUnitClass.Attribute[ "IsAbstract" ];
                if( isAbstractAttribute != null )
                {
                    systemUnitClass.Attribute.RemoveElement( isAbstractAttribute );
                }
            }
        }

        private string GetExistingCreatedPathName(UANode node)
        {
            string createdPathName = "";

            string uri = m_modelManager.FindModelUri(node.DecodedNodeId);
            Dictionary<string, string> uriMap;
            if (LookupNames.TryGetValue(uri, out uriMap))
            {
                string nodePath;
                if (uriMap.TryGetValue(node.DecodedNodeId.ToString(), out nodePath))
                {
                    createdPathName = nodePath;
                }
            }

            return createdPathName;
        }

        private string EqualizeParentNodeId(UAInstance node)
        {
            string parentNodeId = node.ParentNodeId;

            string[] parentSplit = node.ParentNodeId.Split(";");
            if ( parentSplit.Length > 1)
            {
                ModelInfo modelInfo = m_modelManager.FindModel(node.DecodedNodeId);
                parentNodeId = String.Format("ns={0};{1}", 
                    modelInfo.NamespaceIndex, parentSplit[1]);
             }

            return parentNodeId;
        }

        private string GetCreatedPathName(UANode node)
        {
            string pathName = GetExistingCreatedPathName(node);

            if (pathName.Length == 0)
            {
                UAInstance uaInstance = node as UAInstance;
                if (uaInstance != null)
                {
                    if (uaInstance.ParentNodeId.Length > 0)
                    {
                        string parentNodeId = EqualizeParentNodeId(uaInstance);
                        UANode parentNode = FindNode<NodeSet.UANode>(new NodeId(parentNodeId));
                        pathName = GetCreatedPathName(parentNode);
                    }
                }

                if (pathName.Length > 0)
                {
                    pathName += "_";
                }

                pathName += node.DecodedBrowseName.Name;

                string uri = m_modelManager.FindModelUri(node.DecodedNodeId);
                if (!LookupNames.ContainsKey(uri))
                {
                    LookupNames.Add(uri, new Dictionary<string, string>());
                }

                string nodeIdString = node.DecodedNodeId.ToString();

                Dictionary<string, string> uriMap = LookupNames[uri];

                // It's still possible that this has already been added, as it is a recursive method
                string existingPathName;
                if (!uriMap.TryGetValue(nodeIdString, out existingPathName))
                {
                    uriMap.Add(nodeIdString, pathName);
                }
            }

            return pathName;
        }


        SystemUnitFamilyType FindOrAddSUC(ref SystemUnitClassLibType scl, ref RoleClassLibType rcl, NodeId nodeId)
        {
            var refnode = FindNode<NodeSet.UANode>(nodeId);
            string path = "";
            if (refnode.NodeClass != NodeClass.Method)
                path = BuildLibraryReference(SUCPrefix, 
                    m_modelManager.FindModelUri(refnode.DecodedNodeId), 
                    GetCreatedPathName(refnode));
            SystemUnitFamilyType rtn = scl.CAEXDocument.FindByPath(path) as SystemUnitFamilyType;

            if (rtn == null)
            {
                if (m_modelManager.IsTypeOf(nodeId, BaseInterfaceNodeId) == true)
                {
                    var rc = rcl.New_RoleClass(refnode.DecodedBrowseName.Name);  // create a RoleClass for UA interfaces
                    rc.ID = AmlIDFromNodeId(nodeId);
                    if (nodeId == BaseInterfaceNodeId)
                        rc.RefBaseClassPath = RCLPrefix + MetaModelName + "/" + UaBaseRole;
                    else
                        rc.RefBaseClassPath = BuildLibraryReference(RCLPrefix, m_modelManager.FindModelUri(BaseInterfaceNodeId), "BaseInterfaceType");
                }
                // make sure the base type is already created
                NodeId BaseNodeId = m_modelManager.FindFirstTarget(refnode.DecodedNodeId, HasSubTypeNodeId, false);
                if (BaseNodeId != null)
                {
                    var refBaseNode = FindNode<NodeSet.UANode>(BaseNodeId);
                    string basepath = BuildLibraryReference(SUCPrefix, m_modelManager.FindModelUri(refBaseNode.DecodedNodeId), refBaseNode.DecodedBrowseName.Name);
                    SystemUnitFamilyType baseSUC = scl.CAEXDocument.FindByPath(basepath) as SystemUnitFamilyType;
                    if (baseSUC == null)
                        
                        FindOrAddSUC(ref scl, ref rcl, BaseNodeId);

                }
                // now add the SUC with the immediate elements
                // starting with the attributes

                rtn = scl.SystemUnitClass.Append(refnode.DecodedBrowseName.Name);
                // Helpful for Debugging
                //AddModifyAttribute(rtn.Attribute, "NodeId", "String", AmlIDFromNodeId(nodeId));
                if (BaseNodeId != null)
                {
                    rtn.ID = AmlIDFromNodeId(nodeId);

                    rtn.RefBaseClassPath = BaseRefFromNodeId(BaseNodeId, SUCPrefix);

                    // override any attribute values

                    var basenode = FindNode<NodeSet.UANode>(BaseNodeId);
                    AddBaseNodeClassAttributes(rtn.Attribute, refnode, basenode);
                    switch (refnode.NodeClass)
                    {
                        case NodeClass.ObjectType:
                            var obnode = refnode as NodeSet.UAObjectType;
                            var baseob = basenode as NodeSet.UAObjectType;
                            if (baseob.IsAbstract != obnode.IsAbstract)
                                OverrideBooleanAttribute(rtn.Attribute, "IsAbstract", obnode.IsAbstract);
                            break;


                        case NodeClass.VariableType:
                            var varnode = refnode as NodeSet.UAVariableType;
                            var basevar = basenode as NodeSet.UAVariableType;

                            if (varnode.ValueRank != basevar.ValueRank)
                                AddModifyAttribute(rtn.Attribute, "ValueRank", "Int32", varnode.ValueRank);
                            if (basevar.IsAbstract != varnode.IsAbstract)
                                OverrideBooleanAttribute(rtn.Attribute, "IsAbstract", varnode.IsAbstract);
                            if (basevar.DataType != varnode.DataType)
                                AddModifyAttribute(rtn.Attribute, "Value", varnode.DecodedDataType);
                            break;

                    }
                }
                else
                {
                    // add the attributes to the base SUC Class
                    switch (refnode.NodeClass)
                    {
                        case NodeClass.ObjectType:
                            rtn.ID = AmlIDFromNodeId( nodeId );

                            // AddModifyAttribute(rtn.Attribute, "EventNotifier", "EventNotifierType", Variant.Null); // #9 remove unset attributes
                            AddBaseNodeClassAttributes(rtn.Attribute, refnode);
                            break;
                        case NodeClass.VariableType:
                            rtn.ID = AmlIDFromNodeId( nodeId );

                            // AddModifyAttribute(rtn.Attribute, "ArrayDimensions", "ListOfUInt32", Variant.Null); // #9 remove unset attributes
                            // AddModifyAttribute(rtn.Attribute, "ValueRank", "Int32", -2); // #9 remove unset attributes
                            // AddModifyAttribute(rtn.Attribute, "Value", "BaseDataType", Variant.Null); // #9 remove unset attributes
                            // AddModifyAttribute(rtn.Attribute, "AccessLevel", "AccessLevelType", Variant.Null); // #9 remove unset attributes
                            // AddModifyAttribute(rtn.Attribute, "MinimumSamplingInterval", "Duration", Variant.Null); // #9 remove unset attributes
                            AddBaseNodeClassAttributes(rtn.Attribute, refnode);
                            break;
                        case NodeClass.Method:
                            AddBaseNodeClassAttributes(rtn.Attribute, refnode);

                            break;
                    }
                                     
                }


                // now add the references and contained objects
                var refList = m_modelManager.FindReferences(nodeId);
                foreach (var reference in refList)
                {
                    if (reference.IsForward == true)
                    {
                        if (m_modelManager.IsTypeOf(reference.ReferenceTypeId, AggregatesNodeId) == true)
                        {
                            string refURI = m_modelManager.FindModelUri(reference.ReferenceTypeId);
                            var ReferenceTypeNode = FindNode<UANode>(reference.ReferenceTypeId);
                            var sourceInterface = FindOrAddSourceInterface(ref rtn, refURI, ReferenceTypeNode.DecodedBrowseName.Name, nodeId);
                            var targetNode = FindNode<UANode>(reference.TargetId);
                            //                           if (targetNode.NodeClass != NodeClass.Method) //  methods are now processed
                            {
                                var TypeDefNodeId = reference.TargetId;
                                if (targetNode.NodeClass == NodeClass.Variable || targetNode.NodeClass == NodeClass.Object)
                                    TypeDefNodeId = m_modelManager.FindFirstTarget(reference.TargetId, HasTypeDefinitionNodeId, true);
                                if (TypeDefNodeId == null)
                                    TypeDefNodeId = reference.TargetId;

                                var ie = GetReferenceInternalElement(ref scl, ref rcl,
                                    rtn, TypeDefNodeId, reference.TargetId);

                                ie.Name = targetNode.DecodedBrowseName.Name;
                                ie.ID = AmlIDFromNodeId(reference.TargetId);

                                UpdateIsAbstract( targetNode, ie );

                                RebuildExternalInterfaces(rtn, ie);

                                rtn.AddInstance(ie);

                                var basenode = FindNode<NodeSet.UANode>(TypeDefNodeId);                               
                                AddBaseNodeClassAttributes(ie.Attribute, targetNode, basenode);
                                if (targetNode.NodeClass == NodeClass.Variable)
                                {  //  Set the datatype for Value
                                    var varnode = targetNode as NodeSet.UAVariable;
                                    bool bListOf = (varnode.ValueRank >= ValueRanks.OneDimension);  // use ListOf when its a UA array
                                    AddModifyAttribute(ie.Attribute, "Value", varnode.DecodedDataType, varnode.DecodedValue, bListOf);
                                    ie.SetAttributeValue("ValueRank", varnode.ValueRank);
                                    ie.SetAttributeValue("ArrayDimensions", varnode.ArrayDimensions);
                                    
                                    UpdateDerived( ref ie, TypeDefNodeId, reference.TargetId );
                                }
                                else if (targetNode.NodeClass == NodeClass.Method)
                                    ie.RefBaseSystemUnitPath = BuildLibraryReference(SUCPrefix, MetaModelName, MethodNodeClass);
                                var ie_suc = ie as SystemUnitClassType;


                                var destInterface = FindOrAddInterface(ref ie, refURI, ReferenceTypeNode.DecodedBrowseName.Name + "]/[" + sourceInterface.Attribute["InverseName"].Value, reference.TargetId);

                                var internalLink = rtn.New_InternalLink(targetNode.DecodedBrowseName.Name);
                                internalLink.RefPartnerSideA = sourceInterface.ID;
                                internalLink.RefPartnerSideB = destInterface.ID;

                                //   set the modeling rule
                                var modellingId = m_modelManager.FindFirstTarget(reference.TargetId, HasModellingRuleNodeId, true);
                                var modellingRule = m_modelManager.FindNode<UANode>(modellingId);
                                if (modellingRule != null)
                                    AddModifyAttribute(destInterface.Attribute, "ModellingRule", "ModellingRuleType", modellingRule.DecodedBrowseName.Name, false, MetaModelName);
                                
                            }
                        }
                        else if (m_modelManager.IsTypeOf(reference.ReferenceTypeId, HasInterfaceNodeId) == true)
                        {
                            // add the elements of the UA Interface
                            var targetNode = FindNode<UANode>(reference.TargetId);
                            string rolepath = BuildLibraryReference(RCLPrefix, m_modelManager.FindModelUri(targetNode.DecodedNodeId), targetNode.DecodedBrowseName.Name);
                            var roleSUC = FindOrAddSUC(ref scl, ref rcl, reference.TargetId);  // make sure the AMLobjects are already created.
                            var srt = rtn.New_SupportedRoleClass(rolepath, false);
                            var inst = roleSUC.CreateClassInstance();
                            foreach (var element in inst.InternalElement)
                            {
                                rtn.InternalElement.Append(element);
                            }
                        }
                    }
                }
                rtn.New_SupportedRoleClass(RCLPrefix + MetaModelName + "/" + UaBaseRole, false);  // all UA SUCs support the UaBaseRole
            }

            return rtn;
        }

        #endregion


        #region ICL

        private void OverrideAttribute(IClassWithBaseClassReference owner, string Name, string AttType, object val)
        {
            var atts = owner.GetInheritedAttributes();
            foreach (var aa in atts)
            {
                if (aa.Name == Name)
                {
                    if (aa.AttributeDataType == AttType && aa.AttributeValue.Equals(val))
                    {
                        return;  // no need to override
                    }
                }
            }

            AttributeType a = owner.Attribute.Append();
            a.Name = Name;
            a.AttributeDataType = AttType;
            a.AttributeValue = val;

        }

        private void ProcessReferenceType(ref InterfaceClassLibType icl, NodeId nodeId)
        {
            var refnode = FindNode<NodeSet.UAReferenceType>(nodeId);
            var added = icl.InterfaceClass.Append(refnode.DecodedBrowseName.Name);
            added.ID = AmlIDFromNodeId(nodeId, ForwardPrefix);
            NodeId BaseNodeId = m_modelManager.FindFirstTarget(refnode.DecodedNodeId, HasSubTypeNodeId, false);
            if (BaseNodeId != null)
            {
                added.RefBaseClassPath = BaseRefFromNodeId(BaseNodeId, ICLPrefix);
                InterfaceClassType ict = icl.CAEXDocument.FindByPath(added.RefBaseClassPath) as InterfaceClassType;
                if (ict == null)
                    ProcessReferenceType(ref icl, BaseNodeId);
            }
            else
            {
                added.RefBaseClassPath = "AutomationMLInterfaceClassLib/AutomationMLBaseInterface";

             //   AddModifyAttribute(added.Attribute, "InverseName", "LocalizedText", Variant.Null);
             //   AddModifyAttribute(added.Attribute, "ModellingRule", "ModellingRuleType", Variant.Null, false, MetaModelName);
             //   OverrideBooleanAttribute(added.Attribute, "Symmetric", true);
             //   OverrideBooleanAttribute(added.Attribute, "IsAbstract", true);
             //   OverrideAttribute(added, IsSource, "xs:boolean", true);
                OverrideAttribute(added, RefClassConnectsToPath, "xs:string", added.CAEXPath());

            }
            // look for inverse name
            InterfaceFamilyType inverseAdded = null;
            if (refnode.InverseName != null)
            {
                if (refnode.Symmetric == false && refnode.InverseName[0].Value != refnode.DecodedBrowseName.Name)
                {
                    inverseAdded = added.InterfaceClass.Append(refnode.InverseName[0].Value);
                    inverseAdded.ID = AmlIDFromNodeId(nodeId, ReversePrefix);
                    if (BaseNodeId != null)
                        inverseAdded.RefBaseClassPath = BaseRefFromNodeId(BaseNodeId, ICLPrefix, true);
                }
            }

            // Only sets it if it is true -
            // It doesn't matter if 'References' is set x times,
            // it would take more time to look it up each time
            OverrideBooleanAttribute( added.Attribute, "IsAbstract", refnode.IsAbstract );

            // ovveride any attribute values
            if (BaseNodeId != null)
            {
                var basenode = FindNode<NodeSet.UAReferenceType>(BaseNodeId);
 
                if (basenode.IsAbstract != refnode.IsAbstract)
                    OverrideBooleanAttribute(added.Attribute, "IsAbstract", refnode.IsAbstract);
                if (basenode.Symmetric != refnode.Symmetric)
                    OverrideBooleanAttribute(added.Attribute, "Symmetric", refnode.Symmetric);

                if (refnode.InverseName != null)
                    AddModifyAttribute(added.Attribute, "InverseName", "LocalizedText", refnode.InverseName[0].Value);

                OverrideAttribute(added, IsSource, "xs:boolean", true);
                OverrideAttribute(added, RefClassConnectsToPath, "xs:string", (inverseAdded != null ? inverseAdded.CAEXPath() : added.CAEXPath()));




                if (inverseAdded != null)
                {

                    if (basenode.IsAbstract != refnode.IsAbstract)
                        OverrideBooleanAttribute(inverseAdded.Attribute, "IsAbstract", refnode.IsAbstract);
                    if (basenode.Symmetric != refnode.Symmetric)
                        OverrideBooleanAttribute(inverseAdded.Attribute, "Symmetric", refnode.Symmetric);
                    AddModifyAttribute(inverseAdded.Attribute, "InverseName", "LocalizedText", refnode.DecodedBrowseName.Name);

                    OverrideAttribute(inverseAdded, IsSource, "xs:boolean", false);
                    OverrideAttribute(inverseAdded, RefClassConnectsToPath, "xs:string", added.CAEXPath());

                }


            }
        }

        #endregion


        #region ATL
        private AttributeFamilyType CreateListOf(AttributeFamilyType aft)
        {
            var rtn = new AttributeFamilyType(new System.Xml.Linq.XElement(defaultNS + "AttributeType"));
            rtn.Name = ListOf + aft.Name;
            m_atl_temp.AttributeType.Insert(rtn);
            rtn.RefAttributeType = "AutomationMLBaseAttributeTypeLib/OrderedListType";

            /* Mantis #7383 - remove placeholder elements of ListOf types
            AttributeType a = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
            
            a.RecreateAttributeInstance(aft);
            a.Name = aft.Name;
           rtn.Insert(a);
            end Mantis #7383 */

            return rtn;
        }


        private void ProcessEnumerations(ref AttributeTypeType att, NodeId nodeId)
        {
            NodeId EnumStringsPropertyId = m_modelManager.FindFirstTarget(nodeId, HasPropertyNodeId, true, "EnumStrings");
            if (EnumStringsPropertyId != null)
            {
                UADataType enumNode = FindNode<UADataType>( nodeId );

                att.AttributeDataType = "xs:string";
                UAVariable EnumStrings = FindNode<UAVariable>(EnumStringsPropertyId);

                AttributeValueRequirementType stringValueRequirement = new AttributeValueRequirementType( new System.Xml.Linq.XElement( defaultNS + "Constraint" ) );
                UADataType MyNode = FindNode<UADataType>(nodeId);
                stringValueRequirement.Name = MyNode.DecodedBrowseName.Name + " Constraint";
                NominalScaledTypeType stringValueNominalType = stringValueRequirement.New_NominalType();

                Opc.Ua.LocalizedText[] EnumValues = EnumStrings.DecodedValue.Value as Opc.Ua.LocalizedText[];
                foreach ( Opc.Ua.LocalizedText EnumValue in EnumValues )
                {
                    stringValueNominalType.RequiredValue.Append( EnumValue.Text );
                }

                att.Constraint.Insert( stringValueRequirement );
                return;
            }

            NodeId EnumValuesPropertyId = m_modelManager.FindFirstTarget(nodeId, HasPropertyNodeId, true, "EnumValues");
            if (EnumValuesPropertyId != null)
            {
                att.AttributeDataType = "xs:string";
                UAVariable EnumValues = FindNode<UAVariable>(EnumValuesPropertyId);
                AttributeValueRequirementType stringValueRequirement = new AttributeValueRequirementType( new System.Xml.Linq.XElement( defaultNS + "Constraint" ) );
                UADataType MyNode = FindNode<UADataType>(nodeId);

                stringValueRequirement.Name = MyNode.DecodedBrowseName.Name + " Constraint";
                NominalScaledTypeType stringValueNominalType = stringValueRequirement.New_NominalType();

                ExtensionObject[] EnumVals = EnumValues.DecodedValue.Value as ExtensionObject[];
                foreach ( ExtensionObject EnumValue in EnumVals )
                {
                    EnumValueType ev = EnumValue.Body as EnumValueType;
                    stringValueNominalType.RequiredValue.Append( ev.DisplayName.Text );
                }

                att.Constraint.Insert( stringValueRequirement );
            }
        }

        private void ProcessOptionSets(ref AttributeTypeType att, NodeId nodeId)
        {
            NodeId OptionSetsPropertyId = m_modelManager.FindFirstTarget(nodeId, HasPropertyNodeId, true, "OptionSetValues");

            if (OptionSetsPropertyId != null && m_modelManager.IsTypeOf(nodeId, NumberNodeId))
            {
                att.AttributeDataType = "";
                var OptionSetsPropertyNode = FindNode<UANode>(OptionSetsPropertyId);
                var OptionSets = OptionSetsPropertyNode as UAVariable;
                Opc.Ua.LocalizedText[] OptionSetValues = OptionSets.DecodedValue.Value as Opc.Ua.LocalizedText[];
                foreach (var OptionSetValue in OptionSetValues)
                {
                    AttributeType a = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
                    a.Name = OptionSetValue.Text;
                    a.AttributeDataType = "xs:boolean";
                    att.Attribute.Insert(a, false);
                }
            }
            

            if (nodeId == NodeIdNodeId || nodeId == ExpandedNodeIdNodeId)
            {

                // add NodeId
                
                var added2 = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
                added2.Name = "ServerInstanceUri";
                added2.AttributeDataType = "xs:anyURI";
                att.Attribute.Insert( added2, false);

                added2 = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
                added2.Name = "Alias";
                string path = BuildLibraryReference(ATLPrefix, MetaModelName, "Alias");
                var ob = m_cAEXDocument.FindByPath(path);
                var at = ob as AttributeFamilyType;
                added2.RecreateAttributeInstance(at);
                att.Attribute.Insert(added2, false);

                added2 = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
                added2.Name = "RootNodeId";
                path = BuildLibraryReference(ATLPrefix, MetaModelName, "ExplicitNodeId");
                ob = m_cAEXDocument.FindByPath(path);
                at = ob as AttributeFamilyType;
                added2.RecreateAttributeInstance(at);
                att.Attribute.Insert(added2, false);

                added2 = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
                added2.Name = "BrowsePath";
                path = BuildLibraryReference(ATLPrefix, uaNamespaceURI, "RelativePath");
                added2.RefAttributeType = BaseRefFromNodeId(RelativePathNodeId, ATLPrefix);

                var b = added2 as AttributeTypeType;
                RecurseStructures(ref b, RelativePathNodeId );
                
                att.Attribute.Insert(added2, false);


            }
            else if (nodeId == QualifiedNameNodeId)
            {
                att.AttributeDataType = "";
                AttributeType ns = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
                ns.Name = "NamespaceURI";
                ns.AttributeDataType = "xs:anyURI";
                att.Attribute.Insert(ns);
                AttributeType n = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
                n.Name = "Name";
                n.AttributeDataType = "xs:string";
                att.Attribute.Insert(n, false);
            }
            else if (nodeId == GuidNodeId)
            {
                att.AttributeDataType = "xs:string";
            }

        }

        private void FillSubAttributes(ref AttributeTypeType att, NodeId nodeId)
        {
            
                ProcessOptionSets(ref att, nodeId);
                ProcessEnumerations(ref att, nodeId);
                RecurseStructures(ref att, nodeId);
                

        }

        private void MakeBuiltInType(ref AttributeType a)
        {
            a.AttributeDataType = "xs:string";
            a.RefAttributeType = ATLPrefix + MetaModelName + "/BuiltInType";
            a.Constraint.Insert(BuiltInTypeConstraint());
        }


        private void MakeAttributeId(ref AttributeType a)
        {
            a.AttributeDataType = "xs:string";
            a.RefAttributeType = ATLPrefix + MetaModelName + "/AttributeId";
            a.Constraint.Insert(AttributeIdConstraint());
        }


        private void ProcessRelativePathElement(ref AttributeTypeType att )
        {
            // custom build the RelativePathElement to use ExplicitNodeId instead of NodeId
            var added2 = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
            added2.Name = "ReferenceTypeId";
            string path = BuildLibraryReference(ATLPrefix, MetaModelName, "ExplicitNodeId");
            var ob = m_cAEXDocument.FindByPath(path);
            var at = ob as AttributeFamilyType;
            added2.RecreateAttributeInstance(at);
            att.Attribute.Insert(added2, false);

        }



        private void RecurseStructures(ref AttributeTypeType att, NodeId nodeId)
        {
            if( nodeId == RelativePathElementNodeId)
            {
                ProcessRelativePathElement(ref att);
            }
            if (m_modelManager.IsTypeOf(nodeId, structureNode.DecodedNodeId))
            {
                att.AttributeDataType = "";
                var MyNode = FindNode<UANode>(nodeId) as NodeSet.UADataType;
                if (MyNode.Definition != null && MyNode.Definition.Field != null)
                {
                    if (MyNode.Definition.IsOptionSet == true && m_modelManager.IsTypeOf(nodeId, OptionSetStructureNodeId) == true)
                    {
                        AttributeType ValueField = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
                        ValueField.Name = "Value";
                        var attValue = att.Attribute.Insert(ValueField, false);
                        AttributeType ValidBitsField = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
                        ValidBitsField.Name = "ValidBits";
                        var attValidBits = att.Attribute.Insert(ValidBitsField, false);
                        for (int i = 0; i < MyNode.Definition.Field.Length; i++)
                        {
                            AttributeType a = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
                            a.Name = MyNode.Definition.Field[i].Name;
                            a.AttributeDataType = "xs:boolean";
                            attValue.Attribute.Insert(a, false);
                            attValidBits.Attribute.Insert(a, false);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < MyNode.Definition.Field.Length; i++)
                        {
                            AttributeType a = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
                            a.Name = MyNode.Definition.Field[i].Name;
                            if (att.Attribute[a.Name] == null)  // don't add the attribute if it already exists
                            {
                                a.RefAttributeType = BaseRefFromNodeId(MyNode.Definition.Field[i].DecodedDataType, ATLPrefix, false, 
                                    MyNode.Definition.Field[i].ValueRank >= ValueRanks.OneDimension );
                                a.AttributeDataType = GetAttributeDataType(MyNode.Definition.Field[i].DecodedDataType);
                                if (nodeId.NamespaceIndex == 0)
                                {
                                    if (a.Name == "BuiltInType")
                                        MakeBuiltInType(ref a);
                                    else if (a.Name == "AttributeId")
                                        MakeAttributeId(ref a);
                                }

                                if (MyNode.Definition.Field[i].ValueRank >= ValueRanks.OneDimension) // insert the first element in the list as a placeholder
                                {
                                    AttributeType aa = new AttributeType(new System.Xml.Linq.XElement(defaultNS + "Attribute"));
                                    aa.RefAttributeType = BaseRefFromNodeId(MyNode.Definition.Field[i].DecodedDataType, ATLPrefix, false, false);
                                    aa.AttributeDataType = GetAttributeDataType(MyNode.Definition.Field[i].DecodedDataType);
                                    var BaseNode = m_modelManager.FindNode<UANode>(MyNode.Definition.Field[i].DecodedDataType);
                                    if (BaseNode != null)
                                        aa.Name = BaseNode.DecodedBrowseName.Name;
                                    else
                                        aa.Name = "0";
                                    var b = aa as AttributeTypeType;
                                    FillSubAttributes(ref b, MyNode.Definition.Field[i].DecodedDataType);
                                    a.Attribute.Insert(aa, false);
                                }
                                else
                                {
                                    var b = a as AttributeTypeType;
                                    FillSubAttributes(ref b, MyNode.Definition.Field[i].DecodedDataType);
                                }
                                att.Attribute.Insert(a, false);
                            }
                        }
                    }
                }
            }
        }

        private AttributeFamilyType ProcessDataType(NodeSet.UANode node)
        {
            var typeNode = node as MarkdownProcessor.NodeSet.UADataType;

            if (typeNode.Purpose != MarkdownProcessor.NodeSet.DataTypePurpose.Normal)
                return null;
            if (ExcludedDataTypeList.Contains(typeNode.DecodedBrowseName.Name))
                return null;

            var added = new AttributeFamilyType(new System.Xml.Linq.XElement(defaultNS + "AttributeType"));
            var att = added as AttributeTypeType;
            added.Name = node.DecodedBrowseName.Name;
            added.ID = AmlIDFromNodeId(node.DecodedNodeId);
            m_atl_temp.AttributeType.Insert(added);

            added.AttributeDataType = GetAttributeDataType(node.DecodedNodeId);

            NodeId BaseNodeId = m_modelManager.FindFirstTarget(node.DecodedNodeId, HasSubTypeNodeId, false);
            if (BaseNodeId != null)
            {
                string s = BaseRefFromNodeId(BaseNodeId, ATLPrefix);
                added.RefAttributeType = s;
            }

            // Enumerations
            ProcessEnumerations(ref att, node.DecodedNodeId);
            // OptionSets
            ProcessOptionSets(ref att, node.DecodedNodeId);
            // structures     
            RecurseStructures(ref att, node.DecodedNodeId);
       

            return added;
        }
        #endregion

        #region INSTANCE

        private void CreateInstances()
        {
            // add an InstanceHierarchy to the ROOT CAEXFile element
            var myIH = m_cAEXDocument.CAEXFile.New_InstanceHierarchy("OPC UA Instance Hierarchy");
            AddLibraryHeaderInfo(myIH);

            var RootNode = FindNode<UANode>(RootNodeId);
            RecursiveAddModifyInstance<InstanceHierarchyType>(ref myIH, RootNode);

        }

        InternalElementType RecursiveAddModifyInstance<T>(ref T parent, UANode toAdd) where T : IInternalElementContainer
        {
            string amlId = AmlIDFromNodeId(toAdd.DecodedNodeId);
            string prefix = toAdd.DecodedBrowseName.Name;

            //first see if node already exists
            var ie = parent.InternalElement[toAdd.DecodedBrowseName.Name];
            if (ie == null)
            {
                SystemUnitFamilyType suc;
                
                var TypeDefNodeId = m_modelManager.FindFirstTarget(toAdd.DecodedNodeId, HasTypeDefinitionNodeId, true);
                var path = BaseRefFromNodeId(TypeDefNodeId, SUCPrefix);

                suc = m_cAEXDocument.FindByPath(path) as SystemUnitFamilyType;

                if (suc == null && toAdd.NodeClass == NodeClass.Method)
                {
                    suc = m_cAEXDocument.FindByPath( BuildLibraryReference( SUCPrefix, MetaModelName, MethodNodeClass)) as SystemUnitFamilyType;
                    Debug.Assert(suc != null);
                    // AddModifyAttribute(suc.Attribute, "Executable", "Boolean", true);  // #7 removed Executable an UserExecutable
                    // AddModifyAttribute(suc.Attribute, "UserExecutable", "Boolean", true);  // #7 removed Executable an UserExecutable
                }
                Debug.Assert(suc != null);

                // check if instance already exists before adding a new one  #11
                ie = (InternalElementType)m_cAEXDocument.FindByID(amlId);
                if (ie != null)
                    return ie;

                if ( prefix.StartsWith("http://"))
                {
                    prefix = WebUtility.UrlEncode(toAdd.DecodedBrowseName.Name);
                }

                ie = CreateClassInstanceWithIDReplacement(prefix + "_", suc);
                ie.ID = amlId;
                ie.Name = toAdd.DecodedBrowseName.Name;
                AddBaseNodeClassAttributes(ie.Attribute, toAdd);
                
                AttributeType a = ie.Attribute.Append("UaNodeNamespaceUri");  //bucket for the namespace URI of the node when present on an instance node
                a.AttributeDataType = "xs:anyURI";
                
                ie.Attribute["UaNodeNamespaceUri"].Value = m_modelManager.FindModelUri(toAdd.DecodedNodeId);

                parent.Insert(ie);

            }
            Debug.Assert(ie != null);

            // Just because the ie (InternalElement) was found in the parent does not mean that the ID was correctly set.
            // Set/reset the Id even though it might already be done correctly.
            ie.ID = amlId;

            // set the values to match the values in the nodeset
            if (toAdd.NodeClass == NodeClass.Variable)
            {  //  Set the datatype for Value
                var varnode = toAdd as NodeSet.UAVariable;
                bool bListOf = ( varnode.ValueRank >= ValueRanks.OneDimension ); // use ListOf when its a UA array

                AddModifyAttribute( ie.Attribute, "Value", varnode.DecodedDataType, varnode.DecodedValue, bListOf);
                var DataTypeNode = FindNode<UANode>( varnode.DecodedDataType );
                var sUADataType = DataTypeNode.DecodedBrowseName.Name;

                ie.SetAttributeValue("ValueRank", varnode.ValueRank);
                ie.SetAttributeValue("ArrayDimensions", varnode.ArrayDimensions);

            }

            // TODO set the values of the other attributes to match the instance ??

            // follow forward hierarchical references to add the children

            // now add the references and contained objects
            var nodeId = toAdd.DecodedNodeId;
            var refList = m_modelManager.FindReferences(nodeId);
            if (refList != null)
            {
                HashSet<string> foundInternalElements = new HashSet<string>();
                foreach (var reference in refList)
                {
                    if (reference.IsForward == true)
                    {
                        if (m_modelManager.IsTypeOf(reference.ReferenceTypeId, HierarchicalNodeId) == true )
                        {
                            string refURI = m_modelManager.FindModelUri(reference.ReferenceTypeId);
                            var ReferenceTypeNode = FindNode<UANode>(reference.ReferenceTypeId);
                            var sourceInterface = FindOrAddInterface(ref ie, refURI, ReferenceTypeNode.DecodedBrowseName.Name, nodeId);
                            var targetNode = FindNode<UANode>(reference.TargetId);
                           
                            if (reference.TargetId != TypesFolderNodeId)
                            {
                                var childIE = RecursiveAddModifyInstance<InternalElementType>(ref ie, targetNode);
                                foundInternalElements.Add(childIE.Name);
                                var destInterface = FindOrAddInterface(ref childIE, refURI, ReferenceTypeNode.DecodedBrowseName.Name + "]/[" + sourceInterface.Attribute["InverseName"].Value, targetNode.DecodedNodeId);
                                FindOrAddInternalLink(ref ie, sourceInterface.ID, destInterface.ID, targetNode.DecodedBrowseName.Name);
                            }
                        }
                    }
                }

                // Are there internal Elements that are not handled by the references?
                // Should they be removed?
                List<InternalElementType> internalElementsToRemove = new List<InternalElementType>();
                foreach (InternalElementType internalElement in ie.InternalElement)
                {
                    if (!foundInternalElements.Contains(internalElement.Name))
                    {
                        internalElementsToRemove.Add(internalElement);
                    }
                }

                foreach (InternalElementType internalElement in internalElementsToRemove)
                {
                    InternalLinkType link = ie.InternalLink[internalElement.Name];
                    if (link != null)
                    {
                        ExternalInterfaceType externalInterface = null;
                        foreach (ExternalInterfaceType external in internalElement.ExternalInterface)
                        {
                            if (external.ID.Equals(link.RefPartnerSideB))
                            {
                                externalInterface = external;
                                break;
                            }
                        }

                        if (externalInterface != null)
                        {
                            bool delete = true;
                            foreach (ExternalInterfaceType external in internalElement.ExternalInterface)
                            {
                                if (!external.Equals(externalInterface))
                                {
                                    if (external.ID.Equals(link.RefPartnerSideA))
                                    {
                                        delete = false;
                                        break;
                                    }
                                }
                            }

                            if (delete)
                            {
                                ie.ExternalInterface.RemoveElement(externalInterface);
                            }
                        }
                        ie.InternalLink.RemoveElement(link);
                        ie.InternalElement.RemoveElement(internalElement);
                    }
                }
            }

            RebuildExternalInterfaces(prefix + "_", ie);

            CompareLinksToExternaInterfaces(ie, ie);

            return ie;
        }

        // add an internal link if it does not already exist
        InternalLinkType FindOrAddInternalLink(ref InternalElementType ie, string sourceID, string destinationID, string linkName)        
        {
            foreach( var link in ie.InternalLink)
            {
                if (link.RefPartnerSideA == sourceID && link.RefPartnerSideB == destinationID)
                    return link;  // link already exists
                if (link.Name == linkName)
                {
                    ie.InternalLink.RemoveElement(link);
                }
            }
            // add a new link
            var internalLink = ie.New_InternalLink(linkName);
            internalLink.RefPartnerSideA = sourceID;
            internalLink.RefPartnerSideB = destinationID;
            return internalLink;
        }

        #endregion

    }
}