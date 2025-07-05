using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public class CompProperties_BuildingSpawner : CompProperties_SignalReciever
    {
        public CompProperties_BuildingSpawner()
        {
            compClass = typeof(Comp_Signal_BuildingSpawner);
        }
    }

    public class Comp_Signal_BuildingSpawner : Comp_SignalReciever, ITileGroupedSignalNode
    {
        private CompProperties_BuildingSpawner Props => (CompProperties_BuildingSpawner)props;
        private Thing ThingOnTop;
        private bool IsSpawned = true;

        private SignalNodeTileGroup tileGroup;
        public SignalNodeTileGroup TileGroup => tileGroup;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (tileGroup == null)
                tileGroup = new SignalNodeTileGroup(this);

            if (respawningAfterLoad)
            {
                if (ThingOnTop != null && !ThingOnTop.Spawned)
                {
                    ThingOnTop = null;
                }
            }

            tileGroup.ConnectToAdjacentTiles();

            Thing thingOnThisCell = TryGetThingOnThisCell();

            if (thingOnThisCell != null)
            {
                SetThing(thingOnThisCell);
                ThingOnTop.DeSpawn(DestroyMode.Vanish);
            }
        }
        //public override bool IsSignalTerminal()
        //{
        //    return false;
        //}

        //public override int GetConnectionPriority(Comp_SignalNode otherNode)
        //{
        //    //perfer conduits with no children ie leaf conduits
        //    if (otherNode is Comp_SignalConduit conduit && conduit.ConnectedChildren.Count == 0)
        //    {
        //        return 30;
        //    }

        //    return base.GetConnectionPriority(otherNode);
        //}

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            tileGroup?.DisconnectFromAdjacentTiles();
        }

        public override void OnSignalRecieved(Signal signal)
        {
            base.OnSignalRecieved(signal);
            tileGroup?.PropagateSignalToGroup(signal, (node, sig) => {
                if (node is Comp_Signal_BuildingSpawner spawner)
                {
                    spawner.Toggle();
                }
            });

            foreach (var child in ConnectedChildren)
            {
                child.OnSignalRecieved(signal);
            }
        }

        public void Toggle()
        {
            this.IsSpawned = !this.IsSpawned;

            if (IsSpawned)
            {
                OnToggledOn();
            }
            else
            {
                OnToggledOff();
            }
        }


        private void OnToggledOn()
        {
            if (ThingOnTop != null && !ThingOnTop.Spawned)
            {
                GenSpawn.Spawn(ThingOnTop, parent.Position, parent.Map);
            }
        }

        private void OnToggledOff()
        {
            if (ThingOnTop != null && ThingOnTop.Spawned)
            {
                ThingOnTop.DeSpawn(DestroyMode.Vanish);
            }
            else if (ThingOnTop == null)
            {
                Thing thingOnThisCell = TryGetThingOnThisCell();

                if (thingOnThisCell != null)
                {
                    SetThing(thingOnThisCell);
                    ThingOnTop.DeSpawn(DestroyMode.Vanish);
                }
            }
        }

        private void SetThing(Thing thing)
        {
            ThingOnTop = thing;
        }

        private Thing TryGetThingOnThisCell()
        {
            return parent.Position.GetThingList(parent.Map)
           .Where(x => x != parent && x.def.IsBuildingArtificial).FirstOrDefault();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref ThingOnTop, "spawnedThing");
            Scribe_Values.Look(ref IsSpawned, "isActiveFromSignal", false);
            Scribe_Deep.Look(ref tileGroup, "tileGroup", this);
        }
    }
}