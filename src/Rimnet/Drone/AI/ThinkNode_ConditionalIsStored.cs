using Verse;
using Verse.AI;

namespace RimNet
{
    public class ThinkNode_ConditionalIsStored : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn is Drone drone &&
                   !drone.Spawned &&
                   drone.Controller != null &&
                   drone.Controller.GetDirectlyHeldThings().Contains(drone);
        }

        public override string ToString() => "Drone is stored in controller";
    }
}