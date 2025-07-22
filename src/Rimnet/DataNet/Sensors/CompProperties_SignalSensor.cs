using RimWorld;
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

        public abstract float GetSensorValue();



        public bool IsValid(SensorTargetType sensorTargetType, Thing thing)
        {
            switch (sensorTargetType)
            {
                case SensorTargetType.ANY_PAWN:
                    return thing is Pawn;
                case SensorTargetType.COLONISTS:
                    return thing is Pawn pawn && pawn.IsColonist;
                case SensorTargetType.ENEMIES:
                    return thing is Pawn enemy && enemy.HostileTo(Faction.OfPlayer);
                case SensorTargetType.ANIMALS:
                    return thing is Pawn anmalPawn && anmalPawn.RaceProps.Animal;
                case SensorTargetType.NON_COLONISTS:
                    return thing is Pawn non && !non.IsColonist;
            }

            return false;
        }

        public override void SyncWithGroupNode(Comp_SignalNode senderNode)
        {
            base.SyncWithGroupNode(senderNode);


            if (senderNode is Comp_SignalSensor signalSensor)
            {
                this.customUpdateInterval = signalSensor.customUpdateInterval;
                this.ShouldInvert = signalSensor.ShouldInvert;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
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
