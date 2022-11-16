using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace SystemTest
{
    internal class TestHelper
    {
        public const string ExtractPrefix = "Extract_";
        public const string Opc2AmlName = "Opc2Aml";
        public const string Opc2Aml = Opc2AmlName + ".exe";

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
            string exeDirectory = rootDirectoryInfo.FullName + configurationPath;
            return new DirectoryInfo(exeDirectory);
        }

        static public DirectoryInfo GetTestFileDirectory()
        {
            string systemTestDirectory = Directory.GetCurrentDirectory();
            DirectoryInfo systemTestDirectoryInfo = new DirectoryInfo(systemTestDirectory);
            string testFileDirectory = systemTestDirectoryInfo.FullName + "\\TestFiles";
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

        static public bool Execute()
        {
            bool success = true;

            string executableName = ExecutableName();
            ProcessStartInfo processStartInfo = new ProcessStartInfo(executableName);
            Assert.IsNotNull(processStartInfo, "Unable to create ProcessStartInfo");
            processStartInfo.WorkingDirectory = GetOpc2AmlDirectory().FullName;
            Process opc2amlProcess = Process.Start(processStartInfo);
            
            int counter = 0;
            while( !opc2amlProcess.HasExited && counter < 30 )
            {
                System.Threading.Thread.Sleep(1000);
                counter++;
                if ( counter >= 30 )
                {
                    success = false;
                    opc2amlProcess.Kill();
                }
            }

            Assert.AreEqual(0, opc2amlProcess.ExitCode, "Conversion tool failed");
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
            Assert.IsTrue(extractFile.Exists, "Unable to find expected file " + fileName);

            DirectoryInfo extractedDirectory = GetExtractDirectory(amlxFile);

            ZipFile.ExtractToDirectory(extractFile.FullName, extractedDirectory.FullName, overwriteFiles: true);

            extractedDirectory.Refresh();

            Assert.IsTrue(extractedDirectory.Exists, "Unzip " + amlxFile + " Failed");

            string amlFullPath = Path.Combine(extractedDirectory.FullName, amlFile);

            FileInfo resultingAmlFile = new FileInfo(amlFullPath);
            Assert.IsTrue(resultingAmlFile.Exists, "Expected Aml " + amlFile + " Does not exist");

            return resultingAmlFile;
        }
    }
}
