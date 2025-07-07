using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class SignalPort : IExposable
    {
        public SignalPortType Type;
        public IntVec3 LocalOffset = IntVec3.Zero;
        public string PortName;
        public int PortIndex;
        public bool Enabled = true;

        public bool ShowPort = true;
        public IntVec3 WorldPosition => OwnerNode.parent.Position + LocalOffset;

        public Comp_SignalNode OwnerNode { get; set; }

        protected ThingWithComps ConnectedNodeThing;
        public Comp_SignalNode ConnectedNode => ConnectedPort?.OwnerNode;
        public bool HasConnectTarget => ConnectedPort != null;
        public SignalPort ConnectedPort { get; private set; }

        public bool AutoConnectable = true;
        public SignalPort()
        {

        }

        public SignalPort(Comp_SignalNode parentNode, SignalPortType type, IntVec3 localOffset, SignalPort connectedPort = null, string portName = "", int portIndex = 0, bool showPort = true, bool autoConnectable = true)
        {
            OwnerNode = parentNode;
            Type = type;
            LocalOffset = localOffset;
            PortIndex = portIndex;
            PortName = portName == string.Empty ? $"{type}{portIndex}" : portName;
            ShowPort = showPort;
            AutoConnectable = autoConnectable;
            if (connectedPort != null)
            {
                Connect(connectedPort);
            }
        }

        public void Connect(SignalPort otherPort)
        {
            // Prevent invalid connections
            //if (this.Type == otherPort.Type)
            //{
            //    Log.Error("Cannot connect two ports of the same type.");
            //    return;
            //}

            this.ConnectedPort = otherPort;
            otherPort.ConnectedPort = this; 
        }

        public void Disconnect()
        {
            if (this.ConnectedPort != null)
            {
                var other = this.ConnectedPort;
                this.ConnectedPort = null;
                other.ConnectedPort = null;
            }
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
  
        public void ExposeData()
        {
            Scribe_References.Look(ref ConnectedNodeThing, "connectedThing");
            Scribe_Values.Look(ref Type, "type");
            Scribe_Values.Look(ref LocalOffset, "LocalOffset");
            Scribe_Values.Look(ref Enabled, "Enabled");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (ConnectedNodeThing != null && ConnectedNode == null)
                {
                    //ConnectedNode = ConnectedNodeThing.GetComp<Comp_SignalNode>();
                }
            }
        }
    }
}
