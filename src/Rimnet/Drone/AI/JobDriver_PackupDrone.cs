using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class JobDriver_PackupDrone : JobDriver
    {
        private Drone Drone => (Drone)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed) =>
            pawn.Reserve(Drone, job);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            yield return gotoToil;

            yield return Toils_General.Wait(150, TargetIndex.A);

            var installToil = new Toil
            {
                initAction = () =>
                {
                    Map map = this.Map;
                    IntVec3 spawnPos = CellFinder.RandomClosewalkCellNear(this.pawn.Position, map, 3);
                    ThingWithComps withComps = Drone.CreateKit();
                    if (withComps != null)
                    {
                        if (spawnPos.IsValid)
                        {
                            GenSpawn.Spawn(withComps, spawnPos, map);
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield return installToil;
        }
    }
}