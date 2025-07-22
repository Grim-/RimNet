using Verse;
using Verse.AI;

namespace RimNet
{
   public class ThinkNode_ConditionalShouldShutdown : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn is Drone drone && drone.ShouldShutDown();
        }
    }
}