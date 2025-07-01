using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace SystemTest
{
    [TestClass]
    public class TestOptionSets
    {
        CAEXDocument m_document = null;

        #region Tests

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestOperationalHealthOptionSet()
        {
            AttributeTypeLibType attributeLibrary = GetFxAcAttributes();
            AttributeFamilyType attributeFamilyType = attributeLibrary[ "OperationalHealthOptionSet" ];
            TestOptionSet( attributeFamilyType );
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
        public void TestAggregatedHealthOptionSet()
        {
            AttributeTypeLibType attributeLibrary = GetFxAcAttributes();
            AttributeFamilyType attributeFamilyType = attributeLibrary[ "AggregatedHealthDataType" ];
            Assert.IsNotNull( attributeFamilyType );
            AttributeType attributeType = attributeFamilyType.Attribute[ "AggregatedOperationalHealth" ];
            TestOptionSet( attributeType );
        }

        [TestMethod, Timeout( TestHelper.UnitTestTimeout )]
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
