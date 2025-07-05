using Verse;

namespace RimNet
{
    public struct SpatialNodeData
    {
        public Comp_SignalNode FoundNode { get; set; }
        public IntVec3 NormalizedDirection { get; set; }
        public Rot4 RotationToNode { get; set; }

        public SpatialNodeData(Comp_SignalNode foundNode, IntVec3 normalizedDirection, Rot4 rotationToNode)
        {
            FoundNode = foundNode;
            NormalizedDirection = normalizedDirection;
            RotationToNode = rotationToNode;
        }
    }
}
