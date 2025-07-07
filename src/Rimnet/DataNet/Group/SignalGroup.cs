using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public class SignalGroup : IExposable
    {
        private Comp_SignalNode ownerNode;
        private Thing ownerThing;
        private List<Comp_SignalNode> connectedNodes = new List<Comp_SignalNode>();

        public List<Comp_SignalNode> ConnectedNodes => connectedNodes.ToList();
        public int GroupSize => connectedNodes.Count + 1;


        protected string groupID;
        public string GroupLabel => groupID;

        public SignalGroup()
        {

        }

        public SignalGroup(Comp_SignalNode owner, string groupName ="Group")
        {
            ownerNode = owner;
            ownerThing = owner?.parent;
            groupID = groupName;
        }

        public void ConnectToAdjacentTiles()
        {
            if (ownerNode?.parent?.Spawned != true) 
                return;

            foreach (var cell in GenAdjFast.AdjacentCellsCardinal(ownerNode.parent.Position))
            {
                if (!cell.InBounds(ownerNode.parent.Map)) 
                    continue;

                Comp_SignalNode adjacentNode = GetSignalNodeAt(cell);

                if (adjacentNode != null && CanJoinGroup(adjacentNode))
                {
                    JoinGroup(adjacentNode);
                    connectedNodes.Add(adjacentNode);
                    adjacentNode.GetTileGroup()?.JoinGroup(ownerNode);
                }
            }
        }
        //private void CheckAndMergeAdjacentGroups()
        //{
        //    foreach (var cell in GenAdjFast.AdjacentCellsCardinal(this.parent.Position))
        //    {
        //        if (!cell.InBounds(this.parent.Map))
        //            continue;

        //        if (TryGetExistingAdjacentGroup(cell, this.parent.Map, out SignalGroup adjacentGroup) &&
        //            adjacentGroup != this.tileGroup)
        //        {
        //            var nodesToMigrate = new List<Comp_SignalNode>(adjacentGroup.ConnectedNodes);

        //            foreach (var node in nodesToMigrate)
        //            {
        //                node.tileGroup = this.tileGroup;
        //                this.tileGroup.JoinGroup(node);
        //            }
        //        }
        //    }
        //}

        //private bool TryGetExistingAdjacentGroup(IntVec3 cell, Map map, out SignalGroup tileGroup)
        //{
        //    tileGroup = null;
        //    Comp_SignalNode adjacentNode = GetNodeAt(cell);
        //    if (adjacentNode != null &&
        //        adjacentNode.GetType() == this.GetType() &&
        //        adjacentNode.CanFormTileGroup &&
        //        adjacentNode.tileGroup != null)
        //    {
        //        tileGroup = adjacentNode.tileGroup;
        //        return true;
        //    }
        //    return false;
        //}

        public void DisconnectFromAdjacentTiles()
        {
            foreach (var node in connectedNodes.ToList())
            {
                node.GetTileGroup()?.LeaveGroup(ownerNode);
            }
            connectedNodes.Clear();
        }

        public void JoinGroup(Comp_SignalNode node)
        {
            if (!connectedNodes.Contains(node))
            {
                connectedNodes.Add(node);
            }
        }

        public void LeaveGroup(Comp_SignalNode node)
        {
            connectedNodes.Remove(node);
        }

        public void SendSignalToGroup(Signal signal, Action<Comp_SignalNode, Signal> action, HashSet<Comp_SignalNode> visited = null)
        {
            if (visited == null)
                visited = new HashSet<Comp_SignalNode>();

            if (visited.Contains(ownerNode)) 
                return;

            visited.Add(ownerNode);
            action(ownerNode, signal);
            foreach (var node in connectedNodes)
            {
                node.GetTileGroup()?.SendSignalToGroup(signal, action, visited);
            }
        }


        public bool CanJoinGroup(Comp_SignalNode signalNode)
        {
            if (!IsPartOfGroup(signalNode) && signalNode.GetType() == ownerNode.GetType())
            {
                return true;
            }
            return false;
        }

        public bool IsPartOfGroup(Comp_SignalNode signalNode)
        {
            return connectedNodes.Contains(signalNode);
        }

        private Comp_SignalNode GetSignalNodeAt(IntVec3 position)
        {
            var things = position.GetThingList(ownerNode.parent.Map);
            foreach (var thing in things)
            {
                var node = thing.TryGetComp<Comp_SignalNode>();
                if (node != null)
                    return node;
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