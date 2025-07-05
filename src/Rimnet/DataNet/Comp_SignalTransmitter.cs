using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public abstract class Comp_SignalTransmitter : Comp_SignalNode
    {
        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>
            {
                new SignalPort(this, SignalPortType.IN, IntVec3.Zero),
                new SignalPort(this, SignalPortType.OUT, IntVec3.Zero),
                new SignalPort(this, SignalPortType.OUT, IntVec3.Zero),
                new SignalPort(this, SignalPortType.OUT, IntVec3.Zero)
            };
        }

        public override bool IsSplitterNode()
        {
            return true;
        }

        public override bool IsSignalTerminal()
        {
            return ConnectedChildren.Count == 0;
        }

        protected string GetDirectionLabel(IntVec3 direction)
        {
            if (direction == IntVec3.North) return "North";
            if (direction == IntVec3.South) return "South";
            if (direction == IntVec3.East) return "East";
            if (direction == IntVec3.West) return "West";
            return direction.ToString();
        }
    }


}
