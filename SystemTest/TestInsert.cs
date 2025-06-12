using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using Aml.Engine.CAEX;
using Aml.Engine.AmlObjects;

namespace SystemTest
{
    [TestClass]

    public class TestInsert
    {
        #region Tests

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow("Minimal.xml.amlx")]
        [DataRow("OneVariable.xml.amlx")]
        public void Existing(string fileName)
        {
            DirectoryInfo outputDirectoryInfo = TestHelper.GetOpc2AmlDirectory();
            FileInfo existingFile = new FileInfo( 
                Path.Combine( outputDirectoryInfo.FullName, fileName) );
            Assert.IsTrue(existingFile.Exists);

            TestUris(fileName, shouldExist: false);
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        [DataRow("Minimal.xml", "MinimalInserted")]
        [DataRow("OneVariable.xml", "OneVariableInserted")]
        public void Simple(string inputFile, string outputFile)
        {
            DirectoryInfo outputDirectoryInfo = TestHelper.GetOpc2AmlDirectory();
            FileInfo existingFile = new FileInfo(
                Path.Combine(outputDirectoryInfo.FullName, outputFile + ".amlx"));
            if (existingFile.Exists)
            {
                existingFile.Delete();
            }
            existingFile.Refresh();
            Assert.IsFalse(existingFile.Exists);
            string commandLine = "--Nodeset " + inputFile +
                " --Output " + outputFile +
                " --Insert " + TestHelper.GetUri(TestHelper.Uris.Ac);

            TestHelper.Execute(commandLine, expectedResult: 0);

            // Need to crack it now.
            existingFile.Refresh();
            Assert.IsTrue(existingFile.Exists);

            //  Now test it has the proper nodesets
            TestUris(existingFile.Name, shouldExist: true);
        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        public void TestMissingAll()
        {
            // Create a Directory to work in
            DirectoryInfo outputDirectoryInfo = TestHelper.GetOpc2AmlDirectory();
            string missingAll = "MissingAll";
            DirectoryInfo missingAllDirectoryInfo = new DirectoryInfo(
                Path.Combine(outputDirectoryInfo.FullName, missingAll));
            if ( !missingAllDirectoryInfo.Exists )
            {
                missingAllDirectoryInfo.Create();
                missingAllDirectoryInfo.Refresh();
            }

            FileInfo inputFile = new FileInfo(
                Path.Combine(missingAllDirectoryInfo.FullName, "OneVariable.xml"));

            if (!inputFile.Exists)
            {
                FileInfo sourceFile = new FileInfo(
                    Path.Combine(outputDirectoryInfo.FullName, "OneVariable.xml"));
                Assert.IsTrue(sourceFile.Exists);
                sourceFile.CopyTo(inputFile.FullName, true);
            }

            FileInfo outputFile = new FileInfo(
                Path.Combine(missingAllDirectoryInfo.FullName, "OneVariable.xml.amlx"));

            if (outputFile.Exists)
            {
                outputFile.Delete();
                outputFile.Refresh();
            }

            string commandLine = " --DirectoryInfo " + missingAllDirectoryInfo.FullName + 
                "--Nodeset " + inputFile +
                " --Insert " + TestHelper.GetUri(TestHelper.Uris.Ac);

            TestHelper.Execute(commandLine, expectedResult: 1);

        }

        [TestMethod, Timeout(TestHelper.UnitTestTimeout)]
        public void TestMissingOne()
        {
            // Create a Directory to work in
            DirectoryInfo outputDirectoryInfo = TestHelper.GetOpc2AmlDirectory();
            string missingOne = "MissingOne";
            DirectoryInfo missingOneDirectoryInfo = new DirectoryInfo(
                Path.Combine(outputDirectoryInfo.FullName, missingOne));
            if (!missingOneDirectoryInfo.Exists)
            {
                missingOneDirectoryInfo.Create();
                missingOneDirectoryInfo.Refresh();
            }

            List<string> copyFiles = new List<string>
            {
                "OneVariable.xml",
                "Modified.Opc.Ua.NodeSet2.xml",
                "Opc.Ua.Di.NodeSet2.xml",
                //"Opc.Ua.fx.data.nodeset2.xml",  Deliberately not copied
                "Opc.Ua.fx.ac.nodeset2.xml"
            };

            foreach (string copyFile in copyFiles)
            {
                FileInfo inputFile = new FileInfo(
                    Path.Combine(missingOneDirectoryInfo.FullName, copyFile));

                if (!inputFile.Exists)
                {
                    FileInfo sourceFile = new FileInfo(
                        Path.Combine(outputDirectoryInfo.FullName, copyFile));
                    Assert.IsTrue(sourceFile.Exists);
                    sourceFile.CopyTo(inputFile.FullName, true);
                }
            }

            FileInfo outputFile = new FileInfo(
                Path.Combine(missingOneDirectoryInfo.FullName, "OneVariable.xml.amlx"));

            if (outputFile.Exists)
            {
                outputFile.Delete();
                outputFile.Refresh();
            }

            string commandLine = " --DirectoryInfo " + missingOneDirectoryInfo.FullName +
                "--Nodeset OneVariable.xml" +
                " --Insert " + TestHelper.GetUri(TestHelper.Uris.Ac);

            TestHelper.Execute(commandLine, expectedResult: 1);
        }

        #endregion

        public void TestUris(string fileName, bool shouldExist)
        {
            DirectoryInfo outputDirectoryInfo = TestHelper.GetOpc2AmlDirectory();
            FileInfo existingFile = new FileInfo(
                Path.Combine(outputDirectoryInfo.FullName, fileName));
            Assert.IsTrue(existingFile.Exists);

            AutomationMLContainer container = new AutomationMLContainer(existingFile.FullName,
                System.IO.FileMode.Open, FileAccess.Read);
            Assert.IsNotNull(container, "Unable to find container");
            CAEXDocument document = CAEXDocument.LoadFromStream(container.RootDocumentStream());
            Assert.IsNotNull(document, "Unable to find document");

            bool found = false;

            string searchFor = "SUC_" + TestHelper.GetUri(TestHelper.Uris.Ac);
            foreach (SystemUnitClassLibType systemUnitClass in document.CAEXFile.SystemUnitClassLib)
            {
                if (systemUnitClass.Name.Equals(searchFor))
                {
                    found = true;
                    break;
                }
            }

            Assert.AreEqual(shouldExist, found);
        }
    }
}
