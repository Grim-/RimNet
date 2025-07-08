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

            // First, identify all groups and their members
            var processedGroups = new HashSet<SignalGroup>();
            var groupedNodes = new Dictionary<Comp_SignalNode, SignalGroup>();

            foreach (var node in unclaimedNodes.ToList())
            {
                if (node.BelongsToGroup && node.SignalGroup != null && !processedGroups.Contains(node.SignalGroup))
                {
                    processedGroups.Add(node.SignalGroup);
                    foreach (var groupMember in node.SignalGroup.AllNodes)
                    {
                        groupedNodes[groupMember] = node.SignalGroup;
                    }
                }
            }

            // Process nodes
            while (unclaimedNodes.Any())
            {
                var startNode = unclaimedNodes.First();
                SignalNetwork newNetwork = new SignalNetwork($"Network-{Rand.Range(1000, 9999)}");

                // Custom discovery that respects groups
                var queue = new Queue<Comp_SignalNode>();
                queue.Enqueue(startNode);
                unclaimedNodes.Remove(startNode);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    newNetwork.JoinNetwork(current);

                    // If this node is in a group, add all group members to the queue
                    if (groupedNodes.TryGetValue(current, out var group))
                    {
                        foreach (var groupMember in group.AllNodes)
                        {
                            if (unclaimedNodes.Contains(groupMember))
                            {
                                unclaimedNodes.Remove(groupMember);
                                queue.Enqueue(groupMember);
                            }
                        }
                    }

                    // Then process normal neighbors
                    foreach (var neighbor in current.Neighbors)
                    {
                        if (neighbor != null && unclaimedNodes.Contains(neighbor))
                        {
                            unclaimedNodes.Remove(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }

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
    //    private Dictionary<Comp_SignalNode, SignalNetwork> nodeToNetworkMap = new Dictionary<Comp_SignalNode, SignalNetwork>();
    //    private bool rebuildRequested = true;
    //    private int signalProcessTick = 0;
    //    private int signalProcessInterval = 2;

    //    private bool isProcessingSignals => networks.Sum(x => x.ActiveSignals.Count) > 0;

    //    public SignalManager(Map map) : base(map) { }

    //    public void MarkNetworksDirty()
    //    {
    //        rebuildRequested = true;
    //    }

    //    public override void MapComponentTick()
    //    {
    //        base.MapComponentTick();

    //        signalProcessTick++;
    //        if (signalProcessTick >= signalProcessInterval)
    //        {
    //            foreach (var network in networks)
    //            {
    //                network.TickSignals();
    //            }
    //            signalProcessTick = 0;
    //        }

    //        if (rebuildRequested && !isProcessingSignals)
    //        {
    //            RebuildNetworks();
    //        }
    //    }

    //    public void RebuildNetworks()
    //    {
    //        networks.Clear();
    //        nodeToNetworkMap.Clear();

    //        HashSet<Comp_SignalNode> unclaimedNodes = new HashSet<Comp_SignalNode>(
    //            map.listerThings.AllThings
    //                .Select(t => t.TryGetComp<Comp_SignalNode>())
    //                .Where(n => n != null && !n.ExcludeFromNetworkDiscovery)
    //        );

    //        while (unclaimedNodes.Any())
    //        {
    //            SignalNetwork newNetwork = new SignalNetwork($"Network-{Rand.Range(1000, 9999)}");
    //            newNetwork.Discover(unclaimedNodes.First(), unclaimedNodes);

    //            if (newNetwork.HasNodes)
    //            {
    //                networks.Add(newNetwork);
    //                foreach (var node in newNetwork.Nodes)
    //                {
    //                    nodeToNetworkMap[node] = newNetwork;
    //                }
    //            }
    //        }

    //        rebuildRequested = false;
    //    }

    //    public SignalNetwork GetNetworkFor(Comp_SignalNode node)
    //    {
    //        nodeToNetworkMap.TryGetValue(node, out var network);
    //        return network;
    //    }

    //    public void SendSignal(Signal signal, Comp_SignalNode source)
    //    {
    //        var network = GetNetworkFor(source);
    //        network?.SendSignal(signal, source);
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
    //}

}