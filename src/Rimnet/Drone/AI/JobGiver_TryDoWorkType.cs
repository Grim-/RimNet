using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class JobGiver_TryDoWorkType : ThinkNode_JobGiver
    {
        public WorkTypeDef WorkType;
        protected override Job TryGiveJob(Pawn pawn)
        {
			IntVec3 cell = pawn.Position;
			foreach (var workGiver in WorkType.workGiversByPriority)
            {

				WorkGiver_Scanner scanner = workGiver.Worker as WorkGiver_Scanner;
				if (scanner != null)
				{
					if (workGiver.scanThings)
					{
						Predicate<Thing> predicate = (Thing t) => !t.IsForbidden(pawn) && scanner.HasJobOnThing(pawn, t, false);
						foreach (var item in scanner.PotentialWorkThingsGlobal(pawn))
						{
							if (scanner.PotentialWorkThingRequest.Accepts(item) && predicate(item))
							{
								Job job2 = scanner.JobOnThing(pawn, item, false);
								if (job2 != null)
								{
									job2.workGiverDef = workGiver;
								}
								return job2;
							}
						}
					}
					if (workGiver.scanCells && !cell.IsForbidden(pawn) && scanner.HasJobOnCell(pawn, cell, false))
					{
						Job job3 = scanner.JobOnCell(pawn, cell, false);
						if (job3 != null)
						{
							job3.workGiverDef = workGiver;
						}
						return job3;
					}
				}

				return workGiver.Worker.NonScanJob(pawn);
			}

			return null;
		}
    }
}