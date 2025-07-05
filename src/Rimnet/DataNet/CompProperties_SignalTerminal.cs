using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class CompProperties_SignalTerminal : CompProperties_SignalNode
    {
        public CompProperties_SignalTerminal()
        {
            compClass = typeof(Comp_SignalTerminal);
        }
    }

    public class Comp_SignalTerminal : Comp_SignalNode
    {
        public override bool IsSignalTerminal()
        {
            return true;
        }


        public override int GetConnectionPriority(Comp_SignalNode otherNode)
        {
            return otherNode is Comp_SignalConduit ? -100 : base.GetConnectionPriority(otherNode);
        }



        public override bool IsSplitterNode()
        {
            return false;
        }

        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            ConnectionPorts.Add(new SignalPort(this, SignalPortType.IN, IntVec3.Zero));
        }
    }
}
