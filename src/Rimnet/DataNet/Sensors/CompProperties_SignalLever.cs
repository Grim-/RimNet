using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class CompProperties_SignalLever : CompProperties_SignalSource
    {
        public bool startOn = false;

        public CompProperties_SignalLever()
        {
            compClass = typeof(Comp_SignalLever);
        }
    }

    public class Comp_SignalLever : Comp_SignalSource
    {
        private bool leverOn;
        public CompProperties_SignalLever Props => (CompProperties_SignalLever)props;
        public bool LeverOn => leverOn;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                leverOn = Props.startOn;
            }
        }

        public void Toggle()
        {
            leverOn = !leverOn;
            TriggerSignal(leverOn ? 1 : 0);
        }

        public override bool TryStartJob(Pawn pawn)
        {
            Toggle();
            return true;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            yield return new Command_Toggle()
            {
                defaultLabel = "Toggle Lever",
                defaultDesc = leverOn ? "Turn lever off" : "Turn lever on",
                icon = TexButton.AutoRebuild,
                isActive = () => leverOn,
                toggleAction = Toggle
            };
        }

        public override string CompInspectStringExtra()
        {
            string baseString = base.CompInspectStringExtra();
            string leverState = $"Lever: {(leverOn ? "On" : "Off")}";

            if (!baseString.NullOrEmpty())
                return baseString + "\n" + leverState;
            return leverState;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref leverOn, "leverOn", false);
        }
    }
}
