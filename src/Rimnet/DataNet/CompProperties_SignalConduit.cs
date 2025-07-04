namespace RimNet
{
    public class CompProperties_SignalConduit : CompProperties_SignalNode
    {
        public CompProperties_SignalConduit()
        {
            compClass = typeof(Comp_SignalConduit);
        }
    }


    public class Comp_SignalConduit : Comp_SignalNode
    {
        //public override bool VerifyConnection(SignalPort myPort, Comp_SignalNode otherNode, SignalPort otherPort, out string cantConnectReason, bool ignoreConnectionChecks = false)
        //{
        //    if (!(otherNode is Comp_SignalConduit))
        //    {
        //        cantConnectReason = "";
        //        return true;
        //    }
        //    else
        //    {
        //        cantConnectReason = "conduits can only connect to non conduits as parents to ensure forward connection";
        //        return false;
        //    }
        //}
    }
}
