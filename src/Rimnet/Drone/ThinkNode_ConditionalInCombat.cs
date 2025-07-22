using Verse;
using Verse.AI;

namespace RimNet
{
    public class ThinkNode_ConditionalInCombat : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn is Drone drone && drone.mindState?.enemyTarget != null;
        }

        public override string ToString() => "Drone is in combat";
    }
}