using Verse;
using Verse.AI;

namespace RimNet
{
    public class ThinkNode_ConditionalIsDroneState : ThinkNode_Conditional
    {
        public DroneState stateToCheck;

        protected override bool Satisfied(Pawn pawn)
        {
            var drone = pawn as Drone;
            return drone?.controller != null && drone.DroneState == stateToCheck;
        }
    }
}