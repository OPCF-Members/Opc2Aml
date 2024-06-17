using Opc.Ua;
using OpenVsixSignTool.Core;
using SBOffice;
using SBOfficeCommon;
using SBOfficeSecurity;
using SBX509;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Opc2Aml
{
    internal class SignContainerSecureBlackBox : ModifyContainer
    {
        public SignContainerSecureBlackBox( string modelName, FileInfo source, Configuration configuration ) :
            base( Prefix, modelName, source, configuration)
        {

        }

        public void Run()
        {
            if ( _configuration.CertificateFile != string.Empty )
            {
                X509Certificate2 certificate = null;

                if ( _configuration.Password == string.Empty )
                {
                    certificate = new X509Certificate2( _configuration.CertificateFile );
                }
                else
                {
                    certificate = new X509Certificate2( _configuration.CertificateFile, _configuration.Password );
                }

                if ( certificate != null )
                {
                    FileInfo destination = PrepareDestination();
                    if( destination != null )
                    {
                        using( TElOfficeDocument officeDocument = new TElOfficeDocument() )
                        {
                            TElX509Certificate bbCertificate = new TElX509Certificate();
                            bbCertificate.FromX509Certificate2( certificate );

                            officeDocument.RuntimeLicense = blackBoxTemporaryLicense;

                            officeDocument.Open( _destination.FullName );
                            if( ( officeDocument.DocumentFormat != TSBOfficeDocumentFormat.OpenXML ) ||
                                !officeDocument.Signable )
                            {
                                throw new Exception( "Cannot sign document using XML signature handler" );
                            }

                            TElOfficeOpenXMLSignatureHandler openXMLSigHandler = 
                                new TElOfficeOpenXMLSignatureHandler();
                            openXMLSigHandler.RuntimeLicense = blackBoxTemporaryLicense;
                            openXMLSigHandler.DigestMethod = SBXMLSec.Unit.xdmSHA256;

                            officeDocument.AddSignature( openXMLSigHandler, true );
                            openXMLSigHandler.DigestMethod = SBXMLSec.Unit.xdmSHA256;

                            openXMLSigHandler.AddDocument();
                            openXMLSigHandler.DigestMethod = SBXMLSec.Unit.xdmSHA256;
                            openXMLSigHandler.Sign( bbCertificate, TSBOfficeOpenXMLEmbedCertificate.InSignature );
                        }
                    }
                }
                else
                {
                    Utils.LogError( "Unable to load certificate " + _configuration.CertificateFile );
                }
            }
        }

        private static string Prefix = "SignBB_";
        private string blackBoxTemporaryLicense = "";
    }
}
