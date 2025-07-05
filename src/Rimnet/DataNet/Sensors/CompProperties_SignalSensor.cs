using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public abstract class CompProperties_SignalSensor : CompProperties_SignalSource
    {
        public int sensorUpdateInterval = 100;  
    }

    public abstract class Comp_SignalSensor : Comp_SignalSource
    {
        CompProperties_SignalSensor Props => (CompProperties_SignalSensor)props;

        protected bool ShouldInvert = false;

        public bool Invert
        {
            get => ShouldInvert;
            set => ShouldInvert = value;
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

            if (this.parent.Spawned && this.parent.IsHashIntervalTick(Props.sensorUpdateInterval))
            {
                if (DoSensorCheck())
                {
                    OnSensorTriggered(1);
                }
            }
        }

        public virtual void OnSensorTriggered(float sensorValue)
        {
            TriggerSignal(sensorValue);
        }


        protected bool DoSensorCheck()
        {
            if (ShouldInvert)
            {
                return !CheckSensor();
            }

            return CheckSensor();
        }

        protected abstract bool CheckSensor();


        public override bool IsSplitterNode()
        {
            return false;
        }

        public override bool IsSignalTerminal()
        {
            return ConnectedChildren.Count == 0;
        }
    }


}
