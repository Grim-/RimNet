using Verse;

namespace RimNet
{
    public class PlaceWorker_Data : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            //List<Thing> thingList = loc.GetThingList(map);
            //for (int i = 0; i < thingList.Count; i++)
            //{
            //    if (thingList[i].def != RimNetDefOf.NetworkCable)
            //    {
            //        return false;
            //    }
            //    if (thingList[i].def.entityDefToBuild != null)
            //    {
            //        ThingDef thingDef = thingList[i].def.entityDefToBuild as ThingDef;
            //        if (thingDef != null && thingDef != RimNetDefOf.NetworkCable)
            //        {
            //            return false;
            //        }
            //    }
            //}
            return true;
        }
    }
}