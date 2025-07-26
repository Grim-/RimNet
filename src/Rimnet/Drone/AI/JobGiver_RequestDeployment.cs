using Verse;
using Verse.AI;

namespace RimNet
{
    public class JobGiver_RequestDeployment : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!(pawn is Drone drone))
                return null;

            var controller = drone.Controller;
            if (controller == null)
                return null;

            controller.TryDeployDrone(drone);
            return null;
        }
    }
}