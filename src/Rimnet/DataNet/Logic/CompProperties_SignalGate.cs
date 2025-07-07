using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public class CompProperties_SignalGate : CompProperties_SignalNode
    {
        public CompProperties_SignalGate()
        {
            compClass = typeof(Comp_SignalGate);
        }
    }

    public class Comp_SignalGate : Comp_SignalNode
    {
        public override void OnSignalRecieved(Signal signal, SignalPort receivingPort)
        {
            base.OnSignalRecieved(signal, receivingPort);
        }
    }

 
}
