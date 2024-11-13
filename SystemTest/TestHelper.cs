using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace SystemTest
{
    internal class TestHelper
    {
        public const string ExtractPrefix = "Extract_";
        public const string Opc2AmlName = "Opc2AmlConsole";
        public const string Opc2Aml = Opc2AmlName + ".exe";

        public const string TestAmlUri = "http://opcfoundation.org/UA/FX/AML/TESTING";

        static bool Executed = false;

        static Dictionary<string, CAEXDocument> ReadOnlyDocuments = new Dictionary<string, CAEXDocument>();
        
        public static List<FileInfo> RetrieveFiles()
        {
            if (!Executed)
            {
                const bool EXECUTE_CONVERSION = true;

                List<string> testFiles = GetTestFileNames();
                if (EXECUTE_CONVERSION)
                {
                    PrepareUnconvertedXml(testFiles);
                    System.Threading.Thread.Sleep(1000);
                    Execute();
                }
                List<FileInfo> amlFiles = ExtractAmlxFiles( testFiles );
                Assert.AreNotEqual(0, amlFiles.Count, "Unable to get Converted Aml files");
                Executed = true;
            }
            List<FileInfo> amlxFiles = GetAmlxFiles();
            Assert.AreNotEqual(0, amlxFiles.Count, "Unable to get Converted Amlx files");
            return amlxFiles;
        }

        public static CAEXDocument GetReadOnlyDocument( string filename )
        {
            CAEXDocument document = null;

            if ( !ReadOnlyDocuments.TryGetValue( filename, out document ) )
            {
                foreach( FileInfo fileInfo in RetrieveFiles() )
                {
                    if( fileInfo.Name.Equals( filename ) )
                    {
                        AutomationMLContainer container = new AutomationMLContainer( fileInfo.FullName,
                            System.IO.FileMode.Open, FileAccess.Read );
                        Assert.IsNotNull( container, "Unable to find container" );
                        document = CAEXDocument.LoadFromStream( container.RootDocumentStream() );
                        Assert.IsNotNull( document, "Unable to find document" );

                        ReadOnlyDocuments.Add( filename, document );
                    }
                }
            }

            return document;
        }

        static public string GetRootName()
        {
            return "nsu%3Dhttp%3A%2F%2Fopcfoundation.org%2FUA%2FFX%2FAML%2FTESTING%3Bi%3D";
        }

        static public string GetOpcRootName()
        {
            return "nsu%3Dhttp%3A%2F%2Fopcfoundation.org%2FUA%2F%3Bi%3D";
        }

        static public string GetConfigurationPath()
        {
            string systemTestDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo systemTestProjectDirectoryInfo = GetSystemTestProjectDirectory();

            return systemTestDirectory.Substring(systemTestProjectDirectoryInfo.FullName.Length);
        }

        static public DirectoryInfo GetSystemTestProjectDirectory()
        {
            string systemTestDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo systemTestDirectoryInfo = new DirectoryInfo(systemTestDirectory);
            DirectoryInfo configurationDirectoryInfo = systemTestDirectoryInfo.Parent;
            DirectoryInfo binDirectoryInfo = configurationDirectoryInfo.Parent;
            DirectoryInfo systemTestProjectDirectoryInfo = binDirectoryInfo.Parent;

            return systemTestProjectDirectoryInfo;
        }

        static public DirectoryInfo GetRootDirectory()
        {
            string systemTestDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo systemTestDirectoryInfo = new DirectoryInfo(systemTestDirectory);
            DirectoryInfo configurationDirectoryInfo = systemTestDirectoryInfo.Parent;
            DirectoryInfo binDirectoryInfo = configurationDirectoryInfo.Parent;
            DirectoryInfo systemTestProjectDirectoryInfo = binDirectoryInfo.Parent;
            DirectoryInfo projectDirectoryInfo = systemTestProjectDirectoryInfo.Parent;

            return projectDirectoryInfo;
        }

        static public DirectoryInfo GetOpc2AmlDirectory()
        {
            DirectoryInfo rootDirectoryInfo = GetRootDirectory();
            string configurationPath = GetConfigurationPath();
            // Doesn't work https://stackoverflow.com/questions/53102/why-does-path-combine-not-properly-concatenate-filenames-that-start-with-path-di
            //string exeDirectory = Path.Combine(rootDirectoryInfo.FullName, configurationPath);
            string exeDirectory = rootDirectoryInfo.FullName + "\\" + Opc2AmlName + configurationPath;
            return new DirectoryInfo(exeDirectory);
        }

        static public DirectoryInfo GetTestFileDirectory()
        {
            string systemTestDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo systemTestDirectoryInfo = new DirectoryInfo(systemTestDirectory);
            string testFileDirectory = systemTestDirectoryInfo.FullName + "\\NodeSetFiles";
            return new DirectoryInfo(testFileDirectory);
        }

        static public List<string> GetTestFileNames()
        {
            List<string> testFileNames = new List<string>();

            DirectoryInfo directoryInfo = GetTestFileDirectory();

            foreach (FileInfo file in directoryInfo.GetFiles("*.xml"))
            {
                testFileNames.Add(file.Name);
            }

            return testFileNames;
        }

        static public void PrepareUnconvertedXml(List<string> xmlFiles)
        {
            DirectoryInfo outputDirectoryInfo = GetOpc2AmlDirectory();
            DirectoryInfo testFileDirectoryInfo = GetTestFileDirectory();

            foreach ( string xmlFile in xmlFiles)
            {
                CleanupXml(xmlFile);
                File.Copy(Path.Combine(testFileDirectoryInfo.FullName, xmlFile), 
                    Path.Combine(outputDirectoryInfo.FullName, xmlFile), overwrite: true);  
            }
        }

        static public string ExecutableName()
        {
            DirectoryInfo outputDirectoryInfo = GetOpc2AmlDirectory();
            return Path.Combine(outputDirectoryInfo.FullName, Opc2Aml);
        }

        static public bool Execute( string arguments = "", int expectedResult = 0 )
        {
            bool success = true;

            string executableName = ExecutableName();
            ProcessStartInfo processStartInfo = new ProcessStartInfo(executableName);
            Assert.IsNotNull(processStartInfo, "Unable to create ProcessStartInfo");
            processStartInfo.WorkingDirectory = GetOpc2AmlDirectory().FullName;
            processStartInfo.Arguments = "-- SuppressPrompt"; 
            if ( arguments.Length > 0 )
            {
                processStartInfo.Arguments += arguments;
            }

            Process opc2amlProcess = Process.Start(processStartInfo);
            
            int counter = 0;
            int limit = 150;
            while( !opc2amlProcess.HasExited && counter < limit )
            {
                System.Threading.Thread.Sleep(1000);
                counter++;
                if ( counter >= limit )
                {
                    success = false;
                    opc2amlProcess.Kill();
                }
            }

            Assert.AreEqual(expectedResult, opc2amlProcess.ExitCode);
            Assert.IsTrue(success, "Conversion tool exceeded time limit");

            return success;
        }

        static public void CleanupXml(string xmlFileName)
        {
            DirectoryInfo outputDirectoryInfo = GetOpc2AmlDirectory();

            FileInfo xmlFileInfo = new FileInfo(Path.Combine(outputDirectoryInfo.FullName, xmlFileName));
            if (xmlFileInfo.Exists)
            {
                File.Delete(xmlFileInfo.FullName);
            }

            FileInfo amlxFileInfo = new FileInfo(xmlFileInfo.FullName + ".amlx");
            if (amlxFileInfo.Exists)
            {
                File.Delete(amlxFileInfo.FullName);
            }

            DirectoryInfo unzipped = GetExtractDirectory(amlxFileInfo.Name);

            if(unzipped.Exists)
            {
                Directory.Delete(unzipped.FullName, recursive: true);
            }
        }

        static public DirectoryInfo GetExtractDirectory(string xmlFileName)
        {
            DirectoryInfo outputDirectoryInfo = GetOpc2AmlDirectory();
            return new DirectoryInfo(Path.Combine(outputDirectoryInfo.FullName, ExtractPrefix + xmlFileName));
        }

        static public List<FileInfo> GetAmlxFiles()
        {
            DirectoryInfo outputDirectoryInfo = GetOpc2AmlDirectory();

            return outputDirectoryInfo.GetFiles("*.amlx").ToList<FileInfo>();
        }

        static public List<FileInfo> ExtractAmlxFiles(List<string> testFiles)
        {
            List<FileInfo> result = new List<FileInfo>();

            foreach(string testFile in testFiles)
            {
                result.Add(ExtractAmlx(testFile));
            }
            return result;
        }


        static public FileInfo ExtractAmlx(string fileName)
        {
            string amlxFile = fileName + ".amlx";
            string amlFile = fileName + ".aml";

            DirectoryInfo outputDirectoryInfo = GetOpc2AmlDirectory();
            FileInfo extractFile = new FileInfo(Path.Combine(outputDirectoryInfo.FullName, amlxFile));
            Assert.IsTrue(extractFile.Exists, "Unable to find expected file " + amlxFile );

            DirectoryInfo extractedDirectory = GetExtractDirectory(amlxFile);

            ZipFile.ExtractToDirectory(extractFile.FullName, extractedDirectory.FullName, overwriteFiles: true);

            extractedDirectory.Refresh();

            Assert.IsTrue(extractedDirectory.Exists, "Unzip " + amlxFile + " Failed");

            string amlFullPath = Path.Combine(extractedDirectory.FullName, amlFile);

            FileInfo resultingAmlFile = new FileInfo(amlFullPath);
            Assert.IsTrue(resultingAmlFile.Exists, "Expected Aml " + amlFile + " Does not exist");

            return resultingAmlFile;
        }

        static public bool WriteFile(string fileName, List<string> lines)
        {
            bool success = false;

            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName);

                for (int index = 0; index < lines.Count; index++)
                {
                    writer.WriteLine(lines[index]);
                }

                writer.Close();

                success = true;
            }
            catch
            {
            }

            return success;
        }

    }
}
