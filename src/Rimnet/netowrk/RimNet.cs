using System;
using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class RimNet : IExposable, ILoadReferenceable
    {
        public string ID;
        public List<Comp_NetworkNode> NetworkNodes = new List<Comp_NetworkNode>();
        public Action<NetworkMessage> OnMessageSent;

        protected Queue<NetworkMessage> MessageQueue = new Queue<NetworkMessage>();
        private List<Thing> networkThings = new List<Thing>();

        public event EventHandler<NetworkMessageEventArgs> NetworkMessageSent;
        public event EventHandler<NetworkMessageEventArgs> NetworkMessageReceived;
        public event EventHandler<NetworkStatusEventArgs> NodeStatusChanged;
        public RimNet() { }
        public RimNet(string id)
        {
            ID = id;
        }
        public void RegisterNode(Comp_NetworkNode node)
        {
            if (!NetworkNodes.Contains(node))
            {
                NetworkNodes.Add(node);
                networkThings.Add(node.parent);
                node.JoinNetwork(this);
                node.MessageSent += OnNodeMessageSent;
                node.MessageReceived += OnNodeMessageReceived;
                node.StatusChanged += OnNodeStatusChanged;
            }
        }
        public void UnregisterNode(Comp_NetworkNode node)
        {
            if (NetworkNodes.Contains(node))
            {
                node.MessageSent -= OnNodeMessageSent;
                node.MessageReceived -= OnNodeMessageReceived;
                node.StatusChanged -= OnNodeStatusChanged;
                NetworkNodes.Remove(node);
                networkThings.Remove(node.parent);
                node.LeaveNetwork();
            }
        }

        public void BroadcastMessage(Comp_NetworkNode sender, NetworkMessage message)
        {
            if (sender?.parent?.Map == null)
                return;

            message.TargetNodeID = null;
            MessageQueue.Enqueue(message);
            OnMessageSent?.Invoke(message);
        }

        public void SendMessage(Comp_NetworkNode sender, string targetNodeID, NetworkMessage message)
        {
            if (sender?.parent?.Map == null)
                return;

            message.TargetNodeID = targetNodeID;
            MessageQueue.Enqueue(message);
            OnMessageSent?.Invoke(message);
        }
        private void OnNodeMessageSent(object sender, NetworkMessageEventArgs e)
        {
            NetworkMessageSent?.Invoke(sender, e);
        }
        private void OnNodeMessageReceived(object sender, NetworkMessageEventArgs e)
        {
            NetworkMessageReceived?.Invoke(sender, e);
        }
        private void OnNodeStatusChanged(object sender, NetworkStatusEventArgs e)
        {
            NodeStatusChanged?.Invoke(sender, e);
        }
        public void ProcessMessages()
        {
            if (MessageQueue.Count == 0)
                return;

            NetworkMessage message = MessageQueue.Dequeue();

            if (message == null)
                return;

            if (string.IsNullOrEmpty(message.TargetNodeID))
            {
                foreach (var node in NetworkNodes)
                {
                    if (node.NodeID != message.SenderId)
                    {
                        DeliverMessage(node, message);
                    }
                }
            }
            else
            {
                Comp_NetworkNode targetNode = GetConnectedNode(message.TargetNodeID);
                if (targetNode != null)
                {
                    DeliverMessage(targetNode, message);
                }
            }
        }

        private void DeliverMessage(Comp_NetworkNode targetNode, NetworkMessage message)
        {
            if (!targetNode.CanReceive() || message.SenderId == targetNode.NodeID || message.Network.ID != ID)
                return;

            try
            {
                targetNode.OnMessageReceived(message);
            }
            catch (Exception)
            {
            }
        }

        public bool IsConnectedNode(string nodeID)
        {
            return NetworkNodes.Any(x => x.NodeID == nodeID);
        }

        public Comp_NetworkNode GetConnectedNode(string nodeID)
        {
            if (NetworkNodes.Any(x => x.NodeID == nodeID))
            {
                return NetworkNodes.FirstOrDefault(x => x.NodeID == nodeID);
            }
            return null;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref ID, "id");
            Scribe_Collections.Look(ref networkThings, "networkThings", LookMode.Reference);
            Scribe_Collections.Look(ref MessageQueue, "messageQueue", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (networkThings == null)
                    networkThings = new List<Thing>();
                if (MessageQueue == null)
                    MessageQueue = new Queue<NetworkMessage>();

                NetworkNodes = new List<Comp_NetworkNode>();
                foreach (var thing in networkThings)
                {
                    if (thing != null)
                    {
                        var comp = thing.TryGetComp<Comp_NetworkNode>();
                        if (comp != null)
                        {
                            NetworkNodes.Add(comp);
                        }
                    }
                }
            }
        }

        public string GetUniqueLoadID()
        {
            if (string.IsNullOrEmpty(ID))
            {
                ID = "RimNetwork_" + Find.UniqueIDsManager.GetNextThingID();
            }
            return ID;
        }

    }
}