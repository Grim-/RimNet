using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public class SignalManager : MapComponent
    {
        private List<SignalNetwork> networks = new List<SignalNetwork>();
        private bool rebuildRequested = true; // Start true to build once on first tick
        private int signalProcessTick = 0;
        private int signalProcessInterval = 2;

        private bool isProcessingSignals => networks.Sum(x => x.ActiveSignals.Count) > 0;


        private static readonly Dictionary<Rot4, int> DirectionPriority = new Dictionary<Rot4, int>
        {
            { Rot4.East, 0 },
            { Rot4.North, 1 },
            { Rot4.West, 2 },
            { Rot4.South, 3 }
        };

        public SignalManager(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            signalProcessTick++;
            if (signalProcessTick >= signalProcessInterval)
            {
                TickSignals();
                signalProcessTick = 0;
            }

            if (rebuildRequested && !isProcessingSignals)
            {
                RebuildNetworks();
                rebuildRequested = false;
            }
        }

        public void MarkNetworksDirty()
        {
            rebuildRequested = true;
        }

        public void TickSignals()
        {
            foreach (var network in networks.ToArray())
            {
                network.AdvanceSignals();
            }
        }

        public void RebuildNetworks()
        {
            networks.Clear();

            var allNodes = map.listerThings.AllThings
                .Select(t => t.TryGetComp<Comp_SignalNode>())
                .Where(n => n != null)
                .ToList();

            // Separate passive and regular nodes
            var regularNodes = allNodes.Where(n => !n.ExcludeFromNetworkDiscovery).ToList();
            var passiveNodes = allNodes.Where(n => n.IsPassiveNode).ToList();

            // Build networks with regular nodes only
            var visited = new HashSet<Comp_SignalNode>();
            foreach (var node in regularNodes)
            {
                if (!visited.Contains(node))
                {
                    var network = new SignalNetwork();
                    network.networkID = $"network {Rand.Range(1, 100)}";
                    network.DiscoverNetwork(node, visited);
                    if (network.HasNodes)
                    {
                        networks.Add(network);
                    }
                }
            }

            // IMPORTANT: Mark networks as clean BEFORE registering passive nodes
            rebuildRequested = false;

            // Now register passive nodes with their networks
            foreach (var passiveNode in passiveNodes)
            {
                RegisterPassiveNode(passiveNode);
            }
        }

        private void RegisterPassiveNode(Comp_SignalNode passiveNode)
        {
            // Find nodes at the same position
            var nodesAtPosition = GetAllNodesAt(passiveNode.parent.Position, map);

            foreach (var node in nodesAtPosition)
            {
                if (node != passiveNode && !node.IsPassiveNode)
                {
                    // Now GetNetworkFor won't trigger a rebuild
                    var network = GetNetworkFor(node);
                   // network?.RegisterPassiveListener(node, passiveNode);
                    break;
                }
            }
        }

        private List<Comp_SignalNode> GetAllNodesAt(IntVec3 position, Map map)
        {
            var nodes = new List<Comp_SignalNode>();
            if (!position.InBounds(map))
                return nodes;

            foreach (var thing in position.GetThingList(map))
            {
                if (thing is ThingWithComps twc)
                {
                    var node = twc.TryGetComp<Comp_SignalNode>();
                    if (node != null)
                        nodes.Add(node);
                }
            }
            return nodes;
        }

        public void SendSignal(Signal signal, Comp_SignalNode source)
        {
            var network = GetNetworkFor(source);
            network?.StartSignal(signal, source);
        }

        public SignalNetwork GetNetworkFor(Comp_SignalNode node)
        {
            //if (networksDirty)
            //{
            //    RebuildNetworks();
            //    networksDirty = false;
            //}
            return networks.FirstOrDefault(network => network.Contains(node));
        }

        public void SetSignalProcessInterval(int ticks)
        {
            signalProcessInterval = ticks;
        }

        public static List<Comp_SignalNode> GetConnectedNodesByDirection(Comp_SignalNode fromNode)
        {
            var fromPos = fromNode.parent.Position;
            return fromNode.EnabledConnectedChildren
                .OrderBy(node => GetDirectionPriority(fromPos, node.parent.Position))
                .ToList();
        }

        public static int GetDirectionPriority(IntVec3 from, IntVec3 to)
        {
            var direction = GetDirectionBetween(from, to);
            return DirectionPriority.TryGetValue(direction, out int priority) ? priority : 999;
        }

        private static Rot4 GetDirectionBetween(IntVec3 from, IntVec3 to)
        {
            var diff = to - from;
            if (diff.x > 0) return Rot4.East;
            if (diff.z > 0) return Rot4.North;
            if (diff.x < 0) return Rot4.West;
            return Rot4.South;
        }
    }
}