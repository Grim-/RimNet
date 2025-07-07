using System.Collections.Generic;
using Verse;
using RimWorld;
using System;
using System.Linq;

namespace RimNet
{
    public class CompProperties_NetworkServer : CompProperties_NetworkNode
    {
        public CompProperties_NetworkServer()
        {
            compClass = typeof(Comp_NetworkServer);
        }
    }

    public class Comp_NetworkServer : Comp_NetworkNode
    {
        private RimNet hostedNetwork;
        public RimNet HostedNetwork => hostedNetwork;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (string.IsNullOrEmpty(NodeID))
            {
                NodeID = NetworkRouter.GetUniqueNodeID();
            }

            if (hostedNetwork == null)
            {
                hostedNetwork = new RimNet(this, "Server_" + NodeID);
                NetworkRouter.TryAddNetwork(hostedNetwork);
            }

            if (hostedNetwork != null)
            {
                hostedNetwork.RegisterNode(this);
            }

            base.PostSpawnSetup(respawningAfterLoad);
        }

        protected override void TryJoinNetwork()
        {
            if (hostedNetwork != null && ConnectedNetwork != hostedNetwork)
            {
                hostedNetwork.RegisterNode(this);
            }
        }

        public void ConnectNode(Comp_NetworkNode node)
        {
            if (hostedNetwork != null)
            {
                hostedNetwork.RegisterNode(node);
            }
        }

        public void DisconnectNode(Comp_NetworkNode node)
        {
            if (hostedNetwork != null && node.ConnectedNetwork == hostedNetwork)
            {
                hostedNetwork.UnregisterNode(node);
            }
        }

        public override bool IsConnectedToServer(out Comp_NetworkServer foundServer)
        {
            foundServer = this;
            return true;
        }

        public override void PostDeSpawn(Map previousMap)
        {
            if (hostedNetwork != null)
            {
                NetworkConnectionMaker.DisconnectAllFromServer(this, previousMap);
            }
            base.PostDeSpawn(previousMap);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref hostedNetwork, "hostedNetwork");

            if (Scribe.mode == LoadSaveMode.PostLoadInit && hostedNetwork != null)
            {
                if (!hostedNetwork.NetworkNodes.Contains(this))
                {
                    hostedNetwork.RegisterNode(this);
                }
            }
        }
    }
}