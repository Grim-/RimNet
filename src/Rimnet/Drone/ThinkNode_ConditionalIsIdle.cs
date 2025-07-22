using Verse;
using Verse.AI;

namespace RimNet
{
    public class ThinkNode_ConditionalIsIdle : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn is Drone drone && (drone.jobs?.curJob == null || drone.jobs.curDriver == null);
        }

        public override string ToString() => "Drone is idle";
    }
}