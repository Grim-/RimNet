using RimWorld;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class JobGiver_GotoController : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn is Drone drone && drone.Controller != null && drone.ShouldReturn && drone.Controller.TryGetNearbyEmptyCell(out IntVec3 cell))
            {
                Job job = JobMaker.MakeJob(JobDefOf.Goto, cell);
                return job;
            }

            return null;
        }
    }
}