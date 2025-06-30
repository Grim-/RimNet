using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    //Holds all the active networks for a map, provides shortcuts to send network messages.
    public class MapComp_NetworkManager : MapComponent
    {
        protected List<RimNet> ActiveNetworks = new List<RimNet>();



        protected int uniqueNodeIDPrefix = 0;

        public MapComp_NetworkManager(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (Find.TickManager.TicksGame % 10 == 0)
            {
                foreach (var network in ActiveNetworks)
                {
                    network.ProcessMessages();
                }
            }
        }

        private string GetNextNetworkID()
        {
            return $"Net_{ActiveNetworks.Count + 1}";
        }

        public RimNet GetOrRegisterNetwork(string networkID = "")
        {
            if (!string.IsNullOrEmpty(networkID) && ActiveNetworks.Any(x => x.ID == networkID))
            {
                return ActiveNetworks.FirstOrDefault(x => x.ID == networkID);
            }
            else if (ActiveNetworks.Count > 0)
            {
                return ActiveNetworks.First();
            }
            else
            {
                string newID = string.IsNullOrEmpty(networkID) ? GetNextNetworkID() : networkID;
                RimNet newNetwork = new RimNet(newID);
                ActiveNetworks.Add(newNetwork);
                return newNetwork;
            }
        }

        public void RegisterNode(Comp_NetworkNode node)
        {
            if (node?.parent?.Map == null)
                return;
            if (node.ConnectedNetwork == null)
            {
                RimNet rimworldNetwork = GetOrRegisterNetwork(GetNextNetworkID());
                if (rimworldNetwork != null)
                {
                    rimworldNetwork.RegisterNode(node);
                }
            }
        }

        public void UnregisterNode(Comp_NetworkNode node)
        {
            if (node?.parent?.Map == null)
                return;
            if (node.ConnectedNetwork == null)
                return;
            RimNet rimworldNetwork = GetOrRegisterNetwork(node.ConnectedNetwork.ID);
            if (rimworldNetwork != null)
            {
                rimworldNetwork.UnregisterNode(node);
            }
        }

        public void RemoveNetwork(string networkID)
        {
            if (ActiveNetworks.Any(x => x.ID == networkID))
            {
                ActiveNetworks.RemoveWhere(x => x.ID == networkID);
            }
        }

        public void BroadcastMessage(Comp_NetworkNode sender, NetworkMessage message)
        {
            if (sender?.parent?.Map == null)
                return;
            sender.ConnectedNetwork.BroadcastMessage(sender, message);
        }

        public void SendMessage(Comp_NetworkNode sender, string targetNodeID, NetworkMessage message)
        {
            if (sender?.parent?.Map == null)
                return;
            RimNet rimworldNetwork = GetOrRegisterNetwork(sender.ConnectedNetwork.ID);
            if (rimworldNetwork != null)
            {
                rimworldNetwork.SendMessage(sender, targetNodeID, message);
            }
        }
        public string GetUniqueNodeID()
        {
            this.uniqueNodeIDPrefix++;
            return "RimNetworkNode_" + uniqueNodeIDPrefix;
        }
        public static MapComp_NetworkManager GetNetworkManager(Map map)
        {
            return map.GetComponent<MapComp_NetworkManager>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref ActiveNetworks, "activeNetworks", LookMode.Deep);
            Scribe_Values.Look(ref uniqueNodeIDPrefix, "lastUniqueID");
        }
    }
}