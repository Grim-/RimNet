using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimNet
{
    public class Drone : Pawn
    {
        public CompDroneController controller;
        private int ticksSinceLastAttack = 0;

        public bool ShouldReturn = false;

        public DroneState DroneState = DroneState.ACTIVE;

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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksSinceLastAttack, "ticksSinceLastAttack", 0);
        }
    }
}