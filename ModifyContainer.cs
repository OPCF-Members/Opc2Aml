using Opc.Ua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opc2Aml
{
    internal class ModifyContainer
    {
        public ModifyContainer( string prefix, string modelName, FileInfo source, Configuration configuration )
        {
            _prefix = prefix;
            _modelName = modelName.ToLower();
            _source = source;
            _configuration = configuration;
        }

        protected FileInfo PrepareDestination()
        {
            FileInfo destination = GetDestinationName();

            bool success = false;
            bool failed = false;

            if( destination.Exists )
            {
                try
                {
                    destination.Delete();
                }
                catch( Exception ex )
                {
                    Utils.LogError( "Unable to delete " + destination.FullName + " " + ex.Message );
                    failed = true;
                }
            }

            if( !failed )
            {
                try
                {
                    File.Copy( _source.FullName, destination.FullName );
                    success = true;
                }
                catch( Exception ex )
                {
                    Utils.LogError( "Unable to copy " + destination.FullName + " " + ex.Message );
                }
            }

            if( success )
            {
                _destination = destination;
            }
            else
            {
                Utils.LogError( "Unable to create file " + destination.FullName );
            }

            return _destination;
        }

        protected FileInfo GetDestinationName()
        {
            string destinationName = _prefix + _source.Name;
            return new FileInfo( Path.Combine( _source.DirectoryName, destinationName ) );
        }

        public FileInfo Destination { get { return _destination; } }

        protected string _prefix = string.Empty;

        protected string _modelName = string.Empty;
        protected FileInfo _source = null;
        protected FileInfo _destination = null;
        protected Configuration _configuration = null;

    }
}
