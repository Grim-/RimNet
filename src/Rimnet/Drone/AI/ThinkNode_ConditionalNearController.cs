using Verse;
using Verse.AI;

namespace RimNet
{
    public class ThinkNode_ConditionalNearController : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            var drone = pawn as Drone;
            return drone?.Controller != null && drone.Position.DistanceTo(drone.Controller.parent.Position) <= 1f;
        }
    }
}