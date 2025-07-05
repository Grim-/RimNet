using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public class SignalNodeTileGroup : IExposable
    {
        private Comp_SignalNode ownerNode;
        private Thing ownerThing;
        private List<Comp_SignalNode> connectedNodes = new List<Comp_SignalNode>();

        public List<Comp_SignalNode> ConnectedNodes => connectedNodes.ToList();
        public int GroupSize => connectedNodes.Count + 1;

        public SignalNodeTileGroup(Comp_SignalNode owner)
        {
            ownerNode = owner;
            ownerThing = owner?.parent;
        }

        public SignalNodeTileGroup()
        {
        }

        public void ConnectToAdjacentTiles()
        {
            if (ownerNode?.parent?.Spawned != true) return;
            foreach (var cell in GenAdjFast.AdjacentCellsCardinal(ownerNode.parent.Position))
            {
                if (!cell.InBounds(ownerNode.parent.Map)) continue;
                var adjacentNode = GetSignalNodeAt(cell);
                if (adjacentNode != null &&
                    adjacentNode.GetType() == ownerNode.GetType() &&
                    !connectedNodes.Contains(adjacentNode))
                {
                    connectedNodes.Add(adjacentNode);
                    adjacentNode.GetTileGroup()?.AddConnection(ownerNode);
                }
            }
        }

        public void DisconnectFromAdjacentTiles()
        {
            foreach (var node in connectedNodes.ToList())
            {
                node.GetTileGroup()?.RemoveConnection(ownerNode);
            }
            connectedNodes.Clear();
        }

        public void AddConnection(Comp_SignalNode node)
        {
            if (!connectedNodes.Contains(node))
            {
                connectedNodes.Add(node);
            }
        }

        public void RemoveConnection(Comp_SignalNode node)
        {
            connectedNodes.Remove(node);
        }

        public void PropagateSignalToGroup(Signal signal, Action<Comp_SignalNode, Signal> action, HashSet<Comp_SignalNode> visited = null)
        {
            if (visited == null)
                visited = new HashSet<Comp_SignalNode>();
            if (visited.Contains(ownerNode)) return;
            visited.Add(ownerNode);
            action(ownerNode, signal);
            foreach (var node in connectedNodes)
            {
                node.GetTileGroup()?.PropagateSignalToGroup(signal, action, visited);
            }
        }

        private Comp_SignalNode GetSignalNodeAt(IntVec3 position)
        {
            var things = position.GetThingList(ownerNode.parent.Map);
            foreach (var thing in things)
            {
                var node = thing.TryGetComp<Comp_SignalNode>();
                if (node != null) return node;
            }
            return null;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref ownerThing, "ownerThing");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (ownerThing != null)
                {
                    ownerNode = ownerThing.TryGetComp<Comp_SignalNode>();
                }
            }
        }
    }
}