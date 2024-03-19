using Opc.Ua;
using OpenVsixSignTool.Core;
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
    internal class SignContainer : ModifyContainer
    {
        public SignContainer( string modelName, FileInfo source, Configuration configuration ) :
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
                        using( OpcPackage package = OpcPackage.Open( destination.FullName, OpcPackageFileMode.ReadWrite ) )
                        {
                            OpcPackageSignatureBuilder builder = package.CreateSignatureBuilder();
                            builder.EnqueueNamedPreset<VSIXSignatureBuilderPreset>();

                            var signingConfiguration = new SignConfigurationSet(
                                fileDigestAlgorithm: _configuration.Algorithm,
                                signatureDigestAlgorithm: _configuration.Algorithm,
                                publicCertificate: certificate,
                                signingKey: certificate.GetRSAPrivateKey() );

                            OpcSignature signature = builder.Sign( signingConfiguration );
                        }
                    }
                }
                else
                {
                    Utils.LogError( "Unable to load certificate " + _configuration.CertificateFile );
                }
            }
        }

        private static string Prefix = "Sign_";

    }
}
