using Verse;

namespace RimNet
{
    public struct ThingSpatialData
    {
        public Building_Track Origin;
        public Building_Track NORTH;
        public Building_Track EAST;
        public Building_Track WEST;
        public Building_Track SOUTH;


        public Building_Track FromRot4(Rot4 from)
        {
            switch (from.AsInt)
            {
                case Rot4.NorthInt:
                    return NORTH;
                case Rot4.EastInt:
                    return EAST;
                case Rot4.WestInt:
                    return WEST;
                case Rot4.SouthInt:
                    return SOUTH;
                default:
                    return null;
            }
        }
    }
}