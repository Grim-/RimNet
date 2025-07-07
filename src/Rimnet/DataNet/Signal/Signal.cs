using Verse;

namespace RimNet
{
    public struct Signal : IExposable
    {
        public float Value;
        public int LastChangeTick;

        public Comp_SignalNode SignalSource;

        public Signal(Comp_SignalNode source, float value, int lastChangeTick)
        {
            SignalSource = source;
               Value = value;
            LastChangeTick = lastChangeTick;
        }

        public Signal(Comp_SignalNode source, float value)
        {
            SignalSource = source;
            Value = value;
            LastChangeTick = Find.TickManager.TicksGame;
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref Value, "Value");
            Scribe_Values.Look(ref LastChangeTick, "LastChangeTick");
        }
    }
}
