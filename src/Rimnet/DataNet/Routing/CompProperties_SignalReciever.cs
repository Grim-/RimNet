using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class CompProperties_SignalReciever : CompProperties_SignalNode
    {
        public CompProperties_SignalReciever()
        {
            compClass = typeof(Comp_SignalReciever);
        }
    }

    public class Comp_SignalReciever : Comp_SignalNode
    {
        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            ConnectionPorts.Add(new SignalPort(this, SignalPortType.IN, IntVec3.Zero));
        }
    }


}
