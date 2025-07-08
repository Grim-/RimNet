using Verse;

namespace RimNet
{
    public class HeldItem : IExposable
    {
        public Thing Item;
        public float Progress;

        public HeldItem() { }
        public HeldItem(Thing item)
        {
            this.Item = item;
            this.Progress = 0.0f;
        }
        public void ExposeData()
        {
            Scribe_References.Look(ref this.Item, "item");
            Scribe_Values.Look(ref this.Progress, "progress", 0.0f);
        }
    }
}