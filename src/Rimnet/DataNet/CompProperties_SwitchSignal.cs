using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class CompProperties_SwitchSignal : CompProperties_SignalReciever
    {
        public CompProperties_SwitchSignal()
        {
            compClass = typeof(Comp_SwitchSignal);
        }
    }

    public class Comp_SwitchSignal : Comp_SignalReciever
    {
        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            ConnectionPorts.Add(new SignalPort(SignalPortType.IN, IntVec3.Zero));
        }

        public override void OnSignalRecieved(Signal signal)
        {
            base.OnSignalRecieved(signal);

            if (this.parent.TryGetComp(out CompFlickable flickable))
            {
                flickable.DoFlick();
            }
        }
    }
}
