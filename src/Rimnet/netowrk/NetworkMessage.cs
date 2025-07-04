using System;
using Verse;

namespace RimNet
{
    public class NetworkMessage : IExposable
    {
        public string MessageId = string.Empty;
        public string SenderId = string.Empty;
        public RimNet Network;
        public string TargetNodeID;

        public NetworkMessage()
        {

        }

        public NetworkMessage(string senderId, RimNet networkId)
        {
            MessageId = Guid.NewGuid().ToString();
            SenderId = senderId;
            Network = networkId;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref MessageId, "messageId");
            Scribe_Values.Look(ref SenderId, "senderId");
            Scribe_References.Look(ref Network, "networkId");
            Scribe_Values.Look(ref TargetNodeID, "targetNodeId");
        }
    }
}