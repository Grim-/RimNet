using Verse;

namespace RimNet
{
    public class CompProperties_SensorDaylight : CompProperties_SignalSensor
    {
        public CompProperties_SensorDaylight()
        {
            compClass = typeof(Comp_SensorDaylight);
        }
    }

    public class Comp_SensorDaylight : Comp_SignalSensor
    {
        protected float dayLightThreshold = 0.4f;

        protected override bool CheckSensor()
        {
            if (!this.parent.Spawned || this.parent.Map == null)
                return false;

            if (this.parent.Map.skyManager.CurSkyGlow >= dayLightThreshold)
            {
                return true;
            }

            return false;
        }

        public void SetThreshold(float value)
        {
            dayLightThreshold = value;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref dayLightThreshold, "dayLightThreshold");
        }
    }

    public enum SensorFilterType
    {
        LESS_THAN,
        EQUAL_TO,
        GREATER_THAN
    }
}
