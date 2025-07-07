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

    public class Comp_Signal_BuildingSpawner : Comp_SignalReciever
    {
        private CompProperties_BuildingSpawner Props => (CompProperties_BuildingSpawner)props;
        private Thing ThingOnTop;
        private bool IsSpawned = true;

        public override bool CanFormTileGroup => true;

        public override void OnSignalRecieved(Signal signal, SignalPort receivingPort)
        {
            base.OnSignalRecieved(signal, receivingPort);

            this.Toggle();
        }

        //protected override void OnGroupSignalReceived(Signal signal)
        //{
        //    base.OnGroupSignalReceived(signal);
        //    this.Toggle();
        //}

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

            Scribe_Values.Look(ref IsSpawned, "isActiveFromSignal", false);


            if (IsSpawned)
            {
                Scribe_References.Look(ref ThingOnTop, "spawnedThing");
            }
            else
            {
                Scribe_Deep.Look(ref ThingOnTop, "spawnedThing");
            }

        }
    }
}