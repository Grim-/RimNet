using Verse;
using Verse.AI;

namespace RimNet
{
    public class ThinkNode_ConditionalNoController : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            var drone = pawn as Drone;
            return drone?.Controller == null || drone.Controller.parent.Destroyed;
        }
    }
}