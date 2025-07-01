using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public static class NetworkConnectionMaker
    {
        public static void ConnectAllNodesToServer(Comp_NetworkServer newServer)
        {
            foreach (Comp_NetworkNode node in PotentialNodesForServer(newServer))
            {
                if (node.ConnectedNetwork == null)
                {
                    TryConnectNodeToServer(node, newServer);
                }
            }
        }

        public static void DisconnectAllFromServer(Comp_NetworkServer deadServer, Map map)
        {
            if (deadServer.HostedNetwork?.NetworkNodes == null)
                return;

            var nodesToDisconnect = deadServer.HostedNetwork.NetworkNodes.ToList();
            foreach (var node in nodesToDisconnect)
            {
                node.LeaveNetwork();
            }
        }

        public static void TryConnectToAnyNetwork(Comp_NetworkNode node, List<RimNet> disallowedNets = null)
        {
            if (node.ConnectedNetwork != null)
                return;

            if (!node.parent.Spawned)
                return;

            var server = FindServerViaCables(node);
            if (server != null && server.HostedNetwork != null &&
                (disallowedNets == null || !disallowedNets.Contains(server.HostedNetwork)))
            {
                server.ConnectNode(node);
            }
        }

        public static void DisconnectFromNetwork(Comp_NetworkNode node)
        {
            if (node.ConnectedNetwork == null)
                return;

            node.ConnectedNetwork.UnregisterNode(node);
            node.LeaveNetwork();
        }

        public static Comp_NetworkServer FindServerViaCables(Comp_NetworkNode node)
        {
            var adjacentCables = GetAdjacentNetworkCables(node);
            var visitedCells = new HashSet<IntVec3>();
            var cablesToCheck = new Queue<Thing>(adjacentCables);

            while (cablesToCheck.Count > 0)
            {
                var cable = cablesToCheck.Dequeue();
                if (visitedCells.Contains(cable.Position))
                    continue;
                visitedCells.Add(cable.Position);

                // Check all cells adjacent to this cable for servers
                foreach (var adjCell in GenAdjFast.AdjacentCells8Way(cable.Position))
                {
                    if (!adjCell.InBounds(node.parent.Map))
                        continue;

                    // Also check for non-building servers
                    var things = adjCell.GetThingList(node.parent.Map);
                    foreach (var thing in things)
                    {
                        if (thing is ThingWithComps twc)
                        {
                            var serverComp = twc.TryGetComp<Comp_NetworkServer>();
                            if (serverComp != null)
                            {
                                return serverComp;
                            }
                        }
                    }

                    // Add connected cables to the queue
                    if (!visitedCells.Contains(adjCell))
                    {
                        var cables = things.Where(t => t.def == RimNetDefOf.NetworkCable);
                        foreach (var connectedCable in cables)
                        {
                            cablesToCheck.Enqueue(connectedCable);
                        }
                    }
                }
            }

            return null;
        }

        private static List<Thing> GetAdjacentNetworkCables(Comp_NetworkNode node)
        {
            var cables = new List<Thing>();
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(node.parent))
            {
                if (!cell.InBounds(node.parent.Map)) 
                    continue; 


                foreach (Thing t in cell.GetThingList(node.parent.Map))
                {
                    if (t.def == RimNetDefOf.NetworkCable)
                    {
                        cables.Add(t);
                    }
                }
            }
            return cables;
        }

        private static bool TryConnectNodeToServer(Comp_NetworkNode node, Comp_NetworkServer server)
        {
            var foundServer = FindServerViaCables(node);
            if (foundServer == server && node.ConnectedNetwork == null)
            {
                server.ConnectNode(node);
                return true;
            }
            return false;
        }

        private static IEnumerable<Comp_NetworkNode> PotentialNodesForServer(Comp_NetworkServer server)
        {
            if (!server.parent.Spawned)
            {
                Log.Warning($"Can't check potential nodes for {server} because it's unspawned.");
                yield break;
            }

            var allNodes = server.parent.Map.listerThings.AllThings
                .Where(t => t.TryGetComp<Comp_NetworkNode>() != null)
                .Select(t => t.TryGetComp<Comp_NetworkNode>())
                .Where(n => !(n is Comp_NetworkServer) && n.ConnectedNetwork == null);

            foreach (var node in allNodes)
            {
                var foundServer = FindServerViaCables(node);
                if (foundServer == server)
                {
                    yield return node;
                }
            }
        }

        public static Comp_NetworkServer BestServerForNode(IntVec3 nodePos, Map map, List<RimNet> disallowedNets = null)
        {
            var allServers = map.listerThings.AllThings
                .Where(t => t.TryGetComp<Comp_NetworkServer>() != null)
                .Select(t => t.TryGetComp<Comp_NetworkServer>())
                .Where(s => s.HostedNetwork != null &&
                           (disallowedNets == null || !disallowedNets.Contains(s.HostedNetwork)));

            float bestDistance = 999999f;
            Comp_NetworkServer result = null;

            foreach (var server in allServers)
            {
                float distance = (server.parent.Position - nodePos).LengthHorizontalSquared;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    result = server;
                }
            }

            return result;
        }
    }
}