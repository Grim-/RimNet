using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    //public static class SignalConnectionMaker
    //{
    //    public static void AutoConnectNode(Comp_SignalNode lastPlacedNode, List<Comp_SignalNode> excludeNodes = null)
    //    {
    //        if (lastPlacedNode == null || !lastPlacedNode.parent.Spawned)
    //            return;

    //        if (lastPlacedNode.IsPassiveNode)
    //            return;

    //        Comp_SignalNode bestSource = null;
    //        Comp_SignalNode bestTarget = null;
    //        int bestPriority = int.MinValue;

    //        List<IntVec3> cells = GenAdjFast.AdjacentCellsCardinal(lastPlacedNode.parent);
    //        cells.Add(lastPlacedNode.parent.Position);
    //        foreach (var cell in cells)
    //        {
    //            var nodesAtCell = GetNodesAt(cell, lastPlacedNode.parent.Map);
    //            foreach (var otherNode in nodesAtCell)
    //            {
    //                if (otherNode == null || otherNode == lastPlacedNode ||
    //                    IsExcluded(otherNode, excludeNodes) || otherNode.IsPassiveNode)
    //                    continue;

    //                bool otherHasIncomingSignal = HasIncomingSignalFlow(otherNode);
    //                bool lastPlacedHasIncomingSignal = HasIncomingSignalFlow(lastPlacedNode);

    //                Comp_SignalNode source;
    //                Comp_SignalNode target;

    //                if (otherHasIncomingSignal && !lastPlacedHasIncomingSignal)
    //                {
    //                    source = otherNode;
    //                    target = lastPlacedNode;
    //                }
    //                else if (!otherHasIncomingSignal && lastPlacedHasIncomingSignal)
    //                {
    //                    source = lastPlacedNode;
    //                    target = otherNode;
    //                }
    //                else
    //                {
    //                    Comp_SignalNode sourceNode = FindNetworkSource(otherNode);
    //                    if (sourceNode != null)
    //                    {
    //                        if (IsCloserToSource(otherNode, lastPlacedNode, sourceNode))
    //                        {
    //                            source = otherNode;
    //                            target = lastPlacedNode;
    //                        }
    //                        else
    //                        {
    //                            source = lastPlacedNode;
    //                            target = otherNode;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        source = otherNode;
    //                        target = lastPlacedNode;
    //                    }
    //                }

    //                if (!CanConnect(source, target))
    //                    continue;

    //                int priority = target.GetConnectionPriority(source);

    //                if (priority > bestPriority)
    //                {
    //                    bestPriority = priority;
    //                    bestSource = source;
    //                    bestTarget = target;
    //                }
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

    public static class SignalConnectionMaker
    {
        public static void AutoConnectNode(Comp_SignalNode lastPlacedNode, List<Comp_SignalNode> excludeNodes = null)
        {
            if (lastPlacedNode == null || !lastPlacedNode.parent.Spawned)
                return;

            Comp_SignalNode bestSource = null;
            Comp_SignalNode bestTarget = null;
            int bestPriority = int.MinValue;

            List<IntVec3> cells = GenAdjFast.AdjacentCellsCardinal(lastPlacedNode.parent);
            cells.Add(lastPlacedNode.parent.Position);
            foreach (var cell in cells)
            {
                var otherNode = GetNodeAt(cell, lastPlacedNode.parent.Map);
                if (otherNode == null || otherNode == lastPlacedNode || IsExcluded(otherNode, excludeNodes))
                    continue;

                // Determine signal direction (who's source, who's target)
                bool otherHasIncomingSignal = HasIncomingSignalFlow(otherNode);
                bool lastPlacedHasIncomingSignal = HasIncomingSignalFlow(lastPlacedNode);

                Comp_SignalNode source;
                Comp_SignalNode target;

                if (otherHasIncomingSignal && !lastPlacedHasIncomingSignal)
                {
                    source = otherNode;
                    target = lastPlacedNode;
                }
                else if (!otherHasIncomingSignal && lastPlacedHasIncomingSignal)
                {
                    source = lastPlacedNode;
                    target = otherNode;
                }
                else
                {
                    Comp_SignalNode sourceNode = FindNetworkSource(otherNode);
                    if (sourceNode != null)
                    {
                        if (IsCloserToSource(otherNode, lastPlacedNode, sourceNode))
                        {
                            source = otherNode;
                            target = lastPlacedNode;
                        }
                        else
                        {
                            source = lastPlacedNode;
                            target = otherNode;
                        }
                    }
                    else
                    {
                        source = otherNode;
                        target = lastPlacedNode;
                    }
                }

                if (!CanConnect(source, target))
                    continue;

                // Evaluate priority
                int priority = target.GetConnectionPriority(source);

                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    bestSource = source;
                    bestTarget = target;
                }
            }

            if (bestSource != null && bestTarget != null)
            {
                TryConnect(bestSource, bestTarget);
            }
        }

        private static bool HasIncomingSignalFlow(Comp_SignalNode node)
        {
            return node.ConnectedParents.Count > 0;
        }

        private static Comp_SignalNode FindNetworkSource(Comp_SignalNode node)
        {
            var visited = new HashSet<Comp_SignalNode>();
            var current = node;

            while (current != null && !visited.Contains(current))
            {
                visited.Add(current);
                if (current.ConnectedParents.Count == 0)
                    return current;
                current = current.ConnectedParents.FirstOrDefault();
            }

            return null;
        }

        private static bool IsCloserToSource(Comp_SignalNode node1, Comp_SignalNode node2, Comp_SignalNode source)
        {
            return GetDistanceToSource(node1, source) < GetDistanceToSource(node2, source);
        }

        private static int GetDistanceToSource(Comp_SignalNode node, Comp_SignalNode source)
        {
            var visited = new HashSet<Comp_SignalNode>();
            var queue = new Queue<(Comp_SignalNode node, int distance)>();
            queue.Enqueue((node, 0));

            while (queue.Count > 0)
            {
                var (current, distance) = queue.Dequeue();
                if (current == source)
                    return distance;

                if (visited.Contains(current))
                    continue;

                visited.Add(current);

                foreach (var parent in current.ConnectedParents)
                    queue.Enqueue((parent, distance + 1));
            }

            return int.MaxValue;
        }

        public static void RecursivelyConnectForward(Comp_SignalNode startNode)
        {
            if (startNode == null || !startNode.parent.Spawned)
                return;

            var visited = new HashSet<Comp_SignalNode>();
            RecursiveConnectInternal(startNode, visited);
        }

        private static void RecursiveConnectInternal(Comp_SignalNode currentNode, HashSet<Comp_SignalNode> visited)
        {
            if (currentNode == null || visited.Contains(currentNode))
                return;

            visited.Add(currentNode);

            var connectableNodes = GetConnectableNeighbors(currentNode, visited);

            foreach (var neighbor in connectableNodes)
            {
                if (TryConnect(currentNode, neighbor) || TryConnect(neighbor, currentNode))
                {
                    RecursiveConnectInternal(neighbor, visited);
                }
            }
        }

        private static List<Comp_SignalNode> GetConnectableNeighbors(Comp_SignalNode node, HashSet<Comp_SignalNode> visited)
        {
            var neighbors = new List<Comp_SignalNode>();

            foreach (var cell in GenAdjFast.AdjacentCellsCardinal(node.parent))
            {
                var neighbor = GetNodeAt(cell, node.parent.Map);
                if (neighbor != null && neighbor != node && !visited.Contains(neighbor))
                {
                    if (CanConnect(node, neighbor) || CanConnect(neighbor, node))
                        neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        private static Comp_SignalNode GetNodeAt(IntVec3 cell, Map map)
        {
            if (!cell.InBounds(map))
                return null;

            foreach (var thing in cell.GetThingList(map))
            {
                if (thing is ThingWithComps twc)
                {
                    var node = twc.TryGetComp<Comp_SignalNode>();
                    if (node != null)
                        return node;
                }
            }
            return null;
        }

        private static bool IsExcluded(Comp_SignalNode node, List<Comp_SignalNode> excludeNodes)
        {
            return excludeNodes != null && excludeNodes.Contains(node);
        }

        private static bool CanConnect(Comp_SignalNode source, Comp_SignalNode target)
        {
            return source.HasConnectionPort(SignalPortType.OUT) &&
                    target.HasConnectionPort(SignalPortType.IN) &&
                   !target.IsNodeDescendantOf(source);
        }

        private static bool TryConnect(Comp_SignalNode source, Comp_SignalNode target)
        {
            if (!CanConnect(source, target))
                return false;

            return TryMakeValidConnection(source, target);
        }


        private static bool TryMakeValidConnection(Comp_SignalNode source, Comp_SignalNode target)
        {
            var sourcePorts = source.GetUnconnectedPorts(SignalPortType.OUT);
            var targetPorts = target.GetUnconnectedPorts(SignalPortType.IN);

            foreach (var sourcePort in sourcePorts)
            {
                foreach (var targetPort in targetPorts)
                {
                    if (source.CanConnectTo(sourcePort, target, targetPort, out _))
                    {
                        source.MakeConnection(sourcePort, target, targetPort, out _);
                        return true;
                    }
                }
            }
            return false;
        }

        public static void AutoConnectAllOnMap(Map map)
        {
            var allSignalNodes = map.listerThings.AllThings
                .Select(t => t.TryGetComp<Comp_SignalNode>())
                .Where(c => c != null && c.parent.Spawned)
                .ToList();

            foreach (var node in allSignalNodes)
            {
                AutoConnectNode(node);
            }
        }
    }
}