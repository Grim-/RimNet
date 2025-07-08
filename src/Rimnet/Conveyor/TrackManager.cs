using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public class TrackManager : MapComponent
    {
        private List<TrackNetwork> networks = new List<TrackNetwork>();
        private Dictionary<Building_Track, TrackNetwork> beltToNetworkMap = new Dictionary<Building_Track, TrackNetwork>();

        public TrackManager(Map map) : base(map) 
        { 
        
        }

        private void RebuildNetworkMap()
        {
            beltToNetworkMap.Clear();
            foreach (var network in networks)
            {
                foreach (var belt in network.Belts)
                {
                    beltToNetworkMap[belt] = network;
                }
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            foreach (var network in networks)
            {
                network.NetworkTick();
            }
        }

        public void RegisterBelt(Building_Track belt)
        {
            var adjacentNetworks = GetAdjacentNetworks(belt);

            if (!adjacentNetworks.Any())
            {
                var networkType = belt.TrackType.trackType;
                var newNetwork = (TrackNetwork)Activator.CreateInstance(networkType);

                newNetwork.AddBelt(belt);
                networks.Add(newNetwork);
                beltToNetworkMap[belt] = newNetwork;
                return;
            }

            var primaryNetwork = adjacentNetworks.First();
            if (primaryNetwork.AddBelt(belt))
            {
                beltToNetworkMap[belt] = primaryNetwork;
                if (adjacentNetworks.Count > 1)
                {
                    MergeNetworks(primaryNetwork, adjacentNetworks.Skip(1).ToList());
                }
            }
        }

        public void DeregisterBelt(Building_Track belt)
        {
            if (!beltToNetworkMap.TryGetValue(belt, out var network)) 
                return;

            var formerNeighbors = belt.GetAdjacentBelts().Where(b => network.Belts.Contains(b)).ToList();
            network.RemoveBelt(belt);
            beltToNetworkMap.Remove(belt);

            if (network.Belts.Count == 0)
            {
                networks.Remove(network);
                return;
            }

            if (formerNeighbors.Count <= 1) return;

            var firstNeighbor = formerNeighbors.First();
            var reachableFromFirst = network.DiscoverFrom(firstNeighbor);

            if (reachableFromFirst.Count == network.Belts.Count) return;

            var orphanedBelts = new HashSet<Building_Track>(network.Belts.Where(b => !reachableFromFirst.Contains(b)));
            network.SetBelts(reachableFromFirst);

            while (orphanedBelts.Any())
            {
                var newNetwork = new TrackNetwork();
                var newComponent = newNetwork.Discover(orphanedBelts.First(), orphanedBelts);

                networks.Add(newNetwork);
                foreach (var newBelt in newComponent)
                {
                    beltToNetworkMap[newBelt] = newNetwork;
                }
            }
        }

        private HashSet<TrackNetwork> GetAdjacentNetworks(Building_Track belt)
        {
            var adjacentNetworks = new HashSet<TrackNetwork>();
            foreach (var neighbor in belt.GetAdjacentBelts())
            {
                if (beltToNetworkMap.TryGetValue(neighbor, out var adjacentNetwork))
                {
                    adjacentNetworks.Add(adjacentNetwork);
                }
            }
            return adjacentNetworks;
        }

        private void MergeNetworks(TrackNetwork primary, List<TrackNetwork> toMerge)
        {
            foreach (var otherNetwork in toMerge)
            {
                foreach (var belt in otherNetwork.Belts.ToList())
                {
                    primary.AddBelt(belt);
                    beltToNetworkMap[belt] = primary;
                }
                networks.Remove(otherNetwork);
            }
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref networks, "networks", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (networks == null) networks = new List<TrackNetwork>();
                RebuildNetworkMap();
            }
        }

    }
}