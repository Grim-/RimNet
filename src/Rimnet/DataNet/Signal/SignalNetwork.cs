using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public readonly struct PropagationTarget
    {
        public readonly Comp_SignalNode Node;
        public readonly SignalPort ReceivingPort;

        public PropagationTarget(Comp_SignalNode node, SignalPort receivingPort)
        {
            Node = node;
            ReceivingPort = receivingPort;
        }
    }




    public class SignalNetwork
    {
        public string networkID;
        private readonly HashSet<Comp_SignalNode> nodes = new HashSet<Comp_SignalNode>();
        private readonly List<ActiveSignal> activeSignals = new List<ActiveSignal>();

        public IEnumerable<Comp_SignalNode> Nodes => nodes;
        public List<ActiveSignal> ActiveSignals => activeSignals;
        public bool HasNodes => nodes.Any();

        public SignalNetwork(string networkID)
        {
            this.networkID = networkID;
        }

        public void Discover(Comp_SignalNode startNode, HashSet<Comp_SignalNode> unclaimedNodes)
        {
            var queue = new Queue<Comp_SignalNode>();

            if (unclaimedNodes.Contains(startNode))
            {
                queue.Enqueue(startNode);
                unclaimedNodes.Remove(startNode);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                this.nodes.Add(current);

                foreach (var neighbor in current.Neighbors)
                {
                    if (neighbor != null && unclaimedNodes.Contains(neighbor))
                    {
                        unclaimedNodes.Remove(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        public bool Contains(Comp_SignalNode node) => nodes.Contains(node);

        public void TickSignals()
        {
            if (!activeSignals.Any())
                return;

            var signalsToRemove = new List<ActiveSignal>();

            foreach (var activeSignal in activeSignals.ToList())
            {
                var nextFrontNodes = new HashSet<Comp_SignalNode>();

                foreach (var node in activeSignal.NodesAtFront)
                {
                    foreach (var propagationTarget in node.GetPropagationTargets())
                    {
                        if (!activeSignal.VisitedNodes.Contains(propagationTarget.Node))
                        {
                            activeSignal.VisitedNodes.Add(propagationTarget.Node);
                            propagationTarget.Node.OnSignalRecieved(activeSignal.Signal, propagationTarget.ReceivingPort);
                            nextFrontNodes.Add(propagationTarget.Node);
                        }
                    }
                }

                if (nextFrontNodes.Any())
                {
                    activeSignal.NodesAtFront = nextFrontNodes;
                }
                else
                {
                    signalsToRemove.Add(activeSignal);
                }
            }

            activeSignals.RemoveAll(s => signalsToRemove.Contains(s));
        }

        public void SendSignal(Signal signal, Comp_SignalNode source)
        {
            if (nodes.Contains(source))
            {
                activeSignals.Add(new ActiveSignal(signal, source));
            }
        }
    }



    //public class SignalNetwork
    //{
    //    public string networkID = "";

    //    private readonly HashSet<Comp_SignalNode> nodes = new HashSet<Comp_SignalNode>();
    //    private readonly List<ActiveSignal> activeSignals = new List<ActiveSignal>();
    //    public HashSet<Comp_SignalNode> NodesOnNetwork => nodes.ToHashSet();

    //    public List<ActiveSignal> ActiveSignals => activeSignals;
    //    public bool HasNodes => nodes.Count > 0;

    //    public void SendSignal(Signal signal, Comp_SignalNode source)
    //    {
    //        if (nodes.Contains(source))
    //        {
    //            activeSignals.Add(new ActiveSignal(signal, source));
    //        }
    //    }

    //    /// <summary>
    //    /// Propagates the signal forward, depth first, one node per tick.
    //    /// </summary>
    //    public void TickSignals()
    //    {
    //        if (!activeSignals.Any())
    //            return;

    //        var signalsToRemove = new List<ActiveSignal>();

    //        foreach (var activeSignal in activeSignals.ToList())
    //        {
    //            var nextFrontNodes = new HashSet<Comp_SignalNode>();

    //            foreach (var node in activeSignal.NodesAtFront)
    //            {
    //                // Iterate through the ports of the current node.
    //                foreach (var sendingPort in node.AllConnectedPorts)
    //                {
    //                    // CHECK 1: If the port on the sending node is disabled, skip it.
    //                    if (!sendingPort.Enabled || sendingPort.Type == SignalPortType.IN)
    //                    {
    //                        continue;
    //                    }

    //                    var neighborNode = sendingPort.ConnectedNode;
    //                    var receivingPort = sendingPort.ConnectedPort;

    //                    // CHECK 2: If the connection is invalid or the receiving port is disabled, skip it.
    //                    if (neighborNode == null || receivingPort == null || !receivingPort.Enabled || !sendingPort.OwnerNode.CanSendSignal)
    //                    {
    //                        continue;
    //                    }

    //                    // CHECK 3: If the signal has already been to this neighbor, skip it to prevent loops.
    //                    if (activeSignal.VisitedNodes.Contains(neighborNode))
    //                    {
    //                        continue;
    //                    }

    //                    // If all checks pass, propagate the signal.
    //                    activeSignal.VisitedNodes.Add(neighborNode);
    //                    neighborNode.OnSignalRecieved(activeSignal.Signal, receivingPort);
    //                    nextFrontNodes.Add(neighborNode);
    //                }
    //            }

    //            if (nextFrontNodes.Any())
    //            {
    //                activeSignal.NodesAtFront = nextFrontNodes;
    //            }
    //            else
    //            {
    //                signalsToRemove.Add(activeSignal);
    //            }
    //        }

    //        activeSignals.RemoveAll(s => signalsToRemove.Contains(s));
    //    }


    //    /// <summary>
    //    /// Crawl a network from a given node, recursively following their children and adding them to the network.
    //    /// </summary>
    //    /// <param name="startNode"></param>
    //    /// <param name="globalVisited"></param>
    //    public void DiscoverNetwork(Comp_SignalNode startNode, HashSet<Comp_SignalNode> globalVisited)
    //    {
    //        var queue = new Queue<Comp_SignalNode>();
    //        queue.Enqueue(startNode);

    //        while (queue.Count > 0)
    //        {
    //            var current = queue.Dequeue();
    //            if (globalVisited.Contains(current) || nodes.Contains(current))
    //                continue;

    //            nodes.Add(current);
    //            globalVisited.Add(current);

    //            // Follow connections
    //            foreach (var child in current.Neighbors)
    //            {
    //                if (!globalVisited.Contains(child))
    //                    queue.Enqueue(child);
    //            }

    //            // ALSO include adjacent nodes even if not connected
    //            foreach (var cell in GenAdjFast.AdjacentCellsCardinal(current.parent.Position))
    //            {
    //                var adjacentNode = current.GetNodeAt(cell);
    //                if (adjacentNode != null && !adjacentNode.IsPassiveNode && !adjacentNode.ExcludeFromNetworkDiscovery)
    //                {
    //                    if (!globalVisited.Contains(adjacentNode))
    //                        queue.Enqueue(adjacentNode);
    //                }
    //            }
    //        }
    //    }

    //    public void ClearNetwork()
    //    {
    //        nodes.Clear();
    //    }

    //    public bool Contains(Comp_SignalNode node)
    //    {
    //        return nodes.Contains(node);
    //    }

    //    public Comp_SignalNode FindSourceNode()
    //    {
    //        return nodes.FirstOrDefault(n => n.ConnectedParents.Count == 0);
    //    }

    //    public IEnumerable<Comp_SignalNode> GetNodesInPropagationOrder()
    //    {
    //        var source = FindSourceNode();
    //        if (source == null)
    //            return Enumerable.Empty<Comp_SignalNode>();

    //        var ordered = new List<Comp_SignalNode>();
    //        var visited = new HashSet<Comp_SignalNode>();
    //        var queue = new Queue<Comp_SignalNode>();
    //        queue.Enqueue(source);

    //        while (queue.Count > 0)
    //        {
    //            var current = queue.Dequeue();
    //            if (visited.Contains(current))
    //                continue;

    //            visited.Add(current);
    //            ordered.Add(current);

    //            foreach (var child in current.ConnectedChildren)
    //            {
    //                if (nodes.Contains(child) && !visited.Contains(child))
    //                    queue.Enqueue(child);
    //            }
    //        }
    //        return ordered;
    //    }
    //}
}