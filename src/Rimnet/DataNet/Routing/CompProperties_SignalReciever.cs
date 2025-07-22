using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace RimNet
{
    public class CompProperties_SignalReciever : CompProperties_SignalNode
    {
        public CompProperties_SignalReciever()
        {
            compClass = typeof(Comp_SignalReciever);
        }
    }

    public class Comp_SignalReciever : Comp_SignalNode
    {
        protected override void SetupDefaultPorts()
        {
            ConnectionPorts = new List<SignalPort>();
            ConnectionPorts.Add(new SignalPort(this, SignalPortType.IN, IntVec3.Zero));
        }
    }
    public class CompProperties_SignalMediator : CompProperties_SignalReciever
    {
        public CompProperties_SignalMediator()
        {
            compClass = typeof(CompSignalMediator);
        }
    }

    public class CompSignalMediator : Comp_SignalReciever
    {
        private List<Action<Signal>> registeredActions = new List<Action<Signal>>();
        public void RegisterAction(Action<Signal> action)
        {
            if (action != null)
            {
                registeredActions.Add(action);
            }
        }
        public override void OnSignalRecieved(Signal signal, SignalPort receivingPort)
        {
            base.OnSignalRecieved(signal, receivingPort);

            foreach (var action in registeredActions)
            {
                action(signal);
            }
        }
    }
}
