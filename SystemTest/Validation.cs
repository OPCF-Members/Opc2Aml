using Aml.Engine.CAEX;
using Aml.Engine.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemTest
{
    [TestClass]
    public class Validation
    {
        #region Tests

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        [DataRow("TestAml.xml.amlx")]
        [DataRow("AmlFxTest.xml.amlx")]
        [DataRow("Opc.Ua.Fx.IOPModel.xml.amlx")]
        [DataRow("Opc.Ua.Fx.Show.BottleMachine.xml.amlx")]
        [DataRow("Opc.Ua.Fx.Show.Controller.xml.amlx")]
        public void ValidationTest( string fileName )
        {
            var lookupService = LookupService.Register();
            var service = ValidatorService.Register();

            IEnumerable<ValidationElement> issues = service.ValidateAll(GetDocument(fileName));
            if (issues != null)
            {
                Assert.AreEqual(0, issues.AliasReferenceValidationResults().Count(), "AliasReferenceValidationResults");
                Assert.AreEqual(0, issues.ExternalReferenceValidationResults().Count(), "ExternalReferenceValidationResults");
                Assert.AreEqual(0, issues.ClassPathValidationResults().Count(), "ClassPathValidationResults");
                Assert.AreEqual(0, issues.IDReferenceValidationResults().Count(), "IDReferenceValidationResults");
                Assert.AreEqual(0, issues.PathReferenceValidationResults().Count(), "PathReferenceValidationResults");
                Assert.AreEqual(0, issues.NameReferenceValidationResults().Count(), "NameReferenceValidationResults");
                Assert.AreEqual(0, issues.NameValidationResults().Count(), "NameValidationResults");
                Assert.AreEqual(0, issues.IDValidationResults().Count(), "IDValidationResults");
                Assert.AreEqual(0, issues.MetaDataValidationResults().Count(), "MetaDataValidationResults");
                Assert.AreEqual(0, issues.InvalidAutomationMLVersion().Count(), "InvalidAutomationMLVersion");
                Assert.AreEqual(0, issues.InvalidSchemaVersion().Count(), "InvalidSchemaVersion");
                Assert.AreEqual(0, issues.MissingDocumentSourceInformation().Count(), "MissingDocumentSourceInformation");
                Assert.AreEqual(0, issues.InvalidIDs().Count(), "InvalidIDs");
                Assert.AreEqual(0, issues.NotUniquePath().Count(), "NotUniquePath");
                Assert.AreEqual(0, issues.Repairable().Count(), "Repairable");
                Assert.AreEqual(0, issues.NotRepairable().Count(), "NotRepairable");
                Assert.AreEqual(0, issues.UnresolvedIDRefs().Count(), "UnresolvedIDRefs");
                Assert.AreEqual(0, issues.UnresolvedPathRefs().Count(), "UnresolvedPathRefs");
                Assert.AreEqual(0, issues.EmptyPathRefs().Count(), "EmptyPathRefs");
                Assert.AreEqual(0, issues.EmptyIDRefs().Count(), "EmptyIDRefs");
                Assert.AreEqual(0, issues.MissingIDs().Count(), "MissingIDs");
                Assert.AreEqual(0, issues.NotUniqueIDs().Count(), "NotUniqueIDs");
                Assert.AreEqual(0, issues.UnidentifiedAlias().Count(), "UnidentifiedAlias");
                Assert.AreEqual(0, issues.UselessAlias().Count(), "UselessAlias");
                Assert.AreEqual(0, issues.Count(), "TotalIssues");
            }
            else
            {
                Assert.Fail("Unable to get validation results");
            }
        }

        #endregion

        #region Helpers

        private CAEXDocument GetDocument( string fileName )
        {
            CAEXDocument document = TestHelper.GetReadOnlyDocument( fileName );
            Assert.IsNotNull( document, "Unable to retrieve Document" );
            return document;
        }

        #endregion

    }
}
