using UnityEngine;

namespace RimNet
{
    public struct NodePulseAnimation
    {
        public Comp_NetworkNode Node;
        public int StartTick;
        public Color PulseColor;

        public NodePulseAnimation(Comp_NetworkNode node, int startTick, Color? color = null)
        {
            Node = node;
            StartTick = startTick;
            PulseColor = color ?? Color.white;
        }
    }
}