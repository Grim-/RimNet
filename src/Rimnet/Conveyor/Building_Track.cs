using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class Building_Track : Building, IThingHolder
    {
        protected List<HeldItem> heldItems = new List<HeldItem>();
        protected ThingOwner<Thing> innerContainer;
        protected const float CellsPerTick = 0.02f;
        protected TrackNetwork network;
        public Building_Track cachedNextBelt;
        public Building_Track cachedPrevBelt;

        public virtual Building_Track NextBelt => cachedNextBelt;
        public virtual Building_Track PreviousBelt => cachedPrevBelt;

        protected TrackManager ConveyorManager = null;

        public virtual TrackTypeDef TrackType { get; } = RimNetDefOf.ItemTrack;

        private ThingSpatialData spatialCache;


        public Building_Track()
        {
            innerContainer = new ThingOwner<Thing>(this, false);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            ConveyorManager = map.GetComponent<TrackManager>();
            UpdateSpatialCache();
            if (!respawningAfterLoad)
            {
                ConveyorManager.RegisterBelt(this);
            }
        }

        public override void DeSpawn(DestroyMode mode)
        {
            ConveyorManager.DeregisterBelt(this);
            base.DeSpawn(mode);
        }
        public ThingSpatialData GetSpatialData()
        {
            return spatialCache;
        }

        public void ClearConnections()
        {
            cachedNextBelt = null;
            cachedPrevBelt = null;
        }
        public virtual Building_Track SelectNextBeltForItem()
        {
            return this.cachedNextBelt;
        }
        public void UpdateSpatialCache()
        {
            spatialCache = new ThingSpatialData
            {
                Origin = this,
                NORTH = Map.thingGrid.ThingAt<Building_Track>(Position + IntVec3.North),
                EAST = Map.thingGrid.ThingAt<Building_Track>(Position + IntVec3.East),
                SOUTH = Map.thingGrid.ThingAt<Building_Track>(Position + IntVec3.South),
                WEST = Map.thingGrid.ThingAt<Building_Track>(Position + IntVec3.West)
            };
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            foreach (var heldItem in this.heldItems)
            {
                Vector3 direction = SelectNextBeltForItem() != null
                    ? (cachedNextBelt.DrawPos - this.DrawPos).normalized
                    : (cachedPrevBelt != null
                        ? (this.DrawPos - cachedPrevBelt.DrawPos).normalized
                        : Vector3.forward);

                Vector3 itemOffset = direction * (heldItem.Progress - 0.5f) * network.Speed;
                Vector3 drawPos = this.DrawPos + itemOffset;
                heldItem.Item.Graphic.Draw(drawPos.SetToAltitude(AltitudeLayer.MapDataOverlay), Rot4.South, heldItem.Item);
            }
        }

        public virtual bool TryAcceptItem(Thing item)
        {
            if (network == null || !network.CanAddToTrack(item))
            {
                return false;
            }

            if (item.Spawned)
            {
                item.DeSpawn();
            }

            if (innerContainer.TryAddOrTransfer(item, item.stackCount, true) != 0)
            {
                heldItems.Add(new HeldItem(item));
                return true;
            }
            return false;
        }

        public virtual void UpdateItemPositions()
        {
            foreach (var item in heldItems)
            {
                item.Progress += CellsPerTick;
            }
        }

        public virtual void ProcessItemHandoffs()
        {
            if (heldItems.Count == 0) 
                return;

            var itemsToTransfer = new List<HeldItem>();
            foreach (var item in heldItems)
            {
                if (item.Progress >= 1.0f)
                {
                    itemsToTransfer.Add(item);
                }
            }

            foreach (var item in itemsToTransfer)
            {
                if (SelectNextBeltForItem() != null && SelectNextBeltForItem().CanAcceptItem())
                {
                    heldItems.Remove(item);
                    innerContainer.Remove(item.Item);
                    cachedNextBelt.TryAcceptItem(item.Item);
                }
                else
                {
                    heldItems.Remove(item);
                    innerContainer.Remove(item.Item);
                    GenPlace.TryPlaceThing(item.Item, Position, Map, ThingPlaceMode.Near);
                }
            }
        }

        public virtual bool CanAcceptItem()
        {
            const float minSpacing = 0.3f;
            foreach (var item in heldItems)
            {
                if (item.Progress < minSpacing)
                    return false;
            }
            return true;
        }

        public virtual Building_Track GetPreferredNextBelt()
        {
            return null;
        }

        public void SetNetwork(TrackNetwork net)
        {
            this.network = net;
        }

        public IEnumerable<Building_Track> GetAdjacentBelts()
        {
            foreach (var cell in GenAdjFast.AdjacentCellsCardinal(this.Position))
            {
                var belt = this.Map.thingGrid.ThingAt<Building_Track>(cell);
                if (belt != null)
                {
                    yield return belt;
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new Command_Action()
            {
                defaultLabel = "Wdwd",
                defaultDesc = "efefefe",
                action = () =>
                {
                    Thing stackOfThing = ThingMaker.MakeThing(ThingDefOf.Steel, null);
                    stackOfThing.stackCount = stackOfThing.def.stackLimit;
                    TryAcceptItem(GenSpawn.Spawn(stackOfThing, this.Position.RandomAdjacentCell8Way(), this.Map));
                }
            };
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", new object[] { this });
            Scribe_Collections.Look(ref heldItems, "heldItems", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (heldItems == null) 
                    heldItems = new List<HeldItem>();
                if (innerContainer == null) 
                    innerContainer = new ThingOwner<Thing>(this, false);
            }
        }

        public override string GetInspectString()
        {
            return base.GetInspectString() + $"{(this.network != null ? "connected" : "disconnected")}";
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }
    }
}