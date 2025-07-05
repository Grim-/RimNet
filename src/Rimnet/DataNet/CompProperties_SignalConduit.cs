using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class CompProperties_SignalConduit : CompProperties_SignalNode
    {
        public int signalDurationTicks = 60;

        public CompProperties_SignalConduit()
        {
            compClass = typeof(Comp_SignalConduit);
        }
    }

    public class Comp_SignalConduit : Comp_SignalTransmitter
    {
        private int signalEndTick = -1;
        private CompColorable colorableComp;

        public CompProperties_SignalConduit Props => (CompProperties_SignalConduit)props;

        public bool IsSignalActive => signalEndTick > 0 && Find.TickManager.TicksGame < signalEndTick;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            colorableComp = parent.GetComp<CompColorable>();
        }

        public override int GetConnectionPriority(Comp_SignalNode otherNode)
        {
            return otherNode.IsSignalTerminal() ? -100 : base.GetConnectionPriority(otherNode);
        }

        public override void CompTick()
        {
            base.CompTick();

            if (ShouldResetColor())
            {
                ResetColor();
            }
        }

        public override void OnSignalRecieved(Signal signal)
        {
            base.OnSignalRecieved(signal);
            SetSignalColor();
        }

        private bool ShouldResetColor()
        {
            return signalEndTick > 0 && Find.TickManager.TicksGame >= signalEndTick;
        }

        private void SetSignalColor()
        {
            signalEndTick = Find.TickManager.TicksGame + Props.signalDurationTicks;
            SetColor(Color.green);
        }

        private void ResetColor()
        {
            signalEndTick = -1;
            DisableColor();
        }

        private void SetColor(Color color)
        {
            colorableComp?.SetColor(color);
        }

        private void DisableColor()
        {
            colorableComp?.Disable();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref signalEndTick, "signalEndTick", -1);
        }
    }
}