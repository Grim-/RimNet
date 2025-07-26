using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class CompProperties_InstallDroneComp : CompProperties
    {
        public PawnKindDef dronePawnKind;

        public CompProperties_InstallDroneComp()
        {
            compClass = typeof(CompDroneInstall);
        }
    }

    public class CompDroneInstall : ThingComp, IThingHolder
    {
        public CompProperties_InstallDroneComp Props => (CompProperties_InstallDroneComp)props;
        private ThingOwner<Drone> storedDrones;

        public bool HasDroneStored => storedDrones != null && storedDrones.Any;

        public Drone StoredDrone => storedDrones.InnerListForReading.First();

        public CompDroneInstall()
        {
            storedDrones = new ThingOwner<Drone>(this, true, LookMode.Deep, false);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (HasDroneStored)
            {
                SyncKitHealthWithDrone();
            }
            else
            {
                Pawn dronePawn = PawnGenerator.GeneratePawn(Props.dronePawnKind);
                StoreDroneInstance((Drone)dronePawn);
            }
        }

        public void SyncKitHealthWithDrone()
        {
            if (HasDroneStored)
                parent.HitPoints = Mathf.RoundToInt(StoredDrone.health.summaryHealth.SummaryHealthPercent * parent.MaxHitPoints);
        }

        public void StoreDroneInstance(Drone drone)
        {
            if (storedDrones.Any)
            {
                Log.Message($"Cannot store drone instance, one exists");
                return;
            }
               

            if (drone.Spawned)
                drone.DeSpawn();
            storedDrones.TryAdd(drone, true);
        }

        public Drone RemoveStoredDrone()
        {
            if (!storedDrones.Any)
            {
                return null;
            }

            Drone drone = storedDrones.InnerListForReading.First();
            storedDrones.Remove(drone);
            drone.SetStoredInKit(null);
            return drone;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            yield return new FloatMenuOption($"Install to..", () =>
            {
                Find.Targeter.BeginTargeting(TargetingParameters.ForBuilding(), (LocalTargetInfo target) =>
                {
                    if (target.HasThing && target.Thing.TryGetComp(out CompDroneController droneController))
                    {
                        Job job = JobMaker.MakeJob(RimNetDefOf.InstallDrone, this.parent, target.Thing);
                        job.count = 1;
                        selPawn.jobs.TryTakeOrderedJob(job);
                    }
                });
            });
        }

        public override string CompInspectStringExtra()
        {
            string baseString = base.CompInspectStringExtra();

            if (Prefs.DevMode && storedDrones != null && storedDrones.Count > 0)
            {
                baseString += $"\r\n DRONE: {storedDrones.InnerListForReading.First().Label}";
            }

            return baseString;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return storedDrones;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref storedDrones, "storedDrones", this);
        }
    }
}