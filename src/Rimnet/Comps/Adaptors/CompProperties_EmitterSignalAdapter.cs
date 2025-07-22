using RimWorld;
using Verse;

namespace RimNet
{
    public class CompProperties_EmitterSignalAdapter : CompProperties
    {
        public CompProperties_EmitterSignalAdapter()
        {
            compClass = typeof(CompSignal_EmitterAdapter);
        }
    }

    public class CompSignal_EmitterAdapter : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            CompSignalMediator mediator = parent.GetComp<CompSignalMediator>();
            CompEmitter emitter = parent.GetComp<CompEmitter>();

            if (mediator != null && emitter != null)
            {
                mediator.RegisterAction(signal =>
                {
                    if (signal.AsBool)
                    {
                        emitter.Emit();
                    }
                });
            }
        }
    }


}