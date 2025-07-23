using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class Drone : Pawn
    {
        public CompDroneController controller;

        protected ThingWithComps parentControllerThing;
        private int ticksSinceLastAttack = 0;

        public bool ShouldReturn = false;

        public DroneState DroneState = DroneState.ACTIVE;


        public void SetController(CompDroneController droneController)
        {
            this.controller = droneController;
            this.parentControllerThing = droneController.parent;
        }

        public bool ShouldShutDown()
        {
            return controller == null || controller.parent.Destroyed || !controller.IsPowered;
        }

        public void TryStartReturn()
        {
            if (controller == null)
            {
                return;
            }
            Job returnJob = JobMaker.MakeJob(RimNetDefOf.StoreSelfInController, controller.parent);
            this.DroneState = DroneState.RETURNING;
            this.jobs.StartJob(returnJob, JobCondition.InterruptForced);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (controller != null)
            {
                if (DroneState == DroneState.ACTIVE)
                {
                    yield return new Command_Action()
                    {
                        defaultLabel = "Return",
                        defaultDesc = "Return to the assigned controller",
                        action = () =>
                        {
                            TryStartReturn();
                        }
                    };
                }


            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.Look(ref parentControllerThing, "parentControllerThing");
            Scribe_Values.Look(ref ticksSinceLastAttack, "ticksSinceLastAttack", 0);

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