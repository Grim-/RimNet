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
        protected int uniqueServerID = 0;

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

        public bool TryAddNetwork(RimNet rimNet)
        {
            if (ActiveNetworks.Any(x=> x.ID == rimNet.ID))
            {
                return false;
            }
            ActiveNetworks.Add(rimNet);
            return true;
        }

        public void RemoveNetwork(string networkID)
        {
            if (ActiveNetworks.Any(x => x.ID == networkID))
            {
                ActiveNetworks.RemoveWhere(x => x.ID == networkID);
            }
        }

        public string GetUniqueNodeID()
        {
            this.uniqueNodeIDPrefix++;
            return "RimNetworkNode_" + uniqueNodeIDPrefix;
        }
        public string GetNextNetworkID()
        {
            uniqueServerID++;
            return $"Net_{uniqueServerID}";
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
            Scribe_Values.Look(ref uniqueServerID, "uniqueServerID");
        }
    }
}