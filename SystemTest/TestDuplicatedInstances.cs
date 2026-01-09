using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Aml.Engine.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace SystemTest
{
    [TestClass]
    public class TestDuplicatedInstances
    {
        const string ControllerNodeSetFile = "Opc.Ua.Fx.Show.Controller.xml";
        const string BottleMachineNodeSetFile = "Opc.Ua.Fx.Show.BottleMachine.xml";

        Dictionary<string, SystemUnitClassType> Locations = null;


        #region Tests

        // The IOP model is validated in the Validate test module.

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow("Opc.Ua.Fx.IOPModel.xml.amlx")]
        [DataRow("Opc.Ua.Fx.Show.BottleMachine.xml.amlx")]
        [DataRow("Opc.Ua.Fx.Show.Controller.xml.amlx")]

        public void FindDuplicates(string fileName)
        {
            CAEXDocument document = GetDocument(fileName);

            HashSet<string> set = new HashSet<string>();
            Dictionary<string, int> duplicates = new Dictionary<string, int>();

            foreach (InstanceHierarchyType type in document.CAEXFile.InstanceHierarchy)
            {
                foreach (InternalElementType internalElement in type.InternalElement)
                {
                    SystemUnitClassType classType = internalElement as SystemUnitClassType;
                    if (classType != null)
                    {
                        WalkInstance(classType, set, duplicates);
                    }
                }
            }

            Console.WriteLine($"Container {fileName} hase {set.Count} instances");

            if ( duplicates.Count > 0 )
            {
                foreach(KeyValuePair<string, int> pair in duplicates)
                {
                    Console.WriteLine($"Duplicate Id: {pair.Key} found {pair.Value} times.");
                }
                Assert.AreEqual(0, duplicates.Count, "Found " + duplicates.Count + " Duplicate Ids");
            }
        }

        public void WalkInstance( SystemUnitClassType instance, 
            HashSet<string> set,
            Dictionary<string, int> duplicates )
        {
            if ( set.TryGetValue(instance.ID, out _))
            {
                if (duplicates.TryGetValue(instance.ID, out int count))
                {
                    duplicates[instance.ID] = count + 1;
                }
                else
                {
                    // Strange, but accurate.  Just not tracking exactly where.
                    duplicates[instance.ID] = 2;
                }
            }
            else
            {
                set.Add(instance.ID);
            }

            foreach (InternalElementType child in instance.InternalElement)
            {
                WalkInstance(child as SystemUnitClassType, set, duplicates);
            }
        }

        // Look for specifics.
        // Problem was discovered in the Controller where the FxShowBottleMachine and
        // FxRoot\BottleController\FunctionalEntities\BottleMachine has both InputData 
        // and OutputData referencing the same instances.
        // FxShowBottleMachine should have many HasComponents to the instances
        // FxRoot\BottleController\FunctionalEntities\BottleMachine Inputs and Outputs should
        // have organizes.
        // Ideally that the FxShowBottleMachine should have the actual components
        // and the BottleMachine should have references to them.
        // This happens because of the way that references are processed in order, from the lowest
        // namespace index to the highest.

        // Use the Controller test file for the remainder of the tests

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]

        public void TestHasComponent()
        {
            CAEXDocument document = GetDocument("Opc.Ua.Fx.Show.Controller.xml.amlx");

            Dictionary<string, SystemUnitClassType> locations = GetLocations(document);
            SystemUnitClassType fxShowBottleMachine = locations["FxShowBottleMachineId"];

            // Ensure the proper ExternalInterface Exists
            ExternalInterfaceType hasComponent = fxShowBottleMachine.ExternalInterface["HasComponent"];
            Assert.IsNotNull(hasComponent);

            DirectoryInfo outputDirectoryInfo = TestHelper.GetOpc2AmlDirectory();
            FileInfo nodeSetFileInfo = new FileInfo(Path.Combine(outputDirectoryInfo.FullName, BottleMachineNodeSetFile));
            Assert.IsTrue(nodeSetFileInfo.Exists);

            // need to check reverses as well
            XmlDocument doc = new XmlDocument();
            doc.Load(nodeSetFileInfo.FullName);

            XmlNode node = doc.SelectSingleNode("//*[local-name()='UAObject' and @NodeId='ns=1;i=5020']");
            Assert.IsNotNull(node);
            XmlNode references = node["References"];
            Assert.IsNotNull(references);
            Assert.IsNotNull(references.ChildNodes);
            Assert.AreNotEqual(0, references.ChildNodes.Count);
            foreach (XmlNode referenceNode in references.ChildNodes)
            {
                XmlAttribute typeAttribute = referenceNode.Attributes["ReferenceType"];
                if (typeAttribute != null)
                {
                    if ( typeAttribute.InnerText.Equals("HasComponent"))
                    {
                        NodeId nodeId = new NodeId( referenceNode.InnerText );
                        string nodeIdIdentifier = nodeId.Identifier.ToString();
                        string amlId = TestHelper.BuildAmlId("", TestHelper.Uris.ShowBottleMachine, nodeIdIdentifier );
                        CAEXObject caexObject = document.FindByID( amlId );
                        Assert.IsNotNull( caexObject);
                        SystemUnitClassType component = caexObject as SystemUnitClassType;
                        Assert.IsNotNull( component );
                        Console.WriteLine($"Testing {component.Name}");

                        // Ensure the proper external interface to the target
                        ExternalInterfaceType externalInterface = component.ExternalInterface["ComponentOf"];
                        Assert.IsNotNull( externalInterface );

                        // Ensure there is an Internal Link from the parent to the target
                        InternalLinkType internalLink = fxShowBottleMachine.InternalLink[component.Name];
                        Assert.IsNotNull( internalLink );

                        Assert.AreEqual(hasComponent.ID, internalLink.RefPartnerSideA);
                        Assert.AreEqual(externalInterface.ID, internalLink.RefPartnerSideB);

                        // Ensure the parent of the target is as expected
                        // This is based off current operations, and could change
                        // if the reference list is sorted in RecursiveAddModifyInstances
                        SystemUnitClassType parent = locations[component.Name];

                        Assert.IsNotNull( parent );

                        SystemUnitClassType componentParent = component.CAEXParent as SystemUnitClassType;
                        Assert.IsNotNull(componentParent);

                        Assert.AreEqual(parent.ID, componentParent.ID);
                    }
                }
            }
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow("OutputData", "5017")]
        [DataRow("InputData", "5016")]

        public void TestOrganizes(string organizesName, string organizesId)
        {
            CAEXDocument document = GetDocument("Opc.Ua.Fx.Show.Controller.xml.amlx");

            Dictionary<string, SystemUnitClassType> locations = GetLocations(document);
            SystemUnitClassType organizedElement = locations[organizesName];

            // Ensure the proper ExternalInterface Exists
            ExternalInterfaceType organizesInterface = organizedElement.ExternalInterface["Organizes"];
            Assert.IsNotNull(organizesInterface);

            DirectoryInfo outputDirectoryInfo = TestHelper.GetOpc2AmlDirectory();
            FileInfo nodeSetFileInfo = new FileInfo(Path.Combine(outputDirectoryInfo.FullName, ControllerNodeSetFile));
            Assert.IsTrue(nodeSetFileInfo.Exists);

            // need to check reverses as well
            XmlDocument doc = new XmlDocument();
            doc.Load(nodeSetFileInfo.FullName);

            XmlNode node = doc.SelectSingleNode("//*[local-name()='UAObject' and @NodeId='ns=1;i=" + organizesId + "']");
            Assert.IsNotNull(node);
            XmlNode references = node["References"];
            Assert.IsNotNull(references);
            Assert.IsNotNull(references.ChildNodes);
            Assert.AreNotEqual(0, references.ChildNodes.Count);
            foreach (XmlNode referenceNode in references.ChildNodes)
            {
                XmlAttribute typeAttribute = referenceNode.Attributes["ReferenceType"];
                if (typeAttribute.InnerText.Equals("Organizes"))
                {
                    NodeId nodeId = new NodeId(referenceNode.InnerText);
                    string nodeIdIdentifier = nodeId.Identifier.ToString();
                    string amlId = TestHelper.BuildAmlId("", TestHelper.Uris.ShowBottleMachine, nodeIdIdentifier);
                    CAEXObject caexObject = document.FindByID(amlId);
                    Assert.IsNotNull(caexObject);
                    SystemUnitClassType component = caexObject as SystemUnitClassType;
                    Assert.IsNotNull(component);
                    Console.WriteLine($"Testing {component.Name}");

                    // Ensure the proper external interface to the target
                    ExternalInterfaceType externalInterface = component.ExternalInterface["OrganizedBy"];
                    Assert.IsNotNull(externalInterface);

                    // Ensure there is an Internal Link from the parent to the target
                    InternalLinkType internalLink = organizedElement.InternalLink[component.Name];
                    Assert.IsNotNull(internalLink);

                    Assert.AreEqual(organizesInterface.ID, internalLink.RefPartnerSideA);
                    Assert.AreEqual(externalInterface.ID, internalLink.RefPartnerSideB);

                    // Ensure the parent of the target is as expected
                    // This is based off current operations, and could change
                    // if the reference list is sorted in RecursiveAddModifyInstances
                    SystemUnitClassType parent = locations[component.Name];

                    Assert.IsNotNull(parent);

                    SystemUnitClassType componentParent = component.CAEXParent as SystemUnitClassType;
                    Assert.IsNotNull(componentParent);

                    Assert.AreEqual(parent.ID, componentParent.ID);
                }

            }



        }

        public Dictionary<string,SystemUnitClassType> GetLocations(CAEXDocument document)
        {
            if ( Locations == null )
            {
                Locations = new Dictionary<string, SystemUnitClassType>();

                SystemUnitClassType fxShowBottleMachine = GetSystemUnitClass( document, 
                    TestHelper.Uris.ShowBottleMachine, "5020");
                Locations.Add("FxShowBottleMachineId", fxShowBottleMachine);

                SystemUnitClassType inputData = GetSystemUnitClass(document,
                    TestHelper.Uris.ShowController, "5016");
                Locations.Add("InputData", inputData);

                SystemUnitClassType outputData = GetSystemUnitClass(document,
                    TestHelper.Uris.ShowController, "5017");
                Locations.Add("OutputData", outputData);

                Locations.Add("BottleSpeed", fxShowBottleMachine);
                Locations.Add("MachineIdentification", fxShowBottleMachine);
                Locations.Add("MachineType", fxShowBottleMachine);
                Locations.Add("ResetBottleId", fxShowBottleMachine);

                Locations.Add("BottleIdIn", inputData);
                Locations.Add("BottleMaterialIn", inputData);
                Locations.Add("BottleSizeIn", inputData);
                Locations.Add("CapColorIn", inputData);
                Locations.Add("CapperIdentificationIn", inputData);
                Locations.Add("FillerIdentificationIn", inputData);
                Locations.Add("LabelDesignIn", inputData);
                Locations.Add("LiquidTypeIn", inputData);
                Locations.Add("WasherIdentificationIn", inputData);

                Locations.Add("BottleId", outputData);
                Locations.Add("BottleMaterial", outputData);
                Locations.Add("BottleSize", outputData);
                Locations.Add("CapColor", outputData);
                Locations.Add("CapperIdentification", outputData);
                Locations.Add("FillerIdentification", outputData);
                Locations.Add("LabelDesign", outputData);
                Locations.Add("LiquidType", outputData);
                Locations.Add("WasherIdentification", outputData);
            }
            return Locations;
        }

        public SystemUnitClassType GetSystemUnitClass( CAEXDocument document, TestHelper.Uris uri, string idString )
        {
            string id = TestHelper.BuildAmlId("", uri, idString);
            CAEXObject theObject = document.FindByID(id);
            Assert.IsNotNull(theObject);
            SystemUnitClassType systemUnitClass = theObject as SystemUnitClassType;
            Assert.IsNotNull(systemUnitClass);
            return systemUnitClass;
        }

        #endregion

        #region Helpers

        private CAEXDocument GetDocument(string fileName)
        {
            CAEXDocument document = TestHelper.GetReadOnlyDocument( fileName );
            Assert.IsNotNull(document, "Unable to retrieve Document" );
            return document;
        }

        #endregion

    }
}
