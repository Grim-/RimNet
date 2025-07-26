using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimNet
{

    public class JobGiver_TrydoWorkGiver : ThinkNode_JobGiver
    {

		public WorkGiverDef workDef;

        protected override Job TryGiveJob(Pawn pawn)
        {
			IntVec3 cell = pawn.Position;
			WorkGiver_Scanner scanner = workDef.Worker as WorkGiver_Scanner;
			if (scanner != null)
			{
				if (workDef.scanThings)
				{					
					Predicate<Thing> predicate = (Thing t) => !t.IsForbidden(pawn) && scanner.HasJobOnThing(pawn, t, false);
                    foreach (var item in scanner.PotentialWorkThingsGlobal(pawn))
                    {
						if (scanner.PotentialWorkThingRequest.Accepts(item) && predicate(item))
						{
							Job job2 = scanner.JobOnThing(pawn, item, false);
							if (job2 != null)
							{
								job2.workGiverDef = workDef;
							}
							return job2;
						}
					}
				}
				if (workDef.scanCells && !cell.IsForbidden(pawn) && scanner.HasJobOnCell(pawn, cell, false))
				{
					Job job3 = scanner.JobOnCell(pawn, cell, false);
					if (job3 != null)
					{
						job3.workGiverDef = workDef;
					}
					return job3;
				}			
			}

			return workDef.Worker.NonScanJob(pawn);
		}
    }
}