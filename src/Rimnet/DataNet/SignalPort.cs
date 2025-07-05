using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class SignalPort : IExposable
    {
        public SignalPortType Type;
        public IntVec3 LocalOffset = IntVec3.Zero;
        public bool Enabled = true;


        public Comp_SignalNode OwnerNode { get; private set; }

        public Comp_SignalNode ConnectedNode { get; private set; }


        public bool HasConnectTarget => ConnectedNode != null;

        protected ThingWithComps ConnectedThing;
        private List<Comp_SignalNode> passiveListeners = new List<Comp_SignalNode>();

        public SignalPort()
        {

        }

        public SignalPort(Comp_SignalNode owner, SignalPortType type, IntVec3 localOffset, Comp_SignalNode connectedNode = null)
        {
            OwnerNode = owner;
            Type = type;
            LocalOffset = localOffset;
            if (connectedNode != null)
            {
                Connect(connectedNode);
            }
        }


        public void Connect(Comp_SignalNode connectedNode)
        {
            if (ConnectedNode != null)
            {
                //do disconnect
            }

            ConnectedNode = connectedNode;
            ConnectedThing = connectedNode.parent;
        }

        public void Disconnect()
        {
            if (ConnectedNode == null)
            {
                return;
            }
            ConnectedNode = null;
            ConnectedThing = null;
        }

        public void Enable()
        {
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
        }

        public void Toggle()
        {
            Enabled = !Enabled;
        }
  
        public void AddPassiveListener(Comp_SignalNode listener)
        {
            if (!passiveListeners.Contains(listener))
                passiveListeners.Add(listener);
        }

        public void RemovePassiveListener(Comp_SignalNode listener)
        {
            passiveListeners.Remove(listener);
        }

        public List<Comp_SignalNode> GetAllConnections()
        {
            var connections = new List<Comp_SignalNode>();
            if (ConnectedNode != null)
                connections.Add(ConnectedNode);
            connections.AddRange(passiveListeners);
            return connections;
        }
        public void ExposeData()
        {
            Scribe_References.Look(ref ConnectedThing, "connectedThing");
            Scribe_Values.Look(ref Type, "type");
            Scribe_Values.Look(ref LocalOffset, "LocalOffset");
            Scribe_Values.Look(ref Enabled, "Enabled");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (ConnectedThing != null && ConnectedNode == null)
                {
                    ConnectedNode = ConnectedThing.GetComp<Comp_SignalNode>();
                }
            }
        }
    }
}
