namespace RimNet
{
    public static class SignalNodeTileGroupExtensions
    {
        public static SignalGroup GetTileGroup(this Comp_SignalNode node)
        {
            if (node is ITileGroupedSignalNode tileGrouped)
                return tileGrouped.TileGroup;
            return null;
        }
    }
}