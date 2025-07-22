using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{


    public class CompProperties_Sprinkler : CompProperties_Contraption
    {
        public float radius = 3f;
        public float growthIncrease = 0.1f;
        public int ticksBetweenEmissions = 2400;
        public int ticksBetweenSplashEffect = 150;
        public FloatRange splashEffectScale = new FloatRange(0.1f, 0.2f);

        public CompProperties_Sprinkler()
        {
            compClass = typeof(CompSprinkler);
        }
    }

    public class CompSprinkler : CompContraption
    {
        private CompProperties_Sprinkler Props => (CompProperties_Sprinkler)props;

        private List<IntVec3> cells = new List<IntVec3>();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            cells = GenRadial.RadialCellsAround(this.parent.Position, Props.radius, true).ToList();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (IsActive())
            {
                IntVec3 chosenCell = cells.Where(x=> x.GetPlant(this.parent.Map) != null).RandomElement();

                if (chosenCell.IsValid)
                {
                    if (parent.IsHashIntervalTick(Props.ticksBetweenSplashEffect))
                    {
                        EffecterDefOf.PawnEmergeFromWater.Spawn(chosenCell, this.parent.Map, Props.splashEffectScale.RandomInRange);
                    }

                    if (parent.IsHashIntervalTick(Props.ticksBetweenEmissions))
                    {
                        Plant plant = chosenCell.GetPlant(this.parent.Map);

                        if (plant != null && plant.LifeStage == PlantLifeStage.Growing)
                        {
                            plant.Growth += Props.growthIncrease;
                        }
                    }
                }

            }



        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            GenDraw.DrawRadiusRing(this.parent.Position, Props.radius);
        }
    }


}