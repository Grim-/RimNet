using RimWorld;
using Verse;

namespace RimNet
{
    public class CompProperties_FlickableSignalAdapter : CompProperties
    {
        public CompProperties_FlickableSignalAdapter()
        {
            compClass = typeof(CompSignal_FlickableAdapter);
        }
    }

    public class CompSignal_FlickableAdapter : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            CompSignalMediator mediator = parent.GetComp<CompSignalMediator>();
            CompFlickable emitter = parent.GetComp<CompFlickable>();

            if (mediator != null && emitter != null)
            {
                mediator.RegisterAction(signal =>
                {
                    emitter.SwitchIsOn = signal.AsBool;
                });
            }
        }
    }


}