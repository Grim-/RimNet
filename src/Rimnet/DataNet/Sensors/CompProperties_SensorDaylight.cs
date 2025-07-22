using System.Collections.Generic;
using System.Linq;
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
        public DayLightSensorMode currentMode = DayLightSensorMode.DAWN;
        public float customThreshold = 0.4f;
        protected float lastSkyGlow = -1f;

        public void SetMode(DayLightSensorMode mode)
        {
            currentMode = mode;
        }

        public void SetCustomThreshold(float value)
        {
            customThreshold = value;
        }

        public override void CheckSensor()
        {
            float currentGlow = GetSensorValue();

            if (lastSkyGlow < 0f)
            {
                lastSkyGlow = currentGlow;
                return;
            }

            bool shouldTrigger = false;

            switch (currentMode)
            {
                case DayLightSensorMode.DAWN:
                    shouldTrigger = lastSkyGlow <= 0.3f && currentGlow > 0.3f;
                    break;

                case DayLightSensorMode.DUSK:
                    shouldTrigger = lastSkyGlow >= 0.7f && currentGlow < 0.7f;
                    break;

                case DayLightSensorMode.MIDNIGHT:
                    shouldTrigger = lastSkyGlow > 0f && currentGlow <= 0f;
                    break;

                case DayLightSensorMode.CUSTOM:
                    shouldTrigger = (lastSkyGlow < customThreshold && currentGlow >= customThreshold) ||
                                   (lastSkyGlow > customThreshold && currentGlow <= customThreshold);
                    break;
            }

            lastSkyGlow = currentGlow;

            if (shouldTrigger)
            {
                TriggerSignal(1f);
            }
        }

        public override float GetSensorValue()
        {
            if (!this.parent.Spawned || this.parent.Map == null)
            {
                return 0f;
            }
            return this.parent.Map.skyManager.CurSkyGlow;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            yield return new Command_Action
            {
                defaultLabel = $"Mode: {currentMode}",
                defaultDesc = "Change daylight sensor mode",
                action = () =>
                {
                    var options = new List<FloatMenuOption>();
                    foreach (DayLightSensorMode mode in System.Enum.GetValues(typeof(DayLightSensorMode)))
                    {
                        var capturedMode = mode;
                        options.Add(new FloatMenuOption(capturedMode.ToString(), () => SetMode(capturedMode)));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };
        }

        public override string CompInspectStringExtra()
        {
            string baseString = base.CompInspectStringExtra();
            baseString += $"\nMode: {currentMode}";
            baseString += $"\nCurrent light: {GetSensorValue():F2}";
            if (currentMode == DayLightSensorMode.CUSTOM)
            {
                baseString += $"\nThreshold: {customThreshold:F2}";
            }
            return baseString;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentMode, "currentMode", DayLightSensorMode.DAWN);
            Scribe_Values.Look(ref customThreshold, "customThreshold", 0.4f);
            Scribe_Values.Look(ref lastSkyGlow, "lastSkyGlow", -1f);
        }
    }

    public enum DayLightSensorMode
    {
        DAWN,
        DUSK,
        MIDNIGHT,
        CUSTOM
    }
}