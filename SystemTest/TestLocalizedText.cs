using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Aml.Engine.AmlObjects;
using Aml.Engine.CAEX;
using Aml.Engine.CAEX.Extensions;
using Opc.Ua;
using static System.Net.Mime.MediaTypeNames;


namespace SystemTest
{
    [TestClass]
    public class TestLocalizedText
    {
        CAEXDocument m_document = null;

        #region Tests

        [TestMethod]
        [DataRow( "DisplayName", DisplayName = "No LocalizedText DisplayName" )]
        [DataRow( "Description", DisplayName = "No LocalizedText Description" )]
        public void NoLocalizedText( string attributeName )
        {
            SystemUnitClassType objectToTest = GetTestObject( "6227" );
            AttributeType attributeType = objectToTest.Attribute[ attributeName ];
            Assert.IsNull( attributeType, "Unexpected attribute found for " + attributeName );
        }


        [TestMethod]
        [DataRow("6228", "DisplayName", "", "Single Localized Text", 
            DisplayName = "Single no Locale DisplayName")]
        [DataRow( "6228", "Description", "", "A Single Description with no Locale",
            DisplayName = "Single no Locale Description" )]
        [DataRow( "6229", "DisplayName", "en", "Single Localized Text with Locale",
            DisplayName = "Single with Locale DisplayName" )]
        [DataRow( "6229", "Description", "en", "A Single Description with a Locale",
            DisplayName = "Single with Locale Description" )]
        public void SingleLocalizedText(string nodeId, string attributeName, string localeId, string text )
        {
            SystemUnitClassType objectToTest = GetTestObject( nodeId );
            AttributeType attributeType = ValidateAttribute( objectToTest.Attribute, attributeName, text );

            if( string.IsNullOrEmpty( localeId ) )
            {
                Assert.AreEqual( 0, attributeType.Attribute.Count, 1 );
            }
            else
            {
                ValidateAttribute( attributeType.Attribute, localeId, text );
            }
        }

        [TestMethod]
        [DataRow( "6230", "DisplayName", 
            new string[] { "qaa", "en", "fr", "de"},
            new string[] { 
                "First DisplayName with no Locale",
                "Second DisplayName with a Locale", 
                "Troisième texte localisé avec paramètres régionaux", 
                "Letzter lokalisierter Text mit Gebietsschema" },
            DisplayName = "Multiple Starting with no Locale DisplayName" )]
        [DataRow( "6230", "Description",
            new string[] { "qaa", "en", "fr", "de" },
            new string[] {
                "First Description with no Locale",
                "Second Description with a Locale",
                "Troisième description avec un paramètre régional",
                "Letzte Beschreibung mit einem Gebietsschema" },
            DisplayName = "Multiple Starting with no Locale Description" )]
        [DataRow( "6231", "DisplayName",
            new string[] { "en", "fr", "de", "qaa"},
            new string[] {
                "First DisplayName with a Locale",
                "Deuxième DisplayName avec une locale",
                "Dritter DisplayName mit einem Gebietsschema",
                "Last DisplayName with no Locale" },
            DisplayName = "Multiple Ending with no Locale DisplayName" )]
        [DataRow( "6231", "Description",
            new string[] { "en", "fr", "de", "qaa" },
            new string[] {
                "First Description with a Locale",
                "Deuxième description avec une locale",
                "Dritte Beschreibung mit einem Gebietsschema",
                "Last Description with no Locale" },
            DisplayName = "Multiple Ending with no Locale Description" )]
        
        
        [DataRow( "6232", "DisplayName",
            new string[] { "qaa", "qab", "qac", "qad" },
            new string[] {
                "First DisplayName with no Locale",
                "Second DisplayName with no Locale",
                "Third DisplayName with no Locale",
                "Last DisplayName with no Locale" },
            DisplayName = "Multiple no Locale DisplayName" )]
        [DataRow( "6232", "Description",
            new string[] { "qaa", "qab", "qac", "qad" },
            new string[] {
                "First Description with no Locale",
                "Second Description with no Locale",
                "Third Description with no Locale",
                "Last Description with no Locale" },
            DisplayName = "Multiple no Locale Description" )]
        public void MultipleLocalizedText( string nodeId, string attributeName, string[] localeId, string[] text )
        {
            SystemUnitClassType objectToTest = GetTestObject( nodeId );
            AttributeType topLevel = ValidateAttribute( objectToTest.Attribute, attributeName, text[ 0 ] );
            AttributeType textLevel = ValidateAttribute( topLevel.Attribute, localeId[0], text[ 0 ] );

            for( int index = 0; index < localeId.Length; index++ )
            {
                string subLocale = "aml-lang=" + localeId[ index ];  
                ValidateAttribute( textLevel.Attribute, subLocale, text[index] );
            }
        }

        private AttributeType ValidateAttribute( AttributeSequence sequence, string attributeName, string attributeValue )
        {
            Assert.IsNotNull( sequence, "AttributeSequence not found" );
            AttributeType attributeType = sequence[ attributeName ];
            Assert.IsNotNull( attributeType, attributeName + " attribute not found" );
            Assert.IsNotNull( attributeType.Value, attributeName + " attribute valid not found" );
            Assert.AreEqual( attributeValue, attributeType.Value, "Unexpected value for " + attributeName );
            return attributeType;
        }

        #endregion

        #region Helpers

        private CAEXDocument GetDocument()
        {
            if( m_document == null )
            {
                m_document = TestHelper.GetReadOnlyDocument( "TestAml.xml.amlx" );
            }
            Assert.IsNotNull( m_document, "Unable to retrieve Document" );
            return m_document;
        }

        public SystemUnitClassType GetTestObject(string nodeId, bool foundationRoot = false)
        {
            CAEXDocument document = GetDocument();
            string rootName = TestHelper.GetRootName();
            if ( foundationRoot )
            {
                rootName = TestHelper.GetOpcRootName();
            }
            CAEXObject initialObject = document.FindByID(rootName + nodeId);
            Assert.IsNotNull(initialObject, "Unable to find Initial Object");
            SystemUnitClassType theObject = initialObject as SystemUnitClassType;
            Assert.IsNotNull(theObject, "Unable to Cast Initial Object");
            return theObject;
        }

        #endregion
    }
}
