using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Aml.Engine.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace SystemTest
{
    [TestClass]
    public class TestOptionSetEmptyValues
    {
        CAEXDocument m_document = null;

        #region Tests

        [ TestMethod]
        public void Validation()
        {
            var lookupService = LookupService.Register();
            var service = ValidatorService.Register();

            IEnumerable<ValidationElement> issues = service.ValidateAll(GetDocument());
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

        [TestMethod]
        public void TestOperationalHealthOptionSet()
        {
            AttributeTypeLibType attributeLibrary = GetFxAcAttributes();
            AttributeFamilyType attributeFamilyType = attributeLibrary[ "OperationalHealthOptionSet" ];
            TestOptionSet( attributeFamilyType );
        }

        [TestMethod]
        public void TestAggregatedHealthOptionSet()
        {
            AttributeTypeLibType attributeLibrary = GetFxAcAttributes();
            AttributeFamilyType attributeFamilyType = attributeLibrary[ "AggregatedHealthDataType" ];
            Assert.IsNotNull( attributeFamilyType );
            AttributeType attributeType = attributeFamilyType.Attribute[ "AggregatedOperationalHealth" ];
            TestOptionSet( attributeType );
        }

        [TestMethod]
        public void TestInstance()
        {
            CAEXDocument document = GetDocument();
            CAEXObject instance = document.FindByID( "nsu%3Dhttp%3A%2F%2Fopcfoundation.org%2FUA%2FFX%2FAML%2FTESTING%2FAmlFxTest%2F%3Bi%3D6003" );
            Assert.IsNotNull( instance );
            InternalElementType internalElementType = instance as InternalElementType; 
            Assert.IsNotNull( internalElementType );
            AttributeType value = internalElementType.Attribute[ "Value" ];
            Assert.IsNotNull( value );
            TestOptionSet( value );
        }


        #endregion

        #region Helpers

        private CAEXDocument GetDocument()
        {
            if( m_document == null )
            {
                m_document = TestHelper.GetReadOnlyDocument( "AmlFxTest.xml.amlx" );
            }
            Assert.IsNotNull( m_document, "Unable to retrieve Document" );
            return m_document;
        }

        public void TestOptionSet( AttributeTypeType attributeFamilyType )
        {
            Assert.IsNotNull( attributeFamilyType );
            Assert.IsTrue( attributeFamilyType.Attribute.Count >= 4 );
            ValidateOptionSetNames( attributeFamilyType );
        }

        public void ValidateOptionSetNames( AttributeTypeType attributeFamilyType )
        {
            HashSet<string> names = GetOptionSetNames( attributeFamilyType );
            Assert.AreEqual( true, names.Contains( "OperationalWarning" ) );
            Assert.AreEqual( true, names.Contains( "OperationalError" ) );
            Assert.AreEqual( true, names.Contains( "SubOperationalWarning" ) );
            Assert.AreEqual( true, names.Contains( "SubOperationalError" ) );
        }

        public HashSet<string> GetOptionSetNames( AttributeTypeType attributeFamilyType )
        {
            HashSet<string> strings = new HashSet<string>();

            foreach( AttributeType attribute in attributeFamilyType.Attribute )
            {
                strings.Add( attribute.Name );
            }

            return strings;
        }

        public AttributeTypeLibType GetFxAcAttributes()
        {
            CAEXDocument document = GetDocument();
            AttributeTypeLibType fxAcAttributes = document.CAEXFile.AttributeTypeLib[
                "ATL_http://opcfoundation.org/UA/FX/AC/" ];
            Assert.IsNotNull( fxAcAttributes );
            return fxAcAttributes;
        }

        #endregion

    }
}
