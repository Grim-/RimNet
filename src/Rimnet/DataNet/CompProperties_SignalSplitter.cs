using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public class CompProperties_SignalSplitter : CompProperties_SignalNode
    {
        public ThingDef conduitDef;

        public CompProperties_SignalSplitter()
        {
            compClass = typeof(Comp_SignalSplitter);
        }
    }

    public class Comp_SignalSplitter : Comp_SignalNode
    {
        private HashSet<IntVec3> enabledDirections = new HashSet<IntVec3>();
        private CompProperties_SignalSplitter Props => (CompProperties_SignalSplitter)props;


        protected Comp_SignalNode RootConnector = null;
        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            ConnectionPorts.Add(new SignalPort(SignalPortType.IN, IntVec3.Zero));
        }

        public override bool MakeConnection(SignalPort myPort, Comp_SignalNode otherNode, SignalPort otherPort, out string reason)
        {
            bool result = base.MakeConnection(myPort, otherNode, otherPort, out reason);

            if (result && RootConnector == null)
            {
                IntVec3 directionToOther = otherNode.parent.Position - this.parent.Position;
                RootConnector = otherNode;
                ToggleDirection(directionToOther.ClampMagnitude(1), RootConnector);
            }

            return result;
        }

        private void ToggleDirection(IntVec3 direction, Comp_SignalNode targetNode)
        {
            if (enabledDirections.Contains(direction))
            {
                DisableDirection(direction, targetNode);
            }
            else
            {
                EnableDirection(direction, targetNode);
            }
        }
        private bool CanConnectAt(IntVec3 cell)
        {
            if (!cell.InBounds(parent.Map))
                return false;

            if (!cell.Standable(parent.Map))
                return false;

            Comp_SignalNode existingConduit = GetNodeAt(cell);
            if (existingConduit == null)
            {
                Messages.Message($"Cannot connect no existing signal node found at {cell}", MessageTypeDefOf.NegativeEvent);
                return false;
            }
            return true;
        }
        private void EnableDirection(IntVec3 direction, Comp_SignalNode targetNode)
        {
            enabledDirections.Add(direction);

            if (targetNode != null)
            {
                SignalPort outPort = CreateOutputPort(direction);
                SignalPort targetInPort = FindAvailableInputPort(targetNode);

                if (targetInPort != null)
                {
                    if (!VerifyConnection(outPort, targetNode, targetInPort, out string reason, true))
                    {
                        Messages.Message($"Cannot verify connection! : {reason}", MessageTypeDefOf.NegativeEvent);
                        return;
                    }

                    if (!MakeConnection(outPort, targetNode, targetInPort, out reason))
                    {
                        Messages.Message($"Cannot make connection! : {reason}", MessageTypeDefOf.NegativeEvent);
                        return;
                    }
                  
                }
                else
                {
                    ConnectionPorts.Remove(outPort);
                }
            }
        }
        private void DisableDirection(IntVec3 direction, Comp_SignalNode targetNode)
        {
            SignalPort outPort = GetOutputPortForDirection(targetNode);
            if (outPort != null)
            {
                DisconnectPort(outPort);
                ConnectionPorts.Remove(outPort);
                DisconnectChild(targetNode);
                targetNode.DisconnectParent(this);
            }
            enabledDirections.Remove(direction);
        }
        private SignalPort CreateOutputPort(IntVec3 direction)
        {
            SignalPort outPort = new SignalPort(SignalPortType.OUT, direction);
            ConnectionPorts.Add(outPort);
            return outPort;
        }
        private SignalPort FindAvailableInputPort(Comp_SignalNode targetNode)
        {
            if (targetNode.TryGetConnectionPort(SignalPortType.IN, out var foundPart))
            {
                return foundPart;
            }
            return null;
        }
        private SignalPort GetOutputPortForDirection(Comp_SignalNode direction)
        {
            foreach (SignalPort port in ConnectionPorts)
            {
                if (port.Type == SignalPortType.OUT && port.ConnectedNode == direction)
                    return port;
            }
            return null;
        }
        private void DisconnectPort(SignalPort port)
        {
            if (port.ConnectedNode != null)
            {
                Comp_SignalNode connectedNode = port.ConnectedNode;
                SignalPort connectedPort = FindConnectedPort(connectedNode);

                if (connectedPort.HasConnectTarget)
                {
                    port.Disconnect();
                }
            }
        }
        private SignalPort FindConnectedPort(Comp_SignalNode node)
        {
            foreach (SignalPort port in node.ConnectionPorts)
            {
                if (port.ConnectedNode == this)
                    return port;
            }
            return null;
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (IntVec3 dir in GenAdjFast.AdjacentCellsCardinal(parent.Position))
            {
                IntVec3 direction = dir - parent.Position;
                Comp_SignalNode targetNode = GetNodeInDirection(direction);

                if (targetNode == null)
                    continue;

                bool isEnabled = enabledDirections.Contains(direction);

                yield return new Command_Toggle
                {
                    defaultLabel = $"Split {GetDirectionLabel(direction)}",
                    defaultDesc = $"Toggle signal output to the {GetDirectionLabel(direction).ToString().ToLower()}",
                    icon = TexCommand.Attack,
                    isActive = () => isEnabled,
                    toggleAction = () => ToggleDirection(direction, targetNode),
                    Disabled = !CanConnectAt(parent.Position + direction),
                    disabledReason = "Space blocked"
                };
            }
        }
        private string GetDirectionLabel(IntVec3 direction)
        {
            if (direction == IntVec3.North) return "North";
            if (direction == IntVec3.South) return "South";
            if (direction == IntVec3.East) return "East";
            if (direction == IntVec3.West) return "West";
            return direction.ToString();
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref enabledDirections, "enabledDirections", LookMode.Value);
            if (enabledDirections == null)
                enabledDirections = new HashSet<IntVec3>();
        }
    }
}