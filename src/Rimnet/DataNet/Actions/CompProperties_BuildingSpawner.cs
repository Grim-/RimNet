using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

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

        public override bool CanFormSignalGroup => true;


        public bool HasThingStored => ThingOnTop != null;


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            TrySetThingOnTop();
        }

        public override void OnSignalRecieved(Signal signal, SignalPort receivingPort)
        {
            base.OnSignalRecieved(signal, receivingPort);

            this.SetState(signal.AsBool);
        }

        public override void OnGroupSignalReceived(Signal signal, SignalGroup signalGroup)
        {
            base.OnGroupSignalReceived(signal, signalGroup);
            this.SetState(signal.AsBool);
        }

        public void SetState(bool shouldBeSpawned)
        {
            if (this.IsSpawned == shouldBeSpawned)
            {
                return;
            }

            this.IsSpawned = shouldBeSpawned;

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
            TrySetThingOnTop();

            if (ThingOnTop != null && !ThingOnTop.Spawned)
            {
                GenSpawn.Spawn(ThingOnTop, parent.Position, parent.Map);
            }
        }


        private void TrySetThingOnTop()
        {
            if (!HasThingStored)
            {
                Thing thingOnThisCell = TryGetThingOnThisCell();
                if (thingOnThisCell != null)
                {
                    SetThing(thingOnThisCell);
                    ThingOnTop.DeSpawn(DestroyMode.Vanish);
                }
            }
        }

        private void OnToggledOff()
        {
            TrySetThingOnTop();

            if (ThingOnTop != null && ThingOnTop.Spawned)
            {
                ThingOnTop.DeSpawn(DestroyMode.Vanish);
            }
        }

        private void SetThing(Thing thing)
        {
            ThingOnTop = thing;
        }

        private Thing TryGetThingOnThisCell()
        {
            return parent.Position.GetThingList(parent.Map)
           .Where(x => x != parent && x.def.IsBuildingArtificial && x.def.BuildableByPlayer && x.Faction == Faction.OfPlayer).FirstOrDefault();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action()
            {
                defaultLabel = "WDWDw",
                defaultDesc = "Ffefefe",
                action = () =>
                {

                }
            };
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

        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra() + $"Stored thing {(ThingOnTop != null ? ThingOnTop : null)}";
        }
    }
}