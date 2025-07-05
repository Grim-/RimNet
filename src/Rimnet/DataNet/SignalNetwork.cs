using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    /// <summary>
    /// Represents a signal currently propagating through the network.
    /// It holds the signal data and a list of nodes that are currently processing it.
    /// </summary>
    public class ActiveSignal
    {
        public Signal Signal { get; }
        public HashSet<Comp_SignalNode> NodesAtFront { get; set; }

        public ActiveSignal(Signal signal, Comp_SignalNode startNode)
        {
            Signal = signal;
            NodesAtFront = new HashSet<Comp_SignalNode> { startNode };
        }
    }

    public class SignalNetwork
    {
        private readonly HashSet<Comp_SignalNode> nodes = new HashSet<Comp_SignalNode>();
        // Replaced the old signal queue with a list of ActiveSignal objects.
        private readonly List<ActiveSignal> activeSignals = new List<ActiveSignal>();

        public bool HasNodes => nodes.Count > 0;

        /// <summary>
        /// Initiates a new signal propagation from a source node.
        /// </summary>
        public void StartSignal(Signal signal, Comp_SignalNode source)
        {
            if (nodes.Contains(source))
            {
                activeSignals.Add(new ActiveSignal(signal, source));
            }
        }

        /// <summary>
        /// This is the new core logic. It is called every X ticks by the SignalManager.
        /// It processes the current front of each active signal and then advances the front to the next set of nodes.
        /// </summary>
        public void AdvanceSignals()
        {
            if (!activeSignals.Any()) return;

            var signalsToRemove = new List<ActiveSignal>();

            // Iterate over a copy as the original list might be modified
            foreach (var activeSignal in activeSignals.ToList())
            {
                var nextFrontNodes = new HashSet<Comp_SignalNode>();

                // 1. Process all nodes at the current front of the signal
                foreach (var node in activeSignal.NodesAtFront)
                {
                    // Call the node's own logic for receiving a signal
                    node.OnSignalRecieved(activeSignal.Signal);

                    // 2. Find all valid children to form the next front
                    foreach (var child in node.EnabledConnectedChildren)
                    {
                        if (nodes.Contains(child))
                        {
                            nextFrontNodes.Add(child);
                        }
                    }
                }

                // 3. Check if the signal propagation should continue
                if (nextFrontNodes.Any())
                {
                    // If there are children, update the front for the next tick
                    activeSignal.NodesAtFront = nextFrontNodes;
                }
                else
                {
                    // If there are no more children, the signal has finished propagating
                    signalsToRemove.Add(activeSignal);
                }
            }

            // 4. Clean up finished signals
            activeSignals.RemoveAll(s => signalsToRemove.Contains(s));
        }

        public void DiscoverNetwork(Comp_SignalNode startNode, HashSet<Comp_SignalNode> globalVisited)
        {
            var queue = new Queue<Comp_SignalNode>();
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (globalVisited.Contains(current) || nodes.Contains(current))
                    continue;

                nodes.Add(current);
                globalVisited.Add(current);
                foreach (var child in current.ConnectedChildren)
                {
                    if (!globalVisited.Contains(child))
                        queue.Enqueue(child);
                }

                foreach (var parent in current.ConnectedParents)
                {
                    if (!globalVisited.Contains(parent))
                        queue.Enqueue(parent);
                }
            }
        }

        public bool Contains(Comp_SignalNode node)
        {
            return nodes.Contains(node);
        }

        // The following methods (FindSourceNode, GetNodesInPropagationOrder) may need
        // adjustment depending on their use, but are left as-is for now as they don't
        // interfere with the new propagation model.
        public Comp_SignalNode FindSourceNode()
        {
            return nodes.FirstOrDefault(n => n.ConnectedParents.Count == 0);
        }

        public IEnumerable<Comp_SignalNode> GetNodesInPropagationOrder()
        {
            var source = FindSourceNode();
            if (source == null)
                return Enumerable.Empty<Comp_SignalNode>();

            var ordered = new List<Comp_SignalNode>();
            var visited = new HashSet<Comp_SignalNode>();
            var queue = new Queue<Comp_SignalNode>();
            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (visited.Contains(current))
                    continue;

                visited.Add(current);
                ordered.Add(current);

                foreach (var child in current.ConnectedChildren)
                {
                    if (nodes.Contains(child) && !visited.Contains(child))
                        queue.Enqueue(child);
                }
            }
            return ordered;
        }
    }
}