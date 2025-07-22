using Verse;

namespace RimNet
{
    public class CompProperties_SignalDelayNode : CompProperties_SignalNode
    {
        public int defaultDelayTicks = 400;
        public float defaultCustomSignalValue = 1f;
        public bool defaultUseCustomSignal = false;

        public CompProperties_SignalDelayNode()
        {
            compClass = typeof(CompSignalDelayNode);
        }
    }

    public class CompSignalDelayNode : Comp_SignalNode
    {
        private CompProperties_SignalDelayNode Props => (CompProperties_SignalDelayNode)props;
        private int delayTicks;
        private float customSignalValue;
        private bool useCustomSignal;

        private bool isArmed = false;
        private int scheduledTick = -1;
        private Signal signalToSend;

        public int DelayTicks
        {
            get => delayTicks;
            set => delayTicks = value;
        }
        public float CustomSignalValue
        {
            get => customSignalValue;
            set => customSignalValue = value;
        }
        public bool UseCustomSignal
        {
            get => useCustomSignal;
            set => useCustomSignal = value;
        }



        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new System.Collections.Generic.List<SignalPort>();
            CreatePort(SignalPortType.BOTH, IntVec3.Zero, "BOTH", 0);
        }

        public override void OnSignalRecieved(Signal signal, SignalPort receivingPort)
        {
            if (isArmed)
            {
                return;
            }

            isArmed = true;
            signalToSend = signal;
            scheduledTick = Find.TickManager.TicksGame + delayTicks;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!isArmed)
            {
                return;
            }

            if (Find.TickManager.TicksGame >= scheduledTick)
            {
                float valueToSend = useCustomSignal ? this.customSignalValue : this.signalToSend.Value;

                TriggerSignal(valueToSend);
                isArmed = false;
                scheduledTick = -1;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref delayTicks, "delayTicks", Props.defaultDelayTicks);
            Scribe_Values.Look(ref customSignalValue, "customSignalValue", Props.defaultCustomSignalValue);
            Scribe_Values.Look(ref useCustomSignal, "useCustomSignal", Props.defaultUseCustomSignal);
            Scribe_Values.Look(ref isArmed, "isArmed", false);
            Scribe_Values.Look(ref scheduledTick, "scheduledTick", -1);
            Scribe_Deep.Look(ref signalToSend, "signalToSend");
        }
    }
}
