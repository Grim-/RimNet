using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    /// <summary>
    /// flicks connected device on signal recieve
    /// </summary>
    public class CompProperties_SignalFlicker: CompProperties_SignalReciever
    {
        public CompProperties_SignalFlicker()
        {
            compClass = typeof(Comp_SignalFlicker);
        }
    }

    public class Comp_SignalFlicker : Comp_SignalReciever
    {
        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            CreatePort(SignalPortType.IN, IntVec3.Zero, "IN", 0);
            CreatePort(SignalPortType.OUT, IntVec3.Zero, "OUT", 0);
        }


        public override bool IsSignalTerminal()
        {
            return true;
        }

        protected override bool IsValidSelectionTarget(SignalPort sourcePort, SignalPort targetPort, out string notValidSelectionReason)
        {
            notValidSelectionReason = string.Empty;

            if (!targetPort.OwnerNode.parent.HasComp<CompFlickable>())
            {
                notValidSelectionReason = "Must target flickable";
                return false;
            }
        
            return base.IsValidSelectionTarget(sourcePort, targetPort, out notValidSelectionReason);
        }

        public override void OnSignalRecieved(Signal signal, SignalPort receivingPort)
        {   
            base.OnSignalRecieved(signal, receivingPort);

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
