using Verse;

namespace RimNet
{
    public struct Signal : IExposable
    {
        public float Value;
        public int LastChangeTick;

        public Signal(float value, int lastChangeTick)
        {
            Value = value;
            LastChangeTick = lastChangeTick;
        }
        public Signal(float value)
        {
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
