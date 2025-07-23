using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class CompProperties_DroneComp : CompProperties
    {
        public PawnKindDef dronePawnKind;

        public CompProperties_DroneComp()
        {
            compClass = typeof(DroneComp);
        }
    }

    public class DroneComp : ThingComp
    {
        public CompProperties_DroneComp Props => (CompProperties_DroneComp)props;

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            yield return new FloatMenuOption($"Install to..", () =>
            {
                Find.Targeter.BeginTargeting(TargetingParameters.ForBuilding(), (LocalTargetInfo target) =>
                {
                    if (target.HasThing && target.Thing.TryGetComp(out CompDroneController droneController))
                    {
                        Job job = JobMaker.MakeJob(RimNetDefOf.InstallDrone, this.parent, target.Thing);
                        job.count = 1;
                        selPawn.jobs.TryTakeOrderedJob(job);
                    }
                });



            });

            foreach (var item in this.parent.MapHeld.listerBuildings.allBuildingsColonist.Where(x => x.TryGetComp<CompDroneController>() != null))
            {

            }
        }
    }
}