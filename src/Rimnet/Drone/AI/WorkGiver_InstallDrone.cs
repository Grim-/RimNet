using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class WorkGiver_InstallDrone : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.HaulableEver);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerThings.AllThings;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t == null || t.Map == null)
                return null;
            if (!IsDisassembledDrone(t))
                return null;

            Thing controller = GenClosest.ClosestThingReachable(
                t.Position,
                t.Map,
                ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
                PathEndMode.InteractionCell,
                TraverseParms.For(pawn),
                20f,
                c =>
                {
                    var comp = c.TryGetComp<CompDroneController>();
                    return comp != null;
                });

            if (controller == null)
                return null;

            return JobMaker.MakeJob(RimNetDefOf.InstallDrone, t, controller);
        }

        private bool IsDisassembledDrone(Thing t)
        {
            return t.def.thingCategories?.Contains(RimNetDefOf.Drones) == true;
        }
    }
}