using Verse;

namespace RimNet
{
    public class SignalPort : IExposable
    {
        public SignalPortType Type;
        public IntVec3 LocalOffset;
        public Comp_SignalNode ConnectedNode { get; private set; }


        public bool HasConnectTarget => ConnectedNode != null;

        protected ThingWithComps ConnectedThing;

        public SignalPort()
        {

        }

        public SignalPort(SignalPortType type, IntVec3 localOffset, Comp_SignalNode connectedNode = null)
        {
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

        public void ExposeData()
        {

            Scribe_References.Look(ref ConnectedThing, "connectedThing");
            Scribe_Values.Look(ref Type, "type");
            Scribe_Values.Look(ref LocalOffset, "LocalOffset");

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
