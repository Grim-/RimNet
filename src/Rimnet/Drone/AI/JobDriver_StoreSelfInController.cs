using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class JobDriver_StoreSelfInController : JobDriver
    {
        protected CompDroneController DroneController => this.TargetA.Thing.TryGetComp<CompDroneController>();


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.WaitWith(TargetIndex.A, 100, true, false, false, TargetIndex.A);
            Toil someToil = new Toil();
            someToil.defaultCompleteMode = ToilCompleteMode.Instant;
            someToil.initAction = () =>
            {
                DroneController.StoreDrone((Drone)this.pawn);
            };
            yield return someToil;
        }
    }
}