using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimNet
{
    public class CompProperties_DroneController : CompProperties
    {
        public CompProperties_DroneController()
        {
            compClass = typeof(CompDroneController);
        }
    }

    public class CompDroneController : ThingComp, IThingHolder
    {
        protected List<Drone> activeDrones = new List<Drone>();

        public List<Drone> ActiveDrones => activeDrones.ToList();
        private ThingOwner<Drone> storedDrones;

        public List<Drone> StoredDrones => storedDrones.InnerListForReading;

        public float TotalMaintenaceCostPerTick
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < activeDrones.Count; i++)
                {
                    var comp = activeDrones[i].TryGetComp<CompDrone>();
                    if (comp != null && activeDrones[i].IsEnabled)
                        total += comp.Props.powerCost;
                }
                return total;
            }
        }

        protected bool _IsActive = false;
        public bool IsActive => _IsActive;
        public bool IsPowered
        {
            get
            {
                if (PowerTrader != null)
                {
                    return PowerTrader.PowerOn;
                }

                return true;
            }
        }

        public CompPowerTrader PowerTrader => this.parent.GetComp<CompPowerTrader>();
        public CompProperties_DroneController Props => (CompProperties_DroneController)props;

        public CompDroneController()
        {
            storedDrones = new ThingOwner<Drone>(this, false, LookMode.Deep);
        }

        public override void CompTick()
        {
            base.CompTick();

            if (parent.IsHashIntervalTick(60))
            {
                if (!IsPowered && _IsActive)
                {
                    OnDisabled();
                }

                PowerTrader.PowerOutput = -(TotalMaintenaceCostPerTick * 60 + PowerTrader.Props.idlePowerDraw);
                activeDrones.RemoveAll(d => d == null || d.Destroyed);
            }
        }


        public void OnEnabled()
        {
            if (_IsActive || !IsPowered)
            {
                return;
            }

            _IsActive = true;
            DeployDrones();
        }

        public void OnDisabled()
        {
            if (!_IsActive)
            {
                return;
            }

            _IsActive = false;
            foreach (var item in activeDrones)
            {
                item.TryStartReturn();
                item.DroneState = DroneState.RETURNING;
            }


        }


        public void RemoveActiveDrone(Drone drone)
        {
            if (drone != null && activeDrones.Contains(drone))
            {
                activeDrones.Remove(drone);
            }
        }

        /// <summary>
        /// deploy all stored drones
        /// </summary>
        public void DeployDrones()
        {
            var dronesToDeploy = storedDrones.InnerListForReading.ToList();
            for (int i = 0; i < dronesToDeploy.Count; i++)
            {
                TryDeployDrone(dronesToDeploy[i]);
            }
        }

        /// <summary>
        /// eject all stored drones
        /// </summary>
        public void EjectDrones()
        {
            var dronesToEject = storedDrones.InnerListForReading.ToList();
            for (int i = 0; i < dronesToEject.Count; i++)
            {
                EjectDrone(dronesToEject[i]);
            }
        }    
        
        /// <summary>                
        /// return all active drones             
        /// </summary>
        public void ReturnDrones()
        {
            for (int i = 0; i < activeDrones.ToList().Count; i++)
            {
                activeDrones[i].TryStartReturn();
            }
        }

        /// <summary>
        /// deploy a stored drone
        /// </summary>
        /// <param name="drone"></param>
        /// <returns></returns>
        public bool TryDeployDrone(Drone drone)
        {
            if (!storedDrones.InnerListForReading.Contains(drone))
            {
                return false;
            }


            if (!TryGetNearbyEmptyCell(out IntVec3 emptyCell))
            {
                return false;
            }

            Drone droneToDeploy = drone;
            storedDrones.Remove(droneToDeploy);

            if (droneToDeploy.Faction != this.parent.Faction)
                droneToDeploy.SetFaction(this.parent.Faction);


            droneToDeploy.DroneState = DroneState.ACTIVE;
            GenSpawn.Spawn(droneToDeploy, emptyCell, parent.Map);
            Log.Message($"Deployed {droneToDeploy.Label}");
            activeDrones.Add(droneToDeploy);
            return true;
        }


        public bool TryGetNearbyEmptyCell(out IntVec3 cell, int maxAttempts = 50, int radius = 5)
        {
            cell = IntVec3.Invalid;
            for (int i = 0; i < maxAttempts; i++)
            {
                IntVec3 spawnPos = CellFinder.RandomClosewalkCellNear(parent.Position, parent.Map, radius, (IntVec3 chosenCell) =>
                {
                    return chosenCell.IsValid && chosenCell.Standable(parent.Map);
                });
                if (spawnPos.IsValid)
                {
                    cell = spawnPos;
                    return true;
                }
            }
            return false;
        }

        //store a drone in the controller
        public void StoreDrone(Drone drone)
        {
            if (drone == null)
                return;

            Log.Message($"Storing {drone.Label}");

            if (activeDrones.Contains(drone))
            {
                activeDrones.Remove(drone);
            }

            if (drone.Spawned)
            {
                drone.DeSpawn();
            }

            if (storedDrones.TryAdd(drone, false))
            {
                drone.SetController(this);
            }                   
        }

        public ThingWithComps EjectDrone(Drone drone)
        {
            Log.Message($"Ejecting {drone.Label}");

            if (!storedDrones.Contains(drone))
            {
                Log.Error($"Failed to eject drone, not in stored drones");
                return null;
            }

            storedDrones.Remove(drone);
            drone.SetController(null);
            ThingWithComps withComps = drone.CreateKit();

            if (withComps == null)
            {
                Log.Error($"Failed to create kit from drone");
                return null;
            }

            if (!TryGetNearbyEmptyCell(out IntVec3 emptyCell))
            {
                Log.Error($"Failed to find spawn position for ejecting drone");
                return null;
            }

            withComps.HitPoints = Mathf.RoundToInt(Mathf.Lerp(0, withComps.MaxHitPoints, drone.health.summaryHealth.SummaryHealthPercent));
            //withComps.SetFaction(this.parent.Faction);
            GenSpawn.Spawn(withComps, emptyCell, this.parent.Map);
            return withComps;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            yield return new Command_Action()
            {
                defaultLabel = IsActive ? "Return" : "Deploy",
                defaultDesc = IsActive ? "Return drones" : "Deploy drones",
                action = () =>
                {
                    if (IsActive)
                    {
                        OnDisabled();
                    }
                    else OnEnabled();
                }
            };
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
            Scribe_Collections.Look(ref activeDrones, "activeDrones", LookMode.Reference);
            Scribe_Deep.Look(ref storedDrones, "storedDrones", this);
            Scribe_Values.Look(ref _IsActive, "_IsActive");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (activeDrones == null) activeDrones = new List<Drone>();
                activeDrones.RemoveAll(d => d == null || d.Destroyed);
                if (storedDrones == null) storedDrones = new ThingOwner<Drone>(this, false, LookMode.Deep);
            }
        }
    }

    public enum DroneState
    {
        ACTIVE,
        RETURNING
    }
}