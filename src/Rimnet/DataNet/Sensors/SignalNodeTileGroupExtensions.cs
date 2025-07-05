namespace RimNet
{
    public static class SignalNodeTileGroupExtensions
    {
        public static SignalNodeTileGroup GetTileGroup(this Comp_SignalNode node)
        {
            if (node is ITileGroupedSignalNode tileGrouped)
                return tileGrouped.TileGroup;
            return null;
        }
    }
}