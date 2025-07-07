using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimNet
{

    public class SignalManager : MapComponent
    {
        private List<SignalNetwork> networks = new List<SignalNetwork>();
        private Dictionary<Comp_SignalNode, SignalNetwork> nodeToNetworkMap = new Dictionary<Comp_SignalNode, SignalNetwork>();
        private bool rebuildRequested = true;
        private int signalProcessTick = 0;
        private int signalProcessInterval = 2;

        private bool isProcessingSignals => networks.Sum(x => x.ActiveSignals.Count) > 0;

        public SignalManager(Map map) : base(map) { }

        public void MarkNetworksDirty()
        {
            rebuildRequested = true;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            signalProcessTick++;
            if (signalProcessTick >= signalProcessInterval)
            {
                foreach (var network in networks)
                {
                    network.TickSignals();
                }
                signalProcessTick = 0;
            }

            if (rebuildRequested && !isProcessingSignals)
            {
                RebuildNetworks();
            }
        }

        public void RebuildNetworks()
        {
            networks.Clear();
            nodeToNetworkMap.Clear();

            HashSet<Comp_SignalNode> unclaimedNodes = new HashSet<Comp_SignalNode>(
                map.listerThings.AllThings
                    .Select(t => t.TryGetComp<Comp_SignalNode>())
                    .Where(n => n != null && !n.ExcludeFromNetworkDiscovery)
            );

            while (unclaimedNodes.Any())
            {
                SignalNetwork newNetwork = new SignalNetwork($"Network-{Rand.Range(1000, 9999)}");
                newNetwork.Discover(unclaimedNodes.First(), unclaimedNodes);

                if (newNetwork.HasNodes)
                {
                    networks.Add(newNetwork);
                    foreach (var node in newNetwork.Nodes)
                    {
                        nodeToNetworkMap[node] = newNetwork;
                    }
                }
            }

            rebuildRequested = false;
        }

        public SignalNetwork GetNetworkFor(Comp_SignalNode node)
        {
            nodeToNetworkMap.TryGetValue(node, out var network);
            return network;
        }

        public void SendSignal(Signal signal, Comp_SignalNode source)
        {
            var network = GetNetworkFor(source);
            network?.SendSignal(signal, source);
        }

        public override void MapComponentOnGUI()
        {
            base.MapComponentOnGUI();

            foreach (var network in networks)
            {
                foreach (var signal in network.ActiveSignals)
                {
                    foreach (var node in signal.NodesAtFront)
                    {
                        Vector2 screenPos = GenMapUI.LabelDrawPosFor(node.parent.Position);
                        GUI.color = Color.yellow;
                        Widgets.Label(new Rect(screenPos.x - 20, screenPos.y - 10, 40, 20), "►");
                    }
                }
            }
        }
    }

    //public class SignalManager : MapComponent
    //{
    //    private List<SignalNetwork> networks = new List<SignalNetwork>();
    //    private bool rebuildRequested = true;
    //    private int signalProcessTick = 0;
    //    private int signalProcessInterval = 2;

    //    private bool isProcessingSignals => networks.Sum(x => x.ActiveSignals.Count) > 0;


    //    private static readonly Dictionary<Rot4, int> DirectionPriority = new Dictionary<Rot4, int>
    //    {
    //        { Rot4.East, 0 },
    //        { Rot4.North, 1 },
    //        { Rot4.West, 2 },
    //        { Rot4.South, 3 }
    //    };

    //    public SignalManager(Map map) : base(map)
    //    {
    //    }

    //    public override void MapComponentTick()
    //    {
    //        base.MapComponentTick();

    //        signalProcessTick++;
    //        if (signalProcessTick >= signalProcessInterval)
    //        {
    //            TickSignals();
    //            signalProcessTick = 0;
    //        }

    //        if (rebuildRequested && !isProcessingSignals)
    //        {
    //            RebuildNetworks();
    //            rebuildRequested = false;
    //        }
    //    }


    //    public override void MapComponentOnGUI()
    //    {
    //        base.MapComponentOnGUI();

    //        foreach (var network in networks)
    //        {
    //            foreach (var signal in network.ActiveSignals)
    //            {
    //                foreach (var node in signal.NodesAtFront)
    //                {
    //                    Vector2 screenPos = GenMapUI.LabelDrawPosFor(node.parent.Position);
    //                    GUI.color = Color.yellow;
    //                    Widgets.Label(new Rect(screenPos.x - 20, screenPos.y - 10, 40, 20), "►");
    //                }
    //            }
    //        }
    //    }

    //    public void MarkNetworksDirty()
    //    {
    //        rebuildRequested = true;
    //    }

    //    public void TickSignals()
    //    {
    //        foreach (var network in networks.ToArray())
    //        {
    //            network.TickSignals();
    //        }
    //    }

    //    public void RebuildNetworks()
    //    {
    //        networks.Clear();

    //        var allNodes = map.listerThings.AllThings
    //            .Select(t => t.TryGetComp<Comp_SignalNode>())
    //            .Where(n => n != null)
    //            .ToList();

    //        var regularNodes = allNodes.Where(n => !n.ExcludeFromNetworkDiscovery).ToList();
    //        var passiveNodes = allNodes.Where(n => n.IsPassiveNode).ToList();

    //        var visited = new HashSet<Comp_SignalNode>();
    //        foreach (var node in regularNodes)
    //        {
    //            if (!visited.Contains(node))
    //            {
    //                var network = new SignalNetwork();
    //                network.networkID = $"network {Rand.Range(1, 100)}";
    //                network.DiscoverNetwork(node, visited);
    //                if (network.HasNodes)
    //                {
    //                    networks.Add(network);
    //                }
    //            }
    //        }

    //        rebuildRequested = false;

    //        foreach (var passiveNode in passiveNodes)
    //        {
    //            RegisterPassiveNode(passiveNode);
    //        }
    //    }

    //    private void RegisterPassiveNode(Comp_SignalNode passiveNode)
    //    {
    //        var nodesAtPosition = GetAllNodesAt(passiveNode.parent.Position, map);

    //        foreach (var node in nodesAtPosition)
    //        {
    //            if (node != passiveNode && !node.IsPassiveNode)
    //            {
    //                break;
    //            }
    //        }
    //    }

    //    private List<Comp_SignalNode> GetAllNodesAt(IntVec3 position, Map map)
    //    {
    //        var nodes = new List<Comp_SignalNode>();
    //        if (!position.InBounds(map))
    //            return nodes;

    //        foreach (var thing in position.GetThingList(map))
    //        {
    //            if (thing is ThingWithComps twc)
    //            {
    //                var node = twc.TryGetComp<Comp_SignalNode>();
    //                if (node != null)
    //                    nodes.Add(node);
    //            }
    //        }
    //        return nodes;
    //    }

    //    public void SendSignal(Signal signal, Comp_SignalNode source)
    //    {
    //        var network = GetNetworkFor(source);
    //        network?.SendSignal(signal, source);
    //    }

    //    public SignalNetwork GetNetworkFor(Comp_SignalNode node)
    //    {
    //        return networks.FirstOrDefault();
    //    }

    //    public void SetSignalProcessInterval(int ticks)
    //    {
    //        signalProcessInterval = ticks;
    //    }

    //    public static List<Comp_SignalNode> GetConnectedNodesByDirection(Comp_SignalNode fromNode)
    //    {
    //        var fromPos = fromNode.parent.Position;
    //        return fromNode.EnabledConnectedChildren
    //            .OrderBy(node => GetDirectionPriority(fromPos, node.parent.Position))
    //            .ToList();
    //    }

    //    public static int GetDirectionPriority(IntVec3 from, IntVec3 to)
    //    {
    //        var direction = GetDirectionBetween(from, to);
    //        return DirectionPriority.TryGetValue(direction, out int priority) ? priority : 999;
    //    }

    //    private static Rot4 GetDirectionBetween(IntVec3 from, IntVec3 to)
    //    {
    //        var diff = to - from;
    //        if (diff.x > 0)
    //            return Rot4.East;
    //        if (diff.z > 0)
    //            return Rot4.North;
    //        if (diff.x < 0)
    //            return Rot4.West;
    //        return Rot4.South;
    //    }
    //}
}