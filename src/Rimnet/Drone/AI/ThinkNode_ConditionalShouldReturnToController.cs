using RimWorld;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class ThinkNode_ConditionalShouldReturnToController : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (!(pawn is Drone drone) || drone.Controller == null || drone.Destroyed || !drone.Spawned)
                return false;

            return drone.ShouldReturn &&
               (drone.CurJob == null || drone.CurJob.def != JobDefOf.Goto);
        }

        public override string ToString() => "Drone should return to controller";
    }
}