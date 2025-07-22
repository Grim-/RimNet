using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class BridgeBuildingProps : BuildingProperties
    {
        public int bridgeLength = 15;
        public int bridgeWidth = 10;
        public TerrainDef bridgeTerrainDef;
    }
    public class Building_RetractableBridge : Building
    {
        private bool isExtended = false;
        private List<IntVec3> modifiedCells = new List<IntVec3>();
        private List<TerrainDef> originalTerrains = new List<TerrainDef>();
        private CompPowerTrader powerComp;

        private BridgeBuildingProps DefExtension => def.building as BridgeBuildingProps;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = this.GetComp<CompPowerTrader>();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (isExtended)
            {
                TryRetract(false);
            }
            base.DeSpawn(mode);
        }

        private IEnumerable<IntVec3> GetBridgeCells()
        {
            IntVec3 facingDir = this.Rotation.FacingCell;
            IntVec3 perpDir = new IntVec3(-facingDir.z, 0, facingDir.x);

            int halfWidth = DefExtension.bridgeWidth / 2;

            for (int lengthOffset = 1; lengthOffset <= DefExtension.bridgeLength; lengthOffset++)
            {
                for (int widthOffset = -halfWidth; widthOffset <= halfWidth; widthOffset++)
                {
                    IntVec3 cell = this.Position + (facingDir * lengthOffset) + (perpDir * widthOffset);
                    yield return cell;
                }
            }
        }

        private bool IsPathClear()
        {
            foreach (IntVec3 cell in GetBridgeCells())
            {
                if (!cell.InBounds(this.Map))
                {
                    return false;
                }

                Building edifice = cell.GetEdifice(this.Map);
                if (edifice != null)
                {
                    return false;
                }
            }
            return true;
        }

        private void TryExtend()
        {
            if (isExtended || !powerComp.PowerOn) return;

            if (IsPathClear())
            {
                modifiedCells.Clear();
                originalTerrains.Clear();

                foreach (IntVec3 cell in GetBridgeCells())
                {
                    TerrainDef currentTerrain = cell.GetTerrain(this.Map);
                    modifiedCells.Add(cell);
                    originalTerrains.Add(currentTerrain);

                    this.Map.terrainGrid.SetTerrain(cell, DefExtension.bridgeTerrainDef);
                }

                isExtended = true;
            }
            else
            {
                Messages.Message("Cannot extend bridge: Path is blocked.", MessageTypeDefOf.RejectInput, false);
            }
        }

        private void TryRetract(bool showMessage = true)
        {
            if (!isExtended) return;

            if (!powerComp.PowerOn && showMessage)
            {
                Messages.Message("Cannot retract bridge: No power.", MessageTypeDefOf.RejectInput, false);
                return;
            }

            for (int i = 0; i < modifiedCells.Count; i++)
            {
                IntVec3 cell = modifiedCells[i];
                if (cell.InBounds(this.Map) && i < originalTerrains.Count)
                {
                    this.Map.terrainGrid.SetTerrain(cell, originalTerrains[i]);
                }
            }

            modifiedCells.Clear();
            originalTerrains.Clear();
            isExtended = false;
        }


        public override void DrawExtraSelectionOverlays()
        {
            base.DrawExtraSelectionOverlays();

            GenDraw.DrawFieldEdges(GetBridgeCells().ToList(), Color.white);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (isExtended)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Retract Bridge",
                    defaultDesc = "Retracts the bridge.",
                    icon = TexCommand.ForbidOff,
                    action = () => TryRetract()
                };
            }
            else
            {
                yield return new Command_Action
                {
                    defaultLabel = "Extend Bridge",
                    defaultDesc = "Extends the bridge over a gap.",
                    icon = TexCommand.ForbidOn,
                    action = () => TryExtend()
                };
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isExtended, "isExtended", false);
            Scribe_Collections.Look(ref modifiedCells, "modifiedCells", LookMode.Value);
            Scribe_Collections.Look(ref originalTerrains, "originalTerrains", LookMode.Def);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (modifiedCells == null) modifiedCells = new List<IntVec3>();
                if (originalTerrains == null) originalTerrains = new List<TerrainDef>();
            }
        }
    }
}
