using Opc.Ua;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opc2Aml
{
    internal class AddManifest : ModifyContainer
    {
        public AddManifest( string modelName, FileInfo source, Configuration configuration ) :
            base ( Prefix, modelName, source, configuration )
        {
        }

        public void Run( )
        {
            Dictionary<string, string> manifests = _configuration.GetManifests( _modelName );

            if ( manifests != null )
            {
                FileInfo destination = PrepareDestination();

                if (  destination != null )
                {
                    Package package = Package.Open( destination.FullName );
                    if( package != null )
                    {
                        foreach( KeyValuePair<string, string> pair in manifests )
                        {
                            // This has already been validated to exist
                            FileInfo manifestFile = new FileInfo( pair.Value );
                            Uri uri = new Uri( "/" + manifestFile.Name, UriKind.Relative );

                            try
                            {
                                PackagePart packagePart = package.GetPart( uri );
                                Utils.LogError( "Part " + uri.ToString() + " already exists in " + _modelName );
                            }
                            catch ( Exception ex )
                            {
                                // Odd, but okay.  Could check the exception

                                PackagePart descriptor = package.CreatePart(
                                    uri, "text/xml" );

                                using( FileStream fileStream = new FileStream(
                                    manifestFile.FullName, FileMode.Open, FileAccess.Read ) )
                                {
                                    Stream writeMe = descriptor.GetStream();
                                    fileStream.CopyTo( writeMe );
                                    writeMe.Close();
                                }

                                PackageRelationship relationship = package.CreateRelationship(
                                    uri, TargetMode.Internal, pair.Key );
                            }
                        }
                    }

                    package.Close();
                }
            }
        }

        private static string Prefix = "Manifest_";

    }
}
