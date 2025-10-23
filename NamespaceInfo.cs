using MarkdownProcessor;
using MarkdownProcessor.NodeSet;
using System.Collections.Generic;
using System.Linq;

namespace Opc2Aml
{
    public class NamespaceInfo
    {
        public NamespaceInfo( string uri, HashSet<string> dependencies )
        {
            Uri = uri;
            Dependencies = dependencies.ToList<string>();
        }

        public int Count()
        {
            return Dependencies.Count;
        }

        public ushort Index { get; set; }
        public string Uri { get; set; }
        public List<string> Dependencies { get; set; }
    }
}
