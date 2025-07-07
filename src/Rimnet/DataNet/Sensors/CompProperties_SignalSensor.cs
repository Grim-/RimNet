using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public abstract class CompProperties_SignalSensor : CompProperties_SignalSource
    {

    }

    public abstract class Comp_SignalSensor : Comp_SignalSource
    {
        CompProperties_SignalSensor Props => (CompProperties_SignalSensor)props;

        public bool ShouldInvert = false;

        public int customUpdateInterval = -1;
        public string intervalBuffer;

        public virtual int DefaultUpdateInterval => 250;

        public int CurrentUpdateInterval => customUpdateInterval >= 0 ? customUpdateInterval : DefaultUpdateInterval;

        public bool Invert
        {
            get => ShouldInvert;
            set => ShouldInvert = value;
        }

        public int CustomUpdateInterval
        {
            get => customUpdateInterval;
            set => customUpdateInterval = value;
        }

        public string IntervalBuffer
        {
            get => intervalBuffer;
            set => intervalBuffer = value;
        }


        public override void Initialize(CompProperties props)
        {
            this.parent.def.tickerType = TickerType.Normal;
            base.Initialize(props);
        }

        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>
            {
                new SignalPort(this, SignalPortType.OUT, IntVec3.Zero)
            };
        }

        public override void CompTick()
        {
            base.CompTick();

            if (this.parent.Spawned && this.parent.IsHashIntervalTick(CurrentUpdateInterval))
            {
                CheckSensor();
            }
        }

        public virtual void CheckSensor()
        {
            float currentValue = GetSensorValue();
            TriggerSignal(currentValue);
        }

        protected abstract float GetSensorValue();

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            yield return new Gizmo_SensorConfig(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ShouldInvert, "ShouldInvert", false);
            Scribe_Values.Look(ref customUpdateInterval, "customUpdateInterval", -1);
        }

        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra() + $"\nSensor Value: {GetSensorValue():F2}" + $"\nUpdate Interval: {CurrentUpdateInterval / 60f:F2}s";
        }
    }

}
