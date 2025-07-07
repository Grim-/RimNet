using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public static class SignalConnectionMaker
    {
        public static void UpdateConnectionsFor(Comp_SignalNode node)
        {
            if (node == null || !node.parent.Spawned) return;

            foreach (var cell in GenAdjFast.AdjacentCellsCardinal(node.parent.Position))
            {
                var adjacentNodes = GetNodesAt(cell, node.parent.Map);
                foreach (var otherNode in adjacentNodes)
                {
                    if (otherNode == null || otherNode == node)
                        continue;
                    TryMakeBestConnection(node, otherNode);
                }
            }
        }

        private static void TryMakeBestConnection(Comp_SignalNode nodeA, Comp_SignalNode nodeB)
        {
            var potentialConnections = new List<(SignalPort sourcePort, SignalPort targetPort, int priority)>();

            // Only check valid source->target connections
            var sourcePortsA = nodeA.AllUnConnectedPorts.Where(p => p.Type == SignalPortType.OUT || p.Type == SignalPortType.BOTH);
            var targetPortsB = nodeB.AllUnConnectedPorts.Where(p => p.Type == SignalPortType.IN || p.Type == SignalPortType.BOTH);

            foreach (var sourcePort in sourcePortsA)
            {
                foreach (var targetPort in targetPortsB)
                {
                    if (targetPort.AutoConnectable && sourcePort.OwnerNode.CanConnectTo(sourcePort, targetPort.OwnerNode, targetPort, out _))
                    {
                        int priority = targetPort.OwnerNode.GetConnectionPriority(sourcePort.OwnerNode);
                        potentialConnections.Add((sourcePort, targetPort, priority));
                    }
                }
            }

            var sourcePortsB = nodeB.AllUnConnectedPorts.Where(p => p.Type == SignalPortType.OUT || p.Type == SignalPortType.BOTH);
            var targetPortsA = nodeA.AllUnConnectedPorts.Where(p => p.Type == SignalPortType.IN || p.Type == SignalPortType.BOTH);

            foreach (var sourcePort in sourcePortsB)
            {
                foreach (var targetPort in targetPortsA)
                {
                    if (targetPort.AutoConnectable && sourcePort.OwnerNode.CanConnectTo(sourcePort, targetPort.OwnerNode, targetPort, out _))
                    {
                        int priority = targetPort.OwnerNode.GetConnectionPriority(sourcePort.OwnerNode);
                        potentialConnections.Add((sourcePort, targetPort, priority));
                    }
                }
            }

            if (!potentialConnections.Any())
            {
                return;
            }

            var bestConnection = potentialConnections.OrderByDescending(c => c.priority).FirstOrDefault();
            if (bestConnection.sourcePort != null && bestConnection.targetPort != null)
            {
                bestConnection.sourcePort.Connect(bestConnection.targetPort);
            }
        }

        private static Rot4 GetRelativeDirection(IntVec3 from, IntVec3 to)
        {
            var diff = to - from;
            if (diff.x > 0) return Rot4.East;
            if (diff.z > 0) return Rot4.North;
            if (diff.x < 0) return Rot4.West;
            return Rot4.South;
        }

        private static List<Comp_SignalNode> GetNodesAt(IntVec3 cell, Map map)
        {
            var nodes = new List<Comp_SignalNode>();
            if (!cell.InBounds(map)) return nodes;

            foreach (var thing in cell.GetThingList(map))
            {
                var comp = thing.TryGetComp<Comp_SignalNode>();
                if (comp != null)
                {
                    nodes.Add(comp);
                }
            }
            return nodes;
        }
    }


    //public static class SignalConnectionMaker
    //{
    //    public static void UpdateConnectionsFor(Comp_SignalNode node)
    //    {
    //        if (node == null || !node.parent.Spawned) return;

    //        foreach (var cell in GenAdjFast.AdjacentCellsCardinal(node.parent.Position))
    //        {
    //            var adjacentNodes = GetNodesAt(cell, node.parent.Map);
    //            foreach (var otherNode in adjacentNodes)
    //            {
    //                if (otherNode == null || otherNode == node) 
    //                    continue;
    //                TryMakeBestConnection(node, otherNode);
    //            }
    //        }
    //    }

    //    private static void TryMakeBestConnection(Comp_SignalNode nodeA, Comp_SignalNode nodeB)
    //    {
    //        var potentialConnections = new List<(SignalPort sourcePort, SignalPort targetPort, int priority)>();

    //        // Get all ports that can act as a source (OUT or BOTH)
    //        var sourcePortsA = nodeA.AllUnConnectedPorts.Where(p => p.Type == SignalPortType.OUT || p.Type == SignalPortType.BOTH);
    //        var sourcePortsB = nodeB.AllUnConnectedPorts.Where(p => p.Type == SignalPortType.OUT || p.Type == SignalPortType.BOTH);

    //        // Get all ports that can act as a target (IN or BOTH)
    //        var targetPortsA = nodeA.AllUnConnectedPorts.Where(p => p.Type == SignalPortType.IN || p.Type == SignalPortType.BOTH);
    //        var targetPortsB = nodeB.AllUnConnectedPorts.Where(p => p.Type == SignalPortType.IN || p.Type == SignalPortType.BOTH);

    //        foreach (var sourcePort in sourcePortsA)
    //        {
    //            foreach (var targetPort in targetPortsB)
    //            {
    //                if (sourcePort == targetPort) 
    //                    continue;

    //                if (!targetPort.AutoConnectable)
    //                {
    //                    continue;
    //                }

    //                int priority = targetPort.OwnerNode.GetConnectionPriority(sourcePort.OwnerNode);
    //                potentialConnections.Add((sourcePort, targetPort, priority));
    //            }
    //        }

    //        foreach (var sourcePort in sourcePortsB)
    //        {
    //            foreach (var targetPort in targetPortsA)
    //            {
    //                if (sourcePort == targetPort) 
    //                    continue;

    //                if (!targetPort.AutoConnectable)
    //                {
    //                    continue;
    //                }
    //                int priority = targetPort.OwnerNode.GetConnectionPriority(sourcePort.OwnerNode);
    //                potentialConnections.Add((sourcePort, targetPort, priority));
    //            }
    //        }

    //        if (!potentialConnections.Any())
    //        {
    //            return;
    //        }

    //        var bestConnection = potentialConnections.OrderByDescending(c => c.priority).FirstOrDefault();

    //        if (bestConnection.sourcePort != null && bestConnection.targetPort != null)
    //        {
    //            bestConnection.sourcePort.Connect(bestConnection.targetPort);
    //        }
    //    }

    //    private static List<Comp_SignalNode> GetNodesAt(IntVec3 cell, Map map)
    //    {
    //        var nodes = new List<Comp_SignalNode>();
    //        if (!cell.InBounds(map)) return nodes;

    //        foreach (var thing in cell.GetThingList(map))
    //        {
    //            var comp = thing.TryGetComp<Comp_SignalNode>();
    //            if (comp != null)
    //            {
    //                nodes.Add(comp);
    //            }
    //        }
    //        return nodes;
    //    }
    //}


    //public static class SignalConnectionMaker
    //{
    //    public static void UpdateConnectionsFor(Comp_SignalNode node)
    //    {
    //        if (node == null || !node.parent.Spawned || node.IsPassiveNode) 
    //            return;

    //        foreach (var cell in GenAdjFast.AdjacentCellsCardinal(node.parent.Position))
    //        {
    //            var adjacentNodes = GetNodesAt(cell, node.parent.Map);
    //            foreach (var otherNode in adjacentNodes)
    //            {
    //                if (otherNode == null || otherNode.IsPassiveNode || otherNode == node) 
    //                    continue;

    //                TryMakeBestConnection(node, otherNode);
    //            }
    //        }
    //    }

    //    private static void TryMakeBestConnection(Comp_SignalNode nodeA, Comp_SignalNode nodeB)
    //    {
    //        var potentialConnections = new List<(SignalPort sourcePort, SignalPort targetPort, int priority)>();

    //        foreach (var outPortA in nodeA.GetUnconnectedPorts(SignalPortType.OUT))
    //        {
    //            foreach (var inPortB in nodeB.GetUnconnectedPorts(SignalPortType.IN))
    //            {
    //                int priority = inPortB.OwnerNode.GetConnectionPriority(outPortA.OwnerNode);
    //                potentialConnections.Add((outPortA, inPortB, priority));
    //            }
    //        }

    //        foreach (var outPortB in nodeB.GetUnconnectedPorts(SignalPortType.OUT))
    //        {
    //            foreach (var inPortA in nodeA.GetUnconnectedPorts(SignalPortType.IN))
    //            {
    //                int priority = inPortA.OwnerNode.GetConnectionPriority(outPortB.OwnerNode);
    //                potentialConnections.Add((outPortB, inPortA, priority));
    //            }
    //        }

    //        if (!potentialConnections.Any())
    //        {
    //            return;
    //        }

    //        var bestConnection = potentialConnections.OrderByDescending(c => c.priority).First();

    //        bestConnection.sourcePort.Connect(bestConnection.targetPort);
    //    }




    //    private static List<Comp_SignalNode> GetConnectableNeighbors(Comp_SignalNode node, HashSet<Comp_SignalNode> visited)
    //    {
    //        var neighbors = new List<Comp_SignalNode>();

    //        foreach (var cell in GenAdjFast.AdjacentCellsCardinal(node.parent))
    //        {
    //            var nodesAtCell = GetNodesAt(cell, node.parent.Map);
    //            foreach (var neighbor in nodesAtCell)
    //            {
    //                if (neighbor != null && neighbor != node && !visited.Contains(neighbor))
    //                {
    //                    if (CanConnect(node, neighbor) || CanConnect(neighbor, node))
    //                        neighbors.Add(neighbor);
    //                }
    //            }
    //        }

    //        return neighbors;
    //    }

    //    private static List<Comp_SignalNode> GetNodesAt(IntVec3 cell, Map map)
    //    {
    //        var nodes = new List<Comp_SignalNode>();

    //        if (!cell.InBounds(map))
    //            return nodes;

    //        foreach (var thing in cell.GetThingList(map))
    //        {
    //            if (thing is ThingWithComps twc)
    //            {
    //                var nodeComps = twc.GetComps<Comp_SignalNode>();
    //                if (nodeComps.Any())
    //                    nodes.Add(nodeComps.First());
    //            }
    //        }
    //        return nodes;
    //    }



    //    private static bool CanConnect(Comp_SignalNode source, Comp_SignalNode target)
    //    {
    //        return source.HasConnectionPort(SignalPortType.OUT) &&
    //                target.HasConnectionPort(SignalPortType.IN) &&
    //               !target.IsNodeDescendantOf(source);
    //    }

    //    private static bool TryConnect(Comp_SignalNode source, Comp_SignalNode target)
    //    {
    //        if (!CanConnect(source, target))
    //            return false;

    //        return TryMakeValidConnection(source, target);
    //    }


    //    private static bool TryMakeValidConnection(Comp_SignalNode source, Comp_SignalNode target)
    //    {
    //        var sourcePorts = source.GetUnconnectedPorts(SignalPortType.OUT);
    //        var targetPorts = target.GetUnconnectedPorts(SignalPortType.IN);

    //        foreach (var sourcePort in sourcePorts)
    //        {
    //            foreach (var targetPort in targetPorts)
    //            {
    //                if (source.CanConnectTo(sourcePort, target, targetPort, out _))
    //                {
    //                    source.MakeConnection(sourcePort, target, targetPort, out _);
    //                    return true;
    //                }
    //            }
    //        }
    //        return false;
    //    }
    //}









    //public static class SignalConnectionMaker
    //{
    //    public static void AutoConnectNode(Comp_SignalNode lastPlacedNode, List<Comp_SignalNode> excludeNodes = null)
    //    {
    //        if (lastPlacedNode == null || !lastPlacedNode.parent.Spawned)
    //            return;

    //        Comp_SignalNode bestSource = null;
    //        Comp_SignalNode bestTarget = null;
    //        int bestPriority = int.MinValue;

    //        List<IntVec3> cells = GenAdjFast.AdjacentCellsCardinal(lastPlacedNode.parent);
    //        cells.Add(lastPlacedNode.parent.Position);
    //        foreach (var cell in cells)
    //        {
    //            var otherNode = GetNodeAt(cell, lastPlacedNode.parent.Map);
    //            if (otherNode == null || otherNode == lastPlacedNode || IsExcluded(otherNode, excludeNodes))
    //                continue;

    //            // Determine signal direction (who's source, who's target)
    //            bool otherHasIncomingSignal = HasIncomingSignalFlow(otherNode);
    //            bool lastPlacedHasIncomingSignal = HasIncomingSignalFlow(lastPlacedNode);

    //            Comp_SignalNode source;
    //            Comp_SignalNode target;

    //            if (otherHasIncomingSignal && !lastPlacedHasIncomingSignal)
    //            {
    //                source = otherNode;
    //                target = lastPlacedNode;
    //            }
    //            else if (!otherHasIncomingSignal && lastPlacedHasIncomingSignal)
    //            {
    //                source = lastPlacedNode;
    //                target = otherNode;
    //            }
    //            else
    //            {
    //                Comp_SignalNode sourceNode = FindNetworkSource(otherNode);
    //                if (sourceNode != null)
    //                {
    //                    if (IsCloserToSource(otherNode, lastPlacedNode, sourceNode))
    //                    {
    //                        source = otherNode;
    //                        target = lastPlacedNode;
    //                    }
    //                    else
    //                    {
    //                        source = lastPlacedNode;
    //                        target = otherNode;
    //                    }
    //                }
    //                else
    //                {
    //                    source = otherNode;
    //                    target = lastPlacedNode;
    //                }
    //            }

    //            if (!CanConnect(source, target))
    //                continue;

    //            // Evaluate priority
    //            int priority = target.GetConnectionPriority(source);

    //            if (priority > bestPriority)
    //            {
    //                bestPriority = priority;
    //                bestSource = source;
    //                bestTarget = target;
    //            }
    //        }

    //        if (bestSource != null && bestTarget != null)
    //        {
    //            TryConnect(bestSource, bestTarget);
    //        }
    //    }

    //    private static bool HasIncomingSignalFlow(Comp_SignalNode node)
    //    {
    //        return node.ConnectedParents.Count > 0;
    //    }

    //    private static Comp_SignalNode FindNetworkSource(Comp_SignalNode node)
    //    {
    //        var visited = new HashSet<Comp_SignalNode>();
    //        var current = node;

    //        while (current != null && !visited.Contains(current))
    //        {
    //            visited.Add(current);
    //            if (current.ConnectedParents.Count == 0)
    //                return current;
    //            current = current.ConnectedParents.FirstOrDefault();
    //        }

    //        return null;
    //    }

    //    private static bool IsCloserToSource(Comp_SignalNode node1, Comp_SignalNode node2, Comp_SignalNode source)
    //    {
    //        return GetDistanceToSource(node1, source) < GetDistanceToSource(node2, source);
    //    }

    //    private static int GetDistanceToSource(Comp_SignalNode node, Comp_SignalNode source)
    //    {
    //        var visited = new HashSet<Comp_SignalNode>();
    //        var queue = new Queue<(Comp_SignalNode node, int distance)>();
    //        queue.Enqueue((node, 0));

    //        while (queue.Count > 0)
    //        {
    //            var (current, distance) = queue.Dequeue();
    //            if (current == source)
    //                return distance;

    //            if (visited.Contains(current))
    //                continue;

    //            visited.Add(current);

    //            foreach (var parent in current.ConnectedParents)
    //                queue.Enqueue((parent, distance + 1));
    //        }

    //        return int.MaxValue;
    //    }

    //    public static void RecursivelyConnectForward(Comp_SignalNode startNode)
    //    {
    //        if (startNode == null || !startNode.parent.Spawned)
    //            return;

    //        var visited = new HashSet<Comp_SignalNode>();
    //        RecursiveConnectInternal(startNode, visited);
    //    }

    //    private static void RecursiveConnectInternal(Comp_SignalNode currentNode, HashSet<Comp_SignalNode> visited)
    //    {
    //        if (currentNode == null || visited.Contains(currentNode))
    //            return;

    //        visited.Add(currentNode);

    //        var connectableNodes = GetConnectableNeighbors(currentNode, visited);

    //        foreach (var neighbor in connectableNodes)
    //        {
    //            if (TryConnect(currentNode, neighbor) || TryConnect(neighbor, currentNode))
    //            {
    //                RecursiveConnectInternal(neighbor, visited);
    //            }
    //        }
    //    }

    //    private static List<Comp_SignalNode> GetConnectableNeighbors(Comp_SignalNode node, HashSet<Comp_SignalNode> visited)
    //    {
    //        var neighbors = new List<Comp_SignalNode>();

    //        foreach (var cell in GenAdjFast.AdjacentCellsCardinal(node.parent))
    //        {
    //            var neighbor = GetNodeAt(cell, node.parent.Map);
    //            if (neighbor != null && neighbor != node && !visited.Contains(neighbor))
    //            {
    //                if (CanConnect(node, neighbor) || CanConnect(neighbor, node))
    //                    neighbors.Add(neighbor);
    //            }
    //        }

    //        return neighbors;
    //    }

    //    private static Comp_SignalNode GetNodeAt(IntVec3 cell, Map map)
    //    {
    //        if (!cell.InBounds(map))
    //            return null;

    //        foreach (var thing in cell.GetThingList(map))
    //        {
    //            if (thing is ThingWithComps twc)
    //            {
    //                var node = twc.TryGetComp<Comp_SignalNode>();
    //                if (node != null)
    //                    return node;
    //            }
    //        }
    //        return null;
    //    }

    //    private static bool IsExcluded(Comp_SignalNode node, List<Comp_SignalNode> excludeNodes)
    //    {
    //        return excludeNodes != null && excludeNodes.Contains(node);
    //    }

    //    private static bool CanConnect(Comp_SignalNode source, Comp_SignalNode target)
    //    {
    //        return source.HasConnectionPort(SignalPortType.OUT) &&
    //                target.HasConnectionPort(SignalPortType.IN) &&
    //               !target.IsNodeDescendantOf(source);
    //    }

    //    private static bool TryConnect(Comp_SignalNode source, Comp_SignalNode target)
    //    {
    //        if (!CanConnect(source, target))
    //            return false;

    //        return TryMakeValidConnection(source, target);
    //    }


    //    private static bool TryMakeValidConnection(Comp_SignalNode source, Comp_SignalNode target)
    //    {
    //        var sourcePorts = source.GetUnconnectedPorts(SignalPortType.OUT);
    //        var targetPorts = target.GetUnconnectedPorts(SignalPortType.IN);

    //        foreach (var sourcePort in sourcePorts)
    //        {
    //            foreach (var targetPort in targetPorts)
    //            {
    //                if (source.CanConnectTo(sourcePort, target, targetPort, out _))
    //                {
    //                    source.MakeConnection(sourcePort, target, targetPort, out _);
    //                    return true;
    //                }
    //            }
    //        }
    //        return false;
    //    }

    //    public static void AutoConnectAllOnMap(Map map)
    //    {
    //        var allSignalNodes = map.listerThings.AllThings
    //            .Select(t => t.TryGetComp<Comp_SignalNode>())
    //            .Where(c => c != null && c.parent.Spawned)
    //            .ToList();

    //        foreach (var node in allSignalNodes)
    //        {
    //            AutoConnectNode(node);
    //        }
    //    }
    //}
}