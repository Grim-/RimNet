using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class JobDriver_InstallDrone : JobDriver
    {
        private ThingWithComps DroneKit => (ThingWithComps)job.targetA.Thing;
        private Thing Building => job.targetB.Thing;


        protected CompDroneInstall DroneComp => DroneKit.GetComp<CompDroneInstall>();

        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            pawn.Reserve(DroneKit, job) && pawn.Reserve(Building, job);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);

            var installToil = new Toil
            {
                initAction = () =>
                {
                    CompDroneController comp = Building.TryGetComp<CompDroneController>();
                    if (comp != null)
                    {
                        Drone dronePawn = DroneComp.RemoveStoredDrone();
                        if (dronePawn != null)
                        {
                            comp.StoreDrone(dronePawn);
                            DroneKit.Destroy();
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield return installToil;
        }
    }
}