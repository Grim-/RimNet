using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimNet
{
    public class SignalManager : MapComponent
    {
        public SignalManager(Map map) : base(map)
        {

        }

        public void TickSignals()
        {

        }
    }

    public class SignalNetwork
    {
        protected Queue<Signal> SignalQueue = new Queue<Signal>();

    }
}
