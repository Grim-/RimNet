using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public abstract class NetworkCommandWorker : IExposable
    {
        public NetworkCommandDef def;
        
        public abstract bool IsCommandValidFor(Comp_NetworkNode targetNode, NetworkCommandContext networkCommandContext);
        public abstract void ExecuteCommand(Comp_NetworkNode targetNode, NetworkCommandContext networkCommandContext);

        public virtual void ExposeData()
        {
            
        }
    }


    public class PowerOffCommand : NetworkCommandWorker
    {
        public override void ExecuteCommand(Comp_NetworkNode targetNode, NetworkCommandContext networkCommandContext)
        {
            if (targetNode.parent.TryGetComp(out CompPowerTrader compPowerTrader))
            {
                compPowerTrader.PowerOn = false;
            }
        }

        public override bool IsCommandValidFor(Comp_NetworkNode networkNode, NetworkCommandContext networkCommandContext)
        {
            return networkNode.parent.TryGetComp(out CompPowerTrader compPowerTrader);
        }
    }


    public class TempCommand : NetworkCommandContext
    {
        public float targetTemp = 20f;
    }

    public class SetTemperatureCommand : NetworkCommandWorker
    {
        public override void ExecuteCommand(Comp_NetworkNode targetNode, NetworkCommandContext networkCommandContext)
        {
            if (targetNode.parent.TryGetComp(out CompTempControl tempControl) && networkCommandContext is TempCommand tempCommand)
            {
                tempControl.targetTemperature = tempCommand.targetTemp;
            }
        }

        public override bool IsCommandValidFor(Comp_NetworkNode networkNode, NetworkCommandContext networkCommandContext)
        {
            return networkNode.parent.TryGetComp(out CompTempControl tempControl);
        }
    }
}