using UnityEngine;

namespace RimNet
{
    public struct NetworkMessageCache
    {
        public Vector2 Start;
        public Vector2 End;
        public int StartTick;
        public Color MessageColor;

        public NetworkMessageCache(Vector2 start, Vector2 end, int startTick, Color? color = null)
        {
            Start = start;
            End = end;
            StartTick = startTick;
            MessageColor = color ?? new Color(1f, 1f, 0f, 1f);
        }
    }
}