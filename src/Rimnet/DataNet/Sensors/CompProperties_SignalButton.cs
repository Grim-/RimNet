using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class CompProperties_SignalButton : CompProperties_SignalSource
    {
        public int pressDurationTicks = 60;

        public CompProperties_SignalButton()
        {
            compClass = typeof(Comp_SignalButton);
        }
    }

    public class Comp_SignalButton : Comp_SignalSource
    {
        private int pressedUntilTick = -1;

        public CompProperties_SignalButton Props => (CompProperties_SignalButton)props;

        public bool IsPressed => Find.TickManager.TicksGame < pressedUntilTick;

        public void Press()
        {
            TriggerSignal(1f);
        }

        public override bool TryStartJob(Pawn pawn)
        {
            Press();
            return true;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Press Button",
                defaultDesc = $"Press the button for {Props.pressDurationTicks.ToStringTicksToPeriod()}",
                icon = TexButton.AutoRebuild,
                action = Press,
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref pressedUntilTick, "pressedUntilTick", -1);
        }
    }
}
