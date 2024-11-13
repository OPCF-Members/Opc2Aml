using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace SystemTest
{
    [TestClass]

    public class TestCommandLine
    {
        private static string _workingDirectory = "Working";
        private static string _outputDirectory = "Output";
        private static string _outputAllDirectory = "OutputAll";
        private static string _configDirectory = "Config";            

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            DirectoryInfo source = new DirectoryInfo( "NodeSetFiles" );
            DirectoryInfo destination = new DirectoryInfo( _workingDirectory );
            if ( destination.Exists )
            {
                destination.Delete( true );
            }

            destination.Create();

            foreach( FileInfo file in source.GetFiles() )
            {
                file.CopyTo( Path.Combine( destination.FullName, file.Name ) );
            }

            destination.CreateSubdirectory( _outputDirectory );
            destination.CreateSubdirectory( _configDirectory );
        }

        #region Tests

        [TestMethod]
        public void OutputSpecific()
        {
            DirectoryInfo outputDirectoryInfo = TestHelper.GetOpc2AmlDirectory();

            FileInfo fileInfo = new FileInfo(outputDirectoryInfo.FullName + "\\InstanceLevel.xml.amlx" );
            if ( fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            // Delete another file
            FileInfo dontBuildfileInfo = new FileInfo(outputDirectoryInfo.FullName + "\\LevelOne.xml.amlx" );
            if( dontBuildfileInfo.Exists )
            {
                dontBuildfileInfo.Delete();
            }

            string commandLine = NodesetParameter( "InstanceLevel.xml" );

            TestHelper.Execute( commandLine, expectedResult: 0 );

            fileInfo.Refresh();
            Assert.IsTrue(fileInfo.Exists);

            dontBuildfileInfo.Refresh();
            Assert.IsFalse( dontBuildfileInfo.Exists );
        }

        [TestMethod]
        public void OutputNonExistent()
        {
            string commandLine = "--Nodeset NonExistent.xml";

            TestHelper.Execute( commandLine, expectedResult: 1 );
        }

        // Test 6:  Output specific output overwrite
        [TestMethod]
        public void OutputSpecificOverwrite()
        {
            string commandLine = DirectoryParameter( ) + 
                NodesetParameter( "LevelOne.xml" ) +
                OutputParameter( _outputDirectory + "\\OverwriteMe" );

            TestHelper.Execute( commandLine, expectedResult: 0 );
            DateTime before = DateTime.UtcNow;
            TestHelper.Execute( commandLine, expectedResult: 0 );
            FileInfo fileInfo = new FileInfo( OutputDirectory().FullName + "\\OverwriteMe.amlx" );
            Assert.IsTrue( fileInfo.Exists );
            Assert.IsTrue( before < fileInfo.LastWriteTimeUtc );
        }

        [TestMethod]
        public void SpecificNoDirectory()
        {
            string commandLine = NodesetParameter( WorkingDirectoryString() + "LevelOne.xml" ) + 
                OutputParameter( "SpecificNoDirectory" );

            TestHelper.Execute( commandLine, expectedResult: 0 );
            // Check the working directory for the file
            FileInfo fileInfo = new FileInfo( WorkingDirectory().FullName + "\\SpecificNoDirectory.amlx" );
            Assert.IsTrue( fileInfo.Exists );
        }

        [TestMethod]
        public void NoDirectoryOutputSubdirectory()
        {
            string commandLine = NodesetParameter( WorkingDirectoryString() + "LevelOne.xml" ) +
                OutputParameter( _outputDirectory + "\\NoDirectoryOutputSubdirectory" );

            TestHelper.Execute( commandLine, expectedResult: 0 );
            // Check the working directory for the file
            FileInfo fileInfo = new FileInfo( OutputDirectory().FullName + "\\NoDirectoryOutputSubdirectory.amlx" );
            Assert.IsTrue( fileInfo.Exists );
        }

        [TestMethod]
        public void RelativeDirectory()
        {
            string commandLine = DirectoryParameter() + 
                NodesetParameter( "LevelOne.xml" ) +
                OutputParameter( _outputDirectory + "\\RelativeDirectory" );

            TestHelper.Execute( commandLine, expectedResult: 0 );
            // Check the working directory for the file
            FileInfo fileInfo = new FileInfo( OutputDirectory().FullName + "\\RelativeDirectory.amlx" );
            Assert.IsTrue( fileInfo.Exists );
        }

        [TestMethod]
        public void AbsoluteDirectory()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo( WorkingDirectory().FullName );
            string commandLine = "--DirectoryInfo " + directoryInfo.FullName  +
                NodesetParameter( "LevelOne.xml" ) +
                OutputParameter( _outputDirectory + "\\AbsoluteDirectory" );

            TestHelper.Execute( commandLine, expectedResult: 0 );
            // Check the working directory for the file
            FileInfo fileInfo = new FileInfo( OutputDirectory().FullName + "\\AbsoluteDirectory.amlx" );
            Assert.IsTrue( fileInfo.Exists );
        }

        [TestMethod]
        public void BadDirectory()
        {
            string commandLine = DirectoryParameter() + "BadDirectory" +
                NodesetParameter( "LevelOne.xml" ) +
                OutputParameter( _outputDirectory + "\\BadDirectory" );

            TestHelper.Execute( commandLine, expectedResult: 1 );
        }

        [TestMethod]
        public void OutputAllDirectory()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo( WorkingDirectory().FullName );

            foreach( FileInfo file in directoryInfo.GetFiles( "*.amlx") )
            {
                file.Delete();
            }

            Assert.AreEqual( 0, directoryInfo.GetFiles( "*.amlx" ).Length );

            string commandLine = DirectoryParameter();
            TestHelper.Execute( commandLine, expectedResult: 0 );

            Assert.IsTrue( directoryInfo.GetFiles( "*.amlx" ).Length > 0 );
            Assert.IsTrue( directoryInfo.GetFiles( "*.amlx" ).Length == 
                directoryInfo.GetFiles( "*.xml" ).Length );
        }

        [TestMethod]
        public void OutputOneDirectory()
        {
            string commandLine = DirectoryParameter() +
                OutputParameter( _outputDirectory + "\\OutputOneDirectory" );

            TestHelper.Execute( commandLine, expectedResult: 1 );
        }

        [TestMethod]
        public void RelativeConfig()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo( ConfigDirectory().FullName );
            string fileName = Path.Combine( directoryInfo.FullName, "RelativeConfig.json" );
            string relativeLog = ConfigDirectoryString() + "RelativeConfig.log";
            string useRelativePath = relativeLog.Replace( '\\', '/' );
            WriteConfigFile( fileName, useRelativePath );

            string relativePath = ".\\Config\\RelativeConfig.json";

            string commandLine = DirectoryParameter() +
                NodesetParameter( "LevelOne.xml" ) +
                OutputParameter( _outputDirectory + "\\RelativeConfig" +
                ConfigParameter( relativePath ) );

            TestHelper.Execute( commandLine, expectedResult: 0 );

            FileInfo expectedLog = new FileInfo( relativeLog );
            Assert.IsTrue( expectedLog.Exists );
        }

        [TestMethod]
        public void AbsoluteConfig()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo( ConfigDirectory().FullName );
            string fileName = Path.Combine( directoryInfo.FullName, "AbsoluteConfig.json" );

            string absoluteLog = directoryInfo.FullName + "\\AbsoluteConfig.log";
            string useAbsolutePath = absoluteLog.Replace( '\\', '/' );
            WriteConfigFile( fileName, useAbsolutePath );

            string commandLine = DirectoryParameter() +
                NodesetParameter( "LevelOne.xml" ) +
                OutputParameter( _outputDirectory + "\\AbsoluteConfig" +
                ConfigParameter( fileName ) );

            TestHelper.Execute( commandLine, expectedResult: 0 );

            FileInfo expectedLog = new FileInfo( absoluteLog );
            Assert.IsTrue( expectedLog.Exists );
        }

        [TestMethod]
        public void BadConfig()
        {
            DateTime start = DateTime.UtcNow;

            DirectoryInfo directoryInfo = new DirectoryInfo( ConfigDirectory().FullName );
            string fileName = Path.Combine( directoryInfo.FullName, "BadConfig.json" );

            string absoluteLog = directoryInfo.FullName + "\\BadConfig.log";
            string useAbsolutePath = absoluteLog.Replace( '\\', '/' );
            WriteConfigFile( fileName, useAbsolutePath );

            string commandLine = DirectoryParameter() +
                NodesetParameter( "LevelOne.xml" ) +
                OutputParameter( _outputDirectory + "\\BadConfig" +
                ConfigParameter( fileName + "bad" ) );

            TestHelper.Execute( commandLine, expectedResult: 0 );

            // Get the time of Opc2Aml.report.txt
            FileInfo defaultReport = new FileInfo( TestHelper.GetOpc2AmlDirectory().FullName + "\\Opc2Aml.report.txt" );
            Assert.IsTrue( defaultReport.Exists );
            Assert.IsTrue( start < defaultReport.LastWriteTimeUtc );
        }



        // Config relative
        // Config absolute
        // Config non existent

        #endregion

        #region Helpers


        private void WriteConfigFile( string filename, string parameter )
        {
            List<string> lines = new List<string>();

            lines.Add( "{" );
            lines.Add( "\t\"TraceConfiguration\": {" );
            lines.Add( "\t\t\"OutputFilePath\": \"" + parameter + "\"," );
            lines.Add( "\t\t\"DeleteOnLoad\": true," );
            lines.Add( "\t\t\"LogLevel\": \"Information\"" );

            lines.Add( "\t}" );
            lines.Add( "}" );

            TestHelper.WriteFile( filename, lines );
        }

        private DirectoryInfo OutputDirectory()
        {
            return new DirectoryInfo( WorkingDirectory().FullName + "\\" + _outputDirectory );
        }

        private DirectoryInfo ConfigDirectory()
        {
            return new DirectoryInfo( WorkingDirectory().FullName + "\\" + _configDirectory );
        }

        private DirectoryInfo WorkingDirectory()
        {
            return new DirectoryInfo( _workingDirectory );
        }

        private string NodesetParameter(string nodeset)
        {
            return " --Nodeset " + nodeset;
        }

        private string DirectoryParameter( )
        {
            return " --DirectoryInfo " + WorkingDirectoryString();
        }

        private string OutputParameter( string output)
        {
            return  " --Output " + output;
        }

        private string ConfigParameter( string config )
        {
            return " --Config " + config;
        }

        private string OutputDirectoryString()
        {
            return WorkingDirectoryString() + _outputDirectory + "\\";
        }

        private string WorkingDirectoryString()
        {
            return RelativePath() + _workingDirectory + "\\";
        }
        private string ConfigDirectoryString()
        {
            return WorkingDirectoryString() + _configDirectory + "\\";
        }


        private string RelativePath( )
        {
            return  "..\\..\\..\\..\\SystemTest\\bin\\" + GetBuildType() + "\\net6.0\\";
        }

        private string GetBuildType()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }

        #endregion
    }
}
