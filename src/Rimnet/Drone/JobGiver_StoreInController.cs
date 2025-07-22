using Verse;
using Verse.AI;

namespace RimNet
{
    public class JobGiver_StoreInController : ThinkNode_JobGiver
    {
        public JobDef storeJob;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn is Drone drone && drone.controller != null)
            {
                return JobMaker.MakeJob(storeJob, drone.controller.parent);
            }

            return null;
        }
    }
}