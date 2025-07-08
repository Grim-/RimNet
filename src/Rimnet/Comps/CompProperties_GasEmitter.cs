using RimWorld;
using Verse;

namespace RimNet
{
    public class CompProperties_GasEmitter : CompProperties
    {
        public GasType gasType;
        public int ticksBetweenEmissions = 100;
        public int emissionAmount = 20;
        public bool requiresPower = true;

        public CompProperties_GasEmitter()
        {
            compClass = typeof(CompGasEmitter);
        }
    }

    public class CompGasEmitter : ThingComp
    {
        private CompFlickable flickableComp;
        private CompPowerTrader powerComp;

        public CompProperties_GasEmitter Props => (CompProperties_GasEmitter)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            flickableComp = parent.GetComp<CompFlickable>();
            powerComp = parent.GetComp<CompPowerTrader>();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (parent.IsHashIntervalTick(Props.ticksBetweenEmissions) && IsActive())
            {
                EmitGas();
            }
        }

        private bool IsActive()
        {
            if (Props.requiresPower)
            {
                if (powerComp == null || !powerComp.PowerOn)
                {
                    return false;
                }
            }

            if (flickableComp != null && !flickableComp.SwitchIsOn)
            {
                return false;
            }

            return true;
        }

        private void EmitGas()
        {
            GasUtility.AddGas(parent.Position, parent.Map, Props.gasType, Props.emissionAmount);
        }
    }
}