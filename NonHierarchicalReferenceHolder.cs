using Aml.Engine.CAEX;
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
    public class NonHierarchicalReferences
    {
        List<NonHierarchicalReferenceHolder> References = null;
        ModelManager m_modelManager = null;

        public HashSet<string> BlackList = new HashSet<string>();

        //NodeId HasTypeDefinition = Opc.Ua.ReferenceTypeIds.HasTypeDefinition;
        //NodeId HasModellingRule = Opc.Ua.ReferenceTypeIds.HasModellingRule;

        public NonHierarchicalReferences( ModelManager modelManager )
        {
            References = new List<NonHierarchicalReferenceHolder>();
            m_modelManager = modelManager;

            // Known Blacklist Items
            BlackList.Add(Opc.Ua.ReferenceTypeIds.HasTypeDefinition.ToString());
            BlackList.Add(Opc.Ua.ReferenceTypeIds.HasModellingRule.ToString());

            BlackList.Add(Opc.Ua.ReferenceTypeIds.HasEffect.ToString());
            BlackList.Add(Opc.Ua.ReferenceTypeIds.AlwaysGeneratesEvent.ToString());
            //BlackList.Add(Opc.Ua.ReferenceTypeIds.HasTrueSubState.ToString());
            BlackList.Add(Opc.Ua.ReferenceTypeIds.HasCondition.ToString());
            BlackList.Add(Opc.Ua.ReferenceTypeIds.GeneratesEvent.ToString());

        }

        public List<NonHierarchicalReferenceHolder> ReferenceList { get { return References; } }

        public void AddReference( ReferenceInfo reference, bool instance)
        {
            //if ( reference.ReferenceTypeId != HasTypeDefinition && reference.ReferenceTypeId != HasModellingRule )
            if ( !BlackList.Contains( reference.ReferenceTypeId.ToString() ) )
            {
                NonHierarchicalReferenceHolder referenceHolder = new NonHierarchicalReferenceHolder(
                    reference, instance );
                References.Add(referenceHolder);
            }
        }
    }

    public class NonHierarchicalReferenceHolder
    {
        public NonHierarchicalReferenceHolder( ReferenceInfo reference, bool instance )
        {
            Reference = reference;
            Instance = instance;
        }

        public ReferenceInfo Reference { get; set; }
        public bool Instance { get; set; }
    }
}
