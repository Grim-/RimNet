using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class CompProperties_SignalSplitter : CompProperties_SignalNode
    {
        public CompProperties_SignalSplitter()
        {
            compClass = typeof(Comp_SignalSplitter);
        }
    }

    public class Comp_SignalSplitter : Comp_SignalTransmitter
    {
        private HashSet<IntVec3> enabledDirections = new HashSet<IntVec3>();

        private void ToggleDirection(SignalPort targetNode)
        {
            targetNode.Toggle();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var port in AllPorts)
            {
                // Ensure there is a connected node to avoid errors
                if (port == null || port.ConnectedNode == null || port.OwnerNode == null) 
                    continue;

                IntVec3 direction = port.OwnerNode.parent.Position - port.ConnectedNode.parent.Position;
                bool isEnabled = port.Enabled;

                yield return new Command_Toggle
                {
                    defaultLabel = $"Split {GetDirectionLabel(direction)}",
                    defaultDesc = $"Toggle signal output to the {GetDirectionLabel(direction).ToLower()}",
                    icon = TexCommand.Attack,
                    isActive = () => isEnabled,
                    toggleAction = () => ToggleDirection(port)
                };
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref enabledDirections, "enabledDirections", LookMode.Value);
            if (enabledDirections == null)
            {
                enabledDirections = new HashSet<IntVec3>();
            }
        }
    }
}