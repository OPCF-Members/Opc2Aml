using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace SystemTest
{
    [TestClass]
    public class Basic
    {
        [TestMethod]
        public void TestMethod1()
        {
            TestHelper.GetConfigurationPath();
            List<string> testFiles = TestHelper.GetTestFileNames();
            TestHelper.PrepareUnconvertedXml(testFiles);
            System.Threading.Thread.Sleep(1000);
            TestHelper.Execute();
            List<FileInfo> amlFiles = TestHelper.ExtractAmlxFiles(testFiles);
        }
    }
}