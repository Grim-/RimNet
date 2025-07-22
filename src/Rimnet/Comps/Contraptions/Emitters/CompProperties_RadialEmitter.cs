using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public abstract class CompProperties_RadialEmitter : CompProperties_Emitter
    {
        public float radius = 3f;

        public CompProperties_RadialEmitter()
        {
            compClass = typeof(CompSprinkler);
        }
    }

    public abstract class CompRadialEmitter : CompEmitter
    {
        private CompFlickable flickableComp;
        private CompPowerTrader powerComp;

        private CompProperties_RadialEmitter Props => (CompProperties_RadialEmitter)props;

        private List<IntVec3> cachedCells = new List<IntVec3>();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            flickableComp = parent.GetComp<CompFlickable>();
            powerComp = parent.GetComp<CompPowerTrader>();
            cachedCells = GenRadial.RadialCellsAround(this.parent.Position, Props.radius, true).Where(x => x.IsValid).ToList();
        }

        private bool IsActive()
        {
            if (powerComp != null && !powerComp.PowerOn)
            {
                return false;
            }

            if (flickableComp != null && !flickableComp.SwitchIsOn)
            {
                return false;
            }

            return true;
        }

        public override void Emit()
        {
            if (IsActive())
            {
                PlayEmissionEffects();
                DoEmit();
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var item in base.CompGetGizmosExtra())
            {
                yield return item;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Emit",
                defaultDesc = $"Emit",
                icon = TexButton.AutoRebuild,
                action = Emit,
            };
        }
        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(this.parent.Position, Props.radius);
        }
    }


}