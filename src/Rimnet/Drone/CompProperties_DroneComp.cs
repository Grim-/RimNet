using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class CompProperties_DroneComp : CompProperties
    {
        public ThingDef disassembledKind;

        public float powerCost = 0.1f;

        public CompProperties_DroneComp()
        {
            compClass = typeof(CompDrone);
        }
    }

    public class CompDrone : ThingComp
    {
        public CompProperties_DroneComp Props => (CompProperties_DroneComp)props;
    }
}