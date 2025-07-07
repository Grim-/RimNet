using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimNet
{

    public class CompProperties_SignalSensorPressurePlate : CompProperties_SignalSensor
    {
        public CompProperties_SignalSensorPressurePlate()
        {
            compClass = typeof(Comp_SignalSensorPressurePlate);
        }
    }

    public class Comp_SignalSensorPressurePlate : Comp_SignalSensor
    {
        private bool wasOccupied = false;
        private PressurePlateTriggerMode triggerMode = PressurePlateTriggerMode.ON_STEP;
        private PressurePlateTargetType targetType = PressurePlateTargetType.ANY_PAWN;

        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            ConnectionPorts.Add(new SignalPort(this, SignalPortType.OUT, IntVec3.Zero));
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

        protected override float GetSensorValue()
        {
            if (!this.parent.Spawned || this.parent.Map == null)
                return 0f;

            Pawn pawnOnCell = this.parent.Position.GetFirstPawn(this.parent.Map);

            if (pawnOnCell == null)
                return 0f;

            switch (targetType)
            {
                case PressurePlateTargetType.ANY_PAWN:
                    return 1f;

                case PressurePlateTargetType.COLONISTS:
                    return pawnOnCell.IsColonist ? 1f : 0f;

                case PressurePlateTargetType.ENEMIES:
                    return pawnOnCell.HostileTo(Faction.OfPlayer) ? 1f : 0f;

                case PressurePlateTargetType.ANIMALS:
                    return pawnOnCell.RaceProps.Animal ? 1f : 0f;

                case PressurePlateTargetType.NON_COLONISTS:
                    return !pawnOnCell.IsColonist ? 1f : 0f;

                default:
                    return 0f;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref wasOccupied, "wasOccupied", false);
            Scribe_Values.Look(ref triggerMode, "triggerMode", PressurePlateTriggerMode.ON_STEP);
            Scribe_Values.Look(ref targetType, "targetType", PressurePlateTargetType.ANY_PAWN);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            yield return new Command_Action
            {
                defaultLabel = $"Trigger: {triggerMode}",
                defaultDesc = "Change when the pressure plate triggers",
                action = () =>
                {
                    var options = new List<FloatMenuOption>();
                    foreach (PressurePlateTriggerMode mode in System.Enum.GetValues(typeof(PressurePlateTriggerMode)))
                    {
                        var capturedMode = mode;
                        options.Add(new FloatMenuOption(capturedMode.ToString(), () => triggerMode = capturedMode));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };

            yield return new Command_Action
            {
                defaultLabel = $"Target: {targetType}",
                defaultDesc = "Change what triggers the pressure plate",
                action = () =>
                {
                    var options = new List<FloatMenuOption>();
                    foreach (PressurePlateTargetType type in System.Enum.GetValues(typeof(PressurePlateTargetType)))
                    {
                        var capturedType = type;
                        options.Add(new FloatMenuOption(capturedType.ToString(), () => targetType = capturedType));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };
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

    public enum PressurePlateTargetType
    {
        ANY_PAWN,
        COLONISTS,
        ENEMIES,
        ANIMALS,
        NON_COLONISTS
    }
}