using System.Collections.Generic;

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
        public HashSet<Comp_SignalNode> VisitedNodes { get; } = new HashSet<Comp_SignalNode>();

        public ActiveSignal(Signal signal, Comp_SignalNode startNode)
        {
            Signal = signal;
            NodesAtFront = new HashSet<Comp_SignalNode> { startNode };
            VisitedNodes.Add(startNode);
        }
    }
}