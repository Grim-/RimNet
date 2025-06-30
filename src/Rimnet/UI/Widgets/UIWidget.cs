using UnityEngine;
using Verse;

namespace RimNet
{
    public abstract class UIWidget
    {
        public virtual void Draw(Rect Rect)
        {
            if(DrawWidgetBackground)
                Widgets.DrawBoxSolidWithOutline(Rect, WidgetBackground, WidgetOutline);
        }

        public abstract Vector2 GetPreferredSize();

        public virtual Color WidgetOutline => Color.white;
        public virtual Color WidgetBackground => Color.grey * 0.5f;

        public virtual bool DrawWidgetBackground => true;
    }
}