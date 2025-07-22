using RimWorld;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class JobGiver_GotoController : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn is Drone drone && drone.controller != null && drone.ShouldReturn)
            {
                Job job = JobMaker.MakeJob(JobDefOf.Goto, drone.controller.parent);
                return job;
            }

            return null;
        }
    }
}