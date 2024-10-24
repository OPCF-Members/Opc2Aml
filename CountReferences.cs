using MarkdownProcessor;
using MarkdownProcessor.NodeSet;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opc2Aml
{


    internal class CountReferences
    {

        public CountReferences( ModelManager modelManager, string modelName )
        {
            _modelManager = modelManager;
            _prefix = modelName;
        }

        public void AddReference( NodeId reference )
        {
            string referenceString = reference.ToString();
            if( _references.ContainsKey( referenceString ) )
            {
                _references[ referenceString ]++;
            }
            else
            {
                _references.Add( referenceString, 1 );
            }

            if ( !_nodeIds.ContainsKey(referenceString))
            {
                _nodeIds.Add(referenceString, reference);
            }
        }

        public void Report()
        {
            List<string> lines = new List<string>();
            foreach( var reference in _references )
            {
                string referenceString = reference.Key;
                if ( _nodeIds.ContainsKey( reference.Key ) )
                {
                    NodeId nodeId = _nodeIds[ reference.Key ];
                    UANode node = _modelManager.FindNode<UANode>( nodeId );
                    if ( node != null )
                    {
                        referenceString = node.BrowseName;
                    }
                }


                lines.Add( string.Format( "{0} [{1}] : {2}", referenceString, reference.Key, reference.Value ) );
            }

            if( lines.Count > 0 )
            {
                WriteFile( _prefix + "_ReferenceCounts.txt", lines );
            }
        }

        static public bool WriteFile( string fileName, List<string> lines )
        {
            bool success = false;

            try
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter( fileName );

                for( int index = 0; index < lines.Count; index++ )
                {
                    writer.WriteLine( lines[ index ] );
                }

                writer.Close();

                success = true;
            }
            catch
            {
            }

            return success;
        }

        string _prefix = string.Empty;
        ModelManager _modelManager = null;
        Dictionary<string, int> _references = new Dictionary<string, int>();
        Dictionary<string, NodeId> _nodeIds = new Dictionary<string, NodeId>();

    }
}
