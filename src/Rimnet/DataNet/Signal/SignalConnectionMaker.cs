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

        private static List<Comp_SignalNode> GetNodesAt(IntVec3 cell, Map map)
        {
            var nodes = new List<Comp_SignalNode>();
            if (!cell.InBounds(map)) 
                return nodes;

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
}