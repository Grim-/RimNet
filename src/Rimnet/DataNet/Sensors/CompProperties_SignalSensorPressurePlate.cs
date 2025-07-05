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
        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            ConnectionPorts.Add(new SignalPort(this, SignalPortType.OUT, IntVec3.Zero));
        }

        public override bool IsSignalTerminal()
        {
            return false;
        }
        public override int GetConnectionPriority(Comp_SignalNode otherNode)
        {
            //perfer conduits with no children ie leaf conduits
            if (otherNode is Comp_SignalConduit conduit && conduit.ConnectedChildren.Count == 0)
            {
                return 30;
            }

            return base.GetConnectionPriority(otherNode);
        }
        protected override bool CheckSensor()
        {
            Pawn pawnOnCell = this.parent.Position.GetFirstPawn(this.parent.Map);
            return pawnOnCell != null;
        }
    }

}
