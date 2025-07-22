using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimNet
{

    public class CompProperties_SensorPressurePlate : CompProperties_SignalSensor
    {
        public CompProperties_SensorPressurePlate()
        {
            compClass = typeof(Comp_SensorPressurePlate);
        }
    }

    public class Comp_SensorPressurePlate : Comp_SignalSensor
    {
        private bool wasOccupied = false;
        public PressurePlateTriggerMode triggerMode = PressurePlateTriggerMode.ON_STEP;
        public SensorTargetType targetType = SensorTargetType.ANY_PAWN;

        public override bool CanFormSignalGroup => true;

        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            CreatePort(SignalPortType.OUT, IntVec3.Zero, "OUT", 0);
        }

        public override bool IsSignalTerminal()
        {
            return true;
        }

        public override int GetConnectionPriority(Comp_SignalNode otherNode)
        {
            if (otherNode is Comp_SignalConduit conduit && conduit.ConnectedChildren.Count == 0)
            {
                return 30;
            }
            return base.GetConnectionPriority(otherNode);
        }

        public override void CheckSensor()
        {
            bool isOccupied = GetSensorValue() > 0;

            switch (triggerMode)
            {
                case PressurePlateTriggerMode.ON_STEP:
                    if (!wasOccupied && isOccupied)
                    {
                        TriggerSignal(GetSensorValue());
                    }
                    break;

                case PressurePlateTriggerMode.ON_RELEASE:
                    if (wasOccupied && !isOccupied)
                    {
                        TriggerSignal(1);
                    }
                    break;

                case PressurePlateTriggerMode.WHILE_PRESSED:
                    if (isOccupied)
                    {
                        TriggerSignal(GetSensorValue());
                    }
                    break;

                case PressurePlateTriggerMode.ON_CHANGE:
                    if (wasOccupied != isOccupied)
                    {
                        TriggerSignal(GetSensorValue());
                    }
                    break;
            }

            wasOccupied = isOccupied;
        }

        public override float GetSensorValue()
        {
            if (!this.parent.Spawned || this.parent.Map == null)
                return 0f;

            Pawn pawnOnCell = this.parent.Position.GetFirstPawn(this.parent.Map);

            if (pawnOnCell == null)
                return 0f;


            if (IsValid(targetType, pawnOnCell))
            {
                return 1f;
            }
            else
            {
                return 0;
            }
        }

        public override void SyncWithGroupNode(Comp_SignalNode ownerNode)
        {
            base.SyncWithGroupNode(ownerNode);
            if (ownerNode != this && ownerNode is Comp_SensorPressurePlate pressurePlate)
            {
                this.targetType = pressurePlate.targetType;
                this.triggerMode = pressurePlate.triggerMode;
            }
        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref wasOccupied, "wasOccupied", false);
            Scribe_Values.Look(ref triggerMode, "triggerMode", PressurePlateTriggerMode.ON_STEP);
            Scribe_Values.Look(ref targetType, "targetType", SensorTargetType.ANY_PAWN);
        }


        public override string CompInspectStringExtra()
        {
            string baseString = base.CompInspectStringExtra();
            baseString += $"\nTrigger: {triggerMode}";
            baseString += $"\nTarget: {targetType}";
            baseString += $"\nStatus: {(wasOccupied ? "Occupied" : "Clear")}";
            return baseString;
        }
    }

    public enum PressurePlateTriggerMode
    {
        ON_STEP,        // Triggers once when stepped on
        ON_RELEASE,     // Triggers once when released
        WHILE_PRESSED,  // Triggers continuously while pressed
        ON_CHANGE       // Triggers on any state change
    }

    public enum SensorTargetType
    {
        ANY_PAWN,
        COLONISTS,
        ENEMIES,
        ANIMALS,
        NON_COLONISTS
    }
}