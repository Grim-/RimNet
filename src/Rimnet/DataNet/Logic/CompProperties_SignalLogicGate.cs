using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimNet
{
    public class CompProperties_SignalLogicGate : CompProperties_SignalGate
    {
        public CompProperties_SignalLogicGate()
        {
            compClass = typeof(Comp_SignalLogicGate);
        }
    }
    public class Comp_SignalLogicGate : Comp_SignalGate
    {
        public LogicGateType GateType = LogicGateType.Or;
        private Dictionary<SignalPort, float> portValues = new Dictionary<SignalPort, float>();

        public override bool CanSendSignal => EvaluateGate() >= 1f;

        protected override void SetupDefaultPorts()
        {
            ClearPorts();
            // Output port
            CreatePort(SignalPortType.OUT, IntVec3.Zero, "OUT", 0, false, true);
            CreatePort(SignalPortType.IN, IntVec3.Zero, "IN", 0, false, true);
            SetUpLogicPOrts();
            portValues.Clear();
            foreach (var port in AllInPorts)
            {
                portValues[port] = 0f;
            }
        }


        protected void SetUpLogicPOrts()
        {
            if (GateType == LogicGateType.Not)
            {
                CreatePort(SignalPortType.IN, IntVec3.Zero, "Input A", 0, true, false);
            }
            else
            {
                CreatePort(SignalPortType.IN, IntVec3.Zero, "Input A", 1, true, false);
                CreatePort(SignalPortType.IN, IntVec3.Zero, "Input B", 2, true, false);
            }
        }



        public float GetPortValue(SignalPort port)
        {
            return portValues.ContainsKey(port) ? portValues[port] : 0f;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            // Gate type selector
            yield return new Command_Action
            {
                defaultLabel = $"Gate: {GateType}",
                defaultDesc = "Change logic gate type",
                icon = TexCommand.RearmTrap,
                action = () =>
                {
                    var options = new List<FloatMenuOption>();
                    foreach (LogicGateType type in System.Enum.GetValues(typeof(LogicGateType)))
                    {
                        var localType = type;
                        options.Add(new FloatMenuOption(localType.ToString(), () =>
                        {
                            GateType = localType;
                            SetupDefaultPorts();
                            var signalManager = parent.Map?.GetComponent<SignalManager>();
                            signalManager?.MarkNetworksDirty();
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            };
        }

        public override void OnSignalRecieved(Signal signal, SignalPort receivingPort)
        {
            base.OnSignalRecieved(signal, receivingPort);

            if (receivingPort == null || receivingPort.Type != SignalPortType.IN)
                return;

            portValues[receivingPort] = signal.Value;
            float outputValue = EvaluateGate();

            if (outputValue >= 1f)
            {
                SendSignal(new Signal(this, outputValue));
            }
            
        }

        private float EvaluateGate()
        {
            var inputs = AllInPorts
                .Where(p => portValues.ContainsKey(p))
                .Select(p => portValues[p])
                .ToList();

            if (!inputs.Any()) 
                return 0f;

            switch (GateType)
            {
                case LogicGateType.And:
                    return inputs.Min();
                case LogicGateType.Or:
                    return inputs.Max();
                case LogicGateType.Not:
                    return 1f - inputs.FirstOrDefault();
                case LogicGateType.Xor:
                    int countOnes = inputs.Count(v => v >= 0.5f);
                    return (countOnes % 2 == 1) ? 1f : 0f;
                case LogicGateType.Nand:
                    return 1f - inputs.Min();
                case LogicGateType.Nor:
                    return 1f - inputs.Max();
                default:
                    return 0f;
            }
        }

        private List<SignalPort> Keys = new List<SignalPort>();
        private List<float> Values = new List<float>();

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref GateType, "GateType", LogicGateType.Or);
            Scribe_Collections.Look(ref portValues, "portValues", LookMode.Deep, LookMode.Value, ref Keys, ref Values);
        }
    }
}
