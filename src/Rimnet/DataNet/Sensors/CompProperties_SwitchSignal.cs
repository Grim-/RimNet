using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
            ConnectionPorts.Add(new SignalPort(this, SignalPortType.IN, IntVec3.Zero));
            ConnectionPorts.Add(new SignalPort(this, SignalPortType.OUT, IntVec3.Zero));
        }

        public override void OnSignalRecieved(Signal signal)
        {
            base.OnSignalRecieved(signal);

            if (TryGetConnectionPort(SignalPortType.OUT, out SignalPort foundPort))
            {
                if (foundPort.HasConnectTarget)
                {
                    if (foundPort.ConnectedNode.parent.TryGetComp(out CompFlickable flickable))
                    {
                        flickable.DoFlick();
                    }
                }
            }
        }
    }



}
