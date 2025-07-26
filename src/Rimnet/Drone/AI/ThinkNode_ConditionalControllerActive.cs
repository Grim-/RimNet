using Verse;
using Verse.AI;

namespace RimNet
{
    public class ThinkNode_ConditionalControllerActive : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (!(pawn is Drone drone))
                return false;

            var controller = drone.Controller;
            return controller != null && controller.IsActive && controller.IsPowered;
        }
    }
}