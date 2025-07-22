using UnityEngine;

namespace RimNet
{
    public class RowLayoutManager
    {
        private float currentX;
        private readonly float rowY;
        private readonly float rowHeight;
        private readonly float rowWidth;
        private readonly float horizontalPadding;
        private readonly float verticalPadding;

        public RowLayoutManager(Rect rowRect, float horizontalPadding = 10f, float verticalPadding = 5f)
        {
            this.rowWidth = rowRect.width;
            this.rowHeight = rowRect.height - (2 * verticalPadding);
            this.horizontalPadding = horizontalPadding;
            this.verticalPadding = verticalPadding;
            this.currentX = rowRect.x + horizontalPadding;
            this.rowY = rowRect.y + verticalPadding;
        }

        public Rect NextRect(float width, float? spacing = null)
        {
            if (currentX + width > rowWidth - horizontalPadding)
            {
                //prevent overflow
                width = rowWidth - horizontalPadding - currentX;
            }
            Rect rect = new Rect(currentX, rowY, width, rowHeight);
            currentX += width + (spacing ?? 0);
            return rect;
        }

        public float RemainingWidth => rowWidth - currentX - horizontalPadding;
    }
}
