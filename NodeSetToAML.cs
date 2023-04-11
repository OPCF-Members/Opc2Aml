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
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.Text;
//using Microsoft.Office.Interop.Word;
// using Word = Microsoft.Office.Interop.Word;
using Opc.Ua;
using MarkdownProcessor.NodeSet;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using System.Reflection.Metadata.Ecma335;
using Opc.Ua.Export;
using UANode = MarkdownProcessor.NodeSet.UANode;
using UAType = MarkdownProcessor.NodeSet.UAType;
using UAVariable = MarkdownProcessor.NodeSet.UAVariable;
using System.Data;
using Aml.Engine.Adapter;
using System.Xml.Linq;
using System.ComponentModel;
using System.CodeDom;
// using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing.Text;
using Aml.Engine.AmlObjects.Extensions;
using System.Security.Cryptography.Xml;
using System.Net;
using NodeSetToAmlUtils;
using Org.BouncyCastle.Asn1.X500;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Asn1.Ocsp;
using static Opc.Ua.RelativePathFormatter;

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
        private readonly System.Xml.Linq.XNamespace defaultNS = "http://www.dke.de/CAEX";
        private const string uaNamespaceURI = "http://opcfoundation.org/UA/";
        private const string OpcLibInfoNamespace = "http://opcfoundation.org/UA/FX/2021/08/OpcUaLibInfo.xsd";
        private UANode structureNode;
        private readonly List<string> ExcludedDataTypeList = new List<string>() { "InstanceNode", "TypeNode" };

        private List<string> PreventInfiniteRecursionList = new List<string>();
        private const int ua2xslookup_count = 16;
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
            { "QualifiedName" , "xs:anyURI"  }

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

                AddLibaryHeaderInfo(m_atl_temp as CAEXBasicObject, modelInfo);

                // create the InterfaceClassLibrary
                var icl_temp = cAEXDocumentTemp.CAEXFile.InterfaceClassLib.Append(ICLPrefix + modelInfo.NamespaceUri);
                AddLibaryHeaderInfo(icl_temp as CAEXBasicObject, modelInfo);

                // Create the RoleClassLibrary
                var rcl_temp = cAEXDocumentTemp.CAEXFile.RoleClassLib.Append(RCLPrefix + modelInfo.NamespaceUri);
                // var rcl_temp = m_cAEXDocument.CAEXFile.RoleClassLib.Append(RCLPrefix + modelInfo.NamespaceUri);
                AddLibaryHeaderInfo(rcl_temp as CAEXBasicObject, modelInfo);

                // Create the SystemUnitClassLibrary
                var scl_temp =  cAEXDocumentTemp.CAEXFile.SystemUnitClassLib.Append(SUCPrefix + modelInfo.NamespaceUri);
                // var scl_temp = m_cAEXDocument.CAEXFile.SystemUnitClassLib.Append(SUCPrefix + modelInfo.NamespaceUri);
                AddLibaryHeaderInfo(scl_temp as CAEXBasicObject, modelInfo);

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
                        AddLibaryHeaderInfo(atl as CAEXBasicObject, modelInfo);
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
                            AddLibaryHeaderInfo(icl as CAEXBasicObject, modelInfo);
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
                            AddLibaryHeaderInfo(scl as CAEXBasicObject, modelInfo);
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
                            AddLibaryHeaderInfo(rcl as CAEXBasicObject, modelInfo);
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



        private void AddLibaryHeaderInfo(CAEXBasicObject bo, ModelInfo modelInfo = null)
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
            AddLibaryHeaderInfo(atl_meta as CAEXBasicObject);

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
            AddLibaryHeaderInfo(rcl_meta as CAEXBasicObject);

            // add meta model SUC
            var suc_meta = m_cAEXDocument.CAEXFile.SystemUnitClassLib.Append(SUCPrefix + MetaModelName);
            // add MethodNodeClass to the SUC
            var mb = suc_meta.New_SystemUnitClass(MethodNodeClass);
            mb.New_SupportedRoleClass(RCLPrefix + MetaModelName + "/" + UaBaseRole, false);
            AddLibaryHeaderInfo(suc_meta as CAEXBasicObject);


        }



        private void AddBaseNodeClassAttributes(IClassWithBaseClassReference owner, bool isAbstract)
        {
            // #9 remove unused attributes from base
            
          //  AttributeType a = owner.Attribute.Append("UaNodeNamespaceUri");  //bucket for the namespace URI of the node when present on an instance node
          //  a.AttributeDataType = "xs:anyURI";

          //  AddModifyAttribute(owner.Attribute, "BrowseName", "QualifiedName", Variant.Null);
          //  AddModifyAttribute(owner.Attribute, "DisplayName", "LocalizedText", Variant.Null);

          //  var abs = AddModifyAttribute(owner.Attribute, "IsAbstract", "Boolean", isAbstract);
            //   abs.Value = isAbstract ? "true" : "false";

           // AddModifyAttribute(owner.Attribute, "Description", "LocalizedText", Variant.Null);
          //  AddModifyAttribute(owner.Attribute, "WriteMask", "AttributeWriteMask", Variant.Null);
           // AddModifyAttribute(owner.Attribute, "RolePermissions", "ListOfRolePermissionType", Variant.Null);
          //  AddModifyAttribute(owner.Attribute, "AccessRestrictions", "AccessRestrictionType", Variant.Null);
            
        }

        private void SetBrowseNameUri( AttributeSequence seq, UANode uanode, UANode basenode = null)
        {
            // only set the value if different from the base node
            string baseuri = "";
            if (basenode != null )
              baseuri = m_modelManager.ModelNamespaceIndexes[basenode.DecodedBrowseName.NamespaceIndex].NamespaceUri;
            string myuri = m_modelManager.ModelNamespaceIndexes[uanode.DecodedBrowseName.NamespaceIndex].NamespaceUri;
            if (myuri != baseuri)
            {
                var browse = seq["BrowseName"];
                if (browse == null)
                {
                    browse = AddModifyAttribute(seq, "BrowseName", "QualifiedName", Variant.Null);
                }
                var uriatt = browse.Attribute["NamespaceURI"];
 
                uriatt.Value = myuri;

                if (uanode.DisplayName != null &&
                    uanode.DisplayName.Length > 0 &&
                    uanode.DisplayName[0].Value != uanode.DecodedBrowseName.Name)
                {
                    AddModifyAttribute(seq, "DisplayName", "LocalizedText",
                        uanode.DisplayName[0].Value);
                }
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

                    switch (val.TypeInfo.BuiltInType)
                    {
                        case BuiltInType.Boolean:
                        case BuiltInType.Byte:
                        case BuiltInType.SByte:
                        case BuiltInType.Int16:
                        case BuiltInType.Int32:
                        case BuiltInType.Int64:
                        case BuiltInType.DateTime:
                        case BuiltInType.String:
                        case BuiltInType.Guid:
                        case BuiltInType.Double:
                        case BuiltInType.Float:
                        case BuiltInType.Integer:
                        case BuiltInType.Number:
                        case BuiltInType.UInt16:
                        case BuiltInType.UInt32:
                        case BuiltInType.UInt64:
                        case BuiltInType.UInteger:
                            a.DefaultAttributeValue = a.AttributeValue = val;
                            break;

                        case BuiltInType.LocalizedText:
                            {
                                if (refDataType == "LocalizedText" && referenceName == "EnumStrings")
                                {
                                    AttributeTypeType attributeType = a as AttributeTypeType;
                                    if (attributeType != null)
                                    {
                                        AttributeValueRequirementType avrt = new AttributeValueRequirementType(
                                            new System.Xml.Linq.XElement(defaultNS + "Constraint"));
                                        avrt.Name = nodeName + " Constraint";
                                        attributeType.AttributeDataType = "xs:string";
                                        NominalScaledTypeType scaledType = avrt.New_NominalType();

                                        var values = val.Value as Opc.Ua.LocalizedText[];
                                        if (values != null)
                                        {
                                            foreach (var value in values)
                                            {
                                                scaledType.RequiredValue.Append(value.Text);
                                            }
                                            var res2 = attributeType.Constraint.Insert(avrt);
                                        }
                                    }
                                }
                                else
                                {
                                    // Do Something else
                                }

                                break;
                            }

                        case BuiltInType.ExtensionObject:
                            {
                                if (refDataType == "EnumValueType" && referenceName == "EnumValues")
                                {
                                    AttributeTypeType attributeType = a as AttributeTypeType;
                                    if (attributeType != null)
                                    {
                                        AttributeValueRequirementType avrt = new AttributeValueRequirementType(
                                            new System.Xml.Linq.XElement(defaultNS + "Constraint"));
                                        avrt.Name = nodeName + " Constraint";
                                        attributeType.AttributeDataType = "xs:string";
                                        NominalScaledTypeType scaledType = avrt.New_NominalType();

                                        var values = val.Value as Opc.Ua.ExtensionObject[];
                                        if (values != null)
                                        {
                                            foreach (var value in values)
                                            {
                                                var enumValueType = value.Body as Opc.Ua.EnumValueType;
                                                if (enumValueType != null)
                                                {
                                                    scaledType.RequiredValue.Append(enumValueType.DisplayName.Text);
                                                }
                                            }
                                            attributeType.Constraint.Insert(avrt);
                                        }
                                    }
                                }
                                else if (refDataType == "Argument")
                                {
                                    var values = val.Value as Opc.Ua.ExtensionObject[];
                                    if (values != null)
                                    {
                                        foreach (var value in values)
                                        {
                                            var argument = value.Body as Opc.Ua.Argument;
                                            if (argument != null)
                                            {
                                                //Debug.WriteLine("Argument " + name + ":" + refDataType + " " +
                                                //    " Method Name " + nodeName + " " + referenceName + " " +
                                                //    argument.Name + " Type " + argument.DataType.ToString());

                                            }
                                        }
                                    }

                                    return a;
                                }

                                break;
                            }

                        default:
                            Debug.WriteLine("Unhandled " + name + ":" + refDataType + " " +
                                " Node Name " + nodeName + " " + referenceName + " " +
                                " Type " + val.TypeInfo.BuiltInType.ToString());

                            break;
                    }
                }
                else
                {
                    switch (val.TypeInfo.BuiltInType)  // TODO -- consider supporting setting values for more complicated types (enums, structures, Qualified Names ...) and arrays
                    {
                        case BuiltInType.Boolean:
                        case BuiltInType.Byte:
                        case BuiltInType.SByte:
                        case BuiltInType.Int16:
                        case BuiltInType.Int32:
                        case BuiltInType.Int64:
                        case BuiltInType.DateTime:
                        case BuiltInType.String:
                        case BuiltInType.Guid:
                        case BuiltInType.Double:
                        case BuiltInType.Float:
                        case BuiltInType.Integer:
                        case BuiltInType.Number:
                        case BuiltInType.UInt16:
                        case BuiltInType.UInt32:
                        case BuiltInType.UInt64:
                        case BuiltInType.UInteger:
                            a.DefaultAttributeValue = a.AttributeValue = val;
                            break;

                        case BuiltInType.LocalizedText:
                            {
                                Opc.Ua.LocalizedText localizedText = (Opc.Ua.LocalizedText)val.Value;
                                if (localizedText != null && localizedText.Text != null)
                                {
                                    a.DefaultAttributeValue = a.AttributeValue = localizedText.Text;
                                }
                                break;
                            }
                    }
                }
            }

            return a;
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
                                    bool bListOf = (propertyNodeValue.ValueRank == 1);

                                    AddModifyAttribute( referenceElement.Attribute, 
                                        "Value",
                                        propertyNodeValue.DecodedDataType, 
                                        propertyNodeValue.DecodedValue, 
                                        bListOf );
                                }
                            }
                        }
                    }
                    else
                    {
                        // This is where my Class ValueAsType should go.
                        Debug.WriteLine("Update Derived cannot find " + referenceName + " for  " + internalElement.Name);
                    }
                }
            }
        }

        private AttributeType AddModifyAttribute(AttributeSequence seq, string name, NodeId refDataType, Variant val, bool bListOf = false)
        {
            var DataTypeNode = FindNode<UANode>(refDataType);
            var sUADataType = DataTypeNode.DecodedBrowseName.Name;
            var sURI = m_modelManager.FindModelUri(DataTypeNode.DecodedNodeId);
            return AddModifyAttribute(seq, name, sUADataType, val, bListOf, sURI);
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
                eit.Attribute.Insert(e);
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

            InternalElementSequence originalInternalElements = child.InternalElement;
            InternalElementSequence createdInternalElements = internalElementType.InternalElement;

            CreateClassInstanceUpdateChildIDs(prefix, originalInternalElements, createdInternalElements);
            CreateClassInstanceUpdateParentIDs(prefix, child, createdInternalElements);

            return internalElementType;
        }

        private void CreateClassInstanceUpdateChildIDs(
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
                        CreateClassInstanceUpdateChildIDs(childPrefix, 
                            originalInternalElement.InternalElement,
                            internalElementRequiresUpdate.InternalElement);
                    }
                }
            }
        }

        private void CreateClassInstanceUpdateParentIDs(
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
                                CreateClassInstanceUpdateChildIDs(addPrefix, more.InternalElement, type.InternalElement);
                            }
                        }
                        
                        CreateClassInstanceUpdateParentIDs(prefix, original.Reference, requiresUpdate);
                    }
                }
            }
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
                int startIndex = nodeIdString.IndexOf("_nsu%3D");
                if (startIndex > 0)
                {
                    nodeId = nodeIdString.Substring(startIndex + 1);
                }
            }
            return nodeId;
        }

        InternalElementType GetReferenceInternalElement(
            ref SystemUnitClassLibType scl, 
            ref RoleClassLibType rcl,
            SystemUnitFamilyType parent,
            NodeId typedefNodeId, NodeId targetId)
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

            // Never seen InternalElement not null
            var typeDefSequence = typeDefSucCreated.InternalElement;
            var targetCreated = CreateClassInstanceWithIDReplacement(prefix, targetChild);

            // Walk through the targetCreated, and only add those items that are missing in the typedefsuccreated.
            // TargetChild should never be returned.

            InternalElementSequence targetSequence = targetCreated.InternalElement;

            foreach (InternalElementType targetInternalElement in targetSequence)
            {
                InternalElementType typeDefInternalElement = typeDefSequence[targetInternalElement.Name];
                if (typeDefInternalElement == null)
                {
                    typeDefSucCreated.AddInstance(targetInternalElement);
                }
                else
                {
                    typeDefInternalElement.ID = targetInternalElement.ID;
                }
            }

            return typeDefSucCreated;
        }

        SystemUnitFamilyType FindOrAddSUC(ref SystemUnitClassLibType scl, ref RoleClassLibType rcl, NodeId nodeId)
        {
            var refnode = FindNode<NodeSet.UANode>(nodeId);
            string path = "";
            if (refnode.NodeClass != NodeClass.Method)
                path = BuildLibraryReference(SUCPrefix, m_modelManager.FindModelUri(refnode.DecodedNodeId), refnode.DecodedBrowseName.Name);
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
                    SetBrowseNameUri(rtn.Attribute, refnode, basenode);
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
                            AddBaseNodeClassAttributes(rtn, false);
                            // AddModifyAttribute(rtn.Attribute, "EventNotifier", "EventNotifierType", Variant.Null); // #9 remove unset attributes
                            SetBrowseNameUri(rtn.Attribute, refnode);
                            break;
                        case NodeClass.VariableType:
                            AddBaseNodeClassAttributes(rtn, true);
                            // AddModifyAttribute(rtn.Attribute, "ArrayDimensions", "ListOfUInt32", Variant.Null); // #9 remove unset attributes
                           // AddModifyAttribute(rtn.Attribute, "ValueRank", "Int32", -2); // #9 remove unset attributes
                           // AddModifyAttribute(rtn.Attribute, "Value", "BaseDataType", Variant.Null); // #9 remove unset attributes
                           // AddModifyAttribute(rtn.Attribute, "AccessLevel", "AccessLevelType", Variant.Null); // #9 remove unset attributes
                           // AddModifyAttribute(rtn.Attribute, "MinimumSamplingInterval", "Duration", Variant.Null); // #9 remove unset attributes
                            SetBrowseNameUri(rtn.Attribute, refnode);
                            break;
                        case NodeClass.Method:
                            AddBaseNodeClassAttributes(rtn, false);
                            SetBrowseNameUri(rtn.Attribute, refnode);

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
                                rtn.AddInstance(ie);

                                var basenode = FindNode<NodeSet.UANode>(TypeDefNodeId);                               
                                SetBrowseNameUri(ie.Attribute, targetNode, basenode);
                                if (targetNode.NodeClass == NodeClass.Variable)
                                {  //  Set the datatype for Value
                                    var varnode = targetNode as NodeSet.UAVariable;
                                    bool bListOf = (varnode.ValueRank == 1);  // use ListOf when its a UA array

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
                // add the attributes to the base ReferenceType
                //    AddBaseNodeClassAttributes(added.Attribute, true);


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
                att.AttributeDataType = "xs:string";
                var EnumStringsPropertyNode = FindNode<UANode>(EnumStringsPropertyId);
                var EnumStrings = EnumStringsPropertyNode as UAVariable;
                AttributeValueRequirementType avrt = new AttributeValueRequirementType(new System.Xml.Linq.XElement(defaultNS + "Constraint"));
                var MyNode = FindNode<UANode>(nodeId) as NodeSet.UADataType;
                avrt.Name = MyNode.DecodedBrowseName.Name + " Constraint";
                var res = avrt.New_NominalType();
                Opc.Ua.LocalizedText[] EnumValues = EnumStrings.DecodedValue.Value as Opc.Ua.LocalizedText[];
                foreach (var EnumValue in EnumValues)
                {
                    res.RequiredValue.Append(EnumValue.Text);
                }
                var res2 = att.Constraint.Insert(avrt);
                return;
            }

            NodeId EnumValuesPropertyId = m_modelManager.FindFirstTarget(nodeId, HasPropertyNodeId, true, "EnumValues");
            if (EnumValuesPropertyId != null)
            {
                att.AttributeDataType = "xs:string";
                var EnumValuesPropertyNode = FindNode<UANode>(EnumValuesPropertyId);
                var EnumValues = EnumValuesPropertyNode as UAVariable;
                AttributeValueRequirementType avrt = new AttributeValueRequirementType(new System.Xml.Linq.XElement(defaultNS + "Constraint"));
                var MyNode = FindNode<UANode>(nodeId) as NodeSet.UADataType;
                avrt.Name = MyNode.DecodedBrowseName.Name + " Constraint";
                var res = avrt.New_NominalType();

                var EnumVals = EnumValues.DecodedValue.Value as Opc.Ua.ExtensionObject[];
                foreach (var EnumValue in EnumVals)
                {
                    var ev = EnumValue.Body as Opc.Ua.EnumValueType;
                    res.RequiredValue.Append(ev.DisplayName.Text);
                }
                var res2 = att.Constraint.Insert(avrt);
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
                                a.RefAttributeType = BaseRefFromNodeId(MyNode.Definition.Field[i].DecodedDataType, ATLPrefix, false, MyNode.Definition.Field[i].ValueRank == 1);
                                a.AttributeDataType = GetAttributeDataType(MyNode.Definition.Field[i].DecodedDataType);
                                if (nodeId.NamespaceIndex == 0)
                                {
                                    if (a.Name == "BuiltInType")
                                        MakeBuiltInType(ref a);
                                    else if (a.Name == "AttributeId")
                                        MakeAttributeId(ref a);
                                }

                                if (MyNode.Definition.Field[i].ValueRank == 1) // insert the first element in the list as a placeholder
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
            AddLibaryHeaderInfo(myIH);

            var RootNode = FindNode<UANode>(RootNodeId);
            RecursiveAddModifyInstance<InstanceHierarchyType>(ref myIH, RootNode);

        }

        InternalElementType RecursiveAddModifyInstance<T>(ref T parent, UANode toAdd) where T : IInternalElementContainer
        {
            string amlId = AmlIDFromNodeId(toAdd.DecodedNodeId);

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
                    AddBaseNodeClassAttributes(suc, false);
                    // AddModifyAttribute(suc.Attribute, "Executable", "Boolean", true);  // #7 removed Executable an UserExecutable
                    // AddModifyAttribute(suc.Attribute, "UserExecutable", "Boolean", true);  // #7 removed Executable an UserExecutable
                }
                Debug.Assert(suc != null);

                // check if instance already exists before adding a new one  #11
                ie = (InternalElementType)m_cAEXDocument.FindByID(amlId);
                if (ie != null)
                    return ie;

                string prefix = toAdd.DecodedBrowseName.Name;
                if ( prefix.StartsWith("http://"))
                {
                    prefix = WebUtility.UrlEncode(toAdd.DecodedBrowseName.Name);
                }

                ie = CreateClassInstanceWithIDReplacement(prefix + "_", suc);
                ie.Name = toAdd.DecodedBrowseName.Name;
                SetBrowseNameUri(ie.Attribute, toAdd);
                
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
                bool bListOf = (varnode.ValueRank == 1);  // use ListOf when its a UA array

                AddModifyAttribute(ie.Attribute, "Value", varnode.DecodedDataType, varnode.DecodedValue, bListOf);
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
                                var destInterface = FindOrAddInterface(ref childIE, refURI, ReferenceTypeNode.DecodedBrowseName.Name + "]/[" + sourceInterface.Attribute["InverseName"].Value, targetNode.DecodedNodeId);
                                FindOrAddInternalLink(ref ie, sourceInterface.ID, destInterface.ID, targetNode.DecodedBrowseName.Name);
                            }
                        }
                    }
                }
            }
            return ie;
        }

        // add an internal link if it does not already exist
        InternalLinkType FindOrAddInternalLink(ref InternalElementType ie, string sourceID, string destinationID, string linkName)        
        {
            foreach( var link in ie.InternalLink)
            {
                if (link.RefPartnerSideA == sourceID && link.RefPartnerSideB == destinationID)
                    return link;  // link already exists
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