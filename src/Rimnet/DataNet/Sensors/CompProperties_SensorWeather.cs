using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class CompProperties_SensorWeather : CompProperties_SignalSensor
    {
        public CompProperties_SensorWeather()
        {
            compClass = typeof(Comp_SensorWeather);
        }
    }

    public class Comp_SensorWeather : Comp_SignalSensor
    {
        private WeatherDef lastWeather = null;
        public WeatherDef targetWeather = null;
        public WeatherSensorMode sensorMode = WeatherSensorMode.ON_START;

        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            ConnectionPorts.Add(new SignalPort(this, SignalPortType.OUT, IntVec3.Zero));
        }

        public override bool IsSignalTerminal()
        {
            return false;
        }

        public override void CheckSensor()
        {
            if (!this.parent.Spawned || this.parent.Map == null || targetWeather == null)
                return;

            WeatherDef currentWeather = this.parent.Map.weatherManager.curWeather;

            if (lastWeather == null)
            {
                lastWeather = currentWeather;
                return;
            }

            bool shouldTrigger = false;

            switch (sensorMode)
            {
                case WeatherSensorMode.ON_START:
                    shouldTrigger = lastWeather != targetWeather && currentWeather == targetWeather;
                    break;

                case WeatherSensorMode.ON_STOP:
                    shouldTrigger = lastWeather == targetWeather && currentWeather != targetWeather;
                    break;
            }

            lastWeather = currentWeather;

            if (shouldTrigger)
            {
                TriggerSignal(1f);
            }
        }

        public override float GetSensorValue()
        {
            if (!this.parent.Spawned || this.parent.Map == null || targetWeather == null)
                return 0f;

            return this.parent.Map.weatherManager.curWeather == targetWeather ? 1f : 0f;
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            yield return new Command_Action
            {
                defaultLabel = $"Mode: {sensorMode}",
                defaultDesc = "Change weather detection mode",
                action = () =>
                {
                    var options = new List<FloatMenuOption>();
                    foreach (WeatherSensorMode mode in System.Enum.GetValues(typeof(WeatherSensorMode)))
                    {
                        var capturedMode = mode;
                        options.Add(new FloatMenuOption(capturedMode.ToString(), () => sensorMode = capturedMode));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };

            yield return new Command_Action
            {
                defaultLabel = targetWeather != null ? $"Weather: {targetWeather.label}" : "Weather: None",
                defaultDesc = "Select weather to detect",
                action = () =>
                {
                    var options = new List<FloatMenuOption>();
                    foreach (WeatherDef weather in DefDatabase<WeatherDef>.AllDefs)
                    {
                        var capturedWeather = weather;
                        options.Add(new FloatMenuOption(weather.label, () => targetWeather = capturedWeather));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };
        }

        public override string CompInspectStringExtra()
        {
            string baseString = base.CompInspectStringExtra();
            WeatherDef currentWeather = this.parent.Map?.weatherManager.curWeather;

            baseString += $"\nMode: {sensorMode}";
            if (targetWeather != null)
            {
                baseString += $"\nTarget: {targetWeather.label}";
            }
            if (currentWeather != null)
            {
                baseString += $"\nCurrent: {currentWeather.label}";
            }

            return baseString;
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref sensorMode, "sensorMode", WeatherSensorMode.ON_START);
            Scribe_Defs.Look(ref lastWeather, "lastWeather");
            Scribe_Defs.Look(ref targetWeather, "targetWeather");
        }
    }

    public enum WeatherSensorMode
    {
        ON_START,
        ON_STOP
    }
}