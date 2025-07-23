using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class JobDriver_InstallDrone : JobDriver
    {
        private ThingWithComps Drone => (ThingWithComps)job.targetA.Thing;
        private Thing Building => job.targetB.Thing;


        protected DroneComp DroneComp => Drone.GetComp<DroneComp>();

        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            pawn.Reserve(Drone, job) && pawn.Reserve(Building, job);

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
                        Drone dronePawn = (Drone)PawnGenerator.GeneratePawn(DroneComp.Props.dronePawnKind, Building.Faction);
                        dronePawn.SetController(comp);
                        comp.StoreDrone(dronePawn);
                        Drone.Destroy();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield return installToil;
        }
    }
}