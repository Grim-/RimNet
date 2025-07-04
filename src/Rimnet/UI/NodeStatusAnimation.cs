namespace RimNet
{
    public struct NodeStatusAnimation
    {
        public Comp_NetworkNode Node;
        public int StartTick;
        public bool GoingOnline;

        public NodeStatusAnimation(Comp_NetworkNode node, int startTick, bool goingOnline)
        {
            Node = node;
            StartTick = startTick;
            GoingOnline = goingOnline;
        }
    }
}