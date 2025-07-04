using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class CompProperties_NetworkClient : CompProperties_NetworkNode
    {
        public CompProperties_NetworkClient()
        {
            compClass = typeof(Comp_NetworkClient);
        }
    }


    /// <summary>
    /// Allows a network node to be a client (interaction mostly)
    /// </summary>
    public class Comp_NetworkClient : Comp_NetworkNode
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (ConnectedNetwork != null)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Net View",
                    defaultDesc = "Open the network viewer",
                    action = () =>
                    {
                        Find.WindowStack.Add(new Window_NetworkGridView(ConnectedNetwork, this));
                    }
                };
            }
        }
    }

}