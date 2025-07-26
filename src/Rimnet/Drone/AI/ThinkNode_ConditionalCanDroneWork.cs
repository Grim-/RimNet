using RimWorld;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class ThinkNode_ConditionalCanDroneWork : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            var drone = pawn as Drone;
            return drone?.Controller != null && drone.IsEnabled && drone.IsPowered && drone.Spawned && drone.DroneState == DroneState.ACTIVE
                && (!pawn.Downed || pawn.health.CanCrawl)
                && !pawn.IsBurning()
                && !pawn.InMentalState
                && !pawn.Drafted
                && pawn.Awake();
        }
    }
}