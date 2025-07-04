using UnityEngine;

namespace RimNet
{
    public static class GUIExtensions
    {
        public static Color SetAlpha(this Color color, float newValue)
        {
            return new Color(color.r, color.g, color.b, newValue);
        }
        public static Rect[] SplitRectVertical(this Rect rect, params float[] heights)
        {
            Rect[] result = new Rect[heights.Length];
            float currentY = rect.y;

            for (int i = 0; i < heights.Length; i++)
            {
                result[i] = new Rect(rect.x, currentY, rect.width, heights[i]);
                currentY += heights[i];
            }

            return result;
        }

        public static Color Mult(this Color color, float multi)
        {
            multi = Mathf.Clamp01(multi);
            return new Color(color.r * multi, color.g * multi, color.b * multi, color.a);
        }
        public static Color MultAlpha(this Color color, float multi)
        {
            multi = Mathf.Clamp01(multi);
            return new Color(color.r, color.g, color.b, color.a * multi);
        }
        public static Rect[] SplitRectHorizontal(Rect rect, params float[] widths)
        {
            Rect[] result = new Rect[widths.Length];
            float currentX = rect.x;

            for (int i = 0; i < widths.Length; i++)
            {
                result[i] = new Rect(currentX, rect.y, widths[i], rect.height);
                currentX += widths[i];
            }

            return result;
        }
        public static Rect ScaledBy(this Rect rect, float scale)
        {
            float newWidth = rect.width * scale;
            float newHeight = rect.height * scale;
            float xOffset = (newWidth - rect.width) / 2f;
            float yOffset = (newHeight - rect.height) / 2f;
            return new Rect(rect.x - xOffset, rect.y - yOffset, newWidth, newHeight);
        }

        public static Rect ExpandedBy(this Rect rect, float pixels)
        {
            return new Rect(rect.x - pixels, rect.y - pixels,
                rect.width + pixels * 2, rect.height + pixels * 2);
        }
        public static Rect Inset(this Rect rect, float inset)
        {
            return new Rect(rect.x + inset, rect.y + inset, rect.width - (inset * 2), rect.height - (inset * 2));
        }

        public static Rect InsetBy(this Rect rect, float left, float top, float right, float bottom)
        {
            return new Rect(rect.x + left, rect.y + top, rect.width - left - right, rect.height - top - bottom);
        }

    }
}