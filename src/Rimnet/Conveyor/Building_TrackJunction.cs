using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class Building_TrackJunction : Building_Track
    {
        private Rot4 outputDirection = Rot4.North;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override Building_Track GetPreferredNextBelt()
        {
            var spatialData = GetSpatialData();
            switch (outputDirection.AsInt)
            {
                case 0: return spatialData.NORTH;
                case 1: return spatialData.EAST;
                case 2: return spatialData.SOUTH;
                case 3: return spatialData.WEST;
                default: return null;
            }
        }

        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();
            var nextBelt = GetPreferredNextBelt();
            if (nextBelt != null)
            {
                GenDraw.DrawFieldEdges(new List<IntVec3>() { nextBelt.Position });
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            yield return new Command_Action
            {
                defaultLabel = "Rotate Output",
                defaultDesc = "Changes the output direction for items.",
                icon = TexButton.Add,
                action = () =>
                {
                    outputDirection.Rotate(RotationDirection.Clockwise);

                    // Force network to rebuild connections
                    if (network != null)
                    {
                        network.UpdateAllConnections();
                    }
                }
            };
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref outputDirection, "outputDirection", Rot4.North);
        }
    }
}