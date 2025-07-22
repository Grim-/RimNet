using RimWorld;
using Verse;

namespace RimNet
{
    public class CompProperties_Contraption : CompProperties
    {
        public CompProperties_Contraption()
        {
            compClass = typeof(CompContraption);
        }
    }

    public class CompContraption : ThingComp
    {
        private CompFlickable flickableComp;
        private CompPowerTrader powerComp;

        public CompProperties_Contraption Props => (CompProperties_Contraption)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            flickableComp = parent.GetComp<CompFlickable>();
            powerComp = parent.GetComp<CompPowerTrader>();
        }

        public virtual bool IsActive()
        {
            if (powerComp != null && !powerComp.PowerOn)
            {
                return false;
            }

            if (flickableComp != null && !flickableComp.SwitchIsOn)
            {
                return false;
            }

            return true;
        }
    }
}