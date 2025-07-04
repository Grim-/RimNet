using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public static class SignalConnectionMaker
    {
        public static void AutoConnectNode(Comp_SignalNode LastPlacedNode, List<Comp_SignalNode> excludeNodes = null)
        {
            if (LastPlacedNode == null || !LastPlacedNode.parent.Spawned)
                return;

            foreach (var cell in GenAdjFast.AdjacentCellsCardinal(LastPlacedNode.parent))
            {
                if (!cell.InBounds(LastPlacedNode.parent.Map))
                    continue;




                foreach (var thing in cell.GetThingList(LastPlacedNode.parent.Map))
                {
                    if (thing is ThingWithComps twc)
                    {
                        var otherNode = twc.TryGetComp<Comp_SignalNode>();
                        if (otherNode == null || otherNode == LastPlacedNode)
                            continue;


                        //if (excludeNodes != null && excludeNodes.Count > 0 && excludeNodes.Contains(otherNode))
                        //{
                        //    Log.Message($"skipping connecting {LastPlacedNode.parent.Label} to {otherNode.parent.Label} because it is excluded");
                        //    continue;
                        //}


                        //IntVec3 directionToExisting = (LastPlacedNode.parent.Position - cell);
                        //IntVec3 directionToPotentialCell = (cell - LastPlacedNode.parent.Position);
                        //bool potentialCellIsNorth = directionToPotentialCell.z > 0;

                        //if (potentialCellIsNorth)
                        //{
                        //    Log.Message($"skipping connecting {LastPlacedNode.parent.Label} to {otherNode.parent.Label} because source is NORTH of this, making this SOUTH of that");
                        //    continue;
                        //}

                        var theirUnconnectedInPorts = otherNode.GetUnconnectedPorts(SignalPortType.IN);
                        var theirInPorts = otherNode.GetPorts(SignalPortType.IN);

                        var inPorts = LastPlacedNode.GetUnconnectedPorts(SignalPortType.IN);
                        var outPorts = otherNode.GetUnconnectedPorts(SignalPortType.OUT);
                        if (inPorts.Any() && outPorts.Any() && !LastPlacedNode.IsNodeDescendantOf(otherNode))
                        {
                            TryMakeValidConnection(otherNode, LastPlacedNode);
                            continue;
                        }

                        var myOutPorts = LastPlacedNode.GetUnconnectedPorts(SignalPortType.OUT);
                        if (myOutPorts.Any() && theirUnconnectedInPorts.Any() && !otherNode.IsNodeDescendantOf(LastPlacedNode))
                        {
                            TryMakeValidConnection(LastPlacedNode, otherNode);
                        }
                    }
                }
            }
        }

        public static void RecursivelyConnectForward(Comp_SignalNode startNode)
        {
            if (startNode == null || !startNode.parent.Spawned)
                return;

            var visited = new HashSet<Comp_SignalNode>();
            RecursivelyConnectForwardInternal(startNode, visited);
        }

        private static void RecursivelyConnectForwardInternal(Comp_SignalNode currentNode, HashSet<Comp_SignalNode> visited)
        {
            if (currentNode == null)
                return;

            visited.Add(currentNode);

            var nodesToConnect = new List<Comp_SignalNode>();

            foreach (var cell in GenAdjFast.AdjacentCellsCardinal(currentNode.parent))
            {
                if (!cell.InBounds(currentNode.parent.Map))
                    continue;

                foreach (var thing in cell.GetThingList(currentNode.parent.Map).ToArray())
                {
                    if (thing is ThingWithComps twc)
                    {
                        var otherNode = twc.TryGetComp<Comp_SignalNode>();
                        if (otherNode == null || otherNode == currentNode)
                            continue;

                        var theirUnconnectedInPorts = otherNode.GetUnconnectedPorts(SignalPortType.IN);
                        var theirInPorts = otherNode.GetPorts(SignalPortType.IN);

                        var inPorts = currentNode.GetUnconnectedPorts(SignalPortType.IN);
                        var outPorts = otherNode.GetUnconnectedPorts(SignalPortType.OUT);
                        if (inPorts.Any() && outPorts.Any() && !currentNode.IsNodeDescendantOf(otherNode))
                        {
                            nodesToConnect.Add(otherNode);
                            //TryMakeValidConnection(otherNode, currentNode);
                            continue;
                        }

                        var myOutPorts = currentNode.GetUnconnectedPorts(SignalPortType.OUT);
                        if (myOutPorts.Any() && theirUnconnectedInPorts.Any() && !otherNode.IsNodeDescendantOf(currentNode))
                        {
                           // TryMakeValidConnection(currentNode, otherNode); 
                            nodesToConnect.Add(otherNode);
                        }

                        //var myOutPorts = currentNode.GetUnconnectedPorts(SignalPortType.OUT);
                        //var theirInPorts = otherNode.GetUnconnectedPorts(SignalPortType.IN);

                        //if (myOutPorts.Any() && theirInPorts.Any() && !otherNode.IsNodeDescendantOf(currentNode))
                        //{
                        //    nodesToConnect.Add(otherNode);
                        //}
                    }
                }
            }

            foreach (var nodeToConnect in nodesToConnect)
            {
                if (TryMakeValidConnection(nodeToConnect, currentNode))
                {
                    RecursivelyConnectForwardInternal(nodeToConnect, visited);
                }
            }
        }
        private static bool TryMakeValidConnection(Comp_SignalNode a, Comp_SignalNode b)
        {
            foreach (var aPort in a.ConnectionPorts.ToArray())
            {
                if (aPort.Type != SignalPortType.OUT || aPort.ConnectedNode != null)
                    continue;
                foreach (var bPort in b.ConnectionPorts.ToArray())
                {
                    if (bPort.Type != SignalPortType.IN || bPort.ConnectedNode != null)
                        continue;
                    if (a.VerifyConnection(aPort, b, bPort, out _))
                    {
                        a.MakeConnection(aPort, b, bPort, out _);
                        return true;
                    }
                }
            }
            return false;
        }

        private static void TraceConnectionPath(Comp_SignalNode node, List<Comp_SignalNode> path, HashSet<Comp_SignalNode> processed)
        {
            if (node == null || processed.Contains(node))
                return;

            processed.Add(node);
            path.Add(node);

            foreach (var port in node.ConnectionPorts.ToArray())
            {
                if (port.ConnectedNode != null)
                {
                    TraceConnectionPath(port.ConnectedNode, path, processed);
                }
            }
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