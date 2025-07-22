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


        public HashSet<Comp_SignalNode> AllNodes
        {
            get
            {
                HashSet<Comp_SignalNode> nodes = new HashSet<Comp_SignalNode>();
                nodes.Add(ownerNode);
                nodes.AddRange(connectedNodes);
                return nodes;
            }
        }

        public Comp_SignalNode OwnerNode => ownerNode;

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
            DiscoverGroup(owner);
        }

        public void SelectBestOwner()
        {
            //a node that is connected to something other than other nodes of the same type
            Comp_SignalNode newBestNode = AllNodes.Where(x => x.AllConnectedPorts.Any(y => y.ConnectedNode.GetType() != ownerNode.GetType())).FirstOrDefault();
            if (newBestNode != null)
            {
                PromoteToOwner(newBestNode);
            }
        }


        public void PromoteToOwner(Comp_SignalNode newOwner)
        {
            if (connectedNodes.Contains(newOwner))
            {
                ownerNode = newOwner;
                ownerThing = newOwner.parent;
            }
        }

        public void OnGroupMemberSelected(Comp_SignalNode member)
        {
            foreach (var item in ConnectedNodes)
            {
                GenDraw.DrawCircleOutline(item.parent.DrawPos, 1f, SimpleColor.Yellow);
            }   
        }

        public bool IsGroupOwner(Comp_SignalNode signalNode)
        {
            return ownerNode == signalNode;
        }

        private void DiscoverGroup(Comp_SignalNode rootNode)
        {
            this.connectedNodes.Clear();

            Queue<Comp_SignalNode> queue = new Queue<Comp_SignalNode>();

            HashSet<Comp_SignalNode> visited = new HashSet<Comp_SignalNode>();

            queue.Enqueue(rootNode);
            visited.Add(rootNode);
            while (queue.Count > 0)
            {
                Comp_SignalNode currentNode = queue.Dequeue();

                if (currentNode != this.ownerNode)
                {
                    this.connectedNodes.Add(currentNode);
                }

                foreach (var neighborInfo in currentNode.GetCardinalNodes())
                {
                    Comp_SignalNode neighborNode = neighborInfo.FoundNode;
                    if (neighborNode != null && !visited.Contains(neighborNode) && neighborNode.GetType() == this.ownerNode.GetType())
                    {
                        visited.Add(neighborNode);
                        queue.Enqueue(neighborNode);
                    }
                }
            }
        }

        public void DisconnectFromAdjacentTiles()
        {
            foreach (var node in connectedNodes.ToList())
            {
                node.SignalGroup?.LeaveGroup(ownerNode);
            }
            connectedNodes.Clear();
        }

        public void JoinGroup(Comp_SignalNode node)
        {
            if (connectedNodes.Contains(node) || ownerNode == node)
            {
                return;
            }

            connectedNodes.Add(node);
            foreach (var neighborData in node.GetCardinalNodes())
            {
                var neighborNode = neighborData.FoundNode;
                if (neighborNode?.SignalGroup != null && neighborNode.SignalGroup != this)
                {
                    MergeGroup(neighborNode.SignalGroup);
                }
            }

            OnGroupChange();
        }
        public void MergeGroup(SignalGroup otherGroup)
        {
            var nodesToMerge = new List<Comp_SignalNode>(otherGroup.ConnectedNodes);
            nodesToMerge.Add(otherGroup.ownerNode);
            foreach (var node in nodesToMerge)
            {
                node.JoinSignalGroup(this);
            }

            OnGroupChange();
        }

        public void LeaveGroup(Comp_SignalNode node)
        {
            connectedNodes.Remove(node);
            OnGroupChange();
        }

        protected void OnGroupChange()
        {
            SelectBestOwner();
        }

        public void SyncGroup(Comp_SignalNode senderNode)
        {
            foreach (var item in AllNodes)
            {
                if (item != senderNode)
                {
                    item.SyncWithGroupNode(senderNode);
                }    
            }
        }

        public bool IsPartOfGroup(Comp_SignalNode signalNode)
        {
            return signalNode == ownerNode || connectedNodes.Contains(signalNode);
        }

        public void SendSignalToGroup(Signal signal, Comp_SignalNode sender, Action<Comp_SignalNode, Signal> action)
        {
            foreach (var node in AllNodes)
            {
                if (node == sender)
                {
                    continue;
                }

                action(node, signal);
            }
        }
        public void PropagateSignal(Signal signal, Comp_SignalNode originator)
        {
            foreach (var node in AllNodes)
            {
                if (node != originator)
                {
                    node.OnGroupSignalReceived(signal, this);
                }
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