using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class CompProperties_DronePowerSource : CompProperties
    {
        public float chargeCost = 0.05f;
        public StatDef powerStat;

        public CompProperties_DronePowerSource()
        {
            compClass = typeof(CompDronePowerSource);
        }
    }

    public class CompDronePowerSource : ThingComp
    {
        public CompProperties_DronePowerSource Props => (CompProperties_DronePowerSource)props;

        public float Current = 0;
        public float Max => Props.powerStat != null ? this.parent.GetStatValue(Props.powerStat) : 100;

        public float AsPercent => Mathf.Clamp01(Current / Max);

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                SetToMax();
            }
        }

        public void Restore(float amount)
        {
            this.Current = Mathf.Clamp(this.Current + amount, 0, Max);
        }

        public void Consume(float amount)
        {
            this.Current = Mathf.Clamp(this.Current - amount, 0, Max);
        }

        public bool Has(float amount)
        {
            return this.Current >= amount;
        }

        public void SetToMax()
        {
            this.Current = Max;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Gizmo_EnergyStatus(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref Current, "current");
        }
    }
}