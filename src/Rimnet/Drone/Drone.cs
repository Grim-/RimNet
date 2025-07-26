using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimNet
{


    public class Drone : Pawn
    {
        public bool IsEnabled = true;

        public bool ShouldReturn = false;
        public DroneState DroneState = DroneState.ACTIVE;


        protected CompDroneController controller;
        protected ThingWithComps parentControllerThing;

        public CompDrone DroneDisassemble => this.GetComp<CompDrone>();
        public bool IsDeployed => this.Spawned && DroneState == DroneState.ACTIVE;
        public CompDroneInstall StoredInKit { get; private set; }



        public CompDronePowerSource PowerSource => this.GetComp<CompDronePowerSource>();


        protected int CurrentCostTick = 0;

        public bool IsPowered
        {
            get
            {
                if (PowerSource == null || PowerSource.Has(DroneDisassemble.Props.powerCost))
                {
                    return true;
                }

                return false;
            }
        }

        public CompDroneController Controller => controller;

        public void SetStoredInKit(CompDroneInstall kit)
        {
            StoredInKit = kit;
        }

        public ThingWithComps CreateKit()
        {
            ThingWithComps thing = (ThingWithComps)ThingMaker.MakeThing(DroneDisassemble.Props.disassembledKind);
            if (thing == null)
            {
                Log.Error($"Failed to create Drone Kit");
                return null;
            }

            CompDroneInstall droneInstall = thing.GetComp<CompDroneInstall>();

            if (droneInstall == null)
            {
                Log.Error($"Failed to find CompDroneInstall");
                return null;
            }

            SetController(null);
            droneInstall.StoreDroneInstance(this);
            SetStoredInKit(droneInstall);
            thing.HitPoints = Mathf.RoundToInt(Mathf.Lerp(1, thing.MaxHitPoints, health.summaryHealth.SummaryHealthPercent));
            return thing;
        }

        public void SetController(CompDroneController droneController)
        {
            if (droneController == null)
            {
                DisconnectFromActiveController();
                this.controller = null;
                this.parentControllerThing = null;
            }
            else
            {
                this.controller = droneController;
                this.parentControllerThing = droneController.parent;
            }
        }

        protected override void Tick()
        {
            base.Tick();

            CurrentCostTick++;

            if (PowerSource != null)
            {
                float costPerTick = PowerSource.Props.chargeCost / 60f;

                if (!PowerSource.Has(costPerTick))
                {
                    if (IsEnabled)
                        this.Disable();
                }
                else PowerSource.Consume(costPerTick);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            DisconnectFromActiveController();
        }


        public void DisconnectFromActiveController()
        {
            if (this.controller != null)
            {
                this.controller.RemoveActiveDrone(this);
            }
        }

        public void Enable()
        {
            this.IsEnabled = true;
            this.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        public void Disable()
        {
            this.IsEnabled = false;
            this.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        public bool ShouldShutDown()
        {
            return controller == null || controller.parent.Destroyed || !controller.IsPowered || !controller.IsActive;
        }

        public void TryStartReturn()
        {
            if (controller == null || controller.parent.DestroyedOrNull())
            {
                Log.Error("Failed to return: no valid controller");
                return;
            }

            if (DroneState == DroneState.RETURNING) 
                return;

            if (!this.CanReach(controller.parent, PathEndMode.Touch, Danger.Deadly))
            {
                Messages.Message("DroneCannotReachController", MessageTypeDefOf.RejectInput, false);
                return;
            }

            var returnJob = JobMaker.MakeJob(RimNetDefOf.StoreSelfInController, controller.parent);
            DroneState = DroneState.RETURNING;
            jobs.StartJob(returnJob, JobCondition.InterruptForced);
        }

        public void TryStartPackUp(Pawn pawn)
        {
            if (pawn.DestroyedOrNull())
            {
                return;
            }

            Job packUpJob = JobMaker.MakeJob(RimNetDefOf.PackUpDrone, this);
            pawn.jobs?.StartJob(packUpJob, JobCondition.InterruptForced);
        }


        public override TipSignal GetTooltip()
        {
            return base.GetTooltip();
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            yield return new FloatMenuOption("Pack up drone", () =>
            {
                TryStartPackUp(selPawn);
            });
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {

            foreach (var item in base.GetGizmos())
            {
                yield return item;
            }

            if (controller != null)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Return",
                    defaultDesc = "Return to the assigned controller",
                    icon = TexUI.ArrowTexLeft,
                    action = () =>
                    {
                        TryStartReturn();
                    }
                };
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref IsEnabled, "IsEnabled", true);
            Scribe_Values.Look(ref ShouldReturn, "ShouldReturn", false);
            Scribe_Values.Look(ref CurrentCostTick, "CurrentCostTick");
            Scribe_Values.Look(ref DroneState, "DroneState", DroneState.ACTIVE);

            Scribe_References.Look(ref parentControllerThing, "parentControllerThing");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (parentControllerThing != null && !parentControllerThing.DestroyedOrNull())
                {
                    controller = parentControllerThing.GetComp<CompDroneController>();
                }
            }
        }
    }


}