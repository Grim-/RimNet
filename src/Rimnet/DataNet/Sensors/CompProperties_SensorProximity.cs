using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class CompProperties_SensorProximity : CompProperties_SignalSensor
    {
        public CompProperties_SensorProximity()
        {
            compClass = typeof(Comp_SensorProximity);
        }
    }


    public class Comp_SensorProximity : Comp_SignalSensor
    {
        public float ActiveRadius = 2f;
        public SensorTargetType TargetType = SensorTargetType.ANY_PAWN;

        public void SetRadius(float newRadius)
        {
            ActiveRadius = Mathf.Max(1, newRadius);
        }


        public override float GetSensorValue()
        {
            if (!this.parent.Spawned || this.parent.Map == null)
            {
                return 0f;
            }

            List<Pawn> pawnsInRange = GenRadial.RadialDistinctThingsAround(this.parent.Position, this.parent.Map, ActiveRadius, true).Where(x => x is Pawn).Cast<Pawn>().ToList();

            return pawnsInRange.Count > 0 ? 1 : 0;
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();

            Color color = GetSensorValue() >= 1f? Color.red : Color.green;

            GenDraw.DrawFieldEdges(GenRadial.RadialCellsAround(this.parent.Position, ActiveRadius, true).ToList(), color);
        }
    }

}
