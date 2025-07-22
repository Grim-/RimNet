using RimWorld;
using Verse;

namespace RimNet
{
    public class CompProperties_PowerSignalAdapter : CompProperties
    {
        public CompProperties_PowerSignalAdapter()
        {
            compClass = typeof(CompSignal_PowerAdapter);
        }
    }

    public class CompSignal_PowerAdapter : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            CompSignalMediator mediator = parent.GetComp<CompSignalMediator>();
            CompPowerTrader emitter = parent.GetComp<CompPowerTrader>();

            if (mediator != null && emitter != null)
            {
                mediator.RegisterAction(signal =>
                {
                    emitter.PowerOn = signal.AsBool;
                });
            }
        }
    }
}