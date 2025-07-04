using RimWorld;
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

    }


}
