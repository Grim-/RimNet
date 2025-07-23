using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public class CompProperties_DroneController : CompProperties
    {
        public PawnKindDef droneDef;

        public CompProperties_DroneController()
        {
            compClass = typeof(CompDroneController);
        }
    }

    public class CompDroneController : ThingComp, IThingHolder
    {
        public List<Drone> activeDrones = new List<Drone>();
        private ThingOwner<Drone> storedDrones;

        public List<Drone> StoredDrones => storedDrones.InnerListForReading;
        public int maxDrones = 5;
        public float costPerDronePerTick = 0.01f;

        public HashSet<Drone> AllDrones
        {
            get
            {
                HashSet<Drone> drones = new HashSet<Drone>();
                drones.AddRange(activeDrones);
                drones.AddRange(storedDrones);
                return drones;
            }
        }


        public float TotalMaintenaceCostPerTick => activeDrones.Count * costPerDronePerTick;

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
                PowerTrader.PowerOutput = -(TotalMaintenaceCostPerTick * 60);

                activeDrones.RemoveAll(d => d == null || d.Destroyed);
            }
        }


        public void OnEnabled()
        {
            if (_IsActive)
            {
                return;
            }

            if (PowerTrader != null)
            {
                PowerTrader.PowerOn = true;
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
            if (PowerTrader != null)
            {
                PowerTrader.PowerOn = false;
            }
            _IsActive = false;
            foreach (var item in activeDrones)
            {
                item.TryStartReturn();
                item.DroneState = DroneState.RETURNING;
            }


        }


        public void DeployDrones()
        {
            while (activeDrones.Count < maxDrones)
            {
                if (!TryDeployDrone())
                {
                    break;
                }
            }

        }

        public bool TryDeployDrone()
        {
            if (activeDrones.Count >= maxDrones)
            {
                return false;
            }

            if (!storedDrones.InnerListForReading.Any())
            {
                return false;
            }

            IntVec3 spawnPos = CellFinder.RandomClosewalkCellNear(parent.Position, parent.Map, 3);
            if (!spawnPos.IsValid)
            {
                return false;
            }

            Drone droneToDeploy = storedDrones.InnerListForReading.First();
            storedDrones.Remove(droneToDeploy);

            GenSpawn.Spawn(droneToDeploy, spawnPos, parent.Map);
            Log.Message($"Deployed {droneToDeploy.Label}");
            droneToDeploy.DroneState = DroneState.ACTIVE;
            activeDrones.Add(droneToDeploy);
            return true;
        }

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

            if (!storedDrones.TryAdd(drone, false))
            {

            }

                       
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
                activeDrones.RemoveAll(d => d == null);
            }
        }
    }

    public enum DroneState
    {
        ACTIVE,
        RETURNING
    }
}