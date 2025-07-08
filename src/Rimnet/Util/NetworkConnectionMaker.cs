using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public static class NetworkConnectionMaker
    {
        public static IntVec3 ToLocalIntVec3(this Rot4 rotation)
        {
            switch (rotation.AsInt)
            {
                case 0: // North
                    return IntVec3.North;
                case 1: // East
                    return IntVec3.East;
                case 2: // South
                    return IntVec3.South;
                case 3: // West
                    return IntVec3.West;
                default:
                    return IntVec3.Zero;
            }
        }
        public static void ConnectAllNodesToServer(Comp_NetworkServer newServer)
        {
            foreach (Comp_NetworkNode node in GetPotentialNodesForServer(newServer))
            {
                if (node.ConnectedNetwork == null)
                {
                    TryConnectNodeToServer(node, newServer);
                }
            }
        }

        public static void DisconnectAllFromServer(Comp_NetworkServer deadServer, Map map)
        {
            var nodesToDisconnect = deadServer.HostedNetwork?.NetworkNodes?.ToList();
            if (nodesToDisconnect == null) return;

            foreach (var node in nodesToDisconnect)
            {
                node.LeaveNetwork();
            }
        }

        public static void TryConnectToAnyNetwork(Comp_NetworkNode node, List<RimNet> disallowedNets = null)
        {
            if (node.ConnectedNetwork != null || !node.parent.Spawned)
                return;

            var server = FindServerViaCables(node);
            if (server?.HostedNetwork != null &&
                (disallowedNets?.Contains(server.HostedNetwork) != true))
            {
                server.ConnectNode(node);
            }
        }

        public static void DisconnectFromNetwork(Comp_NetworkNode node)
        {
            if (node.ConnectedNetwork == null) return;

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
                if (!visitedCells.Add(cable.Position))
                    continue;

                var server = FindServerNearCable(cable, node.parent.Map);
                if (server != null)
                    return server;

                EnqueueAdjacentCables(cable, node.parent.Map, cablesToCheck, visitedCells);
            }

            return null;
        }

        public static Comp_NetworkServer BestServerForNode(IntVec3 nodePos, Map map, List<RimNet> disallowedNets = null)
        {
            var validServers = GetValidServers(map, disallowedNets);

            return validServers
                .OrderBy(server => (server.parent.Position - nodePos).LengthHorizontalSquared)
                .FirstOrDefault();
        }

        private static List<Thing> GetAdjacentNetworkCables(Comp_NetworkNode node)
        {
            var cables = new List<Thing>();

            foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(node.parent))
            {
                if (!cell.InBounds(node.parent.Map))
                    continue;

                cables.AddRange(
                    cell.GetThingList(node.parent.Map)
                        .Where(IsNetworkCable)
                );
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

        private static IEnumerable<Comp_NetworkNode> GetPotentialNodesForServer(Comp_NetworkServer server)
        {
            if (!server.parent.Spawned)
            {
                Log.Warning($"Can't check potential nodes for {server} because it's unspawned.");
                yield break;
            }

            var allNodes = GetAllDisconnectedNodes(server.parent.Map);

            foreach (var node in allNodes)
            {
                if (FindServerViaCables(node) == server)
                {
                    yield return node;
                }
            }
        }

        private static IEnumerable<Comp_NetworkNode> GetAllDisconnectedNodes(Map map)
        {
            return map.listerThings.AllThings
                .Select(t => t.TryGetComp<Comp_NetworkNode>())
                .Where(n => n != null &&
                           !(n is Comp_NetworkServer) &&
                           n.ConnectedNetwork == null);
        }

        private static IEnumerable<Comp_NetworkServer> GetValidServers(Map map, List<RimNet> disallowedNets)
        {
            return map.listerThings.AllThings
                .Select(t => t.TryGetComp<Comp_NetworkServer>())
                .Where(s => s?.HostedNetwork != null &&
                           (disallowedNets?.Contains(s.HostedNetwork) != true));
        }

        private static Comp_NetworkServer FindServerNearCable(Thing cable, Map map)
        {
            foreach (var adjCell in GenAdjFast.AdjacentCells8Way(cable.Position))
            {
                if (!adjCell.InBounds(map))
                    continue;

                var server = FindServerInCell(adjCell, map);
                if (server != null)
                    return server;
            }

            return null;
        }

        private static Comp_NetworkServer FindServerInCell(IntVec3 cell, Map map)
        {
            var things = cell.GetThingList(map);

            foreach (var thing in things.OfType<ThingWithComps>())
            {
                var networkComp = thing.TryGetComp<Comp_NetworkServer>();
                if (networkComp?.ConnectedNetwork != null)
                {
                    return networkComp.ConnectedNetwork.NetworkServer;
                }

                var serverComp = thing.TryGetComp<Comp_NetworkServer>();
                if (serverComp != null)
                {
                    return serverComp;
                }
            }

            return null;
        }

        private static void EnqueueAdjacentCables(Thing cable, Map map, Queue<Thing> cablesToCheck, HashSet<IntVec3> visitedCells)
        {
            foreach (var adjCell in GenAdjFast.AdjacentCells8Way(cable.Position))
            {
                if (!adjCell.InBounds(map) || visitedCells.Contains(adjCell))
                    continue;

                var cables = adjCell.GetThingList(map).Where(IsNetworkCable);
                foreach (var connectedCable in cables)
                {
                    cablesToCheck.Enqueue(connectedCable);
                }
            }
        }

        private static bool IsNetworkCable(Thing thing)
        {
            return thing.def == ThingDefOf.PowerConduit || thing.def == ThingDefOf.HiddenConduit;
        }
    }
}